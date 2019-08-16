using UnityEngine;
using EnjinEditorPanel;

[System.Serializable]
public class UserPrefSettings
{
    public int ItemsPerPage;

    public UserPrefSettings()
    {
        ItemsPerPage = 15;
    }

    public void Update()
    {
        Debug.Log(JsonUtility.ToJson(this));
        PlayerPrefs.SetString("UserPrefs", JsonUtility.ToJson(this));
    }
}

public class SettingsController
{
    public enum AllowanceErrors { NOTLINKED, INVALIDADDRESS, NONE }
    public string Version { get; private set; }
    public bool FieldsFoldout { get; set; }
    public bool GeneralFoldout { get; set; }
    public bool ItemsFoldout { get; set; }
    public bool IsAllowanceApproved { get; private set; }
    public AllowanceErrors AllowanceError { get; private set; }
    public UserPrefSettings UserSettings { get; set; }

    private string _allowanceURL;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public SettingsController()
    {
        FieldsFoldout = false;
        GeneralFoldout = true;
        ItemsFoldout = false;
        UserSettings = new UserPrefSettings();
        AllowanceError = AllowanceErrors.NONE;
        CheckUserPrefs();
        GetVersion();
        CheckAllowance();
    }

    private void CheckUserPrefs()
    {
        if (PlayerPrefs.HasKey("UserPrefs"))
        {
            Debug.Log("User prefs found");
            var prefs = JsonUtility.FromJson<UserPrefSettings>(PlayerPrefs.GetString("UserPrefs"));
            UserSettings.ItemsPerPage = prefs.ItemsPerPage;
        }
    }

    private void GetVersion()
    {
        TextAsset file = Resources.Load<TextAsset>("Version");
        Version = file.text.Trim();
    }

    private void CheckAllowance()
    {
        AllowanceError = AllowanceErrors.NONE;

        if (EnjinEditor.CurrentUserIdentity.linking_code != null && EnjinEditor.CurrentUserIdentity.linking_code != string.Empty)
        {
            AllowanceError = AllowanceErrors.NOTLINKED;
            return;
        }

        EnjinEditor.Log("Checking Allowance");

        string allowance = "";
        allowance = EnjinEditor.GetAllowance(EnjinEditor.CurrentUserIdentity.ethereum_address);

        if (allowance.Contains("502"))
            AllowanceError = AllowanceErrors.INVALIDADDRESS;
        else if (allowance == "0")
            IsAllowanceApproved = false;
        else
            IsAllowanceApproved = true;
    }

    public void UpdateAllowance() { CheckAllowance(); }
}