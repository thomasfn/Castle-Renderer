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
    public class RigidBody2D : BaseComponent, IPhysicsObject2D
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
        /// Gets or sets the position of this body
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets the rotation of this body
        /// </summary>
        public float Rotation { get; set; }

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
        /// Gets the mass of this body
        /// </summary>
        public float Mass { get; private set; }

        /// <summary>
        /// Gets the inverse mass of this body
        /// </summary>
        public float InvMass { get; private set; }

        /// <summary>
        /// Gets the AABB of this body
        /// </summary>
        public BoundingBox AABB
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets if this body is static
        /// </summary>
        public bool Static { get { return MoveType == BodyMoveType.Static; } }

        private bool ignoretransform;

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

            // Hook transform
            Transform transform = Owner.GetComponent<Transform>();
            transform.OnTransformChange += transform_OnTransformChange;
            transform_OnTransformChange(transform);

            // Initialise
            if (World != null) World.AddRigidBody(this);
            Mass = Shape.Mass;
            if (MoveType == BodyMoveType.Static)
                InvMass = 0.0f;
            else
                InvMass = 1.0f / Mass;
        }

        /// <summary>
        /// Called when this component has been detached from an actor
        /// </summary>
        public override void OnDetach()
        {
            // Call base
            base.OnDetach();

            // Unhook transform
            Transform transform = Owner.GetComponent<Transform>();
            if (transform != null) transform.OnTransformChange -= transform_OnTransformChange;

            // Clean up
            if (World != null) World.RemoveRigidBody(this);
        }

        /// <summary>
        /// Called when the transform has been changed
        /// </summary>
        /// <param name="sender"></param>
        private void transform_OnTransformChange(Transform sender)
        {
            if (ignoretransform) return;
            Position = sender.LocalPosition2D;
            // TODO: Read real rotation
            Rotation = 0.0f;
        }

        /// <summary>
        /// Integrates this rigid body over the specified timestep
        /// </summary>
        /// <param name="integrator"></param>
        /// <param name="timestep"></param>
        public void Integrate(IIntegrator2D integrator, float timestep)
        {
            // Check for static
            if (MoveType == BodyMoveType.Static) return;

            // Integrate
            Vector2 newvel, newpos;
            integrator.IntegrateVariable(Position, Velocity, timestep, ComputeAcceleration(), out newpos, out newvel);
            Velocity = newvel;
            Position = newpos;

            // Dampen velocity
            Velocity *= (float)Math.Pow(1.0f - LinearDamping, timestep);
        }

        /// <summary>
        /// Computes the current acceleration acting on this rigid body
        /// </summary>
        /// <returns></returns>
        private Vector2 ComputeAcceleration()
        {
            return new Vector2(0.0f, -9.81f);
            //return Vector2.Zero;
        }

        /// <summary>
        /// Tests for collision between this physics object and another
        /// </summary>
        /// <param name="other"></param>
        /// <param name="manifold"></param>
        /// <returns></returns>
        public bool TestCollision(IPhysicsObject2D other, out Manifold2D manifold)
        {
            // Test for collision with other rigid body
            RigidBody2D otherbody = other as RigidBody2D;
            if (otherbody != null)
            {
                // Test collision
                ICollisionTester2D tester = CollisionTester2D.GetCollisionTester(Shape, otherbody.Shape);
                return tester.Test(Shape, Position, Rotation, otherbody.Shape, other.Position, other.Rotation, out manifold);
            }

            // Unknown body type
            throw new NotImplementedException("RigidBody2D.TestCollision is not implemented for " + other.GetType());
        }

        /// <summary>
        /// Updates this body after a physics iteration
        /// </summary>
        public void Apply()
        {
            // Update transform
            ignoretransform = true;
            Transform transform = Owner.GetComponent<Transform>();
            transform.LocalPosition2D = Position;
            transform.LocalRotation = Quaternion.RotationAxis(Vector3.UnitZ, Rotation);
            ignoretransform = false;
        }
    }
}
