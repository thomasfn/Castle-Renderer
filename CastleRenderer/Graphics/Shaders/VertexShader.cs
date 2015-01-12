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

        /// <summary>
        /// Gets the signature for this shader
        /// </summary>
        public ShaderSignature Signature { get; private set; }

        private static Buffer[] currentconstantbuffers;
        private static ShaderResourceView[] currentresourceviews;
        private static SamplerState[] currentsamplerstates;

        static VertexShader()
        {
            currentconstantbuffers = new Buffer[16];
            currentresourceviews = new ShaderResourceView[16];
            currentsamplerstates = new SamplerState[16];
        }

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

            // Initialise bytecode and signature
            using (DataStream strm = new DataStream(raw, true, false))
            {
                strm.Position = 0;
                Bytecode = new ShaderBytecode(strm);
                strm.Position = 0;
                Signature = ShaderSignature.GetInputOutputSignature(Bytecode);
            }

            // Create the shader
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

        /// <summary>
        /// Sets all constant buffers on this shader
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="buffer"></param>
        public void SetConstantBuffers(DeviceContext context, Buffer[] buffers)
        {
            context.VertexShader.SetConstantBuffers(buffers, 0, buffers.Length);
        }

        /// <summary>
        /// Sets a resource view on this shader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="slot"></param>
        /// <param name="resource"></param>
        public void SetResource(DeviceContext context, int slot, ShaderResourceView resource)
        {
            context.VertexShader.SetShaderResource(resource, slot);
        }

        /// <summary>
        /// Sets all resource views on this shader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="slot"></param>
        /// <param name="resource"></param>
        public void SetResources(DeviceContext context, ShaderResourceView[] resources)
        {
            context.VertexShader.SetShaderResources(resources, 0, resources.Length);
        }

        /// <summary>
        /// Sets a sampler state on this shader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="slot"></param>
        /// <param name="resource"></param>
        public void SetSamplerState(DeviceContext context, int slot, SamplerState samplerstate)
        {
            context.VertexShader.SetSampler(samplerstate, slot);
        }

        /// <summary>
        /// Sets all sampler states on this shader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="slot"></param>
        /// <param name="resource"></param>
        public void SetSamplerStates(DeviceContext context, SamplerState[] samplerstates)
        {
            context.VertexShader.SetSamplers(samplerstates, 0, samplerstates.Length);
        }
    }
}
