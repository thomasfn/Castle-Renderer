using System;
using System.Collections.Generic;

using SlimDX;

namespace CastleRenderer.Graphics
{
    /// <summary>
    /// A utility class that assists with constructing meshes
    /// </summary>
    public class MeshBuilder
    {
        public bool UseNormals { get; set; }
        public bool UseTexCoords { get; set; }
        public bool UseTangents { get; set; }

        private List<Vector3> lstPositions;
        private List<Vector3> lstNormals;
        private List<Vector2> lstTexCoords;
        private List<Vector3> lstTangents;
        private List<List<uint>> lstIndices;

        public int CurrentVertexCount { get { return lstPositions.Count; } }
        public int CurrentIndexCount { get { return lstIndices.Count; } }

        public MeshBuilder()
        {
            lstPositions = new List<Vector3>();
            lstNormals = new List<Vector3>();
            lstTexCoords = new List<Vector2>();
            lstTangents = new List<Vector3>();
            lstIndices = new List<List<uint>>();
        }

        public void AddPosition(Vector3 position)
        {
            lstPositions.Add(position);
        }
        public void AddPositions(IEnumerable<Vector3> positions)
        {
            lstPositions.AddRange(positions);
        }

        public void AddNormal(Vector3 normal)
        {
            if (!UseNormals) return;
            lstNormals.Add(normal);
        }
        public void AddNormals(IEnumerable<Vector3> normals)
        {
            if (!UseNormals) return;
            lstNormals.AddRange(normals);
        }

        public void AddTextureCoord(Vector2 coord)
        {
            if (!UseTexCoords) return;
            lstTexCoords.Add(coord);
        }
        public void AddTextureCoords(IEnumerable<Vector2> coords)
        {
            if (!UseTexCoords) return;
            lstTexCoords.AddRange(coords);
        }

        public void AddIndex(int submesh, ushort idx)
        {
            while (submesh >= lstIndices.Count) lstIndices.Add(new List<uint>());
            lstIndices[submesh].Add(idx);
        }
        public void AddIndex(ushort idx)
        {
            AddIndex(0, idx);
        }

        public void AddIndex(int submesh, uint idx)
        {
            while (submesh >= lstIndices.Count) lstIndices.Add(new List<uint>());
            lstIndices[submesh].Add(idx);
        }
        public void AddIndex(uint idx)
        {
            AddIndex(0, idx);
        }
        public void AddIndices(int submesh, IEnumerable<uint> indices, uint offset = 0)
        {
            while (submesh >= lstIndices.Count) lstIndices.Add(new List<uint>());
            var target = lstIndices[submesh];
            if (offset == 0)
                target.AddRange(indices);
            else
            {
                int len = target.Count;
                target.AddRange(indices);
                for (int i = len; i < target.Count; i++)
                    target[i] += offset;
            }
        }

        public void AddTangent(Vector3 tangent)
        {
            if (!UseTangents) return;
            lstTangents.Add(tangent);
        }
        public void AddTangents(IEnumerable<Vector3> tangents)
        {
            if (!UseTangents) return;
            lstTangents.AddRange(tangents);
        }

        public void RemovePoints(int num)
        {
            for (int i = 0; i < num; i++)
            {
                if (lstPositions.Count > 0) lstPositions.RemoveAt(lstPositions.Count - 1);
                if (lstNormals.Count > 0) lstNormals.RemoveAt(lstNormals.Count - 1);
                if (lstTexCoords.Count > 0) lstTexCoords.RemoveAt(lstTexCoords.Count - 1);
                if (lstTangents.Count > 0) lstTangents.RemoveAt(lstTangents.Count - 1);
            }
        }

