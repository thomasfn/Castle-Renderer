using System;
using System.Collections.Generic;

using SlimDX;
using SlimDX.D3DCompiler;

namespace CastleRenderer.Graphics.Shaders
{
    using SlimDX.Direct3D11;

    public enum ShaderType { Vertex, Geometry, Pixel }

    /// <summary>
    /// Specifies that the concrete implementation is a shader
    /// </summary>
    public interface IShader : IDisposable
    {
        /// <summary>
        /// Gets the shader type
        /// </summary>
        ShaderType Type { get; }

        /// <summary>
        /// Gets the shader bytecode
        /// </summary>
        ShaderBytecode Bytecode { get; }

        /// <summary>
        /// Gets the owner device
        /// </summary>
        Device OwnerDevice { get; }

        /// <summary>
        /// Gets the signature for this shader
        /// </summary>
        ShaderSignature Signature { get; }

        /// <summary>
        /// Makes this shader active on the specified context
        /// </summary>
        /// <param name="context"></param>
        void MakeActive(DeviceContext context);

        /// <summary>
        /// Sets a constant buffer on this shader
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="buffer"></param>
        void SetConstantBuffer(DeviceContext context, int slot, Buffer buffer);

        /// <summary>
        /// Sets all constant buffers on this shader
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="buffer"></param>
        void SetConstantBuffers(DeviceContext context, Buffer[] buffers);

        /// <summary>
        /// Sets a resource view on this shader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="slot"></param>
        /// <param name="resource"></param>
        void SetResource(DeviceContext context, int slot, ShaderResourceView resource);

        /// <summary>
        /// Sets all resource views on this shader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="slot"></param>
        /// <param name="resource"></param>
        void SetResources(DeviceContext context, ShaderResourceView[] resources);

        /// <summary>
        /// Sets a sampler state on this shader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="slot"></param>
        /// <param name="resource"></param>
        void SetSamplerState(DeviceContext context, int slot, SamplerState samplerstate);

        /// <summary>
        /// Sets all sampler states on this shader
        /// </summary>
        /// <param name="context"></param>
        /// <param name="slot"></param>
        /// <param name="resource"></param>
        void SetSamplerStates(DeviceContext context, SamplerState[] samplerstates);
    }
}
