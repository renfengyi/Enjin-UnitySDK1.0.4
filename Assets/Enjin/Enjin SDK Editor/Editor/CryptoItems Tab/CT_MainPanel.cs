using UnityEngine;
using UnityEditor;
using EnjinSDK;
using EnjinEditorPanel;

public class CT_MainPanel
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
	public CT_MainPanel()
    {
        _bgDefault = GUI.backgroundColor;
        _scrollPos = Vector2.zero;
    }

    /// <summary>
    /// Build and Draw Main Panel
    /// </summary>
    public void DrawPane(CryptoItemsController controller, GUISkin skin)
    {
        GUILayout.BeginArea(new Rect(5, 126, 912, 490), skin.GetStyle("TopBackground"));
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        if (controller.HasListRefreshed)
        {
            EditorGUILayout.LabelField(new GUIContent(EnjinEditor.CurrentAppName.ToUpper()), skin.GetStyle("MainTitle"));
            GUILayout.FlexibleSpace();
            //GUILayout.Space(16);
            //EditorStyles.popup.fixedHeight = 30;
            //EditorStyles.popup.fontSize = 12;
            //controller.ItemFilter = (CryptoItemsController.ItemFilterType)EditorGUILayout.EnumPopup(controller.ItemFilter, GUILayout.Width(180), GUILayout.Height(30));
            //EditorStyles.popup.fixedHeight = 15;
            //EditorStyles.popup.fontSize = 11;
        }
        else
            EditorGUILayout.LabelField(new GUIContent("SEARCH RESULTS"), skin.GetStyle("MainTitle"));

        if (GUILayout.Button("REFRESH", GUILayout.Height(30), GUILayout.Width(80)))
            EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADITEMS);

        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();

        if (controller.LastFilterSelected != controller.FilterSelection)
            controller.LastFilterSelected = controller.FilterSelection;

        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);
        EditorGUILayout.LabelField(new GUIContent("Type"), skin.GetStyle("Subtitle"), GUILayout.Width(76));
        EditorGUILayout.LabelField(new GUIContent("Name"), skin.GetStyle("Subtitle"), GUILayout.Width(100));
        EditorGUILayout.LabelField(new GUIContent("Index"), skin.GetStyle("Subtitle"), GUILayout.Width(74));
        EditorGUILayout.LabelField(new GUIContent("Balance"), skin.GetStyle("Subtitle"), GUILayout.Width(108));
        EditorGUILayout.LabelField(new GUIContent("Total Supply"), skin.GetStyle("Subtitle"), GUILayout.Width(102));
        EditorGUILayout.LabelField(new GUIContent("Total Reserve"), skin.GetStyle("Subtitle"), GUILayout.Width(98));
        EditorGUILayout.LabelField(new GUIContent("Circulating"), skin.GetStyle("Subtitle"), GUILayout.Width(86));
        EditorGUILayout.LabelField(new GUIContent("Transferable"), skin.GetStyle("Subtitle"), GUILayout.Width(114));
        EditorGUILayout.LabelField(new GUIContent("Supply Type"), skin.GetStyle("Subtitle"));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(16);

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(890), GUILayout.Height(342));

        if (EnjinEditor.CurrentUserIdentity.linking_code != string.Empty)
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("WARNING: You don't have a wallet linked. Select the wallet tab to link your wallet"), skin.GetStyle("MainTitle"));
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        else if (controller.CryptoItemList != null && controller.CryptoItemList.Count != 0)
        {
            for (int i = 0; i < controller.CryptoItemList.Count; i++)
            {
                if (controller.SelectedIndex == i)
                    EditorGUILayout.BeginHorizontal(skin.box);
                else
                    EditorGUILayout.BeginHorizontal();

                if (controller.CryptoItemList[i].nonFungible)
                    EditorGUILayout.LabelField(new GUIContent("Non-fungible"), skin.GetStyle("ContentDark"), GUILayout.Width(75));
                else
                    EditorGUILayout.LabelField(new GUIContent("Fungible"), skin.GetStyle("ContentDark"), GUILayout.Width(75));

                Rect lastRect = GUILayoutUtility.GetLastRect();

                if (GUI.Button(new Rect(lastRect.x, lastRect.y, 890.0f, lastRect.height), GUIContent.none, skin.button))
                {
                    if (controller.SelectedIndex == i)
                    {
                        controller.State = CryptoItemsController.CryptoItemState.VIEW;
                        controller.CurrentCryptoItem = controller.CryptoItemList[controller.SelectedIndex];
                        controller.CurrentCryptoItem.itemURI = Enjin.GetCryptoItemURI(controller.CurrentCryptoItem.token_id, controller.CurrentCryptoItem.index, false);
                    }

                    controller.SelectedIndex = i;
                }

                GUILayout.Space(10);

                if (controller.CryptoItemList[i].name.Length > 40)
                    EditorGUILayout.LabelField(new GUIContent(controller.CryptoItemList[i].name.Substring(0, 38) + ".."), skin.GetStyle("ContentDark"), GUILayout.Width(160));
                else
                    EditorGUILayout.LabelField(new GUIContent(controller.CryptoItemList[i].name), skin.GetStyle("ContentDark"), GUILayout.Width(100));

                GUILayout.Space(10);

                try
                {
                    if (controller.CryptoItemList[i].index != null)
                    {
                        string indexList = "";
                        string[] indices = controller.CryptoItemList[i].index.Split(',');

                        for (int indexIndex = 0; indexIndex < indices.Length; indexIndex++)
                        {
                            int subItemIndex = 0;
                            if (System.Int32.TryParse(indices[indexIndex], out subItemIndex))
                            {
                                if (indexIndex > 0 && indexIndex < indices.Length)
                                    indexList += ", ";

                                indexList += subItemIndex;
                            }
                        }

                        EditorGUILayout.LabelField(new GUIContent(indexList), skin.GetStyle("ContentDark"), GUILayout.Width(50));
                    }
                    else
                    {
                        EditorGUILayout.LabelField(new GUIContent(""), skin.GetStyle("ContentDark"), GUILayout.Width(50));
                    }
                }
                catch (System.Exception)
                {
                    EditorGUILayout.LabelField(new GUIContent(""), skin.GetStyle("ContentDark"), GUILayout.Width(50));
                }

                GUILayout.Space(10);
                EditorGUILayout.LabelField(new GUIContent(controller.CryptoItemList[i].balance.ToString()), skin.GetStyle("ContentDark"), GUILayout.Width(90));
                GUILayout.Space(10);

                if (controller.CryptoItemList[i].supplyModel != SupplyModel.INFINITE)
                {
                    if (controller.CryptoItemList[i].totalSupply.Length > 12)
                        EditorGUILayout.LabelField(new GUIContent(System.String.Format("{0:N0}..", System.Int32.Parse(controller.CryptoItemList[i].totalSupply.Substring(0, 9)))), skin.GetStyle("ContentDark"), GUILayout.Width(90));
                    else
                        EditorGUILayout.LabelField(new GUIContent(System.String.Format("{0:N0}", System.Int32.Parse(controller.CryptoItemList[i].totalSupply))), skin.GetStyle("ContentDark"), GUILayout.Width(90));
                }
                else
                    EditorGUILayout.LabelField(new GUIContent("INFINITE"), skin.GetStyle("ContentDark"), GUILayout.Width(90));

                GUILayout.Space(10);
                EditorGUILayout.LabelField(new GUIContent(System.String.Format("{0:N0}", System.Int32.Parse(controller.CryptoItemList[i].reserve))), skin.GetStyle("ContentDark"), GUILayout.Width(80));
                GUILayout.Space(10);
                EditorGUILayout.LabelField(new GUIContent(System.String.Format("{0:N0}", System.Int32.Parse(controller.CryptoItemList[i].circulatingSupply))), skin.GetStyle("ContentDark"), GUILayout.Width(70));
                GUILayout.Space(10);
                EditorGUILayout.LabelField(new GUIContent(controller.CryptoItemList[i].transferable.ToString()), skin.GetStyle("ContentDark"), GUILayout.Width(100));
                GUILayout.Space(10);

                if (controller.CryptoItemList[i].supplyModel == SupplyModel.COLLAPSING)
                    EditorGUILayout.LabelField(new GUIContent("COLLAPSE"), skin.GetStyle("ContentDark"), GUILayout.Width(66));
                else if (controller.CryptoItemList[i].supplyModel == SupplyModel.ANNUAL_PERCENTAGE)
                    EditorGUILayout.LabelField(new GUIContent("ANNUAL %"), skin.GetStyle("ContentDark"), GUILayout.Width(66));
                else if (controller.CryptoItemList[i].supplyModel == SupplyModel.ANNUAL_VALUE)
                    EditorGUILayout.LabelField(new GUIContent("ANNUAL #"), skin.GetStyle("ContentDark"), GUILayout.Width(66));
                else
                    EditorGUILayout.LabelField(new GUIContent(controller.CryptoItemList[i].supplyModel.ToString()), skin.GetStyle("ContentDark"), GUILayout.Width(66));

                if (controller.CryptoItemList[i].markedForDelete)
                    EditorGUILayout.LabelField(new GUIContent(skin.GetStyle("Images").normal.scaledBackgrounds[2] as Texture2D, "Marked for Deletion"), GUILayout.Width(16), GUILayout.Height(16));
                else
                    EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(16), GUILayout.Height(16));

                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = _bgDefault;
                GUILayout.Space(8);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();

        if (!controller.IsSearchMode && controller.CryptoItemList.Count != 0)
        {
            if (controller.CurrentPage != 1)
            {
                GUILayout.Space(10);

                if (GUILayout.Button(new GUIContent("<<"), GUILayout.Height(20)))
                {
                    if (controller.CurrentPage != 1)
                    {
                        controller.CurrentPage--;
                        controller.PageCheck();
                    }
                }
            }

            GUILayout.Space(5);

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
        }

        GUILayout.FlexibleSpace();

        if (controller.CryptoItemList.Count > 0)
        {
            // =================== ADVANCED SEND TEST BUTTON ========================
            //if (GUILayout.Button("A SEND", GUILayout.Width(100), GUILayout.Height(30)))
            //{
                //CryptoItemBatch testItems = new CryptoItemBatch(EnjinEditor.CurrentUserIdentity.id);

                //for (int i = 0; i < controller.CryptoItemList.Count; i++)
                //    testItems.Add(EnjinEditor.CurrentUserIdentity.ethereum_address, "0xeD7aA45fd86c4D58261B3B2Cce9f68009c76C7d1", controller.CryptoItemList[i], controller.CryptoItemList[i].balance);

                //testItems.Send();
            //}
            // ======================================================================

            if (GUILayout.Button("VIEW", GUILayout.Width(100), GUILayout.Height(30)))
            {
                controller.CurrentCryptoItem = controller.CryptoItemList[controller.SelectedIndex];

                // On-demand loading of item URI to avoid performance issues-- this is a demanding call.
                if (controller.CurrentCryptoItem.itemURI == null)
                    controller.CurrentCryptoItem.itemURI = Enjin.GetCryptoItemURI(controller.CurrentCryptoItem.token_id, controller.CurrentCryptoItem.index, false);

                controller.State = CryptoItemsController.CryptoItemState.VIEW;
            }

            if (controller.IsCreator(controller.CryptoItemList[controller.SelectedIndex].creator) && !controller.CryptoItemList[controller.SelectedIndex].markedForDelete)
            {
                if (GUILayout.Button("EDIT", GUILayout.Width(100), GUILayout.Height(30)))
                {
                    controller.CurrentCryptoItem = controller.CryptoItemList[controller.SelectedIndex];

                    if (controller.CurrentCryptoItem.itemURI == null)
                    {
                        if (controller.CurrentCryptoItem.nonFungible)
                        {
                            controller.CurrentCryptoItem.itemURI = Enjin.GetCryptoItemURI(controller.CurrentCryptoItem.token_id, controller.CurrentCryptoItem.index, false);
                        }
                        else
                        {
                            controller.CurrentCryptoItem.itemURI = Enjin.GetCryptoItemURI(controller.CurrentCryptoItem.token_id, "0", false);
                        }
                    }

                    controller.TransferFeeEnj = float.Parse((decimal.Parse(controller.CurrentCryptoItem.transferFeeSettings.value) / ((decimal)Mathf.Pow(10, 18))).ToString());
                    controller.NewCryptoItem.transferFeeSettings = controller.CurrentCryptoItem.transferFeeSettings;
                    controller.EditItemName = controller.CurrentCryptoItem.name;
                    controller.MetaDataURI = controller.CurrentCryptoItem.itemURI;
                    controller.EditMetaDataURI = controller.MetaDataURI;
                    controller.CurrentCryptoItem.isCreator = controller.IsCreator(controller.CurrentCryptoItem.creator);
                    controller.MeltFee = controller.CurrentCryptoItem.meltFeeRatio * 0.01f;
                    controller.State = CryptoItemsController.CryptoItemState.EDIT;
                }

                if (GUILayout.Button(new GUIContent("MINT"), GUILayout.Width(100), GUILayout.Height(30)))
                {
                    controller.MintableCryptoItems = Enjin.GetMintableItems(controller.CryptoItemList[controller.SelectedIndex].token_id);
                    controller.NumToMint = 1;
                    controller.RecieverAddress = new string[1];
                    controller.RecieverAddress[0] = EnjinEditor.CurrentUserIdentity.ethereum_address;
                    controller.Reserve = System.Convert.ToInt32(controller.CurrentCryptoItem.reserve);
                    controller.MeltValue2 = (double)((float)System.Convert.ToDecimal(controller.CurrentCryptoItem.meltValue) / Mathf.Pow(10, 18));
                    controller.CurrentCryptoItem = controller.CryptoItemList[controller.SelectedIndex];
                    controller.State = CryptoItemsController.CryptoItemState.MINT;
                }
            }

            if (controller.CryptoItemList[controller.SelectedIndex].nonFungible == false) //  || System.Int32.Parse(controller.CryptoItemList[controller.SelectedIndex].index) != 0
            {
                if (controller.CryptoItemList[controller.SelectedIndex].balance != 0)
                {
                    if (GUILayout.Button(new GUIContent("MELT"), GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        controller.CurrentCryptoItem = controller.CryptoItemList[controller.SelectedIndex];
                        controller.Balance = controller.CurrentCryptoItem.balance;
                        controller.State = CryptoItemsController.CryptoItemState.MELT;
                    }
                }
            }
        }
        GUILayout.Space(8);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.EndArea();

    }
}
