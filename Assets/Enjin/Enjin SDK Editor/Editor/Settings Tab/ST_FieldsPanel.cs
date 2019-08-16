using UnityEngine;
using UnityEditor;

public class ST_FieldsPanel
{
    public void DrawFieldsSettingsPanel(GUISkin skin, SettingsController controller)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(14);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(880));

        controller.FieldsFoldout = EditorGUILayout.Foldout(controller.FieldsFoldout, new GUIContent("FIELDS SETTINGS"));

        if (controller.FieldsFoldout)
        {

        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndHorizontal();
    }
}