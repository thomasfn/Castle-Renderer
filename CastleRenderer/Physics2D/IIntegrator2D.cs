using System;

using SlimDX;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents data to be passed in/out of the integrator
    /// </summary>
    public struct BodyIntegrationInfo
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float RotationalVelocity;
    }

    /// <summary>
    /// Represents an object that can integrate the physics equation
    /// </summary>
    public interface IIntegrator2D
    {
        /// <summary>
        /// Integrates using a variable timestep
        /// </summary>
        /// <param name="old"></param>
        /// <param name="timestep"></param>
        /// <param name="linearacc"></param>
        /// <param name="rotacc"></param>
        /// <returns></returns>
        BodyIntegrationInfo IntegrateVariable(BodyIntegrationInfo old, float timestep, Vector2 acceleration, float torque);

        /// <summary>
        /// Integrates using a fixed timestep
        /// </summary>
        /// <param name="old"></param>
        /// <param name="timestep"></param>
        /// <param name="linearacc"></param>
        /// <param name="rotacc"></param>
        /// <returns></returns>
        BodyIntegrationInfo IntegrateFixed(BodyIntegrationInfo old, float timestep, Vector2 acceleration, float torque);

    }
}
