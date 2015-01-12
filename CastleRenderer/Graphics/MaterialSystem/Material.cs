using System;
using System.Collections.Generic;

using SlimDX.Direct3D11;

namespace CastleRenderer.Graphics.MaterialSystem
{
    public enum MaterialCullingMode { None, Frontface, Backface }

    /// <summary>
    /// Represents a material
    /// </summary>
    public class Material
    {
        // All parameter sets
        protected Dictionary<int, MaterialParameterBlock> parameterblocks;

        // All resources (textures etc)
        protected Dictionary<int, ShaderResourceView> resources;

        // All sampler states
        protected Dictionary<int, SamplerState> samplerstates;

        /// <summary>
        /// Gets the material pipeline associated with this material
        /// </summary>
        public MaterialPipeline Pipeline { get; private set; }

        /// <summary>
        /// Gets the material pipeline used for shadow mapping associated with this material
        /// </summary>
        public MaterialPipeline ShadowPipeline { get; set; }

        /// <summary>
        /// Gets or sets the desired culling mode of this material
        /// </summary>
        public MaterialCullingMode CullingMode { get; set; }

        /// <summary>
        /// Gets the name of this material
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Initialises a new instance of the Material class
        /// </summary>
        public Material(string name, MaterialPipeline pipeline)
        {
            // Store parameters
            Name = name;
            Pipeline = pipeline;

            // Initialise
            parameterblocks = new Dictionary<int, MaterialParameterBlock>();
            resources = new Dictionary<int, ShaderResourceView>();
            samplerstates = new Dictionary<int, SamplerState>();
        }

        /// <summary>
        /// Applies this material
        /// </summary>
        public void Apply()
        {
            foreach (var pair in parameterblocks)
                Pipeline.SetMaterialParameterBlock(pair.Key, pair.Value);
            foreach (var pair in resources)
                Pipeline.SetResource(pair.Key, pair.Value);
            foreach (var pair in samplerstates)
                Pipeline.SetSamplerState(pair.Key, pair.Value);
        }

        /// <summary>
        /// Sets a parameter block on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="set"></param>
        public void SetParameterBlock(string name, MaterialParameterBlock set)
        {
            parameterblocks[Pipeline.LookupMaterialParameterBlockIndex(name)] = set;
        }

        /// <summary>
        /// Gets a parameter set on this material
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public MaterialParameterBlock GetParameterBlock(string name)
        {
            MaterialParameterBlock set;
            if (parameterblocks.TryGetValue(Pipeline.LookupMaterialParameterBlockIndex(name), out set)) return set;
            return null;
        }

        /// <summary>
        /// Sets a resource on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resource"></param>
        public void SetResource(string name, ShaderResourceView resource)
        {
            resources[Pipeline.LookupResourceIndex(name)] = resource;
        }

        /// <summary>
        /// Gets a resource on this material
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ShaderResourceView GetResource(string name)
        {
            ShaderResourceView resource;
            if (!resources.TryGetValue(Pipeline.LookupResourceIndex(name), out resource)) return resource;
            return null;
        }

        /// <summary>
        /// Sets a sampler state on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="samplerstate"></param>
        public void SetSamplerState(string name, SamplerState samplerstate)
        {
            samplerstates[Pipeline.LookupSamplerStateIndex(name)] = samplerstate;
        }

        /// <summary>
        /// Sets a sampler state on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="samplerstate"></param>
        public SamplerState GetSamplerState(string name)
        {
            SamplerState samplerstate;
            if (!samplerstates.TryGetValue(Pipeline.LookupSamplerStateIndex(name), out samplerstate)) return samplerstate;
            return null;
        }
    }
}
