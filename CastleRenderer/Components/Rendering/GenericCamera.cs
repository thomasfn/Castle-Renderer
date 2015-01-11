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
    /// Represents a generic camera from which the scene might be rendered
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    public abstract class GenericCamera : GenericRenderer
    {
        /// <summary>
        /// The projection matrix
        /// </summary>
        public Matrix Projection { get; protected set; }

        /// <summary>
        /// The projection view matrix
        /// </summary>
        public Matrix ProjectionView
        {
            get
            {
                return Owner.GetComponent<Transform>().WorldToObject * Projection;
            }
        }

        /// <summary>
        /// Gets the material parameter block for the camera
        /// </summary>
        public MaterialParameterStruct<CBuffer_Camera> CameraParameterBlock { get; private set; }

        /// <summary>
        /// Gets the material parameter block for the camera
        /// </summary>
        public MaterialParameterStruct<CBuffer_CameraTransform> CameraTransformParameterBlock { get; private set; }

        public override void OnAttach()
        {
            // Base attach
            base.OnAttach();

            // Initialise parameter blocks
            Transform transform = Owner.GetComponent<Transform>();
            var ctxt = Owner.Root.GetComponent<Renderer>().Device.ImmediateContext;
            CameraParameterBlock = new MaterialParameterStruct<CBuffer_Camera>(ctxt, default(CBuffer_Camera));
            CameraTransformParameterBlock = new MaterialParameterStruct<CBuffer_CameraTransform>(ctxt, default(CBuffer_CameraTransform));
        }

        protected override void UpdateMaterialParameterBlocks()
        {
            base.UpdateMaterialParameterBlocks();

            Transform transform = Owner.GetComponent<Transform>();
            CameraParameterBlock.Value = new CBuffer_Camera { CameraPosition = transform.Position, CameraForward = transform.Forward };

            var view = transform.WorldToObject;
            view.set_Rows(3, Vector4.Zero);
            view.set_Columns(3, Vector4.Zero);
            view.M44 = 1.0f;
            CameraTransformParameterBlock.Value = new CBuffer_CameraTransform { ProjectionMatrix = Projection, ViewMatrix = transform.WorldToObject, ViewMatrixRotOnly = view };
        }
    }
}
