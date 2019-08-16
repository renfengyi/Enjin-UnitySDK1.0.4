using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;

public class TeamController
{
    #region Definitions
    // Private variables & objects
    private readonly string[] _permissionList = new string[] { "viewApp", "viewUsers", "viewIdentities", "viewRequests",
            "viewEvents", "viewFields", "viewTokens", "viewRoles", "viewBalances", "manageApp", "manageUsers", "manageIdentities",
            "manageRequests", "manageFields", "manageTokens", "manageRoles", "deleteApp", "deleteUsers", "deleteIdentities",
            "deleteFields", "deleteTokens", "deleteRoles", "transferTokens", "meltTokens", "viewPlatform"};

    // Properties
    public int SelectedIndex { get; set; }
    public int RoleDropDownSelection { get; set; }
    public int PanelLevelIndex { get; set; }
    public int LastSelectedRoleIndex { get; set; }
    public int SelectedRoleIndex { get; set; }
    public int CurrentPage { get; private set; }
    public int FirstPage { get; private set; }
    public int TotalPages { get; private set; }
    public bool IsInSearchMode { get; private set; }
    public bool HasRefreshedList { get; private set; }
    public string NewRoleName { get; set; }
    public string OldRoleName { get; set; }
    public string SearchText { get; set; }
    public string UserCurrentRoles { get; private set; }
    public List<string> UserCurrentRolesList { get; private set; }
    public List<string> UserRoles { get; private set; }
    public TeamState State { get; private set; }
    public User UserObject { get; private set; }
    public User UpdateUserObject { get; set; }
    public PaginationHelper<User> Pagination { get; private set; }
    public List<User> UserList { get; private set; }
    public List<Roles> RolesList { get; private set; }
    public Dictionary<string, bool> PermissionList { get; private set; }
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public TeamController()
    {
        ResetPermissionsList();
        SetTeamState(TeamState.VIEWLIST);
        CurrentPage = 1;
        SelectedIndex = 0;
        SelectedRoleIndex = 0;
        LastSelectedRoleIndex = -1;
        FirstPage = 1;
        PanelLevelIndex = 0;
        IsInSearchMode = false;
        HasRefreshedList = true;
        SearchText = "";
        NewRoleName = "";
        GetPage(CurrentPage);
        RolesList = GetRolesList();
        UpdateUserObject = new User();
    }

    /// <summary>
    /// Sets the state of the team tab
    /// </summary>
    /// <param name="state">TeamState to set team tab state to</param>
    public void SetTeamState(TeamState state) { State = state; }

    /// <summary>
    /// Checks if user has required permission to access feature
    /// </summary>
    /// <param name="perm">permission to check for</param>
    /// <returns>true/false based on results</returns>
    public bool HasPermission(UserPermission perm) { return EnjinEditor.HasPermission(perm); }

    /// <summary>
    /// Gets a page list of users
    /// </summary>
    /// <param name="page">Current page to get</param>
    public void GetPage(int page)
    {
        Pagination = Enjin.GetUsersByAppID(page, EnjinEditor.ItemsPerPage);
        TotalPages = (int)Mathf.Ceil((float)Pagination.cursor.total / (float)Pagination.cursor.perPage);
        UserList = new List<User>(Pagination.items);
        HasRefreshedList = true;
    }

    /// <summary>
    /// Resets the team tab user list
    /// </summary>
    public void ResetTeamList()
    {
        CurrentPage = FirstPage;
        GetPage(CurrentPage);
        RolesList = GetRolesList();
        SelectedRoleIndex = 0;
        ResetPermissionsList();
        SetTeamState(TeamState.VIEWLIST);
    }

    /// <summary>
    /// Resets search
    /// </summary>
    public void ResetSearch()
    {
        SearchText = "";
        IsInSearchMode = false;
        HasRefreshedList = false;
        SelectedIndex = 0;
    }

