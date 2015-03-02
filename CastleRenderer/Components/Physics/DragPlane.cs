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
                    if (point == null)
                    {
                        Vector2 pt = new Vector2(hitpos.X, hitpos.Y);
                        IPhysicsObject2D obj = world.QueryPoint(pt).SingleOrDefault();
                        if (obj != null)
                        {
                            point = new PointConstraint2D(obj, pt - obj.Position);
                            world.AddConstraint(point);
                        }
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
