namespace EnjinSDK
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using GraphQLClient;
    using EnjinSDK.Helpers;

    public class EnjinUsers
    {
        #region Declarations & Properties
        // Private variables & objects
        readonly private string[] _permissionList = new string[] { "viewApp", "viewUsers", "viewIdentities", "viewRequests",
            "viewEvents", "viewFields", "viewTokens", "viewRoles", "viewBalances", "manageApp", "manageUsers", "manageIdentities",
            "manageRequests", "manageFields", "manageTokens", "manageRoles", "deleteApp", "deleteUsers", "deleteIdentities",
            "deleteFields", "deleteTokens", "deleteRoles", "transferTokens", "meltTokens", "viewPlatform"};

        // Properties
        public string[] PermissionList { get { return _permissionList; } }
        #endregion

        /// <summary>
        /// Creates a new User
        /// </summary>
        /// <param name="username">User's username</param>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's password</param>
        /// <param name="role">User's role</param>
        /// <returns>Created user object</returns>
        public User Create(string username, string email, string password, string role)
        {
            role = "\"" + role + "\"";

            GraphQuery.POST(string.Format(Enjin.UserTemplate.GetQuery["CreateUser"], username, password, email, role));

            if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
                return JsonUtility.FromJson<User>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));

            return null;
        }

        /// <summary>
        /// Soft deletes a user from the team
        /// </summary>
        /// <param name="userID">User id</param>
        /// <returns>(true / false) if action was successful</returns>
        public bool Delete(int userID)
        {
            GraphQuery.POST(string.Format(Enjin.UserTemplate.GetQuery["DeleteUser"], userID.ToString()));

            if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
                return true;

            return false;
        }

        /// <summary>
        /// Updates user data
        /// </summary>
        /// <param name="id">ID of user to update</param>
        /// <param name="username">Updated username</param>
        /// <param name="email">Updated email</param>
        /// <returns>Updated user</returns>
        public User Update(int id, string username, string email, string roles)
        {
            string updRoles = string.Empty;

            if (roles != "[]")
                updRoles = ",roles:" + roles;

            GraphQuery.POST(string.Format(Enjin.UserTemplate.GetQuery["UpdateUser"], id.ToString(), username, email, updRoles));

            return JsonUtility.FromJson<User>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
        }

        /// <summary>
        /// Gets a specified user from the current application
        /// </summary>
        /// <param name="userID">ID of user to get</param>
        /// <returns>Specified User</returns>
        public User Get(int userID) { return GetRaw(userID, false); }

        /// <summary>
        /// Gets a specified user no matter what applicaiton it's associated to
        /// </summary>
        /// <param name="userID">ID of user to get</param>
        /// <param name="raw">Flag to turn on/off getting user from application only</param>
        /// <returns>Specified user</returns>
        public User GetRaw(int userID, bool raw = true)
        {
            if (!raw)
                GraphQuery.POST(string.Format(Enjin.UserTemplate.GetQuery["GetUser"], userID.ToString()));
            else
                GraphQuery.POST(string.Format(Enjin.UserTemplate.GetQuery["GetUser"], userID.ToString()), Enjin.AccessToken);

            var tData = JsonUtility.FromJson<JSONArrayHelper<User>>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));

            return tData.result[0];
        }

        /// <summary>
        /// Gets list of all users for current application
        /// </summary>
        /// <returns>Array of all users for application</returns>
        public User[] GetAll()
        {
            GraphQuery.POST(Enjin.UserTemplate.GetQuery["GetAllUsers"], Enjin.AccessToken);

            return JsonUtility.FromJson<JSONArrayHelper<User>>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1)).result;
        }

        /// <summary>
        /// Returns specified number of users per page
        /// </summary>
        /// <param name="page">Page number of user list to retrieve</param>
        /// <param name="limit">Number of users per page</param>
        /// <param name="appID">Application ID to get users list from</param>
        /// <returns>Sepcified page of users</returns>
        public PaginationHelper<User> GetAllUsersByAppID(int page, int limit)
        {
            GraphQuery.POST(string.Format(Enjin.UserTemplate.GetQuery["GetUsersPagination"], page.ToString(), limit.ToString()));

            return JsonUtility.FromJson<PaginationHelper<User>>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
        }

        /// <summary>
        /// Search function for users
        /// </summary>
        /// <param name="term">Term to search user list for</param>
        /// <returns>Users that match search term</returns>
        public User[] SearchUsers(string term)
        {
            GraphQuery.POST(string.Format(Enjin.UserTemplate.GetQuery["SearchUsers"], term));

            List<User> searchResults = new List<User>();
            var results = JsonUtility.FromJson<JSONArrayHelper<User>>(EnjinHelpers.GetJSONString(Regex.Replace(GraphQuery.queryReturn, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1"), 1)).result;

            foreach (User user in results)
            {
                if (user.id != -1)
                    searchResults.Add(user);
            }

            return searchResults.ToArray();
        }
    }
}