using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Physics2D;

using SlimDX;

namespace CastleRenderer.Components.Physics
{
    /// <summary>
    /// A component that represents a system of physically simulated 2D rigid bodies
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    public class PhysicsWorld2D : BaseComponent
    {
        // All rigid bodies
        private HashSet<RigidBody2D> rigidbodies;

        /// <summary>
        /// Gets or sets the integrator to use
        /// </summary>
        public IIntegrator2D Integrator { get; set; }

        /// <summary>
        /// Gets or sets the broadphase to use
        /// </summary>
        public IBroadPhase2D BroadPhase { get; set; }

        /// <summary>
        /// Gets or sets the collision resolver to use
        /// </summary>
        public ICollisionResolver2D CollisionResolver { get; set; }

        /// <summary>
        /// Called when this component has been attached to an actor
        /// </summary>
        public override void OnAttach()
        {
            // Call base
            base.OnAttach();

            // Initialise
            rigidbodies = new HashSet<RigidBody2D>();
        }

        /// <summary>
        /// Adds a rigid body to this physics world
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public void AddRigidBody(RigidBody2D body)
        {
            rigidbodies.Add(body);
            BroadPhase.AddObject(body);
        }

        /// <summary>
        /// Remove a rigid body from this physics world
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public void RemoveRigidBody(RigidBody2D body)
        {
            rigidbodies.Remove(body);
            BroadPhase.RemoveObject(body);
        }

         /// <summary>
        /// Called when it's time to update the frame
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(UpdateMessage))]
        public void OnUpdate(UpdateMessage msg)
        {
            // Integrate all bodies
            foreach (RigidBody2D body in rigidbodies)
                body.Integrate(Integrator, msg.DeltaTime);

            // Iterate though all potential collision pairs
            Manifold2D manifold;
            foreach (CollisionTestPair pair in BroadPhase.Test())
            {
                // Perform narrow-phase collision test
                if (pair.A.TestCollision(pair.B, out manifold))
                {
                    // Set A and B
                    manifold.A = pair.A;
                    manifold.B = pair.B;

                    // Resolve collision
                    CollisionResolver.ResolveManifold(manifold);
                }
            }

            // Apply all bodies
            foreach (RigidBody2D body in rigidbodies)
                body.Apply();
        }
    }
}
