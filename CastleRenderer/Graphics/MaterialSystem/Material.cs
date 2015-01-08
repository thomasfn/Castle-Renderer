using System;
using System.Collections.Generic;

namespace CastleRenderer.Graphics.MaterialSystem
{
    /// <summary>
    /// Represents a material
    /// </summary>
    public class Material
    {
        // All parameter sets
        protected Dictionary<string, MaterialParameterSet> parametersets;

        /// <summary>
        /// Gets the material pipeline associated with this material
        /// </summary>
        public MaterialPipeline Pipeline { get; private set; }

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
            parametersets = new Dictionary<string, MaterialParameterSet>();
        }

        /// <summary>
        /// Applies this material
        /// </summary>
        public void Apply()
        {
            foreach (var pair in parametersets)
                Pipeline.SetMaterialParameterSet(pair.Key, pair.Value);
            Pipeline.Activate();
        }

        /// <summary>
        /// Sets a parameter set on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="set"></param>
        public void SetParameterSet(string name, MaterialParameterSet set)
        {
            parametersets[name] = set;
        }
    }
}
