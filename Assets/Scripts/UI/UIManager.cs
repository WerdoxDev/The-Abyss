using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {
    public static UIManager Instance;

    [Header("UI")]
    public Panel JoinPanel;
    public Panel HostPanel;
    public Panel PausePanel;
    public Panel ChangeNamePanel;
    public StatsPanel StatsPanel;
    public GameObject KeybindLegend;
    public GameObject MainFrame;

    [Header("Settings")]
    [SerializeField] private GameObject eventSystem;
    [SerializeField] private RawImage renderImage;
    public InputReader InputReader;
    public Camera UICamera;

    public event Action<TabGroupIndex, Tab> OnTabChanged;
    public event Action<string> OnUsernameChanged;

    public List<TabGroup> _tabGroups;

    public string Username;

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        eventSystem.SetActive(true);

        SetInputState(true);

        GameManager.Instance.OnPause += () => {
            PausePanel.Open();
            KeybindLegend.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        };

        GameManager.Instance.OnResume += () => {
            KeybindLegend.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        };

        Action onMainMenuState = () => {
            MainFrame.SetActive(true);
            KeybindLegend.SetActive(true);
            StatsPanel.gameObject.SetActive(true);
            JoinPanel.Close(true);
            HostPanel.Close(true);
            PausePanel.Close(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        };

        onMainMenuState();

        GameManager.Instance.OnGameStateChanged += (state) => {
            if (state == GameState.InGame) {
                MainFrame.SetActive(false);
                KeybindLegend.SetActive(false);
                JoinPanel.Close(true);
                HostPanel.Close(true);
                PausePanel.Close(true);
            }
            if (state == GameState.InMainMenu) {
                onMainMenuState();
            };
        };

        SetRenderTexure(Camera.main.targetTexture);
    }

    public void SetRenderTexure(RenderTexture renderTexture) {
        renderImage.texture = renderTexture;
    }

    public void AddTabGroup(TabGroup tabGroup) {
        if (_tabGroups == null) _tabGroups = new List<TabGroup>();
        if (_tabGroups.Exists(x => x.GroupIndex == tabGroup.GroupIndex)) return;
        _tabGroups.Add(tabGroup);
    }

    public void TabChanged(TabGroupIndex groupIndex, Tab tab) => OnTabChanged?.Invoke(groupIndex, tab);

    public void ChangeTab(TabGroupIndex groupIndex, string tabName) =>
        _tabGroups.FirstOrDefault(x => x.GroupIndex == groupIndex)?.SelectTabByName(tabName);

    public void SetBeforeTabChange(TabGroupIndex groupIndex, Action<Action> onBeforeTabChange) =>
        _tabGroups.FirstOrDefault(x => x.GroupIndex == groupIndex)?.SetBeforeTabChange(onBeforeTabChange);

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

        void OnChangeTab(int direction) {
            TabGroupIndex groupIndex = JoinPanel.IsOpen ? TabGroupIndex.JoinPanel : TabGroupIndex.Menu;
            if (direction == 1) _tabGroups.FirstOrDefault(x => x.GroupIndex == groupIndex)?.SelectNextTab();
            else if (direction == -1) _tabGroups.FirstOrDefault(x => x.GroupIndex == groupIndex)?.SelectPreviousTab();
        }

        if (enabled) {
            InputReader.ButtonEvent += OnButtonEvent;
            InputReader.UIChangeTabEvent += OnChangeTab;
        } else {
            InputReader.ButtonEvent -= OnButtonEvent;
            InputReader.UIChangeTabEvent -= OnChangeTab;
        }
    }
}

public enum TabGroupIndex {
    Menu, SettingsPanel, InGameSettingsPanel, JoinPanel, CustomizationPanel, ProfilePanel
}
