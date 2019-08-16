using System.Collections.Generic;
using System;

namespace EnjinSDK
{
    /// <summary>
    /// Roles object that holds the name of role and permissions of role
    /// </summary>
    [Serializable]
    public class Roles
    {
        /* NOTE:
         *      - list version of permissions is to replace array version for better management
         */
        public int id;                          // Role ID
        public string name;                     // Role name
        //public List<Permissions> permissions;   // List of permissions role has
        public Permissions[] permissions;       // Collection of Roles permissions

        /// <summary>
        /// Constructor
        /// </summary>
        public Roles()
        {
            id = -1;
            name = string.Empty;
            //permissions = new List<Permissions>();
        }
    }

    /// <summary>
    /// Permissions object contains the permission id and name.
    /// </summary>
    [Serializable]
    public class Permissions
    {
        public int id;                  // Permission ID
        public string name;             // Permission name
        public string display_name;     // Permission display name

        /// <summary>
        /// Constructor
        /// </summary>
        public Permissions()
        {
            id = -1;
            name = string.Empty;
            display_name = string.Empty;
        }
    }

    /// <summary>
    /// User data structure that contains account info and links to user identity
    /// </summary>
    [Serializable]
    public class User
    {
        /* NOTE: 
         *      - activeIdentity is to replace activeIdentityID
         *      - list verion of roles is to replace array version for better manamgement
         *      - list verion of identities is to replace array version for better manamgement
         */
        public int id;                      // Account ID
        public int activeIdentityID;        // Users current active IdentityID
        public Identity activeIdentity;     // Users current active Identity
        public string name;                 // Username of account
        public string email;                // Email address linked to username
        public string password;             // Account password
        public string access_token;         // oAuth token
        public DateData updated_at;         // Last update to account
        public DateData created_at;         // Date account created
        //public List<Roles> roles;           // List of all roles a user has
        //public List<Identity> identities;   // List of all identities a user has
        public Roles[] roles;               // Users role / permission level
        public Identity[] identities;       // Identity associated to user
        public IErrorHandle ErrorStatus;    // Error handling object

        /// <summary>
        /// Constructor
        /// </summary>
        public User()
        {
            id = -1;
            activeIdentityID = -1;
            name = string.Empty;
            email = string.Empty;
            password = string.Empty;
            access_token = string.Empty;
            updated_at = new DateData();
            created_at = new DateData();
            ErrorStatus = new ErrorHandler();
            //roles = new List<Roles>();
            //identities = new List<Identity>();
        }
    }

    /// <summary>
    /// User credential data structure
    /// </summary>
    [Serializable]
    public class UserCredentials
    {
        public string key;
        public int appID;
        public int userID;
        public string accessToken;
        public string email;
    }

    /// <summary>
    /// Parsing-only data structure for holding a single ID field
    /// </summary>
    [Serializable]
    public class IdFieldObject
    {
        public int id;
    }

    /// <summary>
    /// User existence-checking data structure
    /// </summary>
    [Serializable]
    public class UserExists
    {
        public IdFieldObject[] nameSearch;
        public IdFieldObject[] emailSearch;
    }
}