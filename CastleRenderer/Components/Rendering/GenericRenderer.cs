using System;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;
using CastleRenderer.Graphics.MaterialSystem;

using SlimDX;

namespace CastleRenderer.Components
{
    /// <summary>
    /// Renders a generic object
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    public abstract class GenericRenderer : BaseComponent
    {
        /// <summary>
        /// Gets the material parameter block for the renderer
        /// </summary>
        public MaterialParameterStruct<CBuffer_ObjectTransform> ObjectTransformParameterBlock { get; private set; }

        public override void OnAttach()
        {
            // Base attach
            base.OnAttach();

            // Hook transform change
            Transform transform = Owner.GetComponent<Transform>();
            transform.OnTransformChange += transform_OnTransformChange;

            // Initialise parameter blocks
            var ctxt = Owner.Root.GetComponent<Renderer>().Device.ImmediateContext;
            ObjectTransformParameterBlock = new MaterialParameterStruct<CBuffer_ObjectTransform>(ctxt, new CBuffer_ObjectTransform { ModelMatrix = transform.ObjectToWorld });
        }

        public override void OnDetach()
        {
            // Base detach
            base.OnDetach();

            // Unhook transform change
            Transform transform = Owner.GetComponent<Transform>();
            transform.OnTransformChange -= transform_OnTransformChange;
        }

        private void transform_OnTransformChange(Transform sender)
        {
            UpdateMaterialParameterBlocks();
        }

        protected virtual void UpdateMaterialParameterBlocks()
        {
            Transform transform = Owner.GetComponent<Transform>();
            ObjectTransformParameterBlock.Value = new CBuffer_ObjectTransform { ModelMatrix = transform.ObjectToWorld };
        }

    }
}