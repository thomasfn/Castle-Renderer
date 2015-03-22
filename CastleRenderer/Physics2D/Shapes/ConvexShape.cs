using System;

using SlimDX;

namespace CastleRenderer.Physics2D.Shapes
{
    /// <summary>
    /// Represents a convex shape
    /// </summary>
    public class ConvexShape : Shape2D
    {
        /// <summary>
        /// Gets or sets the points of this convex shape
        /// </summary>
        public Vector2[] Points { get; set; }

        /// <summary>
        /// Gets the area of this convex shape
        /// </summary>
        public override float Area
        {
            get
            {
                float area = 0.0f;
                for (int i = 2; i < Points.Length; i++)
                {
                    area += Util.AreaOfTriangle(Points[i - 2], Points[i - 1], Points[i]);
                }
                return area;
            }
        }

        /// <summary>
        /// Gets the moments of inertia of this convex shape
        /// </summary>
        public override float Inertia
        {
            get
            {
                return Area * Mass * 0.5f;
            }
        }

        public ConvexShape(Components.ConvexHull hull)
        {
            Points = hull.Points;
        }

        /// <summary>
        /// Finds the closest point on this convex shape to the specified point
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override Vector2 FindClosestPoint(Vector2 mypos, float myrot, Vector2 pt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the closest point on this convex shape's edge to the specified point
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override Vector2 FindClosestEdgePoint(Vector2 mypos, float myrot, Vector2 pt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns if this convex shape contains the specified point or not
        /// </summary>
        /// <param name="mypos"></param>
        /// <param name="myrot"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override bool ContainsPoint(Vector2 mypos, float myrot, Vector2 pt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the support point along the specified direction in object space
        /// </summary>
        /// <param name="normal"></param>
        /// <returns></returns>
        public override Vector2 FindSupportPoint(Vector2 direction)
        {
            float bestproj = float.MinValue;
            Vector2 bestvert = Vector2.Zero;
            for (int i = 0; i < Points.Length; i++)
            {
                Vector2 pt = Points[i];
                float proj = Vector2.Dot(pt, direction);
                if (proj > bestproj)
                {
                    bestproj = proj;
                    bestvert = pt;
                }
            }
            return bestvert;
        }
    }
}
