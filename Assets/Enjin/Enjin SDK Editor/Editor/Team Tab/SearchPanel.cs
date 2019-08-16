using UnityEngine;
using UnityEditor;

public class SearchPanel
{
    /// <summary>
    /// Draws the search panel
    /// </summary>
    public void DrawTeamSearchPanel(TeamController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(248, 10, 670, 100), skin.GetStyle("TopBackground"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("SEARCH TEAM"), skin.GetStyle("MainTitle"), GUILayout.Width(130));
        EditorGUILayout.BeginVertical();
        GUILayout.Space(12);
        EditorGUILayout.LabelField(new GUIContent("ID, Username, Email"), skin.GetStyle("ContentDark"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        controller.SearchText = EditorGUILayout.TextField(controller.SearchText, skin.textField, GUILayout.Height(30));

        if (GUILayout.Button(new GUIContent("Search"), GUILayout.Height(32), GUILayout.Width(100)))
            controller.SearchUsers();

        if (controller.SearchText == "" && !controller.HasRefreshedList)
        {
            controller.ResetSearch();
            controller.ResetTeamList();
            GUI.FocusControl(null);
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}