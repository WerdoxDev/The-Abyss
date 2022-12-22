using System.Collections.Generic;
using UnityEngine;

public class WaterGenerator : MonoBehaviour
{
    [SerializeField] private List<WaterMeshLOD> waterMeshes;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private Vector2 size;
    [SerializeField] private Vector2 gridSize;

    public void GenerateWaters()
    {
        int childCount = transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        transform.position = new Vector3(-(gridSize.x * size.x) / 2, 0, -(gridSize.y * size.y) / 2);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                GameObject water = new GameObject($"WaterSection_{x}.{z}");
                water.transform.parent = transform;
                water.transform.localPosition = new Vector3(x * size.x, 0, z * size.y);
                LODGroup lodGroup = water.AddComponent<LODGroup>();
                LOD[] lods = new LOD[waterMeshes.Count];
                for (int i = 0; i < waterMeshes.Count; i++)
                {
                    GameObject go = new GameObject($"WaterMesh_LOD{i}");
                    go.AddComponent<MeshFilter>();
                    go.AddComponent<MeshRenderer>();
                    go.transform.parent = water.transform;
                    go.transform.localPosition = Vector3.zero;

                    MeshFilter _meshFilter = go.GetComponent<MeshFilter>();
                    _meshFilter.mesh = waterMeshes[i].GenerateMesh(size, transform.position.y);

                    Renderer[] renderers = new Renderer[1] { go.GetComponent<Renderer>() };
                    renderers[0].material = waterMaterial;

                    lods[i] = new LOD(waterMeshes[i].lodPoint, renderers);
                }
                lodGroup.SetLODs(lods);
                lodGroup.RecalculateBounds();
            }
        }
    }

    [System.Serializable]
    public class WaterMeshLOD
    {
        public int resolution;
        [Range(0, 1.0f)] public float lodPoint;

        private Vector2 lastSize;
        private int lastResolution;

        public Mesh GenerateMesh(Vector2 size, float yLevel)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            float xPerStep = size.x / resolution;
            float yPerStep = size.y / resolution;

            for (int y = 0; y < resolution + 1; y++)
                for (int x = 0; x < resolution + 1; x++)
                    vertices.Add(new Vector3(x * xPerStep, yLevel, y * yPerStep));

            for (int row = 0; row < resolution; row++)
                for (int column = 0; column < resolution; column++)
                {
                    int i = (row * resolution) + row + column;

                    triangles.Add(i);
                    triangles.Add(i + resolution + 1);
                    triangles.Add(i + resolution + 2);

                    triangles.Add(i);
                    triangles.Add(i + resolution + 2);
                    triangles.Add(i + 1);
                }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            return mesh;
        }
    }
}
