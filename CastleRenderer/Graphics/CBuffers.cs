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
        public float ClipEnabled;
        public Vector4 ClipPlane;
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
        public Vector3 CameraPosition;
        public Vector3 CameraForward;
    }

    /// <summary>
    /// CBuffer that represents camera state
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 208)]
    public struct CBuffer_CameraTransform
    {
        public Matrix ProjectionMatrix;
        public Matrix ViewMatrix;
        public Matrix ViewMatrixRotOnly;
        public float Paraboloid;
        public float PBFar, PBNear, PBDir;
    }

    /// <summary>
    /// CBuffer that represents post-process effect info
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 16)]
    public struct CBuffer_PPEffectInfo
    {
        public Vector2 ImageSize;
    }

    /// <summary>
    /// CBuffer that represents reflection info
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 64)]
    public struct CBuffer_ReflectionInfo
    {
        public Matrix ReflViewMatrix;
    }

    /// <summary>
    /// CBuffer that represents shadow info
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 80)]
    public struct CBuffer_ShadowInfo
    {
        public Matrix ShadowMatrix;
        public float UseShadowMapping;
    }
}
