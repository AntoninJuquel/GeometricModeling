using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshGeneratorQuads : MonoBehaviour
{
    MeshFilter m_Mf;

    void Start()
    {
        m_Mf = GetComponent<MeshFilter>();
        //m_Mf.mesh = CreateStrip(7, new Vector3(4, 1, 3));
        m_Mf.mesh = CreateGridXZ(7, 7, new Vector3(4, 1, 3));
    }

    Mesh CreateStrip(int nSegments, Vector3 halfSize)
    {
        Mesh mesh = new Mesh();
        mesh.name = "strip";

        Vector3[] vertices = new Vector3[(nSegments + 1) * 2];
        int[] quads = new int[nSegments * 4];

        int index = 0;
        Vector3 leftTopPos = new Vector3(-halfSize.x, 0, halfSize.z);
        Vector3 rightTopPos = new Vector3(halfSize.x, 0, halfSize.z);

        // 1 boucle for pour remplir vertices
        for (int i = 0; i < nSegments + 1; i++)
        {
            float k = (float)i / nSegments;

            Vector3 tmpPos = Vector3.Lerp(leftTopPos, rightTopPos, k);
            vertices[index++] = tmpPos; // vertice du haut
            vertices[index++] = tmpPos - 2 * halfSize.z * Vector3.forward; // vertice du bas
        }

        // 1 boucle for pour remplir triangles
        index = 0;
        for (int i = 0; i < nSegments; i++)
        {
            quads[index++] = 2 * i;
            quads[index++] = 2 * i + 2;
            quads[index++] = 2 * i + 3;
            quads[index++] = 2 * i + 1;
        }

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }


    Mesh CreateGridXZ(int nSegmentsX, int nSegmentsZ, Vector3 halfSize)
    {
        var mesh = new Mesh();
        mesh.name = "grid";

        var vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        var quads = new int[nSegmentsX * nSegmentsZ * 4];

        var leftPos = new Vector3(-halfSize.x, 0, 0);
        var topPos = new Vector3(0, 0, halfSize.z);

        var index = 0;
        for (var z = 0; z < nSegmentsZ + 1; z++)
        {
            var i = (float)z / nSegmentsZ;
            var vPos = Vector3.Lerp(topPos, -topPos, i);
            for (var x = 0; x < nSegmentsX + 1; x++)
            {
                var j = (float)x / nSegmentsX;
                var hPos = Vector3.Lerp(leftPos, -leftPos, j);
                vertices[index++] = vPos + hPos;
            }
        }

        // 1 boucle for pour remplir triangles
        index = 0;
        for (int i = 0; i < nSegmentsX * (nSegmentsZ - 1); i++)
        {
            if(i%nSegmentsX==0) continue;
            quads[index++] = i;
            quads[index++] = i + 1;
            quads[index++] = i + nSegmentsX + 2;
            quads[index++] = i + nSegmentsX + 1;
        }

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }


    private void OnDrawGizmos()
    {
        if (!(m_Mf && m_Mf.mesh)) return;

        Mesh mesh = m_Mf.mesh;
        Vector3[] vertices = mesh.vertices;
        int[] quads = mesh.GetIndices(0);

        GUIStyle style = new GUIStyle();
        style.fontSize = 15;
        style.normal.textColor = Color.red;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);
            Handles.Label(worldPos, i.ToString(), style);
        }

        Gizmos.color = Color.black;
        style.normal.textColor = Color.blue;

        for (int i = 0; i < quads.Length / 4; i++)
        {
            int index1 = quads[4 * i];
            int index2 = quads[4 * i + 1];
            int index3 = quads[4 * i + 2];
            int index4 = quads[4 * i + 3];

            Vector3 pt1 = transform.TransformPoint(vertices[index1]);
            Vector3 pt2 = transform.TransformPoint(vertices[index2]);
            Vector3 pt3 = transform.TransformPoint(vertices[index3]);
            Vector3 pt4 = transform.TransformPoint(vertices[index4]);

            Gizmos.DrawLine(pt1, pt2);
            Gizmos.DrawLine(pt2, pt3);
            Gizmos.DrawLine(pt3, pt4);
            Gizmos.DrawLine(pt4, pt1);

            string str = string.Format("{0} ({1},{2},{3},{4})",
                i, index1, index2, index3, index4);

            Handles.Label((pt1 + pt2 + pt3 + pt4) / 4.0f, str, style);
        }
    }
}