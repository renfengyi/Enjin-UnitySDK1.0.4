using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;

public class CT_EditPanel
{
    /// <summary>
    /// Build and Draw View/Edit CI Panel
    /// Embedded UI logic for field validation and request return status (Edit Mode)
    /// </summary>
    public void DrawPane(CryptoItemsController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 126, 912, 490), skin.GetStyle("TopBackground"));
        EditorGUILayout.LabelField(new GUIContent(controller.State + " CRYPTOITEM"), skin.GetStyle("MainTitle"));
        GUILayout.Space(30);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.BeginVertical(GUILayout.Width(260));

        EditorGUILayout.LabelField(new GUIContent("CRYPTOITEM NAME"), skin.GetStyle("Subtitle"), GUILayout.Width(260));

        if (controller.CurrentCryptoItem.isCreator && controller.State.Equals(CryptoItemsController.CryptoItemState.EDIT))
            controller.EditItemName = EditorGUILayout.TextField(controller.EditItemName, skin.textField, GUILayout.Width(260), GUILayout.Height(30));
        else
            EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.name), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));

        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("TOTAL SUPPLY", "Total number of items in the supply"), skin.GetStyle("Subtitle"), GUILayout.Width(260));

        if (controller.CurrentCryptoItem.supplyModel != SupplyModel.INFINITE)
            EditorGUILayout.LabelField(new GUIContent(System.String.Format("{0:N0}", System.Double.Parse(controller.CurrentCryptoItem.totalSupply))), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));
        else
            EditorGUILayout.LabelField(new GUIContent("INFINITE"), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));

        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("INITIAL RESERVE", "Number of items that are prepaid for"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        EditorGUILayout.LabelField(System.String.Format("{0:N0}", System.Double.Parse(controller.CurrentCryptoItem.reserve)), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));
        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("MELT VALUE", "Base value of item in Enjin Coin"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        EditorGUILayout.LabelField(new GUIContent((System.Convert.ToDouble(controller.CurrentCryptoItem.meltValue) / Mathf.Pow(10, 18)).ToString("N4") + " ENJ"), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));
        EditorGUILayout.EndVertical();
        GUILayout.Space(20);
        EditorGUILayout.BeginVertical(GUILayout.Width(260));
        EditorGUILayout.LabelField(new GUIContent("SUPPLY TYPE"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.supplyModel.ToString()), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));
        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("TRANSFERABLE"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.transferable.ToString()), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));
        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("TRANSFER FEE SETTINGS"), skin.GetStyle("Subtitle"), GUILayout.Width(260));

        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.transferFeeSettings.type.ToString()), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));

        EditorGUILayout.EndVertical();
        GUILayout.Space(20);
        EditorGUILayout.BeginVertical(GUILayout.Width(260));
        EditorGUILayout.LabelField(new GUIContent("CREATOR MELT FEE %", "Sets the max melt fee. Max value controller.Is 50"), skin.GetStyle("Subtitle"), GUILayout.Width(260)); // was 120

        EditorGUILayout.LabelField(new GUIContent((controller.CurrentCryptoItem.meltFeeRatio * 0.01f).ToString() + "%"), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));

        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("NONFUNGIBLE ITEM", "Flags the item as nonfungible"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.nonFungible.ToString()), skin.GetStyle("LargeTextDark"), GUILayout.Width(260));

        if (controller.CurrentCryptoItem.nonFungible)
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField(new GUIContent("INDEX", "The particular index for thcontroller.Is non-fungible item"), skin.GetStyle("Subtitle"), GUILayout.Width(260));

            int tokenIndex;
            if (System.Int32.TryParse(controller.CurrentCryptoItem.index, out tokenIndex))
            {
                EditorGUILayout.LabelField(new GUIContent("" + tokenIndex), skin.GetStyle("LargeTextDark"), GUILayout.Width(260));
            }
            else if (controller.CurrentCryptoItem.index.Contains(",") && controller.CurrentCryptoItem.index.Split(',').Length > 0)
            {
                string indexList = "";
                string[] indices = controller.CurrentCryptoItem.index.Split(',');

                for (int indexIndex = 0; indexIndex < indices.Length; indexIndex++)
                {
                    int subItemIndex;
                    if (System.Int32.TryParse(indices[indexIndex], out subItemIndex))
                    {
                        if (indexIndex > 0 && indexIndex < indices.Length)
                            indexList += ", ";

                        indexList += subItemIndex;
                    }
                }

                skin.GetStyle("LargeTextDark").wordWrap = true;
                EditorGUILayout.LabelField(new GUIContent(indexList), skin.GetStyle("LargeTextDark"), GUILayout.Width(260));
                skin.GetStyle("LargeTextDark").wordWrap = false;
            }
            else
            {
                EditorGUILayout.LabelField(new GUIContent("Invalid index"), skin.GetStyle("LargeTextDark"), GUILayout.Width(260));
            }
        }

        if (controller.CurrentCryptoItem.transferFeeSettings.type != TransferType.NONE)
        {
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("TOKEN ID"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(controller.CurrentCryptoItem.token_id, skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (controller.CurrentCryptoItem.transferFeeSettings.type == TransferType.RATIO_CUT || controller.CurrentCryptoItem.transferFeeSettings.type == TransferType.RATIO_EXTRA)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("TRANSFER FEE %"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField((float.Parse(controller.CurrentCryptoItem.transferFeeSettings.value) / Mathf.Pow(10,2)) + "%", skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));
                EditorGUILayout.EndHorizontal();
            }
            else if(controller.CurrentCryptoItem.transferFeeSettings.type == TransferType.PER_CRYPTO_ITEM || controller.CurrentCryptoItem.transferFeeSettings.type == TransferType.PER_TRANSFER)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("TRANSFER FEE (ENJ)"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField((float.Parse(controller.CurrentCryptoItem.transferFeeSettings.value) / Mathf.Pow(10,18)).ToString(), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.BeginVertical();
        GUILayout.Space(10);
        EditorGUILayout.LabelField(new GUIContent("METADATA URI"), skin.GetStyle("Subtitle"), GUILayout.Width(120));
        if (controller.CurrentCryptoItem.isCreator && controller.State.Equals(CryptoItemsController.CryptoItemState.EDIT))
            controller.MetaDataURI = EditorGUILayout.TextField(controller.MetaDataURI, skin.textField, GUILayout.Height(30), GUILayout.Width(820));
        else
            EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.itemURI), skin.GetStyle("LargeTextDark"), GUILayout.Height(30), GUILayout.Width(260));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (controller.CurrentCryptoItem.isCreator && controller.State.Equals(CryptoItemsController.CryptoItemState.EDIT))
        {
            if (GUILayout.Button(new GUIContent("UPDATE"), GUILayout.Width(100), GUILayout.Height(30)))
            {
                if (!controller.IsValidURI(controller.MetaDataURI))
                {
                    EditorUtility.DisplayDialog("INVALID METADATA URI", "Your metadata URI must be valid in a web browser", "Ok");
                }
                else
                {
                    controller.Properties.Clear();
                    controller.Properties.Add("ItemName", controller.EditItemName);
                    controller.Properties.Add("MetaDataURI", controller.MetaDataURI);
                    controller.Properties.Add("TranferFeeEnj", controller.TransferFeeEnj);
                    controller.Properties.Add("MeltFeeRatio", controller.MeltFee);
                    controller.ProcessCryptoItem(ProcessTasks.EDIT, controller.CurrentCryptoItem, controller.Properties);

                    if (Enjin.ServerResponse == ResponseCodes.SUCCESS)
                        EditorUtility.DisplayDialog("SUCCESS", "The request has posted successfully. Please see your wallet to complete the transaction!", "Ok");
                    else
                        EditorUtility.DisplayDialog("FAILURE", "The request could not be processed.", "Ok");

                    controller.State = CryptoItemsController.CryptoItemState.MAIN;
                }
            }
        }

        if (GUILayout.Button(new GUIContent("BACK"), GUILayout.Width(100), GUILayout.Height(30)))
        {
            controller.Reset();
        }

        GUILayout.Space(6);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.EndArea();
    }
}