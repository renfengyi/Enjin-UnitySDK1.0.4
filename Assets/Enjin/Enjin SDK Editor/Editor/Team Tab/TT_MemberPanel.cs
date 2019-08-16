using UnityEngine;
using UnityEditor;
using EnjinEditorPanel;
using System.Collections.Generic;

public class TT_MemberPanel
{
    public Vector2 _roleScrollPos { get; private set; }
    public int _selectedIndex { get; private set; }

    public TT_MemberPanel()
    {
        _selectedIndex = 0;
    }

    /// <summary>
    /// Method for creating and editing team members
    /// </summary>
    public void DrawTeamMemberPanel(TeamController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 10, 912, 606), skin.GetStyle("TopBackground"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent(controller.State.ToString() + " TEAM MEMBER"), skin.GetStyle("MainTitle"));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(30);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(new GUIContent("USERNAME", "Username for user account"), skin.GetStyle("Subtitle"));

        if (controller.State == TeamState.EDIT)
            controller.UpdateUserObject.name = EditorGUILayout.TextField(controller.UpdateUserObject.name, skin.textField, GUILayout.Width(320), GUILayout.Height(30));
        else
            controller.UserObject.name = EditorGUILayout.TextField(controller.UserObject.name, skin.textField, GUILayout.Width(320), GUILayout.Height(30));

        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("EMAIL ADDRESS", "Email address of user you wish to create"), skin.GetStyle("Subtitle"));

        if (controller.State == TeamState.EDIT)
            controller.UpdateUserObject.email = EditorGUILayout.TextField(controller.UpdateUserObject.email, skin.textField, GUILayout.Width(320), GUILayout.Height(30));
        else
            controller.UserObject.email = EditorGUILayout.TextField(controller.UserObject.email, skin.textField, GUILayout.Width(320), GUILayout.Height(30));

        GUILayout.Space(20);

        if (controller.State == TeamState.CREATE)
        {
            EditorGUILayout.LabelField(new GUIContent("PASSWORD (Optional)", "Not setting a passwrod will result in a random one being generated. An invite with password will be sent to provided email address."), skin.GetStyle("Subtitle"));
            controller.UserObject.password = EditorGUILayout.TextField(controller.UserObject.password, skin.textField, GUILayout.Width(320), GUILayout.Height(30));
        }

        if (EnjinEditor.HasRole(EnjinSDK.UserRoles.PLATFORM_OWNER) || EnjinEditor.HasRole(EnjinSDK.UserRoles.ADMIN))
        {
            List<string> availableRoles = new List<string>();
            availableRoles = controller.GetAvailableRoles();

            GUILayout.Space(20);
            EditorGUILayout.LabelField(new GUIContent("SELECT ROLE TO ADD", "Initial role to be assigned to user. If none is selected, Player role will be assigned by default"), skin.GetStyle("Subtitle"));

            if (availableRoles.Count > 0)
            {
                EditorStyles.popup.fixedHeight = 30;
                EditorStyles.popup.fontSize = 12;
                string[] rolesArray = availableRoles.ToArray();
                controller.SelectedRoleIndex = EditorGUILayout.Popup(controller.SelectedRoleIndex, rolesArray, GUILayout.Width(120), GUILayout.Height(30));
                controller.SetLocalRoleIndex(rolesArray);
                EditorStyles.popup.fixedHeight = 15;
                EditorStyles.popup.fontSize = 11;
            }
            else
                EditorGUILayout.LabelField(new GUIContent("All Available Roles Assigned"), skin.GetStyle("LargeText"), GUILayout.Width(320), GUILayout.Height(30));

            GUILayout.Space(30);

        }

        EditorGUILayout.EndVertical();

        // BEGIN COLUMN 2
        if (controller.State != TeamState.CREATE)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(new GUIContent("ASSIGNED ROLES"), skin.GetStyle("Subtitle"));
            GUILayout.Space(10);
            _roleScrollPos = EditorGUILayout.BeginScrollView(_roleScrollPos, GUILayout.Height(450));

            foreach (string role in controller.UserCurrentRolesList)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(role), skin.GetStyle("LargeText"), GUILayout.Width(320), GUILayout.Height(30));

                if (role != "Player" && role != "Platform Owner")
                {
                    if (GUILayout.Button("REMOVE", GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        if (controller.IsEmailValid(controller.UpdateUserObject.email))
                            controller.RemoveRole(role);

                        GUI.FocusControl(null);
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUILayout.Space(30);
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (controller.State == TeamState.CREATE)
        {
            if (GUILayout.Button("CREATE", GUILayout.Width(100), GUILayout.Height(30)))
            {
                if (controller.IsEmailValid(controller.UserObject.email))
                {
                    if (controller.RoleDropDownSelection == -1)
                        controller.RoleDropDownSelection = 0;

                    controller.CreateMember();
                }

                GUI.FocusControl(null);
            }
        }
        else
        {
            if (GUILayout.Button("UPDATE", GUILayout.Width(100), GUILayout.Height(30)))
            {
                if (controller.IsEmailValid(controller.UpdateUserObject.email))
                    controller.UpdateMember();

                GUI.FocusControl(null);
            }
        }

        if (GUILayout.Button("BACK", GUILayout.Width(100), GUILayout.Height(30)))
            controller.SetTeamState(TeamState.VIEWLIST);

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.EndArea();
    }
}