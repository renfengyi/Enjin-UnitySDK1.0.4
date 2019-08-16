using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;

public class CT_MeltPanel
{
    /// <summary>
    /// Build and Draw Melt CI Panel
    /// Embedded UI logic for field validation and initial request return status
    /// </summary>
    public void DrawPane(CryptoItemsController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 126, 912, 490), skin.GetStyle("TopBackground"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("MELT CRYPTOITEM"), skin.GetStyle("MainTitle"));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal(GUILayout.Width(280));
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(new GUIContent("CRYPTOITEM ID"), skin.GetStyle("Subtitle"));
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.token_id.Substring(0, 8) + "..", controller.CurrentCryptoItem.token_id), skin.GetStyle("LargeTextDark"), GUILayout.Height(30), GUILayout.Width(30));
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        GUILayout.Label(controller.CurrentCryptoItem.iconTexture, GUILayout.Width(60), GUILayout.Height(60));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField(new GUIContent("CRYPTOITEM NAME"), skin.GetStyle("Subtitle"));
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.name), skin.GetStyle("LargeTextDark"), GUILayout.Height(30));
        GUILayout.Space(10);
        EditorGUILayout.LabelField(new GUIContent("AVAILABLE COUNT"), skin.GetStyle("Subtitle"));
        EditorGUILayout.LabelField(new GUIContent(controller.Balance.ToString()), skin.GetStyle("LargeTextDark"), GUILayout.Height(30));
        GUILayout.Space(10);

        if (!controller.CurrentCryptoItem.nonFungible)
        {
            EditorGUILayout.LabelField(new GUIContent("NUMBER TO MELT"), skin.GetStyle("Subtitle"));
            controller.NumToMelt = EditorGUILayout.IntField(controller.NumToMelt, skin.textField, GUILayout.Width(260), GUILayout.Height(30));

            if (controller.NumToMelt > controller.Balance)
                controller.NumToMelt = controller.Balance;
        }
        else
        {
            controller.NumToMelt = 1;
            EditorGUILayout.LabelField(new GUIContent("NUMBER TO MELT"), skin.GetStyle("Subtitle"));
            EditorGUILayout.LabelField(new GUIContent(controller.NumToMelt.ToString()), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));
        }

        if (controller.NumToMelt == 0)
            controller.NumToMelt = 1;

        GUILayout.Space(10);
        EditorGUILayout.LabelField(new GUIContent("CREATOR MELT FEE"), skin.GetStyle("Subtitle"));
        EditorGUILayout.LabelField(new GUIContent(controller.EnjPerItem.ToString("N4") + " ENJ"), skin.GetStyle("LargeTextDark"), GUILayout.Height(30));
        GUILayout.Space(10);
        EditorGUILayout.LabelField(new GUIContent("ENJ RETURNED"), skin.GetStyle("Subtitle"));
        controller.EnjReturned = (controller.NumToMelt * controller.EnjPerItem);
        EditorGUILayout.LabelField(new GUIContent(controller.EnjReturned.ToString("N4")), skin.GetStyle("LargeTextDark"), GUILayout.Height(30));
        GUILayout.Space(10);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("MELT", GUILayout.Width(100), GUILayout.Height(30)))
        {
            if (controller.NumToMelt == 0)
                EditorUtility.DisplayDialog("INVALID MELT VALUE", "Melt value must be at least one.", "Ok");
            else
            {
                controller.Properties.Clear();
                controller.Properties.Add("NumToMelt", controller.NumToMelt);

                Request request = controller.ProcessCryptoItem(ProcessTasks.MELT, controller.CurrentCryptoItem, controller.Properties);

                if (EnjinEditor.IsRequestSuccessfull(request.state))
                    EditorUtility.DisplayDialog("SUCCESS", "The request has posted with a status of " + request.state + ". Please see your wallet to complete the transaction!", "Ok");
                else
                    EditorUtility.DisplayDialog("FAILURE", "The request could not be processed due to a status of " + request.state + ".", "Ok");

                EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADITEMS);
                controller.State = CryptoItemsController.CryptoItemState.MAIN;
            }
        }
        GUILayout.Space(16);

        if (GUILayout.Button("BACK", GUILayout.Width(100), GUILayout.Height(30)))
            controller.State = CryptoItemsController.CryptoItemState.MAIN;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();

    }
}
