using System;

using SlimDX;
using SlimDX.Direct3D11;

using CastleRenderer.Graphics.MaterialSystem;

namespace CastleRenderer.Graphics
{
    public enum MeshTopology {  Points, Triangles  }

    /// <summary>
    /// Represents a mesh with a set of vertices and indices
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// The position array
        /// </summary>
        public Vector3[] Positions { get; set; }

        /// <summary>
        /// The normal array
        /// </summary>
        public Vector3[] Normals { get; set; }

        /// <summary>
        /// The texture coordinate array
        /// </summary>
        public Vector2[] TextureCoordinates { get; set; }

        /// <summary>
        /// The tangent array
        /// </summary>
        public Vector3[] Tangents { get; set; }

        /// <summary>
        /// The index arrays
        /// </summary>
        public uint[][] Submeshes { get; set; }

        /// <summary>
        /// The bounding box
        /// </summary>
        public BoundingBox AABB { get; set; }

        /// <summary>
        /// The iteration of this mesh (when a change is made, increment this to get it updated)
        /// </summary>
        public int Iteration { get; set; }

        /// <summary>
        /// The topology of this mesh
        /// </summary>
        public MeshTopology Topology { get; set; }

        private D3DMesh d3dmesh;

        ~Mesh()
        {
            if (d3dmesh != null) d3dmesh.Dispose();
        }

        public void Upload(Device device, DeviceContext context)
        {
            if (d3dmesh != null)
                d3dmesh.Update();
            else
            {
                d3dmesh = new D3DMesh(device, context, this);
                if (Topology == MeshTopology.Points)
                    d3dmesh.Topology = PrimitiveTopology.PointList;
                else if (Topology == MeshTopology.Triangles)
                    d3dmesh.Topology = PrimitiveTopology.TriangleList;
                d3dmesh.Init();
            }
        }

        public bool Render(MaterialPipeline pipeline, int submesh)
        {
            if (d3dmesh == null) return false;
            if (d3dmesh.Iteration < Iteration) return false;
            d3dmesh.SetSubmesh(submesh);
            d3dmesh.Render(pipeline);
            return true;
        }

    }
}
