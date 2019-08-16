using UnityEditor;
using UnityEngine;
using EnjinSDK;

public class HT_ViewPanel
{
    /// <summary>
    /// Draws the view panel
    /// </summary>
    public void DrawViewPanel(HomeController controller, GUISkin skin)
    {
        GUILayout.Space(30);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(76);
        EditorGUILayout.LabelField(new GUIContent("TRUSTED PLATFORM URL"), skin.GetStyle("ContentDark"));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(76);
        EditorGUILayout.LabelField(new GUIContent(Enjin.APIURL), skin.GetStyle("BoldTitle"));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(30);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(76);
        EditorGUILayout.LabelField(new GUIContent("USERNAME"), skin.GetStyle("ContentDark"));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(76);
        GUILayout.Label(new GUIContent(controller.LoginInfo.username), skin.GetStyle("BoldTitle"));
        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("LOGOUT"), GUILayout.Width(120), GUILayout.Height(30)))
            controller.Logout();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
    }
}