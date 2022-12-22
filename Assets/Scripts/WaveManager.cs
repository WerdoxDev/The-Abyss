using UnityEngine;
using Unity.Netcode;

[ExecuteAlways]
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("Wave Settings")]
    [SerializeField] private GerstnerWave waveA;
    [SerializeField] private GerstnerWave waveB;
    [SerializeField] private GerstnerWave waveC;
    [SerializeField] private float gravity = 0.98f;
    [SerializeField] private float multiplier = 1f;
    [SerializeField] private bool networkTime;
    private float _time;

    [Header("Settings")]
    [SerializeField] private Material waterShader;

    [Header("Tests")]
    [SerializeField] private GameObject testObject;
    [SerializeField] private Vector2 testPosition;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Update()
    {
        if (networkTime)
            _time = NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalTime.TimeAsFloat : 0;
        else _time = Time.time;

        float y = GetWaveHeight(testPosition.x, testPosition.y);
        if (testObject != null) testObject.transform.position = new Vector3(testPosition.x, y, testPosition.y);
        waterShader.SetVector("_WaveA", new Vector4(waveA.direction.x, waveA.direction.y, waveA.steepness, waveA.wavelength));
        waterShader.SetVector("_WaveB", new Vector4(waveB.direction.x, waveB.direction.y, waveB.steepness, waveB.wavelength));
        waterShader.SetVector("_WaveC", new Vector4(waveC.direction.x, waveC.direction.y, waveC.steepness, waveC.wavelength));
        waterShader.SetFloat("_Gravity", gravity);
        waterShader.SetFloat("_CustomTime", _time);
    }

    public float GetWaveHeight(float _x, float _z)
    {
        float y = 0;
        y += waveA.GetPointHeight(_x, _z, gravity, _time, multiplier);
        y += waveB.GetPointHeight(_x, _z, gravity, _time, multiplier);
        y += waveC.GetPointHeight(_x, _z, gravity, _time, multiplier);
        return y + transform.position.y;
    }
}

[System.Serializable]
public class GerstnerWave
{
    public Vector2 direction = new Vector2(1.0f, 0);
    [Range(0, 1f)] public float steepness = 0.5f;
    public float wavelength = 10f;

    public float GetPointHeight(float _x, float _z, float gravity, float time, float multiplier = 1f)
    {
        float w = wavelength * multiplier;
        float g = gravity * multiplier;

        float k = 2 * Mathf.PI / w;
        float c = Mathf.Sqrt(g / k);
        Vector2 d = direction.normalized;
        float f = k * (Vector2.Dot(d, new Vector2(_x, _z)) - time * c);
        float a = steepness / k;

        float y = a * Mathf.Sin(f - Mathf.Cos(f - Mathf.Cos(f - Mathf.Cos(f))));

        return y;
    }
}