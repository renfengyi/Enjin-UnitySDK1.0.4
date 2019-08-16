namespace EnjinSDK
{
    using UnityEngine;
    using GraphQLClient;
    using SimpleJSON;
    using PusherClient;
    using System.Collections.Generic;
    using EnjinSDK.Helpers;
    using System;

    public enum ResponseCodes { INITIALIZED = 000, SUCCESS = 200, BADREQUEST = 400, UNAUTHORIZED = 401, NOTFOUND = 404, INVALID = 405, DATACONFLICT = 409, UNKNOWNERROR = 999, INTERNAL = 001 }
    public enum LoginState { NONE, VALID, INVALIDUSERPASS, INVALIDTPURL, AUTO, UNAUTHORIZED }
    public enum SupplyModel { FIXED, SETTABLE, INFINITE, COLLAPSING, ANNUAL_VALUE, ANNUAL_PERCENTAGE }
    public enum SupplyModel2 { FIXED, SETTABLE, INFINITE, COLLAPSING }
    public enum Transferable { PERMANENT, TEMPORARY, BOUND }
    public enum TransferType { NONE, PER_TRANSFER, PER_CRYPTO_ITEM, RATIO_CUT, RATIO_EXTRA }    // TYPE_COUNT removed for V1, will be added back post V1
    public enum CryptoItemFieldType { NAME, TRANSFERABLE, TRANSFERFEE, MELTFEE, MAXMELTFEE, MAXTRANSFERFEE }
    public enum UserPermission
    {
        viewApp, viewUsers, viewIdentities, viewRequests, viewEvents, viewFields, viewTokens,
        viewRoles, viewBalances, manageApp, manageUsers, manageIdentities, manageRequests, manageFields, manageTokens, manageRoles,
        deleteApp, deleteUsers, deleteIdentities, deleteFields, deleteTokens, deleteRoles, transferTokens, meltTokens, viewPlatform
    }
    public enum UserRoles { PLATFORM_OWNER, ADMIN, PLAYER }

    public class EnjinPlatform
    {
        // Private variables & objects
        private int _appID;                 // Application ID
        private Fields[] _defaultFields;    // Array for creating default field values
        private PlatformInfo _platformInfo; // Information about the platform

        // Pusher objects
        private Pusher _client;             // Pucher client connector
        private Channel _channel;           // Pusher channel connection
        private PusherOptions _options;     // Pusher connection options

        // Public URL properties
        public int PlatformID { get { return System.Convert.ToInt32(_platformInfo.id); } }
        public int ApplicationID { get { return _appID; } set { _appID = value; } }
        public PlatformInfo GetPlatform { get { _platformInfo = GetPlatformInfo(); return _platformInfo; } }
        public Fields[] DefaultFields { get { return _defaultFields; } }

        public string TRData;

        /// <summary>
        /// Initializes platform
        /// </summary>
        public void InitializePlatform()
        {
            CreateDeafultFields();
            _platformInfo = GetPlatformInfo();

            PusherSettings.Verbose = false;

            _options = new PusherOptions
            {
                Cluster = _platformInfo.notifications.sdk.options.cluster,
                Encrypted = (_platformInfo.notifications.sdk.options.encrypted == "true") ? true : false
            };

            _client = new Pusher(_platformInfo.notifications.sdk.key, _options);
            _client.Connected += EventConnected;
            _client.ConnectionStateChanged += EventStateChange;
            _client.Connect();
        }

        /// <summary>
        /// Cleans up platform objects
        /// </summary>
        public void CleanUp()
        {
            _client.Disconnect();
        }

        /// <summary>
        /// Reconnects pusher on applicaiton change
        /// </summary>
        public void PusherReconnect()
        {
            _client.Disconnect();
            _client.Connect();
        }

        /// <summary>
        /// Gets the application ID by name
        /// </summary>
        /// <param name="appName">Name of application</param>
        /// <returns>Application ID</returns>
        public int GetAppIDByName(string appName)
        {
            GraphQuery.POST(string.Format(Enjin.PlatformTemplate.GetQuery["GetAppByID"], appName), Enjin.AccessToken);

            var resultGQL = JSON.Parse(GraphQuery.queryReturn);

            return resultGQL["data"]["result"][0]["id"].AsInt;
        }

        /// <summary>
        /// Set the ENJ approval to max
        /// </summary>
        /// <param name="identityID">Identity of user to set max approval on</param>
        public void SetMaxAllowance(int identityID)
        {
            GraphQuery.POST(string.Format(Enjin.PlatformTemplate.GetQuery["SetAllowance"], identityID));
        }

        /// <summary>
        /// Geta an application's information by ID
        /// </summary>
        /// <param name="id">ID of application to get information for</param>
        /// <returns>Application information object</returns>
        public App GetAppByID(int id)
        {
            GraphQuery.POST(string.Format(Enjin.PlatformTemplate.GetQuery["GetAppByID"], id.ToString()));

            var resultGQL = JSON.Parse(GraphQuery.queryReturn);
            // TODO: Convert this to json parsing to datatype (Updates to read back in GraphQuery.cs)
            App appData = new App()
            {
                id = resultGQL["data"]["result"][0]["id"].AsInt,
                name = resultGQL["data"]["result"][0]["name"].Value,
                description = resultGQL["data"]["result"][0]["description"].Value,
                image = resultGQL["data"]["result"][0]["image"].Value
            };

            return appData;
        }

        /// <summary>
        /// Gets all applicaitons user has access to
        /// </summary>
        /// <returns>List of all applications user has access to</returns>
        public List<AppSelectorData> GetAppsByUserID(int userID)
        {
            GraphQuery.POST(string.Format(Enjin.PlatformTemplate.GetQuery["GetAppsByUserID"], userID), Enjin.AccessToken);

            var resultGQL = JSON.Parse(GraphQuery.queryReturn);
            int count = resultGQL["data"]["result"][0]["identities"].Count;
            List<AppSelectorData> appList = new List<AppSelectorData>();

            for (int i = 0; i < count; i++)
            {
                try
                {
                    appList.Add(new AppSelectorData
                    {
                        appID = resultGQL["data"]["result"][0]["identities"][i]["app_id"].AsInt,
                        identityID = resultGQL["data"]["result"][0]["identities"][i]["id"].AsInt,
                        appName = resultGQL["data"]["result"][0]["identities"][i]["app"]["name"].Value
                    });
                }
                catch (NullReferenceException)
                {
                    Enjin.AppID = -1;
                }
            }

            return appList;
        }

        /// <summary>
        /// Invite a new User to this app on this platform
        /// </summary>
        /// <param name="email">Email address to send invitation to</param>
        public void InviteUserToApp(string email)
        {
            GraphQuery.POST(string.Format(Enjin.PlatformTemplate.GetQuery["InviteUser"], email));
        }

        /// <summary>
        /// Invite a new User to this app on this platform with the given name
        /// </summary>
        /// <param name="email">Email address to send invitation to</param>
        /// <param name="username">The invited user's username</param>
        public void InviteUserToApp(string email, string username)
        {
            GraphQuery.POST(string.Format(Enjin.PlatformTemplate.GetQuery["InviteUserWithName"], email, username));
        }


        /// <summary>
        /// Creates a new application
        /// </summary>
        /// <param name="app">Application to create</param>
        /// <returns>New applicaiton</returns>
        public App CreateApp(App app)
        {
            GraphQuery.POST(string.Format(Enjin.PlatformTemplate.GetQuery["CreateApp"], app.name, app.description, app.image), Enjin.AccessToken);

            return JsonUtility.FromJson<App>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));
        }

        /// <summary>
        /// Updates App information
        /// </summary>
        /// <param name="app">App to update information for</param>
        /// <returns>Updated App</returns>
        public App UpdateApp(App app)
        {
            string query;
            query = @"mutation updateApp{App:UpdateEnjinApp(name:""$appName^"",description:""$appDescription^"",image:""$appImageURL^""){id,name,description,image}}";
            GraphQuery.variable["appName"] = app.name;
            GraphQuery.variable["appDescription"] = app.description;
            GraphQuery.variable["appImageURL"] = app.image;
            GraphQuery.POST(query);

            return JsonUtility.FromJson<App>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));
        }

        /// <summary>
        /// Updates a roles permissions
        /// </summary>
        /// <param name="name">Name of role to update</param>
        /// <param name="permissions">Array of permissions to update</param>
        /// <returns>(True / False) if update was successful</returns>
        public bool UpdateRole(string name, string[] permissions) { return UpdateRole(name, "", permissions); }

        /// <summary>
        /// Updates a roles name & permissions
        /// </summary>
        /// <param name="name">Name of role to update</param>
        /// <param name="newName">New name to update role to</param>
        /// <param name="permissions">Array of permissions to update</param>
        /// <returns>(True / False) if update was successful</returns>
        public bool UpdateRole(string name, string newName, string[] permissions)
        {
            string query;

            if (newName == "")
                query = @"mutation updateRole{result:UpdateEnjinRole(name:""$roleName^"",permissions:$permissions[]^){id,name,permissions{name}}}";
            else
            {
                query = @"mutation updateRole{result:UpdateEnjinRole(name:""$roleName^"",new_name:""$roleNewName^"",permissions:$permissions[]^){id,name,permissions{name}}}";
                GraphQuery.variable["roleNewName"] = newName;
            }

            GraphQuery.variable["roleName"] = name;
            GraphQuery.array["permissions[]"] = permissions;
            GraphQuery.POST(query);

            if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
                return true;

            return false;
        }

        /// <summary>
        /// Removes a role from the currenct application
        /// </summary>
        /// <param name="role">Name of role to remove</param>
        /// <returns>(True / False) if deleting a role was successful</returns>
        public Request DeleteRole(Roles role)
        {
            GraphQuery.POST(string.Format(Enjin.PlatformTemplate.GetQuery["DeleteRole"], role.name));

            if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
                return JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));

            return null;
        }

        /// <summary>
        /// Creates a new role for the current application
        /// </summary>
        /// <param name="name">Role name</param>
        /// <param name="permissions">Array of permissions role has</param>
        /// <returns>(True / False) if creating role was successful</returns>
        public bool CreateRole(string name, string[] permissions)
        {
            string query;
            query = @"mutation createRole{result:CreateEnjinRole(name:""$role^"",permissions:$permissions[]^){id,name,permissions{name}}}";
            GraphQuery.variable["role"] = name;
            GraphQuery.array["permissions[]"] = permissions;
            GraphQuery.POST(query);

            if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
                return true;

            return false;
        }

        /// <summary>
        /// Creates the default fields sets if one hasn't been set in settings
        /// </summary>
        private void CreateDeafultFields()
        {
            // Check if defualt fields has been set
            _defaultFields = new Fields[2];

            _defaultFields[0] = new Fields("player_data", string.Empty, 1, 1, 1);
            _defaultFields[1] = new Fields("custom_date_1", string.Empty, 1, 1, 1);
        }

        /// <summary>
        /// Gets the platform information for intiializing platform
        /// </summary>
        /// <returns>PlatformInfo object containing platform info</returns>
        private PlatformInfo GetPlatformInfo()
        {
            GraphQuery.POST(Enjin.PlatformTemplate.GetQuery["GetPlatformInfo"], Enjin.AccessToken);

            Debug.Log(">>> fetched string " + EnjinHelpers.GetJSONString(GraphQuery.queryReturn));

            return JsonUtility.FromJson<PlatformInfo>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn));
        }

        /// <summary>
        /// Gets the Enjin Coin balance from specified Identity
        /// </summary>
        /// <param name="identityID">Identity ID to get balance from</param>
        /// <returns>Balance as a long value</returns>
        public long GetEnjBalance(int identityID)
        {
            GraphQuery.POST(string.Format(Enjin.PlatformTemplate.GetQuery["GetEnjBalances"], identityID), Enjin.AccessToken);

            return 0;
        }

        /// <summary>
        /// Gets the total of active tokens for a given identity
        /// </summary>
        /// <param name="identityID">Identity to get active tokens for</param>
        /// <returns>Total number of active tokens</returns>
        public int GetActiveTokens(int identityID)
        {
            GraphQuery.POST(string.Format(Enjin.PlatformTemplate.GetQuery["GetActiveTokens"], identityID), Enjin.AccessToken);

            var resultGQL = JSON.Parse(GraphQuery.queryReturn);
            int activeTokens = 0;

            for (int i = 0; i < resultGQL["data"]["activeTokens"][0]["tokens"].Count; i++)
                activeTokens += System.Convert.ToInt32(resultGQL["data"]["activeTokens"][0]["tokens"][i]["balance"].Value);

            return activeTokens;
        }

        #region Pusher Methods
        /// <summary>
        /// Pusher connected event
        /// </summary>
        /// <param name="sender">Object connector for pusher</param>
        private void EventConnected(object sender)
        {
            if (Enjin.IsDebugLogActive)
                Debug.Log("<color=aqua>[PUSHER]</color> Client connected");

            _channel = _client.Subscribe("enjin.server." + _platformInfo.network + "." + _platformInfo.id.ToString() + "." + Enjin.AppID.ToString());
            _channel.BindAll(ChannelEvent);
        }

        /// <summary>
        /// Pusher event channel. Can be subscribed to for handling pusher events
        /// </summary>
        /// <param name="eventName">Event type</param>
        /// <param name="eventData">Data associated to event</param>
        private void ChannelEvent(string eventName, object eventData)
        {
            TRData = JsonHelper.Serialize(eventData);
            RequestEvent trackData = JsonUtility.FromJson<RequestEvent>(TRData);
            if (Enjin.IsDebugLogActive)
            {
                Debug.Log("<color=aqua>[PUSHER]</color> Event: " + trackData.event_type);
            }

            if (Enjin.IsDebugLogActive)
            {
                Debug.Log("<color=aqua>[PUSHER]</color> Event " + eventName + " recieved. Data: " + TRData);
            }

            // Temp fix for action duplication issue. Will replace with event manager integration
            // Execute any event handlers which are listening to this specific event.
            if (Enjin.EventListeners.ContainsKey(eventName))
            {
                for (int i = 0; i < Enjin.EventListeners[eventName].Count; i++)
                {
                    Enjin.EventListeners[eventName][i](trackData);
                }
            }

            // Notify any callback functions listening for this request that we've broadcasted.
            /*
            if (trackData.event_type.Equals("tx_broadcast"))
            {
                int requestId = trackData.data.id;
                if (Enjin.RequestCallbacks.ContainsKey(requestId))
                {
                    System.Action<RequestEvent> callback = Enjin.RequestCallbacks[requestId];
                    callback(trackData);
                }
            }
            */

            // Execute any callback function which is listening for this request.
            if (trackData.event_type.Equals("tx_executed"))
            {
                int requestId = trackData.data.transaction_id;
                if (Enjin.RequestCallbacks.ContainsKey(requestId))
                {
                    System.Action<RequestEvent> callback = Enjin.RequestCallbacks[requestId];
                    callback(trackData);
                    Enjin.RequestCallbacks.Remove(requestId);
                }
            }
        }

        /// <summary>
        /// Pusher state change. Reports any state changes from pusher
        /// </summary>
        /// <param name="sender">Object connector to track</param>
        /// <param name="state">State change of pusher connector</param>
        private void EventStateChange(object sender, ConnectionState state)
        {
            if (Enjin.IsDebugLogActive)
                Debug.Log("<color=aqua>[PUSHER]</color> Connection state changed to: " + state);
        }

        /// <summary>
        /// Bind a listener to fire each time some named event is received from pusher
        /// </summary>
        /// <param name="eventName">The string name of the event to track</param>
        /// <param name="listener">The listening action to fire with the responding event data</param>
        public void BindEvent(string eventName, System.Action<RequestEvent> listener)
        {
            bool hasListeners = Enjin.EventListeners.ContainsKey(eventName);
            if (hasListeners)
            {
                List<System.Action<RequestEvent>> listenerList = Enjin.EventListeners[eventName];
                listenerList.Add(listener);
                Enjin.EventListeners[eventName] = listenerList;
            }
            else
            {
                List<System.Action<RequestEvent>> listenerList = new List<System.Action<RequestEvent>>
                {
                    listener
                };
                Enjin.EventListeners[eventName] = listenerList;
            }
        }

        /// <summary>
        /// Bind a listener to fire when an event indicating that the given Identity ID has linked a wallet is received from pusher
        /// </summary>
        /// <param name="identityID">The integer ID of the Identity to listen for a linked wallet on</param>
        /// <param name="listener">The listening action to fire with the responding event data</param>
        internal void ListenForLink(int identityID, System.Action<RequestEvent> listener)
        {
            Channel channel = _client.Subscribe("enjin.server." + _platformInfo.network + "." + _platformInfo.id.ToString() + "." + Enjin.AppID.ToString() + "." + identityID);
            channel.BindAll((eventName, eventData) =>
            {
                string dataString = JsonHelper.Serialize(eventData);
                RequestEvent transactionData = JsonUtility.FromJson<RequestEvent>(dataString);
                if (Enjin.IsDebugLogActive)
                {
                    Debug.Log("<color=aqua>[PUSHER]</color> Event: " + transactionData.event_type);
                }

                if (Enjin.IsDebugLogActive)
                {
                    Debug.Log("<color=aqua>[PUSHER]</color> Event " + eventName + " recieved. Data: " + dataString);
                }

                // If we see that this client has updated their event, fire our awaiting callback.
                if (transactionData.event_type.Equals("identity_updated"))
                {
                    listener(transactionData);
                    channel.Unsubscribe();
                }
            });
        }
        #endregion
    }
}