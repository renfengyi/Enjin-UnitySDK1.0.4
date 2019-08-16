using UnityEngine;
using UnityEditor;
using EnjinEditorPanel;
using EnjinSDK;

public class TT_RolesPanels
{
    #region Declairations & Properties
    // Private variables & Objects
    private TT_RolesListPanel _rolesListPanel;
    private TT_PermissionsPanel _permissionsPanel;
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public TT_RolesPanels()
    {
        _rolesListPanel = new TT_RolesListPanel();
        _permissionsPanel = new TT_PermissionsPanel();
    }

    /// <summary>
    /// Roles management panel. Create, Edit & View
    /// </summary>
    public void DrawTeamRolesPanel(TeamController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 10, 912, 606), skin.GetStyle("TopBackground"));
        EditorGUILayout.LabelField(new GUIContent("ROLE MANAGEMENT"), skin.GetStyle("MainTitle"));
        GUILayout.Space(30);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);

        if (controller.State == TeamState.MANAGEROLES)
            EditorGUILayout.LabelField(new GUIContent("ROLES"), skin.GetStyle("Subtitle"), GUILayout.Width(430));
        else if (controller.State == TeamState.CREATEROLE)
            EditorGUILayout.LabelField(new GUIContent("CREATE NEW ROLE"), skin.GetStyle("Subtitle"), GUILayout.Width(430));
        else if (controller.State == TeamState.EDITROLE)
            EditorGUILayout.LabelField(new GUIContent("EDIT ROLE"), skin.GetStyle("Subtitle"), GUILayout.Width(430));

        GUILayout.Space(16);
        EditorGUILayout.LabelField(new GUIContent("PERMISSIONS"), skin.GetStyle("Subtitle"), GUILayout.Width(350));

        if (controller.State == TeamState.CREATEROLE)
        {
            if (GUILayout.Button("Select All", GUILayout.Width(80)))
                controller.SetAllPermissions();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.Width(430), GUILayout.Height(440));
        _rolesListPanel.BuildRolesList(controller, skin);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.Width(430), GUILayout.Height(440));
        _permissionsPanel.BuildPermissionList(controller, skin);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (controller.State != TeamState.EDITROLE && controller.PanelLevelIndex == 0)
        {
            if (GUILayout.Button("CREATE", GUILayout.Width(100), GUILayout.Height(36)))
            {
                if (controller.State != TeamState.CREATEROLE)
                    controller.CreateRoleMode();
            }
        }

        if (controller.State != TeamState.CREATEROLE && controller.PanelLevelIndex == 0)
        {
            
            if (GUILayout.Button("EDIT", GUILayout.Width(100), GUILayout.Height(36)))
                if (controller.RolesList[controller.SelectedRoleIndex].name == "Player")
                    EditorUtility.DisplayDialog("WARNING", "The role \"Player\" is required and cannot be modified.", "ok");
                else
                    controller.EditRoleMode();
        }

        if (controller.State == TeamState.MANAGEROLES)
        {
            if (GUILayout.Button("DELETE", GUILayout.Width(100), GUILayout.Height(36)))
            {
                if (controller.RolesList[controller.SelectedRoleIndex].name == "Player")
                    EditorUtility.DisplayDialog("WARNING", "The role \"Player\" is a base role and cannot be deleted.", "ok");
                else if (controller.RolesList[controller.SelectedRoleIndex].name == "Admin")
                    EditorUtility.DisplayDialog("WARNING", "The role \"Admin\" is a base role and cannot be deleted.", "ok");
                else 
                    if (EditorUtility.DisplayDialog("WARNING", "Are you sure you want to delete this Role?", "ok", "cancel"))
                        controller.DeleteRole();
            }
        }

        if (controller.State == TeamState.CREATEROLE)
        {
            if (GUILayout.Button("CREATE", GUILayout.Width(100), GUILayout.Height(36)))
            {
                if (controller.IsRoleNameValid(controller.NewRoleName))
                {
                    if (controller.GetSelectedPermissions(controller.PermissionList).Length == 0)
                        EditorUtility.DisplayDialog("NO PERMISSIONS SET", "Role can not have an empty set of permissions.", "OK");
                    else if (Enjin.CreateRole(controller.NewRoleName, controller.GetSelectedPermissions(controller.PermissionList)))
                    {
                        EditorUtility.DisplayDialog("SUCCESS", "Role successfully created.", "OK");
                        controller.ManageRolesMode();
                    }
                }
                else
                    EditorUtility.DisplayDialog("INVALID ROLE NAME", "The role name you have entered is already assigned. Select a different role name.", "OK");
            }
        }

        if (controller.State == TeamState.EDITROLE)
        {
            if (GUILayout.Button("UPDATE", GUILayout.Width(100), GUILayout.Height(36)))
            {
                bool success = false;

                if (controller.OldRoleName == controller.NewRoleName)
                    success = Enjin.UpdateRole(controller.NewRoleName, controller.GetSelectedPermissions(controller.PermissionList));
                else
                {
                    if (controller.IsRoleNameValid(controller.NewRoleName))
                        success = Enjin.UpdateRole(controller.OldRoleName, controller.NewRoleName, controller.GetSelectedPermissions(controller.PermissionList));
                    else
                        EditorUtility.DisplayDialog("INVALID ROLE NAME", "The role name you have entered is already assigned. Select a different role name.", "OK");
                }

                if (success)
                    controller.ManageRolesMode();
            }
        }

        if (GUILayout.Button("BACK", GUILayout.Width(100), GUILayout.Height(36)))
            controller.PanelLevelBack();

        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.EndArea();
    }
}