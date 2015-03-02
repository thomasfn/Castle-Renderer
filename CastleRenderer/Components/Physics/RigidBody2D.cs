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
        /// Gets or sets the rotational velocity of this body
        /// </summary>
        public float RotationalVelocity { get; set; }

        /// <summary>
        /// Gets or sets the linear damping of this body
        /// </summary>
        public float LinearDamping { get; set; }

        /// <summary>
        /// Gets or sets the rotational damping of this body
        /// </summary>
        public float RotationalDamping { get; set; }

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
        /// Gets the inertia of this body
        /// </summary>
        public float Inertia { get; private set; }

        /// <summary>
        /// Gets the inverse inertia of this body
        /// </summary>
        public float InvInertia { get; private set; }

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
            if (World != null) World.AddObject(this);
            Mass = Shape.Mass;
            Inertia = Shape.Inertia;
            if (MoveType == BodyMoveType.Static)
            {
                InvMass = 0.0f;
                InvInertia = 0.0f;
            }
            else
            {
                InvMass = 1.0f / Mass;
                InvInertia = 1.0f / Inertia;
            }
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
            if (World != null) World.RemoveObject(this);
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
            BodyIntegrationInfo newinfo = integrator.IntegrateVariable(new BodyIntegrationInfo
            {
                Velocity = Velocity,
                Position = Position,
                RotationalVelocity = RotationalVelocity,
                Rotation = Rotation
            }, timestep, ComputeAcceleration(), ComputeTorque());
            Velocity = newinfo.Velocity;
            Position = newinfo.Position;
            RotationalVelocity = newinfo.RotationalVelocity;
            Rotation = newinfo.Rotation;

            // Dampen velocity
            if (LinearDamping != 0.0f) Velocity *= (float)Math.Pow(1.0f - LinearDamping, timestep);
            if (RotationalDamping != 0.0f) RotationalVelocity *= (float)Math.Pow(1.0f - RotationalDamping, timestep);
        }

        /// <summary>
        /// Computes the current linear acceleration acting on this rigid body
        /// </summary>
        /// <returns></returns>
        private Vector2 ComputeAcceleration()
        {
            return new Vector2(0.0f, -9.81f);
            //return Vector2.Zero;
        }

        /// <summary>
        /// Computes the current rotational acceleration acting on this rigid body
        /// </summary>
        /// <returns></returns>
        private float ComputeTorque()
        {
            return 0.0f;
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
        /// Tests if this object contains the specified point in world space
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public bool ContainsPoint(Vector2 pt)
        {
            return Shape.ContainsPoint(Position, Rotation, pt);
        }

        /// <summary>
        /// Applies an impulse to this physics object from the specified origin (in object space)
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="origin"></param>
        public void ApplyImpulse(Vector2 impulse, Vector2 origin)
        {
            // Check for static
            if (MoveType == BodyMoveType.Static) return;

            // Apply linear velocity change
            Velocity += impulse * InvMass;

            // Apply rotational velocity change
            RotationalVelocity += Util.Cross(origin, impulse) * InvInertia;
        }

        /// <summary>
        /// Applies an impulse to this physics object from the object's center of mass
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="origin"></param>
        public void ApplyImpulse(Vector2 impulse)
        {
            // Check for static
            if (MoveType == BodyMoveType.Static) return;

            // Apply linear velocity change
            Velocity += impulse * InvMass;
        }

        /// <summary>
        /// Transforms the specified world point into the object space of this physics object
        /// </summary>
        /// <param name="worldpoint"></param>
        /// <returns></returns>
        public Vector2 WorldToObject(Vector2 worldpoint)
        {
            Matrix2x2 mat = Matrix2x2.Rotation(-Rotation);
            return mat.Transform(worldpoint - Position);
        }

        /// <summary>
        /// Transforms the specified local point into world space
        /// </summary>
        /// <param name="worldpoint"></param>
        /// <returns></returns>
        public Vector2 ObjectToWorld(Vector2 localpoint)
        {
            Matrix2x2 mat = Matrix2x2.Rotation(Rotation);
            return mat.Transform(localpoint) + Position;
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
            transform.LocalRotation = Quaternion.RotationAxis(Vector3.UnitZ, Util.ClampAngle(Rotation));
            ignoretransform = false;
        }

        /// <summary>
        /// Gets the velocity of this physics object at the specified origin (in relative space)
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public Vector2 GetVelocityAtPoint(Vector2 origin)
        {
            return Velocity + Util.Cross(RotationalVelocity, origin);
        }
    }
}
