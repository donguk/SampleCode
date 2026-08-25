using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

namespace ClimbGames.Editor
{
    public class SimpleTreeViewWindow : EditorWindow
    {
        TreeModel<TreeElement> treeModel;
        SimpleTreeView treeView;
        TreeViewState<int> treeViewState = new TreeViewState<int>();

        Rect treeViewRect;
        DragAndDropManipulator manipulator;

        void InitIfNeeded()
        {
            if (treeView == null)
            {
                List<TreeElement> datas = new List<TreeElement>()
                {
                    new TreeElement("root", -1, 0),
                    new TreeElement("aa", 0, 1),
                    new TreeElement("bb", 1, 2),
                    new TreeElement("cc", 1, 3),
                    new TreeElement("dd", 0, 4),
                    new TreeElement("ee", 1, 5),
                };

                treeModel = new TreeModel<TreeElement>(datas);
                treeView = new SimpleTreeView(treeViewState, treeModel);
            }

            if (manipulator == null)
                manipulator = new DragAndDropManipulator(this);
        }

        void OnGUI()
        {
            InitIfNeeded();

            if (manipulator != null)
            {
                manipulator.ProcessEvent();

                //
                //
            }

            if (treeView != null)
            {
                treeViewRect.width = position.width;
                treeViewRect.height = position.height;
                treeView.OnGUI(treeViewRect);
            }

            if (manipulator.IsDragArea)
                GUI.Box(manipulator.dropArea, "Drop Assets", manipulator.dropStyle);
        }

        [MenuItem("ClimbGames/Samples/Simple TreeView")]
        public static SimpleTreeViewWindow ShowWindow()
        {
            var window = GetWindow<SimpleTreeViewWindow>();
            window.titleContent = new GUIContent("Simple TreeView");
            window.Show();
            return window;
        }
    }
}