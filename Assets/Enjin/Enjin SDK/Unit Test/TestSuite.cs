using EnjinSDK;
using System;
using UnityEngine;

/**
 * This script formalizes runtime testing of the SDK into a set of repeatable unit tests.
 */
public class TestSuite : MonoBehaviour
{

    // Settings for preparing the unit tests.
    public bool DEBUG;
    public string PLATFORM_URL;
    public string SERVER_EMAIL;
    public string SERVER_PASSWORD;
    public int DEVELOPER_IDENTITY_ID;
    public string TESTING_ITEM_ID;
    public int APP_ID;
    string DEVELOPER_TOKEN;
    string TESTER_TOKEN;
    string USER_ADDRESS;

    // Sequencing mechanism.
    int testingIdentityID = -1;
    Identity testingIdentity = null;
    bool sequenceOne = false;
    bool sequenceTwo = false;
    bool sequenceThree = false;
    bool sequenceFour = false;
    bool sequenceFive = false;
    int tradeId = -1;
    User loginUser = null;

    // Upon project start, execute the battery of Enjin SDK runtime function tests.
    void Start()
    {
        Debug.Log("=== Executing Enjin SDK runtime tests. ===");
        Enjin.IsDebugLogActive = DEBUG;

        Debug.Log("(1/8) Initializing the platform for use as game server ... ");
        Enjin.StartPlatform(PLATFORM_URL, SERVER_EMAIL, SERVER_PASSWORD, APP_ID);
        DEVELOPER_TOKEN = Enjin.AccessToken;
        Debug.Log(" ... PASSED.");

        Debug.Log("(2/8) Creating a new testing account ... ");
        long timestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        string testName = "test" + timestamp;
        string testEmail = testName + "@mail.com";
        string testPassword = "test";
        User testUser = new User
        {
            name = testName,
            email = testEmail,
            password = testPassword
        };
        Enjin.CreateUser(testUser.name, testUser.email, testUser.password, "Platform Owner");
        if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
        {
            Debug.Log(" ... PASSED.");

            Debug.Log("(3/8) Verifying login credentials for the testing account ... ");
            loginUser = Enjin.VerifyLogin(testEmail, testPassword);
            LoginState loginState = Enjin.LoginState;
            if (loginState == LoginState.VALID)
            {
                Debug.Log(" ... PASSED.");
                TESTER_TOKEN = loginUser.access_token;
                foreach (Identity identity in loginUser.identities)
                {
                    if (identity.app_id == APP_ID)
                    {
                        testingIdentityID = identity.id;
                    }
                }
                testingIdentity = Enjin.GetIdentity(testingIdentityID);
                string linkingCode = testingIdentity.linking_code;

                Debug.Log("(4/8) Establishing wallet link for the testing account. Please link with code " + linkingCode + " ... ");
                Enjin.ListenForLink(testingIdentityID, linkingData =>
                {
                    USER_ADDRESS = linkingData.data.ethereum_address;
                    Debug.Log(" ... PASSED.");
                    sequenceOne = true;
                });
            }
            else
            {
                Debug.LogError(" ... FAILED.");
            }
        }
        else
        {
            Debug.LogError(" ... FAILED.");
        }
    }

