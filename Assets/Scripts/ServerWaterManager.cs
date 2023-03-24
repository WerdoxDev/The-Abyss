using System;
using Unity.Netcode;

public class ServerWaveManager : NetworkBehaviour {

    public NetworkVariable<WaterInfo> WaterInfo = new();

    private WaterManager _client;

    private void Awake() {
        _client = GetComponent<WaterManager>();
    }

    public override void OnNetworkSpawn() {
        if (!IsServer) {
            enabled = false;
            _client.Surface.cpuSimulation = false;

            WaterInfo.OnValueChanged += (WaterInfo oldInfo, WaterInfo newInfo) => {
                _client.Surface.timeMultiplier = newInfo.TimeMultiplier;
                _client.Surface.repetitionSize = newInfo.RepetitionSize;
                _client.Surface.largeWindSpeed = newInfo.WindSpeed;
                _client.Surface.largeCurrentSpeedValue = newInfo.CurrentSpeed;
                _client.Surface.largeCurrentOrientationValue = newInfo.CurrentOrientation;
            };
        }
    }

    private void Update() {
        WaterInfo.Value = new WaterInfo() {
            TimeMultiplier = _client.Surface.timeMultiplier,
            RepetitionSize = _client.Surface.repetitionSize,
            WindSpeed = _client.Surface.largeWindSpeed,
            CurrentSpeed = _client.Surface.largeCurrentSpeedValue,
            CurrentOrientation = _client.Surface.largeCurrentOrientationValue
        };
    }
}

public struct WaterInfo : IEquatable<WaterInfo>, INetworkSerializable {
    public float TimeMultiplier;
    public float RepetitionSize;
    public float WindSpeed;
    public float CurrentSpeed;
    public float CurrentOrientation;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref TimeMultiplier);
        serializer.SerializeValue(ref RepetitionSize);
        serializer.SerializeValue(ref WindSpeed);
        serializer.SerializeValue(ref CurrentSpeed);
        serializer.SerializeValue(ref CurrentOrientation);
    }

    public bool Equals(WaterInfo other) =>
        other.TimeMultiplier == TimeMultiplier &&
        other.RepetitionSize == RepetitionSize &&
        other.WindSpeed == WindSpeed &&
        other.CurrentSpeed == CurrentSpeed &&
        other.CurrentOrientation == CurrentOrientation;
}