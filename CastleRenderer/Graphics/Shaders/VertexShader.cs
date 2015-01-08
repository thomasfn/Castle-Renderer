using System;
using System.Collections.Generic;

using SlimDX;
using SlimDX.D3DCompiler;

using d3dVertexShader = SlimDX.Direct3D11.VertexShader;

namespace CastleRenderer.Graphics.Shaders
{
    using SlimDX.Direct3D11;

    /// <summary>
    /// Represents a compiled vertex shader
    /// </summary>
    public class VertexShader : IShader
    {
        /// <summary>
        /// Gets the shader type
        /// </summary>
        public ShaderType Type { get { return ShaderType.Vertex; } }

        /// <summary>
        /// Gets the shader bytecode
        /// </summary>
        public ShaderBytecode Bytecode { get; private set; }

        /// <summary>
        /// Gets the owner device
        /// </summary>
        public Device OwnerDevice { get; private set; }

        // The actual shader object
        private d3dVertexShader shader;

        /// <summary>
        /// Initialises a new instance of the VertexShader class
        /// </summary>
        /// <param name="device"></param>
        /// <param name="raw"></param>
        public VertexShader(Device device, byte[] raw, string name = "Vertex Shader")
        {
            // Store parameters
            OwnerDevice = device;

            // Initialise bytecode
            using (DataStream strm = new DataStream(raw, true, false))
                Bytecode = new ShaderBytecode(strm);
            shader = new d3dVertexShader(device, Bytecode);
            shader.DebugName = name;
        }

        /// <summary>
        /// Disposes this shader
        /// </summary>
        public void Dispose()
        {
            if (shader != null)
            {
                shader.Dispose();
                shader = null;
                Bytecode.Dispose();
                Bytecode = null;
            }
        }

        /// <summary>
        /// Makes this shader active on the specified context
        /// </summary>
        /// <param name="context"></param>
        public void MakeActive(DeviceContext context)
        {
            context.VertexShader.Set(shader);
        }

        /// <summary>
        /// Sets a constant buffer on this shader
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="buffer"></param>
        public void SetConstantBuffer(DeviceContext context, int slot, Buffer buffer)
        {
            context.VertexShader.SetConstantBuffer(buffer, slot);
        }
    }
}