    private void Update()
    {
        // Sequence lock to wait for a successful callback on linking a wallet.
        if (sequenceOne)
        {
            sequenceOne = false;
            Debug.Log("(5/8) Sending an item from the developer account to the testing account ... ");
            Enjin.AccessToken = DEVELOPER_TOKEN;

            // Mint a new token directly from the developer wallet.
            CryptoItem item = Enjin.GetCryptoItem(TESTING_ITEM_ID);
            string reserveCount = item.reserve;
            int developerBalance = Enjin.GetCryptoItemBalance(DEVELOPER_IDENTITY_ID, TESTING_ITEM_ID);
            if (!reserveCount.Equals("0"))
            {
                Enjin.MintFungibleItem(DEVELOPER_IDENTITY_ID, new string[] { USER_ADDRESS }, TESTING_ITEM_ID, 1,
                (requestEvent) =>
                {
                    if (requestEvent.event_type.Equals("tx_executed"))
                    {
                        Debug.Log(" ... PASSED: MINTED.");
                        sequenceTwo = true;
                    }
                });
            }

            // If the developer wallet is unable to mint reward tokens from the reserve, try to send it from the developer wallet.
            else if (developerBalance > 0)
            {
                Enjin.SendCryptoItemRequest(DEVELOPER_IDENTITY_ID, TESTING_ITEM_ID, testingIdentityID, 1, sendData =>
                {
                    if (sendData.event_type.Equals("tx_executed"))
                    {
                        Debug.Log(" ... PASSED: SENT.");
                        sequenceTwo = true;
                    }
                });
            }

            // Otherwise there really is nothing of this token left for the developer to give out.
            else
            {
                Debug.Log(" ... FAILED: NO RESERVE TO MINT OR BALANCE TO SEND.");
            }
        }

        // Sequence lock to wait for a successful callback on sending an item.
        if (sequenceTwo)
        {
            sequenceTwo = false;
            Debug.Log("(6/8) Trading an item from the testing account to the developer account ... ");
            Enjin.AccessToken = TESTER_TOKEN;
            CryptoItem tradeToItem = new CryptoItem
            {
                token_id = TESTING_ITEM_ID,
                nonFungible = false
            };
            CryptoItem tradeFromItem = new CryptoItem
            {
                token_id = TESTING_ITEM_ID,
                nonFungible = false
            };
            testingIdentity = Enjin.GetIdentity(testingIdentityID);
            Identity developerIdentity = Enjin.GetIdentity(DEVELOPER_IDENTITY_ID);
            Enjin.CreateTradeRequest(testingIdentityID, new CryptoItem[] { tradeToItem }, new int[] { 1 },
            DEVELOPER_IDENTITY_ID, new CryptoItem[] { tradeFromItem }, new int[] { 1 }, tradeData =>
            {
                tradeId = tradeData.request_id;
                Debug.Log(" ... PASSED.");
                sequenceThree = true;
            });
        }

        // Sequence lock to wait for successful trade creation before completion.
        if (sequenceThree)
        {
            sequenceThree = false;
            Debug.Log("(7/8) Completing trade from the testing account to the developer account ... ");
            Enjin.AccessToken = DEVELOPER_TOKEN;
            Enjin.CompleteTradeRequest(DEVELOPER_IDENTITY_ID, "" + tradeId, tradeData =>
            {
                Debug.Log(" ... PASSED.");
                sequenceFour = true;
            });
        }

        // Sequence lock to wait for successful trade of item before melting.
        if (sequenceFour)
        {
            sequenceFour = false;
            Debug.Log("(8/8) Melting an item on the testing account ... ");
            Enjin.AccessToken = TESTER_TOKEN;
            Enjin.MeltTokens(testingIdentityID, TESTING_ITEM_ID, 1, meltData =>
            {
                Debug.Log(" ... PASSED.");
                sequenceFive = true;
            });
        }

        // Sequence lock to wait for successful completion of all tests before running static suite.
        if (sequenceFive)
        {
            sequenceFive = false;

            // Execute additional tests for non-runtime SDK calls.
            TestStaticEndpoints();
            Debug.Log("=== All tests executed successfully. ===");
        }
    }

    // Test the return status of a single method with void response type.
    private void TestMethod(System.Action testMethod, string displayName)
    {
        string passedState = "<color=green>PASSED!</color>";
        testMethod();
        if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
        {
            passedState = "<color=red>FAILED!</color> " + Enjin.ServerResponse.ToString();
        }
        Debug.Log("Testing " + displayName + " ... " + passedState);
    }

    // Test the return status of a single method with boolean response type.
    private void TestMethod<T>(Func<T> testMethod, T expected, string displayName)
    {
        string passedState = "<color=green>PASSED!</color>";
        T success = testMethod();
        if (!success.Equals(expected))
        {
            passedState = "<color=red>FAILED!</color>";
        }
        Debug.Log("Testing " + displayName + " ... " + passedState);
    }

