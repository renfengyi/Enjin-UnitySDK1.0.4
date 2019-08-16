using System.Collections.Generic;
using UnityEngine;
using EnjinSDK;

public class IdentitiesTab
{
    #region Definitions & Properties
    private enum IdentityState { VIEW, EDIT, CREATE }

    private GUISkin _skin;                              // Custom UI display object
    private IdentitiesTabController _controller;        // Controller for accessing identities tab business logic
    private IT_ListPane _listPane;                      //
    private IT_CreateEditPane _createEditPane;          // 
    //private IT_OptionsPanel _optionsPane;               //
    private IT_SearchPanel _searchPane;                 //
    #endregion

    /// <summary>
    /// Initializes the variables used by the identity manager tab
    /// </summary>
    /// <param name="skin">GUISkin for custom Editor display</param>
    public IdentitiesTab(GUISkin skin)
    {
        _skin = skin;

        _controller = new IdentitiesTabController();

        _listPane = new IT_ListPane();
        _createEditPane = new IT_CreateEditPane();
        //_optionsPane = new IT_OptionsPanel();
        _searchPane = new IT_SearchPanel();

        _controller.RefreshLists();
    }

    /// <summary>
    /// Draws the information to display in the tab
    /// </summary>
    public void DrawIdentityTab()
    {
        // check for popup notifications
        _controller.CheckForPopUps();

        // options
        //_optionsPane.DrawOptionsPanel(_controller, _skin);

        // search
        _searchPane.DrawSearchPane(_controller, _skin);

        switch (_controller.State)
        {
            case IdentitiesTabController.IdentityState.VIEW:
                _listPane.DrawListPane(_controller, _skin);
                break;

            case IdentitiesTabController.IdentityState.CREATE:
                _createEditPane.DrawCreateEditPane(_controller, _skin);
                break;

            case IdentitiesTabController.IdentityState.EDIT:
                _createEditPane.DrawCreateEditPane(_controller, _skin);
                break;
        }
    }

    /// <summary>
    /// Helper method for external calls to reset content on this tab.
    /// </summary>
    public void ResetSearch()
    {
        _controller.ResetSearch();
    }

    /// <summary>
    /// Helper method for external calls to reset the identity list on this tab.
    /// </summary>
    public void ResetIdentityList()
    {
        _controller.RefreshLists();
    }
}