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
        /// Gets the bounding box for this physics object in world space
        /// </summary>
        BoundingBox AABB { get; }

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
