using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;

public class CT_WalletPane
{
    /// <summary>
    /// Build and draw Wallet pane (main CI tab)
    /// </summary>
    public void DrawPane(CryptoItemsController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 10, 350, 100), skin.GetStyle("TopBackground"));
        EditorGUILayout.LabelField(new GUIContent("WALLET BALANCE"), skin.GetStyle("MainTitle"));
        GUILayout.Space(10);
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.LabelField(new GUIContent("Wallet Balance:"), skin.GetStyle("ContentDark"), GUILayout.Width(94));
        EditorGUILayout.LabelField(new GUIContent(Enjin.GetEnjBalance.ToString("#,##0.###") + " ENJ"), skin.GetStyle("ContentLight"), GUILayout.Width(140));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.LabelField(new GUIContent(EnjinEditor.CurrentUserIdentity.ethereum_address), skin.GetStyle("ContentLight"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }
}
