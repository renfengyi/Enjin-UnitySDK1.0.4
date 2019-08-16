//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using GraphQLClient;
//using SimpleJSON;
//using UnityEngine.Networking;
//using UnityEngine;

//namespace EnjinSDK
//{
//    public class EnjinCore
//    {
//        #region Definitions
//        Private static variables & objects
//        private bool _isLoggedIn = false;
//        private string _apiURL;
//        private string _meltValueURL;
//        private string _allowanceURL;
//        private string _accessToken;
//        private string _graphQLEndPoint;
//        private Dictionary<int, System.Action<RequestEvent>> _requestCallbacks;
//        private Dictionary<string, List<System.Action<RequestEvent>>> _eventListeners;
//        private ResponseCodes _serverCode;
//        private LoginState _loginState;
//        private UserCredentials _userCreds;
//        private EnjinIdentities _identities;
//        private EnjinCryptoItems _cryptoItems;
//        private EnjinUsers _users;
//        private EnjinRequests _requests;
//        private EnjinPlatform _platform;

//        #region Properties
//        Public properties
//        public double GetEnjBalance { get { return _identities.EnjBalance; } }
//        public double GetEthBalance { get { return _identities.EthBalance; } }
//        public string GraphQLURL { get { return _graphQLEndPoint; } }
//        public string APIURL { get { return _apiURL; } }
//        public string MeltValueURL { get { return _meltValueURL; } }
//        public string AllowanceURL { get { return _allowanceURL; } }
//        public string AccessToken { get { return _accessToken; } set { _accessToken = value; } }
//        public bool IsDebugLogActive { get; set; }
//        public bool IsLoggedIn { get { return _isLoggedIn; } set { _isLoggedIn = value; } }
//        public bool IsRequestValid(long code, string response) { return IsRequestResponseValid(code, response); }
//        public ErrorStatus ErrorReport { get; private set; }
//        public Dictionary<int, System.Action<RequestEvent>> RequestCallbacks { get { return _requestCallbacks; } set { _requestCallbacks = value; } }
//        public Dictionary<string, List<System.Action<RequestEvent>>> EventListeners { get { return _eventListeners; } set { _eventListeners = value; } }
//        public GraphQLTemplate UserTemplate { get; private set; }
//        public GraphQLTemplate PlatformTemplate { get; private set; }
//        public GraphQLTemplate CryptoItemTemplate { get; private set; }
//        public GraphQLTemplate IdentityTemplate { get; private set; }
//        public GraphQLTemplate RequestTemplate { get; private set; }
//        #endregion

//        #region Enumerators
//        Enums
//        public ResponseCodes ServerResponse { get { return _serverCode; } }
//        public LoginState LoginState { get { return _loginState; } }
//        #endregion

//        #endregion

//        #region Methods
//        / <summary>
//        / Initializes primary objects for platform use
//        / </summary>
//        private void StartUp()
//        {
//            _requestCallbacks = new Dictionary<int, System.Action<RequestEvent>>();
//            _eventListeners = new Dictionary<string, List<System.Action<RequestEvent>>>();
//            ErrorReport = new ErrorStatus();

//            _accessToken = "";

//            UserTemplate = new GraphQLTemplate("UserTemplate");
//            PlatformTemplate = new GraphQLTemplate("PlatformTemplate");
//            CryptoItemTemplate = new GraphQLTemplate("CryptoItemTemplate");
//            IdentityTemplate = new GraphQLTemplate("IdentityTemplate");
//            RequestTemplate = new GraphQLTemplate("RequestTemplate");

//            _identities = new EnjinIdentities();
//            _cryptoItems = new EnjinCryptoItems();
//            _users = new EnjinUsers();
//            _requests = new EnjinRequests();
//            _platform = new EnjinPlatform();
//        }

//        / <summary>
//        / Reports errors on server interaction if any
//        / </summary>
//        / <param name = "code" > Error code returned from server</param>
//        / <param name = "response" > Response description from server</param>
//        private bool IsRequestResponseValid(long code, string response)
//        {
//            if (response.Contains("errors"))
//            {
//                var errorGQL = JSON.Parse(response);
//                ErrorReport.ErrorCode = errorGQL["errors"][0]["code"].AsInt;
//                ErrorReport.ErrorMessage = errorGQL["errors"][0]["message"].Value;

//                if (ErrorReport.ErrorCode != 0)
//                    _serverCode = (ResponseCodes)System.Convert.ToInt32(ErrorReport.ErrorCode);
//                else
//                    _serverCode = ResponseCodes.INTERNAL;

//                if (IsDebugLogActive)
//                    Debug.Log("<color=red>[ERROR RESPONSE]</color> " + response);
//            }
//            else
//                _serverCode = (ResponseCodes)System.Convert.ToInt32(code);

//            bool status = true;

//            switch (_serverCode)
//            {
//                case ResponseCodes.NOTFOUND:
//                    Debug.Log("<color=red>[ERROR 404]</color> Request Not Found: " + response);
//                    ResetErrorReport();
//                    status = false;
//                    break;

