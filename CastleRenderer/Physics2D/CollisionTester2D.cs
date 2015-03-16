using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CastleRenderer.Structures;

using SlimDX;

namespace CastleRenderer.Physics2D
{
    /// <summary>
    /// Represents a collision tester
    /// </summary>
    public interface ICollisionTester2D
    {
        /// <summary>
        /// Tests for collision between the specified shapes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="apos"></param>
        /// <param name="arot"></param>
        /// <param name="b"></param>
        /// <param name="bpos"></param>
        /// <param name="brot"></param>
        /// <param name="manifold"></param>
        /// <returns></returns>
        bool Test(Shape2D a, Vector2 apos, float arot, Shape2D b, Vector2 bpos, float brot, out Manifold2D manifold);
    }

    /// <summary>
    /// Represents a collision tester that tests in the opposite order
    /// </summary>
    public class InvertedCollisionTester2D : ICollisionTester2D
    {
        private ICollisionTester2D source;

        /// <summary>
        /// Initialises a new instance of the InvertedCollisionTester2D class
        /// </summary>
        /// <param name="source"></param>
        public InvertedCollisionTester2D(ICollisionTester2D source)
        {
            this.source = source;
        }

        /// <summary>
        /// Tests for collision between the specified shapes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="apos"></param>
        /// <param name="arot"></param>
        /// <param name="b"></param>
        /// <param name="bpos"></param>
        /// <param name="brot"></param>
        /// <param name="manifold"></param>
        /// <returns></returns>
        public bool Test(Shape2D a, Vector2 apos, float arot, Shape2D b, Vector2 bpos, float brot, out Manifold2D manifold)
        {
            bool test = source.Test(b, bpos, brot, a, apos, arot, out manifold);
            if (!test) return false;
            manifold.Normal = manifold.Normal * -1.0f;
            return true;
        }
    }

    /// <summary>
    /// Manages all collision testers
    /// </summary>
    public static class CollisionTester2D
    {
        // All collision testers
        private static PairMap<Type, ICollisionTester2D> testers;

        static CollisionTester2D()
        {
            // Initialise
            testers = new PairMap<Type, ICollisionTester2D>();

            // Get all testers
            Type basetype = typeof(ICollisionTester2D);
            foreach (var method in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany((a) => a.GetTypes())
                .Where((t) => basetype.IsAssignableFrom(t))
                .SelectMany((t) => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where((m) => m.Name == "Initialise")
                )
            {
                method.Invoke(null, null);
            }
        }

        /// <summary>
        /// Adds a collision tester for the specified shape types
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="tester"></param>
        public static void AddCollisionTester<T1, T2>(ICollisionTester2D tester)
            where T1 : Shape2D
            where T2 : Shape2D
        {
            // Get types
            Type t1 = typeof(T1);
            Type t2 = typeof(T2);

            // Add testers
            testers.Add(t1, t2, tester);
            if (t1 != t2) testers.Add(t2, t1, new InvertedCollisionTester2D(tester));
        }

        /// <summary>
        /// Gets a collision tester for the specified shapes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static ICollisionTester2D GetCollisionTester(Shape2D a, Shape2D b)
        {
            ICollisionTester2D tester;
            if (testers.TryGetValue(a.GetType(), b.GetType(), out tester))
                return tester;
            else
                return null;
        }
    }
}
