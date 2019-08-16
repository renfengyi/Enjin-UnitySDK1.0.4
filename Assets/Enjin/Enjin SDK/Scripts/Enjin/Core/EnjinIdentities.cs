namespace EnjinSDK
{
    using System.Collections.Generic;
    using UnityEngine;
    using GraphQLClient;
    using SimpleJSON;
    using EnjinSDK.Helpers;
    using System.Text.RegularExpressions;

    public class EnjinIdentities
    {
        #region Variables, Objects, & Properties
        // Private veriables & objects
        private double _ethBalance;     // Eth balance
        private double _enjBalance;     // Enjin Coin balance

        // Properties
        public double EthBalance { get { return _ethBalance; } }
        public double EnjBalance { get { return _enjBalance; } }
        #endregion

        /// <summary>
        /// Searches identities for a key term
        /// </summary>
        /// <param name="term">Term to search identities by</param>
        /// <returns>Array of identities based on search results</returns>
        public Identity[] SearchIdentities(string term)
        {
            Enjin.Post(string.Format(Enjin.IdentityTemplate.GetQuery["SearchIdentities"], term));

            List<Identity> searchResults = new List<Identity>();
            var results = JsonUtility.FromJson<JSONArrayHelper<Identity>>(EnjinHelpers.GetJSONString(Regex.Replace(GraphQuery.queryReturn, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1"), 1)).result;

            foreach (Identity result in results)
            {
                if (result.id != 0)
                    searchResults.Add(result);
            }

            return searchResults.ToArray();
        }

        /// <summary>
        /// Gets all identities
        /// </summary>
        /// <param name="byApp">Get only identities for current app or all per platform (true/false)</param>
        /// <returns>Array of identites</returns>
        public Identity[] GetAll(bool byApp = true)
        {
            if (byApp)
                GraphQuery.POST(Enjin.IdentityTemplate.GetQuery["GetAllIdentities"]);
            else
                GraphQuery.POST(Enjin.IdentityTemplate.GetQuery["GetAllIdentities"], Enjin.AccessToken);

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return null;

            var tData = JsonUtility.FromJson<JSONArrayHelper<Identity>>(EnjinHelpers.GetJSONString(Regex.Replace(GraphQuery.queryReturn, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1"), 1)).result;

            return tData;
        }

        /// <summary>
        /// Gets all roles and permission for current application
        /// </summary>
        /// <returns>Array of Roles</returns>
        public Roles[] GetRoles()
        {
            GraphQuery.POST(Enjin.IdentityTemplate.GetQuery["GetRoles"]);

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return null;

            return JsonUtility.FromJson<JSONArrayHelper<Roles>>(EnjinHelpers.GetJSONString(Regex.Replace(GraphQuery.queryReturn, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1"), 1)).result;
        }

        /// <summary>
        /// Gets all idenities in pagination format
        /// </summary>
        /// <param name="page">Page to get identity list from</param>
        /// <param name="limit">Number of identities to get per page</param>
        /// <param name="byApp">Get only identities for current app or all per platform (true/false)</param>
        /// <returns>Array of identities for requested page</returns>
        public PaginationHelper<Identity> GetAllByApp(int page, int limit, bool byApp = true)
        {
            if (byApp)
                GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["GetIdentitiesPagination"], page.ToString(), limit.ToString()));
            else
                GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["GetIdentitiesPagination"], page.ToString(), limit.ToString()), Enjin.AccessToken);

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return null;

            return JsonUtility.FromJson<PaginationHelper<Identity>>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
        }

        /// <summary>
        /// Gets a specific identity
        /// </summary>
        /// <param name="id">ID of identity to get</param>
        /// <returns>Identity associated with passed id</returns>
        public Identity Get(int id)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["GetIdentity"], id.ToString()));

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return null;

            return JsonUtility.FromJson<JSONArrayHelper<Identity>>(EnjinHelpers.GetJSONString(Regex.Replace(GraphQuery.queryReturn, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1"), 1)).result[0];
        }

        /// <summary>
        /// Updates an identities fields property
        /// </summary>
        /// <param name="id">ID of idenitiy to update</param>
        /// <param name="fields">Updated fields object</param>
        /// <returns>Updated Identity</returns>
        public Identity UpdateFields(int id, Fields[] fields)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["UpdateFields"], id.ToString(), EnjinHelpers.GetFieldsString(fields)), Enjin.AccessToken);

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return null;

            return JsonUtility.FromJson<Identity>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));
        }

        /// <summary>
        /// Creates a new identity
        /// </summary>
        /// <param name="newIdentity">New Identity to create</param>
        /// <returns>Created Identity</returns>
        public Identity Create(Identity newIdentity)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["CreateIdentity"], newIdentity.user.id.ToString(), newIdentity.ethereum_address, EnjinHelpers.GetFieldsString(newIdentity.fields)));

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return null;

            return JsonUtility.FromJson<Identity>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));
        }

        /// <summary>
        /// Creates a new identity for the requester's app under a given email address
        /// </summary>
        /// <param name="email">The email address to attempt creation with</param>
        /// <returns>Created Identity</returns>
        public Identity CreateByEmail(string email)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["CreateIdentityWithEmail"], email));

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return null;

            return JsonUtility.FromJson<Identity>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));
        }

        /// <summary>
        /// Updates an Identty
        /// </summary>
        /// <param name="identity">Identity to update</param>
        /// <returns>Updated Identity</returns>
        public Identity Update(Identity identity)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["UpdateIdentity"], identity.id.ToString(), identity.user.id.ToString(), identity.ethereum_address, EnjinHelpers.GetFieldsString(identity.fields)));

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return null;

            return JsonUtility.FromJson<Identity>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));
        }

        /// <summary>
        /// Deletes an identity. if user attached to it, it's deleted as well
        /// </summary>
        /// <param name="id">Identitiy ID to delete</param>
        /// <returns>true/false on success</returns>
        public bool Delete(string id)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["DeleteIdentity"], id.ToString()));

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return false;

            return true;
        }

        /// <summary>
        /// Gets an updated balance for eth and enj
        /// </summary>
        /// <param name="identity">Identity to get new balances for</param>
        public void UpdateBalances(Identity identity)
        {
            if (identity.ethereum_address != "")
            {
                GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["UpdateBalances"], identity.id.ToString(), identity.ethereum_address));

                if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                    return;

                var resultGQL = JSON.Parse(GraphQuery.queryReturn);

                _ethBalance = System.Convert.ToDouble(resultGQL["data"]["balances"][0]["eth_balance"].Value);

                if (resultGQL["data"]["balances"][0]["enj_balance"].Value == "null")
                    _enjBalance = 0;
                else
                    _enjBalance = System.Convert.ToDouble(resultGQL["data"]["balances"][0]["enj_balance"].Value);
            }
            else
            {
                _ethBalance = 0;
                _enjBalance = 0;
            }
        }

        /// <summary>
        /// Links an identity to a wallet or eth address
        /// </summary>
        /// <param name="identity">Identity to link</param>
        /// <returns>Updated identity </returns>
        public Identity Link(Identity identity)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["LinkIdentity"], identity.id.ToString(), identity.ethereum_address));

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return null;

            return JsonUtility.FromJson<Identity>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));
        }

        /// <summary>
        /// Unlinks identity from wallet
        /// </summary>
        /// <param name="id">ID of identity to unlink</param>
        /// <returns>Updated identity</returns>
        public bool UnLink(int id)
        {
            GraphQuery.POST(string.Format(Enjin.IdentityTemplate.GetQuery["UnlinkIdentity"], id.ToString()));

            if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
                return false;

            return true;
        }
    }
}