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
    /// <summary>
    /// Represents a shadow caster
    /// </summary>
    [RequiresComponent(typeof(Light))]
    [ComponentPriority(1)]
    public class ShadowCaster : GenericCamera
    {
        /// <summary>
        /// Gets or sets the resolution of this shadow caster
        /// </summary>
        public int Resolution { get; set; }

        /// <summary>
        /// Gets or sets the render target associated with this shadow caster
        /// </summary>
        public RenderTarget RT { get; private set; }

        /// <summary>
        /// Gets or sets the shadowmap associated with this shadow caster
        /// </summary>
        public Texture2D ShadowTexture { get; private set; }

        /// <summary>
        /// Gets or sets the scale of this shadow caster
        /// </summary>
        public float Scale { get; set; }

        /// <summary>
        /// Gets or sets if the position of the caster should be automatically updated
        /// </summary>
        public bool AutoComputePosition { get; set; }

        private bool ignoretransformchange;

        public override void OnAttach()
        {
            // Base attach
            base.OnAttach();

            // Calculate projection
            RecomputeProjection();

            // Setup RT
            RT = Owner.Root.GetComponent<Renderer>().CreateRenderTarget(1, Resolution, "Shadowmap");
            RT.ClearColour = new Color4(1000.0f, 1000.0f, 1000.0f);
            RT.AddDepthComponent();
            int idx = RT.AddTextureComponent(SlimDX.DXGI.Format.R32G32B32A32_Float);
            RT.Finish();
            ShadowTexture = RT.GetTexture(idx);

            // Update material parameter blocks
            UpdateMaterialParameterBlocks();

            // Hook transform
            Transform transform = Owner.GetComponent<Transform>();
            transform.OnTransformChange += transform_OnTransformChange;
        }

        private void RecomputeProjection()
        {
            // Get light
            Light light = Owner.GetComponent<Light>();

            // Setup matrix
            switch (light.Type)
            {
                case LightType.Directional:
                    Projection = Matrix.OrthoOffCenterLH(-0.5f * Scale, 0.5f * Scale, 0.5f * Scale, -0.5f * Scale, NearZ, FarZ);
                    break;
                case LightType.Spot:
                    Projection = Matrix.PerspectiveFovLH(light.Angle * 2.0f, 1.0f, NearZ, FarZ);
                    break;
                default:
                    Console.WriteLine("WARNING: ShadowCaster does not yet support light of type {0}!", light.Type);
                    break;
            }
        }

        public override void OnDetach()
        {
            // Base detach
            base.OnDetach();

            // Unhook transform
            Transform transform = Owner.GetComponent<Transform>();
            if (transform != null) transform.OnTransformChange -= transform_OnTransformChange;
        }

        private void transform_OnTransformChange(Transform sender)
        {
            if (ignoretransformchange) return;
            if (AutoComputePosition)
                RecomputePosition();
        }

        /// <summary>
        /// Called when a component wishes to know all active cameras
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(PopulateCameraList))]
        public void OnPopulateCameraList(PopulateCameraList msg)
        {
            // For now, assume we're always active
            msg.ShadowCasters.Add(this);
        }

        /// <summary>
        /// Called when the scene has finished loading
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(PostSceneLoadedMessage))]
        public void OnPostSceneLoaded(PostSceneLoadedMessage msg)
        {
            // Are we set to autocompute?
            if (AutoComputePosition)
                RecomputePosition();
        }

        private void RecomputePosition()
        {
            // Get scene bounding box
            BoundingBox bbox = Owner.Root.GetComponent<SceneManager>().ComputeSceneBBox();

            // Compute best light position and scale
            Transform transform = Owner.GetComponent<Transform>();
            Vector3 center = Vector3.Lerp(bbox.Minimum, bbox.Maximum, 0.5f);
            float cornerdist = (center - bbox.Minimum).Length();
            Vector3 pos = center - transform.Forward * cornerdist;
            float scale = (bbox.Maximum - bbox.Minimum).Length();

            // Update new position and scale
            ignoretransformchange = true;
            transform.LocalPosition = pos;
            ignoretransformchange = false;
            Scale = scale;

            // Calculate projection
            RecomputeProjection();

            // Update material parameter blocks
            UpdateMaterialParameterBlocks();
        }

        public override Ray GetRay(int px, int py)
        {
            throw new NotImplementedException();
        }
    }
}
