namespace EnjinEditorPanel
{
    using System;
    using UnityEngine;
    using UnityEditor;
    using EnjinSDK;
    using System.Collections.Generic;

    public enum CryptoItemType { FUNGIBLE, NONFUNGIBLE }

    public class CryptoItemsController
    {
        #region Variables, Objects & Properties
        private readonly EnjinEventManager _enjinEventManager;
        private static Dictionary<string, Result> _resultQueue;

        // moved to EnjinEditor.cs
        //public enum ProcessCryptoItemTasks { CREATE, MELT, MINT, EDIT, DELETE, SETURI };
        //public enum Status { NONE, SUCCESS, FAILURE };
        public enum CryptoItemState { MAIN, CREATE, CREATEBUNDLE, EDIT, VIEW, MINT, MELT, REFRESH, SETURI }
        public enum ItemFilterType { SELECTED, OWNED, CREATED, ALL }

        public CryptoItemState State { get; set; }
        public ItemFilterType ItemFilter { get; set; }
        public CryptoItemType ItemType { get; set; }
        public GUIStyle NumStyle { get; set; }
        public Vector2 ScrollIndex { get; set; }
        public Vector2 ScrollPos { get; set; }
        public CryptoItem NewCryptoItem { get; set; }
        public CryptoItem CurrentCryptoItem { get; set; }
        public List<CryptoItem> CryptoItemList { get; set; }
        public Texture Icon { get; set; }
        public Texture2D ClipBoard { get; set; }
        public Dictionary<string, int> TokenBalanceMap { get; set; }
        public List<string> Whitelist { get; set; }
        public PaginationHelper<CryptoItem> Pagination { get; set; }
        public RequestEvent Request { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public SupplyModel2 ModelType { get; set; }

        public string Address { get; set; }
        public string[] SearchOptions = { "Global", "Selected App", "Created by Wallet", "Owned by Wallet" };
        public string SupplyContractAddress { get; set; }
        public string SearchText { get; set; }
        public string PageNumber { get; set; }
        public string EditItemName { get; set; }
        public string EditMetaDataURI { get; set; }
        public string MetaDataURI { get; set; }
        public string LastReserve { get; set; }
        public string MintableCryptoItems { get; set; }
        public string[] ListItems { get; set; }
        public string[] RecieverAddress { get; set; }
        public int SearchOptionIndex { get; set; }
        public int TotalItems { get; set; }                 // Should be pulled from settings
        public int InitialItems { get; set; }
        public int FilterSelection { get; set; }
        public int LastFilterSelected { get; set; }
        public int SelectedIndex { get; set; }
        public int Reserve { get; set; }
        public int FirstPage { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int NumToMint { get; set; }
        public float MeltFee { get; set; }
        public float TransferFeeEnj { get; set; }
        public float EnjValue { get; set; }
        public double TotalEnjCost { get; set; }
        public double TotalReserveCost { get; set; }
        public double MeltValue2 { get; set; }
        public decimal MeltValue { get; set; }
        public decimal MinWei { get; set; }
        public bool IsSearchMode { get; set; }
        public bool HasListRefreshed { get; set; }
        public bool IsEditingWhitelist { get; set; }
        public bool[] Pages { get; set; }
        public int Balance { get; set; }
        public int NumToMelt { get; set; }
        public double EnjReturned { get; set; }
        public double EnjPerItem { get; set; }
        #endregion

        public CryptoItemsController()
        {
            _resultQueue = new Dictionary<string, Result>();

            ModelType = SupplyModel2.FIXED;
            ItemType = CryptoItemType.FUNGIBLE;
            CurrentPage = 1;
            FirstPage = 1;
            PageNumber = "01";
            Pagination = Enjin.GetAllItems(CurrentPage, EnjinEditor.ItemsPerPage, EnjinEditor.CurrentUserIdentity.id);
            SelectedIndex = 0;
            SearchOptionIndex = 0;
            InitialItems = 1;
            FilterSelection = 1;
            LastFilterSelected = 1;
            ListItems = new string[9];
            SelectedIndex = 0;
            EnjValue = 0;
            ScrollPos = Vector2.zero;
            LastFilterSelected = 1;
            CurrentCryptoItem = new CryptoItem();
            HasListRefreshed = true;
            IsSearchMode = false;
            Whitelist = new List<string>();
            NumToMelt = 0;
            EnjReturned = 0;
            EnjPerItem = 0;
            MinWei = System.Convert.ToInt64(GraphQLClient.GraphQuery.GetEndPointData(Enjin.MeltValueURL + 1));

            if (Pagination != null)
            {
                TotalPages = (int)Mathf.Ceil((float)Pagination.cursor.total / (float)Pagination.cursor.perPage);
                CryptoItemList = new List<CryptoItem>(Pagination.items);
            }

            Reset();
            Properties = new Dictionary<string, object>();
        }

        public void UpdateEnjPerItem()
        {
            EnjPerItem = (System.Convert.ToDouble(CurrentCryptoItem.meltValue) / Mathf.Pow(10, 18));
        }

        public Dictionary<string, Result> GetResults()
        {
            return _resultQueue;
        }

        public void AddResult(Result result)
        {
            _resultQueue.Add(result.Request.transaction_id.ToString(), result);
        }

        public void RemoveResult(Result result)
        {
            _resultQueue.Remove(result.Request.transaction_id.ToString());
        }

        public void RemoveResult(string id)
        {
            _resultQueue.Remove(id);
        }

        public decimal GetMinWei(string reserve)
        {
            return System.Convert.ToInt64(GraphQLClient.GraphQuery.GetEndPointData(Enjin.MeltValueURL + reserve));
        }

        /// <summary>
        /// Takes the current CI's melt Value and Item count as input and calculates the expected melt value
        /// </summary>
        /// <param name="meltValue"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public string GetMeltValue(decimal meltValue, int items)
        {
            if (meltValue <= MinWei)
                return ((System.Convert.ToInt64(GraphQLClient.GraphQuery.GetEndPointData(Enjin.MeltValueURL + 1)) / 1000000000000000000) * items).ToString("N7") + " ENJ";
            else
                return (meltValue * items).ToString("N7") + " ENJ";
        }

        internal Request ProcessCryptoItem(ProcessTasks task, CryptoItem cryptoItem, Dictionary<string, object> properties)
        {
            Request request = new Request();
            string editMetaDataURI = "";

            switch (task)
            {
                case ProcessTasks.CREATE:
                    string minValue = GraphQLClient.GraphQuery.GetEndPointData(Enjin.MeltValueURL + cryptoItem.reserve);

                    if (properties.ContainsKey("MeltValue"))
                    {
                        decimal actual = (decimal)((float)properties["MeltValue"] * Mathf.Pow(10, 18));

                        if (actual < System.Convert.ToInt64(minValue))
                            cryptoItem.meltValue = minValue;
                        else
                            cryptoItem.meltValue = actual.ToString("0");
                    }
                    else
                    {
                        cryptoItem.meltValue = minValue;
                    }


                    if (properties.ContainsKey("MetaDataURI"))
                    {
                        request = Enjin.CreateCryptoItem(cryptoItem, EnjinEditor.CurrentUserIdentity.id, ItemCreated => { HandleChainedRequestAction(request, ItemCreated, ProcessTasks.SETURI, cryptoItem, properties); });
                    }
                    else
                    {
                        request = Enjin.CreateCryptoItem(cryptoItem, EnjinEditor.CurrentUserIdentity.id, ItemCreated => { HandleRequestAction(request); });
                    }

                    return request;

                case ProcessTasks.DELETE:
                    break;

                case ProcessTasks.EDIT:
                    if (cryptoItem.isCreator)
                    {
                        // handle name changes
                        if (properties.ContainsKey("ItemName"))
                        {
                            string editItemName = (string)properties["ItemName"];

                            if (editItemName != cryptoItem.name)
                            {
                                Debug.Log("running name change");
                                string temp = cryptoItem.name;
                                cryptoItem.name = editItemName;
                                request = Enjin.UpdateCryptoItem(EnjinEditor.CurrentUserIdentity.id, cryptoItem, CryptoItemFieldType.NAME, ItemUpdated => { HandleRequestAction(request); });
                                cryptoItem.name = temp;
                            }
                        }

                        // handle metadata URI changes
                        if (properties.ContainsKey("MetaDataURI"))
                        {
                            editMetaDataURI = (string)properties["MetaDataURI"];

                            if (editMetaDataURI != cryptoItem.itemURI)
                            {
                                Debug.Log("running meta data URI update");
                                request = Enjin.SetCryptoItemURI(EnjinEditor.CurrentUserIdentity.id, cryptoItem, editMetaDataURI, URISet => { HandleRequestAction(request); });
                            }
                        }

                        return request;
                    }
                    return null;

                case ProcessTasks.MELT:
                    int numToMelt = 1;
                    if (properties.ContainsKey("NumToMelt"))
                        numToMelt = (int)properties["NumToMelt"];

                    if (cryptoItem.nonFungible)
                    {
                        Debug.Log("running melt request for non-fungible CI");
                        request = Enjin.MeltTokens(EnjinEditor.CurrentUserIdentity.id, cryptoItem.token_id, cryptoItem.index, numToMelt, MeltItem => { HandleRequestAction(request); });
                    }
                    else
                    {
                        Debug.Log("running melt request for fungible CI");
                        request = Enjin.MeltTokens(EnjinEditor.CurrentUserIdentity.id, cryptoItem.token_id, numToMelt, MeltItem => { HandleRequestAction(request); });
                    }

                    return request;

                case ProcessTasks.MINT:
                    string[] addresses = new string[] { };
                    if (properties.ContainsKey("RecieverAddress"))
                        addresses = (string[])properties["RecieverAddress"];

                    if (!cryptoItem.nonFungible)
                    {
                        int numToMint = 0;
                        if (properties.ContainsKey("NumToMint"))
                            numToMint = (int)properties["NumToMint"];

                        // Request should be pushed to event system to register the request by id, then the action callback should push to a method that fires off the seturi method once executed.
                        request = Enjin.MintFungibleItem(EnjinEditor.CurrentUserIdentity.id, addresses, cryptoItem.token_id, numToMint, ItemMinted => { HandleRequestAction(request); });
                    }
                    else
                        // Request should be pushed to event system to register the request by id, then the action callback should push to a method that fires off the seturi method once executed.
                        request = Enjin.MintNonFungibleItem(EnjinEditor.CurrentUserIdentity.id, addresses, cryptoItem.token_id, ItemMinted => { HandleRequestAction(request); });

                    return request;
                case ProcessTasks.SETURI:
                    // handle metadata URI changes
                    if (properties.ContainsKey("MetaDataURI"))
                    {
                        editMetaDataURI = (string)properties["MetaDataURI"];

                        if (editMetaDataURI != cryptoItem.itemURI)
                        {
                            Debug.Log("running meta data URI update");
                            request = Enjin.SetCryptoItemURI(EnjinEditor.CurrentUserIdentity.id, cryptoItem, editMetaDataURI, URISet => { HandleRequestAction(request); });
                        }
                    }

                    return request;
                default:
                    break;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        public void HandleRequestAction(Request request)
        {
            Debug.Log("<color=violet>[Handler Request]</color> " + JsonUtility.ToJson(request));

            Result result = new Result
            {
                Type = Result.Types.CRYPTOITEMS,
                Request = request,
                Refresh = true,
                Message = JsonUtility.ToJson(request),
                Status = (EnjinEditor.IsRequestSuccessfull(request.state)) ? Status.SUCCESS : Status.FAILURE
            };

            EnjinEditor.NotificationMonitor.ResultsQueue.Add(System.Guid.NewGuid().ToString(), result);
            //_resultQueue.Add(System.Guid.NewGuid().ToString(), result);
        }

        public void HandleChainedRequestAction(Request request, RequestEvent requestEvent, ProcessTasks task, CryptoItem cryptoItem, Dictionary<string, object> properties)
        {
            Debug.Log("<color=violet>[Handler Request]</color> " + JsonUtility.ToJson(request));

            Result result = new Result
            {
                Type = Result.Types.CRYPTOITEMS,
                Request = request,
                Refresh = true,
                Message = JsonUtility.ToJson(request),
                Status = (EnjinEditor.IsRequestSuccessfull(request.state)) ? Status.SUCCESS : Status.FAILURE
            };

            CompoundQuery query = new CompoundQuery(task, requestEvent.data.token, properties);
            result.compoundQueries.Add(query);

            EnjinEditor.NotificationMonitor.ResultsQueue.Add(System.Guid.NewGuid().ToString(), result);
            //_resultQueue.Add(System.Guid.NewGuid().ToString(), result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cryptoItem"></param>
        /// <param name="meltValue"></param>
        /// <returns></returns>
        private CryptoItem SetMeltValue(CryptoItem cryptoItem, float meltValue)
        {
            string minValue = GraphQLClient.GraphQuery.GetEndPointData(Enjin.MeltValueURL + cryptoItem.reserve);
            decimal actual = (decimal)(meltValue * Mathf.Pow(10, 18));

            if (actual < System.Convert.ToInt64(minValue))
                cryptoItem.meltValue = minValue;
            else
                cryptoItem.meltValue = actual.ToString("0");

            return cryptoItem;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        private void ShowRequestPopup(Request request)
        {
            if (EnjinEditor.IsRequestSuccessfull(request.state))
                EditorUtility.DisplayDialog("SUCCESS", "The request has posted with a status of " + request.state + ". Please see your wallet to complete the transaction!", "Ok");
            else
                EditorUtility.DisplayDialog("FAILURE", "The request could not be processed due to a status of " + request.state + ".", "Ok");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cryptoItem"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private Request SendCreateRequest(CryptoItem cryptoItem, Action<RequestEvent> action)
        {
            Request request = Enjin.CreateCryptoItem(cryptoItem, EnjinEditor.CurrentUserIdentity.id, action);

            return request;
        }

        #region Utility Methods

        /// <summary>
        /// Pagination page check for searchable index
        /// </summary>
        public void PageCheck()
        {
            if (CurrentPage < FirstPage)
                FirstPage = CurrentPage;

            if (CurrentPage == (FirstPage + 10))
                FirstPage++;

            Pagination = Enjin.GetAllItems(CurrentPage, EnjinEditor.ItemsPerPage, EnjinEditor.CurrentUserIdentity.id);
            CryptoItemList = new List<CryptoItem>(Pagination.items);
            SelectedIndex = 0;
        }

        /// <summary>
        /// Reset and revalidate the CI List
        /// </summary>
        public void ResetCryptoItemList()
        {
            if (CryptoItemList != null)
            {
                CryptoItemList.Clear();
                CurrentPage = FirstPage;
                Pagination = Enjin.GetAllItems(CurrentPage, EnjinEditor.ItemsPerPage, EnjinEditor.CurrentUserIdentity.id);

                if (Pagination != null)
                {
                    TotalPages = (int)Mathf.Ceil((float)Pagination.cursor.total / (float)Pagination.cursor.perPage);
                    CryptoItemList = new List<CryptoItem>(Pagination.items);
                }
                else
                    TotalPages = 1;
            }

            State = CryptoItemState.MAIN;
        }

        /// <summary>
        /// Local UI reset function for working CI object
        /// </summary>
        public void Reset()
        {
            //CurrentCryptoItem = new CryptoItem();
            NewCryptoItem = new CryptoItem();
            MetaDataURI = "";
            Whitelist.Clear();
            MeltValue = 0;
            MeltFee = 0;
            NumToMint = 1;
            NumToMelt = 0;
            TotalItems = 0;
            InitialItems = 1;
            EnjValue = 0;
            State = CryptoItemState.MAIN;
            IsEditingWhitelist = false;
            ScrollIndex = Vector2.zero;
            Address = "";
            UpdateEnjPerItem();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaDataURI"></param>
        /// <returns></returns>
        public bool IsValidURI(string metaDataURI)
        {
            if (metaDataURI.Equals(""))
            {
                return true;
            }
            else if (Uri.IsWellFormedUriString(metaDataURI, UriKind.Absolute))
            {
                return true;
            }
            else if (metaDataURI.Contains("{") && metaDataURI.Contains("}"))
            {
                int leftCount = metaDataURI.Length - metaDataURI.Replace("{", "").Length;
                int rightCount = metaDataURI.Length - metaDataURI.Replace("}", "").Length;
                return (leftCount == rightCount);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// check if current identity context is creator of CI
        /// </summary>
        /// <param name="creator"></param>
        /// <returns></returns>
        public bool IsCreator(string creator)
        {
            return (creator.ToUpper() == EnjinEditor.CurrentUserIdentity.ethereum_address.ToUpper()) ? true : false;
        }

        #endregion
    }
}
