using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{

    MeshFilter m_Mf;

    // Start is called before the first frame update
    void Start()
    {
        m_Mf = GetComponent<MeshFilter>();
        m_Mf.mesh = CreateBox(new Vector3(4,2,3));
        
    }

    private Mesh CreateBox(Vector3 halfSize){
        Mesh mesh = new Mesh();
        mesh.name = "box";

        Vector3[] vertices = new Vector3[8];
        int[] quads = new int[6 * 4];

        vertices[0] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
        vertices[1] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
        vertices[2] = new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
        vertices[3] = new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
        vertices[4] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
        vertices[5] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
        vertices[6] = new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        vertices[7] = new Vector3(-halfSize.x, halfSize.y, -halfSize.z);

        //Face 0
        quads[0] = 0;
        quads[1] = 1;
        quads[2] = 2;
        quads[3] = 3;

        //Face 1
        quads[4] = 7;
        quads[5] = 4;
        quads[6] = 5;
        quads[7] = 6;

        //Face 2
        quads[8] = 4;
        quads[9] = 1;
        quads[10] = 0;
        quads[11] = 5;

        //Face 3
        quads[12] = 3;
        quads[13] = 2;
        quads[14] = 7;
        quads[15] = 6;

        //Face 4
        quads[16] = 6;
        quads[17] = 5;
        quads[18] = 0;
        quads[19] = 3;

        //Face 5
        quads[20] = 1;
        quads[21] = 4;
        quads[22] = 7;
        quads[23] = 2;

        mesh.vertices = vertices;
        mesh.SetIndices(quads,MeshTopology.Quads,0);

        return mesh;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
