using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;
using System;

public class CT_MintCostPane
{
    /// <summary>
    /// Minimum Cost Pane UI Builder
    /// </summary>
    public void DrawPane(CryptoItemsController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(368, 10, 550, 100), skin.GetStyle("TopBackground"));
        EditorGUILayout.LabelField(new GUIContent("CREATE COST"), skin.GetStyle("MainTitle"));
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.LabelField(new GUIContent("ENJ PER ITEM"), skin.GetStyle("ContentDark"), GUILayout.Width(100));
        EditorGUILayout.LabelField(new GUIContent("ITEM COUNT"), skin.GetStyle("ContentDark"), GUILayout.Width(100));
        EditorGUILayout.LabelField(new GUIContent("TOTAL ENJ COST"), skin.GetStyle("ContentDark"), GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        if (controller.MeltValue == 0)
            EditorGUILayout.LabelField(new GUIContent((controller.MinWei / 1000000000000000000).ToString("N7") + " ENJ"), skin.GetStyle("ContentLight"), GUILayout.Width(168));
        else
            EditorGUILayout.LabelField(new GUIContent(controller.MeltValue.ToString("N7") + " ENJ"), skin.GetStyle("ContentLight"), GUILayout.Width(168));

        EditorGUILayout.LabelField(new GUIContent(controller.InitialItems.ToString()), skin.GetStyle("ContentLight"), GUILayout.Width(168));
        skin.GetStyle("ContentLight").fontSize = 12;

        if (controller.MeltValue == 0)
            EditorGUILayout.LabelField(new GUIContent(((controller.MinWei / 1000000000000000000) * controller.InitialItems).ToString("N7") + " ENJ"), skin.GetStyle("ContentGreen"), GUILayout.Width(120));
        else
            EditorGUILayout.LabelField(new GUIContent((controller.MeltValue * controller.InitialItems).ToString("N7") + " ENJ"), skin.GetStyle("ContentGreen"), GUILayout.Width(120));

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}