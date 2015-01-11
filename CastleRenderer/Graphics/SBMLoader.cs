using System;
using System.IO;
using System.Collections.Generic;

using SlimDX;

namespace CastleRenderer.Graphics
{
    /// <summary>
    /// Loads SBM models into the scene
    /// </summary>
    public class SBMLoader
    {
        private string filename;
        private SBMMesh[] meshes;

        private class SBMMesh
        {
            public Mesh Mesh;
            public string[] Materials;
        }

        public SBMLoader(string filename)
        {
            // Store filename
            this.filename = filename;
        }

        /// <summary>
        /// Loads this SBM model
        /// </summary>
        /// <returns></returns>
        public bool Load(out string err)
        {
            // Check the file exists
            if (!File.Exists(filename))
            {
                err = "File not found";
                return false;
            }

            // Load it
            using (FileStream strm = File.OpenRead(filename))
            {
                // Create reader
                BinaryReader rdr = new BinaryReader(strm);

                // Read header
                string magic = new string(rdr.ReadChars(4));
                if (magic != "SBM\0")
                {
                    err = "Specified file is not an SBM file";
                    return false;
                }
                int version = rdr.ReadInt32();
                if (version != 1)
                {
                    err = "Unsupported SBM version";
                    return false;
                }
                int num_materials = rdr.ReadInt32();
                int num_meshes = rdr.ReadInt32();

                // Read all materials
                string[] materials = new string[num_materials];
                for (int i = 0; i < num_materials; i++)
                    materials[i] = rdr.ReadNullTerminatedString();

                // Read all meshes
                meshes = new SBMMesh[num_meshes];
                for (int i = 0; i < num_meshes; i++)
                {
                    // Read mesh header
                    int num_vertices = rdr.ReadInt32();
                    int num_submeshes = rdr.ReadInt32();

                    // Create mesh builder
                    MeshBuilder builder = new MeshBuilder();
                    builder.UseNormals = true;
                    builder.UseTexCoords = true;
                    builder.UseTangents = true;

                    // Read all vertices
                    for (int j = 0; j < num_vertices; j++)
                    {
                        builder.AddPosition(new Vector3(rdr.ReadSingle(), rdr.ReadSingle(), rdr.ReadSingle()));
                        builder.AddNormal(new Vector3(rdr.ReadSingle(), rdr.ReadSingle(), rdr.ReadSingle()));
                        builder.AddTextureCoord(new Vector2(rdr.ReadSingle(), rdr.ReadSingle()));
                        Vector3 tangent = new Vector3(rdr.ReadSingle(), rdr.ReadSingle(), rdr.ReadSingle());
                        builder.AddTangent(tangent);
                    }

                    // Loop each submesh
                    string[] meshmats = new string[num_submeshes];
                    for (int j = 0; j < num_submeshes; j++)
                    {
                        // Read submesh header
                        int num_indices = rdr.ReadInt32();
                        int material_index = rdr.ReadInt32();
                        meshmats[j] = materials[material_index];

                        // Read all indices
                        for (int k = 0; k < num_indices; k++)
                            builder.AddIndex(j, rdr.ReadUInt32());
                    }

                    // Add element to mesh array
                    SBMMesh sbmmesh = new SBMMesh();
                    sbmmesh.Mesh = builder.Build();
                    sbmmesh.Materials = meshmats;
                    meshes[i] = sbmmesh;
                }
            }

            // Success
            err = null;
            return true;
        }

        /// <summary>
        /// The number of meshes loaded by this loader
        /// </summary>
        public int MeshCount
        {
            get
            {
                return meshes.Length;
            }
        }

        /// <summary>
        /// Gets the mesh data at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="mesh"></param>
        /// <param name="materials"></param>
        public void GetMesh(int index, out Mesh mesh, out string[] materials)
        {
            mesh = meshes[index].Mesh;
            materials = meshes[index].Materials;
        }

    }
}
