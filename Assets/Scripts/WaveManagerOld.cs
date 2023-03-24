using UnityEngine;
using System;
using Unity.Netcode;

[ExecuteAlways]
public class WaveManagerOld : MonoBehaviour {
    public static WaveManagerOld Instance;

    [Header("Wave Settings")]
    [SerializeField] private GerstnerWave waveA;
    [SerializeField] private GerstnerWave waveB;
    [SerializeField] private GerstnerWave waveC;
    [SerializeField] private float gravity = 0.98f;
    [SerializeField] private float multiplier = 1f;
    [SerializeField] private bool networkTime;
    private float _time;

    public GerstnerWave WaveA { get => waveA; set => waveA = value; }
    public GerstnerWave WaveB { get => waveB; set => waveB = value; }
    public GerstnerWave WaveC { get => waveC; set => waveC = value; }

    public float Gravity { get => gravity; set => gravity = value; }
    public float Multiplier { get => multiplier; set => multiplier = value; }

    [Header("Settings")]
    [SerializeField] private Material waterShader;

    [Header("Tests")]
    [SerializeField] private GameObject testObject;
    [SerializeField] private Vector2 testPosition;

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