        public void CalculateNormals()
        {
            UseNormals = true;

            Vector3[] outNormals = new Vector3[lstPositions.Count];
            float[] outN = new float[lstPositions.Count];

            
            // Loop each submesh
            foreach (var submesh in lstIndices)
            {

                // Loop each poly
                List<Vector3> normals = new List<Vector3>();
                for (int k = 0; k < submesh.Count; k += 3)
                {
                    // Get the indices
                    uint i0 = submesh[k];
                    uint i1 = submesh[k + 1];
                    uint i2 = submesh[k + 2];

                    // Calculate the face normal
                    Vector3 a = lstPositions[(int)i0];
                    Vector3 b = lstPositions[(int)i1];
                    Vector3 c = lstPositions[(int)i2];
                    Vector3 norm = Vector3.Cross(b - a, c - a);
                    norm.Normalize();
                    normals.Add(norm);

                    // Add to each vertex's counter
                    outN[i0]++;
                    outNormals[i0] += norm;
                    outN[i1]++;
                    outNormals[i1] += norm;
                    outN[i2]++;
                    outNormals[i2] += norm;
                }

            }

            // Loop each vertex and add the normals
            for (int i = 0; i < lstPositions.Count; i++)
            {
                Vector3 norm = outNormals[i] / outN[i];
                norm.Normalize();
                norm *= -1.0f;
                lstNormals.Add(norm);
            }
        }

        public Mesh Build()
        {
            //if (CurrentVertexCount >= uint.MaxValue) throw new Exception("Reached maximum vertex count on mesh");
            Mesh m = new Mesh();
            m.Positions = lstPositions.ToArray();
            if (UseNormals) m.Normals = lstNormals.ToArray();
            if (UseTexCoords) m.TextureCoordinates = lstTexCoords.ToArray();
            if (UseTangents) m.Tangents = lstTangents.ToArray();
            m.Submeshes = new uint[lstIndices.Count][];
            for (int i = 0; i < lstIndices.Count; i++)
                m.Submeshes[i] = lstIndices[i].ToArray();
            m.AABB = BoundingBox.FromPoints(m.Positions);
            m.Topology = MeshTopology.Triangles;
            return m;
        }

