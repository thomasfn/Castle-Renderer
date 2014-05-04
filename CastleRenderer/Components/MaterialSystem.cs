using System;
using System.IO;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;

using SlimDX;
using SlimDX.Direct3D11;

using Newtonsoft.Json.Linq;

namespace CastleRenderer.Components
{
    /// <summary>
    /// The system responsible for loading materials and shaders
    /// </summary>
    [RequiresComponent(typeof(Renderer))]
    [ComponentPriority(2)]
    public class MaterialSystem : BaseComponent
    {
        private Dictionary<string, Material> materialmap;
        private Dictionary<string, Shader> shadermap;
        private Dictionary<string, Texture2D> texturemap;

        /// <summary>
        /// Called when the initialise message has been received
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(InitialiseMessage))]
        public void OnInitialise(InitialiseMessage msg)
        {
            // Initialise material system
            Console.WriteLine("Initialising material system...");

            // Initialise maps
            materialmap = new Dictionary<string, Material>();
            shadermap = new Dictionary<string, Shader>();
            texturemap = new Dictionary<string, Texture2D>();
        }

        /// <summary>
        /// Gets a shader by the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Shader GetShader(string name)
        {
            // Check if it's already loaded
            Shader shader;
            if (shadermap.TryGetValue(name, out shader)) return shader;

            // Load it
            shader = LoadShader(name);
            if (shader == null) return null;
            shadermap.Add(name, shader);
            return shader;
        }

        private Shader LoadShader(string name)
        {
            // Check the file exists
            string filename = string.Format("shaders/{0}.fx", name);
            if (!File.Exists(filename)) return null;

            // Load it
            string source = File.ReadAllText(filename);
            Console.WriteLine("Loading shader {0}...", name);
            Shader shader = new Shader(Owner.GetComponent<Renderer>().Device);
            shader.VertexFromString(source);
            shader.PixelFromString(source);
            if (source.Contains("GShader")) shader.GeometryFromString(source);
            shader.EffectFromString(source);

            // Return it
            return shader;
        }

        /// <summary>
        /// Gets a material by the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Material GetMaterial(string name)
        {
            // Check if it's already loaded
            Material material;
            if (materialmap.TryGetValue(name, out material)) return material;

            // Load it
            material = LoadMaterial(name);
            if (material == null) return null;
            materialmap.Add(name, material);
            return material;
        }

        /// <summary>
        /// Creates a material with the given name and shader
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shadername"></param>
        /// <returns></returns>
        public Material CreateMaterial(string name, string shadername)
        {
            // Check if it's already loaded
            Material material;
            if (materialmap.TryGetValue(name, out material)) return material;

            // Create it
            material = new Material(Owner.GetComponent<Renderer>());
            material.Name = name;
            material.Shader = GetShader(shadername);
            materialmap.Add(name, material);
            return material;
        }

        private Material LoadMaterial(string name)
        {
            // Check the file exists
            string filename = string.Format("materials/{0}.txt", name);
            if (!File.Exists(filename))
            {
                Console.WriteLine("Failed to load material {0} (file not found)!", name);
                return null;
            }

            // Load it
            string source = File.ReadAllText(filename);
            Console.WriteLine("Loading material {0}...", name);
            JObject root = JObject.Parse(source);

            // Define material and loop each parameter
            Material material = new Material(Owner.GetComponent<Renderer>());
            material.Name = name;
            foreach (var obj in root)
            {
                switch (obj.Key)
                {
                    case "shader":
                        material.Shader = GetShader((string)obj.Value);
                        if (material.Shader == null)
                        {
                            Console.WriteLine("Error loading material {0} - failed to load shader {1}!", name, (string)obj.Value);
                            return null;
                        }
                        break;
                    case "shadowshader":
                        material.ShadowShader = GetShader((string)obj.Value);
                        if (material.ShadowShader == null)
                        {
                            Console.WriteLine("Error loading material {0} - failed to load shader {1}!", name, (string)obj.Value);
                            return null;
                        }
                        break;
                    case "culling":
                        string val = (string)obj.Value;
                        switch (val)
                        {
                            case "frontface":
                                material.CullingMode = Owner.GetComponent<Renderer>().Culling_Frontface;
                                break;
                            case "backface":
                                material.CullingMode = Owner.GetComponent<Renderer>().Culling_Backface;
                                break;
                            case "none":
                                material.CullingMode = Owner.GetComponent<Renderer>().Culling_None;
                                break;
                        }
                        break;
                    default:
                        if (obj.Value.Type == JTokenType.Array)
                        {
                            int cnt = (obj.Value as JArray).Count;
                            if (cnt == 3)
                                material.SetParameter(obj.Key, new Vector3((float)obj.Value[0], (float)obj.Value[1], (float)obj.Value[2]));
                            else if (cnt == 2)
                                material.SetParameter(obj.Key, new Vector2((float)obj.Value[0], (float)obj.Value[1]));
                            else
                                Console.WriteLine("Parameter '{0}' in material {1} is not understood!", obj.Key, name);
                        }
                        else if (obj.Value.Type == JTokenType.Float)
                            material.SetParameter(obj.Key, (float)obj.Value);
                        else if (obj.Value.Type == JTokenType.String)
                        {
                            string value = (string)obj.Value;
                            if (value.Length == 0)
                                Console.WriteLine("Parameter '{0}' in material {1} is an emoty string!", obj.Key, name);
                            else
                            {
                                // What type of object is it trying to indicate?
                                if (value[0] == '$')
                                {
                                    // Is it a scripted texture?
                                    string texturedata = value.Substring(1);
                                    string[] script = null;
                                    if (texturedata.Contains("$"))
                                    {
                                        string[] texturearray = texturedata.Split('$');
                                        if (texturearray.Length != 2)
                                        {
                                            Console.WriteLine("Parameter '{0}' in material {1} has bad scripted texture data!");
                                        }
                                        else
                                        {
                                            texturedata = texturearray[1];
                                            if (texturearray[0].Contains(":"))
                                                script = texturearray[0].Split(':');
                                            else
                                                script = new string[] { texturearray[0] };
                                        }
                                    }

                                    // Texture
                                    Texture2D tex = GetTexture(texturedata);
                                    if (tex == null)
                                        Console.WriteLine("Parameter '{0}' in material {1} referenced an invalid texture!", obj.Key, name);
                                    else
                                    {
                                        // Apply script
                                        if (script != null && script.Length >= 1)
                                        {
                                            switch (script[0])
                                            {
                                                case "normalmap_from_heightmap":
                                                    var device = Owner.GetComponent<Renderer>().Device;
                                                    Texture2D output = new Texture2D(device, tex.Description);
                                                    output.DebugName = tex.DebugName + "_normals";
                                                    float height = 1.0f;
                                                    if (script.Length > 1) float.TryParse(script[1], out height);
                                                    Result result = Texture2D.ComputeNormalMap(device.ImmediateContext, tex, output, (NormalMapFlags)0, Channel.Red, height);
                                                    if (result.IsFailure)
                                                        Console.WriteLine("Failed to convert heightmap at '{0}' to normalmap in material {1}!", texturedata, name);
                                                    else
                                                        tex = output;
                                                    break;
                                            }
                                        }

                                        // Apply parameter
                                        material.SetParameter(obj.Key, tex);
                                    }
                                }
                                else if (value[0] == '@')
                                {
                                    // Sampler
                                    switch (value.Substring(1))
                                    {
                                        case "wrap_linear":
                                            material.SetParameter(obj.Key, Owner.GetComponent<Renderer>().Sampler_Wrap);
                                            break;
                                        case "clamp_linear":
                                            material.SetParameter(obj.Key, Owner.GetComponent<Renderer>().Sampler_Clamp_Linear);
                                            break;
                                    }
                                }
                                else
                                    Console.WriteLine("Parameter '{0}' in material {1} is not understood!", obj.Key, name);
                            }
                        }
                        break;
                }
            }
            

            // Return it
            return material;
        }

        /// <summary>
        /// Gets a texture by the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Texture2D GetTexture(string name)
        {
            // Check if it's already loaded
            Texture2D texture;
            if (texturemap.TryGetValue(name, out texture)) return texture;

            // Load it
            texture = LoadTexture(name);
            if (texture == null) return null;
            texturemap.Add(name, texture);
            return texture;
        }

        private Texture2D LoadTexture(string name)
        {
            // Check the file exists
            string filename = string.Format("textures/{0}", name);
            if (!File.Exists(filename)) return null;

            // Load it
            Texture2D tex = Texture2D.FromFile(Owner.GetComponent<Renderer>().Device, filename);
            tex.DebugName = name;
            return tex;
        }


    }
}