using UnityEngine;
using System;
using Unity.Netcode;

// [ExecuteAlways]
public class WaveManager : NetworkBehaviour {
    public static WaveManager Instance;

    [Header("Wave Settings")]
    [SerializeField] private GerstnerWave waveA;
    [SerializeField] private GerstnerWave waveB;
    [SerializeField] private GerstnerWave waveC;
    [SerializeField] private float gravity = 0.98f;
    [SerializeField] private float multiplier = 1f;
    [SerializeField] private bool networkTime;
    private float _time;

    public NetworkVariable<WavesInfo> WavesInfo = new NetworkVariable<WavesInfo>();

    [Header("Settings")]
    [SerializeField] private Material waterShader;

    [Header("Tests")]
    [SerializeField] private GameObject testObject;
    [SerializeField] private Vector2 testPosition;

    public override void OnNetworkSpawn() {
        if (IsServer) return;

        WavesInfo.OnValueChanged += (WavesInfo oldInfo, WavesInfo newInfo) => {
            waveA = new GerstnerWave(newInfo.Wave1Direction, newInfo.Wave1Steepness, newInfo.Wave1Length);
            waveB = new GerstnerWave(newInfo.Wave2Direction, newInfo.Wave2Steepness, newInfo.Wave2Length);
            waveC = new GerstnerWave(newInfo.Wave3Direction, newInfo.Wave3Steepness, newInfo.Wave3Length);
            gravity = newInfo.Gravity;
            multiplier = newInfo.Multiplier;
            Debug.Log("change");
        };
    }

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
            return;
        }
    }

    private void Update() {
        if (networkTime) _time = NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalTime.TimeAsFloat : 0;
        else _time = Time.time;

        if (testObject != null) {
            float y = GetWaveHeight(testPosition.x, testPosition.y);
            testObject.transform.position = new Vector3(testPosition.x, y, testPosition.y);
        }

        waterShader.SetVector("_WaveA", new Vector4(waveA.Direction.x, waveA.Direction.y, waveA.Steepness, waveA.Wavelength));
        waterShader.SetVector("_WaveB", new Vector4(waveB.Direction.x, waveB.Direction.y, waveB.Steepness, waveB.Wavelength));
        waterShader.SetVector("_WaveC", new Vector4(waveC.Direction.x, waveC.Direction.y, waveC.Steepness, waveC.Wavelength));
        waterShader.SetFloat("_Gravity", gravity);
        waterShader.SetFloat("_CustomTime", _time);

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        WavesInfo.Value = new WavesInfo() {
            Wave1Direction = waveA.Direction,
            Wave2Direction = waveB.Direction,
            Wave3Direction = waveC.Direction,
            Wave1Length = waveA.Wavelength,
            Wave2Length = waveB.Wavelength,
            Wave3Length = waveC.Wavelength,
            Wave1Steepness = waveA.Steepness,
            Wave2Steepness = waveB.Steepness,
            Wave3Steepness = waveC.Steepness
        };
    }

    public float GetWaveHeight(float _x, float _z) {
        float y = 0;
        y += waveA.GetPointHeight(_x, _z, gravity, _time, multiplier);
        y += waveB.GetPointHeight(_x, _z, gravity, _time, multiplier);
        y += waveC.GetPointHeight(_x, _z, gravity, _time, multiplier);
        return y + transform.position.y;
    }
}

[System.Serializable]
public class GerstnerWave {
    public Vector2 Direction = new Vector2(1.0f, 0);
    [Range(0, 1f)] public float Steepness = 0.5f;
    public float Wavelength = 10f;

    public GerstnerWave(Vector2 direction, float steepness, float wavelength) {
        Direction = direction;
        Steepness = steepness;
        Wavelength = wavelength;
    }

    public float GetPointHeight(float _x, float _z, float gravity, float time, float multiplier = 1f) {
        float w = Wavelength * multiplier;
        float g = gravity * multiplier;

        float k = 2 * Mathf.PI / w;
        float c = Mathf.Sqrt(g / k);
        Vector2 d = Direction.normalized;
        float f = k * (Vector2.Dot(d, new Vector2(_x, _z)) - time * c);
        float a = Steepness / k;

        float y = a * Mathf.Sin(f - Mathf.Cos(f - Mathf.Cos(f - Mathf.Cos(f))));

        return y;
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