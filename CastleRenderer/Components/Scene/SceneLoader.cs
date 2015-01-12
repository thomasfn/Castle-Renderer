using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;
using CastleRenderer.Graphics.MaterialSystem;

using SlimDX;

using Newtonsoft.Json.Linq;

namespace CastleRenderer.Components
{
    /// <summary>
    /// Loads the scene from a file
    /// </summary>
    [RequiresComponent(typeof(MaterialSystem))] // NOTE: Dependance on MaterialSystem implies a dependence on Renderer
    public class SceneLoader : BaseComponent
    {
        private Dictionary<string, Actor> sceneactors;

        private Dictionary<string, Type> componenttypes;

        private struct ModelMesh
        {
            public Mesh Mesh;
            public string[] Materials;
        }

        private class Model
        {
            public ModelMesh[] Meshes { get; set; }
        }

        private Dictionary<string, Model> models;

        /// <summary>
        /// Called when this component is attached to an Actor
        /// </summary>
        public override void OnAttach()
        {
            // Call base
            base.OnAttach();

            // Load all component types
            componenttypes = new Dictionary<string, Type>();
            Type basetype = typeof(BaseComponent);
            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany((a) => a.GetTypes())
                .Where((t) => !t.IsAbstract && basetype.IsAssignableFrom(t))
                )
            {
                componenttypes.Add(type.Name, type);
            }

            // Initialise
            sceneactors = new Dictionary<string, Actor>();
        }

        /// <summary>
        /// Creates a component on the specified actor
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private BaseComponent CreateComponent(Actor actor, string name)
        {
            Type type;
            if (!componenttypes.TryGetValue(name, out type)) return null;
            return actor.AddComponent(type);
        }

        /// <summary>
        /// Loads a scene from the specified file
        /// </summary>
        /// <param name="filename"></param>
        public bool LoadSceneFromFile(string filename)
        {
            // Check file exists
            if (!File.Exists(filename)) return false;

            // Load it
            string content = File.ReadAllText(filename);

            // Parse into JSON
            JObject root = JObject.Parse(content);

            // Locate the include array
            if (root["Include"] != null)
            {
                // Loop each item
                JArray jinclude = root["Include"] as JArray;
                for (int i = 0; i < jinclude.Count; i++)
                {
                    // Include it
                    string name = (string)jinclude[i];
                    if (!LoadSceneFromFile(name))
                    {
                        Console.WriteLine("Failed to load include scene file '{0}'.", name);
                        return false;
                    }
                }
            }

            // Locate the actors array
            if (root["Actors"] == null) return true;
            JToken actors = root["Actors"];
            int cnt = 0;
            sceneactors["Root"] = Owner;
            models = new Dictionary<string, Model>();
            foreach (var item in actors)
            {
                if (LoadActor(item))
                    cnt++;
            }
            actors = null;
            Console.WriteLine("Loaded {0} actors from scene file '{1}'.", cnt, filename);

            // Success
            return true;
        }

        private bool LoadActor(JToken source)
        {
            // Create the actor
            Actor actor = new Actor(Owner.MessagePool);

            // Assign the name
            if (source["Name"] != null)
            {
                actor.Name = (string)source["Name"];
                if (sceneactors.ContainsKey(actor.Name))
                    Console.WriteLine("Warning - more than one actors hold the same name '{0}'!", actor.Name);
                else
                    sceneactors.Add(actor.Name, actor);
            }
            else
                actor.Name = "Untitled Scene Object";

            // Assign the parent
            if (source["Parent"] != null)
            {
                string parentname = (string)source["Parent"];
                Actor parentactor;
                if (!sceneactors.TryGetValue(parentname, out parentactor))
                {
                    Console.WriteLine("Tried to parent '{0}' to unknown actor '{1}'!", parentname);
                    return false;
                }
            }
            else
                actor.Parent = Owner;

            // Read all components
            if (source["Components"] != null)
            {
                foreach (JToken obj in source["Components"])
                {
                    JProperty jprop = obj as JProperty;
                    if (!LoadComponent(actor, jprop.Name, jprop.Value))
                    {
                        Console.WriteLine("Failed to load components on actor '{0}'!", actor.Name);
                        return false;
                    }
                }
            }
            actor.Init();

            // Success
            return true;
        }

