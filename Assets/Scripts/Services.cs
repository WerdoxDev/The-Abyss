using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using System;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class Services : MonoBehaviour {
    public static Services Instance;

    public event Action<bool> OnInitializeFinished;

    public bool IsSignedIn { get; private set; }
    public bool OfflineMode { get; private set; }

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
            return;
        }
    }

    public async Task<bool> Initialize() {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in" + AuthenticationService.Instance.PlayerId);
        };

#if UNITY_EDITOR
        if (ClonesManager.IsClone()) {
            string customArgument = ClonesManager.GetArgument();
            AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
        }
#endif

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        IsSignedIn = true;
        OfflineMode = false;
        OnInitializeFinished?.Invoke(true);

        return AuthenticationService.Instance.IsAuthorized;
    }

    public void InitializeOffline() {
        OfflineMode = true;
        OnInitializeFinished?.Invoke(false);
    }

    public async Task<Lobby> CreateLobby(string lobbyName, int maxPlayers, string region, string relayCode) {
        try {
            CreateLobbyOptions options = new() {
                Data = new Dictionary<string, DataObject>() {
                    {
                        "Status", new(DataObject.VisibilityOptions.Public, value : "Started", index : DataObject.IndexOptions.S1)
                    },
                    {
                        "Region", new(DataObject.VisibilityOptions.Public, value : region, index : DataObject.IndexOptions.S2)
                    },
                    {
                        "RelayCode", new(DataObject.VisibilityOptions.Member, value: relayCode, index: DataObject.IndexOptions.S3)
                    }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            Debug.Log("Lobby created: " + lobby.Name + " " + lobby.MaxPlayers);
            return lobby;
        }
        catch (LobbyServiceException e) {
            Debug.Log(e);
            return null;
        }
    }

    public async Task<(Allocation, string)> CreateRelay() {
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);

            RelayServerData relayServerData = new(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            TheAbyssNetworkManager.Instance.Host(
                new(UIManager.Instance.PlayerName, UIManager.Instance.CustomizePanel.PlayerCustomization));

            //ChatManager.Instance.SendSystemMessageServerRpc(new("Relay", "Your join code: " + joinCode, GameManager.Instance.SystemMessageColor));

            return (allocation, joinCode);
        }
        catch (RelayServiceException e) {
            Debug.Log(e);
            return (null, null);
        }
    }

    public async Task<Lobby> JoinLobby(string lobbyId) {
        //try {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            return lobby;
        //}
        //catch (LobbyServiceException e) {
        //    Debug.Log(e);
        //    return null;
        //}
    }

    public async void JoinRelay(string joinCode) {
        try {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            TheAbyssNetworkManager.Instance.Client(
                new(UIManager.Instance.PlayerName, UIManager.Instance.CustomizePanel.PlayerCustomization));
        }
        catch (RelayServiceException e) {
            Debug.Log(e);
        }
    }

    public async Task<Lobby[]> GetLobbies() {
        //try {
        QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

        return queryResponse.Results.ToArray();
        //}
        //catch (LobbyServiceException e) {
        //    Debug.Log(e);
        //    return null;
        //}
    }
}
