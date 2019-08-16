using UnityEngine;
using UnityEditor;
using EnjinEditorPanel;
using EnjinSDK.Helpers;

public class IT_ListPane {

    #region Declairations & Properties
    // Private variables & Objects
    private Vector2 _scrollPos;
    private GUIStyle _numStyle;
    private Color _bgDefault;
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
	public IT_ListPane()
    {
        _bgDefault = GUI.backgroundColor;
        _scrollPos = Vector2.zero;
    }

    public void DrawListPane(IdentitiesTabController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 126, 912, 490), skin.GetStyle("TopBackground"));
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        
        if (controller.HasRefreshedList)
            EditorGUILayout.LabelField(new GUIContent("TEAM LIST"), skin.GetStyle("MainTitle"));
        else
            EditorGUILayout.LabelField(new GUIContent("IDENTITIES (" + EnjinEditor.CurrentAppName + ")"), skin.GetStyle("MainTitle"));

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.LabelField(new GUIContent("ID"), skin.GetStyle("Subtitle"), GUILayout.Width(96));
        EditorGUILayout.LabelField(new GUIContent("USER ID"), skin.GetStyle("Subtitle"), GUILayout.Width(124));
        EditorGUILayout.LabelField(new GUIContent("USERNAME"), skin.GetStyle("Subtitle"), GUILayout.Width(148));
        EditorGUILayout.LabelField(new GUIContent("LINK CODE"), skin.GetStyle("Subtitle"), GUILayout.Width(148));
        EditorGUILayout.LabelField(new GUIContent("ETH ADDRESS"), skin.GetStyle("Subtitle"), GUILayout.Width(156));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(880), GUILayout.Height(342));

        for (int i = 0; i < controller.IdentitiesList.Count; i++)
        {
            if (controller.SelectedIndex == i)
                EditorGUILayout.BeginHorizontal(skin.box);
            else
                EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(new GUIContent(controller.IdentitiesList[i].id.ToString()), skin.GetStyle("ContentDark"), GUILayout.Width(90));
            Rect lastRect = GUILayoutUtility.GetLastRect();

            if (GUI.Button(new Rect(lastRect.x, lastRect.y, 890.0f, lastRect.height), GUIContent.none, skin.button))
            {
                if (controller.SelectedIndex == 1)
                {
                    controller.CurrentIdentity = controller.IdentitiesList[controller.SelectedIndex];
                    controller.State = IdentitiesTabController.IdentityState.EDIT;
                }

                controller.SelectedIndex = i;
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent(controller.IdentitiesList[i].user.id.ToString()), skin.GetStyle("ContentDark"), GUILayout.Width(130));
            EditorGUILayout.LabelField(new GUIContent(controller.IdentitiesList[i].user.name), skin.GetStyle("ContentDark"), GUILayout.Width(150));

            if (controller.IdentitiesList[i].linking_code == null)
                EditorGUILayout.LabelField(new GUIContent("Linked"), skin.GetStyle("ContentDark"), GUILayout.Width(150));
            else
                EditorGUILayout.LabelField(new GUIContent(controller.IdentitiesList[i].linking_code), skin.GetStyle("ContentDark"), GUILayout.Width(150));

            if (controller.IdentitiesList[i].ethereum_address == null)
                EditorGUILayout.LabelField(new GUIContent("Not Linked"), skin.GetStyle("ContentDark"));
            else
                EditorGUILayout.LabelField(new GUIContent(controller.IdentitiesList[i].ethereum_address), skin.GetStyle("ContentDark"));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = _bgDefault;

            if (controller.IdentitiesList[i].fields.Length != 0)
            {
                GUILayout.Space(4);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                controller.FieldsFoldout[i] = EditorGUILayout.Foldout(controller.FieldsFoldout[i], new GUIContent("FIELDS (" + controller.IdentitiesList[i].id.ToString() + ")"));

                if (controller.FieldsFoldout[i])
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(14);
                    EditorGUILayout.LabelField(new GUIContent("KEY"), skin.GetStyle("Subtitle"), GUILayout.Width(120));
                    EditorGUILayout.LabelField(new GUIContent("VALUE"), skin.GetStyle("Subtitle"), GUILayout.Width(420));
                    EditorGUILayout.LabelField(new GUIContent("SEARCHABLE"), skin.GetStyle("Subtitle"), GUILayout.Width(100));
                    EditorGUILayout.LabelField(new GUIContent("DISPLAYABLE"), skin.GetStyle("Subtitle"), GUILayout.Width(100));
                    EditorGUILayout.LabelField(new GUIContent("UNIQUE"), skin.GetStyle("Subtitle"), GUILayout.Width(66));
                    EditorGUILayout.EndHorizontal();

                    for (int n = 0; n < controller.IdentitiesList[i].fields.Length; n++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(14);
                        EditorGUILayout.LabelField(new GUIContent(controller.IdentitiesList[i].fields[n].key), GUILayout.Width(120));
                        EditorGUILayout.LabelField(new GUIContent(controller.IdentitiesList[i].fields[n].value), GUILayout.Width(420));
                        EditorGUILayout.LabelField(new GUIContent(EnjinHelpers.IntToBoolString(controller.IdentitiesList[i].fields[n].searchable)), GUILayout.Width(100));
                        EditorGUILayout.LabelField(new GUIContent(EnjinHelpers.IntToBoolString(controller.IdentitiesList[i].fields[n].displayable)), GUILayout.Width(100));
                        EditorGUILayout.LabelField(new GUIContent(EnjinHelpers.IntToBoolString(controller.IdentitiesList[i].fields[n].unique)), GUILayout.Width(66));
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(8);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();

        // Pagination UX
        if (!controller.IsInSearchMode)
        {
            GUILayout.Space(30);
            EditorGUILayout.BeginHorizontal();

            if (controller.CurrentPage != 1)
            {
                if (GUILayout.Button(new GUIContent("<<"), GUILayout.Height(20)))
                {
                    if (controller.CurrentPage != 1)
                    {
                        controller.CurrentPage--;
                        controller.PageCheck();
                    }
                }
            }

            GUILayout.Space(10);

            for (int i = controller.FirstPage; i < controller.TotalPages + 1; i++)
            {
                if (i != controller.CurrentPage)
                    _numStyle = skin.GetStyle("PageNumberDark");
                else
                    _numStyle = skin.GetStyle("PageNumberLight");

                if (GUILayout.Button(new GUIContent(i.ToString()), _numStyle, GUILayout.Width(30)))
                {
                    controller.CurrentPage = i;
                    controller.PageCheck();
                }

                if (i - controller.FirstPage == 9)
                    break;
            }

            if (controller.CurrentPage != controller.TotalPages)
            {
                if (GUILayout.Button(new GUIContent(">>"), GUILayout.Height(20)))
                {
                    if (controller.CurrentPage != controller.TotalPages)
                    {
                        controller.CurrentPage++;
                        controller.PageCheck();
                    }
                }
            }

            GUILayout.FlexibleSpace();

            if (controller.IdentitiesList.Count > 0)
            {
                if (GUILayout.Button("VIEW", GUILayout.Width(100), GUILayout.Height(30)))
                {
                    controller.CurrentIdentity = controller.IdentitiesList[controller.SelectedIndex];
                    controller.State = IdentitiesTabController.IdentityState.EDIT;
                }
            }

            GUILayout.Space(16);
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }

        GUILayout.EndArea();
    }
	

}
