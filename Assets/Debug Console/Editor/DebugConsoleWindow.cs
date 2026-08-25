using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DebugConsoleWindow : EditorWindow
{
    private const float ToggleWidth = 60f;

    private const float ButtonWidth = 80f;

    [MenuItem("Custome Tools/Debug Console")]
    public static void Init()
    {
        DebugConsoleWindow window = (DebugConsoleWindow)GetWindow(typeof(DebugConsoleWindow));

        window.titleContent = new GUIContent("Console");

        window.Show();
    }

    private Vector2 scrollVector;

    private GUIStyle style = new GUIStyle();

    private void Awake()
    {
        style.richText = true;

        style.padding.left = 3;

        style.padding.top = 3;

        style.padding.bottom = 2;

        autoRepaintOnSceneChange = true;

        CustomTool.Bitwise.AddRef(ref CustomTool.Log.Categories, (int)CustomTool.Log.Category.Debug);
    }

    public enum Ref
    {
        Texture,
        AnimationClip,
        AudioClip,
    }

    private void GenericMenuCallback(object type_)
    {

    }

    private void OnGUI()
    {
        Rect rect;

        GUILayout.BeginHorizontal("Toolbar");

        if (GUILayout.Button(EditorApplication.isPaused ? "Play" : "Pause", "ToolbarButton", GUILayout.Width(ButtonWidth)))
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }

        if (GUILayout.Button("Clear", "ToolbarButton", GUILayout.Width(ButtonWidth)))
        {
            CustomTool.Log.Clear();
        }

        if (GUILayout.Button("Save", "ToolbarButton", GUILayout.Width(ButtonWidth)))
        {
            string fileName = "Log_" + System.DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss");

            string filePath = UnityEditor.EditorUtility.SaveFilePanel("Save", Application.persistentDataPath, fileName, "txt");

            if (!string.IsNullOrEmpty(filePath))
            {
                CustomTool.Log.SaveToLocal(filePath);
            }
        }

        rect = GUILayoutUtility.GetLastRect();

        if (GUILayout.Button("Reference", "ToolbarButton", GUILayout.Width(ButtonWidth)))
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i <= (int)Ref.AudioClip; ++i)
            {
                menu.AddItem(new GUIContent(((Ref)i).ToString()), true, GenericMenuCallback, (Ref)i);
            }

            rect.x += ButtonWidth;

            menu.DropDown(rect);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.Separator();

        int length = System.Enum.GetValues(typeof(CustomTool.Log.Category)).Length;

        for (int i = 0; i < length; ++i)
        {
            CustomTool.Log.Category category = (CustomTool.Log.Category)(0x01 << i);

            bool value = GUILayout.Toggle(CustomTool.Bitwise.Check(CustomTool.Log.Categories, (int)category), category.ToString(), "ToolbarButton", GUILayout.Width(ToggleWidth));

            if (value)
            {
                CustomTool.Bitwise.AddRef(ref CustomTool.Log.Categories, (int)category);
            }
            else
            {
                CustomTool.Bitwise.SubRef(ref CustomTool.Log.Categories, (int)category);
            }
        }

        GUILayout.EndHorizontal();

        rect = GUILayoutUtility.GetLastRect();

        GUILayout.BeginHorizontal(/*GUI.skin.box*/);
        scrollVector = GUILayout.BeginScrollView(scrollVector);

        float height = 0;

        List<CustomTool.Log.Message> list = CustomTool.Log.CategorizedLogs;

        int count = list.Count;

        for (int i = Mathf.Clamp(count - 1500, 0, count); i < list.Count; ++i)
        {
            if (CustomTool.Bitwise.Check(CustomTool.Log.Categories, (int)list[i].category))
            {
                GUILayout.Label("<color=silver>" + list[i].text + "</color>", style);

                //GUILayout.TextArea(items[i].text);

                //GUILayout.TextField(items[i].text);
                rect = GUILayoutUtility.GetLastRect();
                if (rect.Contains(Event.current.mousePosition))
                {
                    // Handle events here
                }

                height += rect.height;
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndHorizontal();

        rect = GUILayoutUtility.GetLastRect();

        if (!Event.current.isMouse)
        {
            if (rect.height > style.lineHeight)
            {
                float currentHeight = rect.height + scrollVector.y;
                if (currentHeight >= height)
                {
                    scrollVector.y = currentHeight;
                }
            }
        }

        //scrollVector.y = Mathf.Infinity;

        //Repaint();
    }
}