        private bool LoadComponent(Actor actor, string type, JToken source)
        {
            // Add the component
            BaseComponent component = CreateComponent(actor, type);
            if (component == null)
            {
                Console.WriteLine("Unknown component type '{0}' on actor '{1}'!", actor.Name);
                return false;
            }
            Type ctype = component.GetType();

            // Read all other fields
            foreach (JToken obj in source)
            {
                JProperty p = obj as JProperty;
                PropertyInfo property = ctype.GetProperty(p.Name, BindingFlags.Instance | BindingFlags.Public);
                if (property == null)
                    Console.WriteLine("Warning - unknown property '{0}' on component '{1}' on actor '{2}'!", p.Name, ctype.Name, actor.Name);
                else if (property.SetMethod == null)
                    Console.WriteLine("Warning - property '{0}' has no set method on component '{1}' on actor '{2}'!", p.Name, ctype.Name, actor.Name);
                else
                {
                    object value = TokenToTypeSafe(p.Value, property.PropertyType);
                    if (value != null)
                        property.SetValue(component, value);
                }
            }

            // Loaded
            return true;
        }

        private object TokenToTypeSafe(JToken token, Type desiredtype)
        {
            object val = TokenToType(token, desiredtype);
            if (val == null)
            {
                Console.WriteLine("Warning - unable to convert json token type '{1}' to type '{0}'!", desiredtype.Name, token.Type);
                return null;
            }
            else if (!desiredtype.IsAssignableFrom(val.GetType()))
            {
                Console.WriteLine("Warning - TokenToType produced wrong type '{0}' (desired type is {1}, token type is {2})", val.GetType().Name, desiredtype.Name, token.Type);
                return null;
            }
            else
                return val;
        }

        private object TokenToType(JToken token, Type desiredtype)
        {
            // Check certain desired types
            if (desiredtype == typeof(Vector2))
                return ParseVector2(token);
            else if (desiredtype == typeof(Vector3))
                return ParseVector3(token);
            else if (desiredtype == typeof(Vector4))
                return ParseVector4(token);
            else if (desiredtype == typeof(Quaternion))
            {
                Vector3 vec = ParseVector3(token);
                return Quaternion.RotationYawPitchRoll(vec.Y, vec.X, vec.Z);
            }
            else if (desiredtype == typeof(Plane))
            {
                Vector4 vec = ParseVector4(token);
                return new Plane(vec);
            }
            else if (desiredtype == typeof(SlimDX.Direct3D11.Viewport))
            {
                Vector4 vec = ParseVector4(token);
                return new SlimDX.Direct3D11.Viewport(vec.X, vec.Y, vec.Z, vec.W);
            }
            else if (desiredtype == typeof(Color3))
                return ParseColor3(token);
            else if (desiredtype == typeof(Color4))
                return ParseColor4(token);

