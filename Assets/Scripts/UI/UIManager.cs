using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {
    public static UIManager Instance;

    [Header("UI")]
    public Panel JoinPanel;
    public Panel HostPanel;
    public PausePanel PausePanel;
    public Panel ChangeNamePanel;
    public StatsPanel StatsPanel;
    public ProfileCustomizePanel CustomizePanel;
    public InteractionPanel InteractionPanel;
    public GameObject ChatPanel;
    public GameObject KeybindLegendGO;
    public GameObject MainFrame;

    [Header("Settings")]
    [SerializeField] private GameObject eventSystem;
    [SerializeField] private GameObject playerNamePrefab;
    private Camera _previewCamera;

    public RawImage RenderImage;
    public InputReader InputReader;
    public Camera UICamera;
    public string PlayerName;

    [Header("Autoset Fields")]
    public PlayerCustomization PlayerCustomization;

    public event Action<TabGroupIndex, Tab, Action> OnChangeTabAttempt;
    public event Action<TabGroupIndex, Tab> OnTabChanged;
    public event Action<Panel, bool, Action> OnPanelChangeStateAttempt;
    public event Action<string> OnPlayerNameChanged;

    private List<TabGroup> _tabGroups;
    private List<Panel> _openedPanels = new List<Panel>();
    private CustomButton _enteredCustomButton;
    private AdvancedCustomButton _enteredAdvCustomButton;
    private Dictionary<string, PlayerNamePanel> _playerNames = new Dictionary<string, PlayerNamePanel>();

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

        OnTabChanged += (groupIndex, tab) => {
            if (PlayerCustomization == null) return;

            TabGroup profileTabGroup = _tabGroups?.FirstOrDefault(x =>
                x.GroupIndex == TabGroupIndex.ProfilePanel &&
                x.ActiveTab != null &&
                x.ActiveTab.Name == "Customization");

            if (profileTabGroup != null) {
                _previewCamera.gameObject.SetActive(true);
                // AddPlayerName(PlayerCustomization.transform, PlayerName, PlayerCustomization.NameOffset, true, _previewCamera, CustomizePanel.RawImageTransform);
            } else {
                _previewCamera.gameObject.SetActive(false);
                RemovePlayerName(PlayerName);
            }
        };

        GameManager.Instance.OnPause += () => {
            PausePanel.Panel.Open();
            KeybindLegendGO.SetActive(true);
            GameManager.Instance.FreeCursor();
        };

        GameManager.Instance.OnResume += () => {
            PausePanel.Panel.Close();
            KeybindLegendGO.SetActive(false);
            GameManager.Instance.LockCursor();
        };

        Action onMainMenuState = () => {
            MainFrame.SetActive(true);
            KeybindLegendGO.SetActive(true);
            StatsPanel.gameObject.SetActive(true);
            JoinPanel.Close(true);
            HostPanel.Close(true);
            PausePanel.Panel.Close(true);
            ChatPanel.SetActive(false);

            InteractionPanel.ClearTarget();

            _previewCamera = GameObject.FindGameObjectWithTag("PreviewCamera").GetComponent<Camera>();
            _previewCamera.gameObject.SetActive(false);
            PlayerCustomization = GameObject.FindGameObjectWithTag("PreviewPlayer").GetComponent<PlayerCustomization>();

            GameManager.Instance.IsPaused = false;
            ChangeTab(TabGroupIndex.Menu, "Home");

            GameManager.Instance.FreeCursor();
        };

        onMainMenuState();

        KeybindLegend.Instance.OnBackButtonClicked += () => InputReader.SendUIButtonEvent(UIButtonType.Cancel);

        GameManager.Instance.OnGameStateChanged += (state) => {
            if (state == GameState.InGame) {
                MainFrame.SetActive(false);
                KeybindLegendGO.SetActive(false);
                JoinPanel.Close(true);
                HostPanel.Close(true);
                PausePanel.Panel.Close(true);
            }
            if (state == GameState.InMainMenu) {
                onMainMenuState();
            };
        };

        SetRenderTexure(Camera.main.targetTexture);
    }

    public void SetRenderTexure(RenderTexture renderTexture) {
        RenderImage.texture = renderTexture;
    }

    public void AddTabGroup(TabGroup tabGroup) {
        if (_tabGroups == null) _tabGroups = new List<TabGroup>();
        if (_tabGroups.Exists(x => x.GroupIndex == tabGroup.GroupIndex)) return;
        _tabGroups.Add(tabGroup);
    }

    public void AddPlayerName(Transform player, string name, Vector3 offset, bool ignoreEqual = false, Camera customCamera = null, RectTransform customRect = null) {
        if (name == PlayerName && !ignoreEqual) return;
        if (_playerNames.ContainsKey(name)) return;
        GameObject playerNameGO = Instantiate(playerNamePrefab, Vector3.zero, Quaternion.identity, transform);
        PlayerNamePanel playerNamePanel = playerNameGO.GetComponent<PlayerNamePanel>();
        playerNamePanel.SetPlayer(player, name, offset, customCamera, customRect);
        _playerNames.Add(name, playerNamePanel);
    }

    public void RemovePlayerName(string name) {
        PlayerNamePanel playerNamePanel = _playerNames.FirstOrDefault(x => x.Key == name).Value;
        if (playerNamePanel == null) return;
        _playerNames.Remove(name);
        Destroy(playerNamePanel.gameObject);
    }

    public void ChangeTab(TabGroupIndex groupIndex, string tabName) => GetTabGroupByIndex(groupIndex)?.SelectTabByName(tabName);

    public void ChangeTabAttempt(TabGroupIndex groupIndex, Tab tab, Action cancel) => OnChangeTabAttempt?.Invoke(groupIndex, tab, cancel);
    public void PanelChangeStateAttempt(Panel panel, bool isOpen, Action cancel) => OnPanelChangeStateAttempt?.Invoke(panel, isOpen, cancel);

    public void TabChanged(TabGroupIndex groupIndex, Tab tab) => OnTabChanged?.Invoke(groupIndex, tab);

    public void PlayerNameChanged(string playerName) {
        PlayerName = playerName;
        OnPlayerNameChanged?.Invoke(playerName);
    }

    public void AddOpenedPanel(Panel panel) => _openedPanels.Add(panel);
    public void RemoveOpenedPanel(Panel panel) => _openedPanels.Remove(panel);
    public void SetEnteredCustomButton(CustomButton customButton) => _enteredCustomButton = customButton;
    public void SetEnteredAdvCustomButton(AdvancedCustomButton advCustomButton) => _enteredAdvCustomButton = advCustomButton;

    public Vector3 WorldToScreen(Vector3 worldPosition, Camera camera = null, RectTransform rect = null) {
        if (camera == null) camera = SettingsManager.Instance.CurrentCamera;
        if (camera == null) return Vector3.zero;

        if (rect == null) rect = RenderImage.rectTransform;

        Vector2 viewPos = camera.WorldToViewportPoint(worldPosition);
        Vector2 localPos = new Vector2(viewPos.x * rect.sizeDelta.x, viewPos.y * rect.sizeDelta.y);
        Vector3 worldPos = rect.TransformPoint(localPos);
        float scalerRatio = (1 / this.transform.lossyScale.x) * 2;

        return new Vector3
            (worldPos.x - rect.sizeDelta.x / scalerRatio,
             worldPos.y - rect.sizeDelta.y / scalerRatio, 1f);
    }

    public bool IsInCameraBounds(Vector3 position, Camera camera = null) {
        if (camera == null) camera = SettingsManager.Instance.CurrentCamera;
        if (camera == null) return false;

        return Vector3.Dot(camera.transform.forward, position - camera.transform.position) >= 0;
    }

    private TabGroup GetTabGroupByIndex(TabGroupIndex groupIndex) => _tabGroups?.FirstOrDefault(x => x.GroupIndex == groupIndex);

    private void SetInputState(bool enabled) {
        bool buttonPauseInvoked = false;

        void OnButtonEvent(ButtonType type, bool performed) {
            if (GameManager.Instance.IsPaused || GameManager.Instance.GameState == GameState.InMainMenu) return;

            if (!performed) return;
            if (type == ButtonType.Pause) {
                if (ChatManager.Instance.IsOpen) {
                    ChatManager.Instance.Close();
                    GameManager.Instance.PlayerObject.EnableKeybinds();
                    return;
                }

                buttonPauseInvoked = true;
                if (!GameManager.Instance.IsPlayerSpawned || GameManager.Instance.IsPaused || !PausePanel.Panel.FullyClosed) return;
                GameManager.Instance.PauseGame();
            } else
            if (type == ButtonType.Chat) {
                if (GameManager.Instance.PlayerObject == null) return;
                if (_openedPanels.Count != 0) return;
                Debug.Log("here???");

                if (!ChatManager.Instance.IsOpen) {
                    ChatManager.Instance.Open(true, false);
                    GameManager.Instance.FreeCursor();
                } else {
                    ChatManager.Instance.TrySendMessage();
                }
            }
        }

        void OnUIButtonEvent(UIButtonType type, bool performed) {
            if (!GameManager.Instance.IsPaused && GameManager.Instance.GameState != GameState.InMainMenu) return;

            if (!performed) return;
            if (type == UIButtonType.Cancel) {
                if (buttonPauseInvoked) {
                    buttonPauseInvoked = false;
                    return;
                }

                Panel openPanelOnTop = _openedPanels.Count == 0 ? null : _openedPanels[_openedPanels.Count - 1];
                if (openPanelOnTop != null && openPanelOnTop.CloseOnCancel) {
                    if (openPanelOnTop == PausePanel.Panel) {
                        if (!PausePanel.IsMenuVisible()) PausePanel.ShowMenuPanel();
                        else if (PausePanel.Panel.FullyOpened) GameManager.Instance.ResumeGame();
                        return;
                    }

                    openPanelOnTop.Close();
                    return;
                }

                if (_enteredCustomButton != null) {
                    _enteredCustomButton.Cancel();
                    _enteredCustomButton = null;
                    return;
                }

                if (_enteredAdvCustomButton != null) {
                    _enteredAdvCustomButton.Cancel();
                    _enteredAdvCustomButton = null;
                    return;
                }

                if (GetTabGroupByIndex(TabGroupIndex.Menu)?.ActiveTab?.name == "Home") { return; } // Prompt a exit confirm
                ChangeTab(TabGroupIndex.Menu, "Home");
            }
            if (type == UIButtonType.Submit) {
                Tab enteredTab = _tabGroups.FirstOrDefault(x => x.EnteredTab != null)?.EnteredTab;
                if (enteredTab != null) {
                    enteredTab.Submit();
                    return;
                }

                if (_enteredCustomButton != null) {
                    _enteredCustomButton.Submit();
                    return;
                }

                if (_enteredAdvCustomButton != null) {
                    _enteredAdvCustomButton.Submit();
                    return;
                }

                Panel openPanelOnTop = _openedPanels.Count == 0 ? null : _openedPanels[_openedPanels.Count - 1];
                if (openPanelOnTop != null) {
                    openPanelOnTop.Submit();
                    return;
                }
            }
        }

        void OnChangeTab(int direction) {
            if (!GameManager.Instance.IsPaused && GameManager.Instance.GameState == GameState.InGame) return;
            if (_openedPanels.Count != 0 && !JoinPanel.IsOpen) return;
            TabGroupIndex groupIndex = JoinPanel.IsOpen ? TabGroupIndex.JoinPanel : TabGroupIndex.Menu;
            if (direction == 1) GetTabGroupByIndex(groupIndex)?.SelectNextTab();
            else if (direction == -1) GetTabGroupByIndex(groupIndex)?.SelectPreviousTab();
        }

        if (enabled) {
            InputReader.ButtonEvent += OnButtonEvent;
            InputReader.UIButtonEvent += OnUIButtonEvent;
            InputReader.UIChangeTabEvent += OnChangeTab;
        } else {
            InputReader.ButtonEvent -= OnButtonEvent;
            InputReader.UIButtonEvent -= OnUIButtonEvent;
            InputReader.UIChangeTabEvent -= OnChangeTab;
        }
    }
}

public enum TabGroupIndex {
    Menu, SettingsPanel, InGameSettingsPanel, JoinPanel, CustomizationPanel, ProfilePanel
}
