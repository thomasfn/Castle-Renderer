using System;

using SlimDX;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents an object that can integrate the physics equation
    /// </summary>
    public interface IIntegrator2D
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
        void IntegrateVariable(Vector2 oldposition, Vector2 oldvelocity, float timestep, Vector2 acceleration, out Vector2 newposition, out Vector2 newvelocity);

        /// <summary>
        /// Integrates using a fixed timestep
        /// </summary>
        /// <param name="oldposition"></param>
        /// <param name="oldvelocity"></param>
        /// <param name="timestep"></param>
        /// <param name="acceleration"></param>
        /// <param name="newposition"></param>
        /// <param name="newvelocity"></param>
        void IntegrateFixed(Vector2 oldposition, Vector2 oldvelocity, float timestep, Vector2 acceleration, out Vector2 newposition, out Vector2 newvelocity);

    }
}
