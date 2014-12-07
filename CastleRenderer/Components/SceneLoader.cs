using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;

using SlimDX;

using Newtonsoft.Json.Linq;

namespace CastleRenderer.Components
{
    /// <summary>
    /// Loads the scene from a file
    /// </summary>
    [RequiresComponent(typeof(MaterialSystem))] // NOTE: Dependance on MaterialSystem implies a dependance on Renderer
    public class SceneLoader : BaseComponent
    {
        private Dictionary<string, Actor> actors;

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

            // Locate the objects array
            if (root["objects"] == null) return true;
            JToken objects = root["objects"];
            int cnt = 0;
            actors = new Dictionary<string, Actor>();
            actors["root"] = Owner;
            foreach (var item in objects)
            {
                if (LoadObject(item))
                    cnt++;
            }
            actors = null;
            Console.WriteLine("Loaded {0} objects from scene file.", cnt);

            // Success
            return true;
        }

        private bool LoadObject(JToken source)
        {
            // Check comment
            if (source.Type == JTokenType.Comment) return false;

            // Get the type
            string type = (string)source["type"];

            // Branch off
            Actor actor = null;
            switch (type)
            {
                case "camera":
                    actor = LoadCamera(source);
                    break;
                case "primitive":
                    actor = LoadPrimitive(source);
                    break;
                case "light":
                    actor = LoadLight(source);
                    break;
                case "model":
                    actor = LoadModel(source);
                    break;
                case "particlesystem":
                    actor = LoadParticleSystem(source);
                    break;
                case "ppeffect":
                    actor = LoadPPEffect(source);
                    break;
            }

            // Did we create an actor?
            if (actor != null)
            {
                // Load any extensions
                LoadExtensions(actor, source);

                // Set name
                if (source["name"] != null)
                    actor.Name = (string)source["name"];
                else
                    actor.Name = type;
                actors[actor.Name] = actor;

                // Parent it
                if (source["parent"] != null)
                {
                    Actor parentactor;
                    if (!actors.TryGetValue((string)source["parent"], out parentactor))
                    {
                        Console.WriteLine("Failed to parent object '{0}' to missing object '{1}' when loading scene.", actor.Name, (string)source["parent"]);
                        return false;
                    }
                    actor.Parent = parentactor;
                }
                else
                    actor.Parent = Owner;
                return true;
            }
            else
            {
                // Let user know
                Console.WriteLine("Unrecognised object type '{0}' when loading scene.", type);
                return false;
            }
        }

        private Actor LoadCamera(JToken source)
        {
            // Create the actor
            Actor actor = new Actor(Owner.MessagePool);

            // Add the required components to it
            Transform transform = actor.AddComponent<Transform>();
            Camera camera = actor.AddComponent<Camera>();

            // Load generic transform
            LoadTransform(transform, source);

            // Load camera
            if (source["projectiontype"] != null)
            {
                string proj = (string)source["projectiontype"];
                if (proj == "perspective") camera.ProjectionType = CameraType.Perspective;
                if (proj == "orthographic") camera.ProjectionType = CameraType.Orthographic;
            }
            if (source["fov"] != null) camera.FoV = (float)source["fov"];
            if (source["nearz"] != null) camera.NearZ = (float)source["nearz"];
            if (source["farz"] != null) camera.FarZ = (float)source["farz"];
            if (source["viewport"] != null)
            {
                var viewport = new SlimDX.Direct3D11.Viewport(0.0f, 0.0f, (float)source["viewport"][0], (float)source["viewport"][1]);
                viewport.MaxZ = 1.0f;
                viewport.MinZ = 0.0f;
                camera.Viewport = viewport;
            }
            if (source["skybox"] != null)
            {
                camera.Background = BackgroundType.Skybox;
                camera.Skybox = Owner.GetComponent<MaterialSystem>().GetMaterial((string)source["skybox"]);
            }
            if (source["userendertarget"] != null && (bool)source["userendertarget"])
            {
                camera.Target = Owner.GetComponent<Renderer>().CreateRenderTarget(1, (int)camera.Viewport.Width, (int)camera.Viewport.Height, "camera_rt");
                camera.Target.AddDepthComponent();
                camera.Target.AddTextureComponent();
                camera.Target.Finish();
            }
            if (source["reflectedcamera"] != null && (bool)source["reflectedcamera"])
            {
                ReflectedCamera reflectedcam = actor.AddComponent<ReflectedCamera>();
                reflectedcam.ReflectionPlane = new Plane((float)source["reflectionplane"][0], (float)source["reflectionplane"][1], (float)source["reflectionplane"][2], (float)source["reflectionplane"][3]);
                reflectedcam.MainCamera = actors[(string)source["reflectionmimic"]];
                reflectedcam.ReflectionTarget = Owner.GetComponent<MaterialSystem>().GetMaterial((string)source["reflectiontarget"]);
            }
            if (source["enabled"] != null) camera.Enabled = (bool)source["enabled"];
            if (source["priority"] != null) camera.RenderPriority = (int)source["priority"];
            if (source["clip_plane"] != null)
            {
                camera.UseClipping = true;
                camera.ClipPlane = new Plane((float)source["clip_plane"][0], (float)source["clip_plane"][1], (float)source["clip_plane"][2], (float)source["clip_plane"][3]);
            }

            // Initialise and return
            actor.Init();
            return actor;
        }

