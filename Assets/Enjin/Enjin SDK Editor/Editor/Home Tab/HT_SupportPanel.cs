using UnityEditor;
using UnityEngine;

public class HT_SupportPane
{
    /// <summary>
    /// Draws the support panel
    /// </summary>
    public void DrawSupportPanel(GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(386, 498, 530, 118), skin.GetStyle("TopBackground"));
        EditorGUILayout.LabelField(new GUIContent("SUPPORT"), skin.GetStyle("MainTitle"));
        EditorGUILayout.BeginVertical(GUILayout.Width(500));
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);

        if (GUILayout.Button(new GUIContent("Website"), GUILayout.Height(28)))
            Application.OpenURL("http://enjincoin.io");

        if (GUILayout.Button(new GUIContent("Support"), GUILayout.Height(28)))
            Application.OpenURL("http://enjin.io/support");

        if (GUILayout.Button(new GUIContent("Knowledge Base"), GUILayout.Height(28)))
            Application.OpenURL("https://kovan.cloud.enjin.io/docs/enjin");

        if (GUILayout.Button(new GUIContent("Forums"), GUILayout.Height(28)))
            Application.OpenURL("https://forum.enjin.io/");

        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("v1.0.401.PB1");
        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
    }
}