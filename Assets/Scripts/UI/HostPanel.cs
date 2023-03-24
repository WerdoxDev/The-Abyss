using TMPro;
using Unity.Services.Relay.Models;
using UnityEngine;

public class HostPanel : MonoBehaviour {
    [Header("Lobby Panel")]
    [SerializeField] private CustomButton lobbyCreateButton;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private CustomButton maxPlayersIncrease;
    [SerializeField] private CustomButton maxPlayersDecrease;
    [SerializeField] private TMP_Text maxPlayersText;
    private int maxPlayers = 4;

    [Header("Custom Panel")]
    [SerializeField] private CustomButton customCreateButton;
    [SerializeField] private TMP_InputField ipAddressInputField;

    private void Awake() {
        maxPlayersText.text = maxPlayers.ToString();
        lobbyCreateButton.OnClick += async () => {
            if (lobbyNameInputField.text == "") return;

            UIManager.Instance.LoadingPanel.OpenLobbyLoading(lobbyNameInputField.text);
            (Allocation, string) relay = await Services.Instance.CreateRelay();

            await Services.Instance.CreateLobby(lobbyNameInputField.text, maxPlayers, relay.Item1.Region, relay.Item2);
        };

        customCreateButton.OnClick += () => {
            string[] split = ipAddressInputField.text.Split(":");
            if (split.Length != 2) return;

            UIManager.Instance.LoadingPanel.OpenHostLoading();
            GameManager.Instance.SetConnectionData(split[0], ushort.Parse(split[1]));
            TheAbyssNetworkManager.Instance.Host(
                new PlayerConnData(UIManager.Instance.PlayerName, UIManager.Instance.CustomizePanel.PlayerCustomization));
        };

        maxPlayersIncrease.OnClick += () => {
            if (maxPlayers >= 8) return;
            maxPlayers++;
            maxPlayersText.text = maxPlayers.ToString();
        };

        maxPlayersDecrease.OnClick += () => {
            if (maxPlayers <= 2) return;
            maxPlayers--;
            maxPlayersText.text = maxPlayers.ToString();
        };
    }
}
