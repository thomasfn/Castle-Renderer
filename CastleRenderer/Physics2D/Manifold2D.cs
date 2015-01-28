using System;

using SlimDX;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents data about a collision between two objects
    /// </summary>
    public struct Manifold2D
    {
        public IPhysicsObject2D A, B;
        public float Penetration;
        public Vector2 Normal;
    }
}
