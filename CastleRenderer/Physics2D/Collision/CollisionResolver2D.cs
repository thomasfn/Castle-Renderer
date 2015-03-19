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
            // Resolve contact-by-contact
            switch (manifold.NumContacts)
            {
                case 0:
                    break;
                case 1:
                    ResolveContact(manifold.A, manifold.B, manifold.Contact1, manifold.Normal, manifold.Penetration, 1.0f);
                    break;
                case 2:
                    ResolveContact(manifold.A, manifold.B, manifold.Contact1, manifold.Normal, manifold.Penetration, 0.5f);
                    ResolveContact(manifold.A, manifold.B, manifold.Contact2, manifold.Normal, manifold.Penetration, 0.5f);
                    break;
                default:
                    throw new InvalidOperationException("Maximum of 2 contacts per manifold");
            }

            // Apply correction
            PositionalCorrection(manifold);
        }

        /// <summary>
        /// Resolves collision at the specified contact point
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="contactpoint"></param>
        /// <param name="normal"></param>
        /// <param name="penetration"></param>
        private void ResolveContact(IPhysicsObject2D a, IPhysicsObject2D b, Vector2 contactpoint, Vector2 normal, float penetration, float weight)
        {
            // Calculate contact point relative to objects
            Vector2 contactrelA = contactpoint - a.Position;
            Vector2 contactrelB = contactpoint - b.Position;

            // Calculate relative velocity
            Vector2 relvel = b.GetVelocityAtPoint(contactrelB) - a.GetVelocityAtPoint(contactrelA);

            // Calculate relative velocity along the collision normal
            float velalongnormal = Vector2.Dot(relvel, normal);

            // If they're moving apart, do not resolve
            if (velalongnormal > 0.0f) return;

            // Calculate restitution
            //float e = Math.Min(a.Material.Restitution, b.Material.Restitution);
            float resA = a.Material.Restitution, resB = b.Material.Restitution;
            float e = (float)Math.Sqrt(resA * resA + resB * resB);

            // Calculate impulse scalar
            float j = -(1.0f + e) * velalongnormal;
            j /= (a.InvMass + b.InvMass);

            // Apply impulse
            Vector2 impulse = j * normal;
            a.ApplyImpulse(-impulse * weight, contactrelA);
            b.ApplyImpulse(impulse * weight, contactrelB);

            // Recalculate relative velocity
            relvel = b.GetVelocityAtPoint(contactrelB) - a.GetVelocityAtPoint(contactrelA);

            // Solve for the tangent vector
            Vector2 tangent = relvel - Vector2.Dot(relvel, normal) * normal;
            tangent.Normalize();
            if (tangent.LengthSquared() > 0.0f)
            {
                // Solve for the magnitude of friction
                float jt = -Vector2.Dot(relvel, tangent);
                float ctermA = Util.Cross(contactpoint - a.Position, tangent);
                float ctermB = Util.Cross(contactpoint - b.Position, tangent);
                jt /= (a.InvMass + b.InvMass + ctermA * ctermA * a.InvInertia + ctermB * ctermB * b.InvInertia);
                //jt /= (a.InvMass + b.InvMass);

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
                a.ApplyImpulse(-frictionimpulse * weight, contactrelA);
                b.ApplyImpulse(frictionimpulse * weight, contactrelB);
            }
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
