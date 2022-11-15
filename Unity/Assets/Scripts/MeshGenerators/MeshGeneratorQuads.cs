using System.Linq;
using Polygons;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class MeshGeneratorQuads : Polygon
{
    private delegate Vector3 ComputePosDelegate(float kX, float kZ);

    private delegate float3 ComputePosDelegateSIMD(float3 k);

    private MeshFilter _mMf;

    [SerializeField] private AnimationCurve profile;
    [SerializeField] private bool bothSides;

    private void Start()
    {
        _mMf = GetComponent<MeshFilter>();
        /*Mesh = CreateGridXZSIMD(int3(40, 20, 1), (k) =>
        {
            if (bothSides) k = abs((k - .5f) * 2);
            //return lerp(float3(-5f, 0, -5f), float3(5f, 0, 5f), k.xzy);
            return lerp(float3(-5f, 1, -5f), float3(5f, 0, 5f), float3(
                k.x,
                .5f * (sin(k.x * 2 * PI * 4) * cos(k.y * 2 * PI * 3) + 1),
                k.y));
        });*/

        var nCells = int3(3, 3, 1);
        var nSegmentsPerCell = int3(100, 100, 1);
        var kStep = float3(1) / (nCells * nSegmentsPerCell);

        var cellSize = float3(1, .5f, 1);

        Mesh = CreateGridXZSIMD(nCells * nSegmentsPerCell, (k) =>
        {
            var index = (int3)floor(k / kStep);
            var localIndex = index % nSegmentsPerCell;
            var indexCell = index / nSegmentsPerCell;
            var relIndexCell = (float3)indexCell / nCells;

            var cellOriginPos = lerp(-cellSize * nCells.xzy * .5f, cellSize * nCells.xzy * .5f, relIndexCell.xzy);

            k = frac(k * nCells);

            return cellOriginPos + cellSize * float3(k.x, smoothstep(.2f - .05f, .2f + .05f, k.x * k.y), k.y);
        });
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
            var kZ = (float)i / nSegmentsZ;

            for (var j = 0; j < nSegmentsX + 1; j++)
            {
                var kX = (float)j / nSegmentsX;
                vertices[index++] = computePos?.Invoke(kX, kZ) ?? new Vector3(kX, 0, kZ);
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

    private Mesh CreateGridXZSIMD(int3 nSegments, ComputePosDelegateSIMD computePos = null)
    {
        var mesh = new Mesh
        {
            name = "normalizedGrid"
        };

        var vertices = new Vector3[(nSegments.x + 1) * (nSegments.y + 1)];
        var quads = new int[nSegments.x * nSegments.y * 4];

        //Vertices
        var index = 0;
        for (var i = 0; i < nSegments.y + 1; i++)
        {
            for (var j = 0; j < nSegments.x + 1; j++)
            {
                var k = float3(j, i, 0) / nSegments;
                vertices[index++] = computePos?.Invoke(k) ?? k;
            }
        }

        index = 0;
        var offset = 0;
        //Quads
        for (var i = 0; i < nSegments.y; i++)
        {
            var nextOffset = offset + nSegments.x + 1;

            for (var j = 0; j < nSegments.x; j++)
            {
                quads[index++] = offset + j;
                quads[index++] = nextOffset + j;
                quads[index++] = nextOffset + j + 1;
                quads[index++] = offset + j + 1;
            }

            offset += nSegments.x + 1;
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

        var strings = vertices.Select((pos, i) => $"{i}{separator}{pos.x:N03} {pos.y:N03} {pos.z:N03}{separator}").ToList();

        for (var i = vertices.Length; i < quads.Length / 4; i++)
            strings.Add(separator + separator + separator);

        for (var i = 0; i < quads.Length / 4; i++)
        {
            strings[i] += $"{i}{separator}{quads[4 * i + 0]},{quads[4 * i + 1]},{quads[4 * i + 2]},{quads[4 * i + 3]}";
        }

        return $"Vertices{separator}{separator}{separator}Faces\nIndex{separator}Position{separator}{separator}Index{separator}Indices des vertices\n{string.Join("\n", strings)}";
    }
}