            // Check token type
            switch (token.Type)
            {
                case JTokenType.String:
                    if (desiredtype == typeof(string))
                        return (string)token;
                    else if (desiredtype.IsEnum)
                        return Enum.Parse(desiredtype, (string)token);
                    else if (typeof(Material).IsAssignableFrom(desiredtype))
                        return GetMaterial((string)token);
                    else if (typeof(Actor).IsAssignableFrom(desiredtype))
                    {
                        Actor theactor;
                        if (!sceneactors.TryGetValue((string)token, out theactor)) return null;
                        return theactor;
                    }
                    else if (desiredtype == typeof(Material[]))
                    {
                        string name = (string)token;
                        string[] args = name.Split(':');
                        if (args.Length <= 1)
                        {
                            Console.WriteLine("Failed to parse special material '{0}'!", name);
                            return null;
                        }
                        string cmd = args[0].Trim().ToLowerInvariant();
                        string arg = args[1].Trim().ToLowerInvariant();
                        switch (cmd)
                        {
                            case "model":
                                Model model = GetModel(arg);
                                if (model == null)
                                {
                                    Console.WriteLine("Failed to parse material source '{0}' (unknown model '{1}')", name, cmd);
                                    return null;
                                }
                                int meshindex = 0;
                                if (args.Length >= 3)
                                {
                                    if (!int.TryParse(args[2], out meshindex))
                                    {
                                        Console.WriteLine("Failed to parse material source '{0}' (bad argument #2 '{1}')!", name, args[2]);
                                        return null;
                                    }
                                }
                                string[] matnames = model.Meshes[meshindex].Materials;
                                Material[] mats = new Material[matnames.Length];
                                for (int i = 0; i < mats.Length; i++)
                                {
                                    mats[i] = GetMaterial(matnames[i]);
                                }
                                return mats;
                            default:
                                Console.WriteLine("Failed to parse material source '{0}' (unknown source type '{1}')", name, cmd);
                                return null;
                        }
                    }
                    else if (typeof(Mesh).IsAssignableFrom(desiredtype))
                        return GetMesh((string)token);
                    else if (typeof(Actor).IsAssignableFrom(desiredtype))
                    {
                        Actor actor;
                        if (!sceneactors.TryGetValue((string)token, out actor))
                        {
                            return null;
                        }
                        return actor;
                    }
                    else
                        return null;
                case JTokenType.Boolean:
                    if (desiredtype == typeof(bool))
                        return (bool)token;
                    else
                        return null;
                case JTokenType.Integer:
                    if (desiredtype == typeof(int))
                        return (int)token;
                    else if (desiredtype == typeof(float))
                        return Convert.ToSingle((int)token);
                    else if (desiredtype == typeof(double))
                        return Convert.ToDouble((int)token);
                    else
                        return null;
                case JTokenType.Float:
                    if (desiredtype == typeof(float))
                        return (float)token;
                    else if (desiredtype == typeof(int))
                        return Convert.ToInt32((float)token);
                    else if (desiredtype == typeof(double))
                        return Convert.ToDouble((float)token);
                    else
                        return null;
                case JTokenType.Array:
                    JArray jarr = token as JArray;
                    if (desiredtype.IsArray)
                    {
                        Type etype = desiredtype.GetElementType();
                        Array arr = Activator.CreateInstance(desiredtype, new object[] { jarr.Count }) as Array;
                        for (int i = 0; i < jarr.Count; i++)
                        {
                            object val = TokenToTypeSafe(jarr[i], etype);
                            if (val != null) arr.SetValue(val, i);
                        }
                        return arr;
                    }
                    else
                        return null;
                case JTokenType.Object:
                    JObject jobj = token as JObject;
                    if (typeof(RenderTarget).IsAssignableFrom(desiredtype))
                    {
                        RenderTarget rt = Owner.GetComponent<Renderer>().CreateRenderTarget((int)jobj["SampleCount"], (int)jobj["Width"], (int)jobj["Height"], (string)jobj["Name"]);
                        if ((bool)jobj["TextureComponent"]) rt.AddTextureComponent();
                        if ((bool)jobj["DepthComponent"]) rt.AddDepthComponent();
                        return rt;
                    }
                    else if (desiredtype.IsGenericType)
                    {
                        // Trying to see if it's a Dictionary<string, T>
                        //Type genbase = desiredtype.GetGenericTypeDefinition();
                        //Type[] genargs = desiredtype.GetGenericArguments();
                        throw new NotImplementedException();
                        // TODO: This!
                    }
                    else
                        return null;
            }
            return null;
        }

        private Material GetMaterial(string name)
        {
            return Owner.GetComponent<MaterialSystem>().GetMaterial(name);
        }

