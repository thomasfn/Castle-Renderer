using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.D3DCompiler;

namespace CastleRenderer.Graphics.MaterialSystem
{
    using SlimDX.Direct3D11;

    /// <summary>
    /// Represents a block of material parameters
    /// </summary>
    public abstract class MaterialParameterBlock
    {
        /// <summary>
        /// Gets the parameter buffer
        /// </summary>
        public Buffer Buffer { get; private set; }

        // The datastream
        protected DataStream ds;

        // Do we need to be updated?
        private bool dirty;

        /// <summary>
        /// Gets the context for this parameter block
        /// </summary>
        public DeviceContext Context { get; private set; }

        /// <summary>
        /// Gets the size of this parameter block in bytes
        /// </summary>
        public abstract int Size { get; }

        /// <summary>
        /// Initialises a new instance of the MaterialParameterBlock class
        /// </summary>
        /// <param name="cbuffer"></param>
        public MaterialParameterBlock(DeviceContext context)
        {
            // Store context
            Context = context;

            // Start off dirty
            dirty = true;
        }

        /// <summary>
        /// Updates the buffer behind this parameter block if needed
        /// </summary>
        public void Update()
        {
            // Check if dirty
            if (!dirty) return;

            // Update
            dirty = false;
            if (ds == null) ds = new DataStream(Size, true, true);
            ds.Position = 0;
            if (Buffer == null)
            {
                BufferDescription desc = new BufferDescription
                {
                    Usage = ResourceUsage.Default,
                    SizeInBytes = Size,
                    BindFlags = BindFlags.ConstantBuffer
                };
                Buffer = new Buffer(Context.Device, ds, desc);
                Buffer.DebugName = string.Format("MaterialParameterBlock ({0})", this);
            }
            else
                Context.UpdateSubresource(new DataBox(0, 0, ds), Buffer, 0);
        }

        /// <summary>
        /// Specifies that something in the block has changed
        /// </summary>
        protected void MakeDirty()
        {
            dirty = true;
        }

    }
}
