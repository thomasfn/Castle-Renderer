using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.D3DCompiler;

namespace CastleRenderer.Graphics.MaterialSystem
{
    using SlimDX.Direct3D11;

    /// <summary>
    /// Represents a parameter block based on a struct
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MaterialParameterStruct<T> : MaterialParameterBlock where T : struct
    {
        /// <summary>
        /// Gets the size of this parameter struct in bytes
        /// </summary>
        public override int Size { get { return Marshal.SizeOf(typeof(T)); } }

        // The current state of this parameter struct
        private T state;

        /// <summary>
        /// Gets or sets the state of this parameter struct
        /// </summary>
        public T Value
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
                ds.Position = 0;
                ds.Write(state);
                MakeDirty();
            }
        }

        /// <summary>
        /// Initialises a new instance of the MaterialParameterStruct class
        /// </summary>
        /// <param name="initialvalue"></param>
        public MaterialParameterStruct(DeviceContext context, T initialvalue)
            : base(context)
        {
            Update();
            Value = initialvalue;
        }

        /// <summary>
        /// Clones this parameter struct
        /// </summary>
        /// <returns></returns>
        public MaterialParameterStruct<T> Clone()
        {
            return new MaterialParameterStruct<T>(Context, state);
        }

        public override string ToString()
        {
            return typeof(T).Name;
        }
    }
}
