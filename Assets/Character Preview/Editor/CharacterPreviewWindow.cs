using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SampleCode
{
    [ExecuteInEditMode]
    public class CharacterPreviewWindow : EditorWindow
    {
        private GUISkin cpSkin;
        
        private float panelSizeRatio = 0.5f;
        private bool isResizingPanel = false;
        private Rect tabPanel, previewPanel, resizer, cameraRect;

        enum Tab { Character, Animation }
        private int tabValue = 0;
        private Dictionary<int/*Tab*/, TabContent> tabContentsDic = new Dictionary<int, TabContent>();
        
        private GameObject charObj = null;
        private Animator charAnimator = null;
        private AnimationClip animationClip = null;
        private RuntimeAnimatorController animatorController = null;

        public class CameraOptions
        {
            public bool isChanging, isFollowing;
            public Vector3 origin, pivot, rotation, pan;
            public float zoom = 5f;

            public void Reset(Vector3 centerPosition, float size = 5f)
            {
                pivot = origin = centerPosition;
                pan = Vector2.zero;
                rotation = new Vector2(-120, 20);
                zoom = Mathf.Max(5f, size);
            }
        }
        private CameraOptions cameraOptions;
        
        private PreviewRenderUtility previewRenderUtility;
        private GameObject previewObject;
        private GameObject previewPlane;

        private bool isPlayingAnimation;
        private float animationTime = 0f;
        private float clipProgressValue = 0f, clipSpeedValue = 1f;
        private double lastTimeSinceStartup = 0f;

        [MenuItem("Sample Code/Character Preview", false, 2)]
        public static void ShowWindow()
        {
            GetWindow<CharacterPreviewWindow>();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Preview");
            
            cpSkin = AssetDatabase.LoadAssetAtPath("Assets/Character Preview/Editor/CPSkin.guiskin", typeof(GUISkin)) as GUISkin;
            animatorController = AssetDatabase.LoadAssetAtPath("Assets/Character Preview/Res/PreviewController.controller", typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;

            (tabContentsDic[tabValue = (int)Tab.Character] = new CharacterTabContent()).onChangeAsset.AddListener(OnChangeCharacter);
            (tabContentsDic[(int)Tab.Animation] = new AnimationTabContent()).onChangeAsset.AddListener(OnChangeAnimationClip);

            (cameraOptions = new CameraOptions()).Reset(Vector3.zero);
            
            previewRenderUtility = new PreviewRenderUtility(true);
            previewRenderUtility.camera.fieldOfView = 30f;
            previewRenderUtility.camera.nearClipPlane = 0.3f;
            previewRenderUtility.camera.farClipPlane = 1000f;
            
            if (previewRenderUtility.lights[0] != null)
            {
                previewRenderUtility.lights[0].transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            Object plane = AssetDatabase.LoadAssetAtPath("Assets/Character Preview/Res/PreviewPlane.prefab", typeof(GameObject));
            if (plane != null)
            {
                previewPlane = previewRenderUtility.InstantiatePrefabInScene(plane as GameObject);
            }
            
            OnChangeCharacter(charObj);

            isPlayingAnimation = false;
            animationClip = null;
            
            if (AnimationMode.InAnimationMode())
            {
                AnimationMode.StopAnimationMode();
            }
        }

        private void OnDisable()
        {
            isPlayingAnimation = false;
            animationClip = null;
            if (AnimationMode.InAnimationMode())
            {
                AnimationMode.StopAnimationMode();
            }

            if (previewPlane != null)
            {
                DestroyImmediate(previewPlane);
            }

            if (previewObject != null)
            {
                DestroyImmediate(previewObject);
            }

            previewRenderUtility.Cleanup();
            previewRenderUtility = null;

            tabContentsDic.Clear();
            PlayerPrefs.Save();
        }

        private void OnFocus()
        {
            Repaint();
        }
        
        private void Update()
        {
            if (tabContentsDic.TryGetValue(tabValue, out TabContent tabContent))
            {
                if (tabContent.Update())
                {
                    Repaint();
                }
            }

            if (charObj == null || charAnimator == null || charAnimator.runtimeAnimatorController == null || animationClip == null) 
                return;

            if (isPlayingAnimation)
            {
                if (lastTimeSinceStartup > 0f)
                {
                    float deltaTime = (float)(EditorApplication.timeSinceStartup - lastTimeSinceStartup);

                    animationTime += deltaTime * clipSpeedValue;
                    if (animationTime >= animationClip.length)
                    {
                        animationTime = 0f;
                    }
                    clipProgressValue = Mathf.InverseLerp(0f, animationClip.length, animationTime);
                }
                
                lastTimeSinceStartup = EditorApplication.timeSinceStartup;
            }
            else
            {
                lastTimeSinceStartup = 0f;
            }

            if (AnimationMode.InAnimationMode())
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(previewObject, animationClip, animationTime);
                AnimationMode.EndSampling();
            }
        }

        public virtual void OnGUI()
        {            
            ProcessEvents(Event.current);
            
            DrawTabPanel();
            DrawPreviewPanel();
            DrawResizer();
            
            if (GUI.changed || isResizingPanel || isPlayingAnimation || cameraOptions.isChanging)
            {
                Repaint();
            }            
        }

        private void ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    {
                        if (e.button == 0 && resizer.Contains(e.mousePosition))
                        {
                            isResizingPanel = true;
                        }
                        
                        if (cameraRect.Contains(e.mousePosition))
                        {
                            cameraOptions.isChanging = true;
                        }
                        
                        break;
                    }

                case EventType.MouseUp:
                    {
                        isResizingPanel = cameraOptions.isChanging = false;
                        
                        break;
                    }

                case EventType.MouseDrag:
                    {
                        if (isResizingPanel == false && cameraOptions.isChanging)
                        {
                            if (e.button == 0 || e.button == 2)
                            {
                                Vector3 delta = e.delta * 0.001f * cameraOptions.zoom;

                                cameraOptions.pan -= previewRenderUtility.camera.transform.right * delta.x;
                                cameraOptions.pan += previewRenderUtility.camera.transform.up * delta.y;
                            }
                            else if (e.button == 1)
                            {
                                Vector3 delta = e.delta * 0.25f;

                                cameraOptions.rotation += delta;
                            }

                            UpdatePreviewCamera();
                        }

                        break;
                    }

                case EventType.ScrollWheel:
                    {
                        if (cameraRect.Contains(e.mousePosition))
                        {
                            cameraOptions.zoom = Mathf.Clamp(cameraOptions.zoom + e.delta.y * 0.5f, 2f, float.MaxValue);

                            UpdatePreviewCamera();

                            Repaint();
                        }

                        break;
                    }
            }

            Resize(e);
        }

        private void Resize(Event e)
        {
            if (isResizingPanel)
            {
                panelSizeRatio = Mathf.Clamp(e.mousePosition.x / position.width, 0.01f, 0.99f);
            }
        }

        void DrawTabPanel()
        {
            tabPanel = new Rect(0f, 0f, position.width * panelSizeRatio, position.height);
            GUILayout.BeginArea(tabPanel);
            GUILayout.BeginVertical();

            TabContent tabContent = null;
            GUILayout.BeginHorizontal("Toolbar");

            Dictionary<int, TabContent>.Enumerator tabs = tabContentsDic.GetEnumerator();
            while (tabs.MoveNext())
            {
                if (GUILayout.Toggle(tabValue == tabs.Current.Key, tabs.Current.Value.tabTitle, "ToolbarButton"))
                {
                    tabContent = tabs.Current.Value;
                    if (tabValue != tabs.Current.Key || tabContent.IsInitialized == false)
                    {
                        tabContent.Search();
                        tabValue = tabs.Current.Key;
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (tabContent != null)
            {
                tabContent.Draw(tabPanel);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void DrawPreviewPanel()
        {
            previewPanel = new Rect(position.width * panelSizeRatio, 0f, position.width * (1f - panelSizeRatio), position.height);

            float toolbarHeight = 46f;
            cameraRect = new Rect(previewPanel.x, previewPanel.y + toolbarHeight, previewPanel.width, previewPanel.height - toolbarHeight);

            previewRenderUtility.BeginPreview(cameraRect, "window");            
            previewRenderUtility.camera.Render();
            previewRenderUtility.EndAndDrawPreview(cameraRect);

            GUILayout.BeginArea(previewPanel);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal("Toolbar");
            GUILayout.Space(10f);
            GUILayout.Label($"{(animationClip != null ? animationClip.name : string.Empty)}", cpSkin.GetStyle("clipName"));
            GUILayout.EndHorizontal();

            GUILayout.Space(2f);
            GUILayout.BeginHorizontal("Toolbar");
            
            var playButtonContent = EditorGUIUtility.IconContent("PlayButton");
            var pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
            isPlayingAnimation = GUILayout.Toggle(isPlayingAnimation, isPlayingAnimation ? pauseButtonContent : playButtonContent, "ToolbarButton", GUILayout.Width(30f));
            
            clipProgressValue = GUILayout.HorizontalSlider(clipProgressValue, 0f, 1f, cpSkin.GetStyle("clipSlider"), cpSkin.GetStyle("clipSliderThumb"), GUILayout.Height(20f));
            if (animationClip != null)
            {
                animationTime = Mathf.Lerp(0f, animationClip.length, clipProgressValue);
            }
            
            GUILayout.Space(4f);
            clipSpeedValue = GUILayout.HorizontalSlider(clipSpeedValue, 0.1f, 2f, GUILayout.Width(100f));
            GUILayout.Label($"{clipSpeedValue:0.00}x", GUILayout.Width(35f));

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

            GUILayoutOption cameraBoxWidth = GUILayout.Width(30f), cameraBoxHeight = GUILayout.Height(30f);
            GUILayout.BeginVertical("box", cameraBoxWidth);

            var resetCameraContent = EditorGUIUtility.IconContent("SceneViewTools");
            resetCameraContent.tooltip = "Reset Camera";
            if (GUILayout.Button(resetCameraContent, cameraBoxWidth, cameraBoxHeight))
            {
                Bounds bounds = UpdateBounds(previewObject);
                cameraOptions.Reset(bounds.center, Mathf.Max(bounds.size.x, bounds.size.y));
                UpdatePreviewCamera();
            }
            GUILayout.Space(2f);

            var followCameraContent = EditorGUIUtility.IconContent("SceneViewCamera");
            followCameraContent.tooltip = "Toggle Follow Camera";
            if (cameraOptions.isFollowing = GUILayout.Toggle(cameraOptions.isFollowing, followCameraContent, "Button", cameraBoxWidth, cameraBoxHeight))
            {
                UpdatePreviewCamera();
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(10f);

            int second = 0, fraction = 0, frameCount = 0;
            float percentage = 0f;
            if (animationClip != null)
            {
                second = (int)animationTime;
                fraction = (int)((animationTime - second) * 100);
                percentage = animationTime / animationClip.length * 100;
                frameCount = Mathf.RoundToInt(animationTime / (1 / animationClip.frameRate));
            }
            string stateText = $"{second}:{fraction:00} ({percentage:000.0}%) Frame {frameCount}";
            GUILayout.Label(stateText, cpSkin.GetStyle("clipState"));
            GUILayout.Space(4f);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawResizer()
        {
            resizer = new Rect(position.width * panelSizeRatio - 5f, 0f, 10f, position.height);

            GUILayout.BeginArea(new Rect(resizer.position + (Vector2.up * 5f), new Vector2(position.width, 2)));
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(resizer, MouseCursor.ResizeHorizontal);
        }

        private Bounds UpdateBounds(GameObject obj)
        {
            Bounds bounds = new Bounds();
            
            if (obj != null)
            {
                bounds = new Bounds(obj.transform.position, Vector3.zero);

                //모든 Renderer 컴포넌트를 얻어옵니다
                foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
                {
                    //가장 큰 Bounds를 얻어옵니다
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return bounds;
        }

        private void ResetPreviewCharacter(GameObject obj)
        {
            if (previewObject != null)
            {
                DestroyImmediate(previewObject);
            }

            if (obj != null)
            {
                previewObject = previewRenderUtility.InstantiatePrefabInScene(obj);
            }

            Bounds bounds = UpdateBounds(previewObject);
            cameraOptions.Reset(bounds.center, Mathf.Max(bounds.size.x, bounds.size.y));

            UpdatePreviewCamera();
        }

        private void UpdatePreviewCamera()
        {
            Vector3 centerPosition = cameraOptions.isFollowing ? cameraOptions.pivot = UpdateBounds(previewObject).center : cameraOptions.pivot;
            centerPosition += cameraOptions.pan;

            previewRenderUtility.camera.transform.position = centerPosition + Vector3.forward * -cameraOptions.zoom;
            previewRenderUtility.camera.transform.rotation = Quaternion.identity;

            previewRenderUtility.camera.transform.RotateAround(centerPosition, Vector3.up, cameraOptions.rotation.x);
            previewRenderUtility.camera.transform.RotateAround(centerPosition, previewRenderUtility.camera.transform.right, cameraOptions.rotation.y);
        }

        void OnChangeCharacter(Object obj)
        {
            charObj = obj as GameObject;
            charAnimator = charObj?.GetComponent<Animator>();
            
            if (charAnimator != null)
            {
                if (charAnimator.runtimeAnimatorController == null)
                {
                    charAnimator.runtimeAnimatorController = animatorController;
                }
            }

            ResetPreviewCharacter(charObj);
            clipProgressValue = animationTime = 0f;
        }

        void OnChangeAnimationClip(Object obj)
        {
            animationClip = obj as AnimationClip;
            if (animationClip != null)
            {
                if (AnimationMode.InAnimationMode() == false)
                {
                    AnimationMode.StartAnimationMode();
                }
            }
            else if (AnimationMode.InAnimationMode())
            {
                isPlayingAnimation = false;
                AnimationMode.StopAnimationMode();
            }

            clipProgressValue = animationTime = 0f;
        }


        public abstract class TabContent
        {
            public class AssetContent
            {
                public enum Thumbnail { Preview, Mini, }
                public Object asset;
                public GUIContent content;
                public Thumbnail type = Thumbnail.Mini;
                private double refreshTime;

                public AssetContent(Object asset, Thumbnail type = Thumbnail.Mini)
                {
                    this.asset = asset;
                    this.type = type;
                    RefreshThumb();
                }

                public bool RefreshThumb()
                {
                    if (content == null || content.image == null)
                    {
                        if ((float)(EditorApplication.timeSinceStartup - refreshTime) > 1f)
                        {
                            if (content == null)
                            {
                                content = new GUIContent(asset.name, type == Thumbnail.Preview ? AssetPreview.GetAssetPreview(asset) : AssetPreview.GetMiniThumbnail(asset));
                            }
                            else if (type == Thumbnail.Preview)
                            {
                                if (AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()) == false)
                                {
                                    content.image = AssetPreview.GetAssetPreview(asset);
                                }
                            }

                            refreshTime = EditorApplication.timeSinceStartup;
                            return content?.image != null;
                        }
                    }

                    return false;
                }
            }

            protected GUISkin skin;

            public string tabTitle = "Tab", objTitle = "Object";
            protected Vector2 scrollPosition;

            protected int currentPageIndex, maxPageCount, itemCountPerPage = 24;
            protected int headPageIndex, pageIndexItemCount = 5;
            protected string filterText;

            protected int selectedPerPageIndex;
            protected int[] selectablePerPagevalues = new int[] { 24, 48, 96 };
            protected string[] selectablePerPageTitles = new string[] { "24 Per Page", "48 Per Page", "96 Per Page", };
            protected float itemCellSize = 64f;

            protected string rootPath = "Assets/Character Preview", searchPatterns;
            protected List<string> filePaths;
            protected List<AssetContent> assetList, contentList;
            
            public Object asset;
            protected System.Type assetType = typeof(Object);
            public UnityEngine.Events.UnityEvent<Object> onChangeAsset = new UnityEngine.Events.UnityEvent<Object>();

            public bool IsInitialized => assetList != null;

            public TabContent()
            {
                skin = AssetDatabase.LoadAssetAtPath("Assets/Character Preview/Editor/CPSkin.guiskin", typeof(GUISkin)) as GUISkin;
                itemCellSize = PlayerPrefs.GetFloat($"CPTabCellSize_{tabTitle}", 100f);
                selectedPerPageIndex = PlayerPrefs.GetInt($"CPTabSelectedPerPgaeIndex_{tabTitle}", 0);
            }

            protected abstract AssetContent CreateAssetContent(string filePath);

            public void Search(string path, bool reset = false)
            {
                filePaths = new List<string>();
                if (System.IO.Directory.Exists(path))
                {                    
                    string[] patterns = searchPatterns.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < patterns.Length; ++i)
                    {
                        string[] files = System.IO.Directory.GetFiles(rootPath = path, patterns[i].Trim(), System.IO.SearchOption.AllDirectories);
                        IEnumerable<string> e = System.Linq.Enumerable.Select(files, (file) => { return file.Replace("\\", "/"); });
                        filePaths.AddRange(System.Linq.Enumerable.ToList(e));
                    }

                    PlayerPrefs.SetString($"CPTabPath_{tabTitle}", rootPath);
                }

                assetList = new List<AssetContent>(filePaths.Count);
                for (int i = 0; i < filePaths.Count; ++i)
                {
                    string assetPath = filePaths[i];
                    
                    int index = assetPath.IndexOf("/Assets");
                    if (index > -1)
                    {
                        assetPath = assetPath.Remove(0, index + 1);
                    }

                    AssetContent content = CreateAssetContent(assetPath);
                    if (content != null)
                    {
                        assetList.Add(content);
                    }
                }

                RefreshContentList(reset);
            }

            public void Search(bool force = false)
            {
                if (filePaths == null || force)
                {
                    Search(rootPath);
                }
            }

            private void RefreshPgae(int index)
            {
                currentPageIndex = Mathf.Clamp(index, 0, maxPageCount - 1);
                
                if (maxPageCount > pageIndexItemCount)
                {
                    if (currentPageIndex <= headPageIndex || currentPageIndex >= Mathf.Clamp(headPageIndex + pageIndexItemCount, 0, maxPageCount) - 1)
                    {
                        headPageIndex = Mathf.Clamp(currentPageIndex - (pageIndexItemCount / 2), 0, maxPageCount - pageIndexItemCount);
                    }
                }
                else
                {
                    headPageIndex = 0;
                }
            }

            private void RefreshContentList(bool reset = false)
            {
                contentList = assetList;

                if (string.IsNullOrEmpty(filterText) == false)
                {
                    IEnumerable<AssetContent> e = System.Linq.Enumerable.Where(assetList, (content) =>
                    {
                        return content.asset.name.IndexOf(filterText, System.StringComparison.OrdinalIgnoreCase) > -1;
                    });

                    contentList = System.Linq.Enumerable.ToList(e);
                }
                
                maxPageCount = Mathf.Max(1, contentList.Count / itemCountPerPage + ((contentList.Count % itemCountPerPage) > 0 ? 1 : 0));

                RefreshPgae(reset ? 0 : currentPageIndex);
            }

            public bool Update()
            {
                bool repaint = false;

                if (contentList != null)
                {
                    for (int startIndex = currentPageIndex * itemCountPerPage, itemIndex = 0; startIndex < contentList.Count && itemIndex < itemCountPerPage; ++startIndex, ++itemIndex)
                    {
                        repaint = contentList[startIndex].RefreshThumb() || repaint;
                    }
                }
                
                return repaint;
            }

            public virtual void Draw(Rect rect)
            {
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();

                var directory = EditorGUIUtility.IconContent("d_Folder Icon");
                directory.text = rootPath;
                
                var directoryHeight = GUILayout.Height(22f);
                GUILayout.Label(directory, directoryHeight);

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_FolderOpened Icon"), GUILayout.Width(30f), directoryHeight))
                {
                    string path = EditorUtility.OpenFolderPanel(tabTitle, rootPath, "Assets");
                    if (string.IsNullOrEmpty(path) == false)
                    {
                        Search(path, true);
                    }
                }

                var refreshButton = EditorGUIUtility.IconContent("Refresh");
                refreshButton.tooltip = "Refresh Assets";
                if (GUILayout.Button(refreshButton/*, "toolbarButton"*/, GUILayout.Width(30f), directoryHeight))
                {
                    Search(true);
                }

                GUILayout.Space(30f);
                float sizeValue = GUILayout.HorizontalSlider(itemCellSize, 50f, 200f, GUILayout.Width(60f));
                if (Mathf.Abs(itemCellSize - sizeValue) > 1f)
                {
                    PlayerPrefs.SetFloat($"CPTabCellSize_{tabTitle}", itemCellSize = sizeValue);
                }

                GUILayout.Space(20f);
                GUILayout.EndHorizontal();
                GUILayout.Space(2f);

                GUILayout.BeginHorizontal();
                Object currentAsset = asset;
                asset = EditorGUILayout.ObjectField(objTitle, asset, assetType, true, GUILayout.MaxWidth(rect.width * 0.5f));
                GUILayout.FlexibleSpace();
                
                var filterContent = EditorGUIUtility.IconContent("Search Icon");
                GUILayout.Label(filterContent, skin.GetStyle("searchField"), GUILayout.Width(20f), GUILayout.Height(20f));
                string filterValue = GUILayout.TextField(filterText, GUILayout.MaxWidth(120f));
                if (filterValue != filterText)
                {
                    filterText = filterValue;
                    RefreshContentList();
                }

                GUILayout.Space(2f);
                int selectedIndex = EditorGUILayout.Popup(selectedPerPageIndex, selectablePerPageTitles, "toolbarDropDown", GUILayout.Width(100f));
                if (selectedPerPageIndex != selectedIndex)
                {
                    itemCountPerPage = selectablePerPagevalues[selectedPerPageIndex = selectedIndex];
                    PlayerPrefs.SetInt($"CPTabSelectedPerPgaeIndex_{tabTitle}", selectedPerPageIndex);
                    RefreshContentList();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(2f);
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, "box");
                {
                    GUILayout.BeginVertical();
                    float cellSize = itemCellSize, cellSpace = 5f, panelWidth = rect.width - 40f;
                    int lineCount = Mathf.Clamp((int)(panelWidth / (cellSize + cellSpace)), 1, int.MaxValue);
                    float resizeSpace = (panelWidth - (cellSize * lineCount)) / (lineCount + 1);

                    for (int index = currentPageIndex * itemCountPerPage, itemIndex = 0; index < contentList.Count && itemIndex < itemCountPerPage; ++index, ++itemIndex)
                    {
                        AssetContent assetContent = contentList[index];

                        if (itemIndex % lineCount == 0)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(resizeSpace);
                        }
                        if (GUILayout.Toggle(asset == assetContent.asset, assetContent.content, skin.GetStyle("itemThumb"), GUILayout.Width(cellSize), GUILayout.Height(cellSize)))
                        {
                            asset = assetContent.asset;
                        }
                        if (index == (contentList.Count - 1) || itemIndex % lineCount == (lineCount - 1) || itemIndex >= (itemCountPerPage - 1))
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                            GUILayout.Space(20f);
                        }
                        else
                        {
                            GUILayout.Space(resizeSpace);
                        }
                    }
                    GUILayout.EndVertical();
                    GUILayout.EndScrollView();
                }
               
                if (maxPageCount > 1)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    GUIStyle pageStyle = skin.GetStyle("pageIndex");
                    if (headPageIndex > 0)
                    {
                        if (GUILayout.Button("1", pageStyle, GUILayout.Width(24f), GUILayout.Height(24f)))
                        {
                            RefreshPgae(0);
                        }
                        GUILayout.Label("...");
                    }
                    for (int index = headPageIndex, itemIndex = 0; index < maxPageCount && itemIndex < pageIndexItemCount; ++index, ++itemIndex)
                    {
                        if (GUILayout.Toggle(index == currentPageIndex, $"{index + 1}", pageStyle, GUILayout.Width(24f), GUILayout.Height(24f)))
                        {
                            if (index != currentPageIndex)
                            {
                                RefreshPgae(index);
                            }
                        }
                        GUILayout.Space(2f);
                    }
                    if (headPageIndex < maxPageCount - pageIndexItemCount)
                    {
                        GUILayout.Label("...");
                        if (GUILayout.Button($"{maxPageCount}", pageStyle, GUILayout.Width(24f), GUILayout.Height(24f)))
                        {
                            RefreshPgae(maxPageCount - 1);
                        }
                    }
                    
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2f);
                }
                GUILayout.EndVertical();

                if (asset != currentAsset)
                {
                    OnChangeAsset();
                }
            }

            void OnChangeCountPerPage(object value)
            {

            }

            protected virtual void OnChangeAsset()
            {
                onChangeAsset?.Invoke(asset);
            }
        }

        public class CharacterTabContent : TabContent
        {
            private bool charExist, isHuman, isValidAvatar;

            public CharacterTabContent()
            {
                tabTitle = "Characdter";
                objTitle = "FBX Model";

                rootPath = PlayerPrefs.GetString($"CPTabPath_{tabTitle}", "Assets/Character Preview/Res/Character");
                searchPatterns = "*.fbx;*.prefab";
                assetType = typeof(GameObject);
            }

            protected override AssetContent CreateAssetContent(string filePath)
            {
                Object asset = AssetDatabase.LoadAssetAtPath(filePath, typeof(GameObject));
                return asset != null ? new AssetContent(asset, AssetContent.Thumbnail.Preview) : null;
            }

            public override void Draw(Rect rect)
            {
                base.Draw(rect);

                if (asset)
                {
                    if (!charExist)
                    {
                        EditorGUILayout.HelpBox("Missing a Animator Component", MessageType.Error);
                    }
                    else if (!isHuman)
                    {
                        EditorGUILayout.HelpBox("This is not a Humanoid", MessageType.Error);
                    }
                    else if (!isValidAvatar)
                    {
                        EditorGUILayout.HelpBox(asset.name + " is a invalid Humanoid", MessageType.Info);
                    }
                }
            }

            protected override void OnChangeAsset()
            {
                base.OnChangeAsset();

                Animator charAnimator = (asset as GameObject)?.GetComponent<Animator>();
                charExist = charAnimator != null;
                isHuman = charExist ? charAnimator.isHuman : false;
                isValidAvatar = charExist ? charAnimator.avatar.isValid : false;
            }
        }

        public class AnimationTabContent : TabContent
        {
            public AnimationTabContent()
            {
                tabTitle = "Animation";
                objTitle = "Animation Clip";

                rootPath = PlayerPrefs.GetString($"CPTabPath_{tabTitle}", "Assets/Character Preview/Res/Animation");
                searchPatterns = "*.anim;*.fbx";
                assetType = typeof(AnimationClip);
            }

            protected override AssetContent CreateAssetContent(string filePath)
            {
                Object asset = AssetDatabase.LoadAssetAtPath(filePath, typeof(AnimationClip));
                return asset != null ? new AssetContent(asset) : null;
            }
        }
    }
}
