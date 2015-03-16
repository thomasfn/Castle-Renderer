using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Physics2D;
using CastleRenderer.Physics2D.Constraints;

using SlimDX;

namespace CastleRenderer.Components.Physics
{
    /// <summary>
    /// A component that allows the user to interact with a 2D physics simulation
    /// </summary>
    [RequiresComponent(typeof(PhysicsWorld2D))]
    public class DragPlane : BaseComponent
    {
        /// <summary>
        /// Gets or sets the main camera
        /// </summary>
        public Camera MainCamera { get; set; }

        /// <summary>
        /// Gets or sets the stiffness of the constraint
        /// </summary>
        public float Stiffness { get; set; }

        /// <summary>
        /// Gets or sets the stiffness of the constraint at the tangent
        /// </summary>
        public float TangentStiffness { get; set; }

        private bool depressed;
        private PointConstraint2D point;

        /// <summary>
        /// Called when a mouse button has been pressed
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(MousePressMessage))]
        public void OnMousePress(MousePressMessage msg)
        {
            // Is it left button?
            if (msg.Button == MouseButtons.Left)
            {
                depressed = msg.Depressed;
            }
        }

        /// <summary>
        /// Called when a mouse has moved
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(MouseMoveMessage))]
        public void OnMouseMove(MouseMoveMessage msg)
        {
            PhysicsWorld2D world = Owner.GetComponent<PhysicsWorld2D>();
            if (world == null) return;


            if (depressed)
            {
                
                Plane plane = world.WorldPlane;
                Ray ray = MainCamera.GetRay(msg.X, msg.Y);
                float dist;
                if (Ray.Intersects(ray, plane, out dist))
                {
                    Vector3 hitpos = ray.Position + ray.Direction * dist;
                    Vector2 pt = new Vector2(hitpos.X, hitpos.Y);
                    if (point == null)
                    {
                        IPhysicsObject2D[] arr = world.QueryPoint(pt)
                            .Where((obj) => !obj.Static)
                            .ToArray();
                        if (arr.Length > 0)
                        {
                            IPhysicsObject2D obj = arr[0];
                            RigidBody2D body = obj as RigidBody2D;
                            if (body != null)
                            {
                                pt = body.Shape.FindClosestPoint(obj.Position, obj.Rotation, pt);
                            }
                            //point = new PointConstraint2D(obj, pt - obj.Position, Stiffness, true);
                            point = new PointConstraint2D(obj, pt - obj.Position, Stiffness, TangentStiffness, false);
                            world.AddConstraint(point);
                        }
                    }
                    if (point != null)
                    {
                        point.PositionB = pt;
                    }
                }
            }
            else if (!depressed && point != null)
            {
                world.RemoveConstraint(point);
                point = null;
            }
           
        }

    }
}
