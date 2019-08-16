namespace EnjinEditorPanel
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using EnjinSDK;
    using GraphQLClient;

    public enum PlatformState { VIEW, CREATE, EDIT }
    public enum TeamState { VIEWLIST, MANAGEROLES, EDITROLE, CREATEROLE, CREATE, EDIT, INVITEUSER, REFRESH }

    public enum Status { NONE, SUCCESS, FAILURE };
    public enum ProcessTasks { VIEW, CREATE, MELT, MINT, EDIT, DELETE, SETURI, REFRESH };

    public class Result
    {
        public enum Types { NONE, IDENTITIES, CRYPTOITEMS }

        public Request Request { get; set; }
        public Status Status { get; set; }
        public string Message { get; set; }
        public bool Refresh { get; set; }
        public Types Type { get; set; }
        public List<CompoundQuery> compoundQueries;

        public Result()
        {
            Type = Types.NONE;
            Status = Status.NONE;
            Message = "";
            Request = null;
            Refresh = false;
            compoundQueries = new List<CompoundQuery>();
        }
    }

    public class CompoundQuery
    {
        public ProcessTasks Task { get; set; }
        public CryptoItem CryptoItem { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public CompoundQuery(ProcessTasks task, CryptoItem cryptoItem, Dictionary<string, object> properties)
        {
            this.Task = task;
            this.CryptoItem = cryptoItem;
            this.Properties = properties;
        }
    }

    public class EnjinEditor
    {
        #region Enumerators & Delegates
        // Enumerators
        public enum CallMethod
        {
            INITILAIZEPLATFORM, LOGOUT, REFRESHAPPLIST, REFRESHUSERROLES, CHECKAPPCHANGE,
            RELOADITEMS, RELOADTEAM, RELOADIDENTITIES, RELOADALL, UPDATEITEMSPERPAGE
        }
        #endregion

        #region Declairations & Properties
        private static List<string> _userPermissions;
        private static Dictionary<string, List<string>> _userRoles;

        // private editor Class Objects
        private static int _lastTabIndex;
        private static HomeTab _homeTab;
        private static TeamTab _teamTab;
        private static IdentitiesTab _identitiesTab;
        private static CryptoItemsTab _CryptoItemsTab;
        private static WalletTab _walletTab;
        private static SettingsTab _settingsTab;

        // Properties
        public static decimal MinCostScale { get; private set; }
        public static decimal CostScaleDivider { get; private set; }
        public static int SelectedAppIndex { get; set; }
        public static int LastSelectedApp { get; set; }
        public static int ItemsPerPage { get { return _settingsTab.ItemsPerPage; } }
        public static string CurrentAppName { get { return AppsNameList[SelectedAppIndex]; } }
        public static GUISkin SkinTheme { get; set; }
        public static List<string> AppsNameList { get; private set; }
        public static Identity CurrentUserIdentity { get; private set; }
        public static List<AppSelectorData> AppList { get; private set; }
        public static User CurrentUser { get; set; }
        public static NotificationMonitor NotificationMonitor { get; set; }
        #endregion

        /// <summary>
        /// Initialization of Editor
        /// </summary>
        public static void Init()
        {
            Enjin.IsDebugLogActive = true;
            _lastTabIndex = 0;
            SelectedAppIndex = 0;
            LastSelectedApp = 0;
            MinCostScale = 100000000;
            CostScaleDivider = 1000000000;
            _homeTab = new HomeTab(SkinTheme);
            //_teamTab = new TeamTab(SkinTheme);
            //_identitiesTab = new IdentitiesTab(SkinTheme);
            //_CryptoItemsTab = new CryptoItemsTab(SkinTheme);
            //_walletTab = new WalletTab(SkinTheme);
            //_settingsTab = new SettingsTab(SkinTheme);
            NotificationMonitor = new NotificationMonitor();
        }

        /// <summary>
        /// Checks the selected tab and executes the selected tab panek
        /// </summary>
        /// <param name="tabIndex">Index of selected tab</param>
        /// <returns>Tab index</returns>
        public static int TabSelection(int tabIndex)
        {
            if (Enjin.IsLoggedIn && Enjin.AppID != -1)
            {
                switch (tabIndex)
                {
                    case 0:
                        _homeTab.DrawHomeTab();

                        if (_lastTabIndex != tabIndex)
                        {
                            _teamTab.ResetSearch();
                            _identitiesTab.ResetSearch();
                            _CryptoItemsTab.ResetSearch();
                            _lastTabIndex = tabIndex;
                        }
                        break;

                    case 1:
                        _teamTab.DrawTeamTab();

                        if (_lastTabIndex != tabIndex)
                        {
                            _identitiesTab.ResetSearch();
                            _CryptoItemsTab.ResetSearch();
                            _lastTabIndex = tabIndex;
                        }
                        break;

                    case 2:
                        _identitiesTab.DrawIdentityTab();

                        if (_lastTabIndex != tabIndex)
                        {
                            _teamTab.ResetSearch();
                            _CryptoItemsTab.ResetSearch();
                            _lastTabIndex = tabIndex;
                        }
                        break;

                    case 3:
                        if (CurrentUserIdentity.ethereum_address != "" || CurrentUserIdentity.ethereum_address != null)
                        {
                            _CryptoItemsTab.DrawCryptoItemsTab();

                            if (_lastTabIndex != tabIndex)
                            {
                                _teamTab.ResetSearch();
                                _identitiesTab.ResetSearch();
                                _lastTabIndex = tabIndex;
                            }
                        }
                        break;

                    case 4:
                        _walletTab.DrawWalletTab();

                        if (_lastTabIndex != tabIndex)
                        {
                            _teamTab.ResetSearch();
                            _identitiesTab.ResetSearch();
                            _CryptoItemsTab.ResetSearch();
                            _lastTabIndex = tabIndex;
                        }
                        break;

                    case 5:
                        _settingsTab.DrawSettingsTab();

                        if (_lastTabIndex != tabIndex)
                        {
                            _teamTab.ResetSearch();
                            _identitiesTab.ResetSearch();
                            _CryptoItemsTab.ResetSearch();
                            _lastTabIndex = tabIndex;
                        }
                        break;

                    default:
                        Debug.Log("Option has no end point");
                        break;
                }
            }
            else
            {
                tabIndex = 0;
                _homeTab.DrawHomeTab();
            }

            return tabIndex;
        }

        /// <summary>
        /// Gets the min melt value when creating cryptoItems
        /// </summary>
        /// <param name="reserve">Number of cryptoItems that will be prepaid</param>
        /// <returns></returns>
        public static string GetMinMeltValue(int reserve) { return GraphQuery.GetEndPointData(Enjin.MeltValueURL + reserve.ToString()); }

        public static string GetAllowance(string ethAddress) { return GraphQuery.GetEndPointData(Enjin.AllowanceURL + ethAddress + "/allowance"); }

        /// <summary>
        /// Returns true/false based on the state provided from a Request
        /// </summary>
        /// <param name="state">Request State for testing</param>
        /// <returns></returns>
        public static bool IsRequestSuccessfull(string state)
        {
            if (state == null)
                return false;

            switch(state.ToLower())
            {
                case "confirmed":
                case "executed":
                case "broadcast":
                case "pending":
                    return true;
                case "canceled_user":
                case "canceled_platform":
                case "failed":
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if a user has the correct role
        /// </summary>
        /// <param name="role">Role to check for</param>
        /// <returns>(True / False) if user has the specified role</returns>
        public static bool HasRole(UserRoles role)
        {
            if (_userRoles.ContainsKey(role.ToString()))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if a user has the correct permissions to access editor content / features
        /// </summary>
        /// <param name="perm">Permission to check for</param>
        /// <returns>(True / False) if user has permission</returns>
        public static bool HasPermission(UserPermission perm)
        {
            if (_userPermissions.Contains(perm.ToString()))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if a user has the correct permissions to access editor content / features based on a specified role
        /// </summary>
        /// <param name="role">Role to check for</param>
        /// <param name="perm">Permission to check for</param>
        /// <returns>(True / False) if user has role AND permission</returns>
        public static bool HasPermission(UserRoles role, UserPermission perm)
        {
            if (_userRoles.ContainsKey(role.ToString()))
                if (_userRoles[role.ToString()].Contains(perm.ToString()))
                    return true;

            return false;
        }

        public static void Log(string data) { Debug.Log("<color=yellow>[DEBUG LOG OUTPUT]</color> " + data); }

        public static void DisplayDialog(string title, string message)
        {
            NotificationMonitor.DisplayDialog(title, message);
        }

        /// <summary>
        /// Cleanly logs user out when editor window is closed
        /// </summary>
        private void OnDestroy()
        {
            if (Enjin.IsLoggedIn)
                ExecuteMethod(CallMethod.LOGOUT);
        }

        /// <summary>
        /// Event callback for executing methods
        /// </summary>
        /// <param name="methodCall">Method flag for executing method</param>
        public static void ExecuteMethod(CallMethod methodCall)
        {
            switch (methodCall)
            {
                case CallMethod.LOGOUT:
                    Logout();
                    break;

                case CallMethod.REFRESHAPPLIST:
                    RefreshAppList();
                    break;

                case CallMethod.INITILAIZEPLATFORM:
                    InitializePlatform();
                    break;

                case CallMethod.CHECKAPPCHANGE:
                    CheckAppChanged();
                    break;

                case CallMethod.REFRESHUSERROLES:
                    RefreshUserRoles();
                    break;

                case CallMethod.RELOADITEMS:
                    EditorUtility.DisplayProgressBar("Loading", "Loading CryptoItems", 0.66f);
                    _CryptoItemsTab.ResetCryptoItemList();
                    _walletTab.UpdateWalletBalances();
                    EditorUtility.ClearProgressBar();
                    break;

                case CallMethod.RELOADTEAM:
                    _teamTab.ResetTeamList();
                    break;

                case CallMethod.RELOADIDENTITIES:
                    _identitiesTab.ResetIdentityList();
                    CurrentUserIdentity = Enjin.GetIdentity(AppList[SelectedAppIndex].identityID);
                    break;

                case CallMethod.RELOADALL:
                    RefreshUserRoles();
                    EditorUtility.DisplayProgressBar("Loading", "Loading Team List", 0.33f);
                    _teamTab.ResetTeamList();
                    EditorUtility.DisplayProgressBar("Loading", "Loading CryptoItems", 0.66f);
                    _CryptoItemsTab.ResetCryptoItemList();
                    EditorUtility.DisplayProgressBar("Loading", "Loading Identity List", 1.0f);
                    _identitiesTab.ResetIdentityList();
                    CurrentUserIdentity = Enjin.GetIdentity(AppList[SelectedAppIndex].identityID);
                    _walletTab.UpdateWalletBalances();
                    EditorUtility.ClearProgressBar();
                    break;

                case CallMethod.UPDATEITEMSPERPAGE:
                    EditorUtility.DisplayProgressBar("UPDATING", "Team List Items Per", 0.33f);
                    _teamTab.ResetTeamList();
                    EditorUtility.DisplayProgressBar("UPDATING", "Identities List Items Per", 0.66f);
                    _CryptoItemsTab.ResetCryptoItemList();
                    EditorUtility.DisplayProgressBar("UPDATING", "CryptoItems List Items Per", 1.0f);
                    _identitiesTab.ResetIdentityList();
                    EditorUtility.ClearProgressBar();
                    break;
            }
        }

        /// <summary>
        /// Cleanly logs out of the platform
        /// </summary>
        private static void Logout()
        {
            Enjin.CleanUpPlatform();
            SelectedAppIndex = 0;
        }

        /// <summary>
        /// Creates a list of all apps team member has access to
        /// </summary>
        /// <param name="identities">Current team members identity</param>
        private static void RefreshAppList()
        {
            AppList = new List<AppSelectorData>(Enjin.GetAppsByUserID(CurrentUser.id));

            if (AppList.Count == 0)
            {
                Enjin.AppID = -1;
                return;
            }

            AppsNameList = new List<string>();

            foreach (AppSelectorData app in AppList)
                AppsNameList.Add(app.appName);

            Enjin.AppID = AppList[SelectedAppIndex].appID;
            CurrentUserIdentity = Enjin.GetIdentity(AppList[SelectedAppIndex].identityID);
            CurrentUser = Enjin.GetUser(CurrentUser.id);

            if (CurrentUserIdentity == null || CurrentUserIdentity.id == 0)
            {
                for (int i = 0; i < CurrentUser.identities.Length; i++)
                    if (CurrentUser.identities[i].app_id == Enjin.AppID)
                        CurrentUserIdentity = CurrentUser.identities[i];
            }
        }

        /// <summary>
        /// Initializes the platform
        /// </summary>
        private static void InitializePlatform()
        {
            RefreshAppList();

            if (Enjin.AppID != -1)
            {
                RefreshUserRoles();
                _settingsTab = new SettingsTab(SkinTheme);
                EditorUtility.DisplayProgressBar("Loading", "Loading Team List", 0.5f);
                _teamTab = new TeamTab(SkinTheme);
                EditorUtility.DisplayProgressBar("Loading", "Loading Identities List", 0.6f);
                _identitiesTab = new IdentitiesTab(SkinTheme);
                EditorUtility.DisplayProgressBar("Loading", "Loading CryptoItems List", 0.7f);
                _CryptoItemsTab = new CryptoItemsTab(SkinTheme);
                EditorUtility.DisplayProgressBar("Loading", "Loading Settings", 1.0f);
                _walletTab = new WalletTab(SkinTheme);
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// Checks if the user has selected a different application
        /// </summary>
        private static void CheckAppChanged()
        {
            if (SelectedAppIndex != LastSelectedApp)
            {
                LastSelectedApp = SelectedAppIndex;
                Enjin.AppID = AppList[SelectedAppIndex].appID;
                CurrentUserIdentity = Enjin.GetIdentity(AppList[SelectedAppIndex].identityID);

                ExecuteMethod(CallMethod.RELOADALL);
                _settingsTab.CheckAllowance();
                Enjin.ResetPusher();
            }
        }

        /// <summary>
        /// Refreshes the current users roles & permissions
        /// </summary>
        private static void RefreshUserRoles()
        {
            CurrentUser = Enjin.GetUser(CurrentUser.id);
            _userPermissions = new List<string>();
            _userRoles = new Dictionary<string, List<string>>();

            for (int i = 0; i < CurrentUser.roles.Length; i++)
            {
                string roleName = CurrentUser.roles[i].name.ToUpper();
                roleName = roleName.Replace(" ", "_"); // converts PLATFORM OWNER to
                List<string> permissions = new List<string>();

                for (int n = 0; n < CurrentUser.roles[i].permissions.Length; n++)
                {
                    if (!permissions.Contains(CurrentUser.roles[i].permissions[n].name))
                        permissions.Add(CurrentUser.roles[i].permissions[n].name);

                    if (!_userPermissions.Contains(CurrentUser.roles[i].permissions[n].name))
                        _userPermissions.Add(CurrentUser.roles[i].permissions[n].name);
                }

                if (_userRoles.ContainsKey(roleName))
                    _userRoles.Remove(roleName);

                _userRoles.Add(roleName, permissions);
            }
        }
    }
}