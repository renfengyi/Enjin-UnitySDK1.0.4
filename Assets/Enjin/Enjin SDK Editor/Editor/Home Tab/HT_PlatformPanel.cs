using UnityEditor;
using UnityEngine;
using EnjinSDK;

public class HT_PlatformPanel
{
    private HT_LogininPanel _loginPanel;
    private HT_LoggedInPanel _loggedInPanel;

    public HT_PlatformPanel()
    {
        _loginPanel = new HT_LogininPanel();
        _loggedInPanel = new HT_LoggedInPanel();
    }

    /// <summary>
    /// Draws the main platform panel
    /// </summary>
    public void DrawMainPlatformPanel(HomeController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 10, 370, 606), skin.GetStyle("TopBackground"));
        EditorGUILayout.BeginHorizontal(GUILayout.Width(392));
        EditorGUILayout.LabelField(new GUIContent("PLATFORM"), skin.GetStyle("MainTitle"));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal(GUILayout.Width(370));
        GUILayout.FlexibleSpace();
        GUILayout.Label(skin.GetStyle("Images").normal.scaledBackgrounds[0] as Texture2D, GUILayout.Width(128), GUILayout.Height(128));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(26);

        if (controller.IsLoginStatus(LoginState.VALID))
            _loggedInPanel.DrawPlatformLoggedInPanel(controller, skin);
        else
            _loginPanel.DrawPlatformLoginPanel(controller, skin);

        GUILayout.EndArea();
    }
}