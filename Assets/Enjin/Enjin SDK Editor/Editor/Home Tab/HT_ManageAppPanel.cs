using UnityEditor;
using UnityEngine;
using EnjinEditorPanel;

public class HT_ManageAppPanel
{
    /// <summary>
    /// Handles create and edit functions for applications
    /// </summary>
    public void DrawManageAppsPanel(HomeController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(10, 200, 350, 380), skin.GetStyle("TopBackground"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent(controller.State.ToString() + " APP"), skin.GetStyle("MainTitle"));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(30);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(new GUIContent("APP NAME"), skin.GetStyle("Subtitle"));
        controller.AppInfo.name = EditorGUILayout.TextField(controller.AppInfo.name, skin.textField, GUILayout.Width(320), GUILayout.Height(30));
        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("ICON URL (OPTIONAL)"), skin.GetStyle("Subtitle"));
        controller.AppInfo.image = EditorGUILayout.TextField(controller.AppInfo.image, skin.textField, GUILayout.Width(320), GUILayout.Height(30));
        GUILayout.Space(20);
        EditorGUILayout.LabelField(new GUIContent("DESCRIPTION (OPTIONAL"), skin.GetStyle("Subtitle"));
        controller.AppInfo.description = EditorGUILayout.TextArea(controller.AppInfo.description, skin.textArea, GUILayout.Width(320), GUILayout.Height(90));
        GUILayout.Space(30);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        controller.AppInfo.description = controller.AppInfo.description.Replace("\n", " ");

        if (controller.State == PlatformState.CREATE)
        {
            if (GUILayout.Button("CREATE", GUILayout.Width(150), GUILayout.Height(30)))
            {
                if (controller.AppInfo.name == "")
                    EditorUtility.DisplayDialog("INVALID NAME", "Applicaiton name field must be filled out", "Ok");
                else
                {
                    controller.CreateApp();
                    GUI.FocusControl(null);
                }
            }
        }
        else
        {
            if (GUILayout.Button("UPDATE", GUILayout.Width(150), GUILayout.Height(30)))
                controller.UpdateAp();
        }

        GUILayout.Space(16);

        if (GUILayout.Button("BACK", GUILayout.Width(150), GUILayout.Height(30)))
            controller.ReturnToView();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}