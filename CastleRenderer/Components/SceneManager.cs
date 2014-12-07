using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;

using SlimDX;

namespace CastleRenderer.Components
{
    /// <summary>
    /// Represents the scene manager
    /// </summary>
    [ComponentPriority(3)]
    [RequiresComponent(typeof(MaterialSystem))] // Note: Dependance on MaterialSystem implies dependance on Renderer
    public class SceneManager : BaseComponent
    {
        private ResourcePool<RenderWorkItem> workitempool;
        private OrderedList<RenderWorkItem> renderqueue;
        private OrderedList<PostProcessEffect> effectqueue;

        private PopulateRenderQueue queuemsg;
        private PopulateCameraList cameramsg;
        private PopulateLightList lightmsg;
        private PopulateParticleSystemList psysmsg;

        private RenderTarget gbuffer;
        private int gbuffer_colour, gbuffer_position, gbuffer_normal, gbuffer_material;

        private RenderTarget lightaccum;
        private int lightaccum_diffuse, lightaccum_specular;

        private RenderTarget particleaccum;
        private int particleaccum_colour;

        private RenderTarget swapA;
        private int swapA_colour;

        private RenderTarget swapB;
        private int swapB_colour;

        private Material mat_blit, mat_blitlight;
        private Dictionary<LightType, Material> mat_lights;

        private Mesh mesh_fs;
        private Mesh mesh_skybox;

        /// <summary>
        /// Called when the initialise message has been received
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(InitialiseMessage))]
        public void OnInitialise(InitialiseMessage msg)
        {
            // Initialise scene manager
            Console.WriteLine("Initialising scene manager...");

            // Initialise messages
            queuemsg = new PopulateRenderQueue();
            queuemsg.SceneManager = this;
            cameramsg = new PopulateCameraList();
            cameramsg.Cameras = new OrderedList<Camera>(new CameraComparer());
            cameramsg.ShadowCasters = new HashSet<ShadowCaster>();
            lightmsg = new PopulateLightList();
            lightmsg.Lights = new OrderedList<Light>(new LightComparer());
            psysmsg = new PopulateParticleSystemList();
            psysmsg.ParticleSystems = new List<ParticleSystem>();

            // Create render queue
            workitempool = new ResourcePool<RenderWorkItem>();
            renderqueue = new OrderedList<RenderWorkItem>(new RenderWorkItemComparer());
            effectqueue = new OrderedList<PostProcessEffect>(new PostProcessEffectComparer());

            // Setup GBuffer
            Renderer renderer = Owner.GetComponent<Renderer>();
            gbuffer = renderer.CreateRenderTarget(1, "GBuffer");
            gbuffer.ClearColour = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
            gbuffer.AddDepthComponent();
            gbuffer_colour = gbuffer.AddTextureComponent();
            gbuffer_normal = gbuffer.AddTextureComponent(SlimDX.DXGI.Format.R32G32B32A32_Float);
            gbuffer_position = gbuffer.AddTextureComponent(SlimDX.DXGI.Format.R32G32B32A32_Float);
            gbuffer_material = gbuffer.AddTextureComponent(SlimDX.DXGI.Format.R32G32B32A32_Float);
            gbuffer.Finish();

            // Setup light accumulation buffer
            lightaccum = renderer.CreateRenderTarget(1, "LightAccum");
            lightaccum.ClearColour = new Color4(1.0f, 0.0f, 0.0f, 0.0f);
            lightaccum_diffuse = lightaccum.AddTextureComponent(SlimDX.DXGI.Format.R32G32B32A32_Float);
            lightaccum_specular = lightaccum.AddTextureComponent(SlimDX.DXGI.Format.R32G32B32A32_Float);
            lightaccum.Finish();

            // Setup particle accumulation buffer
            particleaccum = renderer.CreateRenderTarget(1, "ParticleAccum");
            particleaccum.ClearColour = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
            particleaccum_colour = particleaccum.AddTextureComponent();
            particleaccum.Finish();

            // Setup swap buffers
            swapA = renderer.CreateRenderTarget(1, "SwapA");
            swapA_colour = swapA.AddTextureComponent();
            swapA.Finish();
            swapB = renderer.CreateRenderTarget(1, "SwapB");
            swapB_colour = swapB.AddTextureComponent();
            swapB.Finish();

            // Setup materials
            MaterialSystem matsys = Owner.GetComponent<MaterialSystem>();
            mat_blit = matsys.CreateMaterial("blit", "blit");
            mat_blit.SetParameter("smpTexture", renderer.Sampler_Clamp);
            mat_blitlight = matsys.CreateMaterial("blit_light", "blit_light");
            mat_blitlight.SetParameter("smpTexture", renderer.Sampler_Clamp);
            mat_blitlight.SetParameter("texColour", gbuffer.GetTexture(gbuffer_colour));
            mat_blitlight.SetParameter("texDiffuseLight", lightaccum.GetTexture(lightaccum_diffuse));
            mat_blitlight.SetParameter("texSpecularLight", lightaccum.GetTexture(lightaccum_specular));

            // Setup lights
            mat_lights = new Dictionary<LightType, Material>();
            mat_lights.Add(LightType.Ambient, matsys.CreateMaterial("light_ambient", "light_ambient"));
            mat_lights.Add(LightType.Directional, matsys.CreateMaterial("light_directional", "light_directional"));
            mat_lights.Add(LightType.Point, matsys.CreateMaterial("light_point", "light_point"));
            foreach (Material mat in mat_lights.Values)
            {
                mat.SetParameter("texNormal", gbuffer.GetTexture(gbuffer_normal));
                mat.SetParameter("texPosition", gbuffer.GetTexture(gbuffer_position));
                mat.SetParameter("texMaterial", gbuffer.GetTexture(gbuffer_material));
                mat.SetParameter("smpTexture", renderer.Sampler_Clamp);
            }

            // Setup meshes
            mesh_fs = MeshBuilder.BuildFullscreenQuad();
            mesh_skybox = MeshBuilder.BuildCube();
        }

        /// <summary>
        /// Queues a draw order
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="submesh"></param>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        public void QueueDraw(Mesh mesh, int submesh, Material material, Matrix transform)
        {
            // Create the work item
            RenderWorkItem item = workitempool.Request();
            item.Mesh = mesh;
            item.SubmeshIndex = submesh;
            item.Material = material;
            item.Transform = transform;

            // Add to queue
            renderqueue.Add(item);
        }

        /// <summary>
        /// Queues a PP effect
        /// </summary>
        /// <param name="effect"></param>
        public void QueueEffect(PostProcessEffect effect)
        {
            // Add to queue
            effectqueue.Add(effect);
        }

        /// <summary>
        /// Called when it's time to render the frame
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(RenderMessage))]
        public void OnRender(RenderMessage msg)
        {
            // Clear the render queue ready for new work
            ClearRenderQueue();

            // Populate render queue with new work items
            Owner.MessagePool.SendMessage(queuemsg);

            // Populate camera list with active cameras
            cameramsg.Cameras.Clear();
            cameramsg.ShadowCasters.Clear();
            Owner.MessagePool.SendMessage(cameramsg);

            // Populate light list with active lights
            lightmsg.Lights.Clear();
            Owner.MessagePool.SendMessage(lightmsg);

            // Populate psystem list with active particle systems
            psysmsg.ParticleSystems.Clear();
            Owner.MessagePool.SendMessage(psysmsg);

            // Cache the renderer
            Renderer renderer = Owner.GetComponent<Renderer>();

            // Setup initial state
            renderer.Depth = renderer.Depth_Enabled;
            renderer.Blend = renderer.Blend_Opaque;
            renderer.Culling = renderer.Culling_None;

            // Loop each shadow caster
            foreach (ShadowCaster caster in cameramsg.ShadowCasters)
            {
                // Get the projection view matrix and transform
                Matrix projview = caster.ProjectionView;
                Transform transform = caster.Owner.GetComponent<Transform>();
                Vector3 position = transform.Position;

                // Does it belong to a directional-light?
                /*Light light = caster.Owner.GetComponent<Light>();
                if (light.Type == LightType.Directional)
                {
                    // We're responsible for positioning this light such that the shadow maps across the whole scene
                    
                }*/

                // Bind the caster render target
                caster.RT.Bind();
                caster.RT.Clear();

                // Draw all items in the render queue to the shadowmap
                Material activematerial = null;
                foreach (RenderWorkItem item in renderqueue)
                {
                    // Set the material
                    if (activematerial != item.Material)
                    {
                        activematerial = item.Material;
                        if (activematerial.ShadowShader != null)
                            activematerial.ShadowShader.SetVariable("light_position", position);
                        renderer.SetActiveMaterial(activematerial, true);
                    }

                    // Draw it
                    renderer.DrawImmediate(item.Mesh, item.SubmeshIndex, projview, item.Transform);
                }
            }

            // Loop each camera
            foreach (Camera cam in cameramsg.Cameras)
            {
                // Get the projection view matrix and transform
                Matrix projview = cam.ProjectionView;
                Transform transform = cam.Owner.GetComponent<Transform>();
                Vector3 position = transform.Position;
                Vector3 forward = transform.Forward;
                Vector4 clip = new Vector4(cam.ClipPlane.Normal, cam.ClipPlane.D);

                // Setup initial state
                renderer.Depth = renderer.Depth_Enabled;
                renderer.Blend = renderer.Blend_Opaque;
                renderer.Culling = renderer.Culling_Backface;
                gbuffer.Bind();
                gbuffer.Clear();

                // TODO (IN THIS ORDER)
                // Note: The amount of RTs in use here is quite ridiculous
                // 1) Bind GBuffer (and set blend to normal, depth to on)
                // 2) Loop each item in the render queue
                // 2a) Check for visibility against camera
                // 2b) Bind the material
                // 2c) Draw the item
                // 3) Bind light accum buffer (and set blend to additive, depth to off)
                // 4) Loop each light
                // 4a) Check for visibility against camera
                // 4b) Draw the light layer
                // 5) Bind SwapA
                // 6) Draw camera background (skybox)
                // 7) Using the light blit material (inputs include GBuffer's colour buffer & light accum buffers), blit the image
                // 8) Loop each item in the postprocess queue
                // 8a) Bind SwapB
                // 8b) Draw postprocess effect, use SwapA as input
                // 8c) Swap SwapA and SwapB
                // 9) Bind camera target (or backbuffer if null)
                // 10) Blit SwapA to screen

                // Draw all items in the render queue to the GBuffer
                foreach (RenderWorkItem item in renderqueue)
                {
                    // Set the material
                    item.Material.SetParameter("useclip", cam.UseClipping ? 1.0f : 0.0f);
                    if (cam.UseClipping) item.Material.SetParameter("clip", clip);
                    renderer.SetActiveMaterial(item.Material);
                    var desiredculling = item.Material.CullingMode != null ? item.Material.CullingMode : renderer.Culling_Backface;
                    if (renderer.Culling != desiredculling) renderer.Culling = desiredculling;

                    // Draw it
                    renderer.DrawImmediate(item.Mesh, item.SubmeshIndex, projview, item.Transform);
                }

                // Setup light accum state
                lightaccum.Bind();
                lightaccum.Clear();
                renderer.Depth = renderer.Depth_Disabled;
                renderer.Blend = renderer.Blend_Add;
                renderer.Culling = renderer.Culling_None;

                // Update camera details on all light types
                foreach (Material lightmat in mat_lights.Values)
                {
                    lightmat.SetParameter("camera_position", position);
                    lightmat.SetParameter("camera_forward", forward);
                }

                // Draw all lights
                foreach (Light light in lightmsg.Lights)
                {
                    // Set the material
                    Material lightmat = mat_lights[light.Type];
                    light.ApplyLightSettings(lightmat);
                    lightmat.Apply(false);
                    renderer.SetActiveMaterial(lightmat);

                    // Draw it
                    renderer.DrawImmediate(mesh_fs, 0);
                }

                // Are there particle systems to draw?
                if (psysmsg.ParticleSystems.Count > 0)
                {
                    // Setup state
                    renderer.Depth = renderer.Depth_ReadOnly;
                    //renderer.Depth = renderer.Depth_Disabled;

                    // Bind particle system RT (hybrid of GBuffer depth and RT colour)
                    particleaccum.BindHybrid(gbuffer.GetDepthView());
                    //particleaccum.Bind();
                    particleaccum.Clear();

                    // Render
                    for (int i = 0; i < psysmsg.ParticleSystems.Count; i++)
                    {
                        ParticleSystem psys = psysmsg.ParticleSystems[i];
                        if (psys.TransferMode == ParticleTransferMode.Add)
                            renderer.Blend = renderer.Blend_Add;
                        else if (psys.TransferMode == ParticleTransferMode.Alpha)
                            renderer.Blend = renderer.Blend_Alpha;
                        else
                            renderer.Blend = renderer.Blend_Opaque;
                        psys.Material.Shader.SetVariable("view", transform.WorldToObject);
                        psys.Material.Shader.SetVariable("projection", cam.Projection);
                        renderer.SetActiveMaterial(psys.Material);
                        renderer.DrawImmediate(psys.Mesh, 0, projview, Matrix.Identity);
                    }
                }

                // Setup blit state
                swapA.Bind();
                renderer.Blend = renderer.Blend_Alpha;
                renderer.Depth = renderer.Depth_Disabled;

                // Draw background
                if (cam.Background == BackgroundType.Skybox)
                {
                    if (cam.Skybox != null)
                    {
                        renderer.SetActiveMaterial(cam.Skybox);
                        Vector3 campos = transform.Position;
                        renderer.DrawImmediate(mesh_skybox, 0, cam.ProjectionView, Matrix.Translation(campos.X - 0.5f, campos.Y - 0.5f, campos.Z - 0.5f));
                    }
                }

                // Blit lighting
                renderer.SetActiveMaterial(mat_blitlight);
                renderer.DrawImmediate(mesh_fs, 0);

                // Are there particle systems to draw?
                if (psysmsg.ParticleSystems.Count > 0)
                {
                    // Blit particles
                    renderer.Blend = renderer.Blend_Add;
                    mat_blit.SetParameter("texSource", particleaccum.GetTexture(particleaccum_colour));
                    renderer.SetActiveMaterial(mat_blit);
                    renderer.DrawImmediate(mesh_fs, 0);
                }

                // Setup state
                renderer.Blend = renderer.Blend_Opaque;

                // Loop each postprocess
                RenderTarget pp_source = swapA;
                RenderTarget pp_target = swapB;
                foreach (PostProcessEffect effect in effectqueue)
                {
                    // Loop each pass
                    for (int i = 0; i < effect.Passes; i++)
                    {
                        // Bind target
                        pp_target.Bind();

                        // Apply material
                        effect.Material.SetParameter("texImage", pp_source.GetTexture(0));
                        effect.Material.SetParameter("imagesize", new Vector2(pp_source.Width, pp_source.Height));
                        renderer.SetActiveMaterial(effect.Material, false, true);

                        // Blit
                        renderer.DrawImmediate(mesh_fs, 0);

                        // Swap
                        Util.Swap(ref pp_source, ref pp_target);
                    }
                }

                // Bind camera target
                if (cam.Target != null)
                    cam.Target.Bind();
                else
                    renderer.BindBackbuffer();

                // Blit final result
                mat_blit.SetParameter("texSource", pp_source.GetTexture(0));
                renderer.SetActiveMaterial(mat_blit, false, true);
                renderer.DrawImmediate(mesh_fs, 0);
            }
        }

        private void ClearRenderQueue()
        {
            // Recycle all work items
            foreach (var item in renderqueue)
                workitempool.Recycle(item);

            // Clear the render queue
            renderqueue.Clear();

            // Clear the effect queue
            effectqueue.Clear();
        }
    }
}
