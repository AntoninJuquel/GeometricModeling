using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WingedEdge
{
    public delegate void EdgeDelegate(WingedEdge edge);

    public class Vertex
    {
        public int Index;
        public Vector3 Position;
        public WingedEdge Edge;

        public void TraverseEdges(EdgeDelegate edgeDelegate, bool clockwise = true)
        {
            var start = Edge;
            var currentEdge = start;
            do
            {
                edgeDelegate(currentEdge);
                if (currentEdge.StartVertex == this)
                {
                    currentEdge = clockwise ? currentEdge.StartCwEdge : currentEdge.StartCcwEdge;
                }
                else
                {
                    currentEdge = clockwise ? currentEdge.EndCwEdge : currentEdge.EndCcwEdge;
                }
            } while (currentEdge != start);
        }
    }

    public class WingedEdge
    {
        public int Index;

        public Vertex StartVertex;
        public WingedEdge StartCwEdge;
        public WingedEdge StartCcwEdge;

        public Vertex EndVertex;
        public WingedEdge EndCwEdge;
        public WingedEdge EndCcwEdge;

        public Face RightFace;
        public Face LeftFace;
    }

    public class Face
    {
        public int Index;
        public WingedEdge Edge;

        public void TraverseEdges(EdgeDelegate edgeDelegate, bool clockwise = true)
        {
            var start = Edge;
            var currentEdge = start;
            do
            {
                edgeDelegate(currentEdge);
                if (currentEdge.RightFace == this)
                {
                    currentEdge = clockwise ? currentEdge.StartCwEdge : currentEdge.EndCcwEdge;
                }
                else
                {
                    currentEdge = clockwise ? currentEdge.EndCwEdge : currentEdge.StartCcwEdge;
                }
            } while (currentEdge != start);
        }

        public Vector3 GetCentroid()
        {
            var sum = Vector3.zero;
            var iteration = 0;
            TraverseEdges(currentEdge =>
            {
                if (currentEdge.RightFace == this)
                {
                    sum += currentEdge.StartVertex.Position;
                }
                else
                {
                    sum += currentEdge.EndVertex.Position;
                }

                iteration++;
            });
            return sum / iteration;
        }
    }

    public class WingedEdgeMesh
    {
        public List<Vertex> vertices = new();
        public List<WingedEdge> edges = new();
        public List<Face> faces = new();

        public Vector3 GetCentroid()
        {
            var res = vertices.Aggregate(Vector3.zero, (current, vertex) => current + vertex.Position);
            return res / vertices.Count;
        }

        #region Base Methods

        public WingedEdgeMesh(Mesh mesh)
        {
            var meshVertices = mesh.vertices;
            var meshQuads = mesh.GetIndices(0);

            // First, get vertices
            for (var i = 0; i < meshVertices.Length; i++)
            {
                vertices.Add(new Vertex()
                {
                    Index = i,
                    Position = meshVertices[i]
                });
            }

            Dictionary<(int, int), WingedEdge> edgesDictionary = new();

            WingedEdge CreateEdge(Face face, Vertex startVertex, Vertex endVertex, out bool newEdge)
            {
                var tuple = (Mathf.Min(startVertex.Index, endVertex.Index), Mathf.Max(startVertex.Index, endVertex.Index));
                WingedEdge edge;

                if (edgesDictionary.TryGetValue(tuple, out var other))
                {
                    edge = other;
                    edge.LeftFace = face;
                }
                else
                {
                    edge = new WingedEdge
                    {
                        Index = edges.Count,
                        StartVertex = startVertex,
                        EndVertex = endVertex,
                        RightFace = face
                    };
                    edgesDictionary.Add(tuple, edge);
                    edges.Add(edge);
                }

                newEdge = other == null;
                return edge;
            }

            void SetNeighbours(WingedEdge edge, WingedEdge previousEdge, WingedEdge nextEdge, bool newEdge)
            {
                if (!newEdge)
                {
                    edge.EndCwEdge = previousEdge;
                    edge.StartCcwEdge = nextEdge;
                }
                else
                {
                    edge.StartCwEdge = previousEdge;
                    edge.EndCcwEdge = nextEdge;
                }
            }

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

                var face = new Face()
                {
                    Index = i / 4,
                };

                var edge0 = CreateEdge(face, vert0, vert1, out var new0);
                var edge1 = CreateEdge(face, vert1, vert2, out var new1);
                var edge2 = CreateEdge(face, vert2, vert3, out var new2);
                var edge3 = CreateEdge(face, vert3, vert0, out var new3);

                SetNeighbours(edge0, edge3, edge1, new0);
                SetNeighbours(edge1, edge0, edge2, new1);
                SetNeighbours(edge2, edge1, edge3, new2);
                SetNeighbours(edge3, edge2, edge0, new3);

                face.Edge = edge0;
                faces.Add(face);
            }

            foreach (var edge in edges)
            {
                edge.StartVertex.Edge ??= edge;

                if (edge.EndCwEdge != null) continue;

                var currentEdge = edge.EndCcwEdge;
                var isLastEdge = false;
                while (!isLastEdge)
                {
                    if (currentEdge.StartVertex != edge.EndVertex)
                    {
                        currentEdge = currentEdge.EndCcwEdge;
                    }
                    else if (currentEdge.StartCcwEdge != null)
                    {
                        currentEdge = currentEdge.StartCcwEdge;
                    }
                    else
                    {
                        isLastEdge = true;
                    }
                }

                edge.EndCwEdge = currentEdge;
                currentEdge.StartCcwEdge = edge;
            }
        }

        public Mesh ConvertToFaceVertexMesh()
        {
            var faceVertexMesh = new Mesh();

            var meshVertices = new Vector3[vertices.Count];
            var meshQuads = new List<int>();

            var index = 0;
            foreach (var vertex in vertices)
            {
                meshVertices[index++] = vertex.Position;
            }

            foreach (var face in faces)
            {
                face.TraverseEdges(currentEdge => meshQuads.Add(currentEdge.StartVertex.Index));
            }

            faceVertexMesh.vertices = meshVertices;
            faceVertexMesh.SetIndices(meshQuads, MeshTopology.Quads, 0);

            return faceVertexMesh;
        }

        public string ConvertToCsv(string separator)
        {
            var strings = vertices.Select((vertex, i) => $"{i}{separator}{vertex.Position.x:N03} {vertex.Position.y:N03} {vertex.Position.z:N03}{separator}{vertex.Edge.Index}{separator}{separator}").ToList();

            for (var i = vertices.Count; i < edges.Count; i++)
                strings.Add(string.Format("{0}{0}{0}{0}", separator));

            for (var i = 0; i < edges.Count; i++)
            {
                var startVertexIndex = edges[i].StartVertex.Index;
                var endVertexIndex = edges[i].EndVertex.Index;
                var startCwEdgeIndex = edges[i].StartCwEdge.Index;
                var startCcwEdgeIndex = edges[i].StartCcwEdge.Index;
                var endCwEdgeIndex = edges[i].EndCwEdge.Index;
                var endCcwEdgeIndex = edges[i].EndCcwEdge.Index;
                var rightFaceIndex = edges[i].RightFace != null ? edges[i].RightFace.Index.ToString() : "∅";
                var leftFaceIndex = edges[i].LeftFace != null ? edges[i].LeftFace.Index.ToString() : "∅";
                strings[i] += $"{i}{separator}{startVertexIndex}{separator}{endVertexIndex}{separator}{startCwEdgeIndex}{separator}{startCcwEdgeIndex}{separator}{endCwEdgeIndex}{separator}{endCcwEdgeIndex}{separator}{rightFaceIndex}{separator}{leftFaceIndex}{separator}{separator}";
            }

            for (var i = Mathf.Max(vertices.Count, edges.Count); i < faces.Count; i++)
                strings.Add(string.Format("{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}", separator));

            for (var i = 0; i < faces.Count; i++)
            {
                strings[i] += $"{i}{separator}{faces[i].Edge.Index}";
            }

            return $"Vertices{separator}{separator}{separator}{separator}Winged-Edges{separator}{separator}{separator}{separator}{separator}{separator}{separator}{separator}{separator}{separator}Faces\nIndex{separator}Position{separator}Edge-index{separator}{separator}Index{separator}Start-Vertex-Index{separator}End-Vertex-Index{separator}Start-CW-Edge{separator}Start-CCW-Edge{separator}End-CW-Edge{separator}End-CCW-Edge{separator}Right-Face-Index{separator}Left-Face-Index{separator}{separator}Index{separator}Edge-index\n{string.Join("\n", strings)}";
        }

        #endregion

        #region Gizmos Methods

        private void DrawFaces(bool drawHandles, Transform transform)
        {
            Gizmos.color = Color.green;

            var style = new GUIStyle
            {
                fontSize = 15,
                normal =
                {
                    textColor = Gizmos.color
                }
            };

            foreach (var face in faces)
            {
                face.TraverseEdges(currentEdge => Gizmos.DrawLine(transform.TransformPoint(currentEdge.StartVertex.Position), transform.TransformPoint(currentEdge.EndVertex.Position)));
                if (drawHandles)
                    Handles.Label(transform.TransformPoint(face.GetCentroid()), $"Face {face.Index}", style);
            }
        }

        private void DrawEdges(bool drawHandles, Transform transform)
        {
            Gizmos.color = Color.blue;

            var style = new GUIStyle
            {
                fontSize = 15,
                normal =
                {
                    textColor = Gizmos.color
                }
            };

            foreach (var edge in edges)
            {
                var p0 = transform.TransformPoint(edge.StartVertex.Position);
                var p1 = transform.TransformPoint(edge.EndVertex.Position);
                var start = Vector3.Lerp(p0, p1, .1f);
                var end = Vector3.Lerp(p0, p1, .9f);
                var center = (p0 + p1) / 2f;

                DrawArrow.ForGizmo(start, end - start, .1f);
                if (drawHandles)
                    Handles.Label(center, $"Edge {edge.Index}", style);
            }
        }

        private void DrawVertices(bool drawHandles, Transform transform)
        {
            Gizmos.color = Color.black;

            var style = new GUIStyle
            {
                fontSize = 15,
                normal =
                {
                    textColor = Gizmos.color
                }
            };

            foreach (var vertex in vertices)
            {
                var position = transform.TransformPoint(vertex.Position);
                Gizmos.DrawSphere(position, .1f);
                if (drawHandles)
                    Handles.Label(position, $"Vertex {vertex.Index}", style);
            }
        }

        public void DrawGizmos(bool drawVertices, bool drawEdges, bool drawFaces, bool drawCentroid, bool drawHandles, Transform transform)
        {
            if (drawVertices)
                DrawVertices(drawHandles, transform);
            if (drawEdges)
                DrawEdges(drawHandles, transform);
            if (drawFaces)
                DrawFaces(drawHandles, transform);
            if (drawCentroid)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.TransformPoint(GetCentroid()), .5f);
            }
        }

        #endregion
    }
}