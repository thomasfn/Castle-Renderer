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
    public class PostProcessEffectComparer : IComparer<PostProcessEffect>
    {
        public int Compare(PostProcessEffect x, PostProcessEffect y)
        {
            return Comparer<int>.Default.Compare(x.EffectPriority, y.EffectPriority);
        }
    }

    /// <summary>
    /// A post-process effect that alters the final image before it's blitted to the screen
    /// </summary>
    public class PostProcessEffect : BaseComponent
    {
        /// <summary>
        /// The priority of this effect when considering what order to process effects in
        /// </summary>
        public int EffectPriority { get; set; }

        /// <summary>
        /// The material to use when processing this effect
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// The number of passes to run the effect for
        /// </summary>
        public int Passes { get; set; }

        /// <summary>
        /// Called when it's time to populate the render queue
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(PopulateRenderQueue))]
        public void OnPopulateRenderQueue(PopulateRenderQueue msg)
        {
            // Sanity check
            if (Material == null) return;

            // Queue us
            msg.SceneManager.QueueEffect(this);
        }

    }
}