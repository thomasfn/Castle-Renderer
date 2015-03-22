using System;
using System.Runtime.InteropServices;

using CastleRenderer.Structures;
using CastleRenderer.Graphics;
using CastleRenderer.Components.Physics;

using SlimDX;

namespace CastleRenderer.Components
{
    [ComponentPriority(9)]
    public class ConvexHull : BaseComponent
    {
        public Vector2[] Points { get; set; }
        public float Depth { get; set; }
        public bool GenerateMesh { get; set; }

        public override void OnAttach()
        {
            base.OnAttach();

            if (GenerateMesh)
            {
                MeshRenderer renderer = Owner.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.Mesh = MeshBuilder.BuildExtrudedConvexShape(Points, Depth);
                }
                RigidBody2D body = Owner.GetComponent<RigidBody2D>();
                if (body != null)
                {
                    //body.Shape = 
                    // TODO: Create convex hull shape
                }
            }
        }
    }
}
