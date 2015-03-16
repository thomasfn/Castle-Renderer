using System;

using SlimDX;

namespace CastleRenderer.Physics2D.Constraints
{
    /// <summary>
    /// Represents a rope constraint
    /// </summary>
    public class RopeConstraint2D : IBinaryPhysicsConstraint2D
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
        /// Gets the second object constrained by this rope constraint
        /// </summary>
        public IPhysicsObject2D ObjectB { get; private set; }

        /// <summary>
        /// Gets the position of the second attachment point in model space of ObjectB
        /// </summary>
        public Vector2 PositionB { get; private set; }

        /// <summary>
        /// Gets the length of this rope constraint
        /// </summary>
        public float Length { get; private set; }

        /// <summary>
        /// Gets the stiffness of this rope constraint
        /// </summary>
        public float Stiffness { get; private set; }

        /// <summary>
        /// Initialises a new instance of the RopeConstraint2D class
        /// </summary>
        public RopeConstraint2D(IPhysicsObject2D a, Vector2 posA, IPhysicsObject2D b, Vector2 posB, float length, float stiffness)
        {
            // Store attributes
            ObjectA = a;
            PositionA = posA;
            ObjectB = b;
            PositionB = posB;
            Length = length;
            Stiffness = stiffness;
        }

        /// <summary>
        /// Resolves this rope constraint
        /// </summary>
        public void Resolve()
        {
            // Find the connection points in world and relative space
            Vector2 connectA = ObjectA.ObjectToWorld(PositionA);
            Vector2 connectB = ObjectB.ObjectToWorld(PositionB);
            Vector2 connectArel = connectA - ObjectA.Position;
            Vector2 connectBrel = connectB - ObjectB.Position;

            // Find the length
            Vector2 between = connectB - connectA;
            float len2 = between.LengthSquared();

            // If it's less than the rope length, take no action
            if (len2 < Length * Length) return;

            // If it's greater, find the expansion amount
            float len = (float)Math.Sqrt(len2);
            float expansion = len - Length;
            between /= len;

            // Calculate relative velocity
            Vector2 relvel = ObjectB.GetVelocityAtPoint(connectBrel) - ObjectA.GetVelocityAtPoint(connectArel);

            // Calculate relative velocity along the collision normal
            float velalongnormal = Vector2.Dot(relvel, between);

            // Calculate impulse scalar
            float j = (expansion + Math.Max(0.0f, velalongnormal)) * Stiffness;
            j /= (ObjectA.InvMass + ObjectB.InvMass);

            // Apply impulse
            Vector2 impulse = j * between;
            ObjectA.ApplyImpulse(impulse, connectArel);
            ObjectB.ApplyImpulse(-impulse, connectBrel);
        }
    }
}
