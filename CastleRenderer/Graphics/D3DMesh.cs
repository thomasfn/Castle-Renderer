using System;
using System.Collections.Generic;

using SlimDX;
using SlimDX.Direct3D11;

using Buffer = SlimDX.Direct3D11.Buffer;

using CastleRenderer.Graphics.MaterialSystem;

namespace CastleRenderer.Graphics
{
    /// <summary>
    /// A binding between a mesh object and a renderable 3D mesh
    /// </summary>
    public class D3DMesh : IDisposable
    {
        private Mesh mesh;
        private Device device;
        private DeviceContext context;

        private Buffer vtxBuffer;
        private Buffer ibIndices;

        private InputElement[] inputelements;
        private VertexBufferBinding bufferbinding;

        private SlimDX.DXGI.Format indexformat;

        private Buffer[] submeshes;
        private int curindices;

        private Dictionary<MaterialPipeline, InputLayout> pipelinemap;

        public int Iteration { get; private set; }

        private int elementsize, buffersize;

        public PrimitiveTopology Topology { get; set; }

        private Buffer ArrayToBuffer(ushort[] array, ResourceUsage usage)
        {
            var strm = new DataStream(sizeof(ushort) * array.Length, true, true);
            for (int i = 0; i < array.Length; i++)
                strm.Write(array[i]);
            strm.Position = 0;
            return new Buffer(device, strm, new BufferDescription((int)strm.Length, ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0));
        }
        private Buffer ArrayToBuffer(uint[] array, ResourceUsage usage)
        {
            var strm = new DataStream(sizeof(uint) * array.Length, true, true);
            for (int i = 0; i < array.Length; i++)
                strm.Write(array[i]);
            strm.Position = 0;
            return new Buffer(device, strm, new BufferDescription((int)strm.Length, ResourceUsage.Default, BindFlags.IndexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0));
        }

        public D3DMesh(Device device, DeviceContext context, Mesh mesh)
        {
            // Store variables
            this.mesh = mesh;
            this.device = device;
            this.context = context;

            // Defaults
            Topology = PrimitiveTopology.TriangleList;
        }

        public void Init()
        {
            // Determine element and buffer size
            elementsize = Vector3.SizeInBytes;
            if (mesh.Normals != null) elementsize += Vector3.SizeInBytes;
            if (mesh.TextureCoordinates != null) elementsize += Vector2.SizeInBytes;
            if (mesh.Tangents != null) elementsize += Vector3.SizeInBytes;
            buffersize = elementsize * mesh.Positions.Length;

            // Determine input elements
            var inputelementslist = new List<InputElement>();
            int curoffset = 0;
            inputelementslist.Add(new InputElement("POSITION", 0, SlimDX.DXGI.Format.R32G32B32_Float, curoffset, 0));
            curoffset += Vector3.SizeInBytes;
            if (mesh.Normals != null)
            {
                inputelementslist.Add(new InputElement("NORMAL", 0, SlimDX.DXGI.Format.R32G32B32_Float, curoffset, 0));
                curoffset += Vector3.SizeInBytes;
            }
            if (mesh.TextureCoordinates != null)
            {
                inputelementslist.Add(new InputElement("TEXCOORD", 0, SlimDX.DXGI.Format.R32G32_Float, curoffset, 0));
                curoffset += Vector2.SizeInBytes;
            }
            if (mesh.Tangents != null)
            {
                inputelementslist.Add(new InputElement("TANGENT", 0, SlimDX.DXGI.Format.R32G32B32_Float, curoffset, 0));
                curoffset += Vector3.SizeInBytes;
            }
            inputelements = inputelementslist.ToArray();

            // Write the stream
            var strm = new DataStream(buffersize, true, true);
            for (int i = 0; i < mesh.Positions.Length; i++)
            {
                strm.Write(mesh.Positions[i]);
                if (mesh.Normals != null) strm.Write(mesh.Normals[i]);
                if (mesh.TextureCoordinates != null) strm.Write(mesh.TextureCoordinates[i]);
                if (mesh.Tangents != null) strm.Write(mesh.Tangents[i]);
            }
            strm.Position = 0;

            // Build the buffer
            vtxBuffer = new Buffer(device, strm, new BufferDescription((int)strm.Length, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0));
            bufferbinding = new VertexBufferBinding(vtxBuffer, elementsize, 0);

            // Build the indices buffer(s)
            indexformat = SlimDX.DXGI.Format.R32_UInt;
            submeshes = new Buffer[mesh.Submeshes.Length];
            for (int i = 0; i < submeshes.Length; i++ )
                submeshes[i] = ArrayToBuffer(mesh.Submeshes[i], ResourceUsage.Default);
            

            // Build the shader map
            pipelinemap = new Dictionary<MaterialPipeline, InputLayout>();

            // Update iteration
            Iteration = mesh.Iteration;
        }

