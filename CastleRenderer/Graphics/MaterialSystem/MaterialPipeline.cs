using System;
using System.Collections.Generic;
using System.Linq;

using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;

using CastleRenderer.Graphics.Shaders;

namespace CastleRenderer.Graphics.MaterialSystem
{
    /// <summary>
    /// Represents a material pipeline
    /// </summary>
    public class MaterialPipeline
    {
        // All shaders attached to this pipeline
        private HashSet<IShader> shaders;
        private List<IShader> pipeline;

        // Signature for the vertex shader
        private ShaderSignature vtxsig;

        private sealed class ConstantBufferBinding
        {
            public IShader Shader { get; set; }
            public int Slot { get; set; }
            public ConstantBuffer CBuffer { get; set; }
            public MaterialParameterBlock CurrentBlock { get; set; }
        }

        private sealed class ConstantBufferData
        {
            public HashSet<ConstantBufferBinding> Bindings { get; private set; }

            public ConstantBufferData()
            {
                Bindings = new HashSet<ConstantBufferBinding>();
            }
        }

        // All constant buffers attached to this pipeline
        private Dictionary<string, ConstantBufferData> cbuffers;
        private HashSet<ConstantBufferBinding> cbufferbindings;

        /// <summary>
        /// Gets the owner device for this pipeline
        /// </summary>
        public DeviceContext Context { get; private set; }

        /// <summary>
        /// Initialises a new instance of the MaterialPipeline class
        /// </summary>
        public MaterialPipeline(DeviceContext context)
        {
            Context = context;
            shaders = new HashSet<IShader>();
        }

        /// <summary>
        /// Adds a shader object to this pipeline
        /// </summary>
        /// <param name="shader"></param>
        public bool AddShader(IShader shader)
        {
            // Check for duplicate
            foreach (IShader existing in shaders)
                if (shader.Type == existing.Type)
                    return false;

            // Add it
            shaders.Add(shader);

            // Success
            return true;
        }

        /// <summary>
        /// Links this pipeline and prepares it for use
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        public bool Link(out string err)
        {
            // Verify that a vertex shader is present
            if (!shaders.Any((s) => s.Type == ShaderType.Vertex))
            {
                err = "Missing vertex shader";
                return false;
            }

            // Identify the pipeline
            pipeline = new List<IShader>();
            IShader tmp;
            tmp = shaders.SingleOrDefault((s) => s.Type == ShaderType.Vertex);
            if (tmp != null) pipeline.Add(tmp);
            tmp = shaders.SingleOrDefault((s) => s.Type == ShaderType.Geometry);
            if (tmp != null) pipeline.Add(tmp);
            tmp = shaders.SingleOrDefault((s) => s.Type == ShaderType.Pixel);
            if (tmp != null) pipeline.Add(tmp);
            // TODO: Add support for tessellation?

            // Get the vertex shader signature
            vtxsig = pipeline[0].Signature;

            // Reflect all shaders
            ShaderReflection[] refl = new ShaderReflection[pipeline.Count];
            for (int i = 0; i < pipeline.Count; i++)
            {
                refl[i] = new ShaderReflection(pipeline[i].Bytecode);
            }
            
            // Verify the output of each stage matches the input of the next
            for (int i = 0; i < pipeline.Count - 1; i++)
            {
                ShaderReflection first = refl[i];
                ShaderReflection second = refl[i + 1];
                var outputs = Util.GetUnknownShaderReflectionArray(first.GetOutputParameterDescription);
                var inputs = Util.GetUnknownShaderReflectionArray(second.GetInputParameterDescription);
                if (outputs.Length != inputs.Length)
                {
                    err = string.Format("Output parameters of {0} stage do not match input parameters of {1} stage", pipeline[i].Type, pipeline[i + 1].Type);
                    return false;
                }
                for (int j = 0; j < outputs.Length; j++)
                {
                    if (inputs[j] != outputs[j])
                    {
                        err = string.Format("Output parameters of {0} stage do not match input parameters of {1} stage", pipeline[i].Type, pipeline[i + 1].Type);
                        return false;
                    }
                }
            }

            // Find all constant buffers
            cbuffers = new Dictionary<string, ConstantBufferData>();
            cbufferbindings = new HashSet<ConstantBufferBinding>();
            for (int i = 0; i < pipeline.Count; i++)
            {
                var shaderconstantbuffers = Util.GetUnknownShaderReflectionArray(refl[i].GetConstantBuffer);
                for (int j = 0; j < shaderconstantbuffers.Length; j++)
                {
                    ConstantBuffer cbuffer = shaderconstantbuffers[j];
                    var desc = cbuffer.Description;
                    ConstantBufferData data;
                    if (!cbuffers.TryGetValue(desc.Name, out data))
                    {
                        data = new ConstantBufferData();
                        cbuffers.Add(desc.Name, data);

                    }
                    foreach (var existingbinding in data.Bindings)
                    {
                        if (existingbinding.CBuffer.Description != desc)
                        {
                            err = string.Format("CBuffer '{0}' of {1} stage does not match {2} stage", desc.Name, pipeline[i].Type, existingbinding.Shader.Type);
                            return false;
                        }
                    }
                    var binding = new ConstantBufferBinding { Shader = pipeline[i], CBuffer = cbuffer };
                    binding.Slot = j;
                    data.Bindings.Add(binding);
                    cbufferbindings.Add(binding);
                }
            }

            // Success
            err = null;
            return true;
        }

        /// <summary>
        /// Binds the specified parameter block to a CBuffer by the specified name on this pipeline
        /// </summary>
        /// <param name="cbuffername"></param>
        /// <param name="set"></param>
        public void SetMaterialParameterBlock(string cbuffername, MaterialParameterBlock block)
        {
            ConstantBufferData data;
            if (!cbuffers.TryGetValue(cbuffername, out data)) return;
            foreach (var binding in data.Bindings)
            {
                binding.CurrentBlock = block;
            }
        }

        /// <summary>
        /// Makes this pipeline active
        /// </summary>
        public void Activate()
        {
            // Make all shaders active
            foreach (IShader shader in shaders)
                shader.MakeActive(Context);

            // Make all constant buffers active
            // TODO: Optimise by keeping track of a Buffer[] for each shader and batch setting the whole array instead of one binding at a time
            foreach (ConstantBufferBinding binding in cbufferbindings)
                binding.Shader.SetConstantBuffer(Context, binding.Slot, binding.CurrentBlock != null ? binding.CurrentBlock.Buffer : null);
                
        }

        /// <summary>
        /// Gets the names of all CBuffers in use by this pipeline
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetCBufferNames()
        {
            return cbuffers.Keys;
        }

        /// <summary>
        /// Creates a parameter set for the specified CBuffer name
        /// </summary>
        /// <param name="cbuffername"></param>
        /// <returns></returns>
        public MaterialParameterSet CreateParameterSet(string cbuffername)
        {
            ConstantBufferData data;
            if (!cbuffers.TryGetValue(cbuffername, out data)) return null;
            ConstantBuffer cbuf = data.Bindings.Single().CBuffer;
            return new MaterialParameterSet(Context, cbuf);
        }

        /// <summary>
        /// Creates an input layout for the specified set of input elements
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public InputLayout CreateLayout(InputElement[] elements)
        {
            return new InputLayout(Context.Device, vtxsig, elements);
        }


    }
}
