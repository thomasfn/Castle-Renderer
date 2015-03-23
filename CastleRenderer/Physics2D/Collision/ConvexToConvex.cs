using System;

using CastleRenderer.Physics2D.Shapes;

using SlimDX;

namespace CastleRenderer.Physics2D.Collision
{
    /// <summary>
    /// Tests for collisions between a circle and rectangle shape
    /// </summary>
    public class ConvexToConvex : ICollisionTester2D
    {
        

        public ConvexToConvex()
        {
            
        }

        /// <summary>
        /// Adds this tester to the manager
        /// </summary>
        public static void Initialise()
        {
            CollisionTester2D.AddCollisionTester<ConvexShape, ConvexShape>(new ConvexToConvex());
        }

        private struct Projection
        {
            private static readonly Vector2[] vertices = new Vector2[4];


            public float Min, Max;
            public float Origin;

            public static Projection Project(Vector2[] vertices, Vector2 origin, Vector2 projectionaxis)
            {
                // Project all vertices
                Projection result = new Projection
                {
                    Min = float.MaxValue,
                    Max = float.MinValue,
                    Origin = Vector2.Dot(origin, projectionaxis)
                };
                for (int i = 0; i < 4; i++)
                {
                    Vector2 vertex = vertices[i];
                    float projection = Vector2.Dot(vertex, projectionaxis);
                    result.Min = Math.Min(result.Min, projection);
                    result.Max = Math.Max(result.Max, projection);
                }
                return result;
            }

            public bool Intersects(Projection other, out float penetration, out float contact)
            {
                penetration = 0.0f;
                contact = 0.0f;
                if (Min > other.Max) return false;
                if (Max < other.Min) return false;

                // Do we entirely contain the other?
                if (other.Min > Min && other.Max < Max)
                {
                    // Find midpoint of the other
                    contact = Util.Lerp(other.Min, other.Max, 0.5f);
                    penetration = Math.Min(Math.Abs(Min - contact), Math.Abs(Max - contact));
                }
                // Does the other entirely contains us?
                else if (Min > other.Min && Max < other.Max)
                {
                    // Find midpoint of us
                    contact = Util.Lerp(Min, Max, 0.5f);
                    penetration = Math.Min(Math.Abs(other.Min - contact), Math.Abs(other.Max - contact));
                }
                else
                {
                    float pen1 = Math.Abs(Max - other.Min);
                    float pen2 = Math.Abs(Min - other.Max);
                    if (pen1 < pen2)
                    {
                        penetration = pen1;
                        contact = Util.Lerp(Max, other.Min, 0.5f);
                    }
                    else
                    {
                        penetration = pen2;
                        contact = Util.Lerp(Min, other.Max, 0.5f);
                    }
                }
                return true;
            }
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
            // Convex-to-convex
            ConvexShape aconv = a as ConvexShape;
            ConvexShape bconv = b as ConvexShape;

            manifold = default(Manifold2D);
            return false;
        }
    }
}
