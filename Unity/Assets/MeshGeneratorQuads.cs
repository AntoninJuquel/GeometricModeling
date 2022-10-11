using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshGeneratorQuads : MonoBehaviour
{
    delegate Vector3 ComputePosDelegate(float kX, float kZ);

    private MeshFilter m_Mf;

    [SerializeField] private AnimationCurve profile;

    private void Start()
    {
        m_Mf = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        //m_Mf.mesh = CreateStrip(7, new Vector3(4, 1, 3));
        m_Mf.mesh = CreateNormalizedGridXZ(10, 10, (x, z) =>
        {
            //return new Vector3(Mathf.Lerp(-10f, 10f, x), 0, Mathf.Lerp(-10f, 10f, z));
            float rho, theta, y;

            rho = profile.Evaluate(z) * 4;
            theta = x * 2 * Mathf.PI;
            y = z * 6;
            return new Vector3(rho * Mathf.Sin(y) * Mathf.Cos(theta), rho * Mathf.Sin(y) * Mathf.Sin(theta), rho * Mathf.Cos(y));
        });
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
            var k = (float)i / nSegments;

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
            var i = (float)z / nSegmentsZ;
            var vPos = Vector3.Lerp(topPos, -topPos, i);
            for (var x = 0; x < nSegmentsX + 1; x++)
            {
                var j = (float)x / nSegmentsX;
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

    private Mesh CreateNormalizedGridXZ(int nSegmentsX, int nSegmentsZ, ComputePosDelegate computePos = null)
    {
        var mesh = new Mesh
        {
            name = "grid"
        };

        var vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        var quads = new int[nSegmentsX * nSegmentsZ * 4];

        var index = 0;
        for (var z = 0; z < nSegmentsZ + 1; z++)
        {
            var kZ = (float)z / nSegmentsZ;

            for (var x = 0; x < nSegmentsX + 1; x++)
            {
                var kX = (float)x / nSegmentsX;
                vertices[index++] = computePos?.Invoke(kX, kZ) ?? new Vector3(kX, 0, kZ);
            }
        }

        index = 0;
        for (var z = 0; z < nSegmentsZ; z++)
        {
            for (var x = 0; x < nSegmentsX; x++)
            {
                quads[index++] = x + z * (nSegmentsX + 1) + (nSegmentsX + 1);
                quads[index++] = x + z * (nSegmentsX + 1) + 1 + (nSegmentsX + 1);
                quads[index++] = x + z * (nSegmentsX + 1) + 1;
                quads[index++] = x + z * (nSegmentsX + 1);
            }
        }


        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

    string ConvertToCSV(string separator)
    {
        if (!(m_Mf && m_Mf.mesh)) return "";

        var vertices = m_Mf.mesh.vertices;
        var quads = m_Mf.mesh.GetIndices(0);

        var strings = new List<string>();

        for (var i = 0; i < vertices.Length; i++)
        {
            var pos = vertices[i];
            strings.Add($"{i}{separator}{pos.x:N03} {pos.y:N03} {pos.z:N03}{separator}");
        }

        for (var i = vertices.Length; i < quads.Length / 4; i++)
        {
            strings.Add($"{separator}{separator}{separator}");
        }

        for (var i = 0; i < quads.Length / 4; i++)
        {
            strings[i] = $"{strings[i]}{i}{separator}{quads[4 * i + 0]},{quads[4 * i + 1]},{quads[4 * i + 2]},{quads[4 * i + 3]}";
        }

        return $"Vertices{separator}{separator}{separator}Faces\nIndex{separator}Position{separator}{separator}Index{separator}Indices des vertices\n{string.Join("\n", strings)}";
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