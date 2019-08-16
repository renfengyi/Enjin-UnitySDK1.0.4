using UnityEditor;
using UnityEngine;
using EnjinSDK;

public class HT_LogininPanel
{
    /// <summary>
    /// Draws the login panel
    /// </summary>
    public void DrawPlatformLoginPanel(HomeController controller, GUISkin skin)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(new GUIContent("SELECT PLATFORM"), skin.GetStyle("Subtitle"));
        EditorStyles.popup.fixedHeight = 30;
        EditorStyles.popup.fontSize = 12;

        if (!controller.IsDevMode)
            controller.PlatformSelection = EditorGUILayout.Popup(controller.PlatformSelection, controller.PlatformType, GUILayout.Width(300), GUILayout.Height(30));
        else
            controller.PlatformSelection = EditorGUILayout.Popup(controller.PlatformSelection, controller.PlatformTypeDev, GUILayout.Width(300), GUILayout.Height(30));

        EditorStyles.popup.fixedHeight = 15;
        EditorStyles.popup.fontSize = 11;
        GUILayout.Space(20);

        if (!controller.IsDevMode)
        {
            switch (controller.PlatformSelection)
            {
                case 0:
                    controller.LoginInfo.apiurl = "https://kovan.cloud.enjin.io/";
                    break;

                case 1:
                    if (controller.IsLoginStatus(LoginState.INVALIDTPURL))
                        EditorGUILayout.LabelField(new GUIContent("PLEASE PROVIDE VALID PLATFORM URL"), skin.GetStyle("SubtitleRed"));
                    else
                        EditorGUILayout.LabelField(new GUIContent("CUSTOM PLATFORM URL"), skin.GetStyle("Subtitle"));

                    controller.LoginInfo.apiurl = EditorGUILayout.TextField(controller.LoginInfo.apiurl, skin.textField, GUILayout.Width(300), GUILayout.Height(30));
                    GUILayout.Space(20);
                    break;
            }
        }
        else
        {
            switch (controller.PlatformSelection)
            {
                case 0:
                    controller.LoginInfo.apiurl = "https://master.cloud.enjin.dev/";
                    break;

                case 1:
                    controller.LoginInfo.apiurl = "https://kovan.cloud.enjin.io/";
                    break;

                case 2:
                    controller.LoginInfo.apiurl = "https://update-smart-contracts.cloud.enjin.dev/";
                    break;

                case 3:
                    if (controller.IsLoginStatus(LoginState.INVALIDTPURL))
                        EditorGUILayout.LabelField(new GUIContent("PLEASE PROVIDE VALID PLATFORM URL"), skin.GetStyle("SubtitleRed"));
                    else
                        EditorGUILayout.LabelField(new GUIContent("CUSTOM PLATFORM URL"), skin.GetStyle("Subtitle"));

                    controller.LoginInfo.apiurl = EditorGUILayout.TextField(controller.LoginInfo.apiurl, skin.textField, GUILayout.Width(300), GUILayout.Height(30));
                    GUILayout.Space(20);
                    break;

                case 4:
                    controller.LoginInfo.apiurl = "https://cloud.enjin.io/";
                    break;

                case 5:
                    Debug.LogWarning("[WARNING] This feature is not available at this time. Reverting to Kovan Testnet");
                    controller.PlatformSelection = 0;
                    break;
            }
        }

        EditorGUILayout.LabelField(new GUIContent("EMAIL"), skin.GetStyle("Subtitle"));
        GUI.SetNextControlName("emailField");
        controller.LoginInfo.username = EditorGUILayout.TextField(controller.LoginInfo.username, skin.textField, GUILayout.Width(300), GUILayout.Height(30));
        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("PASSWORD"), skin.GetStyle("Subtitle"));
        GUI.SetNextControlName("passField");
        controller.LoginInfo.password = EditorGUILayout.PasswordField(controller.LoginInfo.password, skin.textField, GUILayout.Width(300), GUILayout.Height(30));

        if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
        {
            if (GUI.GetNameOfFocusedControl() == "emailField")
                EditorGUI.FocusTextInControl("passField");
            else if (GUI.GetNameOfFocusedControl() == "passField")
            {
                GUI.FocusControl(null);
                ProcessLogin(controller);
            }
        }

        GUILayout.Space(30);
        EditorGUILayout.BeginHorizontal(GUILayout.Width(300));

        if (GUILayout.Button(new GUIContent("SIGN UP"), GUILayout.Height(36)))
            Application.OpenURL(controller.LoginInfo.apiurl);

        GUILayout.Space(8);

        if (GUILayout.Button(new GUIContent("LOGIN"), GUILayout.Height(36)))
        {
            GUI.FocusControl(null);
            ProcessLogin(controller);
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(14);

        if (controller.IsLoginStatus(LoginState.INVALIDUSERPASS))
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(300));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent("USERNAME OR PASSWORD IS INCORRECT"), skin.GetStyle("SubtitleRed"), GUILayout.Width(250));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        EditorGUILayout.BeginHorizontal(GUILayout.Width(300));
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("FORGOT PASSWORD"), GUILayout.Width(160)))
            Application.OpenURL(controller.LoginInfo.apiurl + "#/reset");

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Displays the loading bar and starts the login process
    /// </summary>
    private void ProcessLogin(HomeController controller)
    {
        if (!controller.ValidateTextFields())
            return;

        EditorUtility.DisplayProgressBar("Loading", "Logging In", .1f);

        if (controller.IsLoginSuccessful())
        {
            EditorUtility.DisplayProgressBar("Loading", "Initializing Platform", 0.4f);
            controller.ProcessStartUp();
        }

        EditorUtility.ClearProgressBar();
        GUIUtility.ExitGUI();
    }
}