using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;
using CastleRenderer.Graphics.MaterialSystem;

using SlimDX;
using SlimDX.Direct3D11;

namespace CastleRenderer.Components
{
    public enum CameraType { Orthographic, Perspective }

    public class CameraComparer : IComparer<Camera>
    {
        public int Compare(Camera x, Camera y)
        {
            return Comparer<int>.Default.Compare(x.RenderPriority, y.RenderPriority);
        }
    }

    public enum BackgroundType { None, Skybox }

    /// <summary>
    /// Represents a camera in the 3D world
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    [ComponentPriority(1)]
    public class Camera : GenericCamera
    {
        /// <summary>
        /// The projection type of this camera
        /// </summary>
        public CameraType ProjectionType { get; set; }

        /// <summary>
        /// The viewport of this camera
        /// </summary>
        public Viewport Viewport { get; set; }

        /// <summary>
        /// The near z plane
        /// </summary>
        public float NearZ { get; set; }

        /// <summary>
        /// The far z plane
        /// </summary>
        public float FarZ { get; set; }

        /// <summary>
        /// The vertical field of view (if applicable) of this camera
        /// </summary>
        public float FoV { get; set; }

        /// <summary>
        /// The background type of this camera
        /// </summary>
        public BackgroundType Background { get; set; }

        /// <summary>
        /// The name of the skybox for this camera
        /// </summary>
        public Material Skybox { get; set; }

        /// <summary>
        /// The render target of this camera
        /// </summary>
        public RenderTarget Target { get; set; }

        /// <summary>
        /// The priority of this camera in the rendering system
        /// </summary>
        public int RenderPriority { get; set; }

        /// <summary>
        /// Whether or not to use a clipping plane
        /// </summary>
        public bool UseClipping { get; set; }

        /// <summary>
        /// The clipping plane to use
        /// </summary>
        public Plane ClipPlane { get; set; }

        public override void OnAttach()
        {
            // Base attach
            base.OnAttach();

            // Setup matrix
            if (ProjectionType == CameraType.Orthographic)
                Projection = Matrix.OrthoLH(1.0f, 1.0f, NearZ, FarZ);
            else if (ProjectionType == CameraType.Perspective)
                Projection = Matrix.PerspectiveFovLH(FoV, Viewport.Width / Viewport.Height, NearZ, FarZ);

            // Update material parameter blocks
            UpdateMaterialParameterBlocks();
        }

        /// <summary>
        /// Called when a component wishes to know all active cameras
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(PopulateCameraList))]
        public void OnPopulateCameraList(PopulateCameraList msg)
        {
            // For now, assume we're always active
            msg.Cameras.Add(this);
        }
    }
}
