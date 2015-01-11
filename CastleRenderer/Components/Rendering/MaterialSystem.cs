using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;
using CastleRenderer.Graphics.MaterialSystem;

using SlimDX;
using SlimDX.Direct3D11;

using Newtonsoft.Json.Linq;

namespace CastleRenderer.Components
{
    using CastleRenderer.Graphics.Shaders;

    /// <summary>
    /// The system responsible for loading materials and shaders
    /// </summary>
    [RequiresComponent(typeof(Renderer))]
    [ComponentPriority(2)]
    public class MaterialSystem : BaseComponent
    {
        private Dictionary<string, Material> materialmap;
        private Dictionary<string, IShader> shadermap;
        private Dictionary<string, Texture2D> texturemap;
        private Dictionary<string, MaterialDefinition> definitionmap;

        private class ParameterSet
        {
            public Dictionary<string, object> Parameters { get; set; }

            public ParameterSet()
            {
                Parameters = new Dictionary<string, object>();
            }

            public ParameterSet(IDictionary<string, object> src)
            {
                Parameters = new Dictionary<string, object>(src);
            }
        }

        private class MaterialDefinition
        {
            public string Name { get; set; }
            public ParameterSet ParameterSet { get; set; }
            public MaterialGroup Group { get; set; }
            public Dictionary<string, ShaderResourceView> Resources { get; set; }
            public Dictionary<string, SamplerState> SamplerStates { get; set; }
        }

        private struct ParameterMapping
        {
            public string ParameterName;
            public string TargetSet;
            public string TargetName;
        }
        

        private class MaterialGroup
        {
            public string Name { get; set; }
            public string[] Shaders { get; set; }
            public ParameterMapping[] Mappings { get; set; }
            public MaterialDefinition[] Definitions { get; set; }
            public MaterialPipeline Pipeline { get; set; }
            public Dictionary<string, ParameterSet> ParameterSets { get; set; }
            public Dictionary<string, ShaderResourceView> Resources { get; set; }
            public Dictionary<string, SamplerState> SamplerStates { get; set; }
            public MaterialCullingMode CullingMode { get; set; }
        }

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
            shadermap = new Dictionary<string, IShader>();
            texturemap = new Dictionary<string, Texture2D>();
            definitionmap = new Dictionary<string, MaterialDefinition>();

