using System;
using System.Collections.Generic;

using CastleRenderer.Components;

using SlimDX;
using SlimDX.Direct3D11;

namespace CastleRenderer.Graphics
{
    /// <summary>
    /// Represents a shader and a set of shader parameters
    /// </summary>
    public class Material
    {
        /// <summary>
        /// The name of this material (only used for debugging purposes)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The shader that this material uses
        /// </summary>
        public Shader Shader { get; set; }

        /// <summary>
        /// The shader that this material uses when rendering a shadow pass
        /// </summary>
        public Shader ShadowShader { get; set; }

        /// <summary>
        /// The type of culling to use (null means use default)
        /// </summary>
        public RasterizerState CullingMode { get; set; }

        /// <summary>
        /// The priority of this material in the render queue (lower = render first) (NOT IN USE YET)
        /// </summary>
        public int RenderPriority { get; set; }

        private Renderer renderer;

        private Dictionary<string, object> parameters;

        public Material(Renderer renderer)
        {
            parameters = new Dictionary<string, object>();
            this.renderer = renderer;
        }

        /// <summary>
        /// Sets a parameter on this material
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        public void SetParameter(string name, object data)
        {
            parameters[name] = data;
        }

        /// <summary>
        /// Applies this material's set of parameters to the material's shader
        /// </summary>
        public void Apply(bool shadowshader)
        {
            Shader shader = shadowshader ? ShadowShader : Shader;
            if (shader == null) return;
            foreach (var pair in parameters)
            {
                if (pair.Value is Texture2D)
                    renderer.BindShaderTexture(shader, pair.Key, pair.Value as Texture2D);
                else
                    shader.SetVariable(pair.Key, pair.Value);
            }
        }

        public override string ToString()
        {
            return "Material (" + Name + ")";
        }

    }
}
