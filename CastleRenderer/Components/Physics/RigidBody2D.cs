using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Physics2D;

using SlimDX;

namespace CastleRenderer.Components.Physics
{
    public enum BodyMoveType { Static, Dynamic }

    /// <summary>
    /// A component that represents a single physically simulated 2D rigid body
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    public class RigidBody2D : BaseComponent
    {
        /// <summary>
        /// Gets or sets the physics world to which this body belongs
        /// </summary>
        public PhysicsWorld2D World { get; set; }

        /// <summary>
        /// Gets or sets the shape of this body
        /// </summary>
        public Shape2D Shape { get; set; }

        /// <summary>
        /// Gets or sets the move type of this body
        /// </summary>
        public BodyMoveType MoveType { get; set; }

        /// <summary>
        /// Gets or sets the current velocity of this body
        /// </summary>
        public Vector2 Velocity { get; set; }

        /// <summary>
        /// Gets or sets the linear damping of this body
        /// </summary>
        public float LinearDamping { get; set; }

        /// <summary>
        /// Gets or sets the physics material of this body
        /// </summary>
        public PhysicsMaterial Material { get; set; }

        /// <summary>
        /// Initialises a new instance of the RigidBody2D class
        /// </summary>
        public RigidBody2D()
        {
            // Default material
            Material = new PhysicsMaterial { StaticFriction = 0.5f, DynamicFriction = 0.5f, Restitution = 0.5f };
        }

        /// <summary>
        /// Called when this component has been attached to an actor
        /// </summary>
        public override void OnAttach()
        {
            // Call base
            base.OnAttach();

            // Initialise
            if (World != null) World.AddRigidBody(this);
        }

        /// <summary>
        /// Called when this component has been detached from an actor
        /// </summary>
        public override void OnDetach()
        {
            // Call base
            base.OnDetach();

            // Clean up
            if (World != null) World.RemoveRigidBody(this);
        }

        /// <summary>
        /// Integrates this rigid body over the specified timestep
        /// </summary>
        /// <param name="integrator"></param>
        /// <param name="timestep"></param>
        public void Integrate(IIntegrator2D integrator, float timestep)
        {
            // Integrate
            Transform transform = Owner.GetComponent<Transform>();
            Vector2 newvel, newpos;
            integrator.IntegrateVariable(transform.LocalPosition2D, Velocity, timestep, ComputeAcceleration(), out newpos, out newvel);
            Velocity = newvel;

            // Fake bounce for now
            if (newpos.Y < 1.5f)
            {
                newpos.Y = 1.5f;
                Velocity = new Vector2(Velocity.X, -Velocity.Y * Material.Restitution);
            }

            // Dampen velocity
            Velocity *= (float)Math.Pow(1.0f - LinearDamping, timestep);

            // Set new position
            transform.LocalPosition2D = newpos;
        }

        /// <summary>
        /// Computes the current acceleration acting on this rigid body
        /// </summary>
        /// <returns></returns>
        private Vector2 ComputeAcceleration()
        {
            return new Vector2(0.0f, -9.81f);
        }

    }
}