    /// <summary>
    /// Deletes the user (soft delete)
    /// </summary>
    /// <returns>true if deleting the user was successful, otherwise false</returns>
    public bool DeleteUser()
    {
        if (UserList.Count == 1 && CurrentPage != 1)
            CurrentPage--;

        if (Enjin.DeleteUser(UserList[SelectedIndex].id))
        {
            SelectedIndex = 0;
            CurrentPage = 1;
            ResetTeamList();
            EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADIDENTITIES);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Searches for specified request
    /// </summary>
    public void SearchUsers()
    {
        UserList = new List<User>(Enjin.SearchUsers(SearchText));
        HasRefreshedList = false;
        IsInSearchMode = true;
    }

    /// <summary>
    /// Checks if the list needs to be refreshed
    /// </summary>
    /// <returns>true if list refreshed, otherwise false</returns>
    public bool IsSearchRefreshing()
    {
        if ((!HasRefreshedList) && (SearchText == ""))
        {
            ResetSearch();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Moves list to previous page if able
    /// </summary>
    public void PreviousPage()
    {
        if (CurrentPage != 1)
        {
            CurrentPage--;
            PageCheck();
        }
    }

    /// <summary>
    /// Moves list to next page if able
    /// </summary>
    public void NextPage()
    {
        if (CurrentPage != TotalPages)
        {
            CurrentPage++;
            PageCheck();
        }

    }

    /// <summary>
    /// Jumps to the specific page selected
    /// </summary>
    /// <param name="page">Page to jump to</param>
    public void SelectedPage(int page)
    {
        CurrentPage = page;
        PageCheck();
    }

    /// <summary>
    /// Initializes edit member mode
    /// </summary>
    public void EditMode()
    {
        UpdateUserObject = new User();
        UserObject = new User();
        UserObject = UserList[SelectedIndex];
        UpdateUserObject.name = UserObject.name;
        UpdateUserObject.email = UserObject.email;
        UpdateUserObject.roles = UserObject.roles;
        UserRoles = GetActiveRoles();
        GetCurrentUserRoles(SelectedIndex);

        SetTeamState(TeamState.EDIT);
    }

    /// <summary>
    /// Initializes create member mode
    /// </summary>
    public void CreateMode()
    {
        UserRoles = GetActiveRoles();
        UserObject = new User();
        UserCurrentRolesList = new List<string>();
        RoleDropDownSelection = -1;
        SetTeamState(TeamState.CREATE);
    }

    /// <summary>
    /// Initializes create role mode
    /// </summary>
    public void CreateRoleMode()
    {
        PanelLevelIndex++;
        NewRoleName = "";
        ResetPermissionsList();
        SetTeamState(TeamState.CREATEROLE);
    }

    /// <summary>
    /// Initializes edit role mode
    /// </summary>
    public void EditRoleMode()
    {
        PanelLevelIndex++;
        OldRoleName = NewRoleName = RolesList[SelectedRoleIndex].name;
        SetTeamState(TeamState.EDITROLE);
    }

    /// <summary>
    /// Deletes the role
    /// </summary>
    public void DeleteRole()
    {
        if (Enjin.DeleteRole(RolesList[SelectedRoleIndex]) != null)
        {
            RolesList = GetRolesList();
            LastSelectedRoleIndex = -1;
            SelectedRoleIndex = 0;
        }
    }

    /// <summary>
    /// Sets all the permissions
    /// </summary>
    public void SetAllPermissions()
    {
        for (int i = 0; i < _permissionList.Length; i++)
            PermissionList[_permissionList[i]] = true;
    }

    /// <summary>
    /// Resets the permission list
    /// </summary>
    public void ResetPermissionsList()
    {
        PermissionList = new Dictionary<string, bool>();

        for (int i = 0; i < _permissionList.Length; i++)
            PermissionList.Add(_permissionList[i], false);
    }

    /// <summary>
    /// Initializes manage roles mode
    /// </summary>
    public void ManageRolesMode()
    {
        RolesList = GetRolesList();
        LastSelectedRoleIndex = -1;
        SelectedRoleIndex = 0;
        NewRoleName = "";
        OldRoleName = "";
        PanelLevelIndex--;
        SetTeamState(TeamState.MANAGEROLES);
    }

    /// <summary>
    /// Gets the selected roles permission list
    /// </summary>
    /// <param name="list">Collection of permissions</param>
    /// <returns>Array of permissions</returns>
    public string[] GetSelectedPermissions(Dictionary<string, bool> list)
    {
        List<string> selectedPermissions = new List<string>();

        foreach (var perm in list)
        {
            if (perm.Value == true)
                selectedPermissions.Add(perm.Key);
        }

        return selectedPermissions.ToArray();
    }

    /// <summary>
    /// Checks the panel level and sets the state appropriately
    /// </summary>
    public void PanelLevelBack()
    {
        if (PanelLevelIndex == 0)
        {
            EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.REFRESHUSERROLES);
            SetTeamState(TeamState.VIEWLIST);
        }
        else
        {
            if (RolesList.Count != 0)
            {
                LastSelectedRoleIndex = -1;
                SelectedRoleIndex = 0;
            }

            PanelLevelIndex--;
            SetTeamState(TeamState.MANAGEROLES);
        }
    }

    /// <summary>
    /// Checks if role name is valid
    /// </summary>
    /// <param name="name">Role name to check</param>
    /// <returns>(True/False) if role name is valid</returns>
    public bool IsRoleNameValid(string name)
    {
        foreach (Roles role in RolesList)
        {
            if (role.name == name)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Updates the toggled role in the permissions list
    /// </summary>
    public void ProcessPermissionSelection()
    {
        if (State == TeamState.MANAGEROLES && RolesList.Count != 0)
        {
            //EnjinEditor.Log(LastSelectedRoleIndex + " >> " + SelectedRoleIndex);

            if (LastSelectedRoleIndex != SelectedRoleIndex)
            {
                LastSelectedRoleIndex = SelectedRoleIndex;
                ResetPermissionsList();

                for (int i = 0; i < RolesList[SelectedRoleIndex].permissions.Length; i++)
                {
                    if (PermissionList.ContainsKey(RolesList[SelectedRoleIndex].permissions[i].name))
                        PermissionList[RolesList[SelectedRoleIndex].permissions[i].name] = true;
                }
            }
        }
    }

    /// <summary>
    /// Creates a new team memeber
    /// </summary>
    public void CreateMember()
    {
        User result = null;

        if (UserObject.password == string.Empty)
        {
            result = new User();
            Enjin.InviteUser(UserObject.email, UserObject.name);
        }
        else
            result = Enjin.CreateUser(UserObject.name, UserObject.email, UserObject.password, RolesList[RoleDropDownSelection].name);

        if (result != null && Enjin.ServerResponse == ResponseCodes.SUCCESS)
        {
            if (UserObject.password == string.Empty)
                EnjinEditor.DisplayDialog("SUCCESS", "User " + UserObject.name + " successfully created and invite sent.");
            else
                EnjinEditor.DisplayDialog("SUCCESS", "User " + UserObject.name + " successfully created.");

            EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADTEAM);
            EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADIDENTITIES);
            SetTeamState(TeamState.VIEWLIST);
        }
        else
            EnjinEditor.DisplayDialog("FAILED", "Eamil address: " + UserObject.email + " has already been registered. Please enter a different email address");
    }

    /// <summary>
    /// Updates an existing team member
    /// </summary>
    public void UpdateMember()
    {
        List<string> userRoles = new List<string>(UserRoles);
        string roles = "[";

        for (int i = 0; i < UpdateUserObject.roles.Length; i++)
        {
            if (userRoles.Contains(UpdateUserObject.roles[i].name))
            {
                roles += "\"" + UpdateUserObject.roles[i].name + "\"";

                if (i != UpdateUserObject.roles.Length)
                    roles += ",";
            }
        }

        if (RoleDropDownSelection != -1)
            roles += "\"" + UserRoles[RoleDropDownSelection] + "\"]";
        else 
            roles += "]";

        if (UserObject.email == UpdateUserObject.email)
            UpdateUserObject.email = string.Empty;

        Enjin.UpdateUser(UserObject.id, UserObject.name, UpdateUserObject.email, roles);

        if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
            if (UnityEditor.EditorUtility.DisplayDialog("SUCCESS", "User updated successfully.", "OK"))
                ResetTeamList();

        SelectedRoleIndex = 0;
        EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADTEAM);
        EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADIDENTITIES);
        SetTeamState(TeamState.VIEWLIST);
    }

