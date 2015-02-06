using System;
using System.Collections.Generic;

using SlimDX;

namespace CastleRenderer.Physics2D.BroadPhases
{
    /// <summary>
    /// Represents a brute-force broadphase implementation
    /// </summary>
    public class BruteForceBroadphase2D : IBroadPhase2D
    {
        // The physics object list
        private IList<IPhysicsObject2D> objects;

        /// <summary>
        /// Initialises a new instance of the BruteForceBroadphase2D class
        /// </summary>
        public BruteForceBroadphase2D()
        {
            // Initialise
            objects = new List<IPhysicsObject2D>();
        }

        /// <summary>
        /// Adds an object to this broadphase
        /// </summary>
        /// <param name="obj"></param>
        public void AddObject(IPhysicsObject2D obj)
        {
            objects.Add(obj);
        }

        /// <summary>
        /// Removes an object from this broadphase
        /// </summary>
        /// <param name="obj"></param>
        public void RemoveObject(IPhysicsObject2D obj)
        {
            // This "swap-remove" is slightly faster (operates in O(n) time) but loses the order of the list
            // Order is only important during iteration so this is OK so long as nothing gets removed during iteration
            int idx = objects.IndexOf(obj);
            if (idx == -1) return;
            int last = objects.Count - 1;
            if (idx < last)
                objects[idx] = objects[last];
            objects.RemoveAt(last);
        }

        /// <summary>
        /// Returns a set of potential collision pairs to test
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CollisionTestPair> Test()
        {
            int cnt = objects.Count;

            // Loop all objects
            for (int i = 0; i < cnt; i++)
                for (int j = i + 1; j < cnt; j++)
                {
                    IPhysicsObject2D objA = objects[i];
                    IPhysicsObject2D objB = objects[j];

                    // If it's not static <-> static, it's a potential collision pair
                    if (!(objA.Static && objB.Static))
                        yield return new CollisionTestPair { A = objA, B = objB };
                }
        }
    }
}
