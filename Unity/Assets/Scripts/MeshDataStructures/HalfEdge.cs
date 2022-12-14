using System.Collections.Generic;
using System.Linq;
using Polygons;
using UnityEditor;
using UnityEngine;

namespace HalfEdge
{
    public delegate void EdgeDelegate(HalfEdge edge);

    public class Vertex
    {
        public int Index;
        public Vector3 Position;
        public HalfEdge OutgoingEdge;

        public bool InBorder
        {
            get
            {
                var yes = false;
                TraverseAdjacentEdges(edge =>
                {
                    if (edge.InBorder)
                        yes = true;
                });

                return yes;
            }
        }

        public Vertex(int index, Vector3 position)
        {
            Index = index;
            Position = position;
        }

        public void TraverseAdjacentEdges(EdgeDelegate edgeDelegate)
        {
            var start = OutgoingEdge;
            var currentEdge = start;
            do
            {
                edgeDelegate(currentEdge);
                currentEdge = currentEdge.PrevEdge.TwinEdge;
            } while (currentEdge != start);
        }
    }

    public class HalfEdge
    {
        public int Index;
        public Vertex SourceVertex;
        public Vertex EndVertex => NextEdge.SourceVertex;
        public Face Face;
        public HalfEdge PrevEdge;
        public HalfEdge NextEdge;
        public HalfEdge TwinEdge;
        public bool InBorder => Face == null || TwinEdge.Face == null;

        public HalfEdge(int index, Vertex sourceVertex)
        {
            Index = index;
            SourceVertex = sourceVertex;
            sourceVertex.OutgoingEdge ??= this;
        }

        public Vector3 GetCenter()
        {
            var pos0 = SourceVertex.Position;
            var pos1 = NextEdge.SourceVertex.Position;

            return (pos0 + pos1) / 2f;
        }
    }

    public class Face
    {
        public int Index;
        public HalfEdge Edge;

        public Face(int index, HalfEdge edge)
        {
            Index = index;
            Edge = edge;
        }

        public void TraverseEdges(EdgeDelegate edgeDelegate, bool clockwise = true)
        {
            var start = Edge;
            var currentEdge = start;
            do
            {
                edgeDelegate(currentEdge);
                currentEdge = clockwise ? currentEdge.NextEdge : currentEdge.PrevEdge;
            } while (currentEdge != start);
        }

        public Vector3 GetCentroid()
        {
            var sum = Vector3.zero;
            var iteration = 0;
            TraverseEdges(currentEdge =>
            {
                sum += currentEdge.SourceVertex.Position;
                iteration++;
            });
            return sum / iteration;
        }
    }

    public class HalfEdgeMesh
    {
        public List<Vertex> Vertices = new();
        public List<HalfEdge> Edges = new();
        public List<Face> Faces = new();

        public Vector3 GetCentroid()
        {
            var res = Vertices.Aggregate(Vector3.zero, (current, vertex) => current + vertex.Position);
            return res / Vertices.Count;
        }

        #region Base Methods

        public HalfEdgeMesh(Mesh mesh)
        {
            var meshVertices = mesh.vertices;
            var meshQuads = mesh.GetIndices(0);

            // First, get vertices
            for (var i = 0; i < meshVertices.Length; i++)
            {
                Vertices.Add(new Vertex(i, meshVertices[i]));
            }

            Dictionary<(int, int), HalfEdge> edgesDictionary = new();

            void CreateEdge(HalfEdge edge, Face face, Vertex startVertex, Vertex endVertex, HalfEdge previousEdge, HalfEdge nextEdge)
            {
                var tuple = (Mathf.Min(startVertex.Index, endVertex.Index), Mathf.Max(startVertex.Index, endVertex.Index));

                if (edgesDictionary.TryGetValue(tuple, out var other))
                {
                    edge.TwinEdge = other;
                    other.TwinEdge = edge;
                }
                else
                {
                    edgesDictionary.Add(tuple, edge);
                }

                Edges.Add(edge);
                edge.Face = face;
                edge.PrevEdge = previousEdge;
                edge.NextEdge = nextEdge;
            }

            // Second, build faces & edged
            for (var i = 0; i < meshQuads.Length; i += 4)
            {
                var i0 = meshQuads[i];
                var i1 = meshQuads[i + 1];
                var i2 = meshQuads[i + 2];
                var i3 = meshQuads[i + 3];

                var edge0 = new HalfEdge(i, Vertices[i0]);
                var edge1 = new HalfEdge(i + 1, Vertices[i1]);
                var edge2 = new HalfEdge(i + 2, Vertices[i2]);
                var edge3 = new HalfEdge(i + 3, Vertices[i3]);

                var face = new Face(Faces.Count, edge0);

                CreateEdge(edge0, face, Vertices[i0], Vertices[i1], edge3, edge1);
                CreateEdge(edge1, face, Vertices[i1], Vertices[i2], edge0, edge2);
                CreateEdge(edge2, face, Vertices[i2], Vertices[i3], edge1, edge3);
                CreateEdge(edge3, face, Vertices[i3], Vertices[i0], edge2, edge0);

                Faces.Add(face);
            }

            Dictionary<int, HalfEdge> startVertexEdgesDictionary = new();
            Dictionary<int, HalfEdge> endVertexEdgesDictionary = new();

            for (var i = 0; i < Edges.Count; i++)
            {
                if (Edges[i].TwinEdge != null) continue;

                var startVertex = Edges[i].EndVertex;
                var endVertex = Edges[i].SourceVertex;

                var twin = new HalfEdge(Edges.Count, startVertex)
                {
                    TwinEdge = Edges[i]
                };
                Edges[i].TwinEdge = twin;

                startVertexEdgesDictionary.Add(startVertex.Index, twin);
                endVertexEdgesDictionary.Add(endVertex.Index, twin);

                if (startVertexEdgesDictionary.TryGetValue(endVertex.Index, out var nextEdge))
                {
                    nextEdge.PrevEdge = twin;
                    twin.NextEdge = nextEdge;
                }

                if (endVertexEdgesDictionary.TryGetValue(startVertex.Index, out var previousEdge))
                {
                    previousEdge.NextEdge = twin;
                    twin.PrevEdge = previousEdge;
                }

                Edges.Add(twin);
            }
        }

