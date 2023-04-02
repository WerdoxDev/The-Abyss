using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetworkStats : NetworkBehaviour {
    public static NetworkStats Instance;

    [SerializeField] private float maxPingAverageCount;
    [SerializeField] private float maxFpsAverageCount;
    private DateTime _timeBeforePing;

    private List<int> _pingAverageList = new();
    private List<int> _fpsAverageList = new();

    private ClientRpcParams _clientRpcParams;

    public event Action<int> OnPingChanged;
    public event Action<int> OnFpsChanged;

    public int AverageFps { get; private set; }
    public int AveragePing { get; private set; }

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn() {
        if (!IsServer) {
            SendPing();
            return;
        }
        else {
            _clientRpcParams = new ClientRpcParams { Send = new ClientRpcSendParams { } };
            return;
        }
    }

    private void Update() {
        int fps = (int)(1f / Time.unscaledDeltaTime);

        _fpsAverageList.Add(fps);
        if (_fpsAverageList.Count > maxFpsAverageCount) _fpsAverageList.RemoveAt(0);

        int averageFps = _fpsAverageList.Count > 1 ? Mathf.FloorToInt((float)_fpsAverageList.Average()) : fps;
        AverageFps = averageFps;

        OnFpsChanged?.Invoke(AverageFps);
    }

    private void SendPing() {
        PingServerRpc();
        _timeBeforePing = DateTime.Now;
    }

    [ServerRpc(RequireOwnership = false)]
    private void PingServerRpc(ServerRpcParams rpcParams = default) {
        _clientRpcParams.Send.TargetClientIds = new[] { rpcParams.Receive.SenderClientId };
        PongClientRpc(_clientRpcParams);
    }

    [ClientRpc]
    private void PongClientRpc(ClientRpcParams rpcParams = default) {
        TimeSpan timeSpan = DateTime.Now - _timeBeforePing;
        Debug.Log(timeSpan);
        int latency = Mathf.FloorToInt((float)timeSpan.TotalMilliseconds);

        _pingAverageList.Add(latency);
        if (_pingAverageList.Count > maxPingAverageCount) _pingAverageList.RemoveAt(0);

        int averageLatency = _pingAverageList.Count > 1 ? Mathf.FloorToInt((float)_pingAverageList.Average()) : latency;
        AveragePing = averageLatency;

        OnPingChanged?.Invoke(AveragePing);

        SendPing();
    }
}
