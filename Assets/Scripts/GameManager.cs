using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using System;
using Netcode.Transports.Ruffles;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    [Header("Settings")]
    [SerializeField] private float fpsInterval;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private string tempUrl;
    [SerializeField] private bool autoSpawn;
    private float _fpsTimer;

    public event Action<Player, bool> OnPlayerSpawned;
    public event Action<Player, bool> OnPlayerDespawned;
    public event Action<string> OnSceneChanged;
    public event Action<GameState> OnGameStateChanged;
    public event Action OnPause;
    public event Action OnResume;

    public Color SystemMessageColor;
    public GameState GameState;
    public ulong PlayerNetworkId;
    public Player PlayerObject;
    public bool IsPlayerSpawned;
    public bool IsPaused;

    private void OnDestroy() {
        SetInternalEventsState(false);
    }

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        inputReader.Init();

        SetInternalEventsState(true);

        OnSceneChanged += (name) => {
            if (name == "Game") {
                GameStateChanged(GameState.InGame);
            }

            // We don't need player object but we need it to happend after scene changed not after player destroyed
            else if (name == "MainMenu") {
                GameStateChanged(GameState.InMainMenu);
                SettingsManager.Instance.CurrentCamera = Camera.main;
                SettingsManager.Instance.CurrentWaterGenerator = FindFirstObjectByType<WaterGenerator>();
                SettingsManager.Instance.ApplyChanges();
            }
        };

        GameStateChanged(GameState.InMainMenu);
    }

#if UNITY_EDITOR
    // Temporary
    private void Start() {
        if (!autoSpawn) return;
        string[] split = tempUrl.Split(":");
        SetConnectionData(split[0], ushort.Parse(split[1]));
        if (ClonesManager.IsClone()) {
            TheAbyssNetworkManager.Instance.Client(
                new(UIManager.Instance.PlayerName, UIManager.Instance.CustomizePanel.PlayerCustomization));
        }
        else {
            TheAbyssNetworkManager.Instance.Host(
                new(UIManager.Instance.PlayerName, UIManager.Instance.CustomizePanel.PlayerCustomization));
        }
    }
#endif

    public void LockCursor() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void FreeCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayerSpawned(Player player, bool isOwner) {
        OnPlayerSpawned?.Invoke(player, isOwner);
    }

    public void PlayerDespawned(Player player) {
        OnPlayerDespawned?.Invoke(player, player.OwnerClientId == PlayerNetworkId);
    }

    public void PauseGame() {
        IsPaused = true;
        OnPause?.Invoke();
    }

    public void ResumeGame() {
        IsPaused = false;
        OnResume?.Invoke();
    }

    public void SceneChanged(string sceneName) => OnSceneChanged?.Invoke(sceneName);

    public void GameStateChanged(GameState newState) {
        GameState = newState;
        OnGameStateChanged?.Invoke(newState);
    }

    public void SetConnectionData(string address, ushort port) {
        //RufflesTransport transport = NetworkManager.S+ingleton.GetComponent<RufflesTransport>();
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        //transport.ConnectAddress = address;
        //transport.Port = port;
        transport.ConnectionData.Address = address;
        transport.ConnectionData.Port = port;
    }

    public PlayerDataInfo GetPlayerDataInfo() => PlayerObject.Data.PlayerDataInfo.Value;

    private void SetInternalEventsState(bool enabled) {
        void PlayerSpawned(Player player, bool isOwner) {
            if (NetworkManager.Singleton.IsServer) {
                PlayerDataInfo dataInfo = player.Data.PlayerDataInfo.Value;
                ChatManager.Instance.SendSystemMessageServerRpc(new("[System]", dataInfo.DisplayName + " Joined the game", SystemMessageColor));
            }

            if (!isOwner) return;
            PlayerObject = player;
            IsPlayerSpawned = true;
            PlayerNetworkId = player.OwnerClientId;
            StopAllCoroutines();

            // We need player object so do this on player spawn
            SettingsManager.Instance.CurrentCamera = player.CLCamera.Camera;
            SettingsManager.Instance.CurrentWaterGenerator = FindFirstObjectByType<WaterGenerator>();
            SettingsManager.Instance.ApplyChanges();
        }

        void PlayerDespawned(Player player, bool isOwner) {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer && !isOwner) {
                if (player.Data == null) return;
                PlayerDataInfo dataInfo = player.Data.PlayerDataInfo.Value;
                ChatManager.Instance.SendSystemMessageServerRpc(new("[System]", dataInfo.DisplayName + " Left the game", SystemMessageColor));
            }

            if (!isOwner) return;
            IsPlayerSpawned = false;
            PlayerNetworkId = 0;
            PlayerObject = null;
            if (gameObject != null) StopAllCoroutines();
        }

        if (enabled) {
            OnPlayerSpawned += PlayerSpawned;
            OnPlayerDespawned += PlayerDespawned;
        }
        else {
            OnPlayerSpawned -= PlayerSpawned;
            OnPlayerDespawned -= PlayerDespawned;
        }
    }
}

public enum GameState {
    InMainMenu,
    InGame,
    InStart
}