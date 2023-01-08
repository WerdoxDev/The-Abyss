using System;
using UnityEngine;
using Unity.Netcode;

public class ServerWaveManager : NetworkBehaviour {

    public NetworkVariable<WavesInfo> WavesInfo = new NetworkVariable<WavesInfo>();

    private WaveManager _client;

    private void Awake() {
        _client = GetComponent<WaveManager>();
    }

    public override void OnNetworkSpawn() {
        if (!IsServer) {
            enabled = false;

            WavesInfo.OnValueChanged += (WavesInfo oldInfo, WavesInfo newInfo) => {
                _client.WaveA = new GerstnerWave(newInfo.Wave1Direction, newInfo.Wave1Steepness, newInfo.Wave1Length);
                _client.WaveB = new GerstnerWave(newInfo.Wave2Direction, newInfo.Wave2Steepness, newInfo.Wave2Length);
                _client.WaveC = new GerstnerWave(newInfo.Wave3Direction, newInfo.Wave3Steepness, newInfo.Wave3Length);
                _client.Gravity = newInfo.Gravity;
                _client.Multiplier = newInfo.Multiplier;
                Debug.Log("change");
            };
        }
    }

    private void Update() {
        WavesInfo.Value = new WavesInfo() {
            Wave1Direction = _client.WaveA.Direction,
            Wave2Direction = _client.WaveB.Direction,
            Wave3Direction = _client.WaveC.Direction,
            Wave1Length = _client.WaveA.Wavelength,
            Wave2Length = _client.WaveB.Wavelength,
            Wave3Length = _client.WaveC.Wavelength,
            Wave1Steepness = _client.WaveA.Steepness,
            Wave2Steepness = _client.WaveB.Steepness,
            Wave3Steepness = _client.WaveC.Steepness,
            Gravity = _client.Gravity,
            Multiplier = _client.Multiplier
        };
    }
}

public struct WavesInfo : IEquatable<WavesInfo>, INetworkSerializable {
    public Vector2 Wave1Direction;
    public float Wave1Steepness;
    public float Wave1Length;
    public Vector2 Wave2Direction;
    public float Wave2Steepness;
    public float Wave2Length;
    public Vector2 Wave3Direction;
    public float Wave3Steepness;
    public float Wave3Length;
    public float Gravity;
    public float Multiplier;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref Wave1Direction);
        serializer.SerializeValue(ref Wave2Direction);
        serializer.SerializeValue(ref Wave3Direction);
        serializer.SerializeValue(ref Wave1Steepness);
        serializer.SerializeValue(ref Wave2Steepness);
        serializer.SerializeValue(ref Wave3Steepness);
        serializer.SerializeValue(ref Wave1Length);
        serializer.SerializeValue(ref Wave2Length);
        serializer.SerializeValue(ref Wave3Length);
        serializer.SerializeValue(ref Gravity);
        serializer.SerializeValue(ref Multiplier);
    }

    public bool Equals(WavesInfo other) =>
        other.Wave1Direction == Wave1Direction && other.Wave2Direction == Wave2Direction && other.Wave3Direction == Wave3Direction &&
        other.Wave1Length == Wave1Length && other.Wave2Length == Wave2Length && other.Wave3Length == Wave3Length &&
        other.Wave1Steepness == Wave1Steepness && other.Wave2Steepness == Wave2Steepness && other.Wave3Steepness == Wave3Steepness;
}