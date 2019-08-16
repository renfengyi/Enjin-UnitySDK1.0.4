using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;

public class WalletTab
{
    private string _activeTokenBalance;
    private GUISkin _skin;

    public Texture2D ClipBoardIcon { get; set; }

    public WalletTab(GUISkin skin)
    {
        _skin = skin;
        ClipBoardIcon = skin.GetStyle("Images").normal.scaledBackgrounds[1];
        UpdateWalletBalances();
    }

    public void DrawWalletTab()
    {
        if (EnjinEditor.CurrentUserIdentity.linking_code != "")
            UnlinkedWallet();
        else
            LinkedWallet();
    }

    public void UpdateWalletBalances()
    {
        _activeTokenBalance = Enjin.GetTotalActiveTokens(EnjinEditor.CurrentUserIdentity.id).ToString();
        Enjin.UpdateBalances(EnjinEditor.CurrentUserIdentity);
    }

    private void UnlinkedWallet()
    {
        GUILayout.BeginArea(new Rect(5, 10, 912, 606), _skin.GetStyle("TopBackground"));
        EditorGUILayout.LabelField(new GUIContent("LINK WALLET"), _skin.GetStyle("MainTitle"));
        GUILayout.Space(30);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);
        EditorGUILayout.LabelField(new GUIContent("Step 1:"), _skin.GetStyle("BoldTitle"), GUILayout.Width(50));
        EditorGUILayout.LabelField(new GUIContent("Download the Enjin Wallet"), _skin.GetStyle("ContentLight"));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);

        //if (GUILayout.Button(new GUIContent("Enjin Wallet iOS"), GUILayout.Width(140), GUILayout.Height(30)))
        //    Application.OpenURL("https://itunes.apple.com/us/app/enjin-cryptocurrency-wallet/id1349078375?ls=1&mt=8");

        if (GUILayout.Button(new GUIContent("Enjin Wallet"), GUILayout.Width(140), GUILayout.Height(30)))
            Application.OpenURL("https://enjinwallet.io/");
            //Application.OpenURL("https://play.google.com/store/apps/details?id=com.enjin.mobile.wallet");

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);
        EditorGUILayout.LabelField(new GUIContent("Step 2:"), _skin.GetStyle("BoldTitle"), GUILayout.Width(50));
        EditorGUILayout.LabelField(new GUIContent("Open the Enjin Wallet, select LINK in settings and enter this\nunique code:"), _skin.GetStyle("ContentLight"), GUILayout.Height(30));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(66);
        EditorGUILayout.LabelField(new GUIContent(EnjinEditor.CurrentUserIdentity.linking_code), _skin.GetStyle("LargeNumbersGreen"));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);
        EditorGUILayout.LabelField(new GUIContent("Step 3:"), _skin.GetStyle("BoldTitle"), GUILayout.Width(50));
        EditorGUILayout.LabelField(new GUIContent("After you entered the code, click refresh button below to\nconfirm that your wallet and address is linked."), _skin.GetStyle("ContentLight"), GUILayout.Height(30));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);

        if (GUILayout.Button(new GUIContent("Refresh"), GUILayout.Width(140), GUILayout.Height(30)))
        {
            EnjinEditor.CurrentUser = Enjin.GetUserRaw(EnjinEditor.CurrentUser.id);
            EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADALL);
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }

    private void LinkedWallet()
    {
        GUILayout.BeginArea(new Rect(5, 10, 912, 606), _skin.GetStyle("TopBackground"));
        EditorGUILayout.LabelField(new GUIContent("WALLET DETAILS"), _skin.GetStyle("MainTitle"));
        GUILayout.Space(30);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.LabelField(new GUIContent("ENJ BALANCE"), _skin.GetStyle("Subtitle"), GUILayout.Width(140));
        EditorGUILayout.LabelField(new GUIContent("ETH BALANCE"), _skin.GetStyle("Subtitle"), GUILayout.Width(140));
        EditorGUILayout.LabelField(new GUIContent("ACTIVE TOKENS"), _skin.GetStyle("Subtitle"), GUILayout.Width(140));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.LabelField(new GUIContent(Enjin.GetEnjBalance.ToString("#,##0.###")), _skin.GetStyle("LargeNumbers"), GUILayout.Width(140));
        EditorGUILayout.LabelField(new GUIContent(Enjin.GetEthBalance.ToString("#,##0.########")), _skin.GetStyle("LargeNumbers"), GUILayout.Width(140));
        EditorGUILayout.LabelField(new GUIContent(_activeTokenBalance), _skin.GetStyle("LargeNumbers"), GUILayout.Width(140));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.LabelField(new GUIContent("LINKED ADDRESS"), _skin.GetStyle("Subtitle"), GUILayout.Width(120));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);
        EditorGUILayout.LabelField(new GUIContent(EnjinEditor.CurrentUserIdentity.ethereum_address), _skin.GetStyle("ContentDark"));
        GUILayout.Space(14);

        if (GUILayout.Button(new GUIContent(ClipBoardIcon, "Copy Wallet Address to Clipboard"), GUILayout.Width(32), GUILayout.Height(32)))
            EditorGUIUtility.systemCopyBuffer = EnjinEditor.CurrentUserIdentity.ethereum_address;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        //EditorGUILayout.BeginHorizontal();
        //GUILayout.Space(14);

        //if (GUILayout.Button(new GUIContent("Download Wallet"), GUILayout.Width(120), GUILayout.Height(30)))
        //    Application.OpenURL("https://enjinwallet.io/");

        //EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);
        EditorGUILayout.LabelField(new GUIContent("To change your wallet addres you will need to unlink this wallet."), _skin.GetStyle("ContentLight"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);

        if (GUILayout.Button(new GUIContent("Unlink Wallet"), GUILayout.Width(120), GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Unlink Wallet", "This will unlink this editor from your developer wallet. Do you want to proceed?", "Accept", "Cancel"))
            {
                Enjin.UnLinkIdentity(EnjinEditor.CurrentUserIdentity.id);
                EnjinEditor.CurrentUser = Enjin.GetUserRaw(EnjinEditor.CurrentUser.id);
                EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADALL);
            }
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}