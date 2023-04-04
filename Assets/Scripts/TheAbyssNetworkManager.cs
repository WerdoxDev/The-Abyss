using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TheAbyssNetworkManager : MonoBehaviour {
    public static TheAbyssNetworkManager Instance;

    [SerializeField] private GameObject playerPrefab;

    private Dictionary<ulong, PlayerConnData> _clientData;

    public event Action<ulong> OnLocalClientConnected;
    public event Action OnClientConnectionStarted;
    public event Action OnHostConnectionStarted;
    public event Action<bool> OnClientDisconnected;

    private bool _wasConnected;

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
            return;
        }
    }

    private void Start() {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        SceneManager.sceneLoaded += OnSceneLoaded;
        //Only for when we are switching from menu to game        
    }

    public void Host(PlayerConnData playerData) {
        _clientData = new Dictionary<ulong, PlayerConnData>();

        if (playerData.PlayerCustomization.Equals(default))
            playerData.PlayerCustomization = PlayerCustomizationInfo.Default;

        NetworkManager.Singleton.NetworkConfig.ConnectionData = GetConnectionData(playerData);
        OnHostConnectionStarted?.Invoke();
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleSceneLoadComplete;
        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void Client(PlayerConnData playerData) {
        if (playerData.PlayerCustomization.Equals(default))
            playerData.PlayerCustomization = PlayerCustomizationInfo.Default;

        NetworkManager.Singleton.NetworkConfig.ConnectionData = GetConnectionData(playerData);
        OnClientConnectionStarted?.Invoke();
        NetworkManager.Singleton.StartClient();
    }

    public void Disconnect() {
        NetworkManager.Singleton.Shutdown();
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= HandleSceneLoadComplete;

        if (_wasConnected) SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        _wasConnected = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        GameManager.Instance.SceneChanged(scene.name);
    }

    private void HandleClientConnected(ulong clientId) {
        // Are we the client?
        if (clientId == NetworkManager.Singleton.LocalClientId) {
            OnLocalClientConnected?.Invoke(clientId);
            _wasConnected = true;
        }
    }

    private void HandleClientDisconnected(ulong clientId) {
        if (NetworkManager.Singleton.IsServer) _clientData.Remove(clientId);
        else if (!NetworkManager.Singleton.IsHost) Disconnect();

        OnClientDisconnected?.Invoke(_wasConnected);
        _wasConnected = false;
    }

    private void HandleSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode) {
        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        string payload = Encoding.ASCII.GetString(request.Payload);
        var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

        // For later use cases
        bool approveConnection = true;

        if (approveConnection) {
            _clientData.Add(request.ClientNetworkId, new PlayerConnData(connectionPayload.PlayerName, connectionPayload.PlayerCustomization));
        }

        response.CreatePlayerObject = false;
        response.Approved = approveConnection;
    }

    private byte[] GetConnectionData(PlayerConnData playerData) {
        string payload = JsonUtility.ToJson(new ConnectionPayload() {
            PlayerName = playerData.PlayerName,
            PlayerCustomization = playerData.PlayerCustomization
        });

        return Encoding.ASCII.GetBytes(payload);
    }

    public PlayerConnData? GetPlayerData(ulong clientId) {
        if (_clientData.TryGetValue(clientId, out PlayerConnData playerData)) return playerData;
        return null;
    }
}

public struct PlayerConnData {
    public string PlayerName { get; private set; }
    public PlayerCustomizationInfo PlayerCustomization;

    public PlayerConnData(string playerName, PlayerCustomizationInfo playerCustomization) {
        PlayerName = playerName;
        PlayerCustomization = playerCustomization;
    }
}

[System.Serializable]
public class ConnectionPayload {
    public string PlayerName;
    public PlayerCustomizationInfo PlayerCustomization;
}