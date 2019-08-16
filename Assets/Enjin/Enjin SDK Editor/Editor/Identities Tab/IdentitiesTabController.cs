using UnityEngine;
using UnityEditor;
using EnjinSDK;
using System.Collections.Generic;
using EnjinEditorPanel;

public class IdentitiesTabController
{
    #region Definitions & Properties
    public enum IdentityState { VIEW, EDIT, CREATE }
    //public enum ProcessIdentitiesTasks { VIEW, CREATE, DELETE, EDIT };
    //public enum Status { NONE, SUCCESS, FAILURE };

    public int FirstPage { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public bool IsInSearchMode { get; set; }
    public bool HasRefreshedList { get; set; }
    public int SelectedIndex { get; set; }
    public string SearchText { get; set; }
    public bool[] Pages { get; set; }

    public Identity CurrentIdentity { get; set; }

    public List<Identity> IdentitiesList { get; set; }
    public List<bool> FieldsFoldout { get; set; }
    public PaginationHelper<Identity> Pagination { get; set; }
    public List<string> UserIDs { get; private set; }

    public IdentityState State { get; set; }
    #endregion

    public class Result
    {
        public Request Request { get; set; }
        public Status Status { get; set; }
        public string Message { get; set; }
        public bool Refresh { get; set; }

        public Result()
        {
            Status = Status.NONE;
            Message = "";
            Request = null;
            Refresh = false;
        }
    }

    private static Dictionary<string, Result> _resultQueue;

    public IdentitiesTabController()
    {
        _resultQueue = new Dictionary<string, Result>();
        IdentitiesList = new List<Identity>();
        FieldsFoldout = new List<bool>();
        Pagination = new PaginationHelper<Identity>();
        FirstPage = 1;
        CurrentPage = 1;
        HasRefreshedList = true;
    }

    public Dictionary<string, Result> GetResults()
    {
        return _resultQueue;
    }

    public void AddResult(Result result)
    {
        _resultQueue.Add(result.Request.transaction_id.ToString(), result);
    }

    public void RemoveResult(Result result)
    {
        _resultQueue.Remove(result.Request.transaction_id.ToString());
    }

    public void RemoveResult(string id)
    {
        _resultQueue.Remove(id);
    }

    public void SetUserIDList()
    {
        UserIDs = new List<string>();

        Identity[] userIDList = Enjin.GetAllIdentities();

        foreach (Identity userID in userIDList)
            UserIDs.Add(userID.user.id.ToString());
    }

    /// <summary>
    /// Placeholder for request handling and passing results to a watch action/event to prompt for pop-up notifications
    /// </summary>
    /// <param name="task"></param>
    /// <param name="identity"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    internal Identity ProcessRequest(ProcessTasks task, Identity identity, Dictionary<string, object> properties)
    {
        Identity retv = null;

        switch (task)
        {
            case ProcessTasks.VIEW:
                break;
            case ProcessTasks.CREATE:
                retv = Enjin.CreateIdentity(identity);
                if (retv != null)
                    EditorUtility.DisplayDialog("SUCCESS", "Identity " + retv.id + " has been created for app id " + retv.app_id + ".", "Ok");
                else
                    EditorUtility.DisplayDialog("FAILURE", "Identity " + retv.id + " could not be created. Please see the Unity Console for the error returned.", "Ok");

                RefreshLists();
                State = IdentityState.VIEW;
                break;
            case ProcessTasks.DELETE:
                break;
            case ProcessTasks.EDIT:
                if (CurrentIdentity.linking_code != "")
                    retv = Enjin.UpdateIdentity(CurrentIdentity);
                else
                    retv = Enjin.UpdateIdentityFields(CurrentIdentity.id, CurrentIdentity.fields);

                if (retv != null)
                    EditorUtility.DisplayDialog("SUCCESS", "Identity " + retv.id + " was updated successfully.", "Ok");
                else
                    EditorUtility.DisplayDialog("FAILURE", "Identity " + retv.id + " could not be updated. Please see the Unity Console for the error returned.", "Ok");

                RefreshLists();
                State = IdentityState.VIEW;
                break;
        }

        return retv;
    }

    /// <summary>
    /// Placeholder for logic changes in the next pass -- adds pop-up notifications for completed requests
    /// </summary>
    public void CheckForPopUps()
    {
        #region Popup Notification Handler
        // handle pending requests on the controller monitoring for result updates
        List<string> requestsCompleted = new List<string>();
        foreach (KeyValuePair<string, IdentitiesTabController.Result> entry in GetResults())
        {
            switch (entry.Value.Status)
            {
                case Status.SUCCESS:
                    if (EditorUtility.DisplayDialog("SUCCESS", "A pending request has posted successfully. Refresh now?\n\nNote, refreshing will return you to the index and clear any active data entry.", "Refresh", "Cancel"))
                    {
                        EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADITEMS);
                        RefreshLists();
                    }
                    requestsCompleted.Add(entry.Key);
                    break;
                case Status.FAILURE:
                    if (EditorUtility.DisplayDialog("FAILURE", "A pending request could not be processed due to a status of " + entry.Value.Status + ". Refresh now?\n\nNote, refreshing will return you to the index and clear any active data entry.", "Refresh", "Cancel"))
                    {
                        EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADITEMS);
                        RefreshLists();
                    }
                    requestsCompleted.Add(entry.Key);
                    break;
                case Status.NONE:
                    if (EditorUtility.DisplayDialog("FAILURE", "Something went wrong and the status of this request was not set.\n The request returned a status of " + entry.Value.Status + ". Refresh now?\n\n note: refreshing will return you to the index and clear any data entry.", "Refresh", "Cancel"))
                    {
                        EnjinEditor.ExecuteMethod(EnjinEditor.CallMethod.RELOADITEMS);
                        RefreshLists();
                    }
                    requestsCompleted.Add(entry.Key);
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// Method to reset and refresh the list of Identities
    /// </summary>
    public void RefreshLists()
    {
        ResetLists();
        CurrentPage = FirstPage;
        Pagination = Enjin.GetIdentities(CurrentPage, EnjinEditor.ItemsPerPage);
        TotalPages = (int)Mathf.Ceil((float)Pagination.cursor.total / (float)Pagination.cursor.perPage);
        IdentitiesList = new List<Identity>(Pagination.items);

        for (int i = 0; i < IdentitiesList.Count; i++)
            FieldsFoldout.Add(false);
    }

    /// <summary>
    /// Method to clear the Identities List
    /// </summary>
    public void ResetLists()
    {
        if (IdentitiesList != null)
            IdentitiesList.Clear();

        if (FieldsFoldout != null)
            FieldsFoldout.Clear();
    }

    /// <summary>
    /// Resets the search field
    /// </summary>
    public void ResetSearch() { SearchText = ""; IsInSearchMode = false; GUI.FocusControl(null); }

    public void PageCheck()
    {
        if (CurrentPage < FirstPage)
            FirstPage = CurrentPage;

        if (CurrentPage == (FirstPage + 10))
            FirstPage++;

        Pagination = Enjin.GetIdentities(CurrentPage, EnjinEditor.ItemsPerPage);
        IdentitiesList = new List<Identity>(Pagination.items);
        SelectedIndex = 0;
    }
}