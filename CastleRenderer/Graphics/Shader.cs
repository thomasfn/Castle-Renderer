using System;
using System.Collections.Generic;

using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;

namespace CastleRenderer.Graphics
{
    public class Shader : IDisposable
    {
        public VertexShader Vertex { get; set; }
        public PixelShader Pixel { get; set; }
        public GeometryShader Geo { get; set; }

        public Effect Effect { get; set; }

        private EffectPass defaultpass;
        public EffectPass DefaultPass
        {
            get
            {
                if (defaultpass == null) defaultpass = Effect.GetTechniqueByIndex(0).GetPassByIndex(0);
                return defaultpass;
            }
        }

        //private ShaderSignature vtxsig;

        private ShaderBytecode bcVertex, bcPixel, bcGeo, bcEffect;

        private class ShaderVariable
        {
            public EffectVariable Variable;
            public object Value;
        }

        private Dictionary<string, ShaderVariable> variablemap;

        public const int MaxResourceViewSlots = 8;

        public Device Device { get; private set; }

        public Shader(Device device)
        {
            Device = device;
            variablemap = new Dictionary<string,ShaderVariable>();
        }

        public virtual void VertexFromString(string input)
        {
            bcVertex = ShaderBytecode.Compile(input, "VShader", "vs_4_0", ShaderFlags.Debug, EffectFlags.None);
            Vertex = new VertexShader(Device, bcVertex);
        }

        public virtual void PixelFromString(string input)
        {
            bcPixel = ShaderBytecode.Compile(input, "PShader", "ps_4_0", ShaderFlags.Debug, EffectFlags.None);
            Pixel = new PixelShader(Device, bcPixel);
        }

        public virtual void GeometryFromString(string input)
        {
            bcGeo = ShaderBytecode.Compile(input, "GShader", "gs_4_0", ShaderFlags.Debug, EffectFlags.None);
            Geo = new GeometryShader(Device, bcGeo);
        }

        public virtual void EffectFromString(string input)
        {
            bcEffect = ShaderBytecode.Compile(input, "-", "fx_5_0", ShaderFlags.Debug, EffectFlags.None);
            Effect = new Effect(Device, bcEffect);
        }

        public virtual void Activate(DeviceContext context)
        {
            context.PixelShader.Set(Pixel);
            context.VertexShader.Set(Vertex);
            context.GeometryShader.Set(Geo);
        }

        public virtual InputLayout CreateLayout(InputElement[] elements)
        {
            return new InputLayout(Device, bcVertex, elements);
        }

        public void SetVariable(string name, object value)
        {
            ShaderVariable variable;
            if (!variablemap.TryGetValue(name, out variable))
            {
                variable = new ShaderVariable();
                variable.Variable = Effect.GetVariableByName(name);
                variable.Value = null;
                variablemap.Add(name, variable);
            }
            if (!variable.Variable.IsValid) return;
            if (variable.Value == value) return;
            if (value is float)
                variable.Variable.AsScalar().Set((float)value);
            else if (value is Vector2)
                variable.Variable.AsVector().Set((Vector2)value);
            else if (value is Vector3)
                variable.Variable.AsVector().Set((Vector3)value);
            else if (value is Vector4)
                variable.Variable.AsVector().Set((Vector4)value);
            else if (value is ShaderResourceView)
                variable.Variable.AsResource().SetResource(value as ShaderResourceView);
            else if (value is SamplerState)
                variable.Variable.AsSampler().SetSamplerState(0, value as SamplerState);
            else if (value is Matrix)
                variable.Variable.AsMatrix().SetMatrix((Matrix)value);
        }

        public virtual void Dispose()
        {
            if (Vertex != null)
            {
                Vertex.Dispose();
                Vertex = null;
                bcVertex.Dispose();
                bcVertex = null;
            }
            if (Pixel != null)
            {
                Pixel.Dispose();
                Pixel = null;
                bcPixel.Dispose();
                bcPixel = null;
            }
            if (Effect != null)
            {
                Effect.Dispose();
                Effect = null;
                bcEffect.Dispose();
                bcEffect = null;
            }
            if (Geo != null)
            {
                Geo.Dispose();
                Geo = null;
                bcGeo.Dispose();
                bcGeo = null;
            }
        }
    }
}
