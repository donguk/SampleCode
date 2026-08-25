using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System;
//using UnityEditor.TreeViewExamples;

namespace ClimbGames.Editor
{
    [Serializable]
    public class FanArtData
    {
        //static readonly System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo("en-US");

        public string title;
        public string author;
        public string assetPath;
        [SerializeField] private string dateTime;

        public DateTime DateTime { get; private set; }
        public int Depth { get; private set; }

        public FanArtData(string path)
        {
            title = System.IO.Path.GetFileNameWithoutExtension(path);
            assetPath = path;
        }

        public void Initialize()
        {
            Depth = assetPath.Split("/").Length;
            if (DateTime.TryParseExact(dateTime, "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime value))
            //if (DateTime.TryParse(dateTime, out DateTime value))
            {
                DateTime = value;
            }
            else
            {
                DateTime = DateTime.UtcNow;
                dateTime = DateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }
        }
    }

    [Serializable]
    public class FanArtStorage
    {
        public List<FanArtData> files;

        public FanArtStorage(Dictionary<string, FanArtData> fanArtInfos)
        {
            files = new List<FanArtData>();
            foreach (var pair in fanArtInfos)
                files.Add(pair.Value);
        }
    }

    public class FanArtTreeViewWindow : EditorWindow
    {
        const string FanArtStoragePath = "Assets/FanArt Viewer/FanArtData.txt";
        Dictionary<string/*guid*/, FanArtData> fanArtInfos;

        TreeModel<FanArtTreeElement> treeModel;
        FanArtTreeView treeView;
        TreeViewState<int> treeViewState = new TreeViewState<int>();
        MultiColumnHeaderState columnHeaderState;
        Rect treeViewRect;
        DragAndDropManipulator manipulator;

        void LoadFromJson()
        {
            if (fanArtInfos == null)
                fanArtInfos = new Dictionary<string, FanArtData>();
            fanArtInfos.Clear();

            string json = string.Empty;
            //TextAsset asset = Resources.Load<TextAsset>(FanArtDataPath);
            //if (asset != null)
            if (System.IO.File.Exists(FanArtStoragePath))
                json = System.IO.File.ReadAllText(FanArtStoragePath, System.Text.Encoding.UTF8);

            if (string.IsNullOrEmpty(json) == false)
            {
                FanArtStorage data = JsonUtility.FromJson<FanArtStorage>(json);
                if (data != null && data.files != null)
                {
                    for (int i = 0; i < data.files.Count; ++i)
                    {
                        string guid = AssetDatabase.AssetPathToGUID(data.files[i].assetPath);
                        if (fanArtInfos.TryGetValue(guid, out FanArtData fanArt) == false)
                            fanArtInfos.Add(guid, fanArt = data.files[i]);

                        fanArt.Initialize();
                    }
                }
            }
        }

        List<FanArtTreeElement> BuildTreeData(List<FanArtData> list)
        {
            list.Sort((lhs, rhs) =>
            {
                if (lhs.Depth == rhs.Depth)
                    return string.Compare(lhs.assetPath, rhs.assetPath);
                return rhs.Depth - lhs.Depth;
            });

            int id = 0;
            List<FanArtTreeElement> datas = new List<FanArtTreeElement>() { new FanArtTreeElement("root", -1, id++) };
            List<HashSet<string>> folders = new List<HashSet<string>>();

            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].assetPath.StartsWith("Assets/"))
                {
                    string[] values = list[i].assetPath.Split('/');
                    for (int depth = 0; depth < values.Length; ++depth)
                    {
                        if (depth < values.Length - 1) // folder
                        {
                            if (depth >= folders.Count)
                                folders.Add(new HashSet<string>());

                            if (folders[depth].Add(values[depth]))
                                datas.Add(new FanArtTreeElement(values[depth], depth, id++));
                        }
                        else // file
                        {
                            datas.Add(new FanArtTreeElement(list[i].title, depth, id++) { data = list[i] });
                        }
                    }
                }
            }

            return datas;
        }

