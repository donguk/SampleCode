using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UIElements;

namespace ClimbGames.Client
{
    public class SimpleTreeViewWindow : EditorWindow
    {
        TreeModel<TreeData> treeModel;
        SimpleTreeView treeView;
        TreeViewState treeViewState = new TreeViewState();
        
        Rect treeViewRect;
        DragAndDropManipulator manipulator;

        void InitIfNeeded()
        {
            if (treeView == null)
            {
                List<TreeData> datas = new List<TreeData>()
                {
                    new TreeData("root", -1, 0),
                    new TreeData("aa", 0, 1),
                    new TreeData("bb", 1, 2),
                    new TreeData("cc", 1, 3),
                    new TreeData("dd", 0, 4),
                    new TreeData("ee", 1, 5),
                };

                treeModel = new TreeModel<TreeData>(datas);
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