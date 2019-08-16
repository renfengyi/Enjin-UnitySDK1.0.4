using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EnjinEditorPanel;
using EnjinSDK;

public class NotificationMonitor
{
    public Dictionary<string, Result> ResultsQueue { get; set; }
    public CryptoItemsController CIController { get; set; }

    /***
     * to implement for other tabs, add any additional controllers here and link them 
     * from the appropriate <thing>Tab.cs scripts.
     */

    private List<string> RequestsCompleted;
    private bool Processing;

    public NotificationMonitor()
    {
        ResultsQueue = new Dictionary<string, Result>();
        RequestsCompleted = new List<string>();
        Processing = false;
    }

    public void DisplayErrorDialog()
    {
        if (Enjin.ErrorReport == null || Enjin.ErrorReport.ErrorCode == 0)
            return;

        DisplayDialog("ERROR " + Enjin.ErrorReport.ErrorCode.ToString(), Enjin.ErrorReport.ErrorMessage);
        Enjin.ResetErrorReport();
    }

    public void ProcessRequests()
    {
        if (Processing) return;

        // Start processing result queue
        Processing = true;
        // handle pending requests on the controller monitoring for result updates
        RequestsCompleted.Clear();

        foreach (KeyValuePair<string, Result> entry in ResultsQueue)
        {
            if (entry.Value.Type == Result.Types.CRYPTOITEMS && CIController != null)
            {
                switch (entry.Value.Status)
                {
                    case Status.SUCCESS:
                        if (entry.Value.compoundQueries.Count > 0)
                        {
                            foreach (CompoundQuery query in entry.Value.compoundQueries)
                            {
                                Request request = CIController.ProcessCryptoItem(query.Task, query.CryptoItem, query.Properties);

                                if (EnjinEditor.IsRequestSuccessfull(request.state))
                                    EditorUtility.DisplayDialog("SUCCESS", "Your previous request contained an additional request which has now posted with a status of " + request.state + ". Please see your wallet to complete the transaction!", "Ok");
                                else
                                    EditorUtility.DisplayDialog("FAILURE", "The request could not be processed due to a status of " + request.state + ".", "Ok");

                                CIController.State = CryptoItemsController.CryptoItemState.MAIN;
                            }
                            RequestsCompleted.Add(entry.Key);
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("SUCCESS", "A pending request has posted successfully. Refresh now?\n\nNote, refreshing will return you to the index and clear any active data entry.", "Refresh", "Cancel"))
                            {
                                EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADITEMS);
                                CIController.Reset();
                            }
                            RequestsCompleted.Add(entry.Key);
                        }
                        break;
                    case Status.FAILURE:
                        if (EditorUtility.DisplayDialog("FAILURE", "A pending request could not be processed due to a status of " + entry.Value.Status + ". Refresh now?\n\nNote, refreshing will return you to the index and clear any active data entry.", "Refresh", "Cancel"))
                        {
                            EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADITEMS);
                            CIController.Reset();
                        }
                        RequestsCompleted.Add(entry.Key);
                        break;
                }
            }
            else if (entry.Value.Type == Result.Types.IDENTITIES)
            {
                // noop -- stub example for future notifications
            } 
            else
            {
                // noop
            }
        }

        if (RequestsCompleted.Count > 0)
            RequestsCompleted.ForEach(e => ResultsQueue.Remove(e));

        // Done processing result queue.
        Processing = false;
    }

    /* NOTE: This method will be upgraded at a later time to handle dialogs
     *       that will use templates for standard messages and allow for an
     *       overridden method for using custom dialog messages.
     */
    public void DisplayDialog(string title, string message)
    {
        EditorUtility.DisplayDialog(title, message, "OK");
    }
}