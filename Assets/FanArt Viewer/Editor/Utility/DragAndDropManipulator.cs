using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ClimbGames.Client
{
    public class DragAndDropManipulator
    {
        public Rect dropArea;
        public GUIStyle dropStyle;
        public List<Object> hierarchyObjects = new List<Object>();
        public List<Object> projectObjects = new List<Object>();
        public List<string> filePaths = new List<string>();

        public bool IsDragArea => DragAndDrop.visualMode == DragAndDropVisualMode.Copy;
        public bool IsDragPerform { get; private set; }
  
        public DragAndDropManipulator(string style)
        {
            dropStyle = new GUIStyle(style)
            {
                alignment = TextAnchor.MiddleCenter,
            };
        }

        public DragAndDropManipulator(Rect dropArea, string style = "box") : this(style)
        {
            this.dropArea = dropArea;
        }

        public DragAndDropManipulator(EditorWindow window, string style = "box") : this(style)
        {
            Rect rect = window.position;
            rect.x = rect.y = 0;
            dropArea = rect;
        }

        void ClaerObjects()
        {
            hierarchyObjects.Clear();
            projectObjects.Clear();
        }
         
        void PerformDrag()
        {        
            // GameObjects from hierarchy.
            if (DragAndDrop.paths.Length == 0 && DragAndDrop.objectReferences.Length > 0)
            {
                hierarchyObjects.AddRange(DragAndDrop.objectReferences);
            }
            // Object outside project. It mays from File Explorer (Finder in OSX).
            else if (DragAndDrop.paths.Length > 0 && DragAndDrop.objectReferences.Length == 0)
            {
                filePaths.AddRange(DragAndDrop.paths);
            }
            // Unity Assets including folder.
            else if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
            {
                projectObjects.AddRange(DragAndDrop.objectReferences);
            }
            // Log to make sure we cover all cases.
            else
            {
                Debug.Log("Out of reach");
                Debug.Log("Paths:");
                foreach (string path in DragAndDrop.paths)
                {
                    Debug.Log("- " + path);
                }

                Debug.Log("ObjectReferences:");
                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    Debug.Log("- " + obj);
                }
            }
        }

        public void ProcessEvent()
        {
            ClaerObjects();
            IsDragPerform = false;

            var current = Event.current;
            switch (current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    {
                        if (!dropArea.Contains(current.mousePosition))
                            break;

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (IsDragPerform = current.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            PerformDrag();
                        }
                    }

                    Event.current.Use();
                    break;
            }
        }
    }
}