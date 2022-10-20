using HalfEdge;
using UnityEngine;
using WingedEdge;

namespace Polygons
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Polygon : MonoBehaviour
    {
        [SerializeField] private bool drawVertices, drawEdges, drawFaces;
        private HalfEdgeMesh _halfEdgeMesh;
        private WingedEdgeMesh _wingedEdgeMesh;
        private MeshFilter _meshFilter;

        protected Mesh Mesh
        {
            get
            {
                _meshFilter ??= GetComponent<MeshFilter>();
                return _meshFilter.mesh;
            }
            set
            {
                _meshFilter.mesh = value;
                _halfEdgeMesh = new HalfEdgeMesh(value);
                _wingedEdgeMesh = new WingedEdgeMesh(value);
            }
        }

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
        }

        private void OnDrawGizmos()
        {
            if (!_meshFilter || !Mesh) return;

            _halfEdgeMesh ??= new HalfEdgeMesh(Mesh);
            _halfEdgeMesh.DrawGizmos(drawVertices, drawEdges, drawFaces);

            _wingedEdgeMesh ??= new WingedEdgeMesh(Mesh);
            _wingedEdgeMesh.DrawGizmos(drawVertices, drawEdges, drawFaces);
        }
    }
}