using System;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;
using CastleRenderer.Graphics.MaterialSystem;

using SlimDX;

namespace CastleRenderer.Components
{
    /// <summary>
    /// Renders a mesh with a set of materials
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    [ComponentPriority(5)]
    public class MeshRenderer : GenericRenderer
    {
        /// <summary>
        /// The mesh to render
        /// </summary>
        public Mesh Mesh { get; set; }

        /// <summary>
        /// The materials to use
        /// </summary>
        public Material[] Materials { get; set; }

        /// <summary>
        /// Called when it's time to populate the render queue
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(PopulateRenderQueue))]
        public void OnPopulateRenderQueue(PopulateRenderQueue msg)
        {
            // Sanity check
            if (Mesh == null || Materials == null) return;

            // Get transform matrix
            Transform transform = Owner.GetComponent<Transform>();
            Matrix mtx = transform.ObjectToWorld;

            // Render all submeshes
            for (int i = 0; i < Materials.Length; i++)
                if (Materials[i] != null)
                    msg.SceneManager.QueueDraw(Mesh, i, Materials[i], ObjectTransformParameterBlock);
        }

    }
}