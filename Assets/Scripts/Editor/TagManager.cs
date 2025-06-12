using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
///    - Search, filter, and apply tags to GameObjects.
///    - Create and add new tags with ease.
///    - Customizable tag colors for visual distinction.
///    - Access via Unity Editor's Tools menu under Tag Manager Test.
/// </summary>
public class TagManager : EditorWindow
{
    private string newTagName = "";
    private Vector2 scrollPosition;

    private string searchQuery = "";
    private List<string> filteredTags = new List<string>();
    private enum TagColor
    {
        LIGHT_RED,
        LIGHT_GREEN,
        LIGHT_BLUE,
        LIGHT_YELLOW,
        LIGHT_CYAN,
        LIGHT_MAGENTA,
        LIGHT_ORANGE,
        LIGHT_LIME,
        LIGHT_PURPLE,
        LIGHT_TEAL
    }

    private Color[] tagColors = {
        new Color(0.8f, 0.6f, 0.6f),   // LIGHT_RED
        new Color(0.6f, 0.8f, 0.6f),   // LIGHT_GREEN
        new Color(0.6f, 0.6f, 0.8f),   // LIGHT_BLUE
        new Color(0.8f, 0.8f, 0.6f),   // LIGHT_YELLOW
        new Color(0.6f, 0.8f, 0.8f),   // LIGHT_CYAN
        new Color(0.8f, 0.6f, 0.8f),   // LIGHT_MAGENTA
        new Color(0.9f, 0.7f, 0.5f),   // LIGHT_ORANGE
        new Color(0.7f, 0.9f, 0.5f),   // LIGHT_LIME
        new Color(0.7f, 0.5f, 0.9f),   // LIGHT_PURPLE
        new Color(0.5f, 0.9f, 0.7f)    // LIGHT_TEAL
    };

    [MenuItem("Tools/Tag Manager Test")]
    public static void ShowWindow()
    {
        GetWindow<TagManager>("Tag Manager");
    }

    private void OnGUI()
    {
        SearchTagField();
        TagsScrollViewField();
        CreateNewTagField();
    }

    private void SearchTagField()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Search tag:", EditorStyles.boldLabel, GUILayout.Width(80));
        searchQuery = EditorGUILayout.TextField(searchQuery);
        GUILayout.EndHorizontal();
    }
    private void TagsScrollViewField()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        string[] tags = UnityEditorInternal.InternalEditorUtility.tags;

        // Filter tags based on search query
        filteredTags = FilterTags(tags, searchQuery);
        filteredTags.Sort();

        foreach (string tag in filteredTags)
        {
            DisplayTagEntry(tag);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void CreateNewTagField()
    {
        GUILayout.Space(10);
        GUILayout.Label("Create New Tag", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        newTagName = EditorGUILayout.TextField(newTagName);

        if (Event.current.isKey && Event.current.keyCode == KeyCode.Return && !string.IsNullOrEmpty(newTagName))
        {
            AddTag(newTagName);
            newTagName = "";
            Repaint();
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Add Tag") && !string.IsNullOrEmpty(newTagName))
        {
            AddTag(newTagName);
            newTagName = "";
        }

        GUILayout.EndHorizontal();
    }
    private List<string> FilterTags(string[] tags, string searchQuery)
    {
        List<string> filtered = new List<string>();

        foreach (string tag in tags)
        {
            if (string.IsNullOrEmpty(searchQuery) || tag.ToLower().Contains(searchQuery.ToLower()))
            {
                filtered.Add(tag);
            }
        }

        return filtered;
    }

    private void DisplayTagEntry(string tag)
    {
        EditorGUILayout.BeginHorizontal();

        // Display tag name with custom color as button
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        //int colorIndex = System.Array.IndexOf(UnityEditorInternal.InternalEditorUtility.tags, tag) % tagColors.Length;  // If we want to filter colors by actual TagManager indexation
        int colorIndex = filteredTags.IndexOf(tag) % tagColors.Length; // Filtering colors per filteredTag list
        buttonStyle.normal.textColor = tagColors[colorIndex];
        buttonStyle.hover.textColor = tagColors[colorIndex]; // Change text color on hover (optional)
        buttonStyle.active.textColor = tagColors[colorIndex]; // Change text color on click (optional)

        GUILayout.Button(tag, buttonStyle, GUILayout.ExpandWidth(true));

        // Apply button
        if (GUILayout.Button("Apply", GUILayout.Width(60)))
        {
            ChangeTagOfSelectedObject(tag);
        }

        // Copy button
        if (GUILayout.Button("Copy", GUILayout.Width(50)))
        {
            CopyToClipboard(tag);
        }

        EditorGUILayout.EndHorizontal();
    }
    //

    private void AddTag(string tag)
    {
        if (!TagExists(tag))
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            int arraySize = tagsProp.arraySize;
            tagsProp.InsertArrayElementAtIndex(arraySize);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(arraySize);
            newTag.stringValue = tag;
            tagManager.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("Tag already exists.");
        }
    }

    private void CopyToClipboard(string text)
    {
        TextEditor te = new TextEditor { text = text };
        te.SelectAll();
        te.Copy();
    }

    private void ChangeTagOfSelectedObject(string tag)
    {
        if (Selection.activeGameObject != null)
        {
            Undo.RecordObject(Selection.activeGameObject, "Change Tag");
            Selection.activeGameObject.tag = tag;
            EditorUtility.SetDirty(Selection.activeGameObject);
        }
        else
        {
            Debug.LogWarning("No GameObject selected.");
        }
    }


    private bool TagExists(string tag)
    {
        foreach (string existingTag in UnityEditorInternal.InternalEditorUtility.tags)
        {
            if (existingTag == tag)
            {
                return true;
            }
        }
        return false;
    }
}
