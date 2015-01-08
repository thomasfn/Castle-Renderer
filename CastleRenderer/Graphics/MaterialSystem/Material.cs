using System;
using System.Collections.Generic;

namespace CastleRenderer.Graphics.MaterialSystem
{
    public enum MaterialCullingMode { None, Forwardface, Backface }

    /// <summary>
    /// Represents a material
    /// </summary>
    public class Material
    {
        // All parameter sets
        protected Dictionary<string, MaterialParameterBlock> parameterblocks;

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
        }

        /// <summary>
        /// Applies this material
        /// </summary>
        public void Apply()
        {
            foreach (var pair in parameterblocks)
                Pipeline.SetMaterialParameterBlock(pair.Key, pair.Value);
            Pipeline.Activate();
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
    }
}
