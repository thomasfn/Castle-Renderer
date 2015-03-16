using System;

using CastleRenderer.Physics2D.Shapes;

using SlimDX;

namespace CastleRenderer.Physics2D.Collision
{
    /// <summary>
    /// Tests for collisions between a circle and rectangle shape
    /// </summary>
    public class RectangleToCircle : ICollisionTester2D
    {
        /// <summary>
        /// Adds this tester to the manager
        /// </summary>
        public static void Initialise()
        {
            CollisionTester2D.AddCollisionTester<RectangleShape, CircleShape>(new RectangleToCircle());
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
            // Rectangle-to-circle
            RectangleShape arect = a as RectangleShape;
            CircleShape bcircle = b as CircleShape;

            // If the rectangle is rotated, we need to transform the circle into rectangle space
            // It's important to ensure that if this happens, the output normal should be transformed back
            Matrix2x2 rot = Matrix2x2.Rotation(-arot);
            bpos = rot.Transform(bpos - apos) + apos;
            
            // For now, just assume the rectangle is axis aligned
            bool test = TestAxisAligned(arect, apos, bcircle, bpos, out manifold);
            if (!test) return false;

            // Transform the manifold
            rot = Matrix2x2.Rotation(arot);
            if (manifold.NumContacts >= 1)
                manifold.Contact1 = rot.Transform(manifold.Contact1 - apos) + apos;
            if (manifold.NumContacts >= 2)
                manifold.Contact2 = rot.Transform(manifold.Contact2 - apos) + apos;
            manifold.Normal = rot.Transform(manifold.Normal);

            // Collision!
            return true;
        }

        /// <summary>
        /// Tests for collision between the specified shapes when the rectangle is axis aligned
        /// </summary>
        /// <param name="a"></param>
        /// <param name="apos"></param>
        /// <param name="arot"></param>
        /// <param name="b"></param>
        /// <param name="bpos"></param>
        /// <param name="brot"></param>
        /// <param name="manifold"></param>
        /// <returns></returns>
        public bool TestAxisAligned(RectangleShape a, Vector2 apos, CircleShape b, Vector2 bpos, out Manifold2D manifold)
        {
            // Find closest point on the rectangle to the circle center
            Vector2 closest;
            bool inside;
            if (a.ContainsPoint(apos, 0.0f, bpos))
            {
                inside = true;
                closest = a.FindClosestEdgePoint(apos, 0.0f, bpos);
            }
            else
            {
                inside = false;
                closest = a.FindClosestPoint(apos, 0.0f, bpos);
            }

            // Find the distance squared
            Vector2 normal = bpos - closest;
            float dist2 = normal.LengthSquared();

            // Check for intersection
            if (dist2 > b.Radius * b.Radius && !inside)
            {
                // No collision
                manifold = default(Manifold2D);
                return false;
            }

            // Find actual distance
            float dist = (float)Math.Sqrt(dist2);

            // Return collision
            manifold = new Manifold2D { Normal = (normal / dist) * (inside ? -1.0f : 1.0f), Penetration = b.Radius - dist };
            manifold.AddContact(closest);
            return true;
        }
    }
}