        #region Utility

        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Vector2 uva, Vector2 uvb, Vector2 uvc)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            normal.Normalize();
            int baseidx = CurrentVertexCount;
            AddPosition(a);
            AddPosition(b);
            AddPosition(c);
            AddTextureCoord(uva);
            AddTextureCoord(uvb);
            AddTextureCoord(uvc);
            AddNormal(normal);
            AddNormal(normal);
            AddNormal(normal);
            AddIndex((uint)baseidx);
            AddIndex((uint)baseidx + 1);
            AddIndex((uint)baseidx + 2);
        }

        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector2 uva, Vector2 uvb, Vector2 uvc, Vector2 uvd, bool inverse = false)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            normal.Normalize();
            if (inverse) normal *= -1.0f;
            int baseidx = CurrentVertexCount;
            AddPosition(a);
            AddPosition(b);
            AddPosition(c);
            AddPosition(d);
            AddTextureCoord(uva);
            AddTextureCoord(uvb);
            AddTextureCoord(uvc);
            AddTextureCoord(uvd);
            AddNormal(normal);
            AddNormal(normal);
            AddNormal(normal);
            AddNormal(normal);
            if (inverse)
            {
                AddIndex((uint)baseidx);
                AddIndex((uint)baseidx + 3);
                AddIndex((uint)baseidx + 2);
                AddIndex((uint)baseidx + 2);
                AddIndex((uint)baseidx + 1);
                AddIndex((uint)baseidx);
            }
            else
            {
                AddIndex((uint)baseidx);
                AddIndex((uint)baseidx + 1);
                AddIndex((uint)baseidx + 2);
                AddIndex((uint)baseidx + 2);
                AddIndex((uint)baseidx + 3);
                AddIndex((uint)baseidx);
            }
        }

        #endregion

        #region Preset Builders

        public delegate float HeightSampler(float x, float y);

        public static Vector3 CalculateNormal(HeightSampler sampler, float x, float y, float d = 1.0f)
        {
            float dy0 = sampler(x + d, y) - sampler(x - d, y);
            float dy1 = sampler(x, y + d) - sampler(x, y - d);
            Vector3 result = new Vector3(-dy0, d, -dy1);
            result.Normalize();
            return result;
        }

        public static Vector3 CalculateTangent(HeightSampler sampler, float x, float y, float d = 1.0f)
        {
            float dy0 = sampler(x + d, y) - sampler(x - d, y);
            Vector3 result = new Vector3(d * 2.0f, dy0, 0.0f);
            result.Normalize();
            return result;
            //return Vector3.UnitX;
        }

        public static Mesh BuildHeightmap(HeightSampler sampler, int width, int height, float tilesize, int subdivide = 1)
        {
            // Determine sizes
            int totalW = width * subdivide;
            int totalH = height * subdivide;
            float invtotalW = 1.0f / totalW;
            float invtotalH = 1.0f / totalH;

            // Precache an array of heights
            float[,] heights = new float[totalW, totalH];
            for (int x = 0; x < totalW; x++)
            {
                float fX = x * invtotalW * width;
                for (int y = 0; y < totalH; y++)
                {
                    float fY = y * invtotalH * height;
                    heights[x, y] = sampler(fX, fY);
                }
            }

            // Recreate the sampler to use the cache
            sampler = new HeightSampler((x, y) =>
            {
                int ix = (int)(x * subdivide);
                int iy = (int)(y * subdivide);
                if (ix < 0) ix = 0; if (iy < 0) iy = 0;
                if (ix >= totalW) ix = totalW - 1;
                if (iy >= totalH) iy = totalH - 1;
                return heights[ix, iy];
            });

            // Prepare the builder
            MeshBuilder builder = new MeshBuilder();
            builder.UseNormals = true;
            builder.UseTangents = true;

            // Perform the loop
            for (int x = 0; x < totalW; x++)
            {
                float fX = x * invtotalW * width;
                for (int y = 0; y < totalH; y++)
                {
                    float fY = y * invtotalH * height;
                    builder.AddPosition(new Vector3(fX * tilesize, sampler(fX, fY), fY * tilesize));
                    builder.AddNormal(CalculateNormal(sampler, fX, fY));
                    builder.AddTangent(CalculateTangent(sampler, fX, fY));
                    if ((x < totalW - 1) && (y < totalH - 1))
                    {
                        int idx = (x * totalH) + y;
                        builder.AddIndex((ushort)idx);
                        builder.AddIndex((ushort)(idx + 1));
                        builder.AddIndex((ushort)(idx + totalH + 1));
                        builder.AddIndex((ushort)(idx + totalH + 1));
                        builder.AddIndex((ushort)(idx + totalH));
                        builder.AddIndex((ushort)idx);
                    }
                }
            }
            return builder.Build();
        }

        public static void BuildGrassPatch(int points, float range, out Mesh high, out Mesh medium, out Mesh low)
        {
            Random rnd = new Random();
            MeshBuilder builder = new MeshBuilder();
            builder.UseNormals = true;
            for (int i = 0; i < points; i++)
            {
                builder.AddPosition(new Vector3((float)rnd.NextDouble() * range, 0.0f, (float)rnd.NextDouble() * range));
                builder.AddNormal(new Vector3((float)(rnd.NextDouble() * Math.PI * 2.0), (float)rnd.NextDouble(), (float)rnd.NextDouble()));
            }
            high = builder.Build();
            builder.RemovePoints(points / 3);
            medium = builder.Build();
            builder.RemovePoints(points / 3);
            low = builder.Build();
        }

        public static Mesh BuildNormalDebugger(Mesh mesh)
        {
            if (mesh.Normals == null) return null;
            MeshBuilder builder = new MeshBuilder();
            for (int i = 0; i < mesh.Normals.Length; i++)
            {
                builder.AddPosition(mesh.Positions[i]);
                builder.AddPosition(mesh.Positions[i] + mesh.Normals[i]);
            }
            return builder.Build();
        }

        public static Mesh CombineMeshes(params Mesh[] meshes)
        {
            MeshBuilder builder = new MeshBuilder();
            if (meshes[0].TextureCoordinates != null) builder.UseTexCoords = true;
            if (meshes[0].Normals != null) builder.UseNormals = true;
            uint indexpos = 0;
            for (int i = 0; i < meshes.Length; i++)
            {
                var mesh = meshes[i];
                if (builder.UseTexCoords) builder.AddTextureCoords(mesh.TextureCoordinates);
                if (builder.UseNormals) builder.AddNormals(mesh.Normals);
                for (int j = 0; j < mesh.Submeshes.Length; j++)
                    builder.AddIndices(j, mesh.Submeshes[j]);
                builder.AddPositions(mesh.Positions);
                indexpos += (uint)mesh.Positions.Length;
            }
            return builder.Build();
        }

        public static Mesh BuildSkybox()
        {
            MeshBuilder builder = new MeshBuilder();
            builder.AddPosition(new Vector3(-1.0f, -1.0f, -1.0f));  // 0
            builder.AddPosition(new Vector3(-1.0f, -1.0f, 1.0f));   // 1
            builder.AddPosition(new Vector3(1.0f, -1.0f, 1.0f));    // 2
            builder.AddPosition(new Vector3(1.0f, -1.0f, -1.0f));   // 3
            builder.AddPosition(new Vector3(-1.0f, 1.0f, -1.0f));   // 4
            builder.AddPosition(new Vector3(-1.0f, 1.0f, 1.0f));    // 5
            builder.AddPosition(new Vector3(1.0f, 1.0f, 1.0f));     // 6
            builder.AddPosition(new Vector3(1.0f, 1.0f, -1.0f));    // 7

            // North
            builder.AddIndex(1);
            builder.AddIndex(5);
            builder.AddIndex(6);
            builder.AddIndex(6);
            builder.AddIndex(2);
            builder.AddIndex(1);

            // East
            builder.AddIndex(2);
            builder.AddIndex(6);
            builder.AddIndex(7);
            builder.AddIndex(7);
            builder.AddIndex(3);
            builder.AddIndex(2);

            // South
            builder.AddIndex(3);
            builder.AddIndex(7);
            builder.AddIndex(4);
            builder.AddIndex(4);
            builder.AddIndex(0);
            builder.AddIndex(3);

            // West
            builder.AddIndex(0);
            builder.AddIndex(4);
            builder.AddIndex(5);
            builder.AddIndex(5);
            builder.AddIndex(2);
            builder.AddIndex(0);

            // Up
            builder.AddIndex(5);
            builder.AddIndex(4);
            builder.AddIndex(7);
            builder.AddIndex(7);
            builder.AddIndex(6);
            builder.AddIndex(5);

            // Down
            builder.AddIndex(0);
            builder.AddIndex(1);
            builder.AddIndex(2);
            builder.AddIndex(2);
            builder.AddIndex(3);
            builder.AddIndex(0);

            // Build mesh
            return builder.Build();
        }

        private static readonly float Root3 = (float)Math.Sqrt(3.0);
        private static readonly float Root6 = (float)Math.Sqrt(6.0);

        private struct Triangle
        {
            public Vector3 A, B, C;

            public Triangle(Vector3 a, Vector3 b, Vector3 c) { A = a; B = b; C = c; }
        }

        public static Mesh BuildSphere(float radius, int subdivisions, bool usetexcoords, bool usetangents)
        {
            // Starting points
            var A = new Vector3(-1.0f, -(1.0f / Root3), -(1.0f / Root6));
            var B = new Vector3(0.0f, 2.0f / Root3, -1.0f / Root6);
            var C = new Vector3(1.0f, -(1.0f / Root3), -(1.0f / Root6));
            var D = new Vector3(0.0f, 0.0f, 3.0f / Root6);

            // Build a triangular based pyramid
            var tris = new List<Triangle>();
            tris.Add(new Triangle(A, B, C));
            tris.Add(new Triangle(C, D, A));
            tris.Add(new Triangle(B, D, C));
            tris.Add(new Triangle(A, D, B));
            var tmplist = new List<Triangle>();

            // Run subdivide passes
            for (int i = 0; i < subdivisions; i++)
            {
                // Normalise all vertices
                for (int j = 0; j < tris.Count; j++)
                {
                    Triangle tri = tris[j];
                    tri.A.Normalize();
                    tri.A *= radius;
                    tri.B.Normalize();
                    tri.B *= radius;
                    tri.C.Normalize();
                    tri.C *= radius;
                    tris[j] = tri;
                }

                // Check for last pass
                if (i == subdivisions - 1) break;

                // Subdivide all triangles
                tmplist.Clear();
                for (int j = 0; j < tris.Count; j++)
                {
                    Triangle tri = tris[j];
                    Vector3 midAB = (tri.A + tri.B) * 0.5f;
                    Vector3 midBC = (tri.B + tri.C) * 0.5f;
                    Vector3 midCA = (tri.C + tri.A) * 0.5f;
                    tmplist.Add(new Triangle(tri.A, midAB, midCA));
                    tmplist.Add(new Triangle(midAB, midBC, midCA));
                    tmplist.Add(new Triangle(midCA, midBC, tri.C));
                    tmplist.Add(new Triangle(midAB, tri.B, midBC));
                }

                // Swap the lists
                var tmp = tris;
                tris = tmplist;
                tmplist = tmp;
            }

            // Create builder
            var builder = new MeshBuilder();
            builder.UseNormals = true;
            builder.UseTexCoords = usetexcoords;
            builder.UseTangents = usetangents;
            for (int i = 0; i < tris.Count; i++)
            {
                Triangle tri = tris[i];
                builder.AddPosition(tri.A);
                builder.AddPosition(tri.B);
                builder.AddPosition(tri.C);
                builder.AddNormal(GetNormal(tri.A));
                builder.AddNormal(GetNormal(tri.B));
                builder.AddNormal(GetNormal(tri.C));
                if (usetangents)
                {
                    builder.AddTangent(GetSphereTangent(tri.A));
                    builder.AddTangent(GetSphereTangent(tri.B));
                    builder.AddTangent(GetSphereTangent(tri.C));
                }
                if (usetexcoords)
                {
                    // TODO: This
                    builder.AddTextureCoord(new Vector2(0.0f, 0.0f));
                    builder.AddTextureCoord(new Vector2(0.0f, 0.0f));
                    builder.AddTextureCoord(new Vector2(0.0f, 0.0f));
                }
                builder.AddIndex((ushort)(i * 3));
                builder.AddIndex((ushort)(i * 3 + 1));
                builder.AddIndex((ushort)(i * 3 + 2));
            }

            // Create mesh
            return builder.Build();
        }

        public static Mesh BuildCube()
        {
            // Create mesh builder
            MeshBuilder builder = new MeshBuilder();
            builder.UseTexCoords = true;
            builder.UseNormals = true;

            // X facing sides
            builder.AddQuad(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 1.0f, 1.0f), new Vector3(0.0f, 1.0f, 0.0f),
                new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), false);
            builder.AddQuad(new Vector3(1.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 0.0f),
                new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, 0.0f), true);

            // Y facing sides
            builder.AddQuad(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, 1.0f),
                new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f), false);
            builder.AddQuad(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.0f, 1.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f), new Vector3(0.0f, 1.0f, 1.0f),
                new Vector2(1.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), true);

            // Z facing sides
            builder.AddQuad(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f), new Vector3(1.0f, 1.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f),
                new Vector2(0.0f, 1.0f), new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), false);
            builder.AddQuad(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 1.0f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 0.0f, 1.0f),
                new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f), true);

            // Done
            return builder.Build();
        }

        public static Mesh BuildFullscreenQuad()
        {
            // Create mesh builder
            MeshBuilder builder = new MeshBuilder();
            builder.UseTexCoords = true;

            // Add vertices
            builder.AddPosition(new Vector3(-1.0f, -1.0f, 0.5f));
            builder.AddPosition(new Vector3(1.0f, -1.0f, 0.5f));
            builder.AddPosition(new Vector3(1.0f, 1.0f, 0.5f));
            builder.AddPosition(new Vector3(-1.0f, 1.0f, 0.5f));
            builder.AddTextureCoord(new Vector2(0.0f, 1.0f));
            builder.AddTextureCoord(new Vector2(1.0f, 1.0f));
            builder.AddTextureCoord(new Vector2(1.0f, 0.0f));
            builder.AddTextureCoord(new Vector2(0.0f, 0.0f));

            // Add indices
            builder.AddIndex(0);
            builder.AddIndex(1);
            builder.AddIndex(2);
            builder.AddIndex(2);
            builder.AddIndex(3);
            builder.AddIndex(0);

            // Build mesh
            return builder.Build();
        }

        public static Mesh BuildPlane(bool texcoords, bool tangents)
        {
            // Create mesh builder
            MeshBuilder builder = new MeshBuilder();
            builder.UseNormals = true;
            builder.UseTexCoords = texcoords;
            builder.UseTangents = tangents;

            // Add vertices
            builder.AddPosition(new Vector3(0.0f, 0.0f, 0.0f));
            builder.AddPosition(new Vector3(0.0f, 0.0f, 1.0f));
            builder.AddPosition(new Vector3(1.0f, 0.0f, 1.0f));
            builder.AddPosition(new Vector3(1.0f, 0.0f, 0.0f));
            builder.AddNormal(Vector3.UnitY);
            builder.AddNormal(Vector3.UnitY);
            builder.AddNormal(Vector3.UnitY);
            builder.AddNormal(Vector3.UnitY);
            if (texcoords)
            {
                builder.AddTextureCoord(new Vector2(0.0f, 1.0f));
                builder.AddTextureCoord(new Vector2(1.0f, 1.0f));
                builder.AddTextureCoord(new Vector2(1.0f, 0.0f));
                builder.AddTextureCoord(new Vector2(0.0f, 0.0f));
            }
            if (tangents)
            {
                builder.AddTangent(new Vector3(1.0f, 0.0f, 0.0f));
                builder.AddTangent(new Vector3(1.0f, 0.0f, 0.0f));
                builder.AddTangent(new Vector3(1.0f, 0.0f, 0.0f));
                builder.AddTangent(new Vector3(1.0f, 0.0f, 0.0f));
            }

            // Add indices
            builder.AddIndex(0);
            builder.AddIndex(1);
            builder.AddIndex(2);
            builder.AddIndex(2);
            builder.AddIndex(3);
            builder.AddIndex(0);

            // Build mesh
            return builder.Build();
        }

        #endregion

        private static Vector3 GetNormal(Vector3 vec)
        {
            vec.Normalize();
            return vec;
        }

        private static Vector3 GetSphereTangent(Vector3 position)
        {
            // Derive a rotation from the position (which rotates from 0,0,-1 to our position)
            position.Normalize();
            Quaternion rot;
            if (position == Vector3.UnitZ)
            {
                rot = Quaternion.Identity;
            }
            else
            {
                Vector3 normal = Vector3.Cross(position, Vector3.UnitZ);
                normal.Normalize();
                float ang = (float)Math.Acos(position.Z);
                rot = Quaternion.RotationAxis(normal, -ang);
            }

            // Transform unit X
            return Util.Vector3Transform(Vector3.UnitX, rot);
        }
    }
}
