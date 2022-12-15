using System.Collections;
using System.Collections.Generic;
using Polygons;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;


delegate Vector3 ComputePosDelegate(float kX, float kZ);

delegate float3 ComputePosDelegate_SIMD(float3 k);


public class MeshGeneratorQuads : Polygon
{
    [SerializeField] AnimationCurve m_Profile;

    float3 CylindricalToCartesian(float rho, float theta, float y)
    {
        return float3(rho * cos(theta), y, rho * sin(theta));
    }

    void Start()
    {
        //m_Mf.mesh = CreateStrip(7, new Vector3(4, 1, 3));
        //m_Mf.mesh = CreateGridXZ(7,4, new Vector3(4, 1, 3));
        //m_Mf.mesh = CreateNormalizedGridXZ(7, 4);

        //Cylindre
        /* m_Mf.mesh = CreateNormalizedGridXZ(20, 40,
             (kX, kZ) =>
             {
                 float rho, theta, y;
                 // coordinates mapping de (kX,kZ) -> (rho,theta,y)
                 theta = kX * 2 * Mathf.PI;
                 y = kZ * 6;
                 //rho = 3 + .25f * Mathf.Sin(kZ*2*Mathf.PI*4) ;
                 rho = m_Profile.Evaluate(kZ) * 2;
                 return new Vector3(rho * Mathf.Cos(theta), y, rho * Mathf.Sin(theta));
                 //return new Vector3(Mathf.Lerp(-1.5f, 5.5f, kX), 1, Mathf.Lerp(-2, 4, kZ));
             }
             );
        */


        // Sphère
        /* m_Mf.mesh = CreateNormalizedGridXZ(10, 5,
             (kX, kZ) =>
             {
                 float rho, theta, phi;
                 // coordinates mapping de (kX,kZ) -> (rho,theta,phi)
                 theta = kX * 2 * Mathf.PI;
                 phi = kZ * Mathf.PI;
                 rho = 2 + .55f * Mathf.Cos(kX * 2 * Mathf.PI * 8)
                                 * Mathf.Sin(kZ * 2 * Mathf.PI * 6);

                 //rho = 3 + .25f * Mathf.Sin(kZ*2*Mathf.PI*4) ;
                 //rho = m_Profile.Evaluate(kZ) * 2;
                 return new Vector3(rho * Mathf.Cos(theta)*Mathf.Sin(phi),
                     rho*Mathf.Cos(phi),
                     rho * Mathf.Sin(theta)*Mathf.Sin(phi));
                 //return new Vector3(Mathf.Lerp(-1.5f, 5.5f, kX), 1, Mathf.Lerp(-2, 4, kZ));
             }
             );
        */

        //Torus (donut)
        /*¨

        m_Mf.mesh = CreateNormalizedGridXZ(20 , 10,
            (kX, kZ) =>
            {
                float R = 3;
                float r = 1;
                float theta =  2 * Mathf.PI * kX;
                Vector3 OOmega = new Vector3(R * Mathf.Cos(theta), 0, R * Mathf.Sin(theta));
                float alpha = Mathf.PI * 2 * kZ;
                Vector3 OmegaP = r * Mathf.Cos(alpha) * OOmega.normalized + r * Mathf.Sin(alpha) * Vector3.up;
                return OOmega + OmegaP;
            }
        );

        */


        //Helix

        Mesh = CreateNormalizedGridXZ(10 * 6, 5,
            (kX, kZ) =>
            {
                float R = 3;
                float r = 1;
                float theta = 6 * 2 * Mathf.PI * kX;
                Vector3 OOmega = new Vector3(R * Mathf.Cos(theta), 0, R * Mathf.Sin(theta));
                float alpha = Mathf.PI * 2 * kZ;
                Vector3 OmegaP = r * Mathf.Cos(alpha) * OOmega.normalized + r * Mathf.Sin(alpha) * Vector3.up
                                                                          + Vector3.up * kX * 2 * r * 6;
                return OOmega + OmegaP;
            }
        );


        // Unity.Mathematics
        /*
        bool bothSides = true;

        m_Mf.mesh = CreateNormalizedGridXZ_SIMD(
            (bothSides ? 2 : 1) * int3(100, 100, 1),
            (k) =>
            {

                if (bothSides) k = abs((k - .5f) * 2);
                //return lerp(float3(-5f, 0, -5f), float3(5f, 0, 5f), k.xzy);
                //return lerp(float3(-5, 1, -5), float3(5, 0, 5), float3(k.x, step(.2f, k.x), k.y)) ;
                //return lerp(float3(-5, 1, -5), float3(5, 0, 5), float3(k.x, smoothstep(.2f - 0.05f, .2f + 0.05f, k.x), k.y)) ;
                //return lerp(float3(-5, 1, -5), float3(5, 0, 5), float3(k.x, smoothstep(0.2f - .05f, .2f + .05f, k.x * k.y), k.y));
                return lerp(float3(-5, 1, -5), float3(5, 0, 5), float3(
                    k.x,
                    0.5f * (sin(k.x * 2 * PI * 4) * cos(k.y * 2 * PI * 3) + 1),
                    //smoothstep(0.2f - .05f, .2f + .05f, 0.5f*(sin(k.x*2*PI*4) * cos(k.y*2*PI*3)+1))
                     k.y));
            }
            );
        */

        // repeated pattern

        // int3 nCells = int3(3, 3, 1);
        // int3 nSegmentsPerCell = int3(100, 100, 1);
        // float3 kStep = float3(1) / (nCells * nSegmentsPerCell);
        // float3 cellSize = float3(1, .5f, 1);
        //
        // Mesh = CreateNormalizedGridXZ_SIMD(
        //     nCells * nSegmentsPerCell,
        //     (k) =>
        //     {
        //         // calculs sur la grille normalisée
        //         int3 index = (int3)floor(k / kStep);
        //         int3 localIndex = index % nSegmentsPerCell;
        //         int3 indexCell = index / nSegmentsPerCell;
        //         float3 relIndexCell = (float3)indexCell / nCells;
        //
        //         // calculs sur les positions dans l'espace
        //         /*
        //         float3 cellOriginPos = lerp(
        //             -cellSize * nCells.xzy * .5f,
        //             cellSize * nCells.xzy * .5f,
        //             relIndexCell.xzy);
        //         */
        //         float3 cellOriginPos = floor(k * nCells).xzy; // Theo's style ... ne prend pas en compte cellSize
        //         k = frac(k * nCells);
        //         return cellOriginPos
        //                + cellSize * float3(k.x, smoothstep(0.2f - .05f, .2f + .05f, k.x * k.y), k.y);
        //     }
        // );
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
            float k = (float) i / nSegments;

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
        Mesh mesh = new Mesh();
        mesh.name = "grid";
        Vector3[] vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        int[] quads = new int[nSegmentsX * nSegmentsZ * 4];

        //Vertices
        int index = 0;
        for (int i = 0; i < nSegmentsZ + 1; i++)
        {
            float kZ = (float) i / nSegmentsZ;
            for (int j = 0; j < nSegmentsX + 1; j++)
            {
                float kX = (float) j / nSegmentsX;
                vertices[index++] = new Vector3(Mathf.Lerp(-halfSize.x, halfSize.x, kX),
                    0,
                    Mathf.Lerp(-halfSize.z, halfSize.z, kZ));
            }
        }

        index = 0;

        //Quads

        for (int i = 0; i < nSegmentsZ; i++)
        {
            for (int j = 0; j < nSegmentsX; j++)
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


    Mesh CreateNormalizedGridXZ(int nSegmentsX, int nSegmentsZ, ComputePosDelegate computePos = null)
    {
        Mesh mesh = new Mesh();
        mesh.name = "normalizedGrid";
        Vector3[] vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        int[] quads = new int[nSegmentsX * nSegmentsZ * 4];

        //Vertices

        int index = 0;
        for (int i = 0; i < nSegmentsZ + 1; i++)
        {
            float kZ = (float) i / nSegmentsZ;
            for (int j = 0; j < nSegmentsX + 1; j++)
            {
                float kX = (float) j / nSegmentsX;
                vertices[index++] = computePos != null ? computePos(kX, kZ) : new Vector3(kX, 0, kZ);
            }
        }

        index = 0;

        //Quads

        for (int i = 0; i < nSegmentsZ; i++)
        {
            for (int j = 0; j < nSegmentsX; j++)
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


    Mesh CreateNormalizedGridXZ_SIMD(int3 nSegments, ComputePosDelegate_SIMD computePos = null)

    {
        Mesh mesh = new Mesh();
        mesh.name = "normalizedGrid";
        Vector3[] vertices = new Vector3[(nSegments.x + 1) * (nSegments.y + 1)];
        int[] quads = new int[nSegments.x * nSegments.y * 4];

        //Vertices

        int index = 0;
        for (int i = 0; i < nSegments.y + 1; i++)
        {
            for (int j = 0; j < nSegments.x + 1; j++)
            {
                float3 k = float3(j, i, 0) / nSegments;
                vertices[index++] = computePos != null ? computePos(k) : k;
            }
        }

        index = 0;
        int offset = 0;
        int offsetNextLine = offset;

        //Quads

        for (int i = 0; i < nSegments.y; i++)
        {
            offsetNextLine += nSegments.x + 1;
            for (int j = 0; j < nSegments.x; j++)
            {
                quads[index++] = offset + j;
                quads[index++] = offsetNextLine + j;
                quads[index++] = offsetNextLine + j + 1;
                quads[index++] = offset + j + 1;
            }

            offset = offsetNextLine;
        }

        mesh.vertices = vertices;
        mesh.SetIndices(quads, MeshTopology.Quads, 0);
        return mesh;
    }
}