//                case ResponseCodes.INVALID:
//                    Debug.Log("<color=red>[ERROR 405]</color> Invalid Call to Serving URL: " + response);
//                    ResetErrorReport();
//                    status = false;
//                    break;

//                case ResponseCodes.DATACONFLICT:
//                    Debug.Log("<color=red>[ERROR 409]</color> Object Already Exisits: " + response);
//                    ResetErrorReport();
//                    status = false;
//                    break;

//                case ResponseCodes.BADREQUEST:
//                    Debug.Log("<color=red>[ERROR 400]</color> Bad Request: " + response);
//                    ResetErrorReport();
//                    status = false;
//                    break;

//                case ResponseCodes.UNAUTHORIZED:
//                    Debug.Log("<color=red>[ERROR 401]</color> Unauthorized Request: " + response);
//                    ResetErrorReport();
//                    status = false;
//                    break;

//                case ResponseCodes.INTERNAL:
//                    Debug.Log("<color=red>[ERROR 999]</color> Internal Request Bad: " + response);
//                    ResetErrorReport();
//                    status = false;
//                    break;
//            }

//            return status;
//        }

//        / <summary>
//        / Initializes the platform
//        / </summary>
//        private void InitializePlatform()
//        {
//            _platform.InitializePlatform();
//            _serverCode = ResponseCodes.INITIALIZED;
//        }

//        / <summary>
//        / Sets all the url endpoints using the base API url as the prefix
//        / </summary>
//        / <param name = "baseURL" > Base API url prefix</param>
//        private void SetupAPI(string baseURL)
//    {
//        _apiURL = baseURL;

//        if (_apiURL.EndsWith("/"))
//        {
//            _graphQLEndPoint = _apiURL + "graphql";
//            _meltValueURL = _apiURL + "api/v1/ethereum/get-min-melt-value/";
//            _allowanceURL = _apiURL + "api/v1/ethereum/";
//        }
//        else
//        {
//            _graphQLEndPoint = _apiURL + "/graphql";
//            _meltValueURL = _apiURL + "/api/v1/ethereum/get-min-melt-value/";
//            _allowanceURL = _apiURL + "/api/v1/ethereum/";
//        }
//    }

//        / <summary>
//        / Starts Platform and sets the platform ID automaticly
//        / </summary>
//        / <param name = "baseAPIURL" > Base API URL connector</param>
//        / <param name = "email" > User email address</param>
//            / <param name = "password" > User password</param>
//            / <returns>User if valid.Null if not valid</returns>
//        public User StartPlatform(string baseAPIURL, string email, string password) { return StartPlatform(baseAPIURL, email, password, -1); }

//        / <summary>
//        / Starts Platform and sets the platform ID manually
//        / </summary>
//        / <param name = "baseAPIURL" > Base API URL connector</param>
//        / <param name = "email" > User email address</param>
//            / <param name = "password" > User password</param>
//            / <param name = "appID" > Application ID</param>
//            / <returns>User if valid.Null if not valid</returns>
//        public User StartPlatform(string baseAPIURL, string email, string password, int appID)
//    {
//        StartUp();

//        SetupAPI(baseAPIURL);
//        User user = Login(email, password);

//        if (_loginState == LoginState.VALID)
//        {
//            InitializePlatform();

//            if (appID != -1)
//                _platform.ApplicationID = appID;

//            return user;
//        }

//        return null;
//    }

//        / <summary>
//        / Initializes entire platform when used as a server
//        / </summary>
//        / <param name = "baseAPIURL" > Base API URL</param>
//            / <param name = "username" > Developer account username</param>
//        / <param name = "password" > Developer account password</param>
//        public void ServerStartUp(string baseAPIURL, string username, string password)
//    {
//        Debug.LogWarning("DEPRESICATED: Use StartPlatform() Method for login processes");
//        StartUp();
//        SetupAPI(baseAPIURL);
//        Login(username, password);

//        if (_loginState == LoginState.VALID)
//            InitializePlatform();
//        else
//            Application.Quit();
//    }

//        / <summary>
//        / Cleans up the platform when exiting the applicaiton
//        / </summary>
//        public void CleanUpPlatform()
//    {
//        Clean up any platform connections here
//            _loginState = LoginState.NONE;
//        _isLoggedIn = false;
//        _accessToken = "";
//        _platform.CleanUp();
//    }

//        / <summary>
//        / Logs user into platform
//        / </summary>
//        / <param name = "username" > User's username</param>
//        / <param name = "password" > User's password</param>
//        / <returns></returns>
//        public User Login(string username, string password)
//    {
//        User currentUser = new User();

//        if (PlayerPrefs.HasKey("UserData"))
//        {
//            try
//            {
//                _userCreds = JsonUtility.FromJson<UserCredentials>(PlayerPrefs.GetString("UserData"));

