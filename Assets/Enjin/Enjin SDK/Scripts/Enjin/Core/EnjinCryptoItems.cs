namespace EnjinSDK
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using GraphQLClient;
    using EnjinSDK.Helpers;
    using SimpleJSON;
    using System;

    public class EnjinCryptoItems
    {
        /// <summary>
        /// Gets all items in a pagination format
        /// </summary>
        /// <param name="page">Page to get</param>
        /// <param name="limit">Total items per page</param>
        /// <param name="identityID">Identity ID of user</param>
        /// <returns></returns>
        public PaginationHelper<CryptoItem> GetItems(int page, int limit, int identityID)
        {
            string query = string.Empty;

            if (limit == 0)
                query = @"query getAllItems{result:EnjinIdentities(id:$id^){tokens(pagination:{page:$page^},include_creator_tokens:true){items{index,token_id,name,creator,totalSupply,reserve,circulatingSupply,supplyModel,meltValue,meltFeeRatio,meltFeeMaxRatio,transferable,transferFeeSettings{type,token_id,value},nonFungible,icon,balance,isCreator,markedForDelete}cursor{total,hasPages,perPage,currentPage}}}}";
            else
            {
                query = @"query getAllItems{result:EnjinIdentities(id:$id^){tokens(pagination:{limit:$limit^,page:$page^},include_creator_tokens:true){items{index,token_id,name,creator,totalSupply,reserve,circulatingSupply,supplyModel,meltValue,meltFeeRatio,meltFeeMaxRatio,transferable,transferFeeSettings{type,token_id,value},nonFungible,icon,balance,isCreator,markedForDelete}cursor{total,hasPages,perPage,currentPage}}}}";
                GraphQuery.variable["limit"] = limit.ToString();
            }

            GraphQuery.variable["id"] = identityID.ToString();
            GraphQuery.variable["page"] = page.ToString();
            GraphQuery.POST(query);

            var results = JSON.Parse(GraphQuery.queryReturn);
            string temp = EnjinHelpers.GetJSONString(Regex.Replace(GraphQuery.queryReturn, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1"), 3);
            var temp2 = JsonUtility.FromJson<PaginationHelper<CryptoItem>>(temp);

            for (int i = 0; i < temp2.items.Length; i++)
            {
                temp2.items[i].supplyModel = (SupplyModel)Enum.Parse(typeof(SupplyModel), results["data"]["result"][0]["tokens"]["items"][i]["supplyModel"].Value);
                temp2.items[i].transferable = (Transferable)Enum.Parse(typeof(Transferable), results["data"]["result"][0]["tokens"]["items"][i]["transferable"].Value);
                temp2.items[i].transferFeeSettings.type = (TransferType)Enum.Parse(typeof(TransferType), results["data"]["result"][0]["tokens"]["items"][i]["transferFeeSettings"]["type"].Value);
            }

            return temp2;
        }

        /// <summary>
        /// Gets all CryptoItems on the platform
        /// </summary>
        /// <returns>All CryptoItems on the platform</returns>
        public CryptoItem[] GetAll()
        {
            string query = string.Empty;

            try
            {
                query = "query getCryptoItems{CryptoItems:EnjinTokens{token_id,name,totalSupply,reserve,supplyModel,meltValue,meltFeeRatio,transferable,transferFeeSettings{type,token_id,value},nonFungible,markedForDelete}}";
                GraphQuery.POST(query, Enjin.AccessToken);

                var tData = JsonUtility.FromJson<JSONArrayHelper<CryptoItem>>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));

                return tData.result;
            }
            catch (Exception err)
            {
                Debug.LogWarning(err);
            }

            return null;
        }

        /// <summary>
        /// Searches the CryptoItems globally for matching term
        /// </summary>
        /// <param name="term">term is what user is searching for</param>
        /// <returns>Array of CryptoItems found in search</returns>
        public CryptoItem[] SearchCryptoItems(string term)
        {
            string query = string.Empty;
            query = @"query searchCryptoItem{result:EnjinSearch(term:""$term^""){__typename... on EnjinToken {index,token_id,name,creator,totalSupply,reserve,circulatingSupply,supplyModel,meltValue,meltFeeRatio,meltFeeMaxRatio,transferable,transferFeeSettings{type,token_id,value},nonFungible,icon,balance,isCreator}}}";
            GraphQuery.variable["term"] = term;
            GraphQuery.POST(query);

            List<CryptoItem> searchResults = new List<CryptoItem>();
            //var results = JsonUtility.FromJson<JSONArrayHelper<CryptoItem>>(EnjinHelpers.GetJSONString(Regex.Replace(GraphQuery.queryReturn, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1"), 1)).result;

            var results = JSON.Parse(GraphQuery.queryReturn);
            string temp = EnjinHelpers.GetJSONString(Regex.Replace(GraphQuery.queryReturn, @"(""[^""\\]*(?:\\.[^""\\]*)*"")|\s+", "$1"), 1);
            var temp2 = JsonUtility.FromJson<JSONArrayHelper<CryptoItem>>(temp);

            for (int i = 0; i < temp2.result.Length; i++)
            {
                if (temp2.result[i].token_id != null)
                {
                    temp2.result[i].supplyModel = (SupplyModel)Enum.Parse(typeof(SupplyModel), results["data"]["result"][i]["supplyModel"].Value);
                    temp2.result[i].transferable = (Transferable)Enum.Parse(typeof(Transferable), results["data"]["result"][i]["transferable"].Value);
                    temp2.result[i].transferFeeSettings.type = (TransferType)Enum.Parse(typeof(TransferType), results["data"]["result"][i]["transferFeeSettings"]["type"].Value);
                    searchResults.Add(temp2.result[i]);
                }
            }

            //foreach (CryptoItem item in results)
            //{
            //    if (item.token_id != null)
            //        searchResults.Add(item);
            //}

            return searchResults.ToArray();
        }

        /// <summary>
        /// Gets a CryptoItem by it's ID
        /// </summary>
        /// <param name="id">ID of CryptoItem</param>
        /// <returns>CryptoItem of ID requested</returns>
        public CryptoItem Get(string id)
        {
            string query = string.Empty;

            try
            {
                query = "query getCryptoItem{result:EnjinTokens(token_id:\"$id^\"){token_id,name,totalSupply,reserve,circulatingSupply,supplyModel,meltValue,meltFeeRatio,transferable,transferFeeSettings{type,token_id,value},nonFungible,markedForDelete,itemURI}}";
                GraphQuery.variable["id"] = id.ToString();
                GraphQuery.POST(query);

                var tData = JsonUtility.FromJson<JSONArrayHelper<CryptoItem>>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));

                return tData.result[0];
            }
            catch (Exception err)
            {
                Debug.LogWarning(err);
            }

            return null;
        }

        // Retrieve all CryptoItems belonging to a given wallet address, organized by app ID.
        internal Dictionary<int, List<CryptoItem>> GetCryptoItemsByAddress(string ethereumAddress)
        {
            Dictionary<int, List<CryptoItem>> output = new Dictionary<int, List<CryptoItem>>();
            string _query = string.Empty;
            try
            {
                _query = @"query getAllItems {result:EnjinIdentities(ethereum_address:""$address^""){tokens(include_creator_tokens:false){app_id,token_id,name,creator,balance}}}";
                GraphQuery.variable["address"] = ethereumAddress;
                GraphQuery.POST(_query, Enjin.AccessToken);

                // Parse a set of items and return them for each app ID.
                if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
                {
                    var results = JSON.Parse(GraphQuery.queryReturn);
                    var result = results["data"]["result"];
                    for (int i = 0; i < result.Count; i++)
                    {
                        List<CryptoItem> items = new List<CryptoItem>();
                        int currentAppID = -1;
                        var tokens = result[i]["tokens"]; // TODO: fix this sorting! It's bad! get app ID at each item!
                        for (int j = 0; j < tokens.Count; j++)
                        {
                            var token = tokens[j];
                            currentAppID = token["app_id"].AsInt;
                            items.Add(new CryptoItem
                            {
                                token_id = token["token_id"].Value,
                                name = token["name"].Value,
                                creator = token["creator"].Value,
                                balance = token["balance"].AsInt
                            });
                        }
                        output.Add(currentAppID, items);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.LogWarning(err);
            }
            return output;
        }

        /// <summary>
        /// Gets a cryptoItem ID by name
        /// </summary>
        /// <param name="itemName">Name of cryptoItem</param>
        /// <returns>ID of cryptoItem</returns>
        public string GetItemIDByName(string itemName)
        {
            string query = string.Empty;

            try
            {
                query = @"query getTokenByName{EnjinTokens(name:""$name^""){token_id}}";
                GraphQuery.variable["name"] = itemName;
                GraphQuery.POST(query);

                var resultGQL = JSON.Parse(GraphQuery.queryReturn);

                return resultGQL["data"]["EnjinTokens"][0]["token_id"].Value;
            }
            catch (Exception err)
            {
                Debug.LogWarning(err);
            }

            return null;
        }

        /// <summary>
        /// Gets the balances of all Fungible and NonFungible base CryptoItems a player owns
        /// </summary>
        /// <param name="identityID">Identity ID of applicaiton to get balances from</param>
        /// <returns>Balances of all tokens pertaining to a speicific application</returns>
        public Dictionary<string, int> GetAllBalancesForID(int identityID)
        {
            string query = string.Empty;

            try
            {
                query = "query getCryptoItemBalance{result:EnjinIdentities(id:$id^){tokens{token_id,balance}}}";
                GraphQuery.variable["id"] = identityID.ToString();
                GraphQuery.POST(query);
                var resultsGQL = JSON.Parse(GraphQuery.queryReturn);

                Dictionary<string, int> balanceMap = new Dictionary<string, int>();

                // TODO: Add check for NFI, Get indexes, Add only the index of the item to balances
                for (int i = 0; i < resultsGQL["data"]["result"][0]["tokens"].Count; i++)
                {
                    if (balanceMap.ContainsKey(resultsGQL["data"]["result"][0]["tokens"][i]["token_id"].Value))
                    {
                        int value;
                        balanceMap.TryGetValue(resultsGQL["data"]["result"][0]["tokens"][i]["token_id"].Value, out value);
                        balanceMap[resultsGQL["data"]["result"][0]["tokens"][i]["token_id"].Value] = value + 1;
                    }
                    else
                        balanceMap.Add(resultsGQL["data"]["result"][0]["tokens"][i]["token_id"].Value, resultsGQL["data"]["result"][0]["tokens"][i]["balance"].AsInt);
                }

                return balanceMap;
            }
            catch (Exception err)
            {
                Debug.Log("<color=magenta>[QUERY FAILED]</color>" + err);
            }

            return null;
        }

        /// <summary>
        /// Gets the balance of a Fungible CryptoItem or base NonFungible item for an application
        /// </summary>
        /// <param name="identityID">Identity ID of application to get CryptoItem balance from</param>
        /// <param name="tokenID">CryptoItem to get the balance for</param>
        /// <returns>Balance of CryptoItem</returns>
        public int GetCryptoItemBalance(int identityID, string tokenID)
        {
            string query = string.Empty;

            try
            {
                query = "query getCryptoItemBalance{result:EnjinIdentities(id:$id^){tokens(token_id:\"$id2^\"){balance}}}";
                GraphQuery.variable["id"] = identityID.ToString();
                GraphQuery.variable["id2"] = tokenID.ToString();
                GraphQuery.POST(query);

                var resultsGQL = JSON.Parse(GraphQuery.queryReturn);

                return resultsGQL["data"]["result"][0]["tokens"][0]["balance"].AsInt;
            }
            catch (Exception err)
            {
                Debug.Log("<color=magenta>[QUERY FAILED]</color>" + err);
            }

            return 0;
        }

        /// <summary>
        /// Returns the balance for a NFI index
        /// </summary>
        /// <param name="identityID">Identity ID of applicaiton to get item balance from</param>
        /// <param name="tokenID">Item to get balance for</param>
        /// <param name="index">Index of item</param>
        /// <returns></returns>
        public int GetCryptoItemBalance(int identityID, string tokenID, int index)
        {
            string query = string.Empty;

            try
            {
                query = "query getCryptoItemBalance{result:EnjinIdentities(id:$id^){tokens(token_id:\"$id2^\",token_index:\"$index^\"){balance}}}";
                GraphQuery.variable["id"] = identityID.ToString();
                GraphQuery.variable["id2"] = tokenID.ToString();
                GraphQuery.variable["index"] = index.ToString();
                GraphQuery.POST(query);

                var resultsGQL = JSON.Parse(GraphQuery.queryReturn);

                return resultsGQL["data"]["result"][0]["tokens"][0]["balance"].AsInt;
            }
            catch (Exception err)
            {
                Debug.Log("<color=magenta>[QUERY FAILED]</color>" + err);
            }

            return 0;
        }

        /// <summary>
        /// Updates an existing CryptoItem (WIP...)
        /// </summary>
        /// <param name="item">CryptoItem with updated fields</param>
        /// <returns>Updated CryptoItem as it was updated</returns>
        public CryptoItem UpdateCryptoItem(CryptoItem item)
        {
            return null;
        }

        /// <summary>
        /// Creates a new CryptoItem for current selected application
        /// </summary>
        /// <param name="newItem">CrytpoItem to create</param>
        /// <param name="identityID">CryptoItem creator user identity ID</param>
        /// <param name="callback">Callback action to execute when request is completed</param>
        /// <returns>Request data</returns>
        public Request CreateCryptoItem(CryptoItem newItem, int identityID, System.Action<RequestEvent> callback)
        {
            Request request = CreateCryptoItem(newItem, identityID);
            Enjin.RequestCallbacks.Add(request.id, callback);
            return request;
        }

        /// <summary>
        /// Creates a new CryptoItem for current selected application
        /// </summary>
        /// <param name="newItem">CryptoItem to create</param>
        /// <param name="identityID">CryptoItem creator user identity ID</param>
        /// <returns>Request data</returns>
        public Request CreateCryptoItem(CryptoItem newItem, int identityID)
        {
            string query = string.Empty;

            if (newItem.icon != null)
            {
                query = "mutation createCryptoItem{request:CreateEnjinRequest(type:CREATE,identity_id:$identity^,create_token_data:{name:\"$name^\",totalSupply:$totalSupply^,initialReserve:$reserve^,supplyModel:$model^,meltValue:\"$meltValue^\",meltFeeRatio:$meltFee^,transferable:$transferable^,transferFeeSettings:{type:$fType^, token_id:\"$fToken^\",value:\"$fValue^\"},nonFungible:$nonFungible^,icon:\"$icon^\"}){id,encoded_data,state}}";
                GraphQuery.variable["icon"] = newItem.icon;
            }
            else
                query = "mutation createCryptoItem{request:CreateEnjinRequest(type:CREATE,identity_id:$identity^,create_token_data:{name:\"$name^\",totalSupply:$totalSupply^,initialReserve:$reserve^,supplyModel:$model^,meltValue:\"$meltValue^\",meltFeeRatio:$meltFee^,transferable:$transferable^,transferFeeSettings:{type:$fType^, token_id:\"$fToken^\",value:\"$fValue^\"},nonFungible:$nonFungible^}){id,encoded_data,state}}";

            GraphQuery.variable["name"] = newItem.name;
            GraphQuery.variable["identity"] = identityID.ToString();
            GraphQuery.variable["totalSupply"] = newItem.totalSupply;
            GraphQuery.variable["reserve"] = newItem.reserve;
            GraphQuery.variable["model"] = newItem.supplyModel.ToString();
            GraphQuery.variable["meltValue"] = newItem.meltValue;
            GraphQuery.variable["meltFee"] = newItem.meltFeeRatio.ToString();
            GraphQuery.variable["transferable"] = newItem.transferable.ToString();
            GraphQuery.variable["fType"] = newItem.transferFeeSettings.type.ToString();
            GraphQuery.variable["fToken"] = newItem.transferFeeSettings.token_id;
            GraphQuery.variable["fValue"] = newItem.transferFeeSettings.value.ToString();
            GraphQuery.variable["nonFungible"] = newItem.nonFungible.ToString().ToLower();
            GraphQuery.POST(query);

            return JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
        }

        /// <summary>
        /// Gets the balance of CryptoItems that can be re-minted based on circulation and total supply
        /// </summary>
        /// <param name="itemID">CryptoItem ID of item to check for mintable balance</param>
        /// <returns>CryptoItems mintable balance</returns>
        public string GetMintableItems(string itemID)
        {
            string query = string.Empty;
            query = @"query getMintableTokens{mintable:EnjinTokens(token_id:""$id^""){availableToMint}}";
            GraphQuery.variable["id"] = itemID;
            GraphQuery.POST(query);

            var requestGQL = JSON.Parse(GraphQuery.queryReturn);

            return requestGQL["data"]["mintable"][0]["availableToMint"].Value;
        }
    }
}