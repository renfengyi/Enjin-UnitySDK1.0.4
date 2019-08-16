using System.Collections.Generic;
using UnityEngine;
using EnjinSDK;
using EnjinSDK.Helpers;
using EnjinEditorPanel;

public class HomeController
{
    #region Class Definitions
    /// <summary>
    /// Application Data Class
    /// </summary>
    public class AppData
    {
        public string name;
        public string image;
        public string description;

        public AppData()
        {
            name = "";
            image = "";
            description = "";
        }
    }

    /// <summary>
    /// Login Credentials Class
    /// </summary>
    public class LoginCredentials
    {
        public string username;
        public string password;
        public string apiurl;

        public LoginCredentials()
        {
            username = "";
            password = "";
            apiurl = "";
        }
    }
    #endregion

    #region Definitions
    // Public variables & objects
    public int _platformSelection;

    // Private variables & objects
    private readonly string _newsURL = "https://editor.api.enjinx.io/";
    private readonly string[] _platformTypeDev = { "Kovan Testnet", "Public Kovan Testnet", "Updated Contracts Test", "Custom Platform", "EnjinX Platform", "EnjinX Testnet" };
    private readonly string[] _platformType = { "Public Kovan Testnet", "Custom Platform" };
    private LoginState _loginState;


    // Properties
    public int PlatformSelection { get { return _platformSelection; } set { _platformSelection = value; } }
    public string[] PlatformType { get { return _platformType; } }
    public string[] PlatformTypeDev { get { return _platformTypeDev; } }
    public bool IsDevMode { get; set; }
    public PlatformState State { get; private set; }
    public AppData AppInfo { get; set; }
    public News[] NewsData { get; private set; }
    public LoginCredentials LoginInfo { get; set; }
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public HomeController()
    {
        //IsDevMode = true;
        PlatformSelection = 0;
        State = PlatformState.VIEW;
        _loginState = LoginState.NONE;
        AppInfo = new AppData();
        LoginInfo = new LoginCredentials();
        GetLatestNews();
    }

    /// <summary>
    /// Sets the state of the platform
    /// </summary>
    /// <param name="state">State to set platform to</param>
    public void SetPlatformState(PlatformState state)
    {
        State = state;

        if (state == PlatformState.EDIT)
        {
            App newApp = new App();
            newApp = Enjin.GetAppByID(Enjin.AppID);
            AppInfo = new AppData{
                name = newApp.name,
                image = newApp.image,
                description = newApp.description
            };
        }
    }

    /// <summary>
    /// Checks if login status is a valid state
    /// </summary>
    /// <param name="state">status to check state against</param>
    /// <returns>true if state is valid. false otherwise</returns>
    public bool IsLoginStatus(LoginState state)
    {
        return (_loginState == state) ? true : false;
    }

    /// <summary>
    /// Creates a new App
    /// </summary>
    public void CreateApp()
    {
        App newApp = new App
        {
            name = AppInfo.name,
            image = AppInfo.image,
            description = AppInfo.description
        };

        if (newApp.image == "" || newApp.image == null)
            newApp.image = "None";
        else
            newApp.image = AppInfo.image;

        if (newApp.description == "" || newApp.description == null)
            newApp.description = "None";
        else
            newApp.description = AppInfo.description;

        Enjin.CreateApp(newApp);
        //EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.REFRESHAPPLIST);
        EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.LOGOUT);
        IsLoginSuccessful();
        ProcessStartUp();
        //Enjin.StartPlatform(Enjin.APIURL, LoginInfo.username, LoginInfo.password);
        //Enjin.Login(LoginInfo.username, LoginInfo.password);
        AppInfo = new AppData();

        State = PlatformState.VIEW;
    }

    /// <summary>
    /// Updates an exsiting App
    /// </summary>
    public void UpdateAp()
    {
        App newApp = new App
        {
            name = AppInfo.name,
            image = AppInfo.image,
            description = AppInfo.description
        };

        Enjin.UpdateApp(newApp);
        EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.REFRESHAPPLIST);

        State = PlatformState.VIEW;
    }

    /// <summary>
    /// Returns the home panel to view main information
    /// </summary>
    public void ReturnToView()
    {
        State = PlatformState.VIEW;
        AppInfo = new AppData();
    }

    /// <summary>
    /// Resets login fields
    /// </summary>
    private void ResetFields()
    {
        LoginInfo.username = "";
        LoginInfo.password = "";
        LoginInfo.apiurl = "";
    }

    /// <summary>
    /// Handles executing logout process
    /// </summary>
    public void Logout()
    {
        _loginState = LoginState.NONE;
        LoginInfo = new LoginCredentials();
        EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.LOGOUT);
    }

    /// <summary>
    /// Validates that text username & password fields are not empty
    /// </summary>
    /// <returns>true if not empty, false otherwise</returns>
    public bool ValidateTextFields()
    {
        if (LoginInfo.apiurl == string.Empty)
        {
            _loginState = LoginState.INVALIDTPURL;
            return false;
        }
        else if (LoginInfo.username.Trim() == "" || LoginInfo.password.Trim() == "")
        {
            _loginState = LoginState.INVALIDUSERPASS;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if login was successful
    /// </summary>
    /// <returns>true/false based on success</returns>
    public bool IsLoginSuccessful()
    {
        EnjinEditor.CurrentUser = Enjin.StartPlatform(LoginInfo.apiurl, LoginInfo.username, LoginInfo.password);

        if (EnjinEditor.CurrentUser == null)
        {
            _loginState = Enjin.LoginState;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Starts up the platform
    /// </summary>
    public void ProcessStartUp()
    {
        Enjin.IsLoggedIn = true;
        EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.INITILAIZEPLATFORM);
        _loginState = LoginState.VALID;
    }

    /// <summary>
    /// Checks if user has required permission to access feature
    /// </summary>
    /// <param name="perm">permission to check for</param>
    /// <returns>true/false based on results</returns>
    public bool HasPermission(UserPermission perm)
    {
        if (Enjin.AppID != -1)
            return EnjinEditor.HasPermission(perm);

        return false;
    }

    /// <summary>
    /// Updates the news panel information
    /// </summary>
    private void GetLatestNews()
    {
        Dictionary<string, string> headerData = new Dictionary<string, string>
        {
            { "User-Agent", "UnitySDK" },
            { "Content-type", "application/json" }
        };
        string urlData = Enjin.URLGetData(_newsURL, headerData);

        if (urlData != string.Empty)
            NewsData = JsonUtility.FromJson<NewsResult>(EnjinHelpers.GetJSONString(urlData, 1)).news;
        else
            NewsData = null;
    }
}