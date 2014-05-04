using System;
using System.Threading;

using CastleRenderer.Structures;
using CastleRenderer.Messages;

namespace CastleRenderer.Components
{
    /// <summary>
    /// A component that aims to maintain the target FPS via sleeping
    /// </summary>
    public class Sleeper : BaseComponent
    {
        /// <summary>
        /// The target FPS
        /// </summary>
        public float TargetFPS { get; set; }

        /// <summary>
        /// Called when it's time to update the frame
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(FrameMessage))]
        public void OnFrame(FrameMessage msg)
        {
            // Get target fps in frametime
            float frametime = 1.0f / TargetFPS;

            // Find delta time
            float tosleep = frametime - msg.DeltaTime;
            if (tosleep > 0.0f) Thread.Sleep((int)(tosleep * 1000.0f));

        }

    }

}