        private Mesh GetMesh(string name)
        {
            string[] args = name.Split(':');
            if (args.Length <= 1)
            {
                Console.WriteLine("Failed to parse mesh '{0}'!", name);
                return null;
            }
            string cmd = args[0].Trim().ToLowerInvariant();
            string arg = args[1].Trim().ToLowerInvariant();
            switch (cmd)
            {
                case "primitive":
                case "prim":
                    switch (arg)
                    {
                        case "cube":
                            return MeshBuilder.BuildCube(Matrix.Identity);
                        case "sphere":
                            int divs = 8;
                            if (args.Length >= 3)
                            {
                                if (!int.TryParse(args[2], out divs))
                                {
                                    Console.WriteLine("Failed to parse mesh '{0}' (bad argument #2 '{1}')!", name, args[2]);
                                    return null;
                                }
                            }
                            return MeshBuilder.BuildSphere(1.0f, divs, true, true);
                        case "plane":
                            return MeshBuilder.BuildPlane(true, true);
                        default:
                            Console.WriteLine("Failed to parse mesh '{0}' (unknown primitive type '{1}')!", name, arg);
                            return null;
                    }
                case "model":
                    Model model = GetModel(arg);
                    if (model == null) return null;
                    int meshindex = 0;
                    if (args.Length >= 3)
                    {
                        if (!int.TryParse(args[2], out meshindex))
                        {
                            Console.WriteLine("Failed to parse mesh '{0}' (bad argument #2 '{1}')!", name, args[2]);
                            return null;
                        }
                    }
                    int subdiv = 0;
                    if (args.Length >= 4)
                    {
                        if (!int.TryParse(args[3], out subdiv))
                        {
                            Console.WriteLine("Failed to parse mesh '{0}' (bad argument #3 '{1}')!", name, args[3]);
                            return null;
                        }
                    }
                    float minarea = 0.0f;
                    if (args.Length >= 5)
                    {
                        if (!float.TryParse(args[4], out minarea))
                        {
                            Console.WriteLine("Failed to parse mesh '{0}' (bad argument #4 '{1}')!", name, args[4]);
                            return null;
                        }
                    }
                    Mesh themesh = model.Meshes[meshindex].Mesh;
                    while (subdiv-- > 0)
                    {
                        MeshBuilder builder = new MeshBuilder(themesh);
                        builder.Subdivide(minarea);
                        themesh = builder.Build();
                    }
                    return themesh;
                default:
                    Console.WriteLine("Failed to parse mesh '{0}' (unknown mesh type '{1}')!", name, cmd);
                    return null;
            }
        }

        private Model GetModel(string name)
        {
            Model model;
            if (!models.TryGetValue(name, out model))
            {
                if (!File.Exists(name))
                {
                    Console.WriteLine("Failed to parse mesh '{0}' (file not found)");
                    return null;
                }
                Console.WriteLine("Loading model '{0}'...", name);
                SBMLoader loader = new SBMLoader(name);
                string err;
                if (!loader.Load(out err))
                {
                    Console.WriteLine("Failed to load model '{0}' ({1})", name, err);
                    return null;
                }
                model = new Model();
                model.Meshes = new ModelMesh[loader.MeshCount];
                for (int i = 0; i < loader.MeshCount; i++)
                {
                    loader.GetMesh(i, out model.Meshes[i].Mesh, out model.Meshes[i].Materials);
                }
                models.Add(name, model);
            }
            return model;
        }

        private static Vector2 ParseVector2(JToken source)
        {
            return new Vector2((float)source[0], (float)source[1]);
        }

        private static Vector3 ParseVector3(JToken source)
        {
            return new Vector3((float)source[0], (float)source[1], (float)source[2]);
        }

        private static Vector4 ParseVector4(JToken source)
        {
            return new Vector4((float)source[0], (float)source[1], (float)source[2], (float)source[3]);
        }

        private static Color3 ParseColor3(JToken source)
        {
            return new Color3((float)source[0], (float)source[1], (float)source[2]);
        }

        private static Color4 ParseColor4(JToken source)
        {
            return new Color4((float)source[3], (float)source[0], (float)source[1], (float)source[2]);
        }
    }
}