using System;
using System.Linq;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;

using SlimDX;

namespace CastleRenderer.Components
{
    /// <summary>
    /// A component that moves the object between a set of nodes in a loop
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    public class PathFollower : BaseComponent
    {
        /// <summary>
        /// Gets or sets the nodes to follow
        /// </summary>
        public Actor[] Nodes { get; set; }

        /// <summary>
        /// Gets or sets the speed at which to spin (in radians per second)
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Gets or sets whether the object should face towards the next node
        /// </summary>
        public bool FaceForwards { get; set; }

        private int targetnode;
        private Vector3[] nodeposarr;

        /// <summary>
        /// Called when this component has been attached to an actor
        /// </summary>
        public override void OnAttach()
        {
            // Call base
            base.OnAttach();

            // Get the node array
            nodeposarr = Nodes
                .Select((n) => n.GetComponent<Transform>().Position)
                .ToArray();

            // Locate nearest position
            float bestdist = float.MaxValue;
            int bestindex = 0;
            Vector3 mypos = Owner.GetComponent<Transform>().Position;
            for (int i = 0; i < nodeposarr.Length; i++)
            {
                float dist = (mypos - nodeposarr[bestindex]).LengthSquared();
                if (dist < bestdist)
                {
                    bestdist = dist;
                    bestindex = i;
                }
            }

            // Head to it
            targetnode = bestindex;
        }

        /// <summary>
        /// Called when it's time to update the frame
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(FrameMessage))]
        public void OnFrame(FrameMessage msg)
        {
            Transform transform = Owner.GetComponent<Transform>();

            Vector3 dir = nodeposarr[targetnode] - transform.Position;
            float dist = dir.Length();
            if (dist <= 1.0f)
            {
                NextNode();
                dir = nodeposarr[targetnode] - transform.Position;
                dist = dir.Length();
            }
            dir /= dist;

            transform.LocalPosition += dir * Speed * msg.DeltaTime;
            if (FaceForwards)
            {
                transform.LocalRotation = Quaternion.Slerp(transform.LocalRotation, Util.ForwardToRotation(dir), (float)Math.Pow(0.05, 1.0 - msg.DeltaTime));
            }
        }

        private void NextNode()
        {
            targetnode = (targetnode + 1) % nodeposarr.Length;
        }
    }
}