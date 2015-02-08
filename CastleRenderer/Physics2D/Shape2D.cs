using System;

using SlimDX;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents a generic shape
    /// </summary>
    public abstract class Shape2D
    {
        /// <summary>
        /// Gets or sets the density of this shape
        /// </summary>
        public float Density { get; set; }

        /// <summary>
        /// Gets the area of this shape
        /// </summary>
        public abstract float Area { get; }

        /// <summary>
        /// Gets the mass of this shape
        /// </summary>
        public virtual float Mass
        {
            get
            {
                return Density * Area;
            }
        }

        /// <summary>
        /// Gets the inertia of this shape
        /// </summary>
        public virtual float Inertia
        {
            get
            {
                return Mass;
            }
        }

        /// <summary>
        /// Initialises a new instance of the Shape2D class
        /// </summary>
        protected Shape2D()
        {
            // Defaults
            Density = 1.0f;
        }

        /// <summary>
        /// Finds the closest point on this shape to the specified point
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public abstract Vector2 FindClosestPoint(Vector2 mypos, float myrot, Vector2 pt);

        /// <summary>
        /// Finds the closest point on this shape's edge to the specified point
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public abstract Vector2 FindClosestEdgePoint(Vector2 mypos, float myrot, Vector2 pt);

        /// <summary>
        /// Returns if this shape contains the specified point or not
        /// </summary>
        /// <param name="mypos"></param>
        /// <param name="myrot"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public abstract bool ContainsPoint(Vector2 mypos, float myrot, Vector2 pt);
    }
}
