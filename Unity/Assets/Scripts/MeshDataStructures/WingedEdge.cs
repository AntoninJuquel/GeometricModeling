using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WingedEdge
{
    public class Vertex
    {
        public int index;
        public Vector3 position;
        public WingedEdge edge;
    }

    public class WingedEdge
    {
        public int index;

        public Vertex startVertex;
        public WingedEdge startCWEdge;
        public WingedEdge startCCWEdge;

        public Vertex endVertex;
        public WingedEdge endCWEdge;
        public WingedEdge endCCWEdge;

        public Face rightFace;
        public Face leftFace;
    }

    public class Face
    {
        public int index;
        public WingedEdge edge;
    }

    public class WingedEdgeMesh
    {
        public List<Vertex> vertices = new();
        public List<WingedEdge> edges = new();
        public List<Face> faces = new();

        public WingedEdgeMesh(Mesh mesh)
        {
            var meshVertices = mesh.vertices;
            var meshQuads = mesh.GetIndices(0);

            // First, get vertices
            for (var i = 0; i < meshVertices.Length; i++)
            {
                vertices.Add(new Vertex()
                {
                    index = i,
                    position = meshVertices[i]
                });
            }

            Dictionary<(int, int), WingedEdge> edgesDictionary = new();

            // Second, build faces & edged
            for (var i = 0; i < meshQuads.Length; i += 4)
            {
                var i0 = meshQuads[i];
                var i1 = meshQuads[i + 1];
                var i2 = meshQuads[i + 2];
                var i3 = meshQuads[i + 3];

                var i0I1 = (Mathf.Min(i0, i1), Mathf.Max(i0, i1));
                var i1I2 = (Mathf.Min(i1, i2), Mathf.Max(i1, i2));
                var i2I3 = (Mathf.Min(i2, i3), Mathf.Max(i2, i3));
                var i3I0 = (Mathf.Min(i3, i0), Mathf.Max(i3, i0));

                var vert0 = vertices[i0];
                var vert1 = vertices[i1];
                var vert2 = vertices[i2];
                var vert3 = vertices[i3];

                var face = new Face()
                {
                    index = i / 4,
                };

                WingedEdge edge0;
                WingedEdge edge1;
                WingedEdge edge2;
                WingedEdge edge3;

                if (edgesDictionary.TryGetValue(i0I1, out var e0))
                {
                    edge0 = e0;
                    edge0.leftFace = face;
                }
                else
                {
                    edge0 = new WingedEdge()
                    {
                        index = edges.Count,
                        startVertex = vert0,
                        endVertex = vert1,
                        rightFace = face
                    };
                    edgesDictionary.Add(i0I1, edge0);
                    edges.Add(edge0);
                }

                if (edgesDictionary.TryGetValue(i1I2, out var e1))
                {
                    edge1 = e1;
                    edge1.leftFace = face;
                }
                else
                {
                    edge1 = new WingedEdge()
                    {
                        index = edges.Count,
                        startVertex = vert1,
                        endVertex = vert2,
                        rightFace = face
                    };
                    edgesDictionary.Add(i1I2, edge1);
                    edges.Add(edge1);
                }

                if (edgesDictionary.TryGetValue(i2I3, out var e2))
                {
                    edge2 = e2;
                    edge2.leftFace = face;
                }
                else
                {
                    edge2 = new WingedEdge()
                    {
                        index = edges.Count,
                        startVertex = vert2,
                        endVertex = vert3,
                        rightFace = face
                    };
                    edgesDictionary.Add(i2I3, edge2);
                    edges.Add(edge2);
                }

                if (edgesDictionary.TryGetValue(i3I0, out var e3))
                {
                    edge3 = e3;
                    edge3.leftFace = face;
                }
                else
                {
                    edge3 = new WingedEdge
                    {
                        index = edges.Count,
                        startVertex = vert3,
                        endVertex = vert0,
                        rightFace = face
                    };
                    edgesDictionary.Add(i3I0, edge3);
                    edges.Add(edge3);
                }

                if (e0 != null)
                {
                    edge0.endCWEdge = edge3;
                    edge0.startCCWEdge = edge1;
                }
                else
                {
                    edge0.startCWEdge = edge3;
                    edge0.endCCWEdge = edge1;
                }

                if (e1 != null)
                {
                    edge1.endCWEdge = edge0;
                    edge1.startCCWEdge = edge2;
                }
                else
                {
                    edge1.startCWEdge = edge0;
                    edge1.endCCWEdge = edge2;
                }

                if (e2 != null)
                {
                    edge2.endCWEdge = edge1;
                    edge2.startCCWEdge = edge3;
                }
                else
                {
                    edge2.startCWEdge = edge1;
                    edge2.endCCWEdge = edge3;
                }

                if (e3 != null)
                {
                    edge3.endCWEdge = edge2;
                    edge3.startCCWEdge = edge0;
                }
                else
                {
                    edge3.startCWEdge = edge2;
                    edge3.endCCWEdge = edge0;
                }

                faces.Add(face);
            }
        }

        public Mesh ConvertToFaceVertexMesh()
        {
            Mesh faceVertexMesh = new Mesh();
            // magic happens
            return faceVertexMesh;
        }

        public string ConvertToCSVFormat(string separator = "\t")
        {
            string str = "";
            //magic happens
            return str;
        }

        public void DrawGizmos(bool drawVertices, bool drawEdges, bool drawFaces, Transform transform)
        {
            if (drawVertices)
                DrawVertices(transform);
            if (drawEdges)
                DrawEdges(transform);
            if (drawFaces)
                DrawFaces(transform);
        }

        private void DrawFaces(Transform transform)
        {
            foreach (var face in faces)
            {
                // var pos0 = transform.TransformPoint(face.edge.startVertex.position);
                // var pos1 = transform.TransformPoint(face.edge.endVertex.position);
                // var pos2 = transform.TransformPoint(face.edge.startCWEdge.endVertex.position);
                // var pos3 = transform.TransformPoint(face.edge.nextEdge.nextEdge.nextEdge.sourceVertex.position);

                //
                // Gizmos.DrawLine(pos0, pos1);
                // Gizmos.DrawLine(pos1, pos2);
                // Gizmos.DrawLine(pos2, pos3);
                // Gizmos.DrawLine(pos3, pos0);
                //
                // Gizmos.DrawLine(pos0, pos2);
                // Gizmos.DrawLine(pos1, pos3);
                //
                // var position = face.edge.sourceVertex.position;
                // var direction = (face.edge.nextEdge.sourceVertex.position - position).normalized;
                // DrawArrow.ForGizmo(position, direction);
            }
        }

        private void DrawEdges(Transform transform)
        {
            var style = new GUIStyle
            {
                fontSize = 15,
                normal =
                {
                    textColor = Color.red
                }
            };

            foreach (var edge in edges)
            {
                var center = transform.TransformPoint((edge.startVertex.position + edge.endVertex.position) / 2f);
                var startPos = transform.TransformPoint(Vector3.Lerp(edge.startVertex.position, edge.endVertex.position, .1f));
                var endPos = transform.TransformPoint(Vector3.Lerp(edge.startVertex.position, edge.endVertex.position, .9f));
                Handles.Label(center, $"e{edge.index}", style);

                if (edge.startCWEdge != null)
                {
                    var scwcenter = transform.TransformPoint((edge.startCWEdge.startVertex.position + edge.startCWEdge.endVertex.position) / 2f);
                    DrawArrow.ForGizmo(startPos, (scwcenter - startPos).normalized);
                }

                if (edge.startCCWEdge != null)
                {
                    var sccwcenter = (edge.startCCWEdge.startVertex.position + edge.startCCWEdge.endVertex.position) / 2f;
                    DrawArrow.ForGizmo(startPos, (sccwcenter - startPos).normalized);
                }

                if (edge.endCWEdge != null)
                {
                    var ecwcenter = (edge.endCWEdge.startVertex.position + edge.endCWEdge.endVertex.position) / 2f;
                    DrawArrow.ForGizmo(endPos, (ecwcenter - endPos).normalized);
                }

                if (edge.endCCWEdge != null)
                {
                    var eccwcenter = transform.TransformPoint((edge.endCCWEdge.startVertex.position + edge.endCCWEdge.endVertex.position) / 2f);
                    DrawArrow.ForGizmo(endPos, (eccwcenter - endPos).normalized);
                }
            }
        }

        private void DrawVertices(Transform transform)
        {
            var style = new GUIStyle
            {
                fontSize = 15,
                normal =
                {
                    textColor = Color.red
                }
            };
            var index = 0;
            foreach (var vertex in vertices)
            {
                Handles.Label(transform.TransformPoint(vertex.position), index++.ToString(), style);
            }
        }
    }
}