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

    private Dictionary<ulong, PlayerConnData> clientData;

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
        clientData = new Dictionary<ulong, PlayerConnData>();

        NetworkManager.Singleton.NetworkConfig.ConnectionData = GetConnectionData(playerData);
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.OnLoadComplete += HandleSceneLoadComplete;
        NetworkManager.Singleton.SceneManager.LoadScene("Water", LoadSceneMode.Single);
    }

    public void Client(PlayerConnData playerData) {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = GetConnectionData(playerData);
        NetworkManager.Singleton.StartClient();
    }

    public void Disconnect() {
        NetworkManager.Singleton.Shutdown();
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= HandleSceneLoadComplete;

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        GameManager.Instance.SceneChanged(scene.name);
    }

    private byte[] GetConnectionData(PlayerConnData playerData) {
        string payload = JsonUtility.ToJson(new ConnectionPayload() {
            playerName = playerData.PlayerName
        });

        return Encoding.ASCII.GetBytes(payload);
    }

    private void HandleClientConnected(ulong clientId) {
        // Are we the client?
        if (clientId == NetworkManager.Singleton.LocalClientId) {
            //
        }
    }

    private void HandleClientDisconnected(ulong clientId) {
        if (NetworkManager.Singleton.IsServer) clientData.Remove(clientId);
        if (NetworkManager.Singleton.IsClient) Disconnect();
    }

    public PlayerConnData? GetPlayerData(ulong clientId) {
        if (clientData.TryGetValue(clientId, out PlayerConnData playerData)) return playerData;
        return null;
    }

    private void HandleSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode) {
        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        // NetworkManager.Singleton.PrefabHandler;?\
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        string payload = Encoding.ASCII.GetString(request.Payload);
        var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

        // For later use cases
        bool approveConnection = true;

        Vector3 spawnPos = Vector3.zero;

        if (approveConnection) {
            spawnPos = new Vector3(NetworkManager.Singleton.ConnectedClients.Count * 2, 0, 0);
            clientData.Add(request.ClientNetworkId, new PlayerConnData(connectionPayload.playerName));
        }

        response.CreatePlayerObject = false;
        response.Approved = approveConnection;
        response.Position = spawnPos;
        response.Rotation = Quaternion.identity;
    }
}

public struct PlayerConnData {
    public string PlayerName { get; private set; }

    public PlayerConnData(string playerName) {
        PlayerName = playerName;
    }
}

[System.Serializable]
public class ConnectionPayload {
    public string playerName;
}