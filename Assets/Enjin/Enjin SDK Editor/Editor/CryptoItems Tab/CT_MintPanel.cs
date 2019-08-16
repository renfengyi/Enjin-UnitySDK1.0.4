using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;

public class CT_MintPanel
{
    /// <summary>
    /// Build and Draw Mint CI Panel
    /// Embedded UI logic for field validation and initial request return status
    /// </summary>
    public void DrawPane(CryptoItemsController controller, GUISkin skin)
    {
        controller.TotalReserveCost = System.Convert.ToInt64(controller.CurrentCryptoItem.reserve);
        controller.TotalReserveCost = controller.TotalReserveCost * (double)((float)System.Convert.ToDecimal(controller.CurrentCryptoItem.meltValue) / Mathf.Pow(10, 18));

        GUILayout.BeginArea(new Rect(5, 126, 912, 490), skin.GetStyle("TopBackground"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("MINT CRYPTOITEM"), skin.GetStyle("MainTitle"));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(30);
        // Begin Columns
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        #region Column 1
        EditorGUILayout.BeginVertical(GUILayout.Width(260));
        EditorGUILayout.LabelField(new GUIContent("CRYPTOITEM NAME"), skin.GetStyle("Subtitle"));
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.name), skin.GetStyle("LargeTextDark"), GUILayout.Height(30));
        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("TOTAL SUPPLY"), skin.GetStyle("Subtitle"), GUILayout.Width(260));

        if (controller.CurrentCryptoItem.supplyModel != SupplyModel.INFINITE)
            EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.totalSupply), skin.GetStyle("LargeTextDark"), GUILayout.Height(30), GUILayout.Width(260));
        else
            EditorGUILayout.LabelField(new GUIContent("INFINITE"), skin.GetStyle("LargeTextDark"), GUILayout.Height(30), GUILayout.Width(260));

        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("TOTAL RESERVE"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.reserve), skin.GetStyle("LargeTextDark"), GUILayout.Height(30), GUILayout.Width(260));
        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("MINT ALLOWANCE"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        EditorGUILayout.LabelField(new GUIContent(controller.MintableCryptoItems.ToString()), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));
        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("NUMBER TO MINT"), skin.GetStyle("Subtitle"), GUILayout.Width(260));

        if (controller.CurrentCryptoItem.nonFungible)
        {
            controller.NumToMint = 1;
            EditorGUILayout.LabelField(new GUIContent(controller.NumToMint.ToString()), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));
        }
        else
        {
            controller.NumToMint = (controller.NumToMint < 1) ? 1 : controller.NumToMint;
            controller.NumToMint = (controller.NumToMint > int.MaxValue) ? int.MaxValue : controller.NumToMint;
            controller.NumToMint = EditorGUILayout.IntField(controller.NumToMint, skin.textField, GUILayout.Width(260), GUILayout.Height(30));
        }

        EditorGUILayout.EndVertical();
        #endregion

        #region Column 2
        EditorGUILayout.BeginVertical(GUILayout.Width(600)); // double wide
        EditorGUILayout.LabelField(new GUIContent("CRYPTOITEM ID"), skin.GetStyle("Subtitle"));
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.token_id, controller.CurrentCryptoItem.token_id), skin.GetStyle("LargeTextDark"), GUILayout.Height(30));
        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("RESERVE BALANCE"), skin.GetStyle("Subtitle"));
        EditorGUILayout.LabelField(new GUIContent(controller.TotalReserveCost.ToString("N4") + " ENJ"), skin.GetStyle("LargeTextDark"), GUILayout.Height(30));
        GUILayout.Space(20);

        if (controller.NumToMint <= controller.Reserve)
        {
            controller.TotalReserveCost = controller.NumToMint * controller.MeltValue2;
            controller.TotalEnjCost = 0;
        }
        else if (controller.NumToMint > controller.Reserve && controller.Reserve == 0)
        {
            controller.TotalReserveCost = 0;
            controller.TotalEnjCost = controller.NumToMint * controller.MeltValue2;
        }
        else if (controller.NumToMint > controller.Reserve && controller.Reserve > 0)
        {
            controller.TotalReserveCost = controller.Reserve * controller.MeltValue2;
            controller.TotalEnjCost = (controller.NumToMint - controller.Reserve) * (double)((float)System.Convert.ToDecimal(controller.CurrentCryptoItem.meltValue) / Mathf.Pow(10, 18));
        }

        EditorGUILayout.LabelField(new GUIContent("RESERVE ENJ COST", "ENJ PER ITEM: " + controller.MeltValue2), skin.GetStyle("Subtitle"));
        EditorGUILayout.LabelField(new GUIContent(controller.TotalReserveCost.ToString("N4") + " ENJ"), skin.GetStyle("LargeTextDark"), GUILayout.Height(30));
        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("TOTAL ENJ COST", "ENJ PER ITEM: " + controller.MeltValue2), skin.GetStyle("Subtitle"));
        EditorGUILayout.LabelField(new GUIContent(controller.TotalEnjCost.ToString("N4") + " ENJ"), skin.GetStyle("LargeTextDark"), GUILayout.Height(30));
        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("TRANSFER TO ID"), skin.GetStyle("Subtitle"));
        controller.RecieverAddress[0] = EditorGUILayout.TextField(controller.RecieverAddress[0], skin.textField, GUILayout.Height(30));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        #endregion
        // End Columns

        // Buttons
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("MINT", GUILayout.Width(100), GUILayout.Height(30)))
        {
            if (!Enjin.ValidateAddress(controller.RecieverAddress[0].Trim()))
            {
                EditorUtility.DisplayDialog("INVALID ADDRESS", "The address you entered is not valid. Please enter a valid address", "Ok");
            }
            else if (controller.CurrentCryptoItem.supplyModel != SupplyModel.INFINITE)
            {
                if (controller.NumToMint > System.Int32.Parse(controller.MintableCryptoItems))
                    EditorUtility.DisplayDialog("INVALID MINT VALUE", "The number of items to mint can not exceed the mint allowance", "Ok");
            }

            if (controller.NumToMint <= int.MaxValue)
            {
                controller.Properties.Clear();
                controller.Properties.Add("RecieverAddress", controller.RecieverAddress);

                if (!controller.CurrentCryptoItem.nonFungible)
                    controller.Properties.Add("NumToMint", controller.NumToMint);

                Request request = controller.ProcessCryptoItem(ProcessTasks.MINT, controller.CurrentCryptoItem, controller.Properties);

                if (EnjinEditor.IsRequestSuccessfull(request.state))
                    EditorUtility.DisplayDialog("SUCCESS", "The request has posted with a status of " + request.state + ". Please see your wallet to complete the transaction!", "Ok");
                else
                    EditorUtility.DisplayDialog("FAILURE", "The request could not be processed due to a status of " + request.state + ".", "Ok");

                controller.State = CryptoItemsController.CryptoItemState.MAIN;
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid Mintable CryptoItem Amount", "The token amount you selected is greater than the maximum limit. The maximum limit is " + int.MaxValue.ToString(), "Ok");
            }
        }

        if (GUILayout.Button("BACK", GUILayout.Width(100), GUILayout.Height(30)))
            controller.State = CryptoItemsController.CryptoItemState.MAIN;

        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.EndArea();
    }
}