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

    }
}
