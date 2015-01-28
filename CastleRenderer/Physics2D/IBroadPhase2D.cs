using System;
using System.Collections.Generic;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents a pair of objects to test for collision in narrowphase
    /// </summary>
    public struct CollisionTestPair
    {
        public IPhysicsObject2D A, B;
    }

    /// <summary>
    /// Represents a broad collision detection phase
    /// </summary>
    public interface IBroadPhase2D
    {
        /// <summary>
        /// Adds an object to this broadphase
        /// </summary>
        /// <param name="obj"></param>
        void AddObject(IPhysicsObject2D obj);

        /// <summary>
        /// Removes an object from this broadphase
        /// </summary>
        /// <param name="obj"></param>
        void RemoveObject(IPhysicsObject2D obj);

        /// <summary>
        /// Returns a set of potential collision pairs to test
        /// </summary>
        /// <returns></returns>
        IEnumerable<CollisionTestPair> Test();
    }
}
