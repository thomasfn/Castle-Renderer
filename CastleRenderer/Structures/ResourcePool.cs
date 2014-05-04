using System;
using System.Collections.Generic;

namespace CastleRenderer.Structures
{
    /// <summary>
    /// Represents a pool of reusable objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResourcePool<T> where T : class
    {
        private HashSet<T> pool; // All objects that can be reused

        public ResourcePool()
        {
            // Initialise
            pool = new HashSet<T>();
        }

        /// <summary>
        /// Requests an object from this pool
        /// </summary>
        /// <returns></returns>
        public T Request()
        {
            // Is there an object in the pool?
            if (pool.Count > 0)
            {
                // Grab the first object we can, remove and return
                var it = pool.GetEnumerator();
                it.MoveNext();
                var item = it.Current;
                pool.Remove(item);
                return item;
            }

            // Create a new one
            return Allocate();
        }

        /// <summary>
        /// Releases an object back into this pool
        /// </summary>
        /// <param name="item"></param>
        public void Recycle(T item)
        {
            pool.Add(item);
        }

        /// <summary>
        /// Creates a new instance of the object type
        /// </summary>
        /// <returns></returns>
        private T Allocate()
        {
            return Activator.CreateInstance<T>();
        }

    }
}
