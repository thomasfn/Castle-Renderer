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

        /// <summary>
        /// Whether or not to use ambient occlusion
        /// </summary>
        public bool UseAO { get; set; }

        /// <summary>
        /// Gets a ray for the specified pixel coordinates
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <returns></returns>
        public override Ray GetRay(int px, int py)
        {
            // Convert to device coords
            float dx = (px / (float)Viewport.Width) * 2.0f - 1.0f;
            float dy = (py / (float)Viewport.Height) * 2.0f - 1.0f;

            // Unproject
            Transform transform = Owner.GetComponent<Transform>();
            Matrix viewproj = transform.WorldToObject * Projection;
            //Vector3 worldnear = Vector3.Unproject(new Vector3(px, py, 0.0f), 0.0f, 0.0f, Viewport.Width, Viewport.Height, NearZ, FarZ, viewproj);
            Vector3 worldfar = Vector3.Unproject(new Vector3(px, py, 1.0f), 0.0f, 0.0f, Viewport.Width, Viewport.Height, NearZ, FarZ, viewproj);
            Vector3 dir = worldfar - transform.Position;
            dir.Normalize();

            // Create ray
            return new Ray(transform.Position, dir);
        }

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
