using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;

namespace ClimbGames.Editor
{
	public class TreeViewItem : TreeViewItem<int>
	{
		public TreeElement element { get; set; }

		public TreeViewItem(TreeElement element, int depth, string displayName) : base(element.id, depth, displayName)
		{
			this.element = element;
		}
	}

	public class TreeView<T> : UnityEditor.IMGUI.Controls.TreeView<int> where T : TreeElement
	{
		private TreeModel<T> m_TreeModel;
		private readonly List<TreeViewItem<int>> m_Rows = new List<TreeViewItem<int>>(100);
		public event Action treeChanged;
		private bool canDrag, canRename;

		public TreeModel<T> treeModel { get { return m_TreeModel; } }
		public event Action<IList<TreeViewItem<int>>> beforeDroppingDraggedItems;

		public TreeView(TreeViewState<int> state, TreeModel<T> model, bool canDrag = false, bool canRename = false) : base(state)
		{
			this.canDrag = canDrag;
			this.canRename = canRename;

			Init(model);
			Reload();
		}

		public TreeView(TreeViewState<int> state, MultiColumnHeader multiColumnHeader, TreeModel<T> model, bool canDrag = false, bool canRename = false) : base(state, multiColumnHeader)
		{
			this.canDrag = canDrag;
			this.canRename = canRename;

			Init(model);
			Reload();
		}

		void Init(TreeModel<T> model)
		{
			m_TreeModel = model;
			m_TreeModel.modelChanged += ModelChanged;
		}

		void ModelChanged()
		{
			if (treeChanged != null)
				treeChanged();

			Reload();
		}

		protected override TreeViewItem<int> BuildRoot()
		{
			int depthForHiddenRoot = -1;
			return new TreeViewItem(m_TreeModel.root, depthForHiddenRoot, m_TreeModel.root.name);
		}

		protected override IList<TreeViewItem<int>> BuildRows(TreeViewItem<int> root)
		{
			if (m_TreeModel.root == null)
			{
				Debug.LogError("tree model root is null. did you call SetData()?");
			}

			m_Rows.Clear();
			if (string.IsNullOrEmpty(searchString) == false)
			{
				Search(m_TreeModel.root, searchString, m_Rows);
			}
			else
			{
				if (m_TreeModel.root.hasChildren)
					AddChildrenRecursive(m_TreeModel.root, 0, m_Rows);
			}

			// We still need to setup the child parent information for the rows since this 
			// information is used by the TreeView internal logic (navigation, dragging etc)
			SetupParentsAndChildrenFromDepths(root, m_Rows);

			return m_Rows;
		}

		void AddChildrenRecursive(T parent, int depth, IList<TreeViewItem<int>> newRows)
		{
			foreach (T child in parent.children)
			{
				var item = new TreeViewItem(child, depth, child.name);
				newRows.Add(item);

				if (child.hasChildren)
				{
					if (IsExpanded(child.id))
					{
						AddChildrenRecursive(child, depth + 1, newRows);
					}
					else
					{
						item.children = CreateChildListForCollapsedParent();
					}
				}
			}
		}

		void Search(T searchFromThis, string search, List<TreeViewItem<int>> result)
		{
			if (string.IsNullOrEmpty(search))
				throw new ArgumentException("Invalid search: cannot be null or empty", "search");

			const int kItemDepth = 0; // tree is flattened when searching

			Stack<T> stack = new Stack<T>();
			foreach (var element in searchFromThis.children)
				stack.Push((T)element);
			while (stack.Count > 0)
			{
				T current = stack.Pop();
				// Matches search?
				if (current.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					result.Add(new TreeViewItem(current, kItemDepth, current.name));
				}

				if (current.children != null && current.children.Count > 0)
				{
					foreach (var element in current.children)
					{
						stack.Push((T)element);
					}
				}
			}
			SortSearchResult(result);
		}

		protected void SortSearchResult(List<TreeViewItem<int>> rows)
		{
			rows.Sort((x, y) => EditorUtility.NaturalCompare(x.displayName, y.displayName)); // sort by displayName by default, can be overriden for multicolumn solutions
		}

