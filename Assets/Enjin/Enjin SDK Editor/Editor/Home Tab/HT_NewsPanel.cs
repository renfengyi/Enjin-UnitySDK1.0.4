using UnityEditor;
using UnityEngine;
using EnjinSDK;

public class HT_NewsPanel
{
    #region Definitions & Properties
    // Private variables & objects
    private Vector2 _scrollPos;             // Scroll position for news articles
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public HT_NewsPanel()
    {
        _scrollPos = Vector2.zero;
    }

    /// <summary>
    /// Draws & populates the news panel
    /// </summary>
    public void DrawNewsPanel(HomeController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(386, 10, 530, 476), skin.GetStyle("TopBackground"));
        EditorGUILayout.LabelField(new GUIContent("NEWS"), skin.GetStyle("MainTitle"));
        GUILayout.Space(10);

        if (controller.NewsData == null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent("News Currently Unavailable"), skin.GetStyle("MainTitle"));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
            return;
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(10);
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(510), GUILayout.Height(426));

        foreach (News newsData in controller.NewsData)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            if (newsData.title.Length > 68)
                EditorGUILayout.LabelField(new GUIContent(newsData.title.Substring(0, 68) + ".."), skin.GetStyle("BoldTitle"));
            else
                EditorGUILayout.LabelField(new GUIContent(newsData.title), skin.GetStyle("BoldTitle"));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(6);
            EditorGUILayout.LabelField(new GUIContent(newsData.description), skin.GetStyle("ContentDark"), GUILayout.Width(470));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Read More", GUILayout.Width(80)))
                Application.OpenURL(newsData.link);

            GUILayout.Space(14);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}