        public Mesh ConvertToFaceVertexMesh()
        {
            Mesh faceVertexMesh = new Mesh();

            var meshVertices = new Vector3[Vertices.Count];
            var meshQuads = new List<int>();

            var index = 0;
            foreach (var vertex in Vertices)
            {
                meshVertices[index++] = vertex.Position;
            }

            foreach (var face in Faces)
            {
                face.TraverseEdges(currentEdge => meshQuads.Add(currentEdge.SourceVertex.Index));
            }

            faceVertexMesh.vertices = meshVertices;
            faceVertexMesh.SetIndices(meshQuads, MeshTopology.Quads, 0);

            return faceVertexMesh;
        }

        public string ConvertToCsv(string separator)
        {
            var strings = Vertices.Select((vertex, i) => $"{i}{separator}{vertex.Position.x:N03} {vertex.Position.y:N03} {vertex.Position.z:N03}{separator}{vertex.OutgoingEdge.Index}{separator}{separator}").ToList();

            for (var i = Vertices.Count; i < Edges.Count; i++)
                strings.Add(string.Format("{0}{0}{0}{0}", separator));

            for (var i = 0; i < Edges.Count; i++)
            {
                var faceIndex = Edges[i].Face != null ? Edges[i].Face.Index.ToString() : "∅";
                var vertexIndex = Edges[i].SourceVertex != null ? Edges[i].SourceVertex.Index.ToString() : "∅";
                var prevIndex = Edges[i].PrevEdge != null ? Edges[i].PrevEdge.Index.ToString() : "∅";
                var nextIndex = Edges[i].NextEdge != null ? Edges[i].NextEdge.Index.ToString() : "∅";
                var twinIndex = Edges[i].TwinEdge != null ? Edges[i].TwinEdge.Index.ToString() : "∅";
                strings[i] += $"{i}{separator}{vertexIndex}{separator}{faceIndex}{separator}{prevIndex}{separator}{nextIndex}{separator}{twinIndex}{separator}{separator}";
            }

            for (var i = Mathf.Max(Vertices.Count, Edges.Count); i < Faces.Count; i++)
                strings.Add(string.Format("{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}{0}", separator));

            for (var i = 0; i < Faces.Count; i++)
            {
                strings[i] += $"{i}{separator}{Faces[i].Edge.Index}";
            }

            return $"Vertices{separator}{separator}{separator}{separator}Half-Edges{separator}{separator}{separator}{separator}{separator}{separator}{separator}Faces\nIndex{separator}Position{separator}Outgoing-edge-index{separator}{separator}Index{separator}Vertex-index{separator}Face-index{separator}Prev-Edge{separator}Next-Edge{separator}Twin-Edge{separator}{separator}Index{separator}Edge-index\n{string.Join("\n", strings)}";
        }

        #endregion

        #region Catmull Clark Methods

