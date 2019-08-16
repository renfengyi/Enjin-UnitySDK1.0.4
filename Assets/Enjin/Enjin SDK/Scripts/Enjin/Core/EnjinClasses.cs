namespace EnjinSDK
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;
    using EnjinSDK.Helpers;

    #region IDENTITY CLASSES
    /// <summary>
    /// Identity data structure
    /// </summary>
    [Serializable]
    public class Identity
    {
        [Serializable]
        public class User
        {
            public int id;
            public string name;

            public User()
            {
                id = 0;
                name = "";
            }
        }

        public int id;                  // Identity ID
        public int app_id;              // Application ID
        public User user;
        public string ethereum_address; // Ethereum address identity is linked to
        public string linking_code;     // Link code for linking identity to user wallet
        public DateData updated_at;     // Last update to identity
        public DateData created_at;     // Date identity created
        public Fields[] fields;         // Attached data to identiy

        public Identity()
        {
            id = 0;
            app_id = 0;
            user = new User();
            ethereum_address = "";
            linking_code = "";
            updated_at = new DateData();
            created_at = new DateData();
        }
    }

    /// <summary>
    /// Fields class for storing custom data in an identity
    /// </summary>
    [Serializable]
    public class Fields
    {
        public string key;              // Field key
        public string value;            // Description of field
        public int searchable;          // Is field searchable
        public int displayable;         // Is field displayable
        public int unique;              // Is field unique

        public Fields()
        {
            key = string.Empty;
            value = string.Empty;
            searchable = 1;
            displayable = 1;
            unique = 1;
        }

        public Fields(string key, string value, int searchable, int displayable, int unique)
        {
            this.key = key;
            this.value = value;
            this.searchable = searchable;
            this.displayable = displayable;
            this.unique = unique;
        }
    }

    [Serializable]
    public class FieldsResult { public Fields[] fields; }
    #endregion

    #region CRYPTOITEM CLASSES
    public class MetadataRetrievalException : Exception {}
    public class ImageRetrievalException : Exception { }

    [Serializable]
    public class MetadataParseException : Exception
    {
        public Exception inner;

        public MetadataParseException(Exception inner)
        {
            this.inner = inner;
        }
    }

    [Serializable]
    public class MetadataBase
    {
        public string name;
        public string description;
        public string image;
    }

    [Serializable]
    public class MetadataInfo
    {
        public enum MetadataRequestState
        {
            SUCCESS, RETRIEVAL_FAILED, PARSE_FAILED
        }

        public MetadataRequestState state;
        public Exception exception;
        public MetadataBase metadata;
    }

    [Serializable]
    public class ImageInfo
    {
        public enum ImageRequestState
        {
            SUCCESS, RETRIEVAL_FAILED, BAD_METADATA
        }

        public ImageRequestState state;
        public Exception exception;
        public Sprite image;
    }

    [Serializable]
    public class TransferFeeSettings
    {
        public TransferType type;
        public string token_id;
        public string value;

        public TransferFeeSettings()
        {
            type = TransferType.NONE;
            token_id = "";
            value = "";
        }
    }

    /// <summary>
    /// CryptoItem data structure
    /// </summary>
    [Serializable]
    public class CryptoItem
    {
        public string index;                            // Index used for nonfungible CryptoItems
        public string itemURI;                          // The URI containing metadata for this item
        public string token_id;                         // CryptoItem ID
        public string creator;                          // CryptoItem creator
        public string name;                             // CryptoItem name
        public string totalSupply;                      // Number of these items that exisit
        public string circulatingSupply;                // Number of CryptoItems in curculation
        public string reserve;                          // Initial supply of crypoItem to be available on chain
        public SupplyModel supplyModel;                 // Supply model type
        public string meltValue;                        // Fee for melting a CryptoItem
        public int meltFeeRatio;                        // Percentage of melt value returned to developer (50% MAX)
        public int meltFeeMaxRatio;                     // Max melt fee ratio. Set to melt fee ratio by defulat
        public Transferable transferable;               // CryptoItem transfer type
        public TransferFeeSettings transferFeeSettings; // Sets the type of transfers the crptoItem has
        public bool nonFungible;                        // Makes the cryptoItem non fungible
        public bool fromBlockchain;                     // Gets cryptoItem data from the block chain
        public string[] whitelists;                     // Collection of addresses which CryptoItems can be traded without approval
        public string icon;                             // URL to location of CryptoItem icon
        public Texture2D iconTexture;                   // Physical CryptoItem icon texture
        public int balance;                             // Balance of the CryptoItem that you physically have
        public bool isCreator;                          // Flag if item was created by current user
        public bool markedForDelete;                    // Flag if item has been marked for deletion

        public MetadataBase metadata;                   // A field to cache this item's basic metadata, if requested.

        /// <summary>
        /// Constructor
        /// </summary>
        public CryptoItem()
        {
            supplyModel = SupplyModel.FIXED;
            transferable = Transferable.PERMANENT;
            transferFeeSettings = new TransferFeeSettings();
        }

        // Retrieve the basic metadata object required for all items.
        internal IEnumerator GetMetadata(Action<MetadataInfo> listener)
        {

            // Prepare the response object for the callback.
            MetadataInfo response = new MetadataInfo();

            // Issue the web request to fetch the metadata JSON at the item URI.
            UnityWebRequest metadataRequest = UnityWebRequest.Get(itemURI);
            yield return metadataRequest.SendWebRequest();

            // If the request failed, log an error and throw an exception.
            if (metadataRequest.isNetworkError)
            {
                Debug.Log("<color=red>[ERROR]</color> Metadata network error: " + metadataRequest.error);
                response.state = MetadataInfo.MetadataRequestState.RETRIEVAL_FAILED;
                response.exception = new MetadataRetrievalException();
            }

            // Otherwise the request succeeded, so attempt to parse and return.
            else
            {
                string metadataString = metadataRequest.downloadHandler.text;

                try
                {
                    this.metadata = JsonUtility.FromJson<MetadataBase>(metadataString);
                    response.state = MetadataInfo.MetadataRequestState.SUCCESS;
                    response.metadata = this.metadata;
                }
                catch (Exception e)
                {
                    Debug.Log("<color=red>[ERROR]</color> Failed to parse metadata: " + metadataString);
                    response.state = MetadataInfo.MetadataRequestState.PARSE_FAILED;
                    response.exception = new MetadataParseException(e);
                }
            }

            // Trigger our callback.
            listener(response);
        }

        // A helper function for supporting image retrieval.
        private IEnumerator GetImageHelper(Action<ImageInfo> listener)
        {

            // Prepare a response object.
            ImageInfo response = new ImageInfo();

            // Retrieve the item's image.
            string imageURI = this.metadata.image;
            UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(imageURI);
            yield return imageRequest.SendWebRequest();

            // If the request failed, log an error and throw an exception.
            if (imageRequest.isNetworkError || imageRequest.isHttpError)
            {
                Debug.Log("<color=red>[ERROR]</color> Image network error: " + imageRequest.error);
                response.state = ImageInfo.ImageRequestState.RETRIEVAL_FAILED;
                response.exception = new ImageRetrievalException();
            }

            // Otherwise the request succeeded, so attempt to parse and return.
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(imageRequest);
                response.state = ImageInfo.ImageRequestState.SUCCESS;
                response.image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
            }

            // Trigger our callback.
            listener(response);
        }

        // Retrieve the image specified for this item in its metadata.
        internal IEnumerator GetImage(Action<ImageInfo> listener)
        {

            // First retrieve basic metadata if we have not done so for this item.
            if (this.metadata == null)
            {
                Debug.Log("Retrieving metadata first");

                this.GetMetadata((metadataInfo) =>
                {
                    if (metadataInfo.state == MetadataInfo.MetadataRequestState.SUCCESS)
                    {
                        this.metadata = metadataInfo.metadata;
                        GetImageHelper(listener);
                    }

                    // Trigger our callback with a notice about bad metadata.
                    else
                    {
                        ImageInfo response = new ImageInfo
                        {
                            state = ImageInfo.ImageRequestState.BAD_METADATA,
                            exception = new ImageRetrievalException()
                        };
                        listener(response);
                    }
                });
            }
            else
            {
                // Prepare a response object.
                ImageInfo response = new ImageInfo();

                // Retrieve the item's image.
                string imageURI = this.metadata.image;
                UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(imageURI);
                yield return imageRequest.SendWebRequest();

                // If the request failed, log an error and throw an exception.
                if (imageRequest.isNetworkError || imageRequest.isHttpError)
                {
                    Debug.Log("<color=red>[ERROR]</color> Image network error: " + imageRequest.error);
                    response.state = ImageInfo.ImageRequestState.RETRIEVAL_FAILED;
                    response.exception = new ImageRetrievalException();
                }

                // Otherwise the request succeeded, so attempt to parse and return.
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(imageRequest);
                    response.state = ImageInfo.ImageRequestState.SUCCESS;
                    response.image = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                }

                // Trigger our callback.
                listener(response);
            }
        }
    }

    /// <summary>
    /// CryptoItem balance data structure
    /// </summary>
    [Serializable]
    public class TokenBalanceEntry
    {
        public string token_id;     // CryptoItem id to get balance for
        public int balance;         // Balance of CryptoItem

        /// <summary>
        /// Constructor
        /// </summary>
        public TokenBalanceEntry()
        {
            token_id = string.Empty;
            balance = 0;
        }
    }

    /// <summary>
    /// CryptoItem Back data structure
    /// </summary>
    public class CryptoItemBatch
    {
        public List<string> Items { get; private set; }
        public int UserID { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userID"></param>
        public CryptoItemBatch(int userID)
        {
            Items = new List<string>();
            UserID = userID;
        }

        /// <summary>
        /// Adds a fungible item to the transfer list
        /// </summary>
        /// <param name="fromAddress">Address to send item from</param>
        /// <param name="toAddress">Address to send item to</param>
        /// <param name="item">Item to send</param>
        /// <param name="amount">Amount of item to send.</param>
        public void Add(string fromAddress, string toAddress, CryptoItem item, int amount)
        {
            if (amount <= 0)
                return;

            if (item.nonFungible)
            {
                string tItem = @"from:""{0}"",to:""{1}"",token_id:""{2}"",token_index:""{3}"",value:""1""";
                tItem = string.Format(tItem, fromAddress, toAddress, item.token_id, item.index);
                Items.Add(tItem);
            }
            else
            {
                string tItem = @"from:""{0}"",to:""{1}"",token_id:""{2}"",value:""{3}""";
                tItem = string.Format(tItem, fromAddress, toAddress, item.token_id, amount.ToString());
                Items.Add(tItem);
            }
        }

        /// <summary>
        /// Clears the list of items to transfer
        /// </summary>
        public void Clear() { Items.Clear(); }

        /// <summary>
        /// Sends the items to be transfered
        /// </summary>
        public void Send() { Enjin.SendBatchCryptoItems(this, UserID); }
    }

    #endregion

    #region REQUEST CLASSES (TRANSACTIONS)
    /// <summary>
    /// Request data structure (Transactions)
    /// </summary>
    [Serializable]
    public class Request
    {
        public int id;                  // ID
        public int transaction_id;      // Transaction ID
        public int app_id;              // Application ID
        public string type;             // Type of transaction
        public string icon;             // Icon for transaction
        public string title;            // Transaction title
        public string value;            // Transaction value
        public string state;            // Current state of transaction
        public int accepted;            // Has transaction been accepted
        public string updated_at;       // Last update for transaction
        public string created_at;       // Transaction creation stamp
    }

    [Serializable]
    public class TransferRequest
    {
        public Identity identity;       // Identity associated with transaction
        public Recipient recipient;     // Recipient of transaction
        public CryptoItem token;        // Token associated with transaction
    }

    [Serializable]
    public class RequestEventData
    {
        public int id;
        public int transaction_id;
        public string param1;
        public string param2;
        public string param3;
        public string param4;
        public int block_number;
        public string created_at;
        public string updated_at;
        public string deleted_at;
        public CryptoItem token;
        public string ethereum_address;
    }

    [Serializable]
    public class RequestEvent
    {
        public string model;
        public string event_type;
        public string contract;
        public RequestEventData data;
        public int request_id;
    }

    [Serializable]
    public class TokenValueInputData
    {
        public string id;
        public string index;
        public int value;

        public TokenValueInputData(string id, string index, int value)
        {
            this.id = id;
            this.index = index;
            this.value = value;
        }

        public static string ToGraphQL(TokenValueInputData data)
        {
            string val = @"{";

            if (EnjinHelpers.IsNullOrEmpty(data.index))
            {
                val += string.Format(@"id:""{0}"",value:{1}", data.id, data.value);
            }
            else
            {
                val += string.Format(@"id:""{0}"",index:{1},value:{2}", data.id, data.index, data.value);
            }

            val += @"}";
            return val;
        }

        public static string ToGraphQL(TokenValueInputData[] datas)
        {
            if (datas == null)
                return null;

            string val = @"[";

            foreach (TokenValueInputData data in datas)
            {
                if (data == null)
                    continue;

                if (val.Length > 1)
                {
                    val += @",";
                }

                val += ToGraphQL(data);
            }

            val += @"]";
            return val;
        }
    }
    #endregion

    #region PLATFORM CLASSES
    [Serializable]
    public class App
    {
        public int id;                  // Application id
        public string name;             // Application name
        public string description;      // Application description
        public string image;            // Application URL to image location

        public App()
        {
            id = 0;
            name = string.Empty;
            description = string.Empty;
            image = string.Empty;
        }
    }

    public class AppSelectorData
    {
        public int appID;
        public int identityID;
        public string appName;
    }

    [Serializable]
    public class PlatformOptions
    {
        public string cluster;
        public string encrypted;
    }

    [Serializable]
    public class SDK
    {
        public string driver;
        public string key;
        public string secret;
        public string app_id;
        public PlatformOptions options;
    }

    [Serializable]
    public class Mobile
    {
        public string type;
    }

    [Serializable]
    public class Notifications
    {
        public SDK sdk;
        public Mobile mobile;
    }

    [Serializable]
    public class Platform
    {
        public string name;
        public string id;
    }

    [Serializable]
    public class PlatformInfo
    {
        public string id;
        public string network;
        public string name;
        public Notifications notifications;
        public App[] apps;
    }
    #endregion

    #region MISC CLASSES
    // NOTE: ********** CLASS TO BE REPLACED WITH ErrorHandler CLASS **********
    [Serializable]
    public class ErrorStatus
    {
        public int ErrorCode;
        public string ErrorMessage;

        public ErrorStatus()
        {
            ErrorCode = 0;
            ErrorMessage = string.Empty;
        }
    }

    public class ErrorHandler : IErrorHandle
    {
        // Public Properties
        public int ErrorCode { get; private set; }
        public string Description { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ErrorHandler()
        {
            ErrorCode = 0;
            Description = string.Empty;
        }

        /// <summary>
        /// Sets the error details of the handler if one should occur
        /// </summary>
        /// <param name="code">Error code encountered</param>
        /// <param name="description">Description of the error encountered</param>
        public void SetErrorDetails(int code, string description)
        {
            ErrorCode = code;
            Description = description;
        }
    }

    [Serializable]
    public class JSONArrayHelper<T>
    {
        public T[] result;
    }

    [Serializable]
    public class PaginationHelper<T>
    {
        public T[] items;
        public Cursor cursor;
    }

    [Serializable]
    public class Cursor
    {
        public int total;           // Total number of records
        public bool hasPages;       // Has any number of pages
        public int perPage;         // Number of records per page
        public int currentPage;     // Currnet page requested
    }

    /// <summary>
    /// Recipient data structure for use with request transfers to other player accounts
    /// </summary>
    [Serializable]
    public class Recipient
    {
        public int id;                  // Recipient ID
        public int user_id;             // User id associated with recipient id
        public string ethereum_address; // Ethereum address linked to user
        public string linking_code;     // Linking code for ethereum address
        public string updated_at;       // Last update for recipient
        public string created_at;       // Recipient creation stamp
    }

    [Serializable]
    public class News
    {
        public string title;
        public string description;
        public string link;
    }

    [Serializable]
    public class NewsResult
    {
        public News[] news;
    }

    [Serializable]
    public class DateData
    {
        public string date;
        public int timezone_type;
        public string timezone;

        public DateData()
        {
            date = "";
            timezone_type = 0;
            timezone = "";
        }
    }

    [Serializable]
    public class Errors
    {
        public string message;
        public string code;

        public Errors()
        {
            message = "";
            code = "";
        }
    }
    #endregion
}