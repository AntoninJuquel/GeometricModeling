using UnityEngine;

namespace Polygons
{
    public class Pacman : Polygon
    {
        [SerializeField] private float startAngle = Mathf.PI / 3f, endAngle = 5f * Mathf.PI / 3f;
        [SerializeField] private Vector3 halfSize = new(4, 2, 4);
        [SerializeField] [Min(3)] private int nSector = 6;

        private void Start()
        {
            Mesh = CreatePacman();
        }

        private Mesh CreatePacman()
        {
            var mesh = new Mesh();
            mesh.name = "pacman";

            var vertexCount = (nSector + 1) * 2;

            var vertices = new Vector3[vertexCount];
            var quads = new int[nSector * 4];

            var angleStep = (endAngle - startAngle) / (nSector + 1);

            for (var i = 0; i < nSector + 1; i++)
            {
                vertices[2 * i] = new Vector3(Mathf.Cos(startAngle + (i * angleStep)) * halfSize.x, 0, Mathf.Sin(startAngle + (i * angleStep)) * halfSize.z);
            }

            for (var i = 0; i < nSector; i++)
            {
                vertices[2 * i + 1] = Vector3.Lerp(vertices[2 * i], vertices[2 * i + 2], .5f);
            }

            var index = 0;
            for (var i = 0; i < nSector; i++)
            {
                quads[index++] = vertexCount - 1;
                quads[index++] = i * 2 + 2;
                quads[index++] = i * 2 + 1;
                quads[index++] = i * 2;
            }

            mesh.vertices = vertices;
            mesh.SetIndices(quads, MeshTopology.Quads, 0);
            return mesh;
        }
    }
}