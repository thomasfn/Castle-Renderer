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

        private Vector2[] vertices1, vertices2;

        public RectangleToRectangle()
        {
            axes = new Vector2[4];
            contactpoints = new float[4];
            vertices1 = new Vector2[4];
            vertices2 = new Vector2[4];
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

            // Calculate the vertices
            {
                Vector2 size = arect.Size;
                vertices1[0] = apos - axes[1] * size.X * 0.5f - axes[0] * size.Y * 0.5f;
                vertices1[1] = vertices1[0] + axes[0] * size.Y;
                vertices1[2] = vertices1[1] + axes[1] * size.X;
                vertices1[3] = vertices1[0] + axes[1] * size.X;
            }
            {
                Vector2 size = brect.Size;
                vertices2[0] = bpos - axes[3] * size.X * 0.5f - axes[2] * size.Y * 0.5f;
                vertices2[1] = vertices2[0] + axes[2] * size.Y;
                vertices2[2] = vertices2[1] + axes[3] * size.X;
                vertices2[3] = vertices2[0] + axes[3] * size.X;
            }

            // Loop each axis
            int minaxis = 0;
            float minpen = float.MaxValue;
            bool flipnormal = false;
            for (int i = 0; i < 4; i++)
            {
                Vector2 axis = axes[i];
                Projection projA = Projection.Project(vertices1, apos, axis);
                Projection projB = Projection.Project(vertices2, bpos, axis);
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

            if (float.IsNaN(manifold.Normal.X) || float.IsInfinity(manifold.Normal.X) || float.IsNaN(manifold.Normal.Y) || float.IsInfinity(manifold.Normal.Y) || float.IsNaN(manifold.Penetration) || float.IsInfinity(manifold.Penetration))
            {
                throw new Exception();
            }

            // Work out the contact point
            /*Vector2 cpt;
            if (minaxis == 0 || minaxis == 1)
            {
                cpt = axes[0] * contactpoints[0] + axes[1] * contactpoints[1]; 
            }
            else
            {
                cpt = axes[2] * contactpoints[2] + axes[3] * contactpoints[3];
            }
            manifold.AddContact(cpt);*/

            // Work out the contact point round 2
            Vector2 sumpt = Vector2.Zero;
            int cnt = 0;
            Vector2 halfsizeB = brect.Size * 0.5f;
            Vector2 halfsizeA = arect.Size * 0.5f;
            amtx = Matrix2x2.Rotation(-arot);
            Vector2 prev = Vector2.Zero, cur = Vector2.Zero;
            for (int i = 0; i < 4; i++)
            {
                Vector2 pt = vertices1[i];
                Vector2 tpt = bmtx.Transform(pt - bpos);
                if (tpt.X >= -halfsizeB.X && tpt.X <= halfsizeB.X && tpt.Y >= -halfsizeB.Y && tpt.Y <= halfsizeB.Y)
                {
                    sumpt += pt;
                    prev = cur;
                    cur = pt;
                    cnt++;
                }

                pt = vertices2[i];
                tpt = amtx.Transform(pt - apos);
                if (tpt.X >= -halfsizeA.X && tpt.X <= halfsizeA.X && tpt.Y >= -halfsizeA.Y && tpt.Y <= halfsizeA.Y)
                {
                    sumpt += pt;
                    prev = cur;
                    cur = pt;
                    cnt++;
                }
            }
            if (cnt == 0) return false;
            sumpt /= cnt;
            sumpt += manifold.Normal * manifold.Penetration * 0.5f;
            if (cnt == 2)
            {
                manifold.AddContact(prev);
                manifold.AddContact(cur);
            }
            else
                manifold.AddContact(sumpt);

            // NOTE: This contact point is WRONG.
            // It's not far off, but it always assumes line<->line contact and produces an average point in the center.
            // This results in inaccurate impulse resolution during a point<->line contact.
            // How do we detect/compute point<->line?

            // All axes intersected, there's collision
            return true;
        }
    }
}
