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

        /// <summary>
        /// Initialises a new instance of the PhysicsMaterial struct
        /// </summary>
        /// <param name="staticfric"></param>
        /// <param name="dynfric"></param>
        /// <param name="rest"></param>
        public PhysicsMaterial(float staticfric, float dynfric, float rest)
        {
            StaticFriction = staticfric;
            DynamicFriction = dynfric;
            Restitution = rest;
        }

        public bool Equals(PhysicsMaterial other)
        {
            return StaticFriction == other.StaticFriction && DynamicFriction == other.DynamicFriction && Restitution == other.Restitution;
        }
    }
}
