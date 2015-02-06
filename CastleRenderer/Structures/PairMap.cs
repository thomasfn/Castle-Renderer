using System;
using System.Collections.Generic;

namespace CastleRenderer.Structures
{
    /// <summary>
    /// Represents a map of key pairs to values
    /// </summary>
    public class PairMap<TKey, TValue>
    {
        private struct Key : IEquatable<Key>
        {
            public TKey A, B;

            public Key(TKey a, TKey b)
            {
                A = a;
                B = b;
            }

            public bool Equals(Key other)
            {
                return object.Equals(A, other.A) && object.Equals(B, other.B);
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Key)) return false;
                return Equals((Key)obj);
            }

            public override int GetHashCode()
            {
                return A.GetHashCode() ^ B.GetHashCode();
            }
        }

        private IDictionary<Key, TValue> dict;

        /// <summary>
        /// Initialises a new instance of the PairMap class
        /// </summary>
        public PairMap()
        {
            dict = new Dictionary<Key, TValue>();
        }

        /// <summary>
        /// Returns if this map contains the specified key pair
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Contains(TKey a, TKey b)
        {
            return dict.ContainsKey(new Key(a, b));
        }

        /// <summary>
        /// Returns if this map contains the specified value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(TValue value)
        {
            return dict.Values.Contains(value);
        }

        /// <summary>
        /// Adds an item to this map
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        public void Add(TKey a, TKey b, TValue value)
        {
            dict.Add(new Key(a, b), value);
        }

        /// <summary>
        /// Gets the value with the associated key
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey a, TKey b, out TValue value)
        {
            return dict.TryGetValue(new Key(a, b), out value);
        }

        /// <summary>
        /// Removes the element with the specified key
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public bool Remove(TKey a, TKey b)
        {
            return dict.Remove(new Key(a, b));
        }
    }
}
