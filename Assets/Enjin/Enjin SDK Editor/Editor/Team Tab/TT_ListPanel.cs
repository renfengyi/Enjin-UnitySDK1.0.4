using UnityEngine;
using UnityEditor;
using EnjinSDK;

public class TT_ListPanel
{
    #region Declairations & Properties
    // Private variables & Objects
    private Vector2 _scrollPos;
    private GUIStyle _numStyle;
    private Color _bgDefault;
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public TT_ListPanel()
    {
        _bgDefault = GUI.backgroundColor;
        _scrollPos = Vector2.zero;
    }

    /// <summary>
    /// Draws and populates the team list panel
    /// </summary>
    public void DrawTeamListPanel(TeamController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 126, 912, 490), skin.GetStyle("TopBackground"));
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        if (controller.HasRefreshedList)
            EditorGUILayout.LabelField(new GUIContent("TEAM LIST"), skin.GetStyle("MainTitle"));
        else
            EditorGUILayout.LabelField(new GUIContent("SEARCH RESULTS"), skin.GetStyle("MainTitle"));

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.LabelField(new GUIContent("ID"), skin.GetStyle("Subtitle"), GUILayout.Width(94));
        EditorGUILayout.LabelField(new GUIContent("ROLE"), skin.GetStyle("Subtitle"), GUILayout.Width(284));
        EditorGUILayout.LabelField(new GUIContent("IDENTITY ID"), skin.GetStyle("Subtitle"), GUILayout.Width(140));
        EditorGUILayout.LabelField(new GUIContent("USERNAME"), skin.GetStyle("Subtitle"), GUILayout.Width(164));
        EditorGUILayout.LabelField(new GUIContent("EMAIL"), skin.GetStyle("Subtitle"), GUILayout.Width(166));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(880), GUILayout.Height(342));

        for (int i = 0; i < controller.UserList.Count; i++)
        {
            if (controller.SelectedIndex == i)
                EditorGUILayout.BeginHorizontal(skin.box);
            else
                EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(new GUIContent(controller.UserList[i].id.ToString()), skin.GetStyle("ContentDark"), GUILayout.Width(70));
            Rect lastRect = GUILayoutUtility.GetLastRect();

            if (GUI.Button(new Rect(lastRect.x, lastRect.y, 890.0f, lastRect.height), GUIContent.none, skin.button))
                controller.SelectedIndex = i;

            GUILayout.Space(10);
            controller.GetCurrentUserRoles(i);
            EditorGUILayout.LabelField(new GUIContent(controller.UserCurrentRoles), skin.GetStyle("ContentDark"), GUILayout.Width(260));

            GUILayout.Space(10);

            if (controller.UserList[i].identities.Length != 0)
                EditorGUILayout.LabelField(new GUIContent(controller.UserList[i].identities[0].id.ToString()), skin.GetStyle("ContentDark"), GUILayout.Width(120));
            else
                EditorGUILayout.LabelField(new GUIContent("Not Linked"), skin.GetStyle("ContentDark"), GUILayout.Width(120));

            GUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent(controller.UserList[i].name), skin.GetStyle("ContentDark"), GUILayout.Width(140));
            GUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent(controller.UserList[i].email), skin.GetStyle("ContentDark"), GUILayout.Width(166));
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = _bgDefault;
            GUILayout.Space(8);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(30);
        EditorGUILayout.BeginHorizontal();

        if (!controller.IsInSearchMode)
        {
            if (controller.CurrentPage != 1)
            {
                GUILayout.Space(10);

                if (GUILayout.Button(new GUIContent("<<"), GUILayout.Height(20)))
                    controller.PreviousPage();
            }

            GUILayout.Space(5);

            for (int i = controller.FirstPage; i < controller.TotalPages + 1; i++)
            {
                if (i != controller.CurrentPage)
                    _numStyle = skin.GetStyle("PageNumberDark");
                else
                    _numStyle = skin.GetStyle("PageNumberLight");


                if (GUILayout.Button(new GUIContent(i.ToString()), _numStyle, GUILayout.Width(30)))
                    controller.SelectedPage(i);

                if (i - controller.FirstPage == 9)
                    break;
            }

            if (controller.CurrentPage != controller.TotalPages)
            {
                if (GUILayout.Button(new GUIContent(">>"), GUILayout.Height(20)))
                    controller.NextPage();
            }
        }

        GUILayout.FlexibleSpace();

        if (controller.HasPermission(UserPermission.manageUsers))
        {
            if (GUILayout.Button("EDIT", GUILayout.Width(100), GUILayout.Height(30)))
                controller.EditMode();

            if (GUILayout.Button("DELETE", GUILayout.Width(100), GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("WARNING", "This action will delete the selected team member from this application. Once deleted this user can not be recreated with the same credentials!", "Confirm", "Cancel"))
                {
                    if (controller.DeleteUser())
                        EditorUtility.DisplayDialog("SUCCESS", "User was successfully deleted.", "Ok");
                    else
                        EditorUtility.DisplayDialog("FAILED", "User not able to be deleted.", "Ok");
                }
            }

            if (controller.HasPermission(UserPermission.manageApp))
            {
                if (GUILayout.Button("INVITE", GUILayout.Width(100), GUILayout.Height(30)))
                    controller.SetTeamState(EnjinEditorPanel.TeamState.INVITEUSER);
            }
        }

        GUILayout.Space(16);
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }
}
