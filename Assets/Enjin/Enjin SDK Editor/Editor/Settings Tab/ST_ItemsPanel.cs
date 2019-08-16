using UnityEngine;
using UnityEditor;

public class ST_ItemsPanel
{
    public void DrawItemsSettingsPanel(GUISkin skin, SettingsController controller)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(880));

        controller.ItemsFoldout = EditorGUILayout.Foldout(controller.ItemsFoldout, new GUIContent("CRYPTOITEM SETTINGS"));

        if (controller.ItemsFoldout)
        {

        }
    }
}