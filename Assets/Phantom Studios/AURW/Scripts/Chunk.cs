using System.Collections.Generic;
using UnityEngine;

namespace aurw
{
    public class Chunk
    {
        public int meshDetail = 4;
        public Vector2 size = new Vector2(1, 1);

        public void SetLinearSize(int _size)
        {
            size = new Vector2(_size, _size);
        }
        public Vector2 GetSize() { return size; }

        public Mesh mainChunk()
        {
            Mesh mesh = new Mesh();
            mesh.name = "AURW_MainChunk_X4";

            int quadsX = meshDetail;
            int quadsY = meshDetail;

            int vertsX = quadsX + 1;
            int vertsY = quadsY + 1;

            float stepX = size.x / quadsX;
            float stepY = size.y / quadsY;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            for (int y = 0; y < quadsY; y++)
            {
                for (int x = 0; x < quadsX; x++)
                {
                    // 4 corners
                    Vector3 v00 = new Vector3(-size.x / 2f + x * stepX, 0f, -size.y / 2f + y * stepY);
                    Vector3 v10 = v00 + new Vector3(stepX, 0f, 0f);
                    Vector3 v01 = v00 + new Vector3(0f, 0f, stepY);
                    Vector3 v11 = v00 + new Vector3(stepX, 0f, stepY);

                    // center
                    Vector3 vc = (v00 + v11) / 2f;

                    int i00 = vertices.Count; vertices.Add(v00); uvs.Add(new Vector2((float)x / quadsX, (float)y / quadsY));
                    int i10 = vertices.Count; vertices.Add(v10); uvs.Add(new Vector2((float)(x + 1) / quadsX, (float)y / quadsY));
                    int i01 = vertices.Count; vertices.Add(v01); uvs.Add(new Vector2((float)x / quadsX, (float)(y + 1) / quadsY));
                    int i11 = vertices.Count; vertices.Add(v11); uvs.Add(new Vector2((float)(x + 1) / quadsX, (float)(y + 1) / quadsY));
                    int ic = vertices.Count; vertices.Add(vc); uvs.Add(new Vector2((float)(x + 0.5f) / quadsX, (float)(y + 0.5f) / quadsY));

                    triangles.Add(i00); triangles.Add(ic); triangles.Add(i10);
                    triangles.Add(i10); triangles.Add(ic); triangles.Add(i11);
                    triangles.Add(i11); triangles.Add(ic); triangles.Add(i01);
                    triangles.Add(i01); triangles.Add(ic); triangles.Add(i00);

                }
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

    }
}