        void AddAssets(List<UnityEngine.Object> assets)
        {
            List<string> assetPaths = new List<string>();
            for (int i = 0; i < assets.Count; ++i)
            {
                string path = AssetDatabase.GetAssetPath(assets[i]);
                if (assets[i] is DefaultAsset) // folder
                {
                    string[] guids = AssetDatabase.FindAssets("t:texture", new string[] { path });
                    for (int j = 0; j < guids.Length; ++j)
                    {
                        path = AssetDatabase.GUIDToAssetPath(guids[j]);
                        assetPaths.Add(path);
                    }
                }
                else if (assets[i] is Texture2D)
                {
                    assetPaths.Add(path);
                }
            }

            for (int i = 0; i < assetPaths.Count; ++i)
            {
                string guid = AssetDatabase.AssetPathToGUID(assetPaths[i]);
                if (fanArtInfos.TryGetValue(guid, out var fanArtData) == false)
                    fanArtInfos.Add(guid, fanArtData = new FanArtData(assetPaths[i]));

                fanArtData.Initialize();
            }

            treeModel.SetData(BuildTreeData(fanArtInfos.Values.ToList()));
            treeView.Reload();
        }

        void InitIfNeeded()
        {
            if (treeView == null)
            {
                if (treeModel == null)
                {
                    LoadFromJson();
                    treeModel = new TreeModel<FanArtTreeElement>(BuildTreeData(fanArtInfos.Values.ToList()));
                    treeModel.modelChanged += OnModelChanged;
                }

                MultiColumnHeader columnHeader = FanArtTreeView.GetMultiColumnHeader(ref columnHeaderState, FanArtTreeView.CreateHeaderColumns());
                treeView = new FanArtTreeView(treeViewState, columnHeader, treeModel);
                treeView.ExpandAll();
            }

            if (manipulator == null)
                manipulator = new DragAndDropManipulator(treeViewRect);
        }

        void OnModelChanged()
        {
            fanArtInfos.Clear();
            Stack<FanArtTreeElement> stack = new Stack<FanArtTreeElement>();
            stack.Push(treeModel.root);
            while (stack.Count > 0)
            {
                FanArtTreeElement parent = stack.Pop();
                if (parent.data != null)
                    fanArtInfos[AssetDatabase.AssetPathToGUID(parent.data.assetPath)] = parent.data;

                if (parent.hasChildren)
                {
                    for (int i = 0; i < parent.children.Count; ++i)
                        stack.Push(parent.children[i] as FanArtTreeElement);
                }
            }
        }

        void SevaFanArtToJson()
        {
            try
            {
                FanArtStorage data = new FanArtStorage(fanArtInfos);
                data.files.Sort((lhs, rhs) =>
                {
                    int diff = DateTime.Compare(rhs.DateTime, lhs.DateTime);
                    if (diff == 0)
                        return string.Compare(lhs.assetPath, rhs.assetPath);
                    return diff;
                });
                string json = JsonUtility.ToJson(data, true);

                string path = System.IO.Path.GetDirectoryName(FanArtStoragePath);
                if (System.IO.Directory.Exists(path) == false)
                    System.IO.Directory.CreateDirectory(path);

                System.IO.File.WriteAllText(FanArtStoragePath, json);
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Alarm", $"Fail to save...", "Ok");
                Debug.LogException(ex);
            }
            finally
            {
                EditorUtility.DisplayDialog("Notice", $"Success to save\n{FanArtStoragePath}", "Ok");
            }
        }

        void OnGUI()
        {
            InitIfNeeded();
            treeViewRect = new Rect(0f, 21f, position.width, position.height - 21f);

            if (manipulator != null)
            {
                manipulator.dropArea = treeViewRect;
                manipulator.ProcessEvent();

                if (manipulator.IsDragPerform)
                    AddAssets(manipulator.projectObjects);
            }

            GUILayout.BeginHorizontal("Toolbar");
            if (GUILayout.Button("Save", "ToolbarButton", GUILayout.MinWidth(60f)))
            {
                SevaFanArtToJson();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (treeView != null)
                treeView.OnGUI(treeViewRect);

            if (manipulator.IsDragArea)
                GUI.Box(manipulator.dropArea, "Drop Assets(png, jpg)", manipulator.dropStyle);
        }

        [MenuItem("Custome Tools/TreeView/FanArt Viewer")]
        public static FanArtTreeViewWindow ShowWindow()
        {
            var window = GetWindow<FanArtTreeViewWindow>();
            window.titleContent = new GUIContent("FanArt Viewer");
            window.Show();
            return window;
        }
    }
}