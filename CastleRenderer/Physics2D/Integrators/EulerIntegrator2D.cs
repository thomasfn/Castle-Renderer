using System;

using SlimDX;

namespace CastleRenderer.Physics2D.Integrators
{
    /// <summary>
    /// Implements an euler integrator
    /// </summary>
    public class EulerIntegrator2D : IIntegrator2D
    {
        /// <summary>
        /// Integrates using a variable timestep
        /// </summary>
        /// <param name="old"></param>
        /// <param name="timestep"></param>
        /// <param name="linearacc"></param>
        /// <param name="rotacc"></param>
        /// <returns></returns>
        public BodyIntegrationInfo IntegrateVariable(BodyIntegrationInfo old, float timestep, Vector2 linearacc, float rotacc)
        {
            return IntegrateFixed(old, timestep, linearacc, rotacc);
        }

        /// <summary>
        /// Integrates using a fixed timestep
        /// </summary>
        /// <param name="old"></param>
        /// <param name="timestep"></param>
        /// <param name="linearacc"></param>
        /// <param name="rotacc"></param>
        /// <returns></returns>
        public BodyIntegrationInfo IntegrateFixed(BodyIntegrationInfo old, float timestep, Vector2 linearacc, float rotacc)
        {
            // Integrate acceleration
            Vector2 newvelocity = old.Velocity + linearacc * timestep;
            float newrotvelocity = old.RotationalVelocity + rotacc * timestep;

            // Integrate velocity
            Vector2 newposition = old.Position + newvelocity * timestep;
            float newrotation = old.Rotation + newrotvelocity * timestep;

            // Return
            return new BodyIntegrationInfo { Position = newposition, Rotation = newrotation, Velocity = newvelocity, RotationalVelocity = newrotvelocity };
        }
    }
}
