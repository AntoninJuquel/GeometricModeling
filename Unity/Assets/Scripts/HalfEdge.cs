﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace HalfEdge
{
    [Serializable]
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
        }
    }

    [Serializable]
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

    [Serializable]
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

    [Serializable]
    public class HalfEdgeMesh
    {
        public List<Vertex> vertices = new();
        public List<HalfEdge> edges = new();
        public List<Face> faces = new();

        public HalfEdgeMesh(Mesh mesh)
        {
            var meshVertices = mesh.vertices;
            var meshQuads = mesh.GetIndices(0);

            for (var i = 0; i < meshQuads.Length; i += 4)
            {
                var i0 = meshQuads[i];
                var i1 = meshQuads[i + 1];
                var i2 = meshQuads[i + 2];
                var i3 = meshQuads[i + 3];

                var vert0 = vertices.Find(v => v.index == i0) ?? new Vertex(i0, meshVertices[i0]);
                var vert1 = vertices.Find(v => v.index == i1) ?? new Vertex(i1, meshVertices[i1]);
                var vert2 = vertices.Find(v => v.index == i2) ?? new Vertex(i2, meshVertices[i2]);
                var vert3 = vertices.Find(v => v.index == i3) ?? new Vertex(i3, meshVertices[i3]);

                var edge0 = new HalfEdge(i, vert0);
                var edge1 = new HalfEdge(i + 1, vert1);
                var edge2 = new HalfEdge(i + 2, vert2);
                var edge3 = new HalfEdge(i + 3, vert3);

                var face = new Face(i, edge0);
                
                edge0.prevEdge = edge2.nextEdge = edge3;
                edge1.prevEdge = edge3.nextEdge = edge0;
                edge2.prevEdge = edge0.nextEdge = edge1;
                edge3.prevEdge = edge1.nextEdge =  edge2;

                edge0.face = edge1.face = edge2.face = edge3.face = face;
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

        public void DrawGizmos(bool drawVertices, bool drawEdges, bool drawFaces)
        {
            //magic happens
        }
    }
}