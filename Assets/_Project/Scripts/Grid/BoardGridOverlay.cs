using System.Collections.Generic;
using UnityEngine;

namespace StrategyDemo.Grid
{
    /// <summary>
    /// Draws the playable board grid as one mesh so the outer border is closed and the overlay costs
    /// a single renderer instead of one sprite per cell.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BoardGridOverlay : MonoBehaviour
    {
        [SerializeField] private Vector2 _origin = Vector2.zero;
        [SerializeField, Min(1)] private int _width = 10;
        [SerializeField, Min(1)] private int _height = 10;
        [SerializeField, Min(0.001f)] private float _lineThickness = 0.025f;
        [SerializeField] private Color _color = new Color(0.08f, 0.18f, 0.08f, 0.35f);
        [SerializeField] private int _sortingOrder = 1;

        private Mesh _mesh;
        private Material _material;

        private void Awake()
        {
            Build();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                Build();
            }
        }

        private void OnDestroy()
        {
            if (_mesh != null)
            {
                Destroy(_mesh);
            }

            if (_material != null)
            {
                Destroy(_material);
            }
        }

        private void Build()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = gameObject.AddComponent<MeshFilter>();
            }

            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            if (_material == null)
            {
                _material = new Material(Shader.Find("Sprites/Default"))
                {
                    color = _color
                };
            }

            _material.color = _color;
            meshRenderer.sharedMaterial = _material;
            meshRenderer.sortingOrder = _sortingOrder;

            if (_mesh == null)
            {
                _mesh = new Mesh { name = "BoardGridOverlay" };
            }
            else
            {
                _mesh.Clear();
            }

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            float half = _lineThickness * 0.5f;
            float left = _origin.x;
            float right = _origin.x + _width;
            float bottom = _origin.y;
            float top = _origin.y + _height;

            for (int x = 0; x <= _width; x++)
            {
                AddQuad(vertices, triangles, _origin.x + x - half, bottom, _origin.x + x + half, top);
            }

            for (int y = 0; y <= _height; y++)
            {
                AddQuad(vertices, triangles, left, _origin.y + y - half, right, _origin.y + y + half);
            }

            _mesh.SetVertices(vertices);
            _mesh.SetTriangles(triangles, 0);
            _mesh.RecalculateBounds();
            filter.sharedMesh = _mesh;
        }

        private static void AddQuad(
            List<Vector3> vertices, List<int> triangles, float minX, float minY, float maxX, float maxY)
        {
            int start = vertices.Count;
            vertices.Add(new Vector3(minX, minY, 0f));
            vertices.Add(new Vector3(maxX, minY, 0f));
            vertices.Add(new Vector3(maxX, maxY, 0f));
            vertices.Add(new Vector3(minX, maxY, 0f));
            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 1);
            triangles.Add(start);
            triangles.Add(start + 3);
            triangles.Add(start + 2);
        }
    }
}