//                if (username == _userCreds.email && password == _userCreds.key)
//                {
//                    AppID = _userCreds.appID;
//                    _accessToken = _userCreds.accessToken;
//                    currentUser = GetUserRaw(_userCreds.userID);
//                    currentUser.access_token = _accessToken;
//                }
//            }
//            catch (System.Exception e)
//            {
//                Debug.LogWarning("[EXECUTION WARNING] " + e.Message);
//                PlayerPrefs.DeleteKey("UserData");
//            }
//        }

//        if (currentUser != null)
//        {
//            _loginState = LoginState.VALID;
//            GraphQuery.POST(string.Format(PlatformTemplate.GetQuery["Login"], username, password), "login");

//            switch (_serverCode)
//            {
//                case ResponseCodes.DATACONFLICT:
//                case ResponseCodes.UNAUTHORIZED:
//                case ResponseCodes.INTERNAL:
//                case ResponseCodes.BADREQUEST:
//                case ResponseCodes.NOTFOUND:
//                    _loginState = LoginState.INVALIDUSERPASS;
//                    break;

//                case ResponseCodes.UNKNOWNERROR:
//                    _loginState = LoginState.INVALIDTPURL;
//                    break;

//                case ResponseCodes.SUCCESS:
//                    var resultGQL = JSON.Parse(GraphQuery.queryReturn);

//                    _accessToken = resultGQL["data"]["result"]["access_tokens"][0]["access_token"].Value;
//                    AppID = resultGQL["data"]["result"]["identities"][0]["app_id"].AsInt;
//                    currentUser = GetUserRaw(resultGQL["data"]["result"]["id"].AsInt);
//                    StoreUserData(currentUser, password);
//                    break;
//            }
//        }

//        currentUser.access_token = _accessToken;

//        return currentUser;
//    }

//        / <summary>
//        / Verifies the user login
//        / </summary>
//        / <param name = "username" > username </ param >
//        / < param name="password">user password</param>
//         / <returns>User if valid null if not valid</returns>
//        public User VerifyLogin(string username, string password)
//    {
//        _loginState = LoginState.VALID;
//        User currentUser = new User();

//        string _query = @"query login{result:EnjinOauth(email:""$user^"",password:""$pass^""){id,access_tokens,roles{name}identities{app_id}}}";
//        GraphQuery.variable["user"] = username;
//        GraphQuery.variable["pass"] = password;
//        GraphQuery.POST(_query, "login");

//        if (_serverCode == ResponseCodes.DATACONFLICT || _serverCode == ResponseCodes.UNAUTHORIZED || _serverCode == ResponseCodes.BADREQUEST || _serverCode == ResponseCodes.NOTFOUND)
//            _loginState = LoginState.INVALIDUSERPASS;
//        else if (_serverCode == ResponseCodes.UNKNOWNERROR)
//            _loginState = LoginState.INVALIDTPURL;
//        else if (_serverCode == ResponseCodes.INTERNAL)
//            _loginState = LoginState.INVALIDUSERPASS;

//        if (_serverCode == ResponseCodes.SUCCESS)
//        {
//            var resultGQL = JSON.Parse(GraphQuery.queryReturn);
//            currentUser = GetUser(resultGQL["data"]["result"]["id"].AsInt);
//            currentUser.access_token = resultGQL["data"]["result"]["access_tokens"][0]["access_token"].Value;
//        }

//        return currentUser;
//    }

//        / <summary>
//        / Stores the users credintials for fast login once user has logged in once
//        / </summary>
//        / <param name = "user" > User object to check stored values against</param>
//        public void StoreUserData(User user) { PlayerPrefs.SetString("UserData", JsonUtility.ToJson(user)); }

//        / <summary>
//        / Method to validate an Ethereum address
//        / </summary>
//        / <param name = "address" > Address to validate</param>
//            / <returns>True if address is valid, false otherwise</returns>
//        public bool ValidateAddress(string address)
//    {
//        Regex r = new Regex("^(0x){1}[0-9a-fA-F]{40}$");
//        Regex r2 = new Regex("^(0x)?[0-9A-F]{40}$");

//        if (r.IsMatch(address) || r2.IsMatch(address))
//            return true;

//        return false;
//    }

//        / <summary>
//        / Gets data from a URL Endpoint
//        / </summary>
//        / <param name = "url" > URI address end point</param>
//        / <param name = "headers" > Header data</param>
//        / <returns></returns>
//        public string URLGetData(string url, Dictionary<string, string> headers)
//    {
//        UnityWebRequest request = UnityWebRequest.Get(url);

//        foreach (var header in headers)
//            request.SetRequestHeader(header.Key, header.Value);

//        request.downloadHandler = new DownloadHandlerBuffer();
//        request.SendWebRequest();

//        while (!request.isDone) { }

//        if (!request.isNetworkError || !request.isHttpError)
//            return request.downloadHandler.text;
//        else
//            Debug.LogError("<color=red>[ERROR]</red> " + request.error);

//        return string.Empty;
//    }

//    public static void ResetErrorReport() { ErrorReport = new ErrorStatus(); }
//    #endregion
//}
//}
