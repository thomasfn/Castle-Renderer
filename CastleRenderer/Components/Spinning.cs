using System;
using System.Collections.Generic;
using System.Windows.Forms;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;

using SlimDX;

namespace CastleRenderer.Components
{
    /// <summary>
    /// A component that spins the object around a certain axis at a certain speed
    /// The object will be rotated in local space (aka, relative to it's parent)
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    public class Spinning : BaseComponent
    {
        /// <summary>
        /// The axis to spin around
        /// </summary>
        public Vector3 Axis { get; set; }

        /// <summary>
        /// The speed at which to spin (in radians per second)
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Called when it's time to update the frame
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(FrameMessage))]
        public void OnFrame(FrameMessage msg)
        {
            // Determine the rotation
            Quaternion rotation = Quaternion.RotationAxis(Axis, msg.DeltaTime * Speed);

            // Apply
            Transform transform = Owner.GetComponent<Transform>();
            transform.LocalRotation = transform.LocalRotation * rotation;
        }
    }
}