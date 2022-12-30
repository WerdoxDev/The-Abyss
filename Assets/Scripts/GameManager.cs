using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using System;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    [Header("Settings")]
    [SerializeField] private float pingInterval;
    [SerializeField] private float fpsInterval;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private string tempUrl;
    [SerializeField] private bool autoSpawn;
    private float _fpsTimer;

    public event Action<Player, bool> OnPlayerSpawned;
    public event Action<ulong, bool> OnPlayerDespawned;
    public event Action<int> OnPingChanged;
    public event Action<int> OnFpsChanged;
    public event Action<string> OnSceneChanged;
    public event Action<GameState> OnGameStateChanged;
    public event Action OnPause;
    public event Action OnResume;

    public GameState GameState;
    public ulong PlayerNetworkId;
    public bool IsPlayerSpawned;
    public bool IsPaused;

    public int Ping { get; private set; }
    public int Fps { get; private set; }

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        inputReader.Init();

        OnPlayerSpawned += (Player player, bool isOwner) => {
            if (!isOwner) return;
            IsPlayerSpawned = true;
            PlayerNetworkId = player.OwnerClientId;
            StopAllCoroutines();
            StartCoroutine(CheckPingEnumerator(pingInterval));

            // We need player object so do this on player spawn
            SettingsManager.Instance.CurrentCamera = player.CLCamera.Camera;
            SettingsManager.Instance.ApplyChanges();
        };

        OnPlayerDespawned += (ulong clientId, bool isOwner) => {
            if (!isOwner) return;
            IsPlayerSpawned = false;
            PlayerNetworkId = 0;
            if (gameObject != null) StopAllCoroutines();
        };

        OnSceneChanged += (name) => {
            if (name == "Water") {
                GameStateChanged(GameState.InGame);
            }

            // We don't need player object but we need it to happend after scene changed not after player destroyed
            else if (name == "MainMenu") {
                GameStateChanged(GameState.InMainMenu);
                SettingsManager.Instance.CurrentCamera = Camera.main;
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
            TheAbyssNetworkManager.Instance.Client(new PlayerConnData(UIManager.Instance.Username));
        } else {
            TheAbyssNetworkManager.Instance.Host(new PlayerConnData(UIManager.Instance.Username));
        }
    }
#endif

    private void Update() {
        if (Time.unscaledTime > _fpsTimer) {
            Fps = (int)(1f / Time.unscaledDeltaTime);
            OnFpsChanged?.Invoke(Fps);
            _fpsTimer = Time.unscaledTime + fpsInterval;
        }
    }

    public void PlayerSpawned(Player player, bool isOwner) {
        OnPlayerSpawned?.Invoke(player, isOwner);
    }

    public void PlayerDespawned(ulong clientId) {
        OnPlayerDespawned?.Invoke(clientId, clientId == PlayerNetworkId);
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
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = address;
        transport.ConnectionData.Port = port;
    }

    private IEnumerator CheckPingEnumerator(float intervalSeconds) {
        // UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        while (true) {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.NetworkTickSystem != null) {
                float actualPing = (NetworkManager.Singleton.LocalTime.TimeAsFloat - NetworkManager.Singleton.ServerTime.TimeAsFloat) * 100;
                actualPing -= 1 / NetworkManager.Singleton.NetworkTickSystem.TickRate;
                Ping = Mathf.FloorToInt(actualPing);
                OnPingChanged?.Invoke(Ping);
            }
            yield return new WaitForSeconds(intervalSeconds);
        }
    }
}

public enum GameState {
    InMainMenu,
    InGame
}