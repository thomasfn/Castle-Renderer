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
        /// Gets the moment of inertia of this circle shape
        /// </summary>
        public override float Inertia
        {
            get
            {
                return Area * Radius * Radius * 0.25f * Mass;
            }
        }

        /// <summary>
        /// Initialises a new instance of the CircleShape class
        /// </summary>
        /// <param name="radius"></param>
        public CircleShape(float radius, float density)
        {
            Radius = radius;
        }

        /// <summary>
        /// Finds the closest point on this circle shape to the specified point
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override Vector2 FindClosestPoint(Vector2 mypos, float myrot, Vector2 pt)
        {
            // Get distance between
            Vector2 between = pt - mypos;
            float dist2 = between.LengthSquared();

            // If it's less than radius, the point is inside us
            if (dist2 <= Radius * Radius) return pt;

            // Find the normalisation ratio
            float dist = (float)Math.Sqrt(dist2);
            float ratio = Radius / dist;

            // Return point
            return mypos + between * ratio;
        }

        /// <summary>
        /// Finds the closest point on this circle shape to the specified point
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override Vector2 FindClosestEdgePoint(Vector2 mypos, float myrot, Vector2 pt)
        {
            // Get distance between
            Vector2 between = pt - mypos;
            float dist2 = between.LengthSquared();

            // If it's 0, there is no closest
            if (dist2 == 0.0f) return pt;

            // Find the normalisation ratio
            float dist = (float)Math.Sqrt(dist2);
            float ratio = Radius / dist;

            // Return point
            return mypos + between * ratio;
        }

        /// <summary>
        /// Returns if this shape contains the specified point or not
        /// </summary>
        /// <param name="mypos"></param>
        /// <param name="myrot"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override bool ContainsPoint(Vector2 mypos, float myrot, Vector2 pt)
        {
            // Get distance squared between
            Vector2 between = pt - mypos;
            float dist2 = between.LengthSquared();

            // If it's less than radius squared, the point is inside us
            return dist2 <= Radius * Radius;
        }

        /// <summary>
        /// Finds the support point along the specified direction in object space
        /// </summary>
        /// <param name="normal"></param>
        /// <returns></returns>
        public override Vector2 FindSupportPoint(Vector2 direction)
        {
            // Simply change the length to radius
            direction.Normalize();
            return direction * Radius;
        }
    }
}
