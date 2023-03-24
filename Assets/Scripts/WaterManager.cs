using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WaterManager : MonoBehaviour {
    public static WaterManager Instance;

    WaterSearchParameters _searchParameters = new();
    WaterSearchResult _searchResult = new();

    [Header("Autoset Fields")]
    public WaterSurface Surface;

    private void Awake() {
        if (Instance == null) Instance = this;
        else {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
            return;
        }

        Surface = GetComponent<WaterSurface>();
    }

    public float GetWaveHeight(Vector3 position) {
        _searchParameters.startPosition = _searchResult.candidateLocation;
        _searchParameters.targetPosition = position;
        _searchParameters.error = 0.01f;
        _searchParameters.maxIterations = 8;

        // Do the search
        if (Surface.FindWaterSurfaceHeight(_searchParameters, out _searchResult))
            return _searchResult.height;

        return 0;
    }
}
