using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.D3DCompiler;

namespace CastleRenderer.Graphics.MaterialSystem
{
    using SlimDX.Direct3D11;

    /// <summary>
    /// Represents a set of material parameters
    /// </summary>
    public class MaterialParameterSet
    {
        // The constant buffer that we map to
        private ConstantBuffer cbuffer;

        /// <summary>
        /// Gets the parameter buffer
        /// </summary>
        public Buffer Buffer { get; private set; }

        // The datastream
        private DataStream ds;

        // Do we need to be updated?
        private bool dirty;

        /// <summary>
        /// Gets the context for this parameter set
        /// </summary>
        public DeviceContext Context { get; private set; }

        private abstract class Holder { public Holder<T> As<T>() where T : struct, IEquatable<T> { return this as Holder<T>; } }
        private sealed class Holder<T> : Holder where T : struct, IEquatable<T> { public T Value; }

        private struct ParameterInfo
        {
            // The .net type of the parameter
            public Type Type;

            // The offset into the buffer
            public int Offset;

            // The current value
            public Holder CurrentValue;

            /// <summary>
            /// Initialises a new instance of the ParameterInfo struct
            /// </summary>
            /// <param name="type"></param>
            /// <param name="offset"></param>
            public ParameterInfo(Type type, int offset)
            {
                Type = type;
                Offset = offset;
                CurrentValue = Activator.CreateInstance(typeof(Holder<>).MakeGenericType(type)) as Holder;
            }
        }

        // All parameters
        private Dictionary<string, ParameterInfo> parameters;

        /// <summary>
        /// Initialises a new instance of the MaterialParameterSet class
        /// </summary>
        /// <param name="cbuffer"></param>
        public MaterialParameterSet(DeviceContext context, ConstantBuffer cbuffer)
        {
            // Store cbuffer
            this.cbuffer = cbuffer;
            var cbufdesc = cbuffer.Description;
            
            // Initialise datastream and buffer
            ds = new DataStream(cbufdesc.Size, true, true);
            BufferDescription desc = new BufferDescription
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = cbufdesc.Size,
                BindFlags = BindFlags.ConstantBuffer
            };
            ds.Position = 0;
            Buffer = new Buffer(context.Device, ds, desc);
            Buffer.DebugName = string.Format("MaterialParameterSet ({0})", cbufdesc.Name);

            // Initialise parameters
            parameters = new Dictionary<string, ParameterInfo>();
            for (int i = 0; i < cbufdesc.Variables; i++)
            {
                ShaderReflectionVariable vbl = cbuffer.GetVariable(i);
                var vbldesc = vbl.Description;
                ShaderReflectionType type = vbl.GetVariableType();
                Type nettype = TranslateType(type);
                if (nettype == null)
                    Console.WriteLine("Unable to translate shader type of parameter '{0}' ({1}) into .NET type! ({2})", vbldesc.Name, type, cbufdesc.Name);
                else
                {
                    if (!nettype.IsValueType)
                        Console.WriteLine("Translation of shader type to .NET type failed! (Returned ref-type)");
                    else
                    {
                        int predictedsize = Marshal.SizeOf(Activator.CreateInstance(nettype));
                        if (predictedsize != vbldesc.Size)
                            Console.WriteLine("Translation of shader type to .NET type failed! (Size mismatch)");
                        else
                            parameters.Add(vbl.Description.Name, new ParameterInfo(nettype, vbldesc.StartOffset));
                    }
                }
            }
        }

        /// <summary>
        /// Clones this parameter set
        /// </summary>
        /// <returns></returns>
        public MaterialParameterSet Clone()
        {
            return new MaterialParameterSet(Context, cbuffer);
        }

        /// <summary>
        /// Translated a reflected shader type into a .net type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type TranslateType(ShaderReflectionType type)
        {
            var desc = type.Description;
            switch (desc.Class)
            {
                case ShaderVariableClass.Scalar:
                    return typeof(float);
                case ShaderVariableClass.Vector:
                    switch (desc.Elements)
                    {
                        case 1:
                            return typeof(float);
                        case 2:
                            return typeof(Vector2);
                        case 3:
                            return typeof(Vector3);
                        case 4:
                            return typeof(Vector4);
                    }
                    break;
            }
            return null;
        }
        
        /// <summary>
        /// Sets a parameter on this parameter set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetParameter<T>(string name, T value) where T : struct, IEquatable<T>
        {
            // Get the parameter info
            ParameterInfo info;
            if (!parameters.TryGetValue(name, out info)) return;

            // Check the types match
            if (info.Type != typeof(T)) return;

            // Check if the value changed
            var holder = info.CurrentValue.As<T>();
            if (holder.Value.Equals(value)) return;

            // Set it
            ds.Position = info.Offset;
            ds.Write<T>(value);
            holder.Value = value;

            // We are now dirty
            dirty = true;
        }

        /// <summary>
        /// Updates the buffer behind this parameter set if needed
        /// </summary>
        public void Update()
        {
            // Check if dirty
            if (!dirty) return;

            // Update
            dirty = false;
            ds.Position = 0;
            Context.UpdateSubresource(new DataBox(0, 0, ds), Buffer, 0);

        }

    }
}
