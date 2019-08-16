using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;

public class CT_DetailsPane
{
    /// <summary>
    /// Build and draw details pane (main CI tab)
    /// </summary>
    public void DrawPane(CryptoItemsController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(368, 10, 550, 100), skin.GetStyle("TopBackground"));
        EditorGUILayout.BeginHorizontal(); // header
        EditorGUILayout.BeginVertical(); // header
        EditorGUILayout.LabelField(new GUIContent("CRYPTOITEM DETAILS"), skin.GetStyle("MainTitle"), GUILayout.Width(140));
        EditorGUILayout.EndVertical(); // end header
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical(); // space?
        EditorGUILayout.EndVertical(); // end space?
        EditorGUILayout.EndHorizontal(); // end header

        GUILayout.Space(30); // hoz. space

        EditorGUILayout.BeginHorizontal(); // detail group

        GUILayout.Space(16); // margin
        EditorGUILayout.BeginVertical(GUILayout.Width(32)); // icon 
        GUILayout.Label(controller.CurrentCryptoItem.iconTexture, GUILayout.Width(32), GUILayout.Height(32));
        EditorGUILayout.EndVertical(); // end icon

        GUILayout.Space(10); // margin

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(new GUIContent("ID"), skin.GetStyle("Subtitle"), GUILayout.Width(80));
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.token_id, controller.CurrentCryptoItem.token_id), skin.GetStyle("ContentDark"), GUILayout.Width(240));
        GUILayout.Space(2);
        if (GUILayout.Button(new GUIContent(controller.ClipBoard, "Copy ID to clipboard"), GUILayout.Width(30), GUILayout.Height(18)))
            EditorGUIUtility.systemCopyBuffer = controller.CurrentCryptoItem.token_id;

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(4); // hoz margin
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("CREATOR"), skin.GetStyle("Subtitle"), GUILayout.Width(80));
        EditorGUILayout.LabelField(new GUIContent(controller.CurrentCryptoItem.creator), skin.GetStyle("ContentDark"), GUILayout.Width(300));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal(); // end group

        GUILayout.EndArea();
    }
}
