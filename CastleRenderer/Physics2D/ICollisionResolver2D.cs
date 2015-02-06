using System;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents a collision resolver
    /// </summary>
    public interface ICollisionResolver2D
    {
        /// <summary>
        /// Resolves a collision using the specified manifold
        /// </summary>
        /// <param name="manifold"></param>
        void ResolveManifold(Manifold2D manifold);
    }
}
