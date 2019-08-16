using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;
using System.Text.RegularExpressions;

public class CT_CreatePanel
{
    private string nTest = "";

    public void DrawPane(CryptoItemsController controller, GUISkin skin)
    {
        if (controller.InitialItems > controller.TotalItems)
            controller.InitialItems = controller.TotalItems;

        // Input Trap for dynamic population of min melt fee and est ENJ cost
        if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
        {
            if (controller.NewCryptoItem.reserve != "0")
            {
                if (controller.LastReserve != controller.NewCryptoItem.reserve)
                {
                    controller.MinWei = System.Convert.ToInt64(GraphQLClient.GraphQuery.GetEndPointData(Enjin.MeltValueURL + controller.NewCryptoItem.reserve));
                    controller.LastReserve = controller.NewCryptoItem.reserve;
                }
            }
        }

        GUILayout.BeginArea(new Rect(5, 126, 912, 490), skin.GetStyle("TopBackground"));

        #region TITLE
        EditorGUILayout.LabelField(new GUIContent(controller.State + " CRYPTOITEM"), skin.GetStyle("MainTitle"));
        #endregion

        GUILayout.Space(30);
        EditorGUILayout.BeginHorizontal();

        #region PANEL COLUMN 1
        GUILayout.Space(16);
        EditorGUILayout.BeginVertical(GUILayout.Width(260));
        EditorGUILayout.LabelField(new GUIContent("CRYPTOITEM NAME"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        controller.NewCryptoItem.name = EditorGUILayout.TextField(controller.NewCryptoItem.name, skin.textField, GUILayout.Width(260), GUILayout.Height(30));

        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("TOTAL SUPPLY", "Max amount of CryptoItems that can be minted"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        controller.TotalItems = EditorGUILayout.IntField(controller.TotalItems, skin.textField, GUILayout.Width(260), GUILayout.Height(30));
        controller.TotalItems = (controller.TotalItems < 0) ? 0 : controller.TotalItems;
        controller.TotalItems = (controller.TotalItems > (int)Mathf.Pow(10, 9)) ? (int)Mathf.Pow(10, 9) : controller.TotalItems;
        controller.NewCryptoItem.totalSupply = controller.TotalItems.ToString();

        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("INITIAL RESERVE", "Actual amount of CryptoItems minted less than total supply. Allows for reserve CryptoItems"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        controller.InitialItems = EditorGUILayout.IntField(controller.InitialItems, skin.textField, GUILayout.Width(260), GUILayout.Height(30));
        controller.InitialItems = (controller.InitialItems <= 0) ? 1 : controller.InitialItems;
        controller.InitialItems = (controller.InitialItems > (int)Mathf.Pow(10, 9)) ? (int)Mathf.Pow(10, 9) : controller.InitialItems;
        controller.NewCryptoItem.reserve = controller.InitialItems.ToString();

        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("MELT VALUE (MIN COST: " + (controller.MinWei / 1000000000000000000).ToString("N8") + " ENJ)", "Base value of item in Enjin Coin"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        //nTest = EditorGUILayout.TextField(Regex.Replace(nTest, @"[^.0-9 ]", ""), skin.textField, GUILayout.Width(260), GUILayout.Height(30));
        //nTest = nTest.Replace(".", "");
        //nTest = nTest.PadRight(18, '0');
        controller.EnjValue = Mathf.Clamp(EditorGUILayout.FloatField(controller.EnjValue, skin.textField, GUILayout.Width(260), GUILayout.Height(30)), (float)(controller.MinWei / 1000000000000000000), Mathf.Pow(10, 9));
        controller.MeltValue = System.Convert.ToDecimal(controller.EnjValue);
        EditorGUILayout.EndVertical();
        #endregion

        #region PANEL COLUMN 2
        GUILayout.Space(20);
        EditorGUILayout.BeginVertical(GUILayout.Width(260));
        EditorGUILayout.LabelField(new GUIContent("SUPPLY TYPE"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        EditorStyles.popup.fixedHeight = 30;
        EditorStyles.popup.fontSize = 12;
        controller.ModelType = (SupplyModel2)EditorGUILayout.EnumPopup(controller.ModelType, GUILayout.Width(260), GUILayout.Height(30));
        controller.NewCryptoItem.supplyModel = (SupplyModel)System.Enum.Parse(typeof(SupplyModel), controller.ModelType.ToString());
        EditorStyles.popup.fixedHeight = 15;
        EditorStyles.popup.fontSize = 11;

        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("TRANSFERABLE"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        EditorStyles.popup.fixedHeight = 30;
        EditorStyles.popup.fontSize = 12;
        controller.NewCryptoItem.transferable = (Transferable)EditorGUILayout.EnumPopup(controller.NewCryptoItem.transferable, GUILayout.Width(260), GUILayout.Height(30));
        EditorStyles.popup.fixedHeight = 15;
        EditorStyles.popup.fontSize = 11;

        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("TRANSFER FEE SETTINGS"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        EditorStyles.popup.fixedHeight = 30;
        EditorStyles.popup.fontSize = 12;
        controller.NewCryptoItem.transferFeeSettings.type = (TransferType)EditorGUILayout.EnumPopup(controller.NewCryptoItem.transferFeeSettings.type, GUILayout.Width(260), GUILayout.Height(30));
        EditorStyles.popup.fixedHeight = 15;
        EditorStyles.popup.fontSize = 11;

        /**
         * Re-enable token ID (for NFI) after v1 as part of the expanded NFI task set
         */
        //if (controller.NewCryptoItem.transferFeeSettings.type != TransferType.NONE)
        //{
        //    GUILayout.Space(20);
        //    EditorGUILayout.LabelField(new GUIContent("TOKEN ID"), skin.GetStyle("Subtitle"), GUILayout.Width(120));

        //    controller.NewCryptoItem.transferFeeSettings.token_id = "0";
        //    EditorGUILayout.LabelField(controller.NewCryptoItem.transferFeeSettings.token_id, skin.GetStyle("LargeTextDark"), GUILayout.Width(120), GUILayout.Height(30));
        //}

        EditorGUILayout.EndVertical();
        #endregion

        #region PANEL COLUMN 3
        GUILayout.Space(20);
        EditorGUILayout.BeginVertical(GUILayout.Width(260));
        EditorGUILayout.LabelField(new GUIContent("CREATOR MELT FEE %", "Sets the max melt fee. Max value is 50"), skin.GetStyle("Subtitle"), GUILayout.Width(260)); // was 120
        controller.MeltFee = Mathf.Clamp(EditorGUILayout.FloatField(controller.MeltFee, skin.textField, GUILayout.Width(260), GUILayout.Height(30)), 0.00f, 50.00f);
        controller.NewCryptoItem.meltFeeRatio = (int)(controller.MeltFee * Mathf.Pow(10, 2));

        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("CRYPTOITEM TYPE"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        EditorStyles.popup.fixedHeight = 30;
        EditorStyles.popup.fontSize = 12;
        controller.ItemType = (CryptoItemType)EditorGUILayout.EnumPopup(controller.ItemType, GUILayout.Width(260), GUILayout.Height(30));
        EditorStyles.popup.fixedHeight = 15;
        EditorStyles.popup.fontSize = 11;
        controller.NewCryptoItem.nonFungible = (controller.ItemType == CryptoItemType.NONFUNGIBLE) ? true : false;

        if (controller.NewCryptoItem.transferFeeSettings.type != TransferType.NONE)
        {
            GUILayout.Space(20);

            if (controller.NewCryptoItem.transferFeeSettings.type == TransferType.NONE)
            {
                EditorGUILayout.LabelField("0", skin.GetStyle("LargeTextDark"), GUILayout.Width(120), GUILayout.Height(30));
                controller.NewCryptoItem.transferFeeSettings.token_id = "0";
                controller.NewCryptoItem.transferFeeSettings.value = ((decimal)0).ToString();
            }
            else if (controller.NewCryptoItem.transferFeeSettings.type == TransferType.RATIO_CUT || controller.NewCryptoItem.transferFeeSettings.type == TransferType.RATIO_EXTRA)
            {
                EditorGUILayout.LabelField(new GUIContent("TRANSFER FEE % (MAX 50)"), skin.GetStyle("Subtitle"), GUILayout.Width(120));

                controller.TransferFeeEnj = Mathf.Clamp(EditorGUILayout.FloatField(controller.TransferFeeEnj, skin.textField, GUILayout.Width(120), GUILayout.Height(30)), 0f, 50f);
                // 50% is 5000 in Unit16 (for queries); Max value for transferFee is 50% or 5000.
                controller.NewCryptoItem.transferFeeSettings.value = ((decimal)(Mathf.Pow(10, 2) * controller.TransferFeeEnj)).ToString();
            }
            else if (controller.NewCryptoItem.transferFeeSettings.type == TransferType.PER_CRYPTO_ITEM || controller.NewCryptoItem.transferFeeSettings.type == TransferType.PER_TRANSFER)
            {
                EditorGUILayout.LabelField(new GUIContent("TRANSFER FEE (ENJ)"), skin.GetStyle("Subtitle"), GUILayout.Width(120));

                controller.TransferFeeEnj = Mathf.Max(EditorGUILayout.FloatField(controller.TransferFeeEnj, skin.textField, GUILayout.Width(120), GUILayout.Height(30)), 0f);
                controller.NewCryptoItem.transferFeeSettings.value = ((decimal)(Mathf.Pow(10, 18) * controller.TransferFeeEnj)).ToString();
            }
        }

        EditorGUILayout.EndVertical();
        #endregion

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.BeginVertical();
        GUILayout.Space(10);
        EditorGUILayout.LabelField(new GUIContent("METADATA URI"), skin.GetStyle("Subtitle"), GUILayout.Width(120));
        controller.MetaDataURI = EditorGUILayout.TextField(controller.MetaDataURI, skin.textField, GUILayout.Height(30), GUILayout.Width(820));

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        #region BUTTON MENU
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("CREATE"), GUILayout.Width(100), GUILayout.Height(36)))
        {
            controller.Properties.Clear();
            bool process = true;

            if (controller.InitialItems == 0)
            {
                if (EditorUtility.DisplayDialog("WARNING", "Initial Reserve is set to 0. You do not have the permissions for this setting. Continue item creation with initial reserve set to 1?", "Pr1oceed", "Cancel"))
                    controller.NewCryptoItem.reserve = "1";
                else
                    process = false;
            }

            if ((decimal)controller.EnjValue < (controller.MinWei / 1000000000000000000))
            {
                if (EditorUtility.DisplayDialog("WARNING", "Your melt value is not greater than or equal to the minimum melt value. Would you like to set the melt value to the minimum (" + controller.MinWei / 1000000000000000000 + " ENJ) and continue?", "Proceed", "Cancel"))
                {
                    controller.EnjValue = (float)(controller.MinWei / 1000000000000000000);
                    controller.MeltValue = System.Convert.ToDecimal(controller.EnjValue);

                    if (controller.Properties.ContainsKey("MeltValue"))
                        controller.Properties.Remove("MeltValue");

                    controller.Properties.Add("MeltValue", (float)controller.MeltValue);
                }
                else
                    process = false;
            }
            else
            {
                controller.MeltValue = System.Convert.ToDecimal(controller.EnjValue);

                if (controller.Properties.ContainsKey("MeltValue"))
                    controller.Properties.Remove("MeltValue");

                controller.Properties.Add("MeltValue", (float)controller.MeltValue);
            }

            if (controller.MeltValue == 0)
            {
                EditorUtility.DisplayDialog("INVALID MELT VALUE", "Melt value can't be 0", "Ok");
                process = false;
            }

            if (controller.NewCryptoItem.transferFeeSettings.type != TransferType.NONE)
            {
                if (controller.TransferFeeEnj == 0)
                {
                    EditorUtility.DisplayDialog("WARNING", "Transfer Fee must be greater than 0.", "OK");
                    process = false;
                }
            }

            if (controller.TotalItems == 0)
            {
                EditorUtility.DisplayDialog("INVALID SUPPLY TOTAL", "Supply total must be at least 1", "Ok");
                process = false;
            }

            if (controller.NewCryptoItem.name == null || controller.NewCryptoItem.name.Trim() == "" || controller.NewCryptoItem.name == string.Empty)
            {
                EditorUtility.DisplayDialog("INVALID TOKEN NAME", "Token must have a name", "Ok");
                process = false;
            }

            if (controller.IsValidURI(controller.MetaDataURI))
            {
                if (controller.MetaDataURI != "")
                    controller.Properties.Add("MetaDataURI", controller.MetaDataURI);
            }
            else if(!controller.IsValidURI(controller.MetaDataURI))
            {
                EditorUtility.DisplayDialog("INVALID METADATA URI", "Your metadata URI must be valid in a web browser", "Ok");
                process = false;
            }

            // Please note that this call is attached to button press logic
            if (process)
            {
                Request request = controller.ProcessCryptoItem(ProcessTasks.CREATE, controller.NewCryptoItem, controller.Properties);

                if (EnjinEditor.IsRequestSuccessfull(request.state))
                    EditorUtility.DisplayDialog("SUCCESS", "The request has posted with a status of " + request.state + ". Please see your wallet to complete the transaction!", "Ok");
                else
                    EditorUtility.DisplayDialog("FAILURE", "The request could not be processed due to a status of " + request.state + ".", "Ok");

                controller.State = CryptoItemsController.CryptoItemState.MAIN;
            }
            // if process is false, we should already have shown a dialog so no false condition is required here.

        }

        if (GUILayout.Button(new GUIContent("BACK"), GUILayout.Width(100), GUILayout.Height(36)))
        {
            controller.Reset();
            controller.State = CryptoItemsController.CryptoItemState.MAIN;
        }

        GUILayout.Space(6);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        #endregion

        GUILayout.EndArea();
    }
}