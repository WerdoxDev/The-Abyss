using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyInfo : MonoBehaviour {
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text regionText;
    [SerializeField] private CustomButton joinButton;
    private string lobbyId;

    private void Awake() {
        joinButton.OnClick += async () => {
            Lobby lobby = await Services.Instance.JoinLobby(lobbyId);
            string relayCode = lobby.Data["RelayCode"].Value;
            UIManager.Instance.LoadingPanel.OpenJoinLoading($"{lobby.Name}");
            Services.Instance.JoinRelay(relayCode);
        };
    }

    public void SetInfo(string name, int maxPlayers, int currentPlayers, string status, string region, string id) {
        nameText.text = name;
        playersText.text = $"{currentPlayers}/{maxPlayers}";
        statusText.text = status;
        regionText.text = region;
        lobbyId = id;
    }
}
