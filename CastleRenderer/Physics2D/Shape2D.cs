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
        public float Mass
        {
            get
            {
                return Density * Area;
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
    }
}
