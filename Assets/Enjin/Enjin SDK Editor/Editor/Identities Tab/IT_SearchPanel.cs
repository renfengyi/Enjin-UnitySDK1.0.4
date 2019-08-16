using UnityEngine;
using UnityEditor;
using EnjinSDK;
using System.Collections.Generic;

public class IT_SearchPanel
{
    public void DrawSearchPane(IdentitiesTabController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 10, 912, 100), skin.GetStyle("TopBackground"));
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("SEARCH IDENTITIES"), skin.GetStyle("MainTitle"), GUILayout.Width(165));
        EditorGUILayout.BeginVertical();
        GUILayout.Space(12);
        EditorGUILayout.LabelField(new GUIContent("ID, User ID, Link Code, Eth Address"), skin.GetStyle("ContentDark"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        controller.SearchText = EditorGUILayout.TextField(controller.SearchText, skin.textField, GUILayout.Height(30));

        if (GUILayout.Button(new GUIContent("Search"), GUILayout.Height(32), GUILayout.Width(100)))
        {
            controller.IdentitiesList.Clear();
            controller.FieldsFoldout.Clear();
            Identity[] sResults = Enjin.SearchIdentities(controller.SearchText);

            if (sResults != null)
                controller.IdentitiesList = new List<Identity>(sResults);

            for (int i = 0; i < controller.IdentitiesList.Count; i++)
                controller.FieldsFoldout.Add(false);

            controller.IsInSearchMode = true;
            controller.HasRefreshedList = false;
        }

        if (controller.SearchText == "" && !controller.HasRefreshedList)
        {
            controller.RefreshLists();
            controller.IsInSearchMode = false;
            controller.HasRefreshedList = true;
            controller.SelectedIndex = 0;
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
