using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGeneratorTriangles : MonoBehaviour
{
    MeshFilter m_Mf;

    void Start()
    {
        m_Mf = GetComponent<MeshFilter>();
        //m_Mf.mesh = CreateTriangle();
        // m_Mf.mesh = CreateStrip(7, new Vector3(4, 1, 3));
        m_Mf.mesh = CreateGridXZ(7, 4, new Vector3(3, 1, 3));
    }

    Mesh CreateTriangle()
    {
        Mesh mesh = new Mesh();
        mesh.name = "triangle";

        Vector3[] vertices = new Vector3[3];
        int[] triangles = new int[1 * 3];

        vertices[0] = Vector3.right; // (1,0,0)
        vertices[1] = Vector3.up; // (0,1,0)
        vertices[2] = Vector3.forward; // (0,0,1)

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }

    Mesh CreateQuad(Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "quad";

        Vector3[] vertices = new Vector3[4];
        int[] triangles = new int[2 * 3];

        vertices[0] = new Vector3(-halfSize.x, 0, -halfSize.z);
        vertices[1] = new Vector3(-halfSize.x, 0, halfSize.z);
        vertices[2] = new Vector3(halfSize.x, 0, halfSize.z);
        vertices[3] = new Vector3(halfSize.x, 0, -halfSize.z);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }

    Mesh CreateStrip(int nSegments, Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "strip";

        Vector3[] vertices = new Vector3[(nSegments + 1) * 2];
        int[] triangles = new int[nSegments * 2 * 3];

        int index = 0;
        Vector3 leftTopPos = new Vector3(-halfSize.x, 0, halfSize.z);
        Vector3 rightTopPos = new Vector3(halfSize.x, 0, halfSize.z);

        // 1 boucle for pour remplir vertices
        for (int i = 0; i < nSegments + 1; i++)
        {
            float k = (float) i / nSegments;

            Vector3 tmpPos = Vector3.Lerp(leftTopPos, rightTopPos, k);
            vertices[index++] = tmpPos; // vertice du haut
            vertices[index++] = tmpPos - 2 * halfSize.z * Vector3.forward; // vertice du bas
        }

        // 1 boucle for pour remplir triangles
        index = 0;
        for (int i = 0; i < nSegments; i++)
        {
            triangles[index++] = 2 * i;
            triangles[index++] = 2 * i + 2;
            triangles[index++] = 2 * i + 1;

            triangles[index++] = 2 * i + 1;
            triangles[index++] = 2 * i + 2;
            triangles[index++] = 2 * i + 3;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }

    Mesh CreateGridXZ(int nSegmentsX, int nSegmentsZ, Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "grid";

        var vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        var triangles = new int[nSegmentsX * nSegmentsZ * 6];

        var leftPos = new Vector3(-halfSize.x, 0, 0);
        var topPos = new Vector3(0, 0, halfSize.z);

        var index = 0;
        for (var z = 0; z < nSegmentsZ + 1; z++)
        {
            var i = (float) z / nSegmentsZ;
            var vPos = Vector3.Lerp(topPos, -topPos, i);
            for (var x = 0; x < nSegmentsX + 1; x++)
            {
                var j = (float) x / nSegmentsX;
                var hPos = Vector3.Lerp(leftPos, -leftPos, j);
                vertices[index++] = vPos + hPos;
            }
        }

        index = 0;
        for (var z = 0; z < nSegmentsZ; z++)
        {
            for (var x = 0; x < nSegmentsX; x++)
            {
                triangles[index++] = x + z * (nSegmentsX + 1);
                triangles[index++] = x + z * (nSegmentsX + 1) + 1;
                triangles[index++] = x + z * (nSegmentsX + 1) + (nSegmentsX + 1);
        
                triangles[index++] = x + z * (nSegmentsX + 1) + 1;
                triangles[index++] = x + z * (nSegmentsX + 1) + 1 + (nSegmentsX + 1);
                triangles[index++] = x + z * (nSegmentsX + 1) + (nSegmentsX + 1);
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }
}