using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

namespace ClimbGames.Editor
{
    public class FanArtTreeElement : TreeElement
    {
        public FanArtData data;

        public FanArtTreeElement(string name, int depth, int id) : base(name, depth, id)
        {

        }
    }

    class FanArtTreeView : TreeView<FanArtTreeElement>
    {
        const float customRowHeight = 20f;
        const float rowIconWidth = 20f;

        public FanArtTreeView(TreeViewState<int> state, MultiColumnHeader header, TreeModel<FanArtTreeElement> model) : base(state, header, model, false, true)
        {
            rowHeight = customRowHeight;
            columnIndexForTreeFoldouts = 1;
            customFoldoutYOffset = (customRowHeight - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = rowIconWidth + 1; // 
            showAlternatingRowBackgrounds = true;
        }

        protected override void OnCellGUI(Rect cellRect, TreeViewItem item, FanArtTreeElement element, int column, ref RowGUIArgs args)
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
                        if (element != null)
                        {
                            Rect rect = cellRect;
                            rect.x += GetContentIndent(item);
                            rect.width = 20f;
                            GUI.DrawTexture(rect, EditorGUIUtility.FindTexture(IsExpanded(element.id) ? "FolderOpened Icon" : "Folder Icon"), ScaleMode.ScaleToFit);
                        }

                        args.rowRect = cellRect;
                        base.OnRowGUI(cellRect, item, element, ref args);
                        break;
                    }

                case 2:
                    {
                        if (element != null && element.data != null)
                            element.data.author = GUI.TextField(cellRect, element.data.author);
                        break;
                    }

                case 3:
                    {
                        if (element != null && element.data != null)
                            GUI.DrawTexture(cellRect, AssetDatabase.GetCachedIcon(element.data.assetPath), ScaleMode.ScaleToFit);
                        break;
                    }

                case 4:
                    {
                        if (element != null && element.data != null)
                            GUI.Label(cellRect, element.data.assetPath);
                        break;
                    }
            }
        }

        protected override bool CanRenameData(FanArtTreeElement element)
        {
            if (element.data == null)
                return false;

            return base.CanRenameData(element);
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem<int> item)
        {
            Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
            CenterRectUsingSingleLineHeight(ref cellRect);
            return base.GetRenameRect(cellRect, row, item);
        }

        protected override void OnRenameEnded(FanArtTreeElement element, RenameEndedArgs args)
        {
            base.OnRenameEnded(element, args);
            element.data.title = args.newName;
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
            IList<FanArtTreeElement> datas = FindDatas(selectedIds);
            List<Object> objects = new List<Object>();
            for (int i = 0; i < datas.Count; ++i)
            {
                if (datas[i].data != null)
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(datas[i].data.assetPath);
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