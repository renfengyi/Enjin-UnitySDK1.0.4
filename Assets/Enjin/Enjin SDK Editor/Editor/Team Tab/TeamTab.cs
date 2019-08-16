using UnityEngine;
using EnjinEditorPanel;

public class TeamTab
{
    #region Declairations & Properties
    // Private variables & objects
    private GUISkin _skin;
    private TeamController _controller;
    private TT_OptionsPanel _optionsPanel;
    private TT_ListPanel _listPanel;
    private SearchPanel _teamSearch;
    private TT_MemberPanel _memberPanel;
    private TT_RolesPanels _rolesPanels;
    private TT_InvitePanel _invitePanel;
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="skin">Theme Object</param>
    public TeamTab(GUISkin skin)
    {
        _skin = skin;
        _controller = new TeamController();
        _optionsPanel = new TT_OptionsPanel();
        _listPanel = new TT_ListPanel();
        _teamSearch = new SearchPanel();
        _memberPanel = new TT_MemberPanel();
        _rolesPanels = new TT_RolesPanels();
        _invitePanel = new TT_InvitePanel();
    }

    /// <summary>
    /// Main draw loop for team tab
    /// </summary>
    public void DrawTeamTab()
    {
        switch(_controller.State)
        {
            case TeamState.VIEWLIST:
                DrawTeamPanels();
                break;

            case TeamState.MANAGEROLES:
            case TeamState.EDITROLE:
            case TeamState.CREATEROLE:
                _rolesPanels.DrawTeamRolesPanel(_controller, _skin);
                break;

            case TeamState.CREATE:
            case TeamState.EDIT:
                _memberPanel.DrawTeamMemberPanel(_controller, _skin);
                break;
            case TeamState.INVITEUSER:
                _optionsPanel.DrawTeamOptionsPanel(_controller, _skin);
                _teamSearch.DrawTeamSearchPanel(_controller, _skin);
                _invitePanel.DrawInviteUserPanel(_controller, _skin);
                break;
            case TeamState.REFRESH:
                EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.REFRESHUSERROLES);
                _controller.SetTeamState(TeamState.VIEWLIST);
                DrawTeamTab();
                break;
        }
    }

    /// <summary>
    /// Resets the search field;
    /// </summary>
    public void ResetSearch() { _controller.ResetSearch(); GUI.FocusControl(null); }

    /// <summary>
    /// Resets the team list
    /// </summary>
    public void ResetTeamList() { _controller.ResetTeamList(); }

    /// <summary>
    /// Displays the main team member panels
    /// </summary>
    private void DrawTeamPanels()
    {
        _optionsPanel.DrawTeamOptionsPanel(_controller, _skin);
        _teamSearch.DrawTeamSearchPanel(_controller, _skin);
        _listPanel.DrawTeamListPanel(_controller, _skin);
    }
}