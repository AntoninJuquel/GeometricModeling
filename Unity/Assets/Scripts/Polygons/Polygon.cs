using System;
using HalfEdge;
using UnityEditor;
using UnityEngine;
using WingedEdge;

namespace Polygons
{
    public enum Edges
    {
        HalfEdge,
        WingedEdge,
        Mesh
    }

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Polygon : MonoBehaviour
    {
        [SerializeField] private bool drawVertices, drawEdges, drawFaces, drawCentroid;
        [SerializeField] private Edges edgeMode;
        [SerializeField] private HalfEdgeMesh _halfEdgeMesh;
        private WingedEdgeMesh _wingedEdgeMesh;
        private MeshFilter _meshFilter;

        protected Mesh Mesh
        {
            get
            {
                if (!_meshFilter)
                    _meshFilter = GetComponent<MeshFilter>();
                return _meshFilter.mesh;
            }
            set
            {
                if (!_meshFilter)
                    _meshFilter = GetComponent<MeshFilter>();
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
            if (!_meshFilter || !Mesh || !Application.isPlaying) return;

            switch (edgeMode)
            {
                case Edges.HalfEdge:
                    _halfEdgeMesh ??= new HalfEdgeMesh(Mesh);
                    _halfEdgeMesh.DrawGizmos(drawVertices, drawEdges, drawFaces, drawCentroid, transform);
                    break;
                case Edges.WingedEdge:
                    _wingedEdgeMesh ??= new WingedEdgeMesh(Mesh);
                    _wingedEdgeMesh.DrawGizmos(drawVertices, drawEdges, drawFaces, transform);
                    break;
                case Edges.Mesh:
                    var vertices = Mesh.vertices;
                    var quads = Mesh.GetIndices(0);

                    var style = new GUIStyle
                    {
                        fontSize = 15,
                        normal =
                        {
                            textColor = Color.red
                        }
                    };

                    for (var i = 0; i < vertices.Length; i++)
                    {
                        var worldPos = transform.TransformPoint(vertices[i]);
                        Handles.Label(worldPos, i.ToString(), style);
                    }

                    for (var i = 0; i < quads.Length / 4; i++)
                    {
                        var index1 = quads[4 * i];
                        var index2 = quads[4 * i + 1];
                        var index3 = quads[4 * i + 2];
                        var index4 = quads[4 * i + 3];

                        var pt1 = transform.TransformPoint(vertices[index1]);
                        var pt2 = transform.TransformPoint(vertices[index2]);
                        var pt3 = transform.TransformPoint(vertices[index3]);
                        var pt4 = transform.TransformPoint(vertices[index4]);

                        Gizmos.DrawLine(pt1, pt2);
                        Handles.Label((pt1 + pt2) / 2.0f, $"e{i}", style);
                        Gizmos.DrawLine(pt2, pt3);
                        Handles.Label((pt2 + pt3) / 2.0f, $"e{i + 1}", style);
                        Gizmos.DrawLine(pt3, pt4);
                        Handles.Label((pt3 + pt4) / 2.0f, $"e{i + 2}", style);
                        Gizmos.DrawLine(pt4, pt1);
                        Handles.Label((pt4 + pt1) / 2.0f, $"e{i + 3}", style);

                        var str = $"{i} ({index1},{index2},{index3},{index4})";

                        Handles.Label((pt1 + pt2 + pt3 + pt4) / 4.0f, str, style);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}