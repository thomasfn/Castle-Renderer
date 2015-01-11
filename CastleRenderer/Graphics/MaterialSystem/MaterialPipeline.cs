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

        // The input parameter
        private ShaderParameterDescription[] pipelineinputs;

        #region Binding Classes

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

        private sealed class ResourceBinding
        {
            public IShader Shader { get; set; }
            public int Slot { get; set; }
            public InputBindingDescription Description { get; set; }
            public ShaderResourceView CurrentView { get; set; }
        }

        private sealed class ResourceData
        {
            public HashSet<ResourceBinding> Bindings { get; private set; }

            public ResourceData()
            {
                Bindings = new HashSet<ResourceBinding>();
            }
        }

        private sealed class SamplerStateBinding
        {
            public IShader Shader { get; set; }
            public int Slot { get; set; }
            public InputBindingDescription Description { get; set; }
            public SamplerState CurrentSamplerState { get; set; }
        }

        private sealed class SamplerStateData
        {
            public HashSet<SamplerStateBinding> Bindings { get; private set; }

            public SamplerStateData()
            {
                Bindings = new HashSet<SamplerStateBinding>();
            }
        }

        #endregion

        // All constant buffers attached to this pipeline
        private Dictionary<string, ConstantBufferData> cbuffers;
        private HashSet<ConstantBufferBinding> cbufferbindings;

        // All resources attached to this pipeline
        private Dictionary<string, ResourceData> resources;
        private HashSet<ResourceBinding> resourcebindings;

        // All sampler states attached to this pipeline
        private Dictionary<string, SamplerStateData> samplerstates;
        private HashSet<SamplerStateBinding> samplerstatebindings;

        /// <summary>
        /// Gets the owner device for this pipeline
        /// </summary>
        public DeviceContext Context { get; private set; }

        // The currently active pipeline
        private static MaterialPipeline activepipeline;

        /// <summary>
        /// Returns if this pipeline is currently active or not
        /// </summary>
        public bool IsActive
        {
            get
            {
                return activepipeline == this;
            }
        }

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

            // Get the input parameter
            pipelineinputs = new ShaderParameterDescription[refl[0].Description.InputParameters];
            for (int i = 0; i < pipelineinputs.Length; i++)
            {
                pipelineinputs[i] = refl[0].GetInputParameterDescription(i);
            }
            
            // Verify the output of each stage matches the input of the next
            for (int i = 0; i < pipeline.Count - 1; i++)
            {
                ShaderReflection first = refl[i];
                ShaderReflection second = refl[i + 1];
                var firstdesc = first.Description;
                var seconddesc = second.Description;
                if (firstdesc.OutputParameters != seconddesc.InputParameters)
                {
                    err = string.Format("Output parameters of {0} stage do not match input parameters of {1} stage", pipeline[i].Type, pipeline[i + 1].Type);
                    return false;
                }
                for (int j = 0; j < firstdesc.OutputParameters; j++)
                {
                    if (!VerifyParameterMatch(first.GetOutputParameterDescription(j), second.GetInputParameterDescription(j)))
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
                var shaderdesc = refl[i].Description;
                for (int j = 0; j < shaderdesc.ConstantBuffers; j++)
                {
                    ConstantBuffer cbuffer = refl[i].GetConstantBuffer(j);
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
                    var binding = new ConstantBufferBinding { Shader = pipeline[i], CBuffer = cbuffer, Slot = j };
                    data.Bindings.Add(binding);
                    cbufferbindings.Add(binding);
                }
            }

            // Find all resource bindings and sampler states
            resources = new Dictionary<string, ResourceData>();
            resourcebindings = new HashSet<ResourceBinding>();
            samplerstates = new Dictionary<string, SamplerStateData>();
            samplerstatebindings = new HashSet<SamplerStateBinding>();
            for (int i = 0; i < pipeline.Count; i++)
            {
                var shaderdesc = refl[i].Description;
                for (int j = 0; j < shaderdesc.BoundResources; j++)
                {
                    InputBindingDescription desc = refl[i].GetResourceBindingDescription(j);
                    if (desc.Type == ShaderInputType.Sampler)
                    {
                        SamplerStateData data;
                        if (!samplerstates.TryGetValue(desc.Name, out data))
                        {
                            data = new SamplerStateData();
                            samplerstates.Add(desc.Name, data);
                        }
                        foreach (var existingbinding in data.Bindings)
                        {
                            if (existingbinding.Description.Type != desc.Type)
                            {
                                err = string.Format("Sampler state '{0}' of {1} stage does not match {2} stage", desc.Name, pipeline[i].Type, existingbinding.Shader.Type);
                                return false;
                            }
                        }
                        var binding = new SamplerStateBinding { Shader = pipeline[i], Description = desc, Slot = desc.BindPoint };
                        data.Bindings.Add(binding);
                        samplerstatebindings.Add(binding);
                    }
                    else
                    {
                        ResourceData data;
                        if (!resources.TryGetValue(desc.Name, out data))
                        {
                            data = new ResourceData();
                            resources.Add(desc.Name, data);
                        }
                        foreach (var existingbinding in data.Bindings)
                        {
                            if (existingbinding.Description.Type != desc.Type)
                            {
                                err = string.Format("Resource '{0}' of {1} stage does not match {2} stage", desc.Name, pipeline[i].Type, existingbinding.Shader.Type);
                                return false;
                            }
                        }
                        var binding = new ResourceBinding { Shader = pipeline[i], Description = desc, Slot = desc.BindPoint };
                        data.Bindings.Add(binding);
                        resourcebindings.Add(binding);
                    }
                }
            }

            // Success
            err = null;
            return true;
        }

        private static bool VerifyParameterMatch(ShaderParameterDescription a, ShaderParameterDescription b)
        {
            if (a.ComponentType != b.ComponentType) return false;
            if (a.SemanticIndex != b.SemanticIndex) return false;
            if (a.SemanticName != b.SemanticName) return false;
            //if (CountComponents(a.ReadWriteMask) != CountComponents(b.ReadWriteMask)) return false;
            return true;
        }

        private static int CountComponents(RegisterComponentMaskFlags flags)
        {
            int c = 0;
            if (flags == RegisterComponentMaskFlags.None) return 0;
            if (flags == RegisterComponentMaskFlags.All) return 4;
            if ((flags & RegisterComponentMaskFlags.ComponentW) == RegisterComponentMaskFlags.ComponentW) c++;
            if ((flags & RegisterComponentMaskFlags.ComponentZ) == RegisterComponentMaskFlags.ComponentZ) c++;
            if ((flags & RegisterComponentMaskFlags.ComponentY) == RegisterComponentMaskFlags.ComponentY) c++;
            if ((flags & RegisterComponentMaskFlags.ComponentX) == RegisterComponentMaskFlags.ComponentX) c++;
            return c;
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
                if (IsActive) binding.Shader.SetConstantBuffer(Context, binding.Slot, binding.CurrentBlock != null ? binding.CurrentBlock.Buffer : null);
            }
            if (IsActive && block != null) block.Update();
        }

        /// <summary>
        /// Binds the specified resource view to this pipeline
        /// </summary>
        /// <param name="resourcename"></param>
        /// <param name="resource"></param>
        public void SetResource(string resourcename, ShaderResourceView resource)
        {
            ResourceData data;
            if (!resources.TryGetValue(resourcename, out data)) return;
            foreach (var binding in data.Bindings)
            {
                binding.CurrentView = resource;
                if (IsActive) binding.Shader.SetResource(Context, binding.Slot, binding.CurrentView);
            }
        }

        /// <summary>
        /// Binds the specified resource view to this pipeline
        /// </summary>
        /// <param name="resourcename"></param>
        /// <param name="resource"></param>
        public void SetSamplerState(string samplerstatename, SamplerState samplerstate)
        {
            SamplerStateData data;
            if (!samplerstates.TryGetValue(samplerstatename, out data)) return;
            foreach (var binding in data.Bindings)
            {
                binding.CurrentSamplerState = samplerstate;
                if (IsActive) binding.Shader.SetSamplerState(Context, binding.Slot, binding.CurrentSamplerState);
            }
        }

        /// <summary>
        /// Makes this pipeline active
        /// </summary>
        public void Activate(bool force = false)
        {
            // Check for already active
            if (!force && activepipeline == this) return;

            // Make all shaders active
            foreach (IShader shader in shaders)
                shader.MakeActive(Context);

            // Make all constant buffers active
            // TODO: Optimise by keeping track of a Buffer[] for each shader and batch setting the whole array instead of one binding at a time
            foreach (ConstantBufferBinding binding in cbufferbindings)
            {
                binding.Shader.SetConstantBuffer(Context, binding.Slot, binding.CurrentBlock != null ? binding.CurrentBlock.Buffer : null);
                if (binding.CurrentBlock != null) binding.CurrentBlock.Update();
            }

            // Make all resources active
            foreach (ResourceBinding binding in resourcebindings)
                binding.Shader.SetResource(Context, binding.Slot, binding.CurrentView);

            // Make all sampler states active
            foreach (SamplerStateBinding binding in samplerstatebindings)
                binding.Shader.SetSamplerState(Context, binding.Slot, binding.CurrentSamplerState);

            // We're now active
            activepipeline = this;
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
            // Filter out elements which aren't inputs to this pipeline
            InputElement[] newelements = elements
                .Where((e) => pipelineinputs.Any((i) => i.SemanticName == e.SemanticName && i.SemanticIndex == e.SemanticIndex))
                .ToArray();
            return new InputLayout(Context.Device, vtxsig, newelements);
        }


    }
}
