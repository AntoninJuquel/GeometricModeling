using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HalfEdge
{
    public delegate void EdgeDelegate(HalfEdge edge);

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

        public void TraverseAdjacentEdges(EdgeDelegate edgeDelegate)
        {
            var start = outgoingEdge;
            var currentEdge = start;
            do
            {
                edgeDelegate(currentEdge);
                currentEdge = currentEdge.prevEdge.twinEdge;
            } while (currentEdge != start);
        }
    }

    public class HalfEdge
    {
        public int index;
        public Vertex sourceVertex;
        public Vertex endVertex => nextEdge.sourceVertex;
        public Face face;
        public HalfEdge prevEdge;
        public HalfEdge nextEdge;
        public HalfEdge twinEdge;

        public HalfEdge(int index, Vertex sourceVertex)
        {
            this.index = index;
            this.sourceVertex = sourceVertex;
            sourceVertex.outgoingEdge ??= this;
        }

        public Vector3 GetCenter()
        {
            var pos0 = sourceVertex.position;
            var pos1 = nextEdge.sourceVertex.position;

            return (pos0 + pos1) / 2f;
        }

        public Vector3 GetCenter(out Vector3 pos0, out Vector3 pos1)
        {
            pos0 = sourceVertex.position;
            pos1 = nextEdge.sourceVertex.position;

            return (pos0 + pos1) / 2f;
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

        public void TraverseEdges(EdgeDelegate edgeDelegate)
        {
            var start = edge;
            var currentEdge = start;
            do
            {
                edgeDelegate(currentEdge);
                currentEdge = currentEdge.nextEdge;
            } while (currentEdge != start);
        }

        public Vector3 GetCentroid()
        {
            var sum = Vector3.zero;
            var iteration = 0;
            TraverseEdges(currentEdge =>
            {
                sum += currentEdge.sourceVertex.position;
                iteration++;
            });
            return sum / iteration;
        }
    }

    [System.Serializable]
    public class HalfEdgeMesh
    {
        public List<Vertex> vertices = new();
        public List<HalfEdge> edges = new();
        public List<Face> faces = new();

        public Vector3 GetCentroid()
        {
            var res = vertices.Aggregate(Vector3.zero, (current, vertex) => current + vertex.position);
            return res / vertices.Count;
        }

        public HalfEdgeMesh(Mesh mesh)
        {
            var meshVertices = mesh.vertices;
            var meshQuads = mesh.GetIndices(0);

            // First, get vertices
            for (var i = 0; i < meshVertices.Length; i++)
            {
                vertices.Add(new Vertex(i, meshVertices[i]));
                VertexInspectors.Add(new VertexInspector());
            }

            Dictionary<(int, int), HalfEdge> edgesDictionary = new();
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

                var edge0 = new HalfEdge(i, vertices[i0]);
                var edge1 = new HalfEdge(i + 1, vertices[i1]);
                var edge2 = new HalfEdge(i + 2, vertices[i2]);
                var edge3 = new HalfEdge(i + 3, vertices[i3]);

                var face = new Face(faces.Count, edge0);

                edge0.prevEdge = edge2.nextEdge = edge3;
                edge1.prevEdge = edge3.nextEdge = edge0;
                edge2.prevEdge = edge0.nextEdge = edge1;
                edge3.prevEdge = edge1.nextEdge = edge2;

                if (edgesDictionary.TryGetValue(i0I1, out var e0))
                {
                    edge0.twinEdge = e0;
                    e0.twinEdge = edge0;
                }
                else
                {
                    edgesDictionary.Add(i0I1, edge0);
                }

                if (edgesDictionary.TryGetValue(i1I2, out var e1))
                {
                    edge1.twinEdge = e1;
                    e1.twinEdge = edge1;
                }
                else
                {
                    edgesDictionary.Add(i1I2, edge1);
                }

                if (edgesDictionary.TryGetValue(i2I3, out var e2))
                {
                    edge2.twinEdge = e2;
                    e2.twinEdge = edge2;
                }
                else
                {
                    edgesDictionary.Add(i2I3, edge2);
                }

                if (edgesDictionary.TryGetValue(i3I0, out var e3))
                {
                    edge3.twinEdge = e3;
                    e3.twinEdge = edge3;
                }
                else
                {
                    edgesDictionary.Add(i3I0, edge3);
                }

                edge0.face = edge1.face = edge2.face = edge3.face = face;

                edges.Add(edge0);
                edges.Add(edge1);
                edges.Add(edge2);
                edges.Add(edge3);
                faces.Add(face);
            }

            Dictionary<int, HalfEdge> startVertexEdgesDictionary = new();
            Dictionary<int, HalfEdge> endVertexEdgesDictionary = new();

            for (var i = 0; i < edges.Count; i++)
            {
                if (edges[i].twinEdge != null) continue;

                var startVertex = edges[i].endVertex;
                var endVertex = edges[i].sourceVertex;

                var twin = new HalfEdge(edges.Count, startVertex)
                {
                    twinEdge = edges[i]
                };
                edges[i].twinEdge = twin;

                startVertexEdgesDictionary.Add(startVertex.index, twin);
                endVertexEdgesDictionary.Add(endVertex.index, twin);

                if (startVertexEdgesDictionary.TryGetValue(endVertex.index, out var nextEdge))
                {
                    nextEdge.prevEdge = twin;
                    twin.nextEdge = nextEdge;
                }

                if (endVertexEdgesDictionary.TryGetValue(startVertex.index, out var previousEdge))
                {
                    previousEdge.nextEdge = twin;
                    twin.prevEdge = previousEdge;
                }

                edges.Add(twin);
            }

            GUIUtility.systemCopyBuffer = ConvertToCsv("\t");
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
                face.TraverseEdges(currentEdge => meshQuads[index++] = currentEdge.index);
            }

            faceVertexMesh.vertices = meshVertices;
            faceVertexMesh.SetIndices(meshQuads, MeshTopology.Quads, 0);

            return faceVertexMesh;
        }

        private string ConvertToCsv(string separator)
        {
            var strings = vertices.Select((vertex, i) => $"{i}{separator}{vertex.position.x:N03} {vertex.position.y:N03} {vertex.position.z:N03}{separator}{vertex.outgoingEdge.index}{separator}{separator}").ToList();

            for (var i = vertices.Count; i < edges.Count; i++)
                strings.Add(string.Format("{0}{0}{0}{0}", separator));

            for (var i = 0; i < edges.Count; i++)
            {
                var faceIndex = edges[i].face != null ? edges[i].face.index.ToString() : "∅";
                strings[i] += $"{i}{separator}{edges[i].sourceVertex.index}{separator}{faceIndex}{separator}{edges[i].prevEdge.index}{separator}{edges[i].nextEdge.index}{separator}{edges[i].twinEdge.index}{separator}{separator}";
            }

            for (var i = Mathf.Max(vertices.Count, edges.Count); i < faces.Count; i++)
                strings.Add(string.Format("{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}", separator));

            for (var i = 0; i < faces.Count; i++)
            {
                strings[i] += $"{i}{separator}{faces[i].edge.index}";
            }

            return $"Vertices{separator}{separator}{separator}{separator}Half-Edges{separator}{separator}{separator}{separator}{separator}{separator}{separator}Faces\nIndex{separator}Position{separator}Outgoing-edge-index{separator}{separator}Index{separator}Vertex-index{separator}Face-index{separator}Prev-Edge{separator}Next-Edge{separator}Twin-Edge{separator}{separator}Index{separator}Edge-index\n{string.Join("\n", strings)}";
        }

        private void DrawVertices(Transform transform)
        {
            var style = new GUIStyle
            {
                fontSize = 15,
                normal =
                {
                    textColor = Color.black
                }
            };

            foreach (var vertex in vertices)
            {
                var position = transform.TransformPoint(vertex.position);
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(position, .1f);
                Handles.Label(position, $"Vertex {vertex.index}", style);
            }
        }

        private void DrawEdges(Transform transform)
        {
            foreach (var edge in edges)
            {
                var isBorder = edge.face == null;

                var centroid = transform.TransformPoint(isBorder ? edge.twinEdge.face.GetCentroid() : edge.face.GetCentroid());
                var p0 = transform.TransformPoint(edge.sourceVertex.position);
                var p1 = transform.TransformPoint(edge.endVertex.position);
                var center = (p0 + p1) / 2f;

                var perpendicular = (isBorder ? center - centroid : centroid - center).normalized;

                var direction = (p1 - p0);
                Gizmos.color = isBorder ? Color.red : Color.blue;
                DrawArrow.ForGizmo(p0 + (perpendicular * .1f), direction, .1f);
                Handles.Label(center + (perpendicular * .1f), $"Edge {edge.index}", new GUIStyle
                {
                    fontSize = 15,
                    normal =
                    {
                        textColor = Gizmos.color
                    }
                });
            }
        }

        private void DrawFaces(Transform transform)
        {
            var style = new GUIStyle
            {
                fontSize = 15,
                normal =
                {
                    textColor = Color.green
                }
            };

            foreach (var face in faces)
            {
                Gizmos.color = Color.green;
                face.TraverseEdges(currentEdge => Gizmos.DrawLine(transform.TransformPoint(currentEdge.sourceVertex.position), transform.TransformPoint(currentEdge.nextEdge.sourceVertex.position)));
                Handles.Label(transform.TransformPoint(face.GetCentroid()), $"Face {face.index}", style);
            }
        }

        [System.Serializable]
        public struct VertexInspector
        {
            public bool drawGizmos;
        }

        [SerializeField] public List<VertexInspector> VertexInspectors = new();

        public void DrawGizmos(bool drawVertices, bool drawEdges, bool drawFaces, bool drawCentroid, Transform transform)
        {
            for (var i = 0; i < VertexInspectors.Count; i++)
            {
                if (!VertexInspectors[i].drawGizmos) continue;
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.TransformPoint(vertices[i].position), .1f);
                vertices[i].TraverseAdjacentEdges(currentEdge => { Gizmos.DrawLine(transform.TransformPoint(currentEdge.sourceVertex.position), transform.TransformPoint(currentEdge.endVertex.position)); });
            }

            if (drawVertices)
                DrawVertices(transform);
            if (drawEdges)
                DrawEdges(transform);
            if (drawFaces)
                DrawFaces(transform);
            if (drawCentroid)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(transform.TransformPoint(GetCentroid()), .5f);
            }
        }
    }
}