		protected override IList<int> GetAncestors(int id)
		{
			return m_TreeModel.GetAncestors(id);
		}

		protected override IList<int> GetDescendantsThatHaveChildren(int id)
		{
			return m_TreeModel.GetDescendantsThatHaveChildren(id);
		}

		protected IList<T> FindDatas(IList<int> ids)
		{
			IList<TreeViewItem<int>> items = FindRows(ids);
			return items.Select(item => (item as TreeViewItem).element as T).ToList();
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			TreeViewItem item = args.item as TreeViewItem;
			if (multiColumnHeader != null)
			{
				int count = args.GetNumVisibleColumns();
				for (int i = 0; i < count; ++i)
				{
					OnCellGUI(args.GetCellRect(i), item, item.element as T, args.GetColumn(i), ref args);
				}
			}
			else
			{
				OnRowGUI(args.rowRect, item, item.element as T, ref args);
			}
		}

		protected virtual void OnRowGUI(Rect rect, TreeViewItem item, T element, ref RowGUIArgs args)
		{
			base.RowGUI(args);
		}

		protected virtual void OnCellGUI(Rect cellRect, TreeViewItem item, T element, int column, ref RowGUIArgs args)
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

		protected override bool CanRename(TreeViewItem<int> item)
		{
			return false;
		}

		protected virtual bool CanRenameData(T element)
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



		// Dragging
		//-----------

		const string k_GenericDragID = "GenericDragColumnDragging";

		protected override bool CanStartDrag(CanStartDragArgs args)
		{
			return canDrag;
		}

		protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
		{
			if (hasSearch)
				return;

			DragAndDrop.PrepareStartDrag();
			var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
			DragAndDrop.SetGenericData(k_GenericDragID, draggedRows);
			DragAndDrop.objectReferences = new UnityEngine.Object[] { }; // this IS required for dragging to work
			string title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
			DragAndDrop.StartDrag(title);
		}

		protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
		{
			// Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
			var draggedRows = DragAndDrop.GetGenericData(k_GenericDragID) as List<TreeViewItem<int>>;
			if (draggedRows == null)
				return DragAndDropVisualMode.None;

			// Parent item is null when dragging outside any tree view items.
			switch (args.dragAndDropPosition)
			{
				case DragAndDropPosition.UponItem:
				case DragAndDropPosition.BetweenItems:
					{
						bool validDrag = ValidDrag(args.parentItem, draggedRows);
						if (args.performDrop && validDrag)
						{
							T parentData = ((TreeViewItem)args.parentItem).element as T;
							OnDropDraggedElementsAtIndex(draggedRows, parentData, args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
						}
						return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
					}

				case DragAndDropPosition.OutsideItems:
					{
						if (args.performDrop)
							OnDropDraggedElementsAtIndex(draggedRows, m_TreeModel.root, m_TreeModel.root.children.Count);

						return DragAndDropVisualMode.Move;
					}
				default:
					Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
					return DragAndDropVisualMode.None;
			}
		}

		public virtual void OnDropDraggedElementsAtIndex(List<TreeViewItem<int>> draggedRows, T parent, int insertIndex)
		{
			if (beforeDroppingDraggedItems != null)
				beforeDroppingDraggedItems(draggedRows);

			var draggedElements = new List<TreeElement>();
			foreach (var x in draggedRows)
				draggedElements.Add(((TreeViewItem)x).element);

			var selectedIDs = draggedElements.Select(x => x.id).ToArray();
			m_TreeModel.MoveElements(parent, insertIndex, draggedElements);
			SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
		}

		bool ValidDrag(TreeViewItem<int> parent, List<TreeViewItem<int>> draggedItems)
		{
			TreeViewItem<int> currentParent = parent;
			while (currentParent != null)
			{
				if (draggedItems.Contains(currentParent))
					return false;
				currentParent = currentParent.parent;
			}
			return true;
		}







		// static func
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
