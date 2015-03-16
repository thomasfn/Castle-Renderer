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
        /// Initialises a new instance of the RectangleShape class
        /// </summary>
        /// <param name="radius"></param>
        public RectangleShape(float width, float height)
        {
            Size = new Vector2(width, height);
        }

        /// <summary>
        /// Initialises a new instance of the RectangleShape class
        /// </summary>
        /// <param name="radius"></param>
        public RectangleShape(Vector2 size)
        {
            Size = size;
        }

        /// <summary>
        /// Finds the closest point on this rectangle shape to the specified point
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override Vector2 FindClosestPoint(Vector2 mypos, float myrot, Vector2 pt)
        {
            // Handle non axis-aligned case
            Matrix2x2 rot = Matrix2x2.Identity;
            //if (myrot != 0.0f)
            {
                // Transform the point and rect to rect space
                rot = Matrix2x2.Rotation(-myrot);
                pt = rot.Transform(pt - mypos);
            }

            // Get extents
            Vector2 halfsize = Size * 0.5f;
            Vector2 min = halfsize * -1.0f;
            Vector2 max = halfsize;

            // Clamp to edge
            Vector2 closest = pt;
            closest.X = closest.X.Clamp(min.X, max.X);
            closest.Y = closest.Y.Clamp(min.Y, max.Y);

            // Handle non axis-aligned case
            //if (myrot != 0.0f)
            {
                // Transform back to world space
                rot.Invert();
                closest = rot.Transform(closest) + mypos;
            }

            // Return
            return closest;
        }

        /// <summary>
        /// Finds the closest point on this rectangle shape's edge to the specified point
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override Vector2 FindClosestEdgePoint(Vector2 mypos, float myrot, Vector2 pt)
        {
            // Transform the point to rect space
            var rot = Matrix2x2.Rotation(-myrot);
            pt = rot.Transform(pt - mypos);

            // Get extents
            Vector2 halfsize = Size * 0.5f;
            Vector2 min = halfsize * -1.0f;
            Vector2 max = halfsize;

            // Clamp to edge
            Vector2 closest = pt;
            closest.X = closest.X.Clamp(min.X, max.X);
            closest.Y = closest.Y.Clamp(min.Y, max.Y);

            // Clip to edge
            Vector2 centeroffset = pt;
            if (Math.Abs(centeroffset.X) > Math.Abs(centeroffset.Y))
            {
                if (Math.Abs(pt.X - max.X) < Math.Abs(pt.X - min.X))
                    pt.X = max.X;
                else
                    pt.X = min.X;
            }
            else
            {
                if (Math.Abs(pt.Y - max.Y) < Math.Abs(pt.Y - min.Y))
                    pt.Y = max.Y;
                else
                    pt.Y = min.Y;
            }

            // Transform back to world space
            rot.Invert();
            closest = rot.Transform(closest) + mypos;            

            // Return
            return pt;
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
            // Transform point relative to the rectangle
            Vector2 relpt = pt - mypos;
            //if (myrot != 0.0f)
            {
                var rot = Matrix2x2.Rotation(-myrot);
                relpt = rot.Transform(relpt);
            }

            // Check vs size
            return Math.Abs(relpt.X) < Size.X * 0.5f && Math.Abs(relpt.Y) < Size.Y * 0.5f;
        }
    }
}
