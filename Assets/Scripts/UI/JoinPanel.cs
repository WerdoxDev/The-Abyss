using TMPro;
using Unity.Services.Lobbies;
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
    [SerializeField] private TMP_Text infoText;

    private void OnEnable() => RefreshLobbies();

    private void Awake() {
        customJoinButton.OnClick += () => {
            string[] split = serverUrlInputField.text.Split(":");

            if (split.Length != 2) return;

            UIManager.Instance.LoadingPanel.OpenJoinLoading($"{split[0]}:{split[1]}");
            GameManager.Instance.SetConnectionData(split[0], ushort.Parse(split[1]));
            TheAbyssNetworkManager.Instance.Client(
                new PlayerConnData(UIManager.Instance.PlayerName, UIManager.Instance.CustomizePanel.PlayerCustomization));
        };

        refreshButton.OnClick += () => {
            RefreshLobbies();
        };

        infoText.gameObject.SetActive(false);
    }

    private async void RefreshLobbies() {
        infoText.gameObject.SetActive(false);
        foreach (Transform child in lobbyHolder) Destroy(child.gameObject);

        try {
            Lobby[] lobbies = await Services.Instance.GetLobbies();

            if (lobbies.Length == 0) {
                infoText.text = "No lobbies found.";
                infoText.gameObject.SetActive(true);
            }

            foreach (Lobby lobby in lobbies) {
                GameObject lobbyObj = Instantiate(lobbyPrefab, lobbyHolder);
                LobbyInfo lobbyInfo = lobbyObj.GetComponent<LobbyInfo>();
                lobbyInfo.SetInfo(lobby.Name, lobby.MaxPlayers, lobby.MaxPlayers - lobby.AvailableSlots, lobby.Data["Status"].Value, lobby.Data["Region"].Value, lobby.Id);
            }
        }
        catch (LobbyServiceException e) {
            if (e.Reason == LobbyExceptionReason.RateLimited)
                infoText.text = "Woah slow down!";
            else
                infoText.text = "Could not get lobbies. Check network connection. (Habibby might need VPN)";

            infoText.gameObject.SetActive(true);
        }
    }
}