            // Load all material groups
            foreach (string filename in Directory.GetFiles("materials").Where((f) => Path.GetExtension(f) == ".json"))
            {
                // Load text
                string matgrpname = Path.GetFileNameWithoutExtension(filename);
                string src = File.ReadAllText(filename);
                JObject root = JObject.Parse(src);

                // Verify
                if (root["Shaders"] == null)
                {
                    Console.WriteLine("Missing shaders for material group '{0}'", matgrpname);
                    continue;
                }
                if (root["ParameterSets"] == null)
                {
                    Console.WriteLine("Missing parameter sets for material group '{0}'", matgrpname);
                    continue;
                }
                if (root["Resources"] == null)
                {
                    Console.WriteLine("Missing resources for material group '{0}'", matgrpname);
                    continue;
                }
                if (root["SamplerStates"] == null)
                {
                    Console.WriteLine("Missing sampler states for material group '{0}'", matgrpname);
                    continue;
                }
                if (root["Mappings"] == null)
                {
                    Console.WriteLine("Missing mappings for material group '{0}'", matgrpname);
                    continue;
                }
                if (root["Materials"] == null)
                {
                    Console.WriteLine("Missing materials for material group '{0}'", matgrpname);
                    continue;
                }
                JArray jshaders = root["Shaders"] as JArray;
                JObject jpsets = root["ParameterSets"] as JObject;
                JObject jresources = root["Resources"] as JObject;
                JObject jsamplerstates = root["SamplerStates"] as JObject;
                JObject jmappings = root["Mappings"] as JObject;
                JObject jmaterials = root["Materials"] as JObject;

                // Create group
                MaterialGroup matgrp = new MaterialGroup();
                matgrp.Name = matgrpname;
                string[] shaders = new string[jshaders.Count];
                int i;
                for (i = 0; i < jshaders.Count; i++)
                {
                    shaders[i] = (string)jshaders[i];
                }
                matgrp.Shaders = shaders;
                ParameterMapping[] mappings = new ParameterMapping[jmappings.Count];
                i = 0;
                foreach (var pair in jmappings)
                {
                    string[] spl = ((string)pair.Value).Split('.');
                    mappings[i++] = new ParameterMapping { ParameterName = (string)pair.Key, TargetName = spl[1], TargetSet = spl[0] };
                }
                matgrp.Mappings = mappings;
                Dictionary<string, ParameterSet> psets = new Dictionary<string, ParameterSet>();
                foreach (var pair in jpsets)
                {
                    ParameterSet set = new ParameterSet();
                    foreach (var pair2 in pair.Value as JObject)
                    {
                        object val = TranslateJsonType(pair2.Value);
                        if (val == null)
                            Console.WriteLine("Unable to translate Json token '{0}' to a .NET type! ({1})", pair2.Value, matgrpname);
                        else
                            set.Parameters.Add((string)pair2.Key, val);
                    }
                    psets.Add((string)pair.Key, set);
                }
                matgrp.ParameterSets = psets;
                Dictionary<string, ShaderResourceView> resources = new Dictionary<string, ShaderResourceView>();
                foreach (var pair in jresources)
                {
                    ShaderResourceView resource = ParseResource(pair.Value);
                    if (resource != null) resources.Add((string)pair.Key, resource);
                }
                matgrp.Resources = resources;
                Dictionary<string, SamplerState> samplerstates = new Dictionary<string, SamplerState>();
                foreach (var pair in jsamplerstates)
                {
                    SamplerState samplerstate = ParseSamplerState(pair.Value);
                    if (samplerstate != null) samplerstates.Add((string)pair.Key, samplerstate);
                }
                matgrp.SamplerStates = samplerstates;
                MaterialDefinition[] definitions = new MaterialDefinition[jmaterials.Count];
                i = 0;
                foreach (var pair in jmaterials)
                {
                    MaterialDefinition matdef = new MaterialDefinition();
                    matdef.Name = (string)pair.Key;
                    matdef.Group = matgrp;
                    matdef.ParameterSet = new ParameterSet();
                    matdef.Resources = new Dictionary<string, ShaderResourceView>();
                    matdef.SamplerStates = new Dictionary<string, SamplerState>();
                    foreach (var pair2 in pair.Value as JObject)
                    {
                        string key = (string)pair2.Key;
                        string keylower = key.ToLowerInvariant();
                        if (keylower.StartsWith("resources."))
                        {
                            ShaderResourceView resource = ParseResource(pair2.Value);
                            if (resource != null) matdef.Resources.Add(key.Substring(10), resource);
                        }
                        else if (keylower.StartsWith("samplerstates."))
                        {
                            SamplerState samplerstate = ParseSamplerState(pair2.Value);
                            if (samplerstate != null) matdef.SamplerStates.Add(key.Substring(14), samplerstate);
                        }
                        else
                        {
                            object val = TranslateJsonType(pair2.Value);
                            if (val == null)
                                Console.WriteLine("Unable to translate Json token '{0}' to a .NET type! ({1})", pair2.Value, matgrpname);
                            else
                                matdef.ParameterSet.Parameters.Add(key, val);
                        }
                    }
                    definitions[i++] = matdef;
                    if (definitionmap.ContainsKey(matdef.Name))
                        Console.WriteLine("Material definition '{0}' in group {1} conflicts with definition of the same name from group {2}", matdef.Name, matgrpname, definitionmap[matdef.Name].Group.Name);
                    else
                        definitionmap.Add(matdef.Name, matdef);
                }
                matgrp.Definitions = definitions;

                if (root["CullingMode"] != null)
                {
                    string cullingmode = (string)root["CullingMode"];
                    switch (cullingmode.ToLowerInvariant())
                    {
                        case "frontface":
                        case "forwardface":
                            matgrp.CullingMode = MaterialCullingMode.Frontface;
                            break;
                        case "backface":
                        case "backwardface":
                            matgrp.CullingMode = MaterialCullingMode.Backface;
                            break;
                        case "none":
                            matgrp.CullingMode = MaterialCullingMode.None;
                            break;
                        default:
                            Console.WriteLine("Unknown culling mode '{0}' in group {1}!", cullingmode, matgrp.Name);
                            break;
                    }
                }
                else
                    matgrp.CullingMode = MaterialCullingMode.None;

                // Create pipeline
                MaterialPipeline pipeline = new MaterialPipeline(Owner.GetComponent<Renderer>().Device.ImmediateContext);
                foreach (string shadername in matgrp.Shaders)
                {
                    IShader shader = GetShader(shadername);
                    if (shader != null)
                    {
                        if (!pipeline.AddShader(shader))
                            Console.WriteLine("Failed to add shader '{0}' to pipeline in group {1}. Are there multiple shaders of the same type in the shaders list?", shadername, matgrp.Name);
                    }
                }
                string err;
                if (!pipeline.Link(out err))
                {
                    Console.WriteLine("Failed to create pipeline for material group '{0}' ({1})", matgrp.Name, err);
                }
                else
                    matgrp.Pipeline = pipeline;
            }
        }