    /// <summary>
    /// Removes a give role from the current user
    /// </summary>
    public void RemoveRole(string role)
    {
        string roles = "[";

        for (int i = 0; i < UpdateUserObject.roles.Length; i++)
        {
            if (UpdateUserObject.roles[i].name == role)
                continue;

            roles += "\"" + UpdateUserObject.roles[i].name + "\"";

            if (i != UpdateUserObject.roles.Length)
                roles += ",";
        }

        roles += "]";

        if (UserObject.email == UpdateUserObject.email)
            UpdateUserObject.email = string.Empty;

        EnjinEditor.Log("Roles - " + roles);
        Enjin.UpdateUser(UserObject.id, UserObject.name, UpdateUserObject.email, roles);

        if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
            if (UnityEditor.EditorUtility.DisplayDialog("SUCCESS", "Removed role successfully.", "OK"))
                ResetTeamList();

        SelectedRoleIndex = 0;
        EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADTEAM);
        SetTeamState(TeamState.VIEWLIST);
    }

    /// <summary>
    /// Builds the list of roles for current user
    /// </summary>
    /// <param name="index">Index of user in list collection</param>
    public void GetCurrentUserRoles(int index)
    {
        UserCurrentRoles = "";

        if (UserList[index].roles.Length == 0)
        {
            UserCurrentRoles = "None";
            return;
        }

        UserCurrentRolesList = new List<string>();

        for (int i = 0; i < UserList[index].roles.Length; i++)
        {
            UserCurrentRolesList.Add(UserList[index].roles[i].name);

            if (i != UserList[index].roles.Length - 1)
                UserCurrentRoles += UserList[index].roles[i].name + ", ";
            else
                UserCurrentRoles += UserList[index].roles[i].name;
        }
    }

    /// <summary>
    /// Validates if an email address is valid
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>true if valid, otherwise false</returns>
    public bool IsEmailValid(string email)
    {
        try
        {
            System.Net.Mail.MailAddress address = new System.Net.Mail.MailAddress(email);
        }
        catch
        {
            if (email == string.Empty || email == null)
                EditorUtility.DisplayDialog("INVALID EMAIL", "Email address can not be empty.", "OK");
            else
                EditorUtility.DisplayDialog("INVALID EMAIL", "Invalid Email address.", "OK");

            return false;
        }

        return true;
    }

    public List<string> GetAvailableRoles()
    {
        List<string> temp = new List<string> { "None" };

        foreach (string role in UserRoles)
            if (!temp.Contains(role))
                temp.Add(role);

        foreach (string role in UserCurrentRolesList)
            if (temp.Contains(role))
                temp.Remove(role);

        if (temp.Contains("Platform Owner"))
            temp.Remove("Platform Owner");

        return temp;
    }

    public void SetLocalRoleIndex(string[] roles)
    {
        for (int i = 0; i < UserRoles.Count; i++)
        {
            string ra = "";

            if (SelectedRoleIndex < roles.Length)
            {
                if (roles[SelectedRoleIndex] != null)
                    ra = roles[SelectedRoleIndex];
            }

            if (UserRoles[i] == ra)
            {
                RoleDropDownSelection = i;
                break;
            }
            else
                RoleDropDownSelection = 0;
        }
    }

    /// <summary>
    /// Checks the current page for pagination
    /// </summary>
    private void PageCheck()
    {
        if (CurrentPage < FirstPage)
            FirstPage = CurrentPage;

        if (CurrentPage == (FirstPage + 10))
            FirstPage++;

        SelectedIndex = 0;
        GetPage(CurrentPage);
    }

    /// <summary>
    /// Gets a list of roles for the app
    /// </summary>
    /// <returns>List of roles for appr</returns>
    private List<Roles> GetRolesList()
    {
        List<Roles> roles = new List<Roles>(Enjin.GetRoles());

        if (roles != null)
        {
            for (int i = 0; i < roles.Count; i++)
            {
                if (roles[i].name.ToLower() == "platform owner")
                {
                    roles.RemoveAt(i);
                    break;
                }
            }
        }

        return roles;
    }

    /// <summary>
    /// Gets an array of active roles for app
    /// </summary>
    /// <returns>Array of active roles</returns>
    private List<string> GetActiveRoles()
    {
        List<Roles> temp1 = GetRolesList();
        List<string> temp2 = new List<string>();

        if (temp1 != null)
        {
            for (int i = 0; i < temp1.Count; i++)
            {
                temp2.Add(temp1[i].name);
            }
        }

        return temp2;
    }
}