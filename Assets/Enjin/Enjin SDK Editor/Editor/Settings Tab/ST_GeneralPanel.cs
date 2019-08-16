using UnityEngine;
using UnityEditor;
using EnjinEditorPanel;

public class ST_GeneralPanel
{
    public void DrawGeneralSettingsPanel(GUISkin skin, SettingsController controller)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(880));
        controller.GeneralFoldout = EditorGUILayout.Foldout(controller.GeneralFoldout, new GUIContent("GENERAL SETTINGS"));

        if (controller.GeneralFoldout)
        {
            #region General Settings Body
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(150));

            #region General Settings Column 1
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            EditorGUILayout.LabelField(new GUIContent("RESET USER CACHE"), GUILayout.Width(140));

            if (GUILayout.Button(new GUIContent("Reset"), GUILayout.Width(120), GUILayout.Height(20)))
                PlayerPrefs.DeleteKey("UserData");

            EditorGUILayout.EndVertical();
            #endregion

            #region General Settings Column 2
            //EditorGUILayout.BeginVertical(GUILayout.Width(150));
            //EditorGUILayout.LabelField(new GUIContent("ITEMS PER PAGE"), GUILayout.Width(140));
            //controller.UserSettings.ItemsPerPage = EditorGUILayout.IntField(controller.UserSettings.ItemsPerPage, skin.textField, GUILayout.Width(120), GUILayout.Height(30));

            //if (GUILayout.Button(new GUIContent("Update", "Updates the items per page and saves as default setting"), GUILayout.Width(120), GUILayout.Height(20)))
            //{
            //    controller.UserSettings.Update();
            //    EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.UPDATEITEMSPERPAGE);
            //}

            //EditorGUILayout.EndVertical();
            #endregion

            #region General Settings Column 3
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            EditorGUILayout.LabelField(new GUIContent("APPROVE ALLOWANCE"), GUILayout.Width(140));

            if (GUILayout.Button(new GUIContent("Approve", "Sets approval allowance to max"), GUILayout.Width(120), GUILayout.Height(20)))
            {
                if (controller.IsAllowanceApproved)
                {
                    EnjinEditor.DisplayDialog("ERROR", "Approval has already been set");
                }
                else
                {
                    if (controller.AllowanceError == SettingsController.AllowanceErrors.NOTLINKED)
                        EnjinEditor.DisplayDialog("ERROR", "Wallet must be linked prior to approving the allowance");
                    else if (controller.AllowanceError == SettingsController.AllowanceErrors.INVALIDADDRESS)
                        EnjinEditor.DisplayDialog("ERROR", "The eth address you're trying to approve doesn't exists");
                    else
                    {
                        EnjinSDK.Enjin.SetAllowance(EnjinEditor.CurrentUserIdentity.id);
                        EnjinEditor.DisplayDialog("INFO", "The approval request has been sent. Approve this in the wallet then press ok.");
                        controller.UpdateAllowance();
                    }
                }

                EditorGUILayout.EndVertical();
            }
            #endregion

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            #endregion
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
}