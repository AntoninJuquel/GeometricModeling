using UnityEditor;
using UnityEngine;

public class MeshGeneratorQuads : MonoBehaviour
{
    private MeshFilter m_Mf;

    private void Start()
    {
        m_Mf = GetComponent<MeshFilter>();
        //m_Mf.mesh = CreateStrip(7, new Vector3(4, 1, 3));
        m_Mf.mesh = CreateGridXZ(7, 4, new Vector3(4, 1, 3));
    }

    private Mesh CreateStrip(int nSegments, Vector3 halfSize)
    {
        var mesh = new Mesh
        {
            name = "strip"
        };

        var vertices = new Vector3[(nSegments + 1) * 2];
        var quads = new int[nSegments * 4];

        var index = 0;
        var leftTopPos = new Vector3(-halfSize.x, 0, halfSize.z);
        var rightTopPos = new Vector3(halfSize.x, 0, halfSize.z);

        for (var i = 0; i < nSegments + 1; i++)
        {
            var k = (float) i / nSegments;

            var tmpPos = Vector3.Lerp(leftTopPos, rightTopPos, k);
            vertices[index++] = tmpPos;
            vertices[index++] = tmpPos - 2 * halfSize.z * Vector3.forward;
        }

        index = 0;
        for (var i = 0; i < nSegments; i++)
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


    private Mesh CreateGridXZ(int nSegmentsX, int nSegmentsZ, Vector3 halfSize)
    {
        var mesh = new Mesh
        {
            name = "grid"
        };

        var vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        var quads = new int[nSegmentsX * nSegmentsZ * 4];

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
                quads[index++] = x + z * (nSegmentsX + 1);
                quads[index++] = x + z * (nSegmentsX + 1) + 1;
                quads[index++] = x + z * (nSegmentsX + 1) + 1 + (nSegmentsX + 1);
                quads[index++] = x + z * (nSegmentsX + 1) + (nSegmentsX + 1);
            }
        }


        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }


    private void OnDrawGizmos()
    {
        if (!(m_Mf && m_Mf.mesh)) return;

        var mesh = m_Mf.mesh;
        var vertices = mesh.vertices;
        var quads = mesh.GetIndices(0);

        var style = new GUIStyle
        {
            fontSize = 15,
            normal =
            {
                textColor = Color.red
            }
        };

        for (var i = 0; i < vertices.Length; i++)
        {
            var worldPos = transform.TransformPoint(vertices[i]);
            Handles.Label(worldPos, i.ToString(), style);
        }

        Gizmos.color = Color.black;
        style.normal.textColor = Color.blue;

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

            Gizmos.DrawLine(pt1, pt2);
            Gizmos.DrawLine(pt2, pt3);
            Gizmos.DrawLine(pt3, pt4);
            Gizmos.DrawLine(pt4, pt1);

            var str = $"{i} ({index1},{index2},{index3},{index4})";

            Handles.Label((pt1 + pt2 + pt3 + pt4) / 4.0f, str, style);
        }
    }
}