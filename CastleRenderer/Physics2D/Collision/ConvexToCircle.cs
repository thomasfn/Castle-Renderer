using System;

using CastleRenderer.Physics2D.Shapes;

using SlimDX;

namespace CastleRenderer.Physics2D.Collision
{
    /// <summary>
    /// Tests for collisions between a convex and circle shape
    /// </summary>
    public class ConvexToCircle : ICollisionTester2D
    {
        /// <summary>
        /// Adds this tester to the manager
        /// </summary>
        public static void Initialise()
        {
            CollisionTester2D.AddCollisionTester<ConvexShape, CircleShape>(new ConvexToCircle());
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
            // Convex-to-circle
            ConvexShape aconv = a as ConvexShape;
            CircleShape bcircle = b as CircleShape;

            // Transform the circle into local space of the convex shape
            Matrix2x2 rot = Matrix2x2.Rotation(-arot);
            bpos = rot.Transform(bpos - apos);

            // Test in local space
            bool test = TestLocal(aconv, bcircle, bpos, out manifold);
            if (!test) return false;

            // Transform the manifold
            rot = Matrix2x2.Rotation(arot);
            if (manifold.NumContacts >= 1)
                manifold.Contact1 = rot.Transform(manifold.Contact1) + apos;
            if (manifold.NumContacts >= 2)
                manifold.Contact2 = rot.Transform(manifold.Contact2) + apos;
            manifold.Normal = rot.Transform(manifold.Normal);

            // Collision!
            return true;
        }

        /// <summary>
        /// Tests for collision between the specified shapes when the convex has zero position and rotation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="apos"></param>
        /// <param name="arot"></param>
        /// <param name="b"></param>
        /// <param name="bpos"></param>
        /// <param name="brot"></param>
        /// <param name="manifold"></param>
        /// <returns></returns>
        public bool TestLocal(ConvexShape a, CircleShape b, Vector2 bpos, out Manifold2D manifold)
        {
            // bpos is essentially a vector from a to b
            // If we find the support point, we have a vertex to test
            Vector2 supportpoint = a.FindSupportPoint(bpos);

            // Find the distance from the circle to support point
            Vector2 between = bpos - supportpoint;
            float len2 = between.LengthSquared();

            // Is it inside circle?
            if (len2 > b.Radius * b.Radius)
            {
                // No collision
                manifold = default(Manifold2D);
                return false;
            }

            manifold = default(Manifold2D);
            return false;
        }
    }
}
