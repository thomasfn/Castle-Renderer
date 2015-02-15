using System;

using CastleRenderer.Physics2D.Shapes;

using SlimDX;

namespace CastleRenderer.Physics2D.Collision
{
    /// <summary>
    /// Tests for collisions between two circle shapes
    /// </summary>
    public class CircleToCircle : ICollisionTester2D
    {
        /// <summary>
        /// Adds this tester to the manager
        /// </summary>
        public static void Initialise()
        {
            CollisionTester2D.AddCollisionTester<CircleShape, CircleShape>(new CircleToCircle());
        }

        /// <summary>
        /// Tests for collision between the specified shapes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="apos"></param>
        /// <param name="arot"></param>
        /// <param name="b"></param>
        /// <param name="bpos"></param>
        /// <param name="brot"></param>
        /// <param name="manifold"></param>
        /// <returns></returns>
        public bool Test(Shape2D a, Vector2 apos, float arot, Shape2D b, Vector2 bpos, float brot, out Manifold2D manifold)
        {
            // Circle-to-circle
            CircleShape acircle = a as CircleShape;
            CircleShape bcircle = b as CircleShape;

            // Find the distance squared between both objects
            Vector2 normal = bpos - apos;
            float dist2 = normal.LengthSquared();

            // Find the distance squared if they were touching
            float r = acircle.Radius + bcircle.Radius;
            float r2 = r * r;

            // Check for intersection
            if (dist2 > r2)
            {
                // No collision
                manifold = default(Manifold2D);
                return false;
            }

            // Find actual distance
            float dist = (float)Math.Sqrt(dist2);

            // Are they at the same position?
            if (dist == 0.0f)
            {
                manifold = new Manifold2D { Normal = Vector2.UnitX, Penetration = acircle.Radius };
                manifold.AddContact(apos);
            }
            else
            {
                normal /= dist;
                manifold = new Manifold2D { Normal = normal, Penetration = r - dist }; // NOTE: Should it be r - dist or r2 - dist?
                manifold.AddContact(Vector2.Lerp(apos + normal * acircle.Radius, bpos - normal * bcircle.Radius, 0.5f));
            }

            // Collision occured
            return true;
        }
    }
}
