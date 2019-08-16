using UnityEngine;
using UnityEditor;
using EnjinEditorPanel;

public class CT_OptionsPane
{
    /// <summary>
    /// Options Pane for main CI tab
    /// </summary>
    public void DrawPane(CryptoItemsController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 10, 230, 100), skin.GetStyle("TopBackground"));
        EditorGUILayout.LabelField(new GUIContent("CREATE"), skin.GetStyle("MainTitle"));

        if (EnjinEditor.HasPermission(EnjinSDK.UserPermission.manageTokens))
        {
            if (EnjinEditor.CurrentUserIdentity.linking_code == string.Empty)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
                GUILayout.Space(14);

                if (GUILayout.Button(new GUIContent("CREATE"), GUILayout.Height(36), GUILayout.Width(100)))
                    controller.State = CryptoItemsController.CryptoItemState.CREATE;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }
        }

        GUILayout.EndArea();
    }
}