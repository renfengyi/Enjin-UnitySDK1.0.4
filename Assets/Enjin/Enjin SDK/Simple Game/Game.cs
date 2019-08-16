using System.Collections;
using System.Collections.Generic;
using EnjinSDK;
using UnityEngine;
using UnityEngine.UI;

// A very simple example game which demonstrates basic item functions on our platform.
public class Game : MonoBehaviour
{

    // Enjin SDK operation settings.
    public string PLATFORM_URL;
    public string DEVELOPER_USERNAME;
    public string DEVELOPER_PASSWORD;
    public int DEVELOPER_IDENTITY_ID;
    public int APP_ID;
    public string REWARD_TOKEN_ID;
    public int SCORE_THRESHOLD;

    // Variables for controlling the game state.
    private List<System.Action> pendingActions;
    private int score;
    private string linkingCode;
    private int identityId;
    private string userAddress;
    private User user;
    private int count;
    private int pending;
    private string tokenName;

    // Unity scene objects.
    private Text status;
    private Text tutorial;
    private GameObject authenticationPanel;
    private GameObject registrationPanel;
    private InputField registrationEmail;
    private GameObject proceedToLoginPanel;
    private GameObject loginPanel;
    private InputField loginEmail;
    private InputField loginPassword;
    private GameObject gamePanel;
    private Text inventory;
    private Image rewardTokenImage;
    private GameObject rewardMask;
    private Image rewardMaskImage;
    private List<GameObject> panelList;