        public void Update()
        {
            // Dispose old buffers
            vtxBuffer.Dispose();
            foreach (var buffer in submeshes)
                buffer.Dispose();

            // Write the stream
            var strm = new DataStream(buffersize, true, true);
            for (int i = 0; i < mesh.Positions.Length; i++)
            {
                strm.Write(mesh.Positions[i]);
                if (mesh.Normals != null) strm.Write(mesh.Normals[i]);
                if (mesh.TextureCoordinates != null) strm.Write(mesh.TextureCoordinates[i]);
                if (mesh.Tangents != null) strm.Write(mesh.Tangents[i]);
            }
            strm.Position = 0;

            // Build the buffer
            vtxBuffer = new Buffer(device, strm, new BufferDescription((int)strm.Length, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0));
            bufferbinding = new VertexBufferBinding(vtxBuffer, elementsize, 0);

            // Build the indices buffer(s)
            submeshes = new Buffer[mesh.Submeshes.Length];
            for (int i = 0; i < submeshes.Length; i++)
                submeshes[i] = ArrayToBuffer(mesh.Submeshes[i], ResourceUsage.Default);

            // Update iteration
            Iteration = mesh.Iteration;
        }

        public void SetSubmesh(int idx)
        {
            ibIndices = submeshes[idx];
            curindices = mesh.Submeshes[idx].Length;
        }

        private InputLayout GetInputLayout(MaterialPipeline pipeline)
        {
            if (pipelinemap.ContainsKey(pipeline)) return pipelinemap[pipeline];
            var layout = pipeline.CreateLayout(inputelements);
            pipelinemap.Add(pipeline, layout);
            return layout;
        }

        public static InputLayout CurrentInputLayout;
        public static VertexBufferBinding CurrentVertexBufferBinding;
        public static Buffer CurrentIndexBuffer;

        public void Render(MaterialPipeline pipeline)
        {
            InputLayout layout = GetInputLayout(pipeline);
            if (layout != CurrentInputLayout)
            {
                CurrentInputLayout = layout;
                context.InputAssembler.InputLayout = layout;
            }
            context.InputAssembler.PrimitiveTopology = Topology;
            if (bufferbinding != CurrentVertexBufferBinding)
            {
                CurrentVertexBufferBinding = bufferbinding;
                context.InputAssembler.SetVertexBuffers(0, bufferbinding);
            }
            if (ibIndices != CurrentIndexBuffer)
            {
                CurrentIndexBuffer = ibIndices;
                context.InputAssembler.SetIndexBuffer(ibIndices, indexformat, 0);
            }
            context.DrawIndexed(curindices, 0, 0);
        }


        public void Dispose()
        {
            if (vtxBuffer != null) vtxBuffer.Dispose();
            foreach (var submesh in submeshes)
                submesh.Dispose();
            foreach (var pair in pipelinemap)
                pair.Value.Dispose();
            pipelinemap.Clear();
        }
    }
}
