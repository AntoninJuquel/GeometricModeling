using System;
using System.Collections.Generic;
using System.Linq;
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

    public enum Highlight
    {
        None,
        Vertex,
        Edge,
        Face
    }

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Polygon : MonoBehaviour
    {
        [SerializeField] private bool toggleAll, drawVertices, drawEdges, drawFaces, drawCentroid, drawHandles;
        [SerializeField] private Edges mode;
        [SerializeField] private Highlight highlight;
        [Min(0)][SerializeField] private int highlightIndex;
        [SerializeField] private bool subdivide;
        [SerializeField] private bool copyCsv;
        private HalfEdgeMesh _halfEdgeMesh;
        private WingedEdgeMesh _wingedEdgeMesh;
        private MeshFilter _meshFilter;
        private List<Mesh> _meshes = new();
        private bool prevToggle;

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
                if (!_meshes.Contains(Mesh))
                    _meshes.Add(Mesh);
                _halfEdgeMesh = new HalfEdgeMesh(value);
                _wingedEdgeMesh = new WingedEdgeMesh(value);
            }
        }

        private void Subdivide()
        {
            _halfEdgeMesh.SubdivideCatmullClark();
            Mesh = _halfEdgeMesh.ConvertToFaceVertexMesh();
        }

        private string ConvertToCsv(string separator)
        {
            var vertices = Mesh.vertices;
            var quads = Mesh.GetIndices(0);

            var strings = vertices.Select((pos, i) => $"{i}{separator}{pos.x:N03} {pos.y:N03} {pos.z:N03}{separator}").ToList();

            for (var i = vertices.Length; i < quads.Length / 4; i++)
                strings.Add(separator + separator + separator);

            for (var i = 0; i < quads.Length / 4; i++)
            {
                strings[i] += $"{i}{separator}{quads[4 * i + 0]},{quads[4 * i + 1]},{quads[4 * i + 2]},{quads[4 * i + 3]}";
            }

            return $"Vertices{separator}{separator}{separator}Faces\nIndex{separator}Position{separator}{separator}Index{separator}Indices des vertices\n{string.Join("\n", strings)}";
        }

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            prevToggle = toggleAll;
        }

        private void Update()
        {
            if (subdivide)
            {
                subdivide = false;
                Subdivide();
            }

            if (copyCsv)
            {
                copyCsv = false;
                var csv = mode switch
                {
                    Edges.HalfEdge => _halfEdgeMesh.ConvertToCsv("\t"),
                    Edges.WingedEdge => _wingedEdgeMesh.ConvertToCsv("\t"),
                    Edges.Mesh => ConvertToCsv("\t"),
                    _ => throw new ArgumentOutOfRangeException()
                };

                GUIUtility.systemCopyBuffer = csv;
                Debug.Log(csv);
            }
        }

        private void OnValidate()
        {
            if (toggleAll != prevToggle)
            {
                drawVertices = drawEdges = drawFaces = drawCentroid = drawHandles = toggleAll;
                prevToggle = toggleAll;
            }
        }

        private void OnDrawGizmos()
        {
            if (!_meshFilter || !Mesh || !Application.isPlaying) return;

            switch (mode)
            {
                case Edges.HalfEdge:
                    _halfEdgeMesh ??= new HalfEdgeMesh(Mesh);
                    _halfEdgeMesh.DrawGizmos(drawVertices, drawEdges, drawFaces, drawCentroid, drawHandles, highlight, highlightIndex, transform);
                    break;
                case Edges.WingedEdge:
                    _wingedEdgeMesh ??= new WingedEdgeMesh(Mesh);
                    _wingedEdgeMesh.DrawGizmos(drawVertices, drawEdges, drawFaces, drawCentroid, drawHandles, highlight, highlightIndex, transform);
                    break;
                case Edges.Mesh:
                    var vertices = Mesh.vertices;
                    var quads = Mesh.GetIndices(0);
                    var style = new GUIStyle
                    {
                        fontSize = 15,
                    };

                    if (drawVertices)
                    {
                        Gizmos.color = Color.black;
                        style.normal.textColor = Gizmos.color;
                        for (var i = 0; i < vertices.Length; i++)
                        {
                            var worldPos = transform.TransformPoint(vertices[i]);
                            Gizmos.DrawSphere(worldPos, .1f);

                            if (drawHandles)
                                Handles.Label(worldPos, $"Vertex {i}", style);
                        }
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

                        if (drawEdges)
                        {
                            Gizmos.color = Color.blue;
                            style.normal.textColor = Gizmos.color;

                            Gizmos.DrawLine(pt1, pt2);
                            Gizmos.DrawLine(pt2, pt3);
                            Gizmos.DrawLine(pt3, pt4);
                            Gizmos.DrawLine(pt4, pt1);

                            if (drawHandles)
                            {
                                Handles.Label((pt1 + pt2) / 2f, $"Edge {index1}", style);
                                Handles.Label((pt2 + pt3) / 2f, $"Edge {index2}", style);
                                Handles.Label((pt3 + pt4) / 2f, $"Edge {index3}", style);
                                Handles.Label((pt4 + pt1) / 2f, $"Edge {index4}", style);
                            }
                        }

                        if (drawFaces)
                        {
                            Gizmos.color = Color.green;
                            style.normal.textColor = Gizmos.color;

                            Gizmos.DrawLine(pt1, pt2);
                            Gizmos.DrawLine(pt2, pt3);
                            Gizmos.DrawLine(pt3, pt4);
                            Gizmos.DrawLine(pt4, pt1);

                            var str = $"Face {i} - ({index1},{index2},{index3},{index4})";

                            if (drawHandles)
                                Handles.Label((pt1 + pt2 + pt3 + pt4) / 4.0f, str, style);
                        }

                        if (drawCentroid)
                        {
                            var center = vertices.Aggregate(Vector3.zero, (current, vertex) => current + transform.TransformPoint(vertex)) / vertices.Length;
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawWireSphere(center, .5f);
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}