using UnityEngine;
using UnityEditor;

public class SettingsTab
{
    // Editor private variables
    private GUISkin _skin;
    private SettingsController _controller;
    private ST_GeneralPanel _generalSettings;
    private ST_FieldsPanel _fieldsSettings;
    private ST_ItemsPanel _itemsSettings;

    // Properties
    public int ItemsPerPage { get { return _controller.UserSettings.ItemsPerPage; } }
    public string GetVersion { get { return _controller.Version; } }

    public SettingsTab(GUISkin skin)
    {
        _skin = skin;
        _controller = new SettingsController();
        _generalSettings = new ST_GeneralPanel();
        _fieldsSettings = new ST_FieldsPanel();
        _itemsSettings = new ST_ItemsPanel();
    }

    public void DrawSettingsTab()
    {
        GUILayout.BeginArea(new Rect(5, 10, 912, 606), _skin.GetStyle("TopBackground"));
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("SETTINGS"), _skin.GetStyle("MainTitle"));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(30);
        _generalSettings.DrawGeneralSettingsPanel(_skin, _controller);
        GUILayout.EndArea();
    }

    public void CheckAllowance()
    {
        _controller.UpdateAllowance();
    }
}