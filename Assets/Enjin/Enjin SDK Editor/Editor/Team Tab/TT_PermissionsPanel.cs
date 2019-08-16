using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;

public class TT_PermissionsPanel
{
    /// <summary>
    /// Builds the permission list based on selected role
    /// </summary>
    public void BuildPermissionList(TeamController controller, GUISkin skin)
    {
        controller.ProcessPermissionSelection();

        EditorGUILayout.BeginVertical();

        if (controller.State == TeamState.MANAGEROLES)
        {
            for (int n = 0; n < controller.PermissionList.Count; n++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Toggle(controller.PermissionList[Enjin.GetPermissionList[n]], " " + Enjin.GetPermissionList[n], GUILayout.Width(180));

                if (n + 1 < controller.PermissionList.Count)
                {
                    GUILayout.Toggle(controller.PermissionList[Enjin.GetPermissionList[n + 1]], " " + Enjin.GetPermissionList[n + 1], GUILayout.Width(180));
                    n++;
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
        }
        else
        {
            for (int n = 0; n < controller.PermissionList.Count; n++)
            {
                EditorGUILayout.BeginHorizontal();
                controller.PermissionList[Enjin.GetPermissionList[n]] = GUILayout.Toggle(controller.PermissionList[Enjin.GetPermissionList[n]], " " + Enjin.GetPermissionList[n], GUILayout.Width(180));

                if (n + 1 < controller.PermissionList.Count)
                {
                    controller.PermissionList[Enjin.GetPermissionList[n + 1]] = GUILayout.Toggle(controller.PermissionList[Enjin.GetPermissionList[n + 1]], " " + Enjin.GetPermissionList[n + 1], GUILayout.Width(180));
                    n++;
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
        }

        EditorGUILayout.EndVertical();
    }
}