using UnityEngine;
using System;

public class UIManager : MonoBehaviour {
    public static UIManager Instance;

    [Header("UI")]
    public Panel JoinPanel;
    public Panel HostPanel;
    public Panel PausePanel;
    public Panel ChangeNamePanel;
    public GameObject MainFrame;

    [Header("Settings")]
    [SerializeField] private InputReader inputReader;

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

        MainFrame.SetActive(true);
        PausePanel.gameObject.SetActive(false);
        JoinPanel.gameObject.SetActive(false);
        HostPanel.gameObject.SetActive(false);

        SetInputState(true);

        GameManager.Instance.OnPlayerSpawned += (Player player, bool isOwner) => {
            MainFrame.SetActive(false);
            JoinPanel.Close();
            HostPanel.Close();
            PausePanel.Close();
        };

        GameManager.Instance.OnPause += () => {
            PausePanel.Open();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        };

        GameManager.Instance.OnResume += () => {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        };
    }

    public void TabChanged(TabGroupIndex groupIndex, Tab tab) => OnTabChanged?.Invoke(groupIndex, tab);

    public void ChangeTab(TabGroupIndex groupIndex, string tabName) => OnManualTabChanged?.Invoke(groupIndex, tabName);

    public void UsernameChanged(string username) {
        Username = username;
        OnUsernameChanged?.Invoke(username);
    }

    private void SetInputState(bool enabled) {
        void OnButtonEvent(ButtonType type, bool performed) {
            if (!performed) return;
            if (type == ButtonType.Pause) {
                if (!GameManager.Instance.IsPlayerSpawned || GameManager.Instance.IsPaused) return;
                GameManager.Instance.PauseGame();
            }
        }

        if (enabled) inputReader.ButtonEvent += OnButtonEvent;
        else inputReader.ButtonEvent -= OnButtonEvent;
    }
}

public enum TabGroupIndex {
    Menu, SettingsPanel, JoinPanel, CustomizationPanel, ProfilePanel
}