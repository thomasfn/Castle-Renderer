using System;

using SlimDX;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents a constraint between two physics objects
    /// </summary>
    public interface IBinaryPhysicsConstraint2D : IUnaryPhysicsConstraint2D
    {
        /// <summary>
        /// Gets the second object constrained by this constraint
        /// </summary>
        IPhysicsObject2D ObjectB { get; }

        /// <summary>
        /// Gets the position of the second attachment point in model space of ObjectB
        /// </summary>
        Vector2 PositionB { get; }
    }
}
