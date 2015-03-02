using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Physics2D;

using SlimDX;

namespace CastleRenderer.Components.Physics
{
    /// <summary>
    /// A component that represents a system of physically simulated 2D rigid bodies
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    public class PhysicsWorld2D : BaseComponent
    {
        // All physics objects
        private HashSet<IPhysicsObject2D> objects;

        // All constraints
        private HashSet<IPhysicsConstraint2D> constraints;

        // The current collision set
        private List<Manifold2D> collisionset;

        /// <summary>
        /// Gets or sets the integrator to use
        /// </summary>
        public IIntegrator2D Integrator { get; set; }

        /// <summary>
        /// Gets or sets the broadphase to use
        /// </summary>
        public IBroadPhase2D BroadPhase { get; set; }

        /// <summary>
        /// Gets or sets the collision resolver to use
        /// </summary>
        public ICollisionResolver2D CollisionResolver { get; set; }

        /// <summary>
        /// Gets or sets the tick rate to use
        /// </summary>
        public float TickRate { get; set; }

        /// <summary>
        /// Gets or sets the number of iterations per frame
        /// </summary>
        public int IterationCount { get; set; }

        private float accum;

        /// <summary>
        /// Called when this component has been attached to an actor
        /// </summary>
        public override void OnAttach()
        {
            // Call base
            base.OnAttach();

            // Initialise
            objects = new HashSet<IPhysicsObject2D>();
            constraints = new HashSet<IPhysicsConstraint2D>();
            collisionset = new List<Manifold2D>();
        }

        /// <summary>
        /// Adds a physics object to this world
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public void AddObject(IPhysicsObject2D physobj)
        {
            objects.Add(physobj);
            BroadPhase.AddObject(physobj);
        }

        /// <summary>
        /// Remove a physics object from this world
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public void RemoveObject(IPhysicsObject2D physobj)
        {
            objects.Remove(physobj);
            BroadPhase.RemoveObject(physobj);
        }

        /// <summary>
        /// Adds a constraint to this world
        /// </summary>
        /// <param name="constraint"></param>
        public void AddConstraint(IPhysicsConstraint2D constraint)
        {
            constraints.Add(constraint);
        }

        /// <summary>
        /// Removes a constraint from this world
        /// </summary>
        /// <param name="constraint"></param>
        public void RemoveConstraint(IPhysicsConstraint2D constraint)
        {
            constraints.Remove(constraint);
        }

        /// <summary>
        /// Called when it's time to update the frame
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(UpdateMessage))]
        public void OnUpdate(UpdateMessage msg)
        {
            // Work out how many ticks to run
            accum += msg.DeltaTime;
            float ticktime = 1.0f / TickRate;
            while (accum > ticktime)
            {
                // Run a tick
                accum -= ticktime;
                PerformTick();
            }
        }

        /// <summary>
        /// Performs a single physics tick
        /// </summary>
        private void PerformTick()
        {
            // Integrate all objects
            foreach (IPhysicsObject2D physobj in objects)
                physobj.Integrate(Integrator, 1.0f / TickRate);

            // Clear collision set
            collisionset.Clear();

            // Iterate though all potential collision pairs
            Manifold2D manifold;
            foreach (CollisionTestPair pair in BroadPhase.Test())
            {
                // Perform narrow-phase collision test
                if (pair.A.TestCollision(pair.B, out manifold))
                {
                    // Set A and B
                    manifold.A = pair.A;
                    manifold.B = pair.B;

                    // Add to collision set
                    collisionset.Add(manifold);
                }
            }

            // Perform iterations
            for (int i = 0; i < IterationCount; i++)
            {
                PerformIteration();
            }

            // Apply all objects
            foreach (IPhysicsObject2D physobj in objects)
                physobj.Apply();
        }

        /// <summary>
        /// Performs a solving iteration
        /// </summary>
        private void PerformIteration()
        {
            // Iterate though all collisions
            foreach (Manifold2D manifold in collisionset)
            {
                // Resolve collision
                CollisionResolver.ResolveManifold(manifold);
            }

            // Solve constraints
            foreach (IPhysicsConstraint2D constraint in constraints)
                constraint.Resolve();
        }
    }
}
