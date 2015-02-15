﻿using System;

using SlimDX;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents a constraint between two physics objects
    /// </summary>
    public interface IPhysicsConstraint2D
    {
        /// <summary>
        /// Gets the first object constrained by this constraint
        /// </summary>
        IPhysicsObject2D ObjectA { get; }

        /// <summary>
        /// Gets the position of the first attachment point in model space of ObjectA
        /// </summary>
        Vector2 PositionA { get; }

        /// <summary>
        /// Gets the second object constrained by this constraint
        /// </summary>
        IPhysicsObject2D ObjectB { get; }

        /// <summary>
        /// Gets the position of the second attachment point in model space of ObjectB
        /// </summary>
        Vector2 PositionB { get; }

        /// <summary>
        /// Resolves this physics constraint
        /// </summary>
        void Resolve();
    }
}
