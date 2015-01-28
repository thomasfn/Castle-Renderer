using System;

using SlimDX;

namespace CastleRenderer.Physics2D.Shapes
{
    /// <summary>
    /// Represents a circle shape
    /// </summary>
    public class CircleShape : Shape2D
    {
        /// <summary>
        /// Gets or sets the radius of this circle shape
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Gets the area of this circle shape
        /// </summary>
        public override float Area
        {
            get
            {
                return (float)Math.PI * Radius * Radius;
            }
        }

        /// <summary>
        /// Tests for collision between this shape and another
        /// </summary>
        /// <param name="mypos"></param>
        /// <param name="myrot"></param>
        /// <param name="other"></param>
        /// <param name="otherpos"></param>
        /// <param name="otherrot"></param>
        /// <param name="manifold"></param>
        /// <returns></returns>
        public override bool TestCollision(Vector2 mypos, float myrot, Shape2D other, Vector2 otherpos, float otherrot, out Manifold2D manifold)
        {
            // Circle-to-circle
            CircleShape othercircle = other as CircleShape;
            if (othercircle != null)
            {
                // Find the distance squared between both objects
                Vector2 normal = otherpos - mypos;
                float dist2 = normal.LengthSquared();

                // Find the distance squared if they were touching
                float r2 = Radius + othercircle.Radius;
                r2 *= r2;

                // Test for collision
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
                    manifold = new Manifold2D { Normal = Vector2.UnitX, Penetration = Radius };
                }
                else
                {
                    manifold = new Manifold2D { Normal = normal / dist, Penetration = r2 - dist }; // NOTE: Should it be r - dist or r2 - dist?
                }

                // Collision occured
                return true;
            }

            // Unhandled shape collision
            throw new NotImplementedException("CircleShape.TestCollision is not implemented for " + other.GetType());
        }
    }
}
