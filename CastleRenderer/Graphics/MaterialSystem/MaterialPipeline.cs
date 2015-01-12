using System;
using System.Collections.Generic;
using System.Linq;

using SlimDX;
using SlimDX.D3DCompiler;

using CastleRenderer.Graphics.Shaders;

namespace CastleRenderer.Graphics.MaterialSystem
{
    using SlimDX.Direct3D11;

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
            public IShader Shader;
            public int ShaderIndex;
            public int Slot;
            public ConstantBuffer CBuffer;
            public MaterialParameterBlock CurrentBlock;
        }

        private sealed class ConstantBufferData
        {
            public string Name;
            public HashSet<ConstantBufferBinding> Bindings;

            public ConstantBufferData()
            {
                Bindings = new HashSet<ConstantBufferBinding>();
            }
        }

        private sealed class ResourceBinding
        {
            public IShader Shader;
            public int ShaderIndex;
            public int Slot;
            public InputBindingDescription Description;
            public ShaderResourceView CurrentView;
        }

        private sealed class ResourceData
        {
            public string Name;
            public HashSet<ResourceBinding> Bindings;

            public ResourceData()
            {
                Bindings = new HashSet<ResourceBinding>();
            }
        }

        private sealed class SamplerStateBinding
        {
            public IShader Shader;
            public int ShaderIndex;
            public int Slot;
            public InputBindingDescription Description;
            public SamplerState CurrentSamplerState;
        }

        private sealed class SamplerStateData
        {
            public string Name;
            public HashSet<SamplerStateBinding> Bindings;

            public SamplerStateData()
            {
                Bindings = new HashSet<SamplerStateBinding>();
            }
        }

        #endregion

        // All constant buffers attached to this pipeline
        private List<ConstantBufferData> cbuffers;
        private HashSet<ConstantBufferBinding> cbufferbindings;

        // All resources attached to this pipeline
        private List<ResourceData> resources;
        private HashSet<ResourceBinding> resourcebindings;

        // All sampler states attached to this pipeline
        private List<SamplerStateData> samplerstates;
        private HashSet<SamplerStateBinding> samplerstatebindings;

        private struct ShaderBindingCache
        {
            public Buffer[] ConstantBuffers;
            public ShaderResourceView[] Resources;
            public SamplerState[] SamplerStates;
        }

        private ShaderBindingCache[] bindingcache;
        private bool cachedirty;

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
            cbuffers = new List<ConstantBufferData>();
            cbufferbindings = new HashSet<ConstantBufferBinding>();
            for (int i = 0; i < pipeline.Count; i++)
            {
                var shaderdesc = refl[i].Description;
                for (int j = 0; j < shaderdesc.ConstantBuffers; j++)
                {
                    ConstantBuffer cbuffer = refl[i].GetConstantBuffer(j);
                    var desc = cbuffer.Description;
                    ConstantBufferData data;
                    if (!cbuffers.Any((c) => c.Name == desc.Name))
                    {
                        data = new ConstantBufferData();
                        data.Name = desc.Name;
                        cbuffers.Add(data);
                    }
                    else
                        data = cbuffers.Single((c) => c.Name == desc.Name);
                    foreach (var existingbinding in data.Bindings)
                    {
                        if (existingbinding.CBuffer.Description != desc)
                        {
                            err = string.Format("CBuffer '{0}' of {1} stage does not match {2} stage", desc.Name, pipeline[i].Type, existingbinding.Shader.Type);
                            return false;
                        }
                    }
                    var binding = new ConstantBufferBinding { Shader = pipeline[i], ShaderIndex = i, CBuffer = cbuffer, Slot = j };
                    data.Bindings.Add(binding);
                    cbufferbindings.Add(binding);
                }
            }

            // Find all resource bindings and sampler states
            resources = new List<ResourceData>();
            resourcebindings = new HashSet<ResourceBinding>();
            samplerstates = new List<SamplerStateData>();
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
                        if (!samplerstates.Any((s) => s.Name == desc.Name))
                        {
                            data = new SamplerStateData();
                            data.Name = desc.Name;
                            samplerstates.Add(data);
                        }
                        else
                            data = samplerstates.Single((s) => s.Name == desc.Name);
                        foreach (var existingbinding in data.Bindings)
                        {
                            if (existingbinding.Description.Type != desc.Type)
                            {
                                err = string.Format("Sampler state '{0}' of {1} stage does not match {2} stage", desc.Name, pipeline[i].Type, existingbinding.Shader.Type);
                                return false;
                            }
                        }
                        var binding = new SamplerStateBinding { Shader = pipeline[i], ShaderIndex = i, Description = desc, Slot = desc.BindPoint };
                        data.Bindings.Add(binding);
                        samplerstatebindings.Add(binding);
                    }
                    else if (desc.Type != ShaderInputType.ConstantBuffer)
                    {
                        ResourceData data;
                        if (!resources.Any((r) => r.Name == desc.Name))
                        {
                            data = new ResourceData();
                            data.Name = desc.Name;
                            resources.Add(data);
                        }
                        else
                            data = resources.Single((r) => r.Name == desc.Name);
                        foreach (var existingbinding in data.Bindings)
                        {
                            if (existingbinding.Description.Type != desc.Type)
                            {
                                err = string.Format("Resource '{0}' of {1} stage does not match {2} stage", desc.Name, pipeline[i].Type, existingbinding.Shader.Type);
                                return false;
                            }
                        }
                        var binding = new ResourceBinding { Shader = pipeline[i], ShaderIndex = i, Description = desc, Slot = desc.BindPoint };
                        data.Bindings.Add(binding);
                        resourcebindings.Add(binding);
                    }
                }
            }

            // Initialise binding cache
            bindingcache = new ShaderBindingCache[pipeline.Count];
            for (int i = 0; i < pipeline.Count; i++)
            {
                int numconstantbuffers;
                if (cbufferbindings.Any((c) => c.Shader == pipeline[i]))
                    numconstantbuffers = cbufferbindings
                        .Where((c) => c.Shader == pipeline[i])
                        .Select((c) => c.Slot)
                        .Max() + 1;
                else
                    numconstantbuffers = 0;

                int numresources;
                if (resourcebindings.Any((c) => c.Shader == pipeline[i]))
                    numresources = resourcebindings
                        .Where((c) => c.Shader == pipeline[i])
                        .Select((c) => c.Slot)
                        .Max() + 1;
                else
                    numresources = 0;

                int numsamplerstates;
                if (samplerstatebindings.Any((c) => c.Shader == pipeline[i]))
                    numsamplerstates = samplerstatebindings
                        .Where((c) => c.Shader == pipeline[i])
                        .Select((c) => c.Slot)
                        .Max() + 1;
                else
                    numsamplerstates = 0;

                ShaderBindingCache cache = new ShaderBindingCache();
                cache.ConstantBuffers = new Buffer[numconstantbuffers];
                cache.Resources = new ShaderResourceView[numresources];
                cache.SamplerStates = new SamplerState[numsamplerstates];
                bindingcache[i] = cache;
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

        #region Material Parameter Blocks

        /// <summary>
        /// Binds the specified parameter block to a CBuffer by the specified name on this pipeline
        /// </summary>
        /// <param name="cbuffername"></param>
        /// <param name="set"></param>
        public void SetMaterialParameterBlock(string cbuffername, MaterialParameterBlock block)
        {
            SetMaterialParameterBlock(LookupMaterialParameterBlockIndex(cbuffername), block);
        }

        /// <summary>
        /// Gets the index of the specified parameter block name
        /// </summary>
        /// <param name="cbuffername"></param>
        /// <returns></returns>
        public int LookupMaterialParameterBlockIndex(string cbuffername)
        {
            for (int i = 0; i < cbuffers.Count; i++)
            {
                if (cbuffers[i].Name == cbuffername)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Binds the specified parameter block to a CBuffer by the specified name on this pipeline
        /// </summary>
        /// <param name="cbuffername"></param>
        /// <param name="set"></param>
        public void SetMaterialParameterBlock(int cbufferindex, MaterialParameterBlock block)
        {
            if (cbufferindex == -1) return;
            ConstantBufferData data = cbuffers[cbufferindex];
            foreach (var binding in data.Bindings)
            {
                binding.CurrentBlock = block;
                bindingcache[binding.ShaderIndex].ConstantBuffers[binding.Slot] = block.Buffer;
                //if (IsActive) binding.Shader.SetConstantBuffer(Context, binding.Slot, binding.CurrentBlock != null ? binding.CurrentBlock.Buffer : null);
            }
            if (IsActive)
            {
                cachedirty = true;
                if (block != null) block.Update();
            }
        }

        #endregion

        #region Resources

        /// <summary>
        /// Binds the specified resource view to this pipeline
        /// </summary>
        /// <param name="resourcename"></param>
        /// <param name="resource"></param>
        public void SetResource(string resourcename, ShaderResourceView resource)
        {
            SetResource(LookupResourceIndex(resourcename), resource);
        }

        /// <summary>
        /// Gets the index of the resource
        /// </summary>
        /// <param name="cbuffername"></param>
        /// <returns></returns>
        public int LookupResourceIndex(string resourcename)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i].Name == resourcename)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Binds the specified resource view to this pipeline
        /// </summary>
        /// <param name="resourcename"></param>
        /// <param name="resource"></param>
        public void SetResource(int resourceindex, ShaderResourceView resource)
        {
            if (resourceindex == -1) return;
            ResourceData data = resources[resourceindex];
            foreach (var binding in data.Bindings)
            {
                binding.CurrentView = resource;
                bindingcache[binding.ShaderIndex].Resources[binding.Slot] = resource;
                //if (IsActive) binding.Shader.SetResource(Context, binding.Slot, binding.CurrentView);
            }
            if (IsActive) cachedirty = true;
        }

        #endregion

        #region Sampler States

        /// <summary>
        /// Binds the specified resource view to this pipeline
        /// </summary>
        /// <param name="resourcename"></param>
        /// <param name="resource"></param>
        public void SetSamplerState(string samplerstatename, SamplerState samplerstate)
        {
            SetSamplerState(LookupSamplerStateIndex(samplerstatename), samplerstate);
        }

        /// <summary>
        /// Gets the index of the specified sampler state
        /// </summary>
        /// <param name="cbuffername"></param>
        /// <returns></returns>
        public int LookupSamplerStateIndex(string samplerstatename)
        {
            for (int i = 0; i < samplerstates.Count; i++)
            {
                if (samplerstates[i].Name == samplerstatename)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Binds the specified resource view to this pipeline
        /// </summary>
        /// <param name="resourcename"></param>
        /// <param name="resource"></param>
        public void SetSamplerState(int samplerstateindex, SamplerState samplerstate)
        {
            if (samplerstateindex == -1) return;
            SamplerStateData data = samplerstates[samplerstateindex];
            foreach (var binding in data.Bindings)
            {
                binding.CurrentSamplerState = samplerstate;
                bindingcache[binding.ShaderIndex].SamplerStates[binding.Slot] = samplerstate;
                //if (IsActive) binding.Shader.SetSamplerState(Context, binding.Slot, binding.CurrentSamplerState);
            }
            if (IsActive) cachedirty = true;
        }

        #endregion

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

            // Update all constant buffers
            foreach (ConstantBufferBinding binding in cbufferbindings)
            {
                if (binding.CurrentBlock != null) binding.CurrentBlock.Update();
            }

            // Undirty the cache
            UndirtyCache();

            // We're now active
            activepipeline = this;
        }

        private void UndirtyCache()
        {
            // Make all resources active
            for (int i = 0; i < pipeline.Count; i++)
            {
                IShader shader = pipeline[i];
                ShaderBindingCache cache = bindingcache[i];
                shader.SetConstantBuffers(Context, cache.ConstantBuffers);
                shader.SetResources(Context, cache.Resources);
                shader.SetSamplerStates(Context, cache.SamplerStates);
            }

            // Done!
            cachedirty = false;
        }

        /// <summary>
        /// Call before making a draw call using this pipeline
        /// </summary>
        public void Use()
        {
            if (activepipeline != this) return;
            if (cachedirty) UndirtyCache();
        }

        /// <summary>
        /// Gets the names of all CBuffers in use by this pipeline
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetCBufferNames()
        {
            return cbuffers.Select((x) => x.Name);
        }

        /// <summary>
        /// Creates a parameter set for the specified CBuffer name
        /// </summary>
        /// <param name="cbuffername"></param>
        /// <returns></returns>
        public MaterialParameterSet CreateParameterSet(string cbuffername)
        {
            ConstantBufferData data = null;
            for (int i = 0; i < cbuffers.Count; i++)
            {
                data = cbuffers[i];
                if (data.Name == cbuffername)
                    break;
            }
            if (data.Name != cbuffername) return null;
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
