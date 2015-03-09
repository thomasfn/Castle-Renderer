using System;

using SlimDX;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents a constraint between two physics objects
    /// </summary>
    public interface IUnaryPhysicsConstraint2D : IPhysicsConstraint2D
    {
        /// <summary>
        /// Gets the first object constrained by this constraint
        /// </summary>
        IPhysicsObject2D ObjectA { get; }

        /// <summary>
        /// Gets the position of the first attachment point in model space of ObjectA
        /// </summary>
        Vector2 PositionA { get; }
    }
}
