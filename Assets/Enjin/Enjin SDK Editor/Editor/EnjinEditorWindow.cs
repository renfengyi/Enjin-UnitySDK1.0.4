using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;

public class EnjinEditorWindow : EditorWindow
{
    #region Declairations & Properties
    // Private variables & Objects
    private static int _toolbarIndex;
    private static GUISkin _skin;
    private static GUIContent[] _toolbarHeaders;
    #endregion

    /// <summary>
    /// Set the window to be shown when selected from the window menu
    /// </summary>
    [MenuItem("Window/Enjin SDK Editor (Alpha)")]
    private static void Init()
    {
        EnjinEditorWindow enjinSDKWindow = (EnjinEditorWindow)EditorWindow.GetWindow(typeof(EnjinEditorWindow));
        enjinSDKWindow.titleContent = new GUIContent("Enjin Editor");
        enjinSDKWindow.autoRepaintOnSceneChange = true;
        enjinSDKWindow.maxSize = new Vector2(940f, 674f);
        enjinSDKWindow.minSize = enjinSDKWindow.maxSize;
        enjinSDKWindow.Show();
    }

    /// <summary>
    /// Initialization when editor window is opened
    /// </summary>
    private void OnEnable()
    {
        _toolbarIndex = 0;
        _toolbarHeaders = new GUIContent[] { new GUIContent("Home", "Entry point panel"), new GUIContent("Team", "Team management panel"), new GUIContent("Identities", "Identities management panel"),
            new GUIContent("CryptoItems", "Token management panel"), new GUIContent("Wallet", "Wallet management panel"), new GUIContent("Settings", "Account settings / options panel") };

        Texture2D bgText = new Texture2D(2, 2);
        var fillColor = bgText.GetPixels32();

        for (int i = 0; i < fillColor.Length; i++)
            fillColor[i] = new Color32(128, 128, 128, 255);

        bgText.SetPixels32(fillColor);
        bgText.Apply();

        _skin = (AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/Enjin/Enjin SDK Editor/Themes/EnjinSDKEditorSkin.guiskin"));
        _skin.label.normal.background = bgText;

        EnjinEditor.SkinTheme = _skin;
        EnjinEditor.Init();
    }

    /// <summary>
    /// Main GUI Rendering Loop
    /// </summary>
    private void OnGUI()
    {
        if (EditorApplication.isCompiling && Enjin.IsLoggedIn)
            EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.LOGOUT);

        if (EditorApplication.isPlaying)
        {
            if (Enjin.IsLoggedIn)
                EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.LOGOUT);

            // Show blank window for now
        }
        else
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), new Color(0.1f, 0.1f, 0.1f));
            _toolbarIndex = GUI.Toolbar(new Rect(15, 5, 910, 40), _toolbarIndex, _toolbarHeaders);
            GUILayout.BeginArea(new Rect(10, 50, position.width, position.height));
            _toolbarIndex = EnjinEditor.TabSelection(_toolbarIndex);
            GUILayout.EndArea();
        }
    }

    private void Update()
    {
        if (EnjinEditor.NotificationMonitor.ResultsQueue.Count > 0)
            EnjinEditor.NotificationMonitor.ProcessRequests();

        EnjinEditor.NotificationMonitor.DisplayErrorDialog();
    }

    private void OnDestroy()
    {
        if (Enjin.IsLoggedIn)
            EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.LOGOUT);
    }
}