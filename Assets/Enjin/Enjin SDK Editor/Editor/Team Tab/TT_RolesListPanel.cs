using UnityEngine;
using UnityEditor;
using EnjinEditorPanel;

public class TT_RolesListPanel
{
    private Vector2 _roleScrollPos;
    private Color _bgDefault;

    /// <summary>
    /// Constructor
    /// </summary>
    public TT_RolesListPanel()
    {
        _roleScrollPos = Vector2.zero;
        _bgDefault = GUI.backgroundColor;
    }

    /// <summary>
    /// Builds the role list layout
    /// </summary>
    public void BuildRolesList(TeamController controller, GUISkin skin)
    {
        if (controller.State == TeamState.MANAGEROLES)
        {
            if (controller.RolesList.Count != 0)
            {
                EditorGUILayout.BeginVertical();
                _roleScrollPos = EditorGUILayout.BeginScrollView(_roleScrollPos, GUILayout.Width(426), GUILayout.Height(436));
                GUILayout.Space(4);

                for (int i = 0; i < controller.RolesList.Count; i++)
                {
                    if (controller.SelectedRoleIndex == i)
                        EditorGUILayout.BeginHorizontal(skin.box);
                    else
                        EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(controller.RolesList[i].name, GUILayout.Width(400));
                    Rect lastRect = GUILayoutUtility.GetLastRect();

                    if (GUI.Button(new Rect(lastRect.x, lastRect.y, 400.0f, lastRect.height), GUIContent.none, skin.button))
                        controller.SelectedRoleIndex = i;

                    GUILayout.Space(8);
                    EditorGUILayout.EndHorizontal();
                    GUI.backgroundColor = _bgDefault;
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(4);
                EditorGUILayout.LabelField(new GUIContent("No Roles Assigned to application"), skin.textArea, GUILayout.Width(410));
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(4);
            EditorGUILayout.LabelField("ROLE NAME", skin.GetStyle("Subtitle"), GUILayout.Width(200));
            controller.NewRoleName = EditorGUILayout.TextField(controller.NewRoleName, skin.textField, GUILayout.Width(300), GUILayout.Height(30));
            EditorGUILayout.EndVertical();
        }
    }
}