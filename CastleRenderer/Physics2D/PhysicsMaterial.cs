using System;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents a physics material
    /// </summary>
    public struct PhysicsMaterial : IEquatable<PhysicsMaterial>
    {
        // The static coefficient of friction
        public float StaticFriction;

        // The dynamic coefficient of friction
        public float DynamicFriction;

        // The coefficient of restitution
        public float Restitution;

        public bool Equals(PhysicsMaterial other)
        {
            return StaticFriction == other.StaticFriction && DynamicFriction == other.DynamicFriction && Restitution == other.Restitution;
        }
    }
}
