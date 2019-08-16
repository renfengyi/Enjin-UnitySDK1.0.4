using UnityEngine;
using UnityEditor;
using EnjinSDK;

public class TT_InvitePanel
{
    private string _email;

    public TT_InvitePanel()
    {
        _email = string.Empty;
    }

    public void DrawInviteUserPanel(TeamController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 126, 912, 490), skin.GetStyle("TopBackground"));
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("INVITE USER"), skin.GetStyle("MainTitle"));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.Space(16);
        GUILayout.BeginVertical();
        EditorGUILayout.LabelField(new GUIContent("EMAIL ADDRESS"), skin.GetStyle("Subtitle"));
        _email = EditorGUILayout.TextField(_email, skin.textField, GUILayout.Width(360), GUILayout.Height(30));
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("SEND INVITE", GUILayout.Width(100), GUILayout.Height(30)))
        {
            Enjin.InviteUser(_email, "");
        }

        if (GUILayout.Button("BACK", GUILayout.Width(100), GUILayout.Height(30)))
        {
            controller.SetTeamState(EnjinEditorPanel.TeamState.VIEWLIST);
            _email = string.Empty;
            controller.SetTeamState(EnjinEditorPanel.TeamState.VIEWLIST);
        }

        GUILayout.Space(16);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.EndArea();
    }
}