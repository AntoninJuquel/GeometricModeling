using System.Linq;
using UnityEditor;
using UnityEngine;

public class MeshGeneratorQuads : MonoBehaviour
{
    private delegate Vector3 ComputePosDelegate(float kX, float kZ);

    private MeshFilter _mMf;

    [SerializeField] private AnimationCurve profile;

    private void Start()
    {
        _mMf = GetComponent<MeshFilter>();
        _mMf.mesh = CreateGridXZ(10, 1, (x, z) =>
        {
            //return new Vector3(Mathf.Lerp(-10f, 10f, x), 0, Mathf.Lerp(-10f, 10f, z));

            var rho = profile.Evaluate(z) * 4;
            var theta = x * 2 * Mathf.PI;
            var y = z * 6;
            return new Vector3(rho * Mathf.Sin(y) * Mathf.Cos(theta), rho * Mathf.Sin(y) * Mathf.Sin(theta), rho * Mathf.Cos(y));
        });
        GUIUtility.systemCopyBuffer = ConvertToCsv("\t");
        Debug.Log(ConvertToCsv("\t"));
    }

    private Mesh CreateGridXZ(int nSegmentsX, int nSegmentsZ, ComputePosDelegate computePos = null)
    {
        var mesh = new Mesh
        {
            name = "normalizedGrid"
        };

        var vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        var quads = new int[nSegmentsX * nSegmentsZ * 4];

        //Vertices
        var index = 0;
        for (var i = 0; i < nSegmentsZ + 1; i++)
        {
            var kZ = (float) i / nSegmentsZ;

            for (var j = 0; j < nSegmentsX + 1; j++)
            {
                var kX = (float) j / nSegmentsX;
                vertices[index++] = computePos != null ? computePos(kX, kZ) : new Vector3(kX, 0, kZ);
            }
        }

        index = 0;
        //Quads
        for (var i = 0; i < nSegmentsZ; i++)
        {
            for (var j = 0; j < nSegmentsX; j++)
            {
                quads[index++] = i * (nSegmentsX + 1) + j;
                quads[index++] = (i + 1) * (nSegmentsX + 1) + j;
                quads[index++] = (i + 1) * (nSegmentsX + 1) + j + 1;
                quads[index++] = i * (nSegmentsX + 1) + j + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);

        return mesh;
    }

    private string ConvertToCsv(string separator)
    {
        if (!(_mMf && _mMf.mesh)) return "";

        var vertices = _mMf.mesh.vertices;
        var quads = _mMf.mesh.GetIndices(0);

        var strings = vertices.Select((pos, i) => $"{i}{separator}{pos.x:N03} {pos.y:N03} {pos.z:N03}{separator}{separator}").ToList();

        for (var i = vertices.Length; i < quads.Length / 4; i++)
            strings.Add(separator + separator + separator);

        for (var i = 0; i < quads.Length / 4; i++)
        {
            strings[i] += $"{i}{separator}{quads[4 * i + 0]},{quads[4 * i + 1]},{quads[4 * i + 2]},{quads[4 * i + 3]}";
        }

        return $"Vertices{separator}{separator}{separator}Faces\nIndex{separator}Position{separator}{separator}Index{separator}Indices des vertices\n{string.Join("\n", strings)}";
    }

    private void OnDrawGizmos()
    {
        if (!(_mMf && _mMf.mesh)) return;

        var mesh = _mMf.mesh;
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