        public void SubdivideCatmullClark()
        {
            CatmullClarkCreateNewPoints(out var facePoints, out var edgePoints, out var vertexPoints);

            for (var i = 0; i < Vertices.Count; i++)
            {
                Vertices[i].Position = vertexPoints[i];
            }

            HashSet<(int, int)> edgesDictionary = new();
            for (var i = 0; i < edgePoints.Count; i++)
            {
                var key = (Mathf.Min(Edges[i].SourceVertex.Index, Edges[i].EndVertex.Index), Mathf.Max(Edges[i].SourceVertex.Index, Edges[i].EndVertex.Index));

                if (!edgesDictionary.Contains(key))
                {
                    SplitEdge(Edges[i], edgePoints[i]);
                    edgesDictionary.Add((Mathf.Min(Edges[^2].SourceVertex.Index, Edges[^2].EndVertex.Index), Mathf.Max(Edges[^2].SourceVertex.Index, Edges[^2].EndVertex.Index)));
                }
            }

            foreach (var edge in Edges)
            {
                edge.NextEdge.PrevEdge = edge;
            }

            for (var i = 0; i < facePoints.Count; i++)
            {
                SplitFace(Faces[i], facePoints[i]);
            }

            foreach (var edge in Edges)
            {
                edge.NextEdge.PrevEdge = edge;
            }
        }

        public void CatmullClarkCreateNewPoints(out List<Vector3> facePoints, out List<Vector3> edgePoints, out List<Vector3> vertexPoints)
        {
            facePoints = new List<Vector3>();

            facePoints = Faces.Select(face => face.GetCentroid()).ToList();


            edgePoints = new List<Vector3>();

            foreach (var edge in Edges)
            {
                if (edge.InBorder)
                {
                    edgePoints.Add(edge.GetCenter());
                }
                else
                {
                    edgePoints.Add((edge.SourceVertex.Position + edge.EndVertex.Position + edge.Face.GetCentroid() + edge.TwinEdge.Face.GetCentroid()) / 4f);
                }
            }

            vertexPoints = new List<Vector3>();
            foreach (var vertex in Vertices)
            {
                if (vertex.InBorder)
                {
                    var sum = vertex.Position;
                    var nb = 1;

                    vertex.TraverseAdjacentEdges(edge =>
                    {
                        if (!edge.InBorder) return;
                        sum += edge.GetCenter();
                        nb++;
                    });

                    vertexPoints.Add(sum / nb);
                }
                else
                {
                    var n = 0;
                    var q = Vector3.zero;
                    var r = Vector3.zero;
                    var v = vertex.Position;

                    vertex.TraverseAdjacentEdges(edge =>
                    {
                        q += edge.Face.GetCentroid();
                        r += edge.GetCenter();
                        n++;
                    });

                    q /= n;
                    r /= n;

                    vertexPoints.Add((q + 2f * r + (n - 3f) * v) / n);
                }
            }
        }

        public void SplitEdge(HalfEdge edge, Vector3 splittingPoint)
        {
            var newVertex = new Vertex(Vertices.Count, splittingPoint);
            Vertices.Add(newVertex);

            var nextEdge = new HalfEdge(Edges.Count, newVertex)
            {
                NextEdge = edge.NextEdge,
                Face = edge.Face,
                TwinEdge = edge.TwinEdge
            };
            edge.NextEdge = nextEdge;
            Edges.Add(nextEdge);

            var nextTwinEdge = new HalfEdge(Edges.Count, newVertex)
            {
                NextEdge = edge.TwinEdge.NextEdge,
                Face = edge.TwinEdge.Face,
                TwinEdge = edge
            };
            edge.TwinEdge.NextEdge = nextTwinEdge;
            Edges.Add(nextTwinEdge);

            edge.TwinEdge.TwinEdge = nextEdge;
            edge.TwinEdge = nextTwinEdge;
        }

        public void SplitFace(Face face, Vector3 splittingPoint)
        {
            var newVertex = new Vertex(Vertices.Count, splittingPoint);
            Vertices.Add(newVertex);
            var start = face.Edge;
            var currentEdge = start;
            var index = 0;
            Dictionary<(int, int), HalfEdge> edgesDictionary = new();
            do
            {
                if (index % 2 == 0)
                {
                    var currentFace = face;

                    if (index != 0)
                    {
                        currentFace = new Face(Faces.Count, currentEdge);
                        Faces.Add(currentFace);
                    }

                    var previousEdge = new HalfEdge(Edges.Count, newVertex)
                    {
                        NextEdge = currentEdge.PrevEdge,
                        Face = currentFace,
                    };
                    var key = (Mathf.Min(previousEdge.SourceVertex.Index, previousEdge.EndVertex.Index), Mathf.Max(previousEdge.SourceVertex.Index, previousEdge.EndVertex.Index));
                    if (edgesDictionary.TryGetValue(key, out var previousEdgeTwin))
                    {
                        previousEdge.TwinEdge = previousEdgeTwin;
                        previousEdgeTwin.TwinEdge = previousEdge;
                    }
                    else
                    {
                        edgesDictionary.Add(key, previousEdge);
                    }

                    Edges.Add(previousEdge);

                    var nextEdge = new HalfEdge(Edges.Count, currentEdge.EndVertex)
                    {
                        NextEdge = previousEdge,
                        Face = currentFace,
                    };
                    key = (Mathf.Min(nextEdge.SourceVertex.Index, nextEdge.EndVertex.Index), Mathf.Max(nextEdge.SourceVertex.Index, nextEdge.EndVertex.Index));
                    if (edgesDictionary.TryGetValue(key, out var nextEdgeTwin))
                    {
                        nextEdge.TwinEdge = nextEdgeTwin;
                        nextEdgeTwin.TwinEdge = nextEdge;
                    }
                    else
                    {
                        edgesDictionary.Add(key, nextEdge);
                    }

                    Edges.Add(nextEdge);
                    currentEdge.NextEdge = nextEdge;
                    currentEdge.Face = currentEdge.PrevEdge.Face = currentFace;
                }

                index++;
                currentEdge = currentEdge.PrevEdge;
            } while (currentEdge != start);
        }

