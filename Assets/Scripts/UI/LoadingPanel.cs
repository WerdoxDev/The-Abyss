using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanel : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private RectTransform progressTransform;
    [SerializeField] private RectTransform backgroundTransform;
    [SerializeField] private int minX;
    private string _connectionName;
    private int _padding;

    public LoadingState State;
    public Panel Panel;

    private void Awake() {
        //TheAbyssNetworkManager.Instance.OnClientConnectionStarted += () => {
        //    if (!Panel.IsOpen) return;

        //    State = LoadingState.Connecting;
        //    loadingText.text = $"Connecting to {_hostName}...";
        //    SetProgressBar(50);
        //};

        //TheAbyssNetworkManager.Instance.OnHostConnectionStarted += () => {
        //    if (!Panel.IsOpen) return;

        //    State = LoadingState.StartingServer;
        //    loadingText.text = $"Starting server...";
        //    SetProgressBar(50);
        //};

        TheAbyssNetworkManager.Instance.OnLocalClientConnected += (clientId) => {
            if (!Panel.IsOpen) return;

            State = LoadingState.LoadingScene;
            loadingText.text = $"Loading scene...";
            SetProgressBar(25);
        };

        GameManager.Instance.OnPlayerSpawned += (player, isOwner) => {
            if (!isOwner) return;
            SetProgressBar(100);
            Close();
        };

        TheAbyssNetworkManager.Instance.OnClientDisconnected += (bool wasConnected) => {
            if (wasConnected) return;

            Close();
            PromptManager.Instance.CreatePrompt("Connection Error", $"Could not connect to {_connectionName}", null, "Ok", null, null, null, null);
        };

        _padding = backgroundTransform.GetComponent<VerticalLayoutGroup>().padding.horizontal;
    }

    public void OpenJoinLoading(string hostName) {
        Panel.Open();
        loadingText.text = $"Connecting to {hostName}...";
        _connectionName = hostName;
        SetProgressBar(0);
    }

    public void OpenHostLoading() {
        Panel.Open();
        loadingText.text = $"Starting server...";
        SetProgressBar(0);
    }

    public void OpenLobbyLoading(string lobbyName) {
        Panel.Open();
        loadingText.text = $"Creating lobby {lobbyName}...";
        _connectionName = lobbyName;
        SetProgressBar(0);
    }

    public void Close() {
        Panel.Close();
        State = LoadingState.None;
    }

    private void SetProgressBar(int percentage) {
        float maxX = backgroundTransform.sizeDelta.x;
        float xSize = (maxX / 100) * percentage;

        if (xSize > maxX - _padding) xSize = maxX - _padding;
        else if (xSize < minX) xSize = minX;

        progressTransform.sizeDelta = new(xSize, 0);
    }
}

public enum LoadingState {
    None,
    Connecting,
    StartingServer,
    LoadingScene
}