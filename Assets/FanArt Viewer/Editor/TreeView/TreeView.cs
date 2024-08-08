using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;
using UnityEngine;

namespace ClimbGames.Client
{
    class TreeView<T> : TreeViewWithTreeModel<T> where T : TreeElement
    {
        bool canDrag, canRename;

        public TreeView(TreeViewState state, TreeModel<T> model, 
            bool canDrag = false, 
            bool canRename = false) : base(state, model)
        {
            this.canDrag = canDrag;
            this.canRename = canRename;

            Reload();
        }

        public TreeView(TreeViewState state, MultiColumnHeader header, TreeModel<T> model, 
            bool canDrag = false, 
            bool canRename = false) : base(state, header, model)
        {
            this.canDrag = canDrag;
            this.canRename = canRename;

            Reload();
        }

        protected IList<T> FindDatas(IList<int> ids)
        {
            IList<TreeViewItem> items = FindRows(ids);
            return items.Select(item => (item as TreeViewItem<T>).data).ToList();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            TreeViewItem<T> item = args.item as TreeViewItem<T>;
            if (multiColumnHeader != null)
            {
                int count = args.GetNumVisibleColumns();
                for (int i = 0; i < count; ++i)
                {
                    OnCellGUI(args.GetCellRect(i), item, item.data, args.GetColumn(i), ref args);
                }
            }
            else
            {
                OnRowGUI(args.rowRect, item, item.data, ref args);
            }
        }

        protected virtual void OnRowGUI(Rect rect, TreeViewItem item, T data, ref RowGUIArgs args) 
        {
            base.RowGUI(args);
        }

        protected virtual void OnCellGUI(Rect cellRect, TreeViewItem item, T data, int column, ref RowGUIArgs args)
        {
            if (column == 0)
            {                
                args.rowRect = cellRect;
                base.RowGUI(args);
            }
        }

        protected override void SingleClickedItem(int id)
        {
            
        }

        protected override void ContextClickedItem(int id)
        {
            
        }

        protected override void ContextClicked()
        {
            
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            
        }

        //
        //
        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return canDrag;
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return canRename && CanRenameData((item as TreeViewItem<T>).data);
        }

        protected virtual bool CanRenameData(T data)
        {
            return canRename;
        }

        protected override void RenameEnded(RenameEndedArgs args)
		{
			// Set the backend name and reload the tree to reflect the new model
			if (args.acceptedRename)
			{
				var element = treeModel.Find(args.itemID);
                OnRenameEnded(element, args);
				Reload();
			}
		}

        protected virtual void OnRenameEnded(T element, RenameEndedArgs args)
        {
            element.name = args.newName;
        }

        protected static MultiColumnHeaderState.Column CreateHeaderColumn(string name, Texture2D image, string tooltip, string menuText, float width, float minWidth, float maxWidth = 1000000f, bool canSort = true)
        {
            return new MultiColumnHeaderState.Column 
				{
					headerContent = new GUIContent(name, image, tooltip),
					contextMenuText = menuText,
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = false,
					sortingArrowAlignment = TextAlignment.Right,
					width = width, 
					minWidth = minWidth,
					maxWidth = maxWidth,
					autoResize = true,
					allowToggleVisibility = true,
                    canSort = canSort,
				};
        }

        protected static MultiColumnHeaderState.Column CreateHeaderColumn(string name, float width, float minWidth)
        {
            return CreateHeaderColumn(name, default, default, default, width, minWidth);
        }

        protected static MultiColumnHeaderState.Column CreateHeaderColumn(Texture2D image, float width, bool canSort = false)
        {
            return CreateHeaderColumn(default, image, default, default, width, width, width, canSort);
        }

        public static MultiColumnHeader GetMultiColumnHeader(ref MultiColumnHeaderState columnHeaderState, MultiColumnHeaderState.Column[] columns)
        {
            // column header
            bool resizeToFit = columnHeaderState == null;
            MultiColumnHeaderState headerState = new MultiColumnHeaderState(columns);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(columnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(columnHeaderState, headerState);
            
            columnHeaderState = headerState;
            MultiColumnHeader columnHeader = new MultiColumnHeader(columnHeaderState);
            if (resizeToFit)
                columnHeader.ResizeToFit();

            return columnHeader;
        }
    }
}