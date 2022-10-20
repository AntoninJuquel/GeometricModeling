using UnityEngine;

namespace Polygons
{
    public class Chips : Polygon
    {
        private void Start()
        {
            Mesh = CreateChips(new Vector3(4, 2, 3));
        }

        private Mesh CreateChips(Vector3 halfSize)
        {
            Mesh mesh = new Mesh();
            mesh.name = "chips";

            Vector3[] vertices = new Vector3[8];
            int[] quads = new int[3 * 4];

            vertices[0] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
            vertices[1] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            vertices[2] = new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            vertices[3] = new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            vertices[4] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            vertices[5] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
            vertices[6] = new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            vertices[7] = new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);

            //Face 0
            quads[0] = 0;
            quads[1] = 1;
            quads[2] = 2;
            quads[3] = 3;

            //Face 1
            quads[4] = 4;
            quads[5] = 5;
            quads[6] = 6;
            quads[7] = 7;

            //Face 2
            quads[8] = 4;
            quads[9] = 1;
            quads[10] = 0;
            quads[11] = 5;


            mesh.vertices = vertices;
            mesh.SetIndices(quads, MeshTopology.Quads, 0);

            return mesh;
        }
    }
}