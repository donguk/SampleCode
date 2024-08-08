using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

namespace ClimbGames.Client
{
    public class FanArtTreeData : TreeData
    {
        public FanArtData fanArt;

        public FanArtTreeData(string name, int depth, int id) : base(name, depth, id)
        {

        }
    }

    class FanArtTreeView : TreeView<FanArtTreeData>
    {
        const float customRowHeight = 20f;
        const float rowIconWidth = 20f;

        public FanArtTreeView(TreeViewState state, MultiColumnHeader header, TreeModel<FanArtTreeData> model) : base(state, header, model, false, true)
        {
            rowHeight = customRowHeight;
            columnIndexForTreeFoldouts = 1;
            customFoldoutYOffset = (customRowHeight - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = rowIconWidth + 1; // 
            showAlternatingRowBackgrounds = true;
        }

        protected override void OnCellGUI(Rect cellRect, TreeViewItem item, FanArtTreeData data, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);
            switch (column)
            {
                case 0:
                {
                    //if (data.info == null)
                    //    GUI.DrawTexture(cellRect, EditorGUIUtility.FindTexture("Folder Icon"), ScaleMode.ScaleToFit);
                    break;
                }
                case 1:
                {
                    if (data.fanArt == null)
                    {
                        Rect rect = cellRect;
                        rect.x += GetContentIndent(item);
                        rect.width = 20f;
                        GUI.DrawTexture(rect, EditorGUIUtility.FindTexture(IsExpanded(data.id) ? "FolderOpened Icon" : "Folder Icon"), ScaleMode.ScaleToFit);
                    }
                    
                    args.rowRect = cellRect;
                    base.OnRowGUI(cellRect, item, data, ref args);
                    break;
                }

                case 2:
                {
                    if (data.fanArt != null)
                        data.fanArt.author = GUI.TextField(cellRect, data.fanArt.author);
                    break;
                }

                case 3:
                {
                    if (data.fanArt != null)
                        GUI.DrawTexture(cellRect, AssetDatabase.GetCachedIcon(data.fanArt.assetPath), ScaleMode.ScaleToFit);
                    break;
                }

                case 4:
                {
                    if (data.fanArt != null)
                        GUI.Label(cellRect, data.fanArt.assetPath);
                    break;
                }
            }
        }

        protected override bool CanRenameData(FanArtTreeData data)
        {
            if (data.fanArt == null)
                return false;

            return base.CanRenameData(data);
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
		{
			Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
			CenterRectUsingSingleLineHeight(ref cellRect);
			return base.GetRenameRect(cellRect, row, item);
		}

        protected override void OnRenameEnded(FanArtTreeData element, RenameEndedArgs args)
        {
            base.OnRenameEnded(element, args);
            element.fanArt.title = args.newName;
		}

        protected override void ContextClicked()
        {
            Vector2 position = Event.current.mousePosition;
            EditorUtility.DisplayCustomMenu(new Rect(position.x, position.y, 0, 0), new GUIContent[] 
            {
                new GUIContent("Remove"),
            }, -1, OnSelectContextMenu, null);
        }

        void OnSelectContextMenu(object userData, string[] options, int selected)
        {
            IList<int> ids = GetSelection();
            treeModel.RemoveElements(ids);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            IList<FanArtTreeData> datas = FindDatas(selectedIds);
            List<Object> objects = new List<Object>();
            for (int i = 0; i < datas.Count; ++i)
            {
                if (datas[i].fanArt != null)
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(datas[i].fanArt.assetPath);
                    if (obj != null)
                        objects.Add(obj);
                }
            }
            Selection.objects = objects.ToArray();
        }

        //
        //
        public static MultiColumnHeaderState.Column[] CreateHeaderColumns()
        {
            return new MultiColumnHeaderState.Column[]
            {
                CreateHeaderColumn(EditorGUIUtility.FindTexture("FilterByType"), 30f),
                CreateHeaderColumn("Title", 100f, 50f),
                CreateHeaderColumn("Author", 80f, 40f),
                CreateHeaderColumn(EditorGUIUtility.FindTexture("ViewToolOrbit"), 40f),
                CreateHeaderColumn("Path", 200f, 200f),
            };
        }
    }
}