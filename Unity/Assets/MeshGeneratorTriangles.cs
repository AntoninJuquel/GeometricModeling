using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGeneratorTriangles : MonoBehaviour
{
    private MeshFilter m_Mf;

    private void Start()
    {
        m_Mf = GetComponent<MeshFilter>();
        //m_Mf.mesh = CreateTriangle();
        // m_Mf.mesh = CreateStrip(7, new Vector3(4, 1, 3));
        m_Mf.mesh = CreateGridXZ(7, 4, new Vector3(3, 1, 3));
    }

    private Mesh CreateTriangle()
    {
        var mesh = new Mesh
        {
            name = "triangle"
        };

        var vertices = new Vector3[3];
        var triangles = new int[1 * 3];

        vertices[0] = Vector3.right;
        vertices[1] = Vector3.up;
        vertices[2] = Vector3.forward;

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        return mesh;
    }

    private Mesh CreateQuad(Vector3 halfSize)
    {
        var mesh = new Mesh
        {
            name = "quad"
        };

        var vertices = new Vector3[4];
        var triangles = new int[2 * 3];

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

    private Mesh CreateStrip(int nSegments, Vector3 halfSize)
    {
        var mesh = new Mesh
        {
            name = "strip"
        };

        var vertices = new Vector3[(nSegments + 1) * 2];
        var triangles = new int[nSegments * 2 * 3];

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

    private Mesh CreateGridXZ(int nSegmentsX, int nSegmentsZ, Vector3 halfSize)
    {
        var mesh = new Mesh
        {
            name = "grid"
        };

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