        #endregion

        #region Gizmos Methods

        private void DrawVertices(bool draw, bool drawHandles, Highlight highlight, int index, Transform transform)
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

            foreach (var vertex in Vertices)
            {
                var position = transform.TransformPoint(vertex.Position);
                var highlightVertex = highlight == Highlight.Vertex && vertex.Index == index;
                var drawVertex = draw || highlightVertex;

                if (drawVertex)
                {
                    Gizmos.DrawSphere(position, highlightVertex ? .25f : .1f);
                    if (drawHandles || highlightVertex)
                        Handles.Label(position, $"Vertex {vertex.Index}", style);
                    if (highlightVertex)
                    {
                        var start = transform.TransformPoint(vertex.OutgoingEdge.SourceVertex.Position);
                        var end = transform.TransformPoint(vertex.OutgoingEdge.EndVertex.Position);
                        DrawArrow.ForGizmo(start, Vector3.Lerp(start, end, .9f) - start, .1f);
                    }
                }
            }
        }

        private void DrawEdges(bool draw, bool drawHandles, Highlight highlight, int index, Transform transform)
        {
            foreach (var edge in Edges)
            {
                var highlightEdge = highlight == Highlight.Edge && edge.Index == index;
                var drawEdge = draw || highlightEdge;

                if (drawEdge)
                {
                    var isBorder = edge.Face == null;

                    var centroid = transform.TransformPoint(isBorder ? edge.TwinEdge.Face.GetCentroid() : edge.Face.GetCentroid());
                    var p0 = transform.TransformPoint(edge.SourceVertex.Position);
                    var p1 = transform.TransformPoint(edge.EndVertex.Position);
                    var center = (p0 + p1) / 2f;
                    var start = Vector3.Lerp(p0, p1, .1f);
                    var end = Vector3.Lerp(p0, p1, .9f);

                    var perpendicular = (isBorder ? center - centroid : centroid - center).normalized;

                    Gizmos.color = isBorder ? Color.red : Color.blue;
                    DrawArrow.ForGizmo(start + (perpendicular * .1f), end - start, .1f);
                    if (drawHandles || highlightEdge)
                        Handles.Label(center + (perpendicular * .1f), $"Edge {edge.Index}", new GUIStyle
                        {
                            fontSize = 15,
                            normal =
                            {
                                textColor = Gizmos.color
                            }
                        });
                }
            }
        }

        private void DrawFaces(bool draw, bool drawHandles, Highlight highlight, int index, Transform transform)
        {
            var style = new GUIStyle
            {
                fontSize = 15,
                normal =
                {
                    textColor = Color.green
                }
            };

            foreach (var face in Faces)
            {
                var highlightFace = highlight == Highlight.Face && face.Index == index;
                var drawFace = draw || highlightFace;

                if (drawFace)
                {
                    Gizmos.color = Color.green;
                    face.TraverseEdges(currentEdge => Gizmos.DrawLine(transform.TransformPoint(currentEdge.SourceVertex.Position), transform.TransformPoint(currentEdge.NextEdge.SourceVertex.Position)));
                    if (drawHandles || highlightFace)
                        Handles.Label(transform.TransformPoint(face.GetCentroid()), $"Face {face.Index}", style);
                }
            }
        }

        public void DrawGizmos(bool drawVertices, bool drawEdges, bool drawFaces, bool drawCentroid, bool drawHandles, Highlight highlight, int highlightIndex, Transform transform)
        {
            DrawVertices(drawVertices, drawHandles, highlight, highlightIndex, transform);
            DrawEdges(drawEdges, drawHandles, highlight, highlightIndex, transform);
            DrawFaces(drawFaces, drawHandles, highlight, highlightIndex, transform);

            if (drawCentroid)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(transform.TransformPoint(GetCentroid()), .5f);
            }
        }

        #endregion
    }
}