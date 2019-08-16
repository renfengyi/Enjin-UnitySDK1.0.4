using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;
using System.Text.RegularExpressions;

public class CryptoItemsTab
{
    #region Variables, Objects & Properties
    private GUISkin _skin;
    private CryptoItemsController _controller;

    private CT_DetailsPane _detailsPane;
    private CT_MintCostPane _mintCostPane;
    private CT_OptionsPane _optionsPane;
    private CT_SearchPane _searchPane;
    private CT_WalletPane _walletPane;

    private CT_CreatePanel _createPanel;
    private CT_EditPanel _editPanel;
    private CT_MainPanel _mainPanel;
    private CT_MeltPanel _meltPanel;
    private CT_MintPanel _mintPanel;
    //private CT_EditPanel _viewPanel;
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="skin">Theme skin</param>
    public CryptoItemsTab(GUISkin skin)
    {
        _skin = skin;
        _controller = new CryptoItemsController
        {
            ClipBoard = skin.GetStyle("Images").normal.scaledBackgrounds[1] as Texture2D
        };

        // link local controller instance to NotificationMonitor
        EnjinEditor.NotificationMonitor.CIController = _controller;

        EditorUtility.DisplayProgressBar("Loading", "Building CryptoItems", 0.95f);

        _detailsPane = new CT_DetailsPane();
        _mintCostPane = new CT_MintCostPane();
        _optionsPane = new CT_OptionsPane();
        _searchPane = new CT_SearchPane();
        _walletPane = new CT_WalletPane();

        _createPanel = new CT_CreatePanel();
        _editPanel = new CT_EditPanel();
        _mainPanel = new CT_MainPanel();
        _meltPanel = new CT_MeltPanel();
        _mintPanel = new CT_MintPanel();
        //_viewPanel = new CT_EditPanel();
    }

    /// <summary>
    /// Resets the search field;
    /// </summary>
    public void ResetSearch() { _controller.SearchText = ""; _controller.IsSearchMode = false; GUI.FocusControl(null); }

    /// <summary>
    /// Main Method for the CI Tab. This method manages the state and is called repeatedly while the CI Tab is selected.
    /// </summary>
    public void DrawCryptoItemsTab()
    {
        // handle CI Tab state actions and panel display choices
        switch (_controller.State)
        {
            case CryptoItemsController.CryptoItemState.MAIN:
                _optionsPane.DrawPane(_controller, _skin);
                _searchPane.DrawPane(_controller, _skin);
                _mainPanel.DrawPane(_controller, _skin);
                break;

            case CryptoItemsController.CryptoItemState.CREATE:
                _walletPane.DrawPane(_controller, _skin);
                _mintCostPane.DrawPane(_controller, _skin);
                _createPanel.DrawPane(_controller, _skin);
                break;

            case CryptoItemsController.CryptoItemState.EDIT:
                _walletPane.DrawPane(_controller, _skin);
                _detailsPane.DrawPane(_controller, _skin);
                _editPanel.DrawPane(_controller, _skin);
                break;

            case CryptoItemsController.CryptoItemState.VIEW:
                _walletPane.DrawPane(_controller, _skin);
                _detailsPane.DrawPane(_controller, _skin);
                _editPanel.DrawPane(_controller, _skin);
                break;

            case CryptoItemsController.CryptoItemState.MINT:
                _walletPane.DrawPane(_controller, _skin);
                _detailsPane.DrawPane(_controller, _skin);
                _mintPanel.DrawPane(_controller, _skin);
                break;

            case CryptoItemsController.CryptoItemState.MELT:
                _walletPane.DrawPane(_controller, _skin);
                _detailsPane.DrawPane(_controller, _skin);
                _meltPanel.DrawPane(_controller, _skin);
                break;

            case CryptoItemsController.CryptoItemState.CREATEBUNDLE:
                // NOTE: Will be implemented post v1 release
                break;

            case CryptoItemsController.CryptoItemState.REFRESH:
                EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADITEMS);
                _controller.Reset();
                break;
        }
    }

    public void ResetCryptoItemList()
    {
        _controller.ResetCryptoItemList();
    }
}