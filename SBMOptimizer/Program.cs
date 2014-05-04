using System;
using System.IO;
using System.Collections.Generic;

using CastleRenderer.Graphics;

using SlimDX;

namespace SBMOptimizer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Missing input argument!");
                Console.ReadKey();
                return;
            }
            if (args.Length == 1)
            {
                Console.WriteLine("Missing output argument!");
                Console.ReadKey();
                return;
            }

            SBMLoader loader = new SBMLoader(args[0]);
            string err;
            if (!loader.Load(out err))
            {
                Console.WriteLine("Failed to load SBM file! {0}", err);
                Console.ReadKey();
                return;
            }

            Mesh merged;
            string[] materials;
            MergeMeshes(loader, out merged, out materials);

            using (FileStream strm = File.OpenWrite(args[1]))
            {
                ExportSBM(strm, merged, materials);
                strm.Close();
            }

            Console.WriteLine("SBM file written, task complete.");
            Console.ReadKey();
        }

        private static void MergeMeshes(SBMLoader loader, out Mesh result, out string[] materials)
        {
            if (loader.MeshCount == 0)
            {
                result = null;
                materials = null;
                return;
            }
            if (loader.MeshCount == 1)
            {
                loader.GetMesh(0, out result, out materials);
                return;
            }

            List<string> finalmaterials = new List<string>();

            MeshBuilder merged = new MeshBuilder();
            merged.UseNormals = true;
            merged.UseTangents = true;
            merged.UseTexCoords = true;

            int totalsubmeshes = 0;

            for (int i = 0; i < loader.MeshCount; i++)
            {
                Mesh cmesh;
                string[] curmats;
                loader.GetMesh(i, out cmesh, out curmats);

                totalsubmeshes += cmesh.Submeshes.Length;

                int baseindex = merged.CurrentVertexCount;
                merged.AddPositions(cmesh.Positions);
                merged.AddNormals(cmesh.Normals);
                merged.AddTextureCoords(cmesh.TextureCoordinates);
                merged.AddTangents(cmesh.Tangents);

                for (int j = 0; j < cmesh.Submeshes.Length; j++)
                {
                    int submeshindex;
                    if (finalmaterials.Contains(curmats[j]))
                        submeshindex = finalmaterials.IndexOf(curmats[j]);
                    else
                    {
                        submeshindex = finalmaterials.Count;
                        finalmaterials.Add(curmats[j]);
                    }

                    merged.AddIndices(submeshindex, cmesh.Submeshes[j], (uint)baseindex);
                }
                
            }

            Console.WriteLine("Merged {0} meshes with a total of {1} submeshes into 1 mesh with a total of {2} submeshes.", loader.MeshCount, totalsubmeshes, finalmaterials.Count);

            result = merged.Build();
            materials = finalmaterials.ToArray();
        }

        private static void ExportSBM(Stream strm, Mesh mesh, string[] materials)
        {
            BinaryWriter wtr = new BinaryWriter(strm);
            wtr.Write(new char[] { 'S', 'B', 'M', '\0' });
            wtr.Write(1);
            wtr.Write(materials.Length);
            wtr.Write(1);

            for (int i = 0; i < materials.Length; i++)
            {
                string mat = materials[i];
                for (int j = 0; j < mat.Length; j++)
                    wtr.Write(mat[j]);
                wtr.Write((char)'\0');
            }

            wtr.Write(mesh.Positions.Length);
            wtr.Write(mesh.Submeshes.Length);
            for (int i = 0; i < mesh.Positions.Length; i++)
            {
                WriteVector(wtr, mesh.Positions[i]);
                WriteVector(wtr, mesh.Normals[i]);
                WriteVector(wtr, mesh.TextureCoordinates[i]);
                WriteVector(wtr, mesh.Tangents[i]);
            }

            for (int i = 0; i < mesh.Submeshes.Length; i++)
            {
                uint[] indices = mesh.Submeshes[i];
                wtr.Write(indices.Length);
                wtr.Write(i);

                for (int j = 0; j < indices.Length; j++)
                    wtr.Write(indices[j]);
            }
            wtr.Flush();
        }

        private static void WriteVector(BinaryWriter wtr, Vector2 vec)
        {
            wtr.Write(vec.X);
            wtr.Write(vec.Y);
        }
        private static void WriteVector(BinaryWriter wtr, Vector3 vec)
        {
            wtr.Write(vec.X);
            wtr.Write(vec.Y);
            wtr.Write(vec.Z);
        }
    }
}
