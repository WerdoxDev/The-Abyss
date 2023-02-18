using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class JoinPanel : MonoBehaviour {
    [Header("Lobby Panel")]
    [SerializeField] Transform lobbyHolder;
    [SerializeField] GameObject lobbyPrefab;
    [SerializeField] CustomButton refreshButton;

    [Header("Custom Panel")]
    [SerializeField] private CustomButton customJoinButton;
    [SerializeField] private TMP_InputField serverNameInputField;
    [SerializeField] private TMP_InputField serverUrlInputField;

    private void OnEnable() => RefreshLobbies();

    private void Awake() {
        customJoinButton.OnClick += () => {
            string[] split = serverUrlInputField.text.Split(":");

            if (split.Length != 2) return;
            GameManager.Instance.SetConnectionData(split[0], ushort.Parse(split[1]));
            TheAbyssNetworkManager.Instance.Client(
                new PlayerConnData(UIManager.Instance.PlayerName, UIManager.Instance.CustomizePanel.PlayerCustomization));
        };

        refreshButton.OnClick += () => {
            RefreshLobbies();
        };
    }

    private async void RefreshLobbies() {
        foreach (Transform child in lobbyHolder) Destroy(child.gameObject);

        Lobby[] lobbies = await Services.Instance.GetLobbies();

        foreach (Lobby lobby in lobbies) {
            GameObject lobbyObj = Instantiate(lobbyPrefab, lobbyHolder);
            LobbyInfo lobbyInfo = lobbyObj.GetComponent<LobbyInfo>();
            lobbyInfo.SetInfo(lobby.Name, lobby.MaxPlayers, lobby.MaxPlayers - lobby.AvailableSlots, lobby.Data["Status"].Value, lobby.Data["Region"].Value, lobby.Id);
        }
    }
}
