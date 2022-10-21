using System.Collections.Generic;
using UnityEngine;

namespace HalfEdge
{
    public class Vertex
    {
        public int index;
        public Vector3 position;
        public HalfEdge outgoingEdge;

        public Vertex(int index, Vector3 position)
        {
            this.index = index;
            this.position = position;
        }
    }

    public class HalfEdge
    {
        public int index;
        public Vertex sourceVertex;
        public Face face;
        public HalfEdge prevEdge;
        public HalfEdge nextEdge;
        public HalfEdge twinEdge;

        public HalfEdge(int index, Vertex sourceVertex)
        {
            this.index = index;
            this.sourceVertex = sourceVertex;
            sourceVertex.outgoingEdge = this;
        }
    }

    public class Face
    {
        public int index;
        public HalfEdge edge;

        public Face(int index, HalfEdge edge)
        {
            this.index = index;
            this.edge = edge;
        }
    }

    public class HalfEdgeMesh
    {
        public List<Vertex> vertices = new();
        public List<HalfEdge> edges = new();
        public List<Face> faces = new();

        public HalfEdgeMesh(Mesh mesh)
        {
            var meshVertices = mesh.vertices;
            var meshQuads = mesh.GetIndices(0);

            // First, get vertices
            for (var i = 0; i < meshVertices.Length; i++)
            {
                vertices.Add(new Vertex(i, meshVertices[i]));
            }

            Dictionary<(int, int), HalfEdge> edgesDictionary = new();
            // Second, build faces & edged
            for (var i = 0; i < meshQuads.Length; i += 4)
            {
                var i0 = meshQuads[i];
                var i1 = meshQuads[i + 1];
                var i2 = meshQuads[i + 2];
                var i3 = meshQuads[i + 3];

                var vert0 = vertices[i0];
                var vert1 = vertices[i1];
                var vert2 = vertices[i2];
                var vert3 = vertices[i3];

                var edge0 = new HalfEdge(i, vert0);
                var edge1 = new HalfEdge(i + 1, vert1);
                var edge2 = new HalfEdge(i + 2, vert2);
                var edge3 = new HalfEdge(i + 3, vert3);

                var face = new Face(i / 4, edge0);

                edge0.prevEdge = edge2.nextEdge = edge3;
                edge1.prevEdge = edge3.nextEdge = edge0;
                edge2.prevEdge = edge0.nextEdge = edge1;
                edge3.prevEdge = edge1.nextEdge = edge2;

                if (edgesDictionary.TryGetValue((i0, i1), out var e0))
                {
                    edge0.twinEdge = e0;
                    e0.twinEdge = edge0;
                }
                else if (edgesDictionary.TryGetValue((i1, i0), out var e1))
                {
                    edge0.twinEdge = e1;
                    e1.twinEdge = edge0;
                }
                else
                {
                    edgesDictionary.Add((i0, i1), edge0);
                }

                if (edgesDictionary.TryGetValue((i1, i2), out var e3))
                {
                    edge1.twinEdge = e3;
                    e3.twinEdge = edge1;
                }
                else if (edgesDictionary.TryGetValue((i2, i1), out var e4))
                {
                    edge1.twinEdge = e4;
                    e4.twinEdge = edge1;
                }
                else
                {
                    edgesDictionary.Add((i1, i2), edge1);
                }

                if (edgesDictionary.TryGetValue((i2, i3), out var e5))
                {
                    edge2.twinEdge = e5;
                    e5.twinEdge = edge2;
                }
                else if (edgesDictionary.TryGetValue((i3, i2), out var e6))
                {
                    edge2.twinEdge = e6;
                    e6.twinEdge = edge2;
                }
                else
                {
                    edgesDictionary.Add((i2, i3), edge2);
                }

                if (edgesDictionary.TryGetValue((i3, i0), out var e7))
                {
                    edge3.twinEdge = e7;
                    e7.twinEdge = edge3;
                }
                else if (edgesDictionary.TryGetValue((i0, i3), out var e8))
                {
                    edge3.twinEdge = e8;
                    e8.twinEdge = edge3;
                }
                else
                {
                    edgesDictionary.Add((i3, i0), edge3);
                }

                edge0.face = edge1.face = edge2.face = edge3.face = face;

                edges.Add(edge0);
                edges.Add(edge1);
                edges.Add(edge2);
                edges.Add(edge3);
                faces.Add(face);
            }
        }

        public Mesh ConvertToFaceVertexMesh()
        {
            Mesh faceVertexMesh = new Mesh();

            var meshVertices = new Vector3[vertices.Count];
            var meshQuads = new int[faces.Count * 4];

            var index = 0;
            foreach (var vertex in vertices)
            {
                meshVertices[index++] = vertex.position;
            }

            index = 0;
            foreach (var face in faces)
            {
                meshQuads[index++] = face.edge.sourceVertex.index;
                meshQuads[index++] = face.edge.nextEdge.sourceVertex.index;
                meshQuads[index++] = face.edge.nextEdge.nextEdge.sourceVertex.index;
                meshQuads[index++] = face.edge.nextEdge.nextEdge.nextEdge.sourceVertex.index;
            }

            faceVertexMesh.vertices = meshVertices;
            faceVertexMesh.SetIndices(meshQuads, MeshTopology.Quads, 0);

            return faceVertexMesh;
        }

        public string ConvertToCSVFormat(string separator = "\t")
        {
            string str = "";
            //magic happens
            return str;
        }

        private void DrawVertices(Transform transform)
        {
            foreach (var vertex in vertices)
            {
                var position = transform.TransformPoint(vertex.position);
                Gizmos.DrawSphere(position, .1f);
                var direction = (transform.TransformPoint(vertex.outgoingEdge.nextEdge.sourceVertex.position) - position).normalized;
                DrawArrow.ForGizmo(position, direction);
            }
        }

        private void DrawEdges(Transform transform)
        {
            foreach (var edge in edges)
            {
                var position = transform.TransformPoint(edge.sourceVertex.position);
                var direction = (transform.TransformPoint(edge.nextEdge.sourceVertex.position) - position).normalized;
                DrawArrow.ForGizmo(position, direction);
            }
        }

        private void DrawFaces(Transform transform)
        {
            foreach (var face in faces)
            {
                var pos0 = transform.TransformPoint(face.edge.sourceVertex.position);
                var pos1 = transform.TransformPoint(face.edge.nextEdge.sourceVertex.position);
                var pos2 = transform.TransformPoint(face.edge.nextEdge.nextEdge.sourceVertex.position);
                var pos3 = transform.TransformPoint(face.edge.nextEdge.nextEdge.nextEdge.sourceVertex.position);
                
                Gizmos.DrawLine(pos0, pos1);
                Gizmos.DrawLine(pos1, pos2);
                Gizmos.DrawLine(pos2, pos3);
                Gizmos.DrawLine(pos3, pos0);
                
                Gizmos.DrawLine(pos0, pos2);
                Gizmos.DrawLine(pos1, pos3);

                var position = face.edge.sourceVertex.position;
                var direction = (face.edge.nextEdge.sourceVertex.position - position).normalized;
                DrawArrow.ForGizmo(position, direction);
            }
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
    }
}