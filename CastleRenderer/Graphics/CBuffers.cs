using System;
using System.Runtime.InteropServices;

using SlimDX;

namespace CastleRenderer.Graphics
{
    /// <summary>
    /// CBuffer that controls clipping plane
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 32)]
    public struct CBuffer_Clip
    {
        public float Enabled;
        public Vector4 Plane;
    }

    /// <summary>
    /// CBuffer that represents object transform
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 64)]
    public struct CBuffer_ObjectTransform
    {
        public Matrix ModelMatrix;
    }

    /// <summary>
    /// CBuffer that represents camera state
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 32)]
    public struct CBuffer_Camera
    {
        public Vector3 Position;
        public Vector3 Forward;
    }

    /// <summary>
    /// CBuffer that represents camera state
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 128)]
    public struct CBuffer_CameraTransform
    {
        public Matrix Projection;
        public Matrix View;
    }
}
