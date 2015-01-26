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
    public enum LightType { None, Ambient, Directional, Spot, Point }

    public class LightComparer : IComparer<Light>
    {
        public int Compare(Light x, Light y)
        {
            return Comparer<int>.Default.Compare((int)x.Type, (int)y.Type);
        }
    }

    /// <summary>
    /// Represents a light source in 3D space
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    [ComponentPriority(0)]
    public class Light : BaseComponent
    {
        /// <summary>
        /// The type of this light
        /// </summary>
        public LightType Type { get; set; }

        /// <summary>
        /// The colour of this light
        /// </summary>
        public Color3 Colour { get; set; }

        /// <summary>
        /// The range of this light, if it's a spot or point light
        /// </summary>
        public float Range { get; set; }

        // The parameter set for this light
        private MaterialParameterSet lightpset;

        /// <summary>
        /// Applies this light's settings to the specified material
        /// </summary>
        /// <param name="material"></param>
        public void ApplyLightSettings(Material material)
        {
            // Initialise the parameter set if needed
            if (lightpset == null)
            {
                lightpset = material.MainPipeline.CreateParameterSet(Type.ToString());
            }

            // Cache the transform
            Transform transform = Owner.GetComponent<Transform>();

            // Do we have a shadow map?
            ShadowCaster caster = Owner.GetComponent<ShadowCaster>();
            if (caster != null)
            {
                // TODO: This
                //material.SetParameter("shadowmap", caster.ShadowTexture);
                lightpset.SetParameter("ShadowMatrix", caster.ProjectionView);
            }

            // All lights have colour
            lightpset.SetParameter("Colour", new Vector3(Colour.Red, Colour.Green, Colour.Blue));

            // Switch on type
            switch (Type)
            {
                case LightType.Directional:
                    lightpset.SetParameter("Position", transform.Position);
                    lightpset.SetParameter("Direction", transform.Forward);
                    break;
                case LightType.Point:
                    lightpset.SetParameter("Position", transform.Position);
                    lightpset.SetParameter("Range", Range);
                    break;
                case LightType.Spot:
                    lightpset.SetParameter("Position", transform.Position);
                    lightpset.SetParameter("Direction", transform.Forward);
                    lightpset.SetParameter("Range", Range);
                    break;
                    
            }

            // Apply to material
            material.SetParameterBlock(Type.ToString(), lightpset);
        }

        /// <summary>
        /// Called when a component wishes to know all active lights
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(PopulateLightList))]
        public void OnPopulateCameraList(PopulateLightList msg)
        {
            // For now, assume we're always active
            if (Type != LightType.None)
                msg.Lights.Add(this);
        }
    }
}