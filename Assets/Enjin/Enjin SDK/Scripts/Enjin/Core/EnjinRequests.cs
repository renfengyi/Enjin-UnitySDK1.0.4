namespace EnjinSDK
{
    using UnityEngine;
    using GraphQLClient;
    using SimpleJSON;
    using EnjinSDK.Helpers;

    public class EnjinRequests
    {
        private string _query;      // Query string to be sent to API

        /// <summary>
        /// Gets a specific request by ID
        /// </summary>
        /// <param name="id">Request ID</param>
        /// <returns>Request of specified ID</returns>
        public Request Get(int id)
        {
            _query = "query getRequest{request:EnjinTransactions(id:$id^){id,transaction_id,app_id,type,icon,title,value,state,accepted,updated_at,created_at}}";
            GraphQuery.variable["id"] = id.ToString();
            GraphQuery.POST(_query);

            return JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(_query, 1));
        }

        /// <summary>
        /// Gets all requests made on the platform
        /// </summary>
        /// <returns>All requests on the platform</returns>
        public Request[] GetAll()
        {
            _query = "query getAllRequests{request:EnjinTransactions{id,transaction_id,app_id,type,icon,title,value,state,accepted,updated_at,created_at}}";
            GraphQuery.POST(_query);

            var tData = JsonUtility.FromJson<JSONArrayHelper<Request>>(EnjinHelpers.GetJSONString(_query, 1));

            return tData.result;
        }

        public Request CreateTradeRequest(int senderIdentityID, CryptoItem[] itemsFromSender, int[] amountsFromSender, string secondPartyAddress, int? secondPartyIdentityID, CryptoItem[] itemsFromSecondParty, int[] amountsFromSecondParty)
        {
            if (EnjinHelpers.IsNullOrEmpty(itemsFromSender) || EnjinHelpers.IsNullOrEmpty(amountsFromSender) || itemsFromSender.Length != amountsFromSender.Length)
                return null;

            if (EnjinHelpers.IsNullOrEmpty(itemsFromSecondParty) || EnjinHelpers.IsNullOrEmpty(amountsFromSecondParty) || itemsFromSecondParty.Length != amountsFromSecondParty.Length)
                return null;

            TokenValueInputData[] fromSender = new TokenValueInputData[itemsFromSender.Length];
            TokenValueInputData[] fromSecondParty = new TokenValueInputData[itemsFromSecondParty.Length];

            for (int i = 0; i < itemsFromSender.Length; i++)
            {
                CryptoItem item = itemsFromSender[i];
                int amount = amountsFromSender[i];
                fromSender[i] = new TokenValueInputData(item.token_id, item.index, amount);
            }

            for (int i = 0; i < itemsFromSecondParty.Length; i++)
            {
                CryptoItem item = itemsFromSecondParty[i];
                int amount = amountsFromSecondParty[i];
                fromSecondParty[i] = new TokenValueInputData(item.token_id, item.index, amount);
            }

            return CreateTradeRequest(senderIdentityID, fromSender, secondPartyAddress, secondPartyIdentityID, fromSecondParty);
        }

        public Request CreateTradeRequest(int senderIdentityID, CryptoItem[] itemsFromSender, int[] amountsFromSender, string secondPartyAddress, int? secondPartyIdentityID, CryptoItem[] itemsFromSecondParty, int[] amountsFromSecondParty, System.Action<RequestEvent> callback)
        {
            Request request = this.CreateTradeRequest(senderIdentityID, itemsFromSender, amountsFromSender, secondPartyAddress, secondPartyIdentityID, itemsFromSecondParty, amountsFromSecondParty);
            int requestID = request.id;
            Enjin.RequestCallbacks.Add(requestID, callback);
            return request;
        }

        public Request CreateTradeRequest(int senderIdentityID, TokenValueInputData[] itemsFromSender, string secondPartyAddress, int? secondPartyIdentityID, TokenValueInputData[] itemsFromSecondParty)
        {
            if (EnjinHelpers.IsNullOrEmpty(itemsFromSender) || EnjinHelpers.IsNullOrEmpty(itemsFromSecondParty))
                return null;

            if (secondPartyAddress == null && !secondPartyIdentityID.HasValue)
                return null;

            string askingStr = TokenValueInputData.ToGraphQL(itemsFromSecondParty);
            string offeringStr = TokenValueInputData.ToGraphQL(itemsFromSender);

            _query = @"mutation sendTrade{result:CreateEnjinRequest(identity_id:$senderIdentityID^,type:CREATE_TRADE,create_trade_data:{asking_tokens:$askingTokens^,offering_tokens:$offeringTokens^";
            if (secondPartyAddress != null)
            {
                _query += @",second_party_address:""$secondPartyAddress^""";
                GraphQuery.variable["secondPartyAddress"] = secondPartyAddress;
            }
            else
            {
                _query += @",second_party_identity_id:$secondPartyIdentityID^";
                GraphQuery.variable["secondPartyIdentityID"] = secondPartyIdentityID.ToString();
            }
            _query += @"}){id,encoded_data,state}}";

            GraphQuery.variable["senderIdentityID"] = senderIdentityID.ToString();
            GraphQuery.variable["askingTokens"] = askingStr;
            GraphQuery.variable["offeringTokens"] = offeringStr;

            GraphQuery.POST(_query);

            if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
                return JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));

            return null;
        }

        public Request CreateTradeRequest(int senderIdentityID, TokenValueInputData[] itemsFromSender, string secondPartyAddress, int? secondPartyIdentityID, TokenValueInputData[] itemsFromSecondParty, System.Action<RequestEvent> callback)
        {
            Request request = this.CreateTradeRequest(senderIdentityID, itemsFromSender, secondPartyAddress, secondPartyIdentityID, itemsFromSecondParty);
            int requestID = request.id;
            Enjin.RequestCallbacks.Add(requestID, callback);
            return request;
        }

        public Request CompleteTradeRequest(int secondPartyID, string tradeID)
        {

            // Build the query.
            _query = @"mutation sendTrade{result:CreateEnjinRequest(identity_id:$senderIdentityID^,type:COMPLETE_TRADE,complete_trade_data:{trade_id:""$tradeID^""}){id,encoded_data,state}}";

            // Populate the query.
            GraphQuery.variable["senderIdentityID"] = secondPartyID.ToString();
            GraphQuery.variable["tradeID"] = tradeID;

            GraphQuery.POST(_query);

            if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
                return JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));

            return null;
        }

        public Request CompleteTradeRequest(int senderIdentityID, string tradeID, System.Action<RequestEvent> callback)
        {
            Request request = this.CompleteTradeRequest(senderIdentityID, tradeID);
            int requestID = request.id;
            Enjin.RequestCallbacks.Add(requestID, callback);
            return request;
        }

        /// <summary>
        /// Sends a Token on the Trusted Platform and blockchain using full CryptoItem.
        /// Allows for fungible and nonfungible token requests
        /// </summary>
        /// <param name="identityID">Identity ID of requestor</param>
        /// <param name="item">CryptoItem to be sent</param>
        /// <param name="recipientID">Identity ID of reciving wallet</param>
        /// <param name="value">Number of tokens to be sent</param>
        /// /// <param name="value">Callback function to execute when request is fulfilled</param>
        /// <returns>Create request data from API</returns>
        public Request SendItem(int identityID, CryptoItem item, int recipientID, int value, System.Action<string> handler, bool async = false)
        {
            _query = @"mutation sendItem{CreateEnjinRequest(type:SEND,identity_id:$identity_id^,send_token_data:{recipient_identity_id:$recipient_id^, token_id: ""$token_id^"", ";
            if (item.nonFungible)
            {
                _query += @"token_index: ""$item_index^"", ";
            }
            _query += "value:$value^}){id,encoded_data,state}}";

            GraphQuery.variable["identity_id"] = identityID.ToString();
            GraphQuery.variable["token_id"] = item.token_id;
            if (item.nonFungible)
            {
                GraphQuery.variable["item_index"] = item.index;
            }
            GraphQuery.variable["recipient_id"] = recipientID.ToString();
            GraphQuery.variable["value"] = value.ToString();
            GraphQuery.POST(_query, "", async, (queryReturn) =>
            {
                handler?.Invoke(queryReturn);
            });

            if (GraphQuery.queryStatus == GraphQuery.Status.Complete)
            {
                return JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new Token on the Trusted Platform and blockchain
        /// </summary>
        /// <param name="identityID">Identity ID of creator</param>
        /// <param name="tokenID">Token ID to create</param>
        /// <param name="recipientID">Identity ID of reciving wallet</param>
        /// <param name="value">Number of tokens to be created</param>
        /// <returns>Create request data from API</returns>
        public Request SendItem(int identityID, string tokenID, int recipientID, int value, System.Action<string> handler, bool async = false)
        {
            _query = @"mutation sendItem{CreateEnjinRequest(type:SEND,identity_id:$identity_id^,send_token_data:{recipient_identity_id:$recipient_id^, token_id: ""$token_id^"", value:$value^}){id,encoded_data,state}}";
            GraphQuery.variable["identity_id"] = identityID.ToString();
            GraphQuery.variable["token_id"] = tokenID;
            GraphQuery.variable["recipient_id"] = recipientID.ToString();
            GraphQuery.variable["value"] = value.ToString();
            GraphQuery.POST(_query, "", async, (queryReturn) =>
            {
                handler?.Invoke(queryReturn);
            });

            if (GraphQuery.queryStatus == GraphQuery.Status.Complete)
            {
                return JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
            }
            else
            {
                return null;
            }
        }

        public Request SendItems(CryptoItemBatch sendItems, int userID)
        {
            _query = @"mutation advancedSend{CreateEnjinRequest(identity_id:" + userID.ToString() + ",type:ADVANCED_SEND,advanced_send_token_data:{transfers:[";

            for (int i = 0; i < sendItems.Items.Count; i++)
            {
                _query += "{" + sendItems.Items[i] + "}";

                if (i < sendItems.Items.Count - 1)
                    _query += ",";
            }

            _query += "]}){id,encoded_data}}";
            GraphQuery.POST(_query);

            Debug.Log("<color=white>[DEBUG INFO]</color> " + _query);

            if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
                return JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));

            return null;
        }

        public Request MintFungibleItem(int senderID, string[] addresses, string itemID, int value, System.Action<string> handler, bool async = false)
        {
            _query = @"mutation mintFToken{request:CreateEnjinRequest(identity_id:$senderID^,type:MINT,mint_token_data:{token_id:""$itemID^"",recipient_address_array:$addresses^,value:$value^}){id,encoded_data,state}}";
            GraphQuery.variable["senderID"] = senderID.ToString();
            GraphQuery.variable["addresses"] = EnjinHelpers<string>.ConvertToJSONArrayString(addresses);
            GraphQuery.variable["itemID"] = itemID;
            GraphQuery.variable["value"] = value.ToString();
            GraphQuery.POST(_query, "", async, (queryReturn) =>
            {
                handler?.Invoke(queryReturn);
            });

            if (GraphQuery.queryStatus == GraphQuery.Status.Complete)
            {
                return JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
            }
            else
            {
                return null;
            }
        }

        public Request MintNonFungibleItem(int senderID, string[] addresses, string itemID, System.Action<string> handler, bool async = false)
        {
            _query = @"mutation mintNFToken{request:CreateEnjinRequest(identity_id:$senderID^,type:MINT,mint_token_data:{token_id:""$itemID^"",recipient_address_array:$addresses^}){id,encoded_data,state}}";
            GraphQuery.variable["senderID"] = senderID.ToString();
            GraphQuery.variable["addresses"] = EnjinHelpers<string>.ConvertToJSONArrayString(addresses);
            GraphQuery.variable["itemID"] = itemID;
            GraphQuery.POST(_query, "", async, (queryReturn) =>
            {
                handler?.Invoke(queryReturn);
            });

            if (GraphQuery.queryStatus == GraphQuery.Status.Complete)
            {
                return JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Melts a specific amount of tokens
        /// </summary>
        /// <param name="identityID">Identity ID of user</param>
        /// <param name="itemID">CryptoItem ID</param>
        /// <param name="index">Index of item within a nonfungible item</param>
        /// <param name="amount">Numbner of cryptoItemss to melt</param>
        /// <returns>Melt request data from API</returns>
        public Request MeltItem(int identityID, string itemID, string index, int amount, System.Action<string> handler, bool async = false)
        {
            if (index != "")
            {
                _query = @"mutation meltToken{request:CreateEnjinRequest(type:MELT,identity_id:$identityid^,melt_token_data:{token_id:""$itemid^"",token_index:""$index^"",value:$amount^}){id,encoded_data,state}}";
                GraphQuery.variable["index"] = index.ToString();
            }
            else
                _query = @"mutation meltToken{request:CreateEnjinRequest(type:MELT,identity_id:$identityid^,melt_token_data:{token_id:""$itemid^"",value:$amount^}){id,encoded_data,state}}";

            GraphQuery.variable["identityid"] = identityID.ToString();
            GraphQuery.variable["itemid"] = itemID.ToString();
            GraphQuery.variable["amount"] = amount.ToString();
            GraphQuery.POST(_query, "", async, (queryReturn) =>
            {
                handler?.Invoke(queryReturn);
            });

            if (GraphQuery.queryStatus == GraphQuery.Status.Complete)
            {
                return JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Updates CryptoItem information on blockchain
        /// </summary>
        /// <param name="identityID">Identity of user</param>
        /// <param name="item">CryptoItem to update</param>
        /// <param name="fieldType">What field to update</param>
        /// <returns></returns>
        public Request UpdateCryptoItem(int identityID, CryptoItem item, CryptoItemFieldType fieldType, System.Action<RequestEvent> callback)
        {
            switch (fieldType)
            {
                case CryptoItemFieldType.NAME:
                    _query = @"mutation updateItemName{request:CreateEnjinRequest(identity_id:$identityID^,type:UPDATE_NAME,update_item_name_data:{token_id:""$id^"",name:""$name^""}){id,encoded_data,state}}";
                    GraphQuery.variable["id"] = item.token_id;
                    GraphQuery.variable["identityID"] = identityID.ToString();
                    GraphQuery.variable["name"] = item.name;
                    break;

                case CryptoItemFieldType.MELTFEE:
                    break;

                case CryptoItemFieldType.TRANSFERABLE:
                    break;

                case CryptoItemFieldType.TRANSFERFEE:
                    break;

                case CryptoItemFieldType.MAXMELTFEE:
                    break;

                case CryptoItemFieldType.MAXTRANSFERFEE:
                    break;
            }

            GraphQuery.POST(_query);

            if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
            {
                Request request = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
                Enjin.RequestCallbacks.Add(request.id, callback);

                return request;
            }

            return null;
        }

        /// <summary>
        /// Gets a cryptoItems metadata URI
        /// </summary>
        /// <param name="itemID">ID of cryptoItem metadata URI to get</param>
        /// <param name="itemIndex">optional NFI index of cryptoItem metadata URI to get, defaults to 0</param>
        /// <param name="replaceTags">optional boolean for whether or not to perform tag substitution in URI, defaults to true</param>
        /// <returns>Specifed cryptoItem metadata URI</returns>
        public string GetCryptoItemURI(string itemID, string itemIndex = "0", bool replaceTags = true)
        {
            _query = @"query cryptoItemURI{EnjinTokens(token_id:""$itemID^"", token_index:""$itemIndex^""){name,itemURI(replace_uri_parameters:$replaceTags^)}}";
            GraphQuery.variable["itemID"] = itemID;
            GraphQuery.variable["itemIndex"] = itemIndex;
            GraphQuery.variable["replaceTags"] = replaceTags.ToString().ToLower();
            GraphQuery.POST(_query);

            var requestGQL = JSON.Parse(GraphQuery.queryReturn);

            return requestGQL["data"]["EnjinTokens"][0]["itemURI"].Value;
        }

        /// <summary>
        /// Sets the metadata URI for a cryptoItem
        /// </summary>
        /// <param name="identityID">Identity ID of user setting metadata URI</param>
        /// <param name="itemID">ID of cryptoItem to set metadata URI for</param>
        /// <param name="itemData">metadata URI data</param>
        /// <returns></returns>
		public Request SetCryptoItemURI(int identityID, CryptoItem item, string itemData, System.Action<RequestEvent> callback)
        {
            if (item.index != null)
            {
                // Validate that index is not just empty, if so set it to null
                if (item.index == string.Empty)
                    item.index = null;
            }

            if (item.index == null || item.balance == 0)
            {
                _query = @"mutation setItemUri{request:CreateEnjinRequest(identity_id:$identityID^,type:SET_ITEM_URI,set_item_uri_data:{token_id:""$itemID^"",item_uri:""$itemData^""}){id,encoded_data,state}}";
            }
            else
            {
                _query = @"mutation setURI{request:CreateEnjinRequest(identity_id:$identityID^,type:SET_ITEM_URI,set_item_uri_data:{token_id:""$itemID^"",token_index:$tokenIndex^,item_uri:""$itemData^""}){id,encoded_data,state}}";
                GraphQuery.variable["tokenIndex"] = item.index.TrimStart('0');

                //int subItemIndex;

                //if (System.Int32.TryParse(item.index, out subItemIndex))
                //{
                //    GraphQuery.variable["tokenIndex"] = (subItemIndex + "");
                //}
            }

            GraphQuery.variable["identityID"] = identityID.ToString();
            GraphQuery.variable["itemID"] = item.token_id;
            GraphQuery.variable["itemData"] = itemData;
            GraphQuery.POST(_query);

            if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
            {
                Request request = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 2));
                Enjin.RequestCallbacks.Add(request.id, callback);
                return request;
            }

            return null;
        }

        /// <summary>
        /// Delete request by ID
        /// </summary>
        /// <param name="id">Request ID to delete</param>
        /// <returns>Success of deletion request</returns>
        public bool Delete(int id)
        {
            // Add call to delete request

            return false;
        }

        /// <summary>
        /// Cancels a request made by ID
        /// </summary>
        /// <param name="id">Request ID to cancel</param>
        /// <returns>Success of canceled request</returns>
        public bool Cancel(int id)
        {
            _query = "mutation cancelRequest{request:UpdateEnjinRequest(id:$id^,state:CANCELED_USER){id,state,transaction_id}}";
            GraphQuery.variable["id"] = id.ToString();
            GraphQuery.POST(_query);

            Request temp = JsonUtility.FromJson<Request>(EnjinHelpers.GetJSONString(GraphQuery.queryReturn, 1));

            if (temp.state == "CANCELED_USER")
                return true;

            return false;
        }
    }
}