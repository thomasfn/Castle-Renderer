using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;
using CastleRenderer.Graphics.MaterialSystem;

using SlimDX;
using SlimDX.Direct3D11;
namespace CastleRenderer.Components.Rendering
{
    /// <summary>
    /// Manages the various sub-objects and processes required to render reflections
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    [ComponentPriority(10)]
    public class ReflectionProbe : BaseComponent
    {
        private Actor frontcamera, backcamera;

        /// <summary>
        /// Gets or sets the resolution of this probe
        /// </summary>
        public int Resolution { get; set; }

        /// <summary>
        /// Gets or sets the main camera to mimic
        /// </summary>
        public Actor MainCamera { get; set; }

        /// <summary>
        /// Gets the front target
        /// </summary>
        public RenderTarget FrontTarget { get; private set; }

        /// <summary>
        /// Gets the back target
        /// </summary>
        public RenderTarget BackTarget { get; private set; }

        private MaterialParameterStruct<CBuffer_ReflectionInfo> matpset_reflinfo;

        /// <summary>
        /// Called when this component has been attached to an actor
        /// </summary>
        public override void OnAttach()
        {
            // Call base
            base.OnAttach();

            // Create the RTs
            var renderer = Owner.Root.GetComponent<Renderer>();
            FrontTarget = renderer.CreateRenderTarget(1, Resolution, Resolution, string.Format("RT_{0}_Front_{1}", Owner.Name, 0));
            FrontTarget.AddTextureComponent();
            FrontTarget.AddDepthComponent();
            BackTarget = renderer.CreateRenderTarget(1, Resolution, Resolution, string.Format("RT_{0}_Back_{1}", Owner.Name, 0));
            BackTarget.AddTextureComponent();
            BackTarget.AddDepthComponent();

            // Get the main camera
            Camera maincam = MainCamera.GetComponent<Camera>();
            Camera camera;

            // Create the front and back cameras
            frontcamera = new Actor(Owner.MessagePool);
            frontcamera.Parent = Owner;
            frontcamera.AddComponent<Transform>();
            camera = frontcamera.AddComponent<Camera>();
            camera.Enabled = true;
            camera.ProjectionType = CameraType.Orthographic;
            camera.NearZ = maincam.NearZ;
            camera.FarZ = maincam.FarZ;
            camera.Background = maincam.Background;
            camera.Skybox = maincam.Skybox;
            camera.Viewport = new Viewport(0.0f, 0.0f, Resolution, Resolution);
            camera.Paraboloid = true;
            camera.ParaboloidDirection = 1.0f;
            camera.Target = FrontTarget;

            backcamera = new Actor(Owner.MessagePool);
            backcamera.Parent = Owner;
            backcamera.AddComponent<Transform>();
            camera = backcamera.AddComponent<Camera>();
            camera.Enabled = true;
            camera.ProjectionType = CameraType.Orthographic;
            camera.NearZ = maincam.NearZ;
            camera.FarZ = maincam.FarZ;
            camera.Background = maincam.Background;
            camera.Skybox = maincam.Skybox;
            camera.Viewport = new Viewport(0.0f, 0.0f, Resolution, Resolution);
            camera.Paraboloid = true;
            camera.ParaboloidDirection = -1.0f;
            camera.Target = BackTarget;

            matpset_reflinfo = new MaterialParameterStruct<CBuffer_ReflectionInfo>(renderer.Device.ImmediateContext, new CBuffer_ReflectionInfo { ReflViewMatrix = Owner.GetComponent<Transform>().WorldToObject });

            // Add us to the mesh renderer
            MeshRenderer meshrenderer = Owner.GetComponent<MeshRenderer>();
            if (meshrenderer != null && meshrenderer.Materials != null)
            {
                foreach (Material mat in meshrenderer.Materials)
                    if (mat != null)
                    {
                        mat.SetResource("FrontReflectionTexture0", renderer.AcquireResourceView(FrontTarget.GetTexture(0)));
                        mat.SetResource("BackReflectionTexture0", renderer.AcquireResourceView(BackTarget.GetTexture(0)));
                        mat.SetSamplerState("ReflectionSampler", renderer.Sampler_Clamp_Linear);
                        mat.SetParameterBlock("ReflectionInfo", matpset_reflinfo);
                    }
            }
        }

        /// <summary>
        /// Called when this component has been detached from an actor
        /// </summary>
        public override void OnDetach()
        {
            // Call base
            base.OnDetach();

            // Remove cameras
            frontcamera.Destroy(true);
            backcamera.Destroy(true);
        }

    }
}
