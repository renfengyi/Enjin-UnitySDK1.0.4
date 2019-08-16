using UnityEngine;
using UnityEditor;
using EnjinSDK;

public class IT_OptionsPanel
{
    public void DrawOptionsPanel(IdentitiesTabController controller, GUISkin skin)
    {
        #region Identity Options Panel
        GUILayout.BeginArea(new Rect(5, 10, 230, 100), skin.GetStyle("TopBackground"));
        EditorGUILayout.LabelField(new GUIContent("ADD IDENTITY"), skin.GetStyle("MainTitle"));
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
        GUILayout.Space(14);

        //if (GUILayout.Button(new GUIContent("Create Identity"), GUILayout.Height(36), GUILayout.Width(100)))
        //{
        //    controller.CurrentIdentity = new Identity();
        //    controller.SetUserIDList();
        //    controller.State = IdentitiesTabController.IdentityState.CREATE;
        //}

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
        #endregion
    }
}
