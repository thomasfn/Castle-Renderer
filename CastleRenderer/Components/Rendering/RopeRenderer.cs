using System;
using System.Runtime.InteropServices;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;
using CastleRenderer.Graphics.MaterialSystem;

using SlimDX;

namespace CastleRenderer.Components
{
    /// <summary>
    /// Renders a rope mesh
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    [ComponentPriority(5)]
    public class RopeRenderer : GenericRenderer
    {
        /// <summary>
        /// Gets or sets the material to use
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Gets the bounding box of this mesh in world space
        /// </summary>
        public BoundingBox AABB { get; private set; }

        /// <summary>
        /// Gets or sets the number of divisions in the rope
        /// </summary>
        public int Divisions { get; set; }

        /// <summary>
        /// Gets or sets the curvature of the rope
        /// </summary>
        public float Curvature { get; set; }

        /// <summary>
        /// Gets or sets the start point of the rope
        /// </summary>
        public Transform StartPoint { get; set; }

        /// <summary>
        /// Gets or sets the offset of the start point of the rope
        /// </summary>
        public Vector3 StartOffset { get; set; }

        /// <summary>
        /// Gets or sets the end point of the rope
        /// </summary>
        public Transform EndPoint { get; set; }

        /// <summary>
        /// Gets or sets the offset of the end point of the rope
        /// </summary>
        public Vector3 EndOffset { get; set; }

        /// <summary>
        /// Gets or sets the width of the rope
        /// </summary>
        public float Width { get; set; }

        // The mesh
        private Mesh mesh;

        [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 32)]
        private struct RopeInfo
        {
            public Vector4 StartPos;
            public Vector4 EndPos;
        }

        private MaterialParameterStruct<RopeInfo> matpset_ropeinfo;

        public override void OnAttach()
        {
            base.OnAttach();

            matpset_ropeinfo = new MaterialParameterStruct<RopeInfo>(Owner.Root.GetComponent<Renderer>().Device.ImmediateContext, default(RopeInfo));

            MeshBuilder builder = new MeshBuilder();
            builder.UseTexCoords = true;
            builder.UseNormals = true;
            builder.UseTangents = true;

            Vector3 curve = new Vector3(Curvature, 0.0f, 1.0f);
            curve.Normalize();
            Vector3 curve2 = new Vector3(-curve.X, curve.Y, curve.Z);

            Vector3 curve3 = new Vector3(1.0f, 0.0f, -Curvature);
            curve3.Normalize();

            Vector3 curve4 = new Vector3(curve3.X, curve3.Y, -curve3.Z);

            float dy = 1.0f / Divisions;
            for (int i = 0; i <= Divisions; i++)
            {
                builder.AddPosition(new Vector3(Width * -0.5f, i * dy, 0.0f));
                builder.AddPosition(new Vector3(Width * 0.5f, i * dy, 0.0f));
                
                builder.AddNormal(curve2);
                builder.AddNormal(curve);
                builder.AddTangent(curve4);
                builder.AddTangent(curve3);
                builder.AddTextureCoord(new Vector2(0.0f, i));
                builder.AddTextureCoord(new Vector2(1.0f, i));
                if (i > 0)
                {
                    builder.AddIndex((uint)((i - 1) * 2 + 0)); builder.AddIndex((uint)((i - 1) * 2 + 2)); builder.AddIndex((uint)((i - 1) * 2 + 3));
                    builder.AddIndex((uint)((i - 1) * 2 + 3)); builder.AddIndex((uint)((i - 1) * 2 + 1)); builder.AddIndex((uint)((i - 1) * 2 + 0));
                }
            }

            mesh = builder.Build();

            Material.SetParameterBlock("RopeInfo", matpset_ropeinfo);
        }

        /// <summary>
        /// Called when it's time to populate the render queue
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(PopulateRenderQueue))]
        public void OnPopulateRenderQueue(PopulateRenderQueue msg)
        {
            // Sanity check
            if (mesh == null || Material == null) return;
            if (StartPoint == null || EndPoint == null) return;

            // Get start and endpos in world space
            Vector3 startpos = Util.Vector3Transform(StartOffset, StartPoint.ObjectToWorld);
            Vector3 endpos = Util.Vector3Transform(EndOffset, EndPoint.ObjectToWorld);

            // See if we can work out the "looseness"
            float looseness = 0.0f;
            var physcon = Owner.GetComponent<Physics.Constraint2D>();
            if (physcon != null && physcon.Constraint is Physics2D.Constraints.RopeConstraint2D)
            {
                var rope = physcon.Constraint as Physics2D.Constraints.RopeConstraint2D;
                looseness = rope.Length - (endpos - startpos).Length();
            }

            // Update
            matpset_ropeinfo.Value = new RopeInfo
            {
                StartPos = new Vector4(startpos, looseness),
                EndPos = new Vector4(endpos, 1.0f)
            };

            // Render rope
            msg.SceneManager.QueueDraw(mesh, 0, Material, AABB, ObjectTransformParameterBlock);
        }

        protected override void UpdateMaterialParameterBlocks()
        {
            // Call base
            base.UpdateMaterialParameterBlocks();

            // Update bounding box
            if (mesh != null)
                AABB = Util.BoundingBoxTransform(mesh.AABB, Owner.GetComponent<Transform>().ObjectToWorld);
        }

    }
}