using System;
using System.Collections.Generic;

using SlimDX.Direct3D11;

namespace CastleRenderer.Graphics.MaterialSystem
{
    public enum MaterialCullingMode { None, Frontface, Backface }

    public enum PipelineType { Main, ShadowMapping, _last }

    /// <summary>
    /// Represents a material
    /// </summary>
    public class Material
    {
        // All parameter sets
        protected MaterialParameterBlock[][] parameterblocks;

        // All resources (textures etc)
        protected ShaderResourceView[][] resources;

        // All sampler states
        protected SamplerState[][] samplerstates;

        /// <summary>
        /// Gets the material pipelines associated with this material
        /// </summary>
        public MaterialPipeline[] Pipelines { get; private set; }

        /// <summary>
        /// Gets the main material pipeline associated with this material
        /// </summary>
        public MaterialPipeline MainPipeline { get { return Pipelines[(int)PipelineType.Main]; } }

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
        public Material(string name, MaterialPipeline[] pipelines)
        {
            // Store parameters
            Name = name;
            Pipelines = pipelines;

            // Initialise
            parameterblocks = new MaterialParameterBlock[(int)PipelineType._last][];
            resources = new ShaderResourceView[(int)PipelineType._last][];
            samplerstates = new SamplerState[(int)PipelineType._last][];
            for (int i = 0; i < (int)PipelineType._last; i++)
            {
                MaterialPipeline pipeline = pipelines[i];
                if (pipeline != null)
                {
                    parameterblocks[i] = new MaterialParameterBlock[pipeline.MaterialParameterBlockCount];
                    resources[i] = new ShaderResourceView[pipeline.ResourceCount];
                    samplerstates[i] = new SamplerState[pipeline.SamplerStateCount];
                }
            }
        }

        /// <summary>
        /// Applies this material
        /// </summary>
        public void Apply()
        {
            Apply(PipelineType.Main);
        }

        /// <summary>
        /// Applies this material to the specified pipeline
        /// </summary>
        public void Apply(PipelineType pipelinetype)
        {
            MaterialPipeline pipeline = Pipelines[(int)pipelinetype];
            MaterialParameterBlock[] pblocks = parameterblocks[(int)pipelinetype];
            ShaderResourceView[] resviews = resources[(int)pipelinetype];
            SamplerState[] sstates = samplerstates[(int)pipelinetype];
            for (int i = 0; i < pblocks.Length; i++)
            {
                MaterialParameterBlock pblock = pblocks[i];
                if (pblock != null)
                    pipeline.SetMaterialParameterBlock(i, pblock);
            }
            for (int i = 0; i < resviews.Length; i++)
            {
                ShaderResourceView resview = resviews[i];
                if (resview != null)
                    pipeline.SetResource(i, resviews[i]);
            }
            for (int i = 0; i < sstates.Length; i++)
            {
                pipeline.SetSamplerState(i, sstates[i]);
            }
        }

        /// <summary>
        /// Sets a parameter block on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pblock"></param>
        public void SetParameterBlock(string name, MaterialParameterBlock pblock)
        {
            for (int i = 0; i < Pipelines.Length; i++)
            {
                MaterialPipeline pipeline = Pipelines[i];
                if (pipeline != null)
                {
                    int idx = pipeline.LookupMaterialParameterBlockIndex(name);
                    if (idx != -1) parameterblocks[i][idx] = pblock;
                }
            }
        }

        /// <summary>
        /// Gets a parameter set on this material
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public MaterialParameterBlock GetParameterBlock(string name)
        {
            for (int i = 0; i < Pipelines.Length; i++)
			{
                MaterialPipeline pipeline = Pipelines[i];
                if (pipeline != null)
                {
                    int idx = pipeline.LookupMaterialParameterBlockIndex(name);
                    if (idx != -1) return parameterblocks[i][idx];
                }
			}
            return null;
        }

        /// <summary>
        /// Sets a resource on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resource"></param>
        public void SetResource(string name, ShaderResourceView resource)
        {
            for (int i = 0; i < Pipelines.Length; i++)
            {
                MaterialPipeline pipeline = Pipelines[i];
                if (pipeline != null)
                {
                    int idx = pipeline.LookupResourceIndex(name);
                    if (idx != -1) resources[i][idx] = resource;
                }
            }
        }

        /// <summary>
        /// Gets a resource on this material
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ShaderResourceView GetResource(string name)
        {
            for (int i = 0; i < Pipelines.Length; i++)
            {
                MaterialPipeline pipeline = Pipelines[i];
                if (pipeline != null)
                {
                    int idx = pipeline.LookupResourceIndex(name);
                    if (idx != -1) return resources[i][idx];
                }
            }
            return null;
        }

        /// <summary>
        /// Sets a sampler state on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="samplerstate"></param>
        public void SetSamplerState(string name, SamplerState samplerstate)
        {
            for (int i = 0; i < Pipelines.Length; i++)
            {
                MaterialPipeline pipeline = Pipelines[i];
                if (pipeline != null)
                {
                    int idx = pipeline.LookupSamplerStateIndex(name);
                    if (idx != -1) samplerstates[i][idx] = samplerstate;
                }
            }
        }

        /// <summary>
        /// Sets a sampler state on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="samplerstate"></param>
        public SamplerState GetSamplerState(string name)
        {
            for (int i = 0; i < Pipelines.Length; i++)
            {
                MaterialPipeline pipeline = Pipelines[i];
                if (pipeline != null)
                {
                    int idx = pipeline.LookupSamplerStateIndex(name);
                    if (idx != -1) return samplerstates[i][idx];
                }
            }
            return null;
        }
    }
}
