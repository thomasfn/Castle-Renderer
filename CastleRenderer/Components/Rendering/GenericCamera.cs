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
            CameraParameterBlock = new MaterialParameterStruct<CBuffer_Camera>(ctxt, new CBuffer_Camera { CameraPosition = transform.Position, CameraForward = transform.Forward });
            CameraTransformParameterBlock = new MaterialParameterStruct<CBuffer_CameraTransform>(ctxt, new CBuffer_CameraTransform { ProjectionMatrix = Projection, ViewMatrix = transform.WorldToObject });
        }

        protected override void UpdateMaterialParameterBlocks()
        {
            base.UpdateMaterialParameterBlocks();

            Transform transform = Owner.GetComponent<Transform>();
            CameraParameterBlock.Value = new CBuffer_Camera { CameraPosition = transform.Position, CameraForward = transform.Forward };

            var view = transform.WorldToObject;
            view.Invert();
            view = Matrix.Transpose(view);

            CameraTransformParameterBlock.Value = new CBuffer_CameraTransform { ProjectionMatrix = Projection, ViewMatrix = transform.WorldToObject, ViewMatrixInvTrans = view };
        }
    }
}