        private ShaderResourceView ParseResource(JToken token)
        {
            if (token.Type != JTokenType.String)
            {
                Console.WriteLine("Unable to parse json token into resource ({0})", token);
                return null;
            }
            string str = (string)token;
            string[] args = str.Split(':');
            if (args.Length <= 1)
            {
                Console.WriteLine("Unable to parse json token into resource ({0})", token);
                return null;
            }
            string type = args[0].Trim().ToLowerInvariant();
            var renderer = Owner.GetComponent<Renderer>();
            switch (type)
            {
                case "texture":
                    string name = args[1];
                    Texture2D tex = GetTexture(name);
                    if (tex == null) return null;
                    return renderer.AcquireResourceView(tex);
            }
            Console.WriteLine("Unknown resource type '{0}'!", type);
            return null;
            /*// Is it a scripted texture?
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
                }*/
        }

        private SamplerState ParseSamplerState(JToken token)
        {
            if (token.Type != JTokenType.String)
            {
                Console.WriteLine("Unable to parse json token into sampler state ({0})", token);
                return null;
            }
            string str = ((string)token).ToLowerInvariant().Trim();
            var renderer = Owner.GetComponent<Renderer>();
            switch (str)
            {
                case "clamp":
                    return renderer.Sampler_Clamp;
                case "clamplinear":
                case "clamp_linear":
                    return renderer.Sampler_Clamp_Linear;
                case "clamppoint":
                case "clamp_point":
                    return renderer.Sampler_Clamp_Point;
                case "wrap":
                    return renderer.Sampler_Wrap;
            }
            Console.WriteLine("Unknown sampler state '{0}'!", str);
            return null;
        }

        private static object TranslateJsonType(JToken token)
        {
            return null;
        }

        /// <summary>
        /// Gets a shader by the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IShader GetShader(string name)
        {
            // Check if it's already loaded
            IShader shader;
            if (shadermap.TryGetValue(name, out shader)) return shader;

            // Load it
            shader = LoadShader(name);
            if (shader == null) return null;
            shadermap.Add(name, shader);
            return shader;
        }

