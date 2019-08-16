using UnityEngine;
using UnityEditor;
using EnjinEditorPanel;

public class CT_ViewPanel
{
    /// <summary>
    /// Build and Draw View CI Panel (Deprecated)
    /// </summary>
    public void DrawPane(CryptoItemsController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 126, 912, 490), skin.GetStyle("TopBackground"));
        EditorGUILayout.LabelField(new GUIContent("VIEW CRYPTOITEM"), skin.GetStyle("MainTitle"));
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(8);
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField(new GUIContent("CRYPTOITEM NAME"), skin.GetStyle("Subtitle"), GUILayout.Width(260));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(controller.CurrentCryptoItem.name, skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));

        EditorGUILayout.LabelField(controller.CurrentCryptoItem.token_id, skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));

        if (GUILayout.Button(new GUIContent(controller.ClipBoard, "Copy ID to clipboard"), GUILayout.Width(32), GUILayout.Height(32)))
            EditorGUIUtility.systemCopyBuffer = controller.CurrentCryptoItem.token_id;

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.LabelField(new GUIContent("ITEM TYPE"), skin.GetStyle("Subtitle"), GUILayout.Width(260));

        if (controller.CurrentCryptoItem.nonFungible)
            EditorGUILayout.LabelField(new GUIContent("Non-fungible"), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));
        else
            EditorGUILayout.LabelField(new GUIContent("Fungible"), skin.GetStyle("LargeTextDark"), GUILayout.Width(260), GUILayout.Height(30));

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("TOTAL SUPPLY"), skin.GetStyle("Subtitle"), GUILayout.Width(200));
        EditorGUILayout.LabelField(new GUIContent("TOTAL RESERVE"), skin.GetStyle("Subtitle"), GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        if (controller.CurrentCryptoItem.supplyModel != EnjinSDK.SupplyModel.INFINITE)
            EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.totalSupply), skin.GetStyle("LargeTextDark"), GUILayout.Width(200), GUILayout.Height(30));
        else
            EditorGUILayout.LabelField(new GUIContent("INFINITE"), skin.GetStyle("LargeTextDark"), GUILayout.Width(200), GUILayout.Height(30));

        EditorGUILayout.LabelField(controller.CurrentCryptoItem.reserve, skin.GetStyle("LargeTextDark"), GUILayout.Width(200), GUILayout.Height(30));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("TRANSFERABLE"), skin.GetStyle("Subtitle"), GUILayout.Width(200));
        EditorGUILayout.LabelField(new GUIContent("SUPPLY TYPE"), skin.GetStyle("Subtitle"), GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.transferable.ToString()), skin.GetStyle("LargeTextDark"), GUILayout.Width(200), GUILayout.Height(30));
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.supplyModel.ToString()), skin.GetStyle("LargeTextDark"), GUILayout.Width(200), GUILayout.Height(30));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("CREATOR MELT FEE %"), skin.GetStyle("Subtitle"), GUILayout.Width(200));
        EditorGUILayout.LabelField(new GUIContent("ENJ PER ITEM"), skin.GetStyle("Subtitle"), GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent((controller.CurrentCryptoItem.meltFeeRatio * 0.01f).ToString() + "%"), skin.GetStyle("LargeTextDark"), GUILayout.Width(200), GUILayout.Height(30));
        EditorGUILayout.LabelField(new GUIContent((System.Convert.ToDouble(controller.CurrentCryptoItem.meltValue) / Mathf.Pow(10, 18)).ToString("N4") + " ENJ"), skin.GetStyle("LargeTextDark"), GUILayout.Width(200), GUILayout.Height(30));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("CIRCULATING SUPPLY"), skin.GetStyle("Subtitle"), GUILayout.Width(200));
        EditorGUILayout.LabelField(new GUIContent("ITEM URI"), skin.GetStyle("Subtitle"), GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.circulatingSupply), skin.GetStyle("LargeTextDark"), GUILayout.Width(200), GUILayout.Height(30));
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.itemURI), skin.GetStyle("LargeTextDark"), GUILayout.Width(200), GUILayout.Height(30));
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(250);

        if (GUILayout.Button("BACK", GUILayout.Width(80), GUILayout.Height(30)))
            controller.State = CryptoItemsController.CryptoItemState.MAIN;

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}