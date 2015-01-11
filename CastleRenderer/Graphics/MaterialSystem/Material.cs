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
        protected Dictionary<string, MaterialParameterBlock> parameterblocks;

        // All resources (textures etc)
        protected Dictionary<string, ShaderResourceView> resources;

        // All sampler states
        protected Dictionary<string, SamplerState> samplerstates;

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
            parameterblocks = new Dictionary<string, MaterialParameterBlock>();
            resources = new Dictionary<string, ShaderResourceView>();
            samplerstates = new Dictionary<string, SamplerState>();
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
            parameterblocks[name] = set;
        }

        /// <summary>
        /// Gets a parameter set on this material
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public MaterialParameterBlock GetParameterBlock(string name)
        {
            MaterialParameterBlock set;
            if (parameterblocks.TryGetValue(name, out set)) return set;
            return null;
        }

        /// <summary>
        /// Sets a resource on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resource"></param>
        public void SetResource(string name, ShaderResourceView resource)
        {
            resources[name] = resource;
        }

        /// <summary>
        /// Gets a resource on this material
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ShaderResourceView GetResource(string name)
        {
            ShaderResourceView resource;
            if (!resources.TryGetValue(name, out resource)) return resource;
            return null;
        }

        /// <summary>
        /// Sets a sampler state on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="samplerstate"></param>
        public void SetSamplerState(string name, SamplerState samplerstate)
        {
            samplerstates[name] = samplerstate;
        }

        /// <summary>
        /// Sets a sampler state on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="samplerstate"></param>
        public SamplerState GetSamplerState(string name)
        {
            SamplerState samplerstate;
            if (!samplerstates.TryGetValue(name, out samplerstate)) return samplerstate;
            return null;
        }
    }
}
