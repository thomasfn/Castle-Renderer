using System;

using SlimDX;

namespace CastleRenderer.Physics2D.Collision
{
    /// <summary>
    /// Represents a collision resolver
    /// </summary>
    public class CollisionResolver2D : ICollisionResolver2D
    {
        /// <summary>
        /// Resolves a collision using the specified manifold
        /// </summary>
        /// <param name="manifold"></param>
        public void ResolveManifold(Manifold2D manifold)
        {
            // Cache objects in locals
            IPhysicsObject2D a = manifold.A;
            IPhysicsObject2D b = manifold.B;

            // Calculate relative velocity
            Vector2 relvel = b.Velocity - a.Velocity;

            // Calculate relative velocity along the collision normal
            float velalongnormal = Vector2.Dot(relvel, manifold.Normal);

            // If they're moving apart, do not resolve
            if (velalongnormal > 0.0f) return;

            // Calculate restitution
            float e = Math.Min(a.Material.Restitution, b.Material.Restitution);

            // Calculate impulse scalar
            float j = -(1.0f + e) * velalongnormal;
            j /= (a.InvMass + b.InvMass);

            // Apply impulse
            Vector2 impulse = j * manifold.Normal;
            manifold.A.Velocity -= a.InvMass * impulse;
            manifold.B.Velocity += b.InvMass * impulse;

            // Recalculate relative velocity
            relvel = b.Velocity - a.Velocity;

            // Solve for the tangent vector
            Vector2 tangent = relvel - Vector2.Dot(relvel, manifold.Normal) * manifold.Normal;
            tangent.Normalize();

            // Solve for the magnitude of friction
            float jt = -Vector2.Dot(relvel, tangent);
            jt /= (a.InvMass + b.InvMass);

            // Approximate mu
            float astaticfric = a.Material.StaticFriction;
            float bstaticfric = b.Material.StaticFriction;
            float mu = (float)Math.Sqrt(astaticfric * astaticfric + bstaticfric * bstaticfric);

            // Clamp magnitude of friction and create impulse vector
            Vector2 frictionimpulse;
            if (Math.Abs(jt) < j * mu)
            {
                // Static friction is good enough
                frictionimpulse = jt * tangent;
            }
            else
            {
                // Recalculate mu for dynamic friction
                float adynfric = a.Material.DynamicFriction;
                float bdynfric = b.Material.DynamicFriction;
                mu = (float)Math.Sqrt(adynfric * adynfric + bdynfric * bdynfric);
                frictionimpulse = -j * tangent * mu;
            }

            // Apply friction impulse
            manifold.A.Velocity -= a.InvMass * frictionimpulse;
            manifold.B.Velocity += b.InvMass * frictionimpulse;

            // Apply correction
            PositionalCorrection(manifold);
        }

        /// <summary>
        /// Applies linear projection positional correction using the specified manifold
        /// </summary>
        /// <param name="manifold"></param>
        private void PositionalCorrection(Manifold2D manifold)
        {
            // Define constants
            const float percent = 0.2f;
            const float slop = 0.01f;

            // Calculate correction
            Vector2 correction = (Math.Max(manifold.Penetration - slop, 0.0f) / (manifold.A.InvMass + manifold.B.InvMass)) * percent * manifold.Normal;
            manifold.A.Position -= manifold.A.InvMass * correction;
            manifold.B.Position += manifold.B.InvMass * correction;
        }
    }
}
