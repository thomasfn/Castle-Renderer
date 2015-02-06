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
            // Calculate relative velocity
            Vector2 relvel = manifold.B.Velocity - manifold.A.Velocity;

            // Calculate relative velocity along the collision normal
            float velalongnormal = Vector2.Dot(relvel, manifold.Normal);

            // If they're moving apart, do not resolve
            if (velalongnormal > 0.0f) return;

            // Calculate restitution
            float e = Math.Min(manifold.A.Material.Restitution, manifold.B.Material.Restitution);

            // Calculate impulse scalar
            float j = -(1.0f + e) * velalongnormal;
            j /= manifold.A.InvMass + manifold.B.InvMass;

            // Apply impulse
            Vector2 impulse = j * manifold.Normal;
            manifold.A.Velocity -= manifold.A.InvMass * impulse;
            manifold.B.Velocity += manifold.B.InvMass * impulse;

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
