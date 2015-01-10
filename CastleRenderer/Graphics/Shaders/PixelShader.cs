using System;
using System.Collections.Generic;

using SlimDX;
using SlimDX.D3DCompiler;

using d3dPixelShader = SlimDX.Direct3D11.PixelShader;

namespace CastleRenderer.Graphics.Shaders
{
    using SlimDX.Direct3D11;

    /// <summary>
    /// Represents a compiled vertex shader
    /// </summary>
    public class PixelShader : IShader
    {
        /// <summary>
        /// Gets the shader type
        /// </summary>
        public ShaderType Type { get { return ShaderType.Pixel; } }

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

        // The actual shader object
        private d3dPixelShader shader;

        /// <summary>
        /// Initialises a new instance of the VertexShader class
        /// </summary>
        /// <param name="device"></param>
        /// <param name="raw"></param>
        public PixelShader(Device device, byte[] raw, string name = "Pixel Shader")
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
            shader = new d3dPixelShader(device, Bytecode);
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
            context.PixelShader.Set(shader);
        }

        /// <summary>
        /// Sets a constant buffer on this shader
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="buffer"></param>
        public void SetConstantBuffer(DeviceContext context, int slot, Buffer buffer)
        {
            context.PixelShader.SetConstantBuffer(buffer, slot);
        }

        /// <summary>
        /// Sets a resource on this shader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="slot"></param>
        /// <param name="resource"></param>
        public void SetResource(DeviceContext context, int slot, ShaderResourceView resource)
        {
            context.PixelShader.SetShaderResource(resource, slot);
        }

        /// <summary>
        /// Sets a sampler state on this shader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="slot"></param>
        /// <param name="samplerstate"></param>
        public void SetSamplerState(DeviceContext context, int slot, SamplerState samplerstate)
        {
            context.PixelShader.SetSampler(samplerstate, slot);
        }
    }
}