    // This method tests Enjin's non-runtime API methods mostly used in the editor window.
    private void TestStaticEndpoints()
    {

        // Helper methods.
        TestMethod(() => { Enjin.StoreUserData(new User(), "test"); }, "user data storage");
        TestMethod<Boolean>(() => { return Enjin.ValidateAddress("0xf629cbd94d3791c9250152bd8dfbdf380e2a3b9c"); }, true, "address validation");
        TestMethod(() => { Enjin.URLGetData("https://enjincoin.io/", new System.Collections.Generic.Dictionary<string, string>()); }, "fetching URL data");
        TestMethod<String>(() => { return Enjin.URLGetData("bad URL", new System.Collections.Generic.Dictionary<string, string>()); }, string.Empty, "expected result from bad URL data");
        TestMethod(() => { Enjin.ResetErrorReport(); }, "error report reset");


        // CryptoItems methods.
        TestMethod(() => { Enjin.GetCryptoItemBalance(testingIdentityID, TESTING_ITEM_ID); }, "balance by identity");
        TestMethod(() => { Enjin.GetMintableItems(TESTING_ITEM_ID); }, "mintable item retrieval");
        TestMethod(() => { 
            CryptoItem item = Enjin.GetCryptoItem(TESTING_ITEM_ID);
            Enjin.GetCryptoItemIDByName(item.name);
        }, "item identity by name");
        TestMethod(() => { Enjin.GetCryptoItemBalances(testingIdentityID); }, "all balances by identity");
        TestMethod(() => { Enjin.UpdateCryptoItem(new CryptoItem()); }, "item updating");
        TestMethod(() => { Enjin.GetAllCryptoItems(); }, "searching all items");
        TestMethod(() => {
            CryptoItem item = Enjin.GetCryptoItem(TESTING_ITEM_ID);
            Enjin.GetCryptoItemIDByName(item.name);
        }, "searching for item name");
        TestMethod(() => { Enjin.GetAllItems(0, 0, DEVELOPER_IDENTITY_ID); }, "getting all items by page");
        TestMethod(() => { Enjin.GetAllItems(DEVELOPER_IDENTITY_ID); }, "getting all items");

        // Enjin Platform API methods.
        TestMethod(() => { Enjin.GetTotalActiveTokens(DEVELOPER_IDENTITY_ID); }, "getting all active tokens");
        TestMethod(() => { Enjin.GetAppsByUserID(loginUser.id); }, "getting apps for user");
        TestMethod(() => { Enjin.GetAppByID(APP_ID); }, "getting app by id");

        // Identity methods.
        TestMethod(() => { Enjin.UpdateBalances(new Identity()); }, "identity balance updates");
        TestMethod(() => { Enjin.GetIdentity(testingIdentityID); }, "identity retrieval");
        TestMethod(() => { Enjin.GetAllIdentities(); }, "bulk identity retrieval");
        TestMethod(() => { Enjin.SearchIdentities("enjin"); }, "identity search");
        Identity sampleIdentity = new Identity
        {
            user = new Identity.User
            {
                name = loginUser.name,
                id = loginUser.id
            },
            id = testingIdentityID,
            fields = new Fields[] { new Fields("test", "enjin", 0, 0, 0) }
        };
        TestMethod(() => { Enjin.UpdateIdentity(sampleIdentity); }, "identity update");
        Enjin.AccessToken = TESTER_TOKEN;
        TestMethod(() => { Enjin.UpdateIdentityFields(testingIdentityID, new Fields[] { new Fields("test", "enjin!", 0, 0, 0) }); }, "identity field update");
        TestMethod(() => { Enjin.LinkIdentity(sampleIdentity); }, "identity link");
        TestMethod(() => { Enjin.UnLinkIdentity(testingIdentityID); }, "identity unlinking");
        TestMethod(() => { Enjin.DeleteIdentity(testingIdentityID + ""); }, "identity deletion");
        TestMethod(() => { Enjin.CreateIdentity(sampleIdentity); }, "identity creation");
        TestMethod(() => { Enjin.GetRoles(); }, "fetching identity roles");
        TestMethod(() => { Enjin.GetIdentities(0, 0); }, "fetching identity pages");
    }
}
