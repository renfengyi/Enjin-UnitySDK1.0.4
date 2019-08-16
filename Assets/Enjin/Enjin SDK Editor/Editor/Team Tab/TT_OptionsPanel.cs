using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;

public class TT_OptionsPanel
{
    /// <summary>
    /// Draws the team options panel
    /// </summary>
    public void DrawTeamOptionsPanel(TeamController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 10, 230, 100), skin.GetStyle("TopBackground"));
        EditorGUILayout.LabelField(new GUIContent("TEAM OPTIONS"), skin.GetStyle("MainTitle"));
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
        GUILayout.Space(14);

        if (controller.HasPermission(UserPermission.manageUsers))
        {
            if (GUILayout.Button(new GUIContent("Create Member"), GUILayout.Height(36), GUILayout.Width(100)))
                controller.CreateMode();
        }

        if (controller.HasPermission(UserPermission.manageRoles))
        {
            if (GUILayout.Button(new GUIContent("Manage Roles"), GUILayout.Height(36), GUILayout.Width(100)))
                controller.SetTeamState(TeamState.MANAGEROLES);
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
