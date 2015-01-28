using System;

using SlimDX;

namespace CastleRenderer.Physics2D.Shapes
{
    /// <summary>
    /// Represents a rectangle shape
    /// </summary>
    public class RectangleShape : Shape2D
    {
        /// <summary>
        /// Gets or sets the size of this rectangle shape
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// Gets the area of this circle shape
        /// </summary>
        public override float Area
        {
            get
            {
                return Size.X * Size.Y;
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
            // Unhandled shape collision
            throw new NotImplementedException("RectangleShape.TestCollision is not implemented for " + other.GetType());
        }
    }
}
