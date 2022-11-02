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

        public Vertex(int index, Vector3 position)
        {
            this.index = index;
            this.position = position;
        }
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

        public WingedEdge(int index, Vertex startVertex, Vertex endVertex)
        {
            this.index = index;
            this.startVertex = startVertex;
            this.endVertex = endVertex;
            startVertex.edge = this;
            endVertex.edge = this;
        }
    }

    public class Face
    {
        public int index;
        public WingedEdge edge;

        public Face(int index)
        {
            this.index = index;
        }
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
                vertices.Add(new Vertex(i, meshVertices[i]));
            }

            Dictionary<(int, int), WingedEdge> edgesDictionary = new();

            // Second, build faces & edged
            for (var i = 0; i < meshQuads.Length; i += 4)
            {
                var i0 = meshQuads[i];
                var i1 = meshQuads[i + 1];
                var i2 = meshQuads[i + 2];
                var i3 = meshQuads[i + 3];

                var i0i1 = (Mathf.Min(i0, i1), Mathf.Max(i0, i1));
                var i1i2 = (Mathf.Min(i1, i2), Mathf.Max(i1, i2));
                var i2i3 = (Mathf.Min(i2, i3), Mathf.Max(i2, i3));
                var i3i0 = (Mathf.Min(i3, i0), Mathf.Max(i3, i0));

                var vert0 = vertices[i0];
                var vert1 = vertices[i1];
                var vert2 = vertices[i2];
                var vert3 = vertices[i3];

                var face = new Face(i / 4);

                WingedEdge edge0;
                WingedEdge edge1;
                WingedEdge edge2;
                WingedEdge edge3;

                if (edgesDictionary.TryGetValue(i0i1, out var e0))
                {
                    edge0 = e0;
                    edge0.leftFace = face;
                }
                else
                {
                    edge0 = new WingedEdge(i, vert0, vert1);
                    edgesDictionary.Add(i0i1, edge0);
                    edge0.rightFace = face;
                    edges.Add(edge0);
                }

                if (edgesDictionary.TryGetValue(i1i2, out var e1))
                {
                    edge1 = e1;
                    edge1.leftFace = face;
                }
                else
                {
                    edge1 = new WingedEdge(i + 1, vert1, vert2);
                    edgesDictionary.Add(i1i2, edge1);
                    edge1.rightFace = face;
                    edges.Add(edge1);
                }

                if (edgesDictionary.TryGetValue(i2i3, out var e2))
                {
                    edge2 = e2;
                    edge2.leftFace = face;
                }
                else
                {
                    edge2 = new WingedEdge(i + 2, vert2, vert3);
                    edgesDictionary.Add(i2i3, edge2);
                    edge2.rightFace = face;
                    edges.Add(edge2);
                }

                if (edgesDictionary.TryGetValue(i3i0, out var e3))
                {
                    edge3 = e3;
                    edge3.leftFace = face;
                }
                else
                {
                    edge3 = new WingedEdge(i + 3, vert3, vert0);
                    edgesDictionary.Add(i3i0, edge3);
                    edge3.rightFace = face;
                    edges.Add(edge3);
                }

                face.edge = edge0;

                edge0.startCWEdge = edge3;
                edge0.endCCWEdge = edge1;

                edge1.startCWEdge = edge0;
                edge1.endCCWEdge = edge2;

                edge2.startCWEdge = edge1;
                edge2.endCCWEdge = edge3;

                edge3.startCWEdge = edge2;
                edge3.endCCWEdge = edge0;

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

            var index = 0;
            foreach (var edge in edges)
            {
                var center = transform.TransformPoint((edge.startVertex.position + edge.endVertex.position) / 2f);
                Handles.Label(center, $"e{index++}", style);

                var scwcenter = transform.TransformPoint((edge.startCWEdge.startVertex.position + edge.startCWEdge.endVertex.position) / 2f);
                DrawArrow.ForGizmo(center, (scwcenter - center).normalized);
                // var sccwcenter = (edge.startCCWEdge.startVertex.position + edge.startCCWEdge.endVertex.position) / 2f;
                // DrawArrow.ForGizmo(center, (sccwcenter - center).normalized);
                // var ecwcenter = (edge.endCWEdge.startVertex.position + edge.endCWEdge.endVertex.position) / 2f;
                // DrawArrow.ForGizmo(center, (ecwcenter - center).normalized);
                var eccwcenter = transform.TransformPoint((edge.endCCWEdge.startVertex.position + edge.endCCWEdge.endVertex.position) / 2f);
                DrawArrow.ForGizmo(center, (eccwcenter - center).normalized);
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