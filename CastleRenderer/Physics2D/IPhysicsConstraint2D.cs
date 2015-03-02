using System;

using SlimDX;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents a generic constraint
    /// </summary>
    public interface IPhysicsConstraint2D
    {
        /// <summary>
        /// Resolves this physics constraint
        /// </summary>
        void Resolve();
    }
}
