using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chips : MonoBehaviour
{
    MeshFilter m_Mf;

    // Start is called before the first frame update
    void Start()
    {
        m_Mf = GetComponent<MeshFilter>();
    }

    private Mesh CreateChips(Vector3 halfSize){
        Mesh mesh = new Mesh();
        mesh.name = "chips";

        Vector3[] vertices = new Vector3[8];
        int[] quads = new int[3 * 4];



        mesh.vertices = vertices;
        mesh.SetIndices(quads,MeshTopology.Quads,0);

        return mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
