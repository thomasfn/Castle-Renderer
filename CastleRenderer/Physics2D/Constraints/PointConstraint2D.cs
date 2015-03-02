using System;

using SlimDX;

namespace CastleRenderer.Physics2D.Constraints
{
    /// <summary>
    /// Represents a "weld to point" constraint
    /// </summary>
    public class PointConstraint2D : IUnaryPhysicsConstraint2D
    {
        /// <summary>
        /// Gets the first object constrained by this rope constraint
        /// </summary>
        public IPhysicsObject2D ObjectA { get; private set; }

        /// <summary>
        /// Gets the position of the first attachment point in model space of ObjectA
        /// </summary>
        public Vector2 PositionA { get; private set; }

        /// <summary>
        /// Gets the position of the constraint in world space
        /// </summary>
        public Vector2 PositionB { get; set; }

        /// <summary>
        /// Initialises a new instance of the PointConstraint2D class
        /// </summary>
        public PointConstraint2D(IPhysicsObject2D a, Vector2 posA)
        {
            // Store attributes
            ObjectA = a;
            PositionA = posA;
            PositionB = a.ObjectToWorld(posA);
        }

        /// <summary>
        /// Resolves this point constraint
        /// </summary>
        public void Resolve()
        {
            
        }
    }
}
