using UnityEngine;

public class HomeTab
{
    #region Definitions & Properties
    // Private variables & objects
    private GUISkin _skin;                      // Theme file
    private HomeController _controller;         // Controller process user input & interaction
    private HT_PlatformPanel _platformPanel;    // Main GUI plaform panel
    private HT_NewsPanel _newsPanel;            // News GUI panel
    private HT_SupportPane _supportPanel;       // Support GUI panel
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="skin">Theme</param>
    public HomeTab(GUISkin skin)
    {
        _skin = skin;
        _controller = new HomeController();
        _platformPanel = new HT_PlatformPanel();
        _newsPanel = new HT_NewsPanel();
        _supportPanel = new HT_SupportPane();
    }

    /// <summary>
    /// Draws the main sub panels
    /// </summary>
    public void DrawHomeTab()
    {
        _platformPanel.DrawMainPlatformPanel(_controller, _skin);
        _newsPanel.DrawNewsPanel(_controller, _skin);
        _supportPanel.DrawSupportPanel(_skin);
    }
}