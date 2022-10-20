using UnityEngine;

namespace Polygons
{
    public class RegularPolygon : Polygon
    {
        [SerializeField] private Vector3 halfSize = new(4, 2, 4);
        [SerializeField] [Min(3)] private int nSector = 6;

        private void Start()
        {
            Mesh = CreateRegularPolygon();
        }

        private Mesh CreateRegularPolygon()
        {
            var mesh = new Mesh();
            mesh.name = "regular polygon";

            var vertices = new Vector3[nSector * 2 + 1];
            var quads = new int[nSector * 4];

            var angle = 2 * Mathf.PI / nSector;

            for (var i = 0; i < nSector; i++)
            {
                vertices[2 * i] = new Vector3(Mathf.Cos(i * angle) * halfSize.x, 0, Mathf.Sin(i * angle) * halfSize.z);
            }

            for (var i = 0; i < nSector; i++)
            {
                vertices[2 * i + 1] = Vector3.Lerp(vertices[2 * i], vertices[(2 * i + 2) % (nSector * 2)], .5f);
            }

            var index = 0;
            for (var i = 0; i < nSector; i++)
            {
                quads[index++] = nSector * 2;
                quads[index++] = (i * 2 + 3) % (nSector * 2);
                quads[index++] = (i * 2 + 2) % (nSector * 2);
                quads[index++] = (i * 2 + 1) % (nSector * 2);
            }

            mesh.vertices = vertices;
            mesh.SetIndices(quads, MeshTopology.Quads, 0);
            return mesh;
        }
    }
}