        /// <summary>
        /// Loads the specified shader
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private IShader LoadShader(string name)
        {
            // Check the file exists
            string filename = string.Format("shaders/{0}.cso", name);
            if (!File.Exists(filename)) return null;

            // Load it
            byte[] raw = File.ReadAllBytes(filename);
            Console.WriteLine("Loading shader {0}...", name);

            // Create the shader
            IShader shader = null;
            var device = Owner.GetComponent<Renderer>().Device;
            string lowername = name.ToLowerInvariant();
            if (lowername.StartsWith("vertex"))
                shader = new VertexShader(device, raw, name);
            else if (lowername.StartsWith("pixel"))
                shader = new PixelShader(device, raw, name);

            // Error check
            if (shader == null)
            {
                Console.WriteLine("Failed to identify type of shader '{0}'!", name);
                return null;
            }

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
        /// Creates a material with the given name and set of shaders
        /// </summary>
        /// <param name="name"></param>
        /// <param name="shadername"></param>
        /// <returns></returns>
        public Material CreateMaterial(string name, params IShader[] shaders)
        {
            // Check if it's already loaded
            Material material;
            if (materialmap.TryGetValue(name, out material)) return material;

            // Create the pipeline
            MaterialPipeline pipeline = new MaterialPipeline(Owner.GetComponent<Renderer>().Device.ImmediateContext);
            foreach (IShader shader in shaders)
                pipeline.AddShader(shader);
            string err;
            if (!pipeline.Link(out err))
            {
                Console.WriteLine("Failed to create pipeline for material '{0}' ({1})", name, err);
                return null;
            }

            // Create it
            material = new Material(name, pipeline);
            materialmap.Add(name, material);
            return material;
        }

        private static void SetOnParameterSet(MaterialParameterSet pset, string key, object val)
        {
            if (val is float)
                pset.SetParameter(key, (float)val);
            else if (val is Vector2)
                pset.SetParameter(key, (Vector2)val);
            else if (val is Vector3)
                pset.SetParameter(key, (Vector3)val);
            else if (val is Vector4)
                pset.SetParameter(key, (Vector4)val);
            else
                Console.WriteLine("Warning - Unhandled parameter type in MaterialSystem.SetOnParameterSet! ({0})", val.GetType());
        }

        private Material LoadMaterial(string name)
        {
            // Check for a definition
            MaterialDefinition matdef;
            if (!definitionmap.TryGetValue(name, out matdef))
            {
                Console.WriteLine("Failed to load material '{0}' (not found in any material group)", name);
                return null;
            }

            // Create the material
            Material material = new Material(name, matdef.Group.Pipeline);
            material.CullingMode = matdef.Group.CullingMode;

            // Setup the parameter sets
            foreach (var pair in matdef.Group.ParameterSets)
            {
                MaterialParameterSet matpset = material.GetParameterBlock(pair.Key) as MaterialParameterSet;
                if (matpset == null)
                {
                    matpset = material.Pipeline.CreateParameterSet(pair.Key);
                    material.SetParameterBlock(pair.Key, matpset);
                }
                foreach (var pair2 in pair.Value.Parameters)
                    SetOnParameterSet(matpset, pair2.Key, pair2.Value);
            }
            foreach (var mapping in matdef.Group.Mappings)
            {
                MaterialParameterSet matpset = material.GetParameterBlock(mapping.TargetSet) as MaterialParameterSet;
                if (matpset == null)
                {
                    matpset = material.Pipeline.CreateParameterSet(mapping.TargetSet);
                    material.SetParameterBlock(mapping.TargetSet, matpset);
                }
                object val;
                if (matdef.ParameterSet.Parameters.TryGetValue(mapping.ParameterName, out val) && val != null)
                    SetOnParameterSet(matpset, mapping.TargetName, val);
            }

            // Setup resources and sampler states
            foreach (var pair in matdef.Group.Resources)
                material.SetResource(pair.Key, pair.Value);
            foreach (var pair in matdef.Group.SamplerStates)
                material.SetSamplerState(pair.Key, pair.Value);
            foreach (var pair in matdef.Resources)
                material.SetResource(pair.Key, pair.Value);
            foreach (var pair in matdef.SamplerStates)
                material.SetSamplerState(pair.Key, pair.Value);
            

            // Check the file exists
            /*string filename = string.Format("materials/{0}.txt", name);
            if (!File.Exists(filename))
            {
                Console.WriteLine("Failed to load material {0} (file not found)!", name);
                return null;
            }

            // Load it
            string source = File.ReadAllText(filename);
            Console.WriteLine("Loading material {0}...", name);
            JObject root = JObject.Parse(source);*/

            // Define material and loop each parameter
            /*Material material = new Material(Owner.GetComponent<Renderer>());
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
            }*/
            

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