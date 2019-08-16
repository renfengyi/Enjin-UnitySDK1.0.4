using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;
using System.Collections.Generic;

public class CT_SearchPane
{
    /// <summary>
    /// Build and draw search pane (main CI tab)
    /// </summary>
    public void DrawPane(CryptoItemsController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(248, 10, 670, 100), skin.GetStyle("TopBackground"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("SEARCH CRYPTOITEMS"), skin.GetStyle("MainTitle"), GUILayout.Width(190));
        EditorGUILayout.BeginVertical();
        GUILayout.Space(12);
        EditorGUILayout.LabelField(new GUIContent("Eth Address, CryptoItem ID, CryptoItem Name"), skin.GetStyle("ContentDark"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        controller.SearchText = EditorGUILayout.TextField(controller.SearchText, skin.textField, GUILayout.Height(30));
        // NOTE: Below code will be reinstated once we have a filter system in place on TP to filter by search criteria
        //EditorStyles.popup.fixedHeight = 30;
        //EditorStyles.popup.fontSize = 12;
        //_searchOptionIndex = EditorGUILayout.Popup(_searchOptionIndex, _searchOptions, GUILayout.Width(140), GUILayout.Height(30));
        //EditorStyles.popup.fixedHeight = 15;
        //EditorStyles.popup.fontSize = 11;

        if (GUILayout.Button(new GUIContent("SEARCH"), GUILayout.Width(100), GUILayout.Height(32)))
        {
            controller.CryptoItemList.Clear();
            controller.CryptoItemList = new List<CryptoItem>(Enjin.SearchCryptoItems(controller.SearchText));
            controller.HasListRefreshed = false;
            controller.IsSearchMode = true;
            controller.SelectedIndex = 0;
        }

        if (controller.SearchText == "" && !controller.HasListRefreshed)
        {
            controller.ResetCryptoItemList();
            controller.HasListRefreshed = true;
            controller.IsSearchMode = false;
            controller.SelectedIndex = 0;
        }

        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
        GUILayout.Space(10);

        GUILayout.EndArea();

    }
}