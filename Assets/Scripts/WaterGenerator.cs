using System.Collections.Generic;
using UnityEngine;

public class WaterGenerator : MonoBehaviour {
    public List<WaterMeshLOD> WaterMeshes;

    [SerializeField] private Material waterMaterial;
    [SerializeField] private Vector2 size;
    [SerializeField] private Vector2 gridSize;

    public void GenerateWater() {
        int childCount = transform.childCount;

        for (int i = childCount - 1; i >= 0; i--) {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        transform.localPosition = new Vector3(-(gridSize.x * size.x) / 2, 0, -(gridSize.y * size.y) / 2);

        for (int x = 0; x < gridSize.x; x++) {
            for (int z = 0; z < gridSize.y; z++) {
                GameObject water = new($"WaterSection_{x}.{z}");
                water.transform.parent = transform;
                water.transform.localPosition = new Vector3(x * size.x, 0, z * size.y);
                LODGroup lodGroup = water.AddComponent<LODGroup>();
                LOD[] lods = new LOD[WaterMeshes.Count];
                for (int i = 0; i < WaterMeshes.Count; i++) {
                    GameObject go = new($"WaterMesh_LOD{i}");
                    MeshFilter meshFilter = go.AddComponent<MeshFilter>();
                    MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();

                    go.transform.parent = water.transform;
                    go.transform.localPosition = Vector3.zero;

                    meshFilter.mesh = WaterMeshes[i].GenerateMesh(size, 0);
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    Renderer[] renderers = new Renderer[1] { go.GetComponent<Renderer>() };
                    renderers[0].material = waterMaterial;

                    lods[i] = new LOD(WaterMeshes[i].LodPoint, renderers);
                }
                lodGroup.SetLODs(lods);
                lodGroup.RecalculateBounds();
            }
        }
    }

    [System.Serializable]
    public class WaterMeshLOD {
        public int Resolution;
        [Range(0, 1.0f)] public float LodPoint;

        public WaterMeshLOD(int resolution, float lodPoint) {
            Resolution = resolution;
            LodPoint = lodPoint;
        }

        public Mesh GenerateMesh(Vector2 size, float yLevel) {
            List<Vector3> vertices = new();
            List<int> triangles = new();

            float xPerStep = size.x / Resolution;
            float yPerStep = size.y / Resolution;

            for (int y = 0; y < Resolution + 1; y++)
                for (int x = 0; x < Resolution + 1; x++)
                    vertices.Add(new Vector3(x * xPerStep, yLevel, y * yPerStep));

            for (int row = 0; row < Resolution; row++)
                for (int column = 0; column < Resolution; column++) {
                    int i = (row * Resolution) + row + column;

                    triangles.Add(i);
                    triangles.Add(i + Resolution + 1);
                    triangles.Add(i + Resolution + 2);

                    triangles.Add(i);
                    triangles.Add(i + Resolution + 2);
                    triangles.Add(i + 1);
                }

            Mesh mesh = new() {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };

            return mesh;
        }
    }
}
