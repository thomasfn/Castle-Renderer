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
        /// Gets the object constrained by this point constraint
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
        /// Gets the stiffness of this point constraint
        /// </summary>
        public float Stiffness { get; private set; }

        /// <summary>
        /// Gets the stiffness of this point constraint at the tangent
        /// </summary>
        public float TangentStiffness { get; private set; }

        /// <summary>
        /// Gets if rotation of the object should be restricted
        /// </summary>
        public bool RestrictRotation { get; private set; }

        /// <summary>
        /// Initialises a new instance of the PointConstraint2D class
        /// </summary>
        public PointConstraint2D(IPhysicsObject2D a, Vector2 posA, float stiffness, float tanstiffness, bool norot)
        {
            // Store attributes
            ObjectA = a;
            PositionA = posA;
            PositionB = a.ObjectToWorld(posA);
            Stiffness = stiffness;
            RestrictRotation = norot;
            TangentStiffness = tanstiffness;
        }

        /// <summary>
        /// Resolves this point constraint
        /// </summary>
        public void Resolve()
        {
            // Restrict rotation
            if (RestrictRotation)
            {
                ObjectA.RotationalVelocity = 0.0f;
            }

            // Find the connection points in world and relative space
            Vector2 connectA = ObjectA.ObjectToWorld(PositionA);
            Vector2 connectArel = connectA - ObjectA.Position;

            // Find the length
            Vector2 between = PositionB - connectA;
            float len2 = between.LengthSquared();

            // Find the expansion amount
            float len = (float)Math.Sqrt(len2);
            between /= len;

            // Calculate relative velocity
            Vector2 relvel = ObjectA.GetVelocityAtPoint(connectArel) * -1.0f;

            // Calculate relative velocity along the collision normal
            float velalongnormal = Vector2.Dot(relvel, between);

            // Calculate impulse scalar
            float j = (len + Math.Max(0.0f, velalongnormal)) * Stiffness;
            j /= ObjectA.InvMass;

            // Apply impulse
            Vector2 impulse = j * between;
            ObjectA.ApplyImpulse(impulse, connectArel);

            // Recalculate relative velocity
            relvel = ObjectA.GetVelocityAtPoint(connectArel) * -1.0f;

            // Solve for the tangent vector
            Vector2 tangent = relvel - Vector2.Dot(relvel, between) * between;
            tangent.Normalize();
            if (tangent.LengthSquared() > 0.0f)
            {
                float jt = Vector2.Dot(relvel, tangent) * TangentStiffness;
                jt /= ObjectA.InvMass;
                ObjectA.ApplyImpulse(jt * tangent, connectArel);
            }
        }
    }
}
