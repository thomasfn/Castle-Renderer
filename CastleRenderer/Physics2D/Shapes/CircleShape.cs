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

    }
}
