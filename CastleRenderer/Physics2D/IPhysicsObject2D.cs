using System;

using SlimDX;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents an object with a physical presence
    /// </summary>
    public interface IPhysicsObject2D
    {
        /// <summary>
        /// Gets or sets the position of this physics object
        /// </summary>
        Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets the rotation of this physics object
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// Gets the bounding box for this physics object in world space
        /// </summary>
        BoundingBox AABB { get; }

        /// <summary>
        /// Gets or sets the linear velocity for this physics object
        /// </summary>
        Vector2 Velocity { get; set; }

        /// <summary>
        /// Gets or sets the rotational velocity for this physics object
        /// </summary>
        float RotationalVelocity { get; set; }

        /// <summary>
        /// Gets the mass for this physics object
        /// </summary>
        float Mass { get; }

        /// <summary>
        /// Gets the inverse mass for this physics object
        /// </summary>
        float InvMass { get; }

        /// <summary>
        /// Gets the inertia for this physics object
        /// </summary>
        float Inertia { get; }

        /// <summary>
        /// Gets the inverse inertia for this physics object
        /// </summary>
        float InvInertia { get; }

        /// <summary>
        /// Gets the physics material of this body
        /// </summary>
        PhysicsMaterial Material { get; }

        /// <summary>
        /// Gets if this object is static
        /// </summary>
        bool Static { get; }

        /// <summary>
        /// Tests for collision between this physics object and another
        /// </summary>
        /// <param name="other"></param>
        /// <param name="manifold"></param>
        /// <returns></returns>
        bool TestCollision(IPhysicsObject2D other, out Manifold2D manifold);

        /// <summary>
        /// Tests if this object contains the specified point in world space
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        bool ContainsPoint(Vector2 pt);

        /// <summary>
        /// Applies an impulse to this physics object from the specified origin (in relative space)
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="origin"></param>
        void ApplyImpulse(Vector2 impulse, Vector2 origin);

        /// <summary>
        /// Applies an impulse to this physics object from the object's center of mass
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="origin"></param>
        void ApplyImpulse(Vector2 impulse);

        /// <summary>
        /// Transforms the specified world point into the object space of this physics object
        /// </summary>
        /// <param name="worldpoint"></param>
        /// <returns></returns>
        Vector2 WorldToObject(Vector2 worldpoint);

        /// <summary>
        /// Transforms the specified local point into world space
        /// </summary>
        /// <param name="worldpoint"></param>
        /// <returns></returns>
        Vector2 ObjectToWorld(Vector2 localpoint);

        /// <summary>
        /// Gets the velocity of this physics object at the specified origin (in relative space)
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        Vector2 GetVelocityAtPoint(Vector2 origin);

        /// <summary>
        /// Integrates this physics object over the specified timestep
        /// </summary>
        /// <param name="integrator"></param>
        /// <param name="timestep"></param>
        void Integrate(IIntegrator2D integrator, float timestep);

        /// <summary>
        /// Updates this physics object after a physics iteration
        /// </summary>
        void Apply();
    }
}
