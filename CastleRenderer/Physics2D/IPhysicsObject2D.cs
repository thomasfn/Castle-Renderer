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
        /// Gets the mass for this physics object
        /// </summary>
        float Mass { get; }

        /// <summary>
        /// Gets the inverse mass for this physics object
        /// </summary>
        float InvMass { get; }

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
    }
}
