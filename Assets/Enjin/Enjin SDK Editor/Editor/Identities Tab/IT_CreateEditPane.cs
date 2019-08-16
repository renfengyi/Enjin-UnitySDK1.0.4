using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;
using System.Collections.Generic;

public class IT_CreateEditPane
{
    #region Declairations & Properties
    // Private variables & Objects
    private Vector2 _scrollPos;
    private Dictionary<string, object> _properties;
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
	public IT_CreateEditPane()
    {
        _scrollPos = Vector2.zero;
        _properties = new Dictionary<string, object>();
    }

    public void DrawCreateEditPane(IdentitiesTabController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 10, 912, 606), skin.GetStyle("TopBackground"));
        EditorGUILayout.BeginHorizontal();

        if (controller.State == IdentitiesTabController.IdentityState.EDIT)
            EditorGUILayout.LabelField(new GUIContent("VIEW IDENTITY"), skin.GetStyle("MainTitle"));
        else
            EditorGUILayout.LabelField(new GUIContent(controller.State.ToString() + " IDENTITY"), skin.GetStyle("MainTitle"));

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(30);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);
        EditorGUILayout.LabelField(new GUIContent("CURRENT APPLICATION"), skin.GetStyle("Subtitle"));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);
        EditorGUILayout.LabelField(new GUIContent("ID: " + Enjin.AppID.ToString() + " -> " + EnjinEditor.AppsNameList[EnjinEditor.SelectedAppIndex]), skin.GetStyle("LargeTextDark"));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);
        EditorGUILayout.LabelField(new GUIContent("USER ID"), skin.GetStyle("Subtitle"));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);

        if (controller.State == IdentitiesTabController.IdentityState.CREATE)
        {
            EditorStyles.popup.fixedHeight = 30;
            EditorStyles.popup.fontSize = 12;
            controller.CurrentIdentity.user.id = System.Convert.ToInt32(EditorGUILayout.Popup(controller.CurrentIdentity.user.id, controller.UserIDs.ToArray(), GUILayout.Width(220), GUILayout.Height(30)));
            EditorStyles.popup.fixedHeight = 15;
            EditorStyles.popup.fontSize = 11;
        }
        else
            EditorGUILayout.LabelField(new GUIContent(controller.CurrentIdentity.user.id.ToString()), skin.GetStyle("LargeText"), GUILayout.Width(320), GUILayout.Height(30));

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);

        if (controller.State == IdentitiesTabController.IdentityState.CREATE)
        {
            EditorGUILayout.LabelField(new GUIContent("ETHEREUM ADDRESS"), skin.GetStyle("Subtitle"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(14);

            controller.CurrentIdentity.ethereum_address = EditorGUILayout.TextField(controller.CurrentIdentity.ethereum_address, skin.textField, GUILayout.Width(320), GUILayout.Height(30));
        }
        else
        {
            if (controller.CurrentIdentity.linking_code == "")
            {
                EditorGUILayout.LabelField(new GUIContent("ETHEREUM ADDRESS"), skin.GetStyle("Subtitle"));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(14);

                EditorGUILayout.LabelField(new GUIContent(controller.CurrentIdentity.ethereum_address), skin.GetStyle("LargeText"), GUILayout.Width(320), GUILayout.Height(30));
            }
            else
            {
                EditorGUILayout.LabelField(new GUIContent("LINKING CODE"), skin.GetStyle("Subtitle"));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(14);

                EditorGUILayout.LabelField(new GUIContent(controller.CurrentIdentity.linking_code), skin.GetStyle("LargeText"), GUILayout.Width(320), GUILayout.Height(30));
            }
        }

        EditorGUILayout.EndHorizontal();


        if (controller.State != IdentitiesTabController.IdentityState.CREATE && controller.CurrentIdentity.linking_code == "")
        {

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(14);
            EditorGUILayout.LabelField(new GUIContent("To change your wallet addres you will need to unlink this wallet."), skin.GetStyle("ContentDark"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(14);

            if (GUILayout.Button(new GUIContent("Unlink Wallet"), GUILayout.Width(120), GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Unlink Wallet", "This will unlink this editor from your developer wallet. Do you want to proceed?", "Accept", "Cancel"))
                {
                    Enjin.UnLinkIdentity(controller.CurrentIdentity.id);
                    EnjinEditor.CurrentUser = Enjin.GetUserRaw(controller.CurrentIdentity.id);
                    EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADALL);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        /***
         * Disabled for V1
         */
        //GUILayout.Space(20);
        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField(new GUIContent("FIELDS"), skin.GetStyle("MainTitle"));
        //EditorGUILayout.EndHorizontal();
        //GUILayout.Space(20);
        //EditorGUILayout.BeginHorizontal();
        //GUILayout.Space(14);

        ///* TODO POST V1
        // *      - Make fields additive so user can add fields as nessisary
        // */

        //if (controller.CurrentIdentity.fields == null || controller.CurrentIdentity.fields.Length == 0)
        //{
        //    controller.CurrentIdentity.fields = new Fields[2];
        //    controller.CurrentIdentity.fields = Enjin.DefaultFields;
        //}

        //_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(320), GUILayout.Height(140));

        //for (int i = 0; i < controller.CurrentIdentity.fields.Length; i++)
        //{
        //    EditorGUILayout.BeginVertical("helpBox");
        //    EditorGUILayout.BeginHorizontal();
        //    GUILayout.Space(14);
        //    EditorGUILayout.LabelField(new GUIContent("KEY"), skin.GetStyle("Subtitle"), GUILayout.Width(140));
        //    EditorGUILayout.LabelField(new GUIContent("VALUE"), skin.GetStyle("Subtitle"), GUILayout.Width(140));
        //    EditorGUILayout.EndHorizontal();
        //    EditorGUILayout.BeginHorizontal();
        //    GUILayout.Space(14);
        //    EditorGUILayout.LabelField(new GUIContent(controller.CurrentIdentity.fields[i].key), GUILayout.Width(140), GUILayout.Height(30));
        //    controller.CurrentIdentity.fields[i].value = EditorGUILayout.TextField(controller.CurrentIdentity.fields[i].value, skin.textField, GUILayout.Width(140), GUILayout.Height(30));
        //    EditorGUILayout.EndHorizontal();
        //    GUILayout.Space(8);
        //    EditorGUILayout.EndVertical();
        //}

        //EditorGUILayout.EndScrollView();
        //EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (controller.State == IdentitiesTabController.IdentityState.CREATE)
        {
            if (GUILayout.Button(new GUIContent("CREATE"), GUILayout.Width(100), GUILayout.Height(30)))
            {
                controller.ProcessRequest(ProcessTasks.CREATE, controller.CurrentIdentity, _properties);

                //if (!Enjin.ValidateAddress(controller.CurrentIdentity.ethereum_address))
                //    EditorUtility.DisplayDialog("INVALID ADDRESS", "The address you entered is not valid. Please enter a valid address", "Ok");
                //else
                //{
                //    controller.ProcessRequest(ProcessTasks.CREATE, controller.CurrentIdentity, _properties);
                //}
            }
        }

        if (GUILayout.Button(new GUIContent("BACK"), GUILayout.Width(100), GUILayout.Height(30)))
        {
            controller.CurrentIdentity.fields = new Fields[0];
            controller.State = IdentitiesTabController.IdentityState.VIEW;
        }

        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.EndArea();
    }
}
