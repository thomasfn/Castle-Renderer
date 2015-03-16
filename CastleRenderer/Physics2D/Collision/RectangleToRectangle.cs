using System;

using CastleRenderer.Physics2D.Shapes;

using SlimDX;

namespace CastleRenderer.Physics2D.Collision
{
    /// <summary>
    /// Tests for collisions between a circle and rectangle shape
    /// </summary>
    public class RectangleToRectangle : ICollisionTester2D
    {
        private Vector2[] axes;
        private float[] contactpoints;
        private Vector2[] projorigins;

        public RectangleToRectangle()
        {
            axes = new Vector2[4];
            contactpoints = new float[4];
            projorigins = new Vector2[4];
        }

        /// <summary>
        /// Adds this tester to the manager
        /// </summary>
        public static void Initialise()
        {
            CollisionTester2D.AddCollisionTester<RectangleShape, RectangleShape>(new RectangleToRectangle());
        }

        private struct Projection
        {
            private static readonly Vector2[] vertices = new Vector2[4];
            

            public float Min, Max;
            public float Origin;

            public static Projection Project(RectangleShape rectshape, Vector2 origin, Vector2 axis1, Vector2 axis2, Vector2 projectionaxis)
            {
                // Calculate the vertices
                Vector2 size = rectshape.Size;
                vertices[0] = origin - axis1 * size.X * 0.5f - axis2 * size.Y * 0.5f;
                vertices[1] = vertices[0] + axis2 * size.Y;
                vertices[2] = vertices[1] + axis1 * size.X;
                vertices[3] = vertices[0] + axis1 * size.X;

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
            // Rectangle-to-rectangle
            RectangleShape arect = a as RectangleShape;
            RectangleShape brect = b as RectangleShape;

            // Determine axes to test for SAT
            Matrix2x2 amtx = Matrix2x2.Rotation(arot);
            Matrix2x2 bmtx = Matrix2x2.Rotation(brot);
            axes[0] = amtx.Row2;
            axes[1] = amtx.Row1;
            axes[2] = bmtx.Row2;
            axes[3] = bmtx.Row1;

            // Loop each axis
            int minaxis = 0;
            float minpen = float.MaxValue;
            bool flipnormal = false;
            for (int i = 0; i < 4; i++)
            {
                Vector2 axis = axes[i];
                Projection projA = Projection.Project(arect, apos, axes[1], axes[0], axis);
                Projection projB = Projection.Project(brect, bpos, axes[3], axes[2], axis);
                projorigins[i] = new Vector2(projA.Origin, projB.Origin);
                float pen;
                if (!projA.Intersects(projB, out pen, out contactpoints[i]))
                {
                    manifold = default(Manifold2D);
                    return false;
                }
                if (pen < minpen)
                {
                    minpen = pen;
                    minaxis = i;
                    flipnormal = projB.Origin < projA.Origin;
                }
            }

            // Setup manifold
            manifold = new Manifold2D
            {
                Normal = axes[minaxis] * (flipnormal ? -1.0f : 1.0f),
                Penetration = minpen
            };

            // Work out the contact point
            Vector2 cpt;
            if (minaxis == 0 || minaxis == 1)
            {
                cpt = axes[0] * contactpoints[0] + axes[1] * contactpoints[1]; 
            }
            else
            {
                cpt = axes[2] * contactpoints[2] + axes[3] * contactpoints[3];
            }
            manifold.AddContact(cpt);

            // All axes intersected, there's collision
            return true;
        }
    }
}
