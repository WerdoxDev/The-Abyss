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
    [SerializeField] private string tempUrl;
    private float _fpsTimer;

    public event Action<Player, bool> OnPlayerSpawned;
    public event Action<ulong> OnPlayerDespawned;
    public event Action<int> OnPingChanged;
    public event Action<int> OnFpsChanged;

    public ulong PlayerNetworkId;
    public int Ping { get; private set; }
    public int Fps { get; private set; }

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        OnPlayerSpawned += (Player player, bool isOwner) => {
            if (!isOwner) return;
            Debug.Log("all good");
            PlayerNetworkId = player.OwnerClientId;
            StopAllCoroutines();
            StartCoroutine(CheckPingEnumerator(pingInterval));
        };

        OnPlayerDespawned += (ulong clientId) => {
            if (clientId != PlayerNetworkId) return;
            PlayerNetworkId = 0;
            StopAllCoroutines();
        };
    }

#if UNITY_EDITOR
    // Temporary
    private void Start() {
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
        OnPlayerDespawned?.Invoke(clientId);
    }

    public void SetConnectionData(string address, ushort port) {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = address;
        transport.ConnectionData.Port = port;
    }

    private IEnumerator CheckPingEnumerator(float intervalSeconds) {
        // UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        while (true) {
            float actualPing = (NetworkManager.Singleton.LocalTime.TimeAsFloat - NetworkManager.Singleton.ServerTime.TimeAsFloat) * 100;
            actualPing -= 1 / NetworkManager.Singleton.NetworkTickSystem.TickRate;
            Ping = Mathf.FloorToInt(actualPing);
            // Debug.Log();
            // Ping = NetworkManager.Singleton.LocalTime.TimeAsFloat - NetworkManager.Singleton.ServerTime.TimeAsFloat;
            OnPingChanged?.Invoke(Ping);
            yield return new WaitForSeconds(intervalSeconds);
        }
    }
}
