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
        /// <param name="oldposition"></param>
        /// <param name="oldvelocity"></param>
        /// <param name="timestep"></param>
        /// <param name="acceleration"></param>
        /// <param name="newposition"></param>
        /// <param name="newvelocity"></param>
        public void IntegrateVariable(Vector2 oldposition, Vector2 oldvelocity, float timestep, Vector2 acceleration, out Vector2 newposition, out Vector2 newvelocity)
        {
            IntegrateFixed(oldposition, oldvelocity, timestep, acceleration, out newposition, out newvelocity);
        }

        /// <summary>
        /// Integrates using a fixed timestep
        /// </summary>
        /// <param name="oldposition"></param>
        /// <param name="oldvelocity"></param>
        /// <param name="timestep"></param>
        /// <param name="acceleration"></param>
        /// <param name="newposition"></param>
        /// <param name="newvelocity"></param>
        public void IntegrateFixed(Vector2 oldposition, Vector2 oldvelocity, float timestep, Vector2 acceleration, out Vector2 newposition, out Vector2 newvelocity)
        {
            // Compute new velocity
            newvelocity = oldvelocity + acceleration * timestep;

            // Compute new position
            newposition = oldposition + newvelocity * timestep;
        }
    }
}
