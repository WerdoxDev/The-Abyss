using UnityEngine;
using System;

public class UIManager : MonoBehaviour {
    public static UIManager Instance;

    public Panel JoinPanel;
    public Panel HostPanel;
    public Panel ChangeNamePanel;
    public GameObject MainFrame;

    public event Action<TabGroupIndex, Tab> OnTabChanged;
    public event Action<TabGroupIndex, string> OnManualTabChanged;
    public event Action<string> OnUsernameChanged;

    public string Username;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        GameManager.Instance.OnPlayerSpawned += (Player player, bool isOwner) => {
            MainFrame.SetActive(false);
            JoinPanel.Close();
            HostPanel.Close();
        };

        JoinPanel.gameObject.SetActive(false);
        HostPanel.gameObject.SetActive(false);
    }

    public void TabChanged(TabGroupIndex groupIndex, Tab tab) => OnTabChanged?.Invoke(groupIndex, tab);

    public void ChangeTab(TabGroupIndex groupIndex, string tabName) => OnManualTabChanged?.Invoke(groupIndex, tabName);

    public void UsernameChanged(string username) {
        Username = username;
        OnUsernameChanged?.Invoke(username);
    }
}

public enum TabGroupIndex {
    Menu, SettingsPanel, JoinPanel, CustomizationPanel, ProfilePanel
}