    // A helper method to toggle the display of one specific panel at a time.
    private void ShowPanel(GameObject showPanel)
    {
        foreach (GameObject panel in panelList)
        {
            if (panel == showPanel)
            {
                foreach (Transform child in panel.transform)
                {
                    child.gameObject.SetActive(true);
                }
            }
            else
            {
                foreach (Transform child in panel.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    // A helper method to set the in-game status field and shake it a little.
    private void SetStatus(string message, bool shake = true)
    {
        status.text = message;
        status.GetComponent<Animator>().SetBool("shake", shake);
    }

    // A helper method to prepare the game for play.
    private void SetupPlay()
    {
        SetStatus("Linked successfully.\nYour score is: " + score, false);
        tutorial.text = Resources.Load<TextAsset>("play-game-tutorial").text;
        count = Enjin.GetCryptoItemBalance(identityId, REWARD_TOKEN_ID);
        inventory.text = (tokenName + "\nYou have: " + count + "\nCurrently pending: " + pending);
        ShowPanel(gamePanel);
    }

    // Initialize the game state on start.
    private void Start()
    {
        pendingActions = new List<System.Action>();
        score = 0;
        linkingCode = "";
        identityId = -1;
        userAddress = "";
        user = null;
        count = 0;
        pending = 0;
        tokenName = "";

        // Initialize references to scene objects.
        status = GameObject.Find("Status").GetComponent<Text>();
        tutorial = GameObject.Find("Tutorial").GetComponent<Text>();
        authenticationPanel = GameObject.Find("Authentication Panel");
        registrationPanel = GameObject.Find("Registration Panel");
        registrationEmail = registrationPanel.transform.Find("Email").GetComponent<InputField>();
        proceedToLoginPanel = GameObject.Find("Proceed to Login Panel");
        loginPanel = GameObject.Find("Login Panel");
        loginEmail = loginPanel.transform.Find("Email").GetComponent<InputField>();
        loginPassword = loginPanel.transform.Find("Password").GetComponent<InputField>();
        gamePanel = GameObject.Find("Game Panel");
        inventory = gamePanel.transform.Find("Inventory").GetComponent<Text>();
        rewardTokenImage = gamePanel.transform.Find("Reward Image").GetComponent<Image>();
        rewardMask = gamePanel.transform.Find("Reward Mask").gameObject;
        rewardMaskImage = rewardMask.GetComponent<Image>();
        panelList = new List<GameObject>
        {
            authenticationPanel,
            registrationPanel,
            proceedToLoginPanel,
            loginPanel,
            gamePanel
        };

        // Prepare the first scene.
        ShowPanel(authenticationPanel);
        tutorial.text = Resources.Load<TextAsset>("auth-method-tutorial").text;

        // Start the Enjin SDK.
        Enjin.StartPlatform(PLATFORM_URL, DEVELOPER_USERNAME, DEVELOPER_PASSWORD, APP_ID);
        Debug.Log("<color=aqua>[Simple Game]</color> Using app with ID " + Enjin.AppID);

        // Retrieve the specified reward token's metadata.
        CryptoItem rewardToken = Enjin.GetCryptoItem(REWARD_TOKEN_ID);
        StartCoroutine(rewardToken.GetMetadata((metadataInfo) =>
        {

            // Handle any potential errors in metadata retrieval.
            MetadataInfo.MetadataRequestState requestState = metadataInfo.state;
            switch (requestState)
            {
                case MetadataInfo.MetadataRequestState.PARSE_FAILED:
                    SetStatus("Unable to parse the reward item's metadata.");
                    break;
                case MetadataInfo.MetadataRequestState.RETRIEVAL_FAILED:
                    SetStatus("Unable to retrieve the reward item's metadata.");
                    break;
                case MetadataInfo.MetadataRequestState.SUCCESS:
                    {
                        tokenName = metadataInfo.metadata.name;
                        StartCoroutine(rewardToken.GetImage((imageInfo) =>
                        {

                            // Handle any potential errors with retrieving the item image.
                            ImageInfo.ImageRequestState imageRequestState = imageInfo.state;
                            switch (imageRequestState)
                            {
                                case ImageInfo.ImageRequestState.BAD_METADATA:
                                    SetStatus("Unable to handle item metadata for the image.");
                                    break;
                                case ImageInfo.ImageRequestState.RETRIEVAL_FAILED:
                                    SetStatus("Unable to retrieve the reward item's image.");
                                    break;
                                case ImageInfo.ImageRequestState.SUCCESS:
                                    rewardTokenImage.sprite = imageInfo.image;
                                    break;
                            }
                        }));
                        break;
                    }
            }
        }));
    }

    // Show the registration panel for first-time users.
    public void ShowRegistration()
    {
        ShowPanel(registrationPanel);
    }

    // Show the login panel for returning users.
    public void ShowLogin()
    {
        ShowPanel(loginPanel);
    }

    // Register the user as a new player on this app.
    public void Register()
    {
        string email = registrationEmail.text;
        Enjin.InviteUser(email);
        if (Enjin.ServerResponse != ResponseCodes.SUCCESS)
        {
            SetStatus("Failed to send registration email; check your logs.", true);
        }
        else
        {
            ShowPanel(proceedToLoginPanel);
        }
    }

    // Fetch the user's login details and log them in.
    public void Login()
    {

        // Retrieve and verify the user's login details.
        string email = loginEmail.text;
        string password = loginPassword.text;
        user = Enjin.VerifyLogin(email, password);

        // If the login was successful, proceed.
        switch (Enjin.LoginState)
        {
            case LoginState.VALID:
                {

                    // Find the identity for this app and confirm it has been linked.
                    foreach (Identity identity in user.identities)
                    {
                        if (identity.app_id == Enjin.AppID)
                        {
                            linkingCode = identity.linking_code;
                            identityId = identity.id;
                            userAddress = identity.ethereum_address;
                            break;
                        }
                    }

                    // If the user has already linked, proceed directly to play.
                    bool hasLinked = (linkingCode == "");
                    if (hasLinked)
                    {
                        SetupPlay();
                    }

                    // Otherwise register a listener to trigger once we have succeeded in linking.
                    else
                    {
                        SetStatus("Your wallet is unlinked.\nPlease use code: " + linkingCode);
                        tutorial.text = Resources.Load<TextAsset>("link-wallet-tutorial").text;
                        ShowPanel(null);
                        Enjin.ListenForLink(identityId, (requestEvent) =>
                        {
                            identityId = requestEvent.data.id;
                            userAddress = requestEvent.data.ethereum_address;
                            pendingActions.Add(SetupPlay);
                        });
                    }

                    break;
                }

            case LoginState.INVALIDUSERPASS:
                SetStatus("You entered an invalid\nusername or password.");
                break;
            case LoginState.INVALIDTPURL:
                SetStatus("This app is trying to reach an\ninvalid Trusted Cloud URL:\n" + Enjin.APIURL);
                break;
            case LoginState.UNAUTHORIZED:
                SetStatus("You are unauthorized for this login.");
                break;
        }
    }

    // On each update, check the current game state and handle logic appropriately.
    void Update()
    {

        // Handle any events posted here from an asynchronous Enjin task.
        for (int i = 0; i < pendingActions.Count; i++)
        {
            System.Action action = pendingActions[i];
            action();
        }
        pendingActions.Clear();

        // Reward the users with a token every time they get 15 clicks.
        if (score >= SCORE_THRESHOLD)
        {
            score = 0;
            SetStatus("Linked successfully.\nYour score is: " + score, false);
            rewardMask.transform.localScale = new Vector3(1, 1, 1);

            // Mint a new token directly from the developer wallet.
            CryptoItem item = Enjin.GetCryptoItem(REWARD_TOKEN_ID);
            string reserveCount = item.reserve;
            int developerBalance = Enjin.GetCryptoItemBalance(DEVELOPER_IDENTITY_ID, REWARD_TOKEN_ID);
            if (!reserveCount.Equals("0"))
            {
                pending += 1;
                Enjin.MintFungibleItem(DEVELOPER_IDENTITY_ID, new string[] { userAddress }, REWARD_TOKEN_ID, 1,
                (requestEvent) =>
                {
                    if (requestEvent.event_type.Equals("tx_executed"))
                    {
                        pending -= 1;
                        count += 1;
                        pendingActions.Add(() =>
                        {
                            inventory.text = (tokenName + "\nYou have: " + count + "\nCurrently pending: " + pending);
                        });
                    }
                });
                inventory.text = (tokenName + "\nYou have: " + count + "\nCurrently pending: " + pending);
            }

            // If the developer wallet is unable to mint reward tokens from the reserve, try to send it from the developer wallet.
            else if (developerBalance > 0)
            {
                pending += 1;
                Enjin.SendCryptoItemRequest(DEVELOPER_IDENTITY_ID, REWARD_TOKEN_ID, identityId, 1,
                (requestEvent) =>
                {
                    if (requestEvent.event_type.Equals("tx_executed"))
                    {
                        pending -= 1;
                        count += 1;
                        pendingActions.Add(() =>
                        {
                            inventory.text = (tokenName + "\nYou have: " + count + "\nCurrently pending: " + pending);
                        });
                    }
                });
                inventory.text = (tokenName + "\nYou have: " + count + "\nCurrently pending: " + pending);
            }

            // Otherwise there really is nothing of this token left for the developer to give out.
            else
            {
                SetStatus("Uh oh! The game developer is out of reward items!", true);
            }
        }
    }

    // Increment our score when the user presses the button.
    public void Click()
    {
        score++;
        SetStatus("Linked successfully.\nYour score is: " + score, false);
        rewardMask.transform.localScale = new Vector3(((SCORE_THRESHOLD - 1) - score) / ((SCORE_THRESHOLD - 1) * 1.0f), 1, 1);
    }
}
