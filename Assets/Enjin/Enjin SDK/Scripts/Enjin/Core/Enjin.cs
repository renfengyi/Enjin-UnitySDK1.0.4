namespace EnjinSDK
{
    using UnityEngine;
    using SimpleJSON;
    using GraphQLClient;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine.Networking;
    using EnjinSDK.Helpers;

    public class Enjin
    {
        #region Definitions
        // Private static variables & objects
        private static bool _isLoggedIn = false;
        private static string _apiURL;
        private static string _meltValueURL;
        private static string _allowanceURL;
        private static string _accessToken;
        private static string _graphQLEndPoint;
        private static Dictionary<int, System.Action<RequestEvent>> _requestCallbacks;
        private static Dictionary<string, List<System.Action<RequestEvent>>> _eventListeners;
        private static ResponseCodes _serverCode;
        private static LoginState _loginState;
        private static UserCredentials _userCreds;
        private static EnjinIdentities _identities;
        private static EnjinCryptoItems _cryptoItems;
        private static EnjinUsers _users;
        private static EnjinRequests _requests;
        private static EnjinPlatform _platform;

        // Public properties
        public static double GetEnjBalance { get { return _identities.EnjBalance; } }
        public static double GetEthBalance { get { return _identities.EthBalance; } }
        public static string GraphQLURL { get { return _graphQLEndPoint; } }
        public static string APIURL { get { return _apiURL; } }
        public static string MeltValueURL { get { return _meltValueURL; } }
        public static string AllowanceURL { get { return _allowanceURL; } }
        public static string AccessToken { get { return _accessToken; } set { _accessToken = value; } }
        public static bool IsDebugLogActive { get; set; }
        public static bool IsLoggedIn { get { return _isLoggedIn; } set { _isLoggedIn = value; } }
        public static bool IsRequestValid(long code, string response) { return IsRequestResponseValid(code, response); }
        public static ErrorStatus ErrorReport { get; private set; }
        public static Dictionary<int, System.Action<RequestEvent>> RequestCallbacks { get { return _requestCallbacks; } set { _requestCallbacks = value; } }
        public static Dictionary<string, List<System.Action<RequestEvent>>> EventListeners { get { return _eventListeners; } set { _eventListeners = value; } }
        public static GraphQLTemplate UserTemplate { get; private set; }
        public static GraphQLTemplate PlatformTemplate { get; private set; }
        public static GraphQLTemplate CryptoItemTemplate { get; private set; }
        public static GraphQLTemplate IdentityTemplate { get; private set; }
        public static GraphQLTemplate RequestTemplate { get; private set; }

        // Enums
        public static ResponseCodes ServerResponse { get { return _serverCode; } }
        public static LoginState LoginState { get { return _loginState; } }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes primary objects for platform use
        /// </summary>
        private static void StartUp()
        {
            _requestCallbacks = new Dictionary<int, System.Action<RequestEvent>>();
            _eventListeners = new Dictionary<string, List<System.Action<RequestEvent>>>();
            ErrorReport = new ErrorStatus();

            _accessToken = "";

            UserTemplate = new GraphQLTemplate("UserTemplate");
            PlatformTemplate = new GraphQLTemplate("PlatformTemplate");
            CryptoItemTemplate = new GraphQLTemplate("CryptoItemTemplate");
            IdentityTemplate = new GraphQLTemplate("IdentityTemplate");
            RequestTemplate = new GraphQLTemplate("RequestTemplate");

            _identities = new EnjinIdentities();
            _cryptoItems = new EnjinCryptoItems();
            _users = new EnjinUsers();
            _requests = new EnjinRequests();
            _platform = new EnjinPlatform();
        }

        /// <summary>
        /// Reports errors on server interaction if any
        /// </summary>
        /// <param name="code">Error code returned from server</param>
        /// <param name="response">Response description from server</param>
        private static bool IsRequestResponseValid(long code, string response)
        {
            if (response.Contains("errors"))
            {
                var errorGQL = JSON.Parse(response);
                ErrorReport.ErrorCode = errorGQL["errors"][0]["code"].AsInt;
                ErrorReport.ErrorMessage = errorGQL["errors"][0]["message"].Value;

                if (ErrorReport.ErrorCode != 0)
                    _serverCode = (ResponseCodes)System.Convert.ToInt32(ErrorReport.ErrorCode);
                else
                    _serverCode = ResponseCodes.INTERNAL;

                if (IsDebugLogActive)
                    Debug.Log("<color=red>[ERROR RESPONSE]</color> " + response);
            }
            else
                _serverCode = (ResponseCodes)System.Convert.ToInt32(code);

            bool status = true;

            switch (_serverCode)
            {
                case ResponseCodes.NOTFOUND:
                    Debug.Log("<color=red>[ERROR 404]</color> Request Not Found: " + response);
                    ResetErrorReport();
                    status = false;
                    break;

                case ResponseCodes.INVALID:
                    Debug.Log("<color=red>[ERROR 405]</color> Invalid Call to Serving URL: " + response);
                    ResetErrorReport();
                    status = false;
                    break;

                case ResponseCodes.DATACONFLICT:
                    Debug.Log("<color=red>[ERROR 409]</color> Object Already Exisits: " + response);
                    ResetErrorReport();
                    status = false;
                    break;

                case ResponseCodes.BADREQUEST:
                    Debug.Log("<color=red>[ERROR 400]</color> Bad Request: " + response);
                    ResetErrorReport();
                    status = false;
                    break;

                case ResponseCodes.UNAUTHORIZED:
                    Debug.Log("<color=red>[ERROR 401]</color> Unauthorized Request: " + response);
                    ResetErrorReport();
                    status = false;
                    break;

                case ResponseCodes.INTERNAL:
                    Debug.Log("<color=red>[ERROR 999]</color> Internal Request Bad: " + response);
                    ResetErrorReport();
                    status = false;
                    break;
            }

            return status;
        }

        /// <summary>
        /// Initializes the platform
        /// </summary>
        private static void InitializePlatform()
        {
            _platform.InitializePlatform();
            _serverCode = ResponseCodes.INITIALIZED;
        }

        /// <summary>
        /// Sets all the url endpoints using the base API url as the prefix
        /// </summary>
        /// <param name="baseURL">Base API url prefix</param>
        private static void SetupAPI(string baseURL)
        {
            _apiURL = baseURL;

            if (_apiURL.EndsWith("/"))
            {
                _graphQLEndPoint = _apiURL + "graphql";
                _meltValueURL = _apiURL + "api/v1/ethereum/get-min-melt-value/";
                _allowanceURL = _apiURL + "api/v1/ethereum/";
            }
            else
            {
                _graphQLEndPoint = _apiURL + "/graphql";
                _meltValueURL = _apiURL + "/api/v1/ethereum/get-min-melt-value/";
                _allowanceURL = _apiURL + "/api/v1/ethereum/";
            }
        }

        /// <summary>
        /// Starts Platform and sets the platform ID automaticly
        /// </summary>
        /// <param name="baseAPIURL">Base API URL connector</param>
        /// <param name="email">User email address</param>
        /// <param name="password">User password</param>
        /// <returns>User if valid. Null if not valid</returns>
        public static User StartPlatform(string baseAPIURL, string email, string password) { return StartPlatform(baseAPIURL, email, password, -1); }

        /// <summary>
        /// Starts Platform and sets the platform ID manually
        /// </summary>
        /// <param name="baseAPIURL">Base API URL connector</param>
        /// <param name="email">User email address</param>
        /// <param name="password">User password</param>
        /// <param name="appID">Application ID</param>
        /// <returns>User if valid. Null if not valid</returns>
        public static User StartPlatform(string baseAPIURL, string email, string password, int appID)
        {
            StartUp();

            SetupAPI(baseAPIURL);
            User user = Login(email, password);

            if (_loginState == LoginState.VALID)
            {
                InitializePlatform();

                if (appID != -1)
                    _platform.ApplicationID = appID;

                return user;
            }

            return null;
        }

        /// <summary>
        /// Initializes entire platform when used as a server
        /// </summary>
        /// <param name="baseAPIURL">Base API URL</param>
        /// <param name="username">Developer account username</param>
        /// <param name="password">Developer account password</param>
        public static void ServerStartUp(string baseAPIURL, string username, string password)
        {
            Debug.LogWarning("DEPRESICATED: Use StartPlatform() Method for login processes");
            StartUp();
            SetupAPI(baseAPIURL);
            Login(username, password);

            if (_loginState == LoginState.VALID)
                InitializePlatform();
            else
                Application.Quit();
        }

        /// <summary>
        /// Cleans up the platform when exiting the applicaiton
        /// </summary>
        public static void CleanUpPlatform()
        {
            // Clean up any platform connections here
            _loginState = LoginState.NONE;
            _isLoggedIn = false;
            _accessToken = "";
            _platform.CleanUp();
        }

        /// <summary>
        /// Logs user into platform
        /// </summary>
        /// <param name="username">User's username</param>
        /// <param name="password">User's password</param>
        /// <returns></returns>
        public static User Login(string username, string password)
        {
            User currentUser = new User();

            if (PlayerPrefs.HasKey("UserData"))
            {
                try
                {
                    _userCreds = JsonUtility.FromJson<UserCredentials>(PlayerPrefs.GetString("UserData"));

                    if (username == _userCreds.email && password == _userCreds.key)
                    {
                        AppID = _userCreds.appID;
                        _accessToken = _userCreds.accessToken;
                        currentUser = GetUserRaw(_userCreds.userID);
                        currentUser.access_token = _accessToken;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning("[EXECUTION WARNING] " + e.Message);
                    PlayerPrefs.DeleteKey("UserData");
                }
            }

            if (currentUser != null)
            {
                _loginState = LoginState.VALID;
                GraphQuery.POST(string.Format(PlatformTemplate.GetQuery["Login"], username, password), "login");

                switch (_serverCode)
                {
                    case ResponseCodes.DATACONFLICT:
                    case ResponseCodes.UNAUTHORIZED:
                    case ResponseCodes.INTERNAL:
                    case ResponseCodes.BADREQUEST:
                    case ResponseCodes.NOTFOUND:
                        _loginState = LoginState.INVALIDUSERPASS;
                        break;

                    case ResponseCodes.UNKNOWNERROR:
                        _loginState = LoginState.INVALIDTPURL;
                        break;

                    case ResponseCodes.SUCCESS:
                        var resultGQL = JSON.Parse(GraphQuery.queryReturn);

                        _accessToken = resultGQL["data"]["result"]["access_tokens"][0]["access_token"].Value;
                        AppID = resultGQL["data"]["result"]["identities"][0]["app_id"].AsInt;
                        currentUser = GetUserRaw(resultGQL["data"]["result"]["id"].AsInt);
                        StoreUserData(currentUser, password);
                        break;
                }
            }

            currentUser.access_token = _accessToken;

            return currentUser;
        }

        /// <summary>
        /// Verifies the user login
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="password">user password</param>
        /// <returns>User if valid null if not valid</returns>
        public static User VerifyLogin(string username, string password)
        {
            _loginState = LoginState.VALID;
            User currentUser = new User();

            string _query = @"query login{result:EnjinOauth(email:""$user^"",password:""$pass^""){id,access_tokens,roles{name}identities{app_id}}}";
            GraphQuery.variable["user"] = username;
            GraphQuery.variable["pass"] = password;
            GraphQuery.POST(_query, "login");

            if (_serverCode == ResponseCodes.DATACONFLICT || _serverCode == ResponseCodes.UNAUTHORIZED || _serverCode == ResponseCodes.BADREQUEST || _serverCode == ResponseCodes.NOTFOUND)
                _loginState = LoginState.INVALIDUSERPASS;
            else if (_serverCode == ResponseCodes.UNKNOWNERROR)
                _loginState = LoginState.INVALIDTPURL;
            else if (_serverCode == ResponseCodes.INTERNAL)
                _loginState = LoginState.INVALIDUSERPASS;

            if (_serverCode == ResponseCodes.SUCCESS)
            {
                var resultGQL = JSON.Parse(GraphQuery.queryReturn);
                currentUser = GetUser(resultGQL["data"]["result"]["id"].AsInt);
                currentUser.access_token = resultGQL["data"]["result"]["access_tokens"][0]["access_token"].Value;
            }

            return currentUser;
        }

        /// <summary>
        /// Stores the users credintials for fast login once user has logged in once
        /// </summary>
        /// <param name="user">User object to check stored values against</param>
        /// <param name="key">User key to validate user credentials</param>
        public static void StoreUserData(User user, string key)
        {
            _userCreds = new UserCredentials
            {
                key = key,
                appID = AppID,
                email = user.email,
                accessToken = _accessToken,
                userID = user.id
            };

            PlayerPrefs.SetString("UserData", JsonUtility.ToJson(_userCreds));
        }

        /// <summary>
        /// Method to validate an Ethereum address
        /// </summary>
        /// <param name="address">Address to validate</param>
        /// <returns>True if address is valid, false otherwise</returns>
        public static bool ValidateAddress(string address)
        {
            Regex r = new Regex("^(0x){1}[0-9a-fA-F]{40}$");
            Regex r2 = new Regex("^(0x)?[0-9A-F]{40}$");

            if (r.IsMatch(address) || r2.IsMatch(address))
                return true;

            return false;
        }

        /// <summary>
        /// Gets data from a URL Endpoint
        /// </summary>
        /// <param name="url">URI address end point</param>
        /// <param name="headers">Header data</param>
        /// <returns></returns>
        public static string URLGetData(string url, Dictionary<string, string> headers)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);

            foreach (var header in headers)
                request.SetRequestHeader(header.Key, header.Value);

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SendWebRequest();

            while (!request.isDone) { }

            if (!request.isNetworkError || !request.isHttpError)
                return request.downloadHandler.text;
            else
                Debug.LogError("<color=red>[ERROR]</red> " + request.error);

            return string.Empty;
        }

        public static void ResetErrorReport() { ErrorReport = new ErrorStatus(); }
        #endregion

        #region Identity Methods
        public static bool UnLinkIdentity(int id) { return _identities.UnLink(id); }
        public static bool DeleteIdentity(string id) { return _identities.Delete(id); }
        public static void UpdateBalances(Identity identity) { _identities.UpdateBalances(identity); }
        public static Identity GetIdentity(int id) { return _identities.Get(id); }
        public static Identity[] GetAllIdentities() { return _identities.GetAll(); }
        public static Identity[] SearchIdentities(string term) { return _identities.SearchIdentities(term); }
        public static Identity CreateIdentity(Identity newIdentity) { return _identities.Create(newIdentity); }
        public static Identity UpdateIdentity(Identity identity) { return _identities.Update(identity); }
        public static Identity UpdateIdentityFields(int id, Fields[] fields) { return _identities.UpdateFields(id, fields); }
        public static Identity LinkIdentity(Identity identity) { return _identities.Link(identity); }
        public static Roles[] GetRoles() { return _identities.GetRoles(); }
        public static PaginationHelper<Identity> GetIdentities(int page, int limit) { return _identities.GetAllByApp(page, limit); }
        #endregion

        #region CryptoItem Methods
        public static int GetCryptoItemBalance(int identityID, string tokenID) { return _cryptoItems.GetCryptoItemBalance(identityID, tokenID); }
        public static Dictionary<int, List<CryptoItem>> GetCryptoItemsByAddress(string ethereumAddress) { return _cryptoItems.GetCryptoItemsByAddress(ethereumAddress); }
        public static string GetMintableItems(string itemID) { return _cryptoItems.GetMintableItems(itemID); }
        public static string GetCryptoItemIDByName(string name) { return _cryptoItems.GetItemIDByName(name); }
        public static Dictionary<string, int> GetCryptoItemBalances(int identityID) { return _cryptoItems.GetAllBalancesForID(identityID); }
        public static CryptoItem UpdateCryptoItem(CryptoItem item) { return _cryptoItems.UpdateCryptoItem(item); }
        public static CryptoItem GetCryptoItem(string id) { return _cryptoItems.Get(id); }
        public static CryptoItem[] GetAllCryptoItems() { return _cryptoItems.GetAll(); }
        public static CryptoItem[] SearchCryptoItems(string term) { return _cryptoItems.SearchCryptoItems(term); }
        public static PaginationHelper<CryptoItem> GetAllItems(int page, int limit, int identityID) { return _cryptoItems.GetItems(page, limit, identityID); }
        public static PaginationHelper<CryptoItem> GetAllItems(int identityID) { return _cryptoItems.GetItems(1, 0, identityID); }
        #endregion

        #region Platform Methods
        public static int AppID { get { return _platform.ApplicationID; } set { _platform.ApplicationID = value; } }
        public static int GetTotalActiveTokens(int identityID) { return _platform.GetActiveTokens(identityID); }
        public static void ResetPusher() { _platform.PusherReconnect(); }
        public static void SetAllowance(int identityID) { _platform.SetMaxAllowance(identityID); }
        public static bool UpdateRole(string name, string[] permissions) { return _platform.UpdateRole(name, permissions); }
        public static bool UpdateRole(string name, string newName, string[] permissions) { return _platform.UpdateRole(name, newName, permissions); }
        public static List<AppSelectorData> GetAppsByUserID(int userID) { return _platform.GetAppsByUserID(userID); }
        public static bool CreateRole(string name, string[] permissions) { return _platform.CreateRole(name, permissions); }
        public static PlatformInfo GetPlatformInfo { get { return _platform.GetPlatform; } }
        public static Fields[] DefaultFields { get { return _platform.DefaultFields; } }
        public static App GetAppByID(int appID) { return _platform.GetAppByID(appID); }
        public static App CreateApp(App app) { return _platform.CreateApp(app); }
        public static App UpdateApp(App app) { return _platform.UpdateApp(app); }
        public static void InviteUser(string email) { _platform.InviteUserToApp(email); }
        public static void InviteUser(string email, string name) { _platform.InviteUserToApp(email, name); }
        #endregion

        #region Request Methods
        public static bool DeleteRequest(int requestID) { return _requests.Delete(requestID); }
        public static bool CancelRequest(int requestID) { return _requests.Cancel(requestID); }
        public static string GetCryptoItemURI(string itemID, string itemIndex = "0", bool replaceTags = true) { return _requests.GetCryptoItemURI(itemID, itemIndex, replaceTags); }
        public static Request DeleteRole(Roles role) { return _platform.DeleteRole(role); }
        public static Request CreateCryptoItem(CryptoItem item, int identityID) { return _cryptoItems.CreateCryptoItem(item, identityID); }
        public static Request CreateCryptoItem(CryptoItem item, int identityID, System.Action<RequestEvent> callback) { return _cryptoItems.CreateCryptoItem(item, identityID, callback); }
        public static Request MintFungibleItem(int senderID, string[] addresses, string itemID, int value, bool async = false) { return _requests.MintFungibleItem(senderID, addresses, itemID, value, null, async); }
        public static Request MintFungibleItem(int senderID, string[] addresses, string itemID, int value, System.Action<RequestEvent> callback, bool async = false)
        {
            if (!async)
            {
                Request request = MintFungibleItem(senderID, addresses, itemID, value, async);
                RequestCallbacks.Add(request.id, callback);
                return request;
            }
            else
            {
                _requests.MintFungibleItem(senderID, addresses, itemID, value, (queryReturn) =>
                {
                    Request fullRequest = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(queryReturn, 2));
                    RequestCallbacks.Add(fullRequest.id, callback);
                }, async);
                return null;
            }
        }
        public static Request MintNonFungibleItem(int senderID, string[] addresses, string itemID, bool async = false) { return _requests.MintNonFungibleItem(senderID, addresses, itemID, null, async); }
        public static Request MintNonFungibleItem(int senderID, string[] addresses, string itemID, System.Action<RequestEvent> callback, bool async = false)
        {
            if (!async)
            {
                Request request = MintNonFungibleItem(senderID, addresses, itemID, async);
                RequestCallbacks.Add(request.id, callback);
                return request;
            }
            else
            {
                _requests.MintNonFungibleItem(senderID, addresses, itemID, (queryReturn) =>
                 {
                     Request fullRequest = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(queryReturn, 2));
                     RequestCallbacks.Add(fullRequest.id, callback);
                 }, async);
                return null;
            }
        }
        public static Request SetCryptoItemURI(int identityID, CryptoItem item, string URI, System.Action<RequestEvent> callback) { return _requests.SetCryptoItemURI(identityID, item, URI, callback); }
        public static Request GetRequest(int requestID) { return _requests.Get(requestID); }
        public static Request[] GetAllRequests() { return _requests.GetAll(); }
        public static Request SendCryptoItemRequest(int identityID, string tokenID, int recipientID, int value, bool async = false) { return _requests.SendItem(identityID, tokenID, recipientID, value, null, async); }
        public static Request SendCryptoItemRequest(int identityID, string tokenID, int recipientID, int value, System.Action<RequestEvent> callback, bool async = false)
        {
            if (!async)
            {
                Request request = SendCryptoItemRequest(identityID, tokenID, recipientID, value, async);
                RequestCallbacks.Add(request.id, callback);
                return request;
            }
            else
            {
                _requests.SendItem(identityID, tokenID, recipientID, value, (queryReturn) =>
                {
                    Request fullRequest = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(queryReturn, 2));
                    RequestCallbacks.Add(fullRequest.id, callback);
                }, async);
                return null;
            }
        }
        public static Request SendBatchCryptoItems(CryptoItemBatch items, int userID) { return _requests.SendItems(items, userID); }
        public static Request MeltTokens(int userIdentityID, string itemID, string index, int amount, bool async = false) { return _requests.MeltItem(userIdentityID, itemID, index, amount, null, async); }
        public static Request MeltTokens(int userIdentityID, string itemID, string index, int amount, System.Action<RequestEvent> callback, bool async = false)
        {
            if (!async)
            {
                Request request = MeltTokens(userIdentityID, itemID, index, amount, async);
                RequestCallbacks.Add(request.id, callback);
                return request;
            }
            else
            {
                _requests.MeltItem(userIdentityID, itemID, index, amount, (queryReturn) =>
                {
                    Request fullRequest = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(queryReturn, 2));
                    RequestCallbacks.Add(fullRequest.id, callback);
                }, async);
                return null;
            }
        }
        public static Request MeltTokens(int userIdentityID, string itemID, int amount, System.Action<RequestEvent> callback, bool async = false)
        {
            if (!async)
            {
                Request request = MeltTokens(userIdentityID, itemID, "", amount, async);
                RequestCallbacks.Add(request.id, callback);
                return request;
            }
            else
            {
                _requests.MeltItem(userIdentityID, itemID, "", amount, (queryReturn) =>
                {
                    Request fullRequest = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(queryReturn, 2));
                    RequestCallbacks.Add(fullRequest.id, callback);
                }, async);
                return null;
            }
        }
        public static Request UpdateCryptoItem(int identityID, CryptoItem item, CryptoItemFieldType fieldType, System.Action<RequestEvent> callback) { return _requests.UpdateCryptoItem(identityID, item, fieldType, callback); }
        public static Request CreateTradeRequest(int senderIdentityID, CryptoItem[] itemsFromSender, int[] amountsFromSender, string secondPartyAddress, CryptoItem[] itemsFromSecondParty, int[] amountsFromSecondParty)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, amountsFromSender, secondPartyAddress, null, itemsFromSecondParty, amountsFromSecondParty);
        }
        public static Request CreateTradeRequest(int senderIdentityID, CryptoItem[] itemsFromSender, int[] amountsFromSender, string secondPartyAddress, CryptoItem[] itemsFromSecondParty, int[] amountsFromSecondParty, System.Action<RequestEvent> callback)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, amountsFromSender, secondPartyAddress, null, itemsFromSecondParty, amountsFromSecondParty, callback);
        }
        public static Request CreateTradeRequest(int senderIdentityID, CryptoItem[] itemsFromSender, int[] amountsFromSender, int secondPartyIdentityID, CryptoItem[] itemsFromSecondParty, int[] amountsFromSecondParty)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, amountsFromSender, null, secondPartyIdentityID, itemsFromSecondParty, amountsFromSecondParty);
        }
        public static Request CreateTradeRequest(int senderIdentityID, CryptoItem[] itemsFromSender, int[] amountsFromSender, int secondPartyIdentityID, CryptoItem[] itemsFromSecondParty, int[] amountsFromSecondParty, System.Action<RequestEvent> callback)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, amountsFromSender, null, secondPartyIdentityID, itemsFromSecondParty, amountsFromSecondParty, callback);
        }
        public static Request CreateTradeRequest(int senderIdentityID, TokenValueInputData[] itemsFromSender, string secondPartyAddress, TokenValueInputData[] itemsFromSecondParty)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, secondPartyAddress, null, itemsFromSecondParty);
        }
        public static Request CreateTradeRequest(int senderIdentityID, TokenValueInputData[] itemsFromSender, string secondPartyAddress, TokenValueInputData[] itemsFromSecondParty, System.Action<RequestEvent> callback)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, secondPartyAddress, null, itemsFromSecondParty, callback);
        }
        public static Request CreateTradeRequest(int senderIdentityID, TokenValueInputData[] itemsFromSender, int secondPartyIdentityID, TokenValueInputData[] itemsFromSecondParty)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, null, secondPartyIdentityID, itemsFromSecondParty);
        }
        public static Request CreateTradeRequest(int senderIdentityID, TokenValueInputData[] itemsFromSender, int secondPartyIdentityID, TokenValueInputData[] itemsFromSecondParty, System.Action<RequestEvent> callback)
        {
            return _requests.CreateTradeRequest(senderIdentityID, itemsFromSender, null, secondPartyIdentityID, itemsFromSecondParty, callback);
        }
        public static Request CompleteTradeRequest(int senderIdentityID, string tradeID)
        {
            return _requests.CompleteTradeRequest(senderIdentityID, tradeID);
        }
        public static Request CompleteTradeRequest(int secondPartyID, string tradeID, System.Action<RequestEvent> callback)
        {
            return _requests.CompleteTradeRequest(secondPartyID, tradeID, callback);
        }
        #endregion

        #region User Methods
        public static string[] GetPermissionList { get { return _users.PermissionList; } }
        public static bool DeleteUser(int userID) { return _users.Delete(userID); }
        public static User CreateUser(string username, string email, string password, string role) { return _users.Create(username, email, password, role); }
        public static User UpdateUser(int userID, string username) { return _users.Update(userID, username, null, null); }
        public static User UpdateUser(int userID, string username, string email) { return _users.Update(userID, username, email, null); }
        public static User UpdateUser(int userID, string username, string email, string roles) { return _users.Update(userID, username, email, roles); }
        public static User GetUser(int userID) { return _users.Get(userID); }
        public static User GetUserRaw(int userID) { return _users.GetRaw(userID); }
        public static User[] GetAllUsers() { return _users.GetAll(); }
        public static User[] SearchUsers(string term) { return _users.SearchUsers(term); }
        public static PaginationHelper<User> GetUsersByAppID(int page, int limit) { return _users.GetAllUsersByAppID(page, limit); }
        #endregion

        #region Event Handler
        public static void BindEvent(string eventName, System.Action<RequestEvent> listener) { _platform.BindEvent(eventName, listener); }
        public static void ListenForLink(int identityID, System.Action<RequestEvent> listener)
        {
            _platform.ListenForLink(identityID, listener);
        }
        #endregion

        #region GraphQL Methods
        public static void Post(string details, string token) { GraphQuery.POST(details, token, false, null); }
        public static void Post(string details) { GraphQuery.POST(details, "", false, null); }
        public static Dictionary<string, string> SetVarible { get { return GraphQuery.variable; } set { GraphQuery.variable = value; } }

        public static string GetQueryResults { get { return GraphQuery.queryReturn; } }
        #endregion
    }
}