        private Actor LoadPrimitive(JToken source)
        {
            // Create the actor
            Actor actor = new Actor(Owner.MessagePool);

            // Add the required components to it
            Transform transform = actor.AddComponent<Transform>();
            MeshRenderer renderer = actor.AddComponent<MeshRenderer>();

            // Load generic transform
            LoadTransform(transform, source);

            // Load primitive
            if (source["primitive"] != null)
            {
                bool usetexcoords = source["texcoords"] != null && (bool)source["texcoords"];
                bool usetangents = source["tangents"] != null && (bool)source["tangents"];
                string prim = (string)source["primitive"];
                switch (prim)
                {
                    case "sphere":
                        renderer.Mesh = MeshBuilder.BuildSphere(1.0f, 5, usetexcoords, usetangents);
                        break;
                    case "cube":
                        renderer.Mesh = MeshBuilder.BuildCube();
                        break;
                    case "fsquad":
                        renderer.Mesh = MeshBuilder.BuildFullscreenQuad();
                        break;
                    case "plane":
                        renderer.Mesh = MeshBuilder.BuildPlane(usetexcoords, usetangents);
                        break;
                }
            }
            if (source["material"] != null) renderer.Materials = new Material[] { Owner.GetComponent<MaterialSystem>().GetMaterial((string)source["material"]) };

            // Initialise and return
            actor.Init();
            return actor;
        }

        private Actor LoadLight(JToken source)
        {
            // Create the actor
            Actor actor = new Actor(Owner.MessagePool);

            // Add the required components to it
            Transform transform = actor.AddComponent<Transform>();
            Light light = actor.AddComponent<Light>();

            // Load generic transform
            LoadTransform(transform, source);

            // Load light type
            if (source["light"] != null)
            {
                string lighttype = (string)source["light"];
                switch (lighttype)
                {
                    case "ambient":
                        light.Type = LightType.Ambient;
                        break;
                    case "directional":
                        light.Type = LightType.Directional;
                        break;
                    case "point":
                        light.Type = LightType.Point;
                        break;
                    case "spot":
                        light.Type = LightType.Spot;
                        break;
                }
            }
            else
                light.Type = LightType.None;

            // Load generic light properties
            if (source["colour"] != null) light.Colour = ParseColor3(source["colour"]);
            if (source["range"] != null) light.Range = (float)source["range"];

            // Load shadow caster
            if (source["castshadows"] != null && (bool)source["castshadows"])
            {
                ShadowCaster caster = actor.AddComponent<ShadowCaster>();
                caster.Resolution = 512;
                if (source["shadowmapsize"] != null) caster.Resolution = (int)source["shadowmapsize"];
                if (source["shadowcasterscale"] != null) caster.Scale = (int)source["shadowcasterscale"];
            }

            // Initialise and return
            actor.Parent = Owner;
            actor.Init();
            return actor;
        }

        private Actor LoadModel(JToken source)
        {
            // Create the actor
            Actor actor = new Actor(Owner.MessagePool);

            // Add the required components to it
            Transform transform = actor.AddComponent<Transform>();

            // Load generic transform
            LoadTransform(transform, source);

            // Load model
            if (source["model"] != null)
            {
                string model = (string)source["model"];
                SBMLoader loader = new SBMLoader("models/" + model + ".sbm");
                string err;
                if (!loader.Load(out err))
                {
                    Console.WriteLine("Failed to load model '{0}'! ({1})", model, err);
                    return null;
                }

                // Is there more than 1 mesh?
                if (loader.MeshCount > 1)
                {
                    for (int i = 0; i < loader.MeshCount; i++)
                    {
                        Actor tmp = new Actor(Owner.MessagePool);
                        tmp.AddComponent<Transform>();
                        MeshRenderer renderer = tmp.AddComponent<MeshRenderer>();
                        Mesh mesh;
                        string[] materialnames;
                        loader.GetMesh(i, out mesh, out materialnames);
                        Material[] materials = new Material[materialnames.Length];
                        for (int j = 0; j < materials.Length; j++)
                        {
                            materials[j] = Owner.GetComponent<MaterialSystem>().GetMaterial(materialnames[j]);
                            if (materials[j] == null) Console.WriteLine("Failed to load material '{0}'!", materialnames[j]);
                        }
                        renderer.Mesh = mesh;
                        renderer.Materials = materials;
                        tmp.Parent = actor;
                        tmp.Init();
                    }
                }
                else
                {
                    MeshRenderer renderer = actor.AddComponent<MeshRenderer>();
                    Mesh mesh;
                    string[] materialnames;
                    loader.GetMesh(0, out mesh, out materialnames);
                    Material[] materials = new Material[materialnames.Length];
                    for (int j = 0; j < materials.Length; j++)
                        materials[j] = Owner.GetComponent<MaterialSystem>().GetMaterial(materialnames[j]);
                    renderer.Mesh = mesh;
                    renderer.Materials = materials;
                }
            }

            // Initialise and return
            actor.Init();
            return actor;
        }

        private Actor LoadParticleSystem(JToken source)
        {
            // Create the actor
            Actor actor = new Actor(Owner.MessagePool);

            // Add the required components to it
            Transform transform = actor.AddComponent<Transform>();
            ParticleSystem psystem = actor.AddComponent<ParticleSystem>();

            // Load generic transform
            LoadTransform(transform, source);

            // Load particle system settings
            if (source["particlecount"] != null) psystem.ParticleCount = (int)source["particlecount"];
            if (source["transfermode"] != null)
            {
                string transfermode = (string)source["transfermode"];
                switch (transfermode)
                {
                    case "add":
                        psystem.TransferMode = ParticleTransferMode.Add;
                        break;
                    case "alpha":
                        psystem.TransferMode = ParticleTransferMode.Alpha;
                        break;
                }
            }
            if (source["material"] != null) psystem.Material = Owner.GetComponent<MaterialSystem>().GetMaterial((string)source["material"]);
            if (source["particlelife"] != null) psystem.ParticleLife = (float)source["particlelife"];
            if (source["startcolour"] != null) psystem.StartColour = ParseColor4(source["startcolour"]);
            if (source["endcolour"] != null) psystem.EndColour = ParseColor4(source["endcolour"]);
            if (source["startsize"] != null) psystem.StartSize = (float)source["startsize"];
            if (source["endsize"] != null) psystem.EndSize = (float)source["endsize"];
            if (source["initialvelocity"] != null) psystem.InitialVelocity = ParseVector3(source["initialvelocity"]);
            if (source["randomvelocity"] != null) psystem.RandomVelocity = ParseVector3(source["randomvelocity"]);
            if (source["randomposition"] != null) psystem.RandomPosition = ParseVector3(source["randomposition"]);
            if (source["acceleration"] != null) psystem.Acceleration = ParseVector3(source["acceleration"]);
            if (source["emissionrate"] != null) psystem.EmissionRate = (int)source["emissionrate"];

            // Initialise and return
            actor.Parent = Owner;
            actor.Init();
            return actor;
        }

        private Actor LoadPPEffect(JToken source)
        {
            // Create the actor
            Actor actor = new Actor(Owner.MessagePool);

            // Add the required components to it
            PostProcessEffect effect = actor.AddComponent<PostProcessEffect>();

            // Load effect settings
            if (source["material"] != null) effect.Material = Owner.GetComponent<MaterialSystem>().GetMaterial((string)source["material"]);
            if (source["priority"] != null) effect.EffectPriority = (int)source["priority"];
            if (source["passes"] != null) effect.Passes = (int)source["passes"];

            // Initialise and return
            actor.Parent = Owner;
            actor.Init();
            return actor;
        }

        private static void LoadTransform(Transform transform, JToken source)
        {
            // Check for certain fields
            if (source["position"] != null) transform.LocalPosition = ParseVector3(source["position"]);
            if (source["forward"] != null) transform.LocalForward = ParseVector3(source["forward"]);
            if (source["rotation"] != null)
            {
                Vector3 euler = ParseVector3(source["rotation"]);
                transform.LocalRotation = Quaternion.RotationYawPitchRoll(euler.Y.ToRadians(), euler.X.ToRadians(), euler.Z.ToRadians());
            }
            if (source["scale"] != null) transform.LocalScale = ParseVector3(source["scale"]);
        }

        private static void LoadExtensions(Actor actor, JToken source)
        {
            // Check for flyable
            if (source["flyable"] != null && (bool)source["flyable"])
            {
                // Add component
                UserFlyable flyable = actor.AddComponent<UserFlyable>();
                if (source["flyable_maxspeed"] != null) flyable.MaxSpeed = (float)source["flyable_maxspeed"];
                if (source["flyable_accel"] != null) flyable.Acceleration = (float)source["flyable_accel"];
                if (source["flyable_drag"] != null) flyable.Resistance = (float)source["flyable_drag"];
                if (source["flyable_sens"] != null) flyable.Sensitivity = (float)source["flyable_sens"];
                actor.Init();
            }

            // Check for spinning
            if (source["spinning"] != null && (bool)source["spinning"])
            {
                // Add component
                Spinning spinning = actor.AddComponent<Spinning>();
                if (source["spinning_axis"] != null) spinning.Axis = ParseVector3(source["spinning_axis"]);
                if (source["spinning_speed"] != null) spinning.Speed = (float)source["spinning_speed"];
                actor.Init();
            }
        }



        private static Vector3 ParseVector3(JToken source)
        {
            return new Vector3((float)source[0], (float)source[1], (float)source[2]);
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