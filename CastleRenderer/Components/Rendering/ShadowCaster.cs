using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;

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
        /// The resolution of this shadow caster
        /// </summary>
        public int Resolution { get; set; }

        /// <summary>
        /// The render target associated with this shadow caster
        /// </summary>
        public RenderTarget RT { get; private set; }

        /// <summary>
        /// The shadowmap associated with this shadow caster
        /// </summary>
        public Texture2D ShadowTexture { get; private set; }

        /// <summary>
        /// The scale of this shadow caster
        /// </summary>
        public float Scale { get; set; }

        public override void OnAttach()
        {
            // Base attach
            base.OnAttach();

            // Get light
            Light light = Owner.GetComponent<Light>();

            // Setup matrix
            switch (light.Type)
            {
                case LightType.Directional:
                    Projection = Matrix.OrthoOffCenterLH(-0.5f * Scale, 0.5f * Scale, 0.5f * Scale, -0.5f * Scale, 0.125f, 256.0f);
                    //Projection = Matrix.Identity;
                    break;
                default:
                    Console.WriteLine("WARNING: ShadowCaster does not yet support light of type {0}!", light.Type);
                    break;
            }

            // Setup RT
            RT = Owner.Root.GetComponent<Renderer>().CreateRenderTarget(1, Resolution, "Shadowmap");
            RT.ClearColour = new Color4(1000.0f, 1000.0f, 1000.0f);
            RT.AddDepthComponent();
            int idx = RT.AddTextureComponent(SlimDX.DXGI.Format.R32G32B32A32_Float);
            RT.Finish();
            ShadowTexture = RT.GetTexture(idx);
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
    }
}
