using UnityEditor;
using UnityEngine;
using EnjinSDK;
using EnjinEditorPanel;

public class HT_LoggedInPanel
{
    #region Definitions & Properties
    // Private variables & objects
    private HT_ViewPanel _viewPanel;
    private HT_ManageAppPanel _appManagePanel;
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public HT_LoggedInPanel()
    {
        _viewPanel = new HT_ViewPanel();
        _appManagePanel = new HT_ManageAppPanel();        
    }

    /// <summary>
    /// Draws the logged in panel
    /// </summary>
    public void DrawPlatformLoggedInPanel(HomeController controller, GUISkin skin)
    {
        if (Enjin.AppID != -1 && controller.State == PlatformState.VIEW)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorStyles.popup.fixedHeight = 30;
            EditorStyles.popup.fontSize = 12;
            EnjinEditor.SelectedAppIndex = EditorGUILayout.Popup(EnjinEditor.SelectedAppIndex, EnjinEditor.AppsNameList.ToArray(), GUILayout.Width(220), GUILayout.Height(30));
            EditorStyles.popup.fixedHeight = 15;
            EditorStyles.popup.fontSize = 11;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.CHECKAPPCHANGE);

        GUILayout.Space(30);

        if (controller.State == PlatformState.VIEW)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("ADD APP", GUILayout.Width(102), GUILayout.Height(30)))
                controller.SetPlatformState(PlatformState.CREATE);

            if (controller.HasPermission(UserPermission.manageApp))
            {
                GUILayout.Space(10);

                if (GUILayout.Button("EDIT APP", GUILayout.Width(102), GUILayout.Height(30)))
                    controller.SetPlatformState(PlatformState.EDIT);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        switch (controller.State)
        {
            case PlatformState.CREATE:
            case PlatformState.EDIT:
                _appManagePanel.DrawManageAppsPanel(controller, skin);
                break;

            case PlatformState.VIEW:
                _viewPanel.DrawViewPanel(controller, skin);
                break;
        }
    }
}