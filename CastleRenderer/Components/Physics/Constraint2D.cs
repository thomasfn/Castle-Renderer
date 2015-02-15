using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Physics2D;

using SlimDX;

namespace CastleRenderer.Components.Physics
{
    /// <summary>
    /// A component that represents a constraint between two physics objects
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    public class Constraint2D : BaseComponent
    {
        /// <summary>
        /// Gets or sets the physics world to which this constraint belongs
        /// </summary>
        public PhysicsWorld2D World { get; set; }

        /// <summary>
        /// Gets or sets the constraint object for this constraint
        /// </summary>
        public IPhysicsConstraint2D Constraint { get; set; }

        /// <summary>
        /// Called when this component has been attached to an actor
        /// </summary>
        public override void OnAttach()
        {
            // Call base
            base.OnAttach();

            // Add to world
            if (World != null && Constraint != null)
                World.AddConstraint(Constraint);
        }

        /// <summary>
        /// Called when this component has been detached from an actor
        /// </summary>
        public override void OnDetach()
        {
            // Call base
            base.OnDetach();

            // Remove from world
            if (World != null && Constraint != null)
                World.RemoveConstraint(Constraint);
        }
    }
}
