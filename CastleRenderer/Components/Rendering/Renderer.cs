using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;
using CastleRenderer.Graphics.MaterialSystem;

using SlimDX;
using SlimDX.Windows;
using SlimDX.DXGI;
using SlimDX.Direct3D11;

using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace CastleRenderer.Components
{
    /// <summary>
    /// Represents the D3D11 renderer responsible for graphics rendering
    /// </summary>
    [ComponentPriority(1)]
    public class Renderer : BaseComponent
    {
        public Device Device { get; private set; }
        private DeviceContext context;
        private SwapChain swapchain;
        private Viewport viewport;

        private RenderForm window;

        private RenderTargetView rtBackbuffer;
        private Texture2D texDepthBuffer;
        private DepthStencilView vwDepthBuffer;

        #region Depth State
        public DepthStencilState Depth_Enabled { get; private set; }
        public DepthStencilState Depth_Disabled { get; private set; }
        public DepthStencilState Depth_ReadOnly { get; private set; }
        public DepthStencilState Depth
        {
            get
            {
                return context.OutputMerger.DepthStencilState;
            }
            set
            {
                context.OutputMerger.DepthStencilState = value;
            }
        }
        #endregion

        #region Culling State
        public RasterizerState Culling_Backface { get; private set; }
        public RasterizerState Culling_Frontface { get; private set; }
        public RasterizerState Culling_None { get; private set; }
        public RasterizerState Culling
        {
            get
            {
                return context.Rasterizer.State;
            }
            set
            {
                context.Rasterizer.State = value;
            }
        }
        #endregion

        #region Blend State
        public BlendState Blend_Opaque { get; private set; }
        public BlendState Blend_Alpha { get; private set; }
        public BlendState Blend_Add { get; private set; }
        public BlendState Blend_Multiply { get; private set; }
        public BlendState Blend
        {
            get
            {
                return context.OutputMerger.BlendState;
            }
            set
            {
                context.OutputMerger.BlendState = value;
            }
        }
        #endregion

        #region Sampler State
        public SamplerState Sampler_Clamp { get; private set; }
        public SamplerState Sampler_Clamp_Point { get; private set; }
        public SamplerState Sampler_Clamp_Linear { get; private set; }
        public SamplerState Sampler_Wrap { get; private set; }
        #endregion

        public Color4 ClearColour { get; set; }

        private RenderMessage rendermsg;
        private UpdateMessage updatemsg;

        private Material activematerial;
        private bool activematerialshadow;

        private Dictionary<Resource, ShaderResourceView> shaderresourcemap;

        private int frame_drawcalls;
        private int frame_materialswitches;
        private int frame_shaderswitches;

        private struct ShaderResourceViewData
        {
            public int NumUses;
            public ShaderResourceView View;
        }

        private const int MaxPixelShaderResourceViewSlots = 16;
        private ShaderResourceViewData[] resourceviewslots;

        private const FillMode fillmode = FillMode.Wireframe;

        /// <summary>
        /// Called when the initialise message has been received
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(InitialiseMessage))]
        public void OnInitialise(InitialiseMessage msg)
        {
            // Initialise renderer
            Console.WriteLine("Initialising renderer...");
            rendermsg = new RenderMessage();
            updatemsg = new UpdateMessage();

            // Create the window
            window = new RenderForm("Castle Renderer - 11030062");
            window.Width = 1280;
            window.Height = 720;

            // Add form events
            window.FormClosed += window_FormClosed;

            // Defaults
            ClearColour = new Color4(1.0f, 0.0f, 0.0f, 1.0f);

            // Setup the device
            var description = new SwapChainDescription()
            {
                BufferCount = 1,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = window.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
            };
            Device tmp;
            var result = Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, description, out tmp, out swapchain);
            if (result.IsFailure)
            {
                Console.WriteLine("Failed to create Direct3D11 device (" + result.Code.ToString() + ":" + result.Description + ")");
                return;
            }
            Device = tmp;
            Device.DebugName = "Device";
            context = Device.ImmediateContext;
            context.DebugName = "Context";
            using (var factory = swapchain.GetParent<Factory>())
                factory.SetWindowAssociation(window.Handle, WindowAssociationFlags.IgnoreAltEnter);
            swapchain.DebugName = "Swapchain";

            // Check AA stuff
            int q = Device.CheckMultisampleQualityLevels(Format.R8G8B8A8_UNorm, 8);

            // Setup the viewport
            viewport = new Viewport(0.0f, 0.0f, window.ClientSize.Width, window.ClientSize.Height);
            viewport.MinZ = 0.0f;
            viewport.MaxZ = 1.0f;
            context.Rasterizer.SetViewports(viewport);

            // Setup the backbuffer
            using (var resource = Resource.FromSwapChain<Texture2D>(swapchain, 0))
                rtBackbuffer = new RenderTargetView(Device, resource);
            rtBackbuffer.DebugName = "Backbuffer";

            // Setup depth for backbuffer
            {
                Texture2DDescription texdesc = new Texture2DDescription()
                {
                    ArraySize = 1,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Format.D32_Float,
                    Width = (int)viewport.Width,
                    Height = (int)viewport.Height,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default
                };
                texDepthBuffer = new Texture2D(Device, texdesc);
                texDepthBuffer.DebugName = "Backbuffer.DepthBuffer";
                DepthStencilViewDescription viewdesc = new DepthStencilViewDescription()
                {
                    ArraySize = 0,
                    Format = Format.D32_Float,
                    Dimension = DepthStencilViewDimension.Texture2D,
                    MipSlice = 0,
                    Flags = 0,
                    FirstArraySlice = 0
                };
                vwDepthBuffer = new DepthStencilView(Device, texDepthBuffer, viewdesc);
                vwDepthBuffer.DebugName = "-> Backbuffer.DepthBuffer";
            }

            // Setup states
            #region Depth States
            // Setup depth states
            {
                DepthStencilStateDescription desc = new DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    IsStencilEnabled = false,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.Less
                };
                Depth_Enabled = DepthStencilState.FromDescription(Device, desc);
                Depth_Enabled.DebugName = "Depth_Enabled";
            }
            {
                DepthStencilStateDescription desc = new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                    IsStencilEnabled = false,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Less
                };
                Depth_Disabled = DepthStencilState.FromDescription(Device, desc);
                Depth_Disabled.DebugName = "Depth_Disabled";
            }
            {
                DepthStencilStateDescription desc = new DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    IsStencilEnabled = false,
                    DepthWriteMask = DepthWriteMask.Zero,
                    DepthComparison = Comparison.Less
                };
                Depth_ReadOnly = DepthStencilState.FromDescription(Device, desc);
                Depth_ReadOnly.DebugName = "Depth_ReadOnly";
            }
            #endregion
            #region Sampler States
            Sampler_Clamp = SamplerState.FromDescription(Device, new SamplerDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.Anisotropic,
                MinimumLod = 0.0f,
                MaximumLod = float.MaxValue,
                MaximumAnisotropy = 16
            });
            Sampler_Clamp.DebugName = "Sampler_Clamp";
            Sampler_Clamp_Point = SamplerState.FromDescription(Device, new SamplerDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipPoint
            });
            Sampler_Clamp_Point.DebugName = "Sampler_Clamp_Point";
            Sampler_Clamp_Linear = SamplerState.FromDescription(Device, new SamplerDescription()
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipLinear
            });
            Sampler_Clamp_Linear.DebugName = "Sampler_Clamp_Linear";
            Sampler_Wrap = SamplerState.FromDescription(Device, new SamplerDescription()
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = Filter.Anisotropic,
                MinimumLod = 0.0f,
                MaximumLod = float.MaxValue,
                MaximumAnisotropy = 16
            });
            Sampler_Wrap.DebugName = "Sampler_Wrap";
            #endregion
            #region Rasterizer States
            Culling_Backface = RasterizerState.FromDescription(Device, new RasterizerStateDescription()
            {
                CullMode = CullMode.Back,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                IsDepthClipEnabled = true,
                FillMode = FillMode.Solid,
                IsAntialiasedLineEnabled = false,
                IsFrontCounterclockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0.0f
            });
            Culling_Backface.DebugName = "Culling_Backface";
            Culling_Frontface = RasterizerState.FromDescription(Device, new RasterizerStateDescription()
            {
                CullMode = CullMode.Front,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                IsDepthClipEnabled = true,
                FillMode = FillMode.Solid,
                IsAntialiasedLineEnabled = false,
                IsFrontCounterclockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0.0f
            });
            Culling_Frontface.DebugName = "Culling_Frontface";
            Culling_None = RasterizerState.FromDescription(Device, new RasterizerStateDescription()
            {
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 0.0f,
                IsDepthClipEnabled = true,
                FillMode = FillMode.Solid,
                IsAntialiasedLineEnabled = false,
                IsFrontCounterclockwise = false,
                IsMultisampleEnabled = true,
                IsScissorEnabled = false,
                SlopeScaledDepthBias = 0.0f
            });
            Culling_None.DebugName = "Culling_None";
            #endregion
            #region Blend States
            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.RenderTargets[0].BlendEnable = true;
                desc.RenderTargets[0].BlendOperation = BlendOperation.Add;
                desc.RenderTargets[0].SourceBlend = BlendOption.One;
                desc.RenderTargets[0].DestinationBlend = BlendOption.Zero;
                desc.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
                desc.RenderTargets[0].SourceBlendAlpha = BlendOption.One;
                desc.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
                desc.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                Blend_Opaque = BlendState.FromDescription(Device, desc);
                Blend_Opaque.DebugName = "Blend_Opaque";
            }
            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.RenderTargets[0].BlendEnable = true;
                desc.RenderTargets[0].BlendOperation = BlendOperation.Add;
                desc.RenderTargets[0].SourceBlend = BlendOption.SourceAlpha;
                desc.RenderTargets[0].DestinationBlend = BlendOption.InverseSourceAlpha;
                desc.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
                desc.RenderTargets[0].SourceBlendAlpha = BlendOption.One;
                desc.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
                desc.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                Blend_Alpha = BlendState.FromDescription(Device, desc);
                Blend_Alpha.DebugName = "Blend_Alpha";
            }
            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.RenderTargets[0].BlendEnable = true;
                desc.RenderTargets[0].BlendOperation = BlendOperation.Add;
                desc.RenderTargets[0].SourceBlend = BlendOption.One;
                desc.RenderTargets[0].DestinationBlend = BlendOption.One;
                desc.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
                desc.RenderTargets[0].SourceBlendAlpha = BlendOption.One;
                desc.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
                desc.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                Blend_Add = BlendState.FromDescription(Device, desc);
                Blend_Add.DebugName = "Blend_Add";
            }
            {
                BlendStateDescription desc = new BlendStateDescription();
                desc.RenderTargets[0].BlendEnable = true;
                desc.RenderTargets[0].BlendOperation = BlendOperation.Add;
                desc.RenderTargets[0].SourceBlend = BlendOption.DestinationColor;
                desc.RenderTargets[0].DestinationBlend = BlendOption.Zero;
                desc.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
                desc.RenderTargets[0].SourceBlendAlpha = BlendOption.One;
                desc.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
                desc.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                Blend_Multiply = BlendState.FromDescription(Device, desc);
                Blend_Multiply.DebugName = "Blend_Multiply";
            }
            #endregion

            // Setup default states
            Depth = Depth_Enabled;
            Culling = Culling_Backface;
            Blend = Blend_Opaque;

            // Setup other objects
            shaderresourcemap = new Dictionary<Resource, ShaderResourceView>();
            resourceviewslots = new ShaderResourceViewData[MaxPixelShaderResourceViewSlots];

            // Send the window created message
            WindowCreatedMessage windowcreatedmsg = new WindowCreatedMessage();
            windowcreatedmsg.Form = window;
            Owner.MessagePool.SendMessage(windowcreatedmsg);

            // Show the form
            window.Show();
        }

        #region Window Events

        private void window_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            // Send exit message
            ExitMessage msg = new ExitMessage();
            Owner.MessagePool.SendMessage(msg);
        }

        #endregion

        /// <summary>
        /// Called when it's time to render the frame
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(FrameMessage))]
        public void OnFrame(FrameMessage msg)
        {
            // Clear the backbuffer
            BindBackbuffer();
            context.ClearRenderTargetView(rtBackbuffer, ClearColour);
            context.ClearDepthStencilView(vwDepthBuffer, DepthStencilClearFlags.Depth, 1.0f, 0);

            // Reset counters
            frame_drawcalls = 0;
            frame_materialswitches = 0;
            frame_shaderswitches = 0;

            // Send update message
            updatemsg.DeltaTime = msg.DeltaTime;
            updatemsg.FrameNumber = msg.FrameNumber;
            Owner.MessagePool.SendMessage(updatemsg);

            // Send render message
            Owner.MessagePool.SendMessage(rendermsg);

            // Update console title
            Console.Title = string.Format("drawcalls: {0}, matswitches: {1}, shaderswitches: {2}", frame_drawcalls, frame_materialswitches, frame_shaderswitches);

            // Present
            swapchain.Present(0, PresentFlags.None);
        }

        #region Draw Commands

        /// <summary>
        /// Sets the current active material, returns true if a material switch occured
        /// </summary>
        /// <param name="material"></param>
        public void SetActiveMaterial(Material material, bool shadow = false, bool forceswitch = false, bool noapply = false, bool invertculling = false)
        {
            // Null check
            if (material == null)
            {
                activematerial = null;
                activematerialshadow = false;
                return;
            }

            // Should we ignore optimisations?
            if (activematerial == null || forceswitch)
            {
                activematerial = material;
                MaterialPipeline pipeline = shadow ? material.ShadowPipeline : material.Pipeline;
                if (pipeline == null)
                {
                    activematerial = null;
                    activematerialshadow = false;
                    return;
                }
                if (!noapply) material.Apply();
                pipeline.Activate(true);
                SetCullingMode(material.CullingMode, invertculling);
                frame_materialswitches++;
                frame_shaderswitches++;
                return;
            }

            // Did the material change?
            if (material != activematerial)
            {
                // Did the shader change?
                MaterialPipeline oldpipeline = shadow ? activematerial.ShadowPipeline : activematerial.Pipeline;
                MaterialPipeline newpipeline = shadow ? material.ShadowPipeline : material.Pipeline;
                if (newpipeline == null)
                {
                    activematerial = null;
                    activematerialshadow = false;
                    return;
                }

                // Apply
                activematerial = material;
                activematerialshadow = shadow;
                if (!noapply) material.Apply();
                SetCullingMode(material.CullingMode, invertculling);
                frame_materialswitches++;

                // Activate new shader
                if (oldpipeline != newpipeline)
                {
                    newpipeline.Activate(true);
                    frame_shaderswitches++;
                }
            }
        }

        /// <summary>
        /// Draws a mesh immediately
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="submesh"></param>
        /// <param name="material"></param>
        /// <param name="transform"></param>
        public void DrawImmediate(Mesh mesh, int submesh, MaterialParameterBlock cameratransform, MaterialParameterBlock objecttransform)
        {
            // Sanity check
            if (activematerial == null) return;

            // Setup the material
            MaterialPipeline pipeline = activematerialshadow ? activematerial.ShadowPipeline : activematerial.Pipeline;
            if (pipeline == null) return;
            pipeline.SetMaterialParameterBlock("CameraTransform", cameratransform);
            pipeline.SetMaterialParameterBlock("ObjectTransform", objecttransform);

            // Render
            pipeline.Use();
            if (!mesh.Render(pipeline, submesh))
            {
                mesh.Upload(Device, context);
                mesh.Render(pipeline, submesh);
            }
            frame_drawcalls++;
        }

        /// <summary>
        /// Draws a mesh immediately with no transform setup
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="submesh"></param>
        public void DrawImmediate(Mesh mesh, int submesh)
        {
            // Sanity check
            if (activematerial == null) return;

            // Setup the material
            //activematerial.Shader.Effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply(context);

            // Render
            activematerial.Pipeline.Use();
            if (!mesh.Render(activematerial.Pipeline, submesh))
            {
                mesh.Upload(Device, context);
                mesh.Render(activematerial.Pipeline, submesh);
            }
        }

        /// <summary>
        /// Sets the culling mode
        /// </summary>
        /// <param name="cullingmode"></param>
        public void SetCullingMode(MaterialCullingMode cullingmode, bool invert = false)
        {
            switch (cullingmode)
            {
                case MaterialCullingMode.None:
                    Culling = Culling_None;
                    break;
                case MaterialCullingMode.Frontface:
                    if (invert)
                        Culling = Culling_Backface;
                    else
                        Culling = Culling_Frontface;
                    break;
                case MaterialCullingMode.Backface:
                    if (invert)
                        Culling = Culling_Frontface;
                    else
                        Culling = Culling_Backface;
                    break;
            }
        }

        #endregion

        /// <summary>
        /// Sets the active viewport
        /// </summary>
        /// <param name="viewport"></param>
        public void SetViewport(Viewport viewport)
        {
            context.Rasterizer.SetViewports(viewport);
        }

        /// <summary>
        /// Binds the back buffer to the draw target
        /// </summary>
        public void BindBackbuffer()
        {
            context.OutputMerger.SetTargets(vwDepthBuffer, rtBackbuffer);
        }

        /// <summary>
        /// Creates a render target the size of the window
        /// </summary>
        /// <param name="samplecount"></param>
        /// <returns></returns>
        public RenderTarget CreateRenderTarget(int samplecount, string name = "rt")
        {
            return new RenderTarget(Device, context, window.Width, window.Height, samplecount, name);
        }

        /// <summary>
        /// Creates a render target with the specified size
        /// </summary>
        /// <param name="samplecount"></param>
        /// <returns></returns>
        public RenderTarget CreateRenderTarget(int samplecount, int size, string name = "rt")
        {
            return new RenderTarget(Device, context, size, size, samplecount, name);
        }

        /// <summary>
        /// Creates a render target with the specified size
        /// </summary>
        /// <param name="samplecount"></param>
        /// <param name="size"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public RenderTarget CreateRenderTarget(int samplecount, int width, int height, string name = "rt")
        {
            return new RenderTarget(Device, context, width, height, samplecount, name);
        }

        /// <summary>
        /// Gets a shader resource view for the specified resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public ShaderResourceView AcquireResourceView(Resource resource)
        {
            // See if it already exists
            ShaderResourceView view;
            if (shaderresourcemap.TryGetValue(resource, out view)) return view;

            // Create it
            view = new ShaderResourceView(Device, resource);
            view.DebugName = string.Format("-> {0}", resource.DebugName);
            shaderresourcemap.Add(resource, view);

            // Return it
            return view;
        }

        /// <summary>
        /// Binds the specified texture to the specified texture at the given variable name and slot
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="uniform"></param>
        /// <param name="texture"></param>
        /// <param name="slot"></param>
        /*public void BindShaderTexture(MaterialPipeline pipeline, string uniform, Texture2D texture)
        {
            // Get the view and bind it to the shader
            ShaderResourceView view = AcquireResourceView(texture);
            //pipeline.SetVariable(uniform, view);
            // TODO: This

            // See if the view is already bound to a slot
            // Whilst we're at it, see if there's a free slot we can use, and search for slot with the lowest uses
            int freeslot = -1;
            int lowestuses = resourceviewslots[0].NumUses;
            int lowestusesidx = 0;
            for (int i = 0; i < MaxPixelShaderResourceViewSlots; i++)
                if (resourceviewslots[i].View == view)
                {
                    resourceviewslots[i].NumUses++;
                    return;
                }
                else if (resourceviewslots[i].View == null && freeslot == -1)
                    freeslot = i;
                else if (resourceviewslots[i].NumUses < lowestuses)
                {
                    lowestuses = resourceviewslots[i].NumUses;
                    lowestusesidx = i;
                }

            // If we got this far, we need to find a slot
            // If there is none free, select the one with the lowest uses
            freeslot = lowestusesidx;

            // Use it
            context.PixelShader.SetShaderResource(view, freeslot);
            resourceviewslots[freeslot].View = view;
            resourceviewslots[freeslot].NumUses = 1;
        }*/

        /// <summary>
        /// Called when it's time to shutdown
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(ShutdownMessage))]
        public void OnShutdown(ShutdownMessage msg)
        {
            // Debug
            Console.WriteLine("Shutting down renderer...");

            // Clean up all shader resource views
            /*foreach (var pair in shaderresourcemap)
                pair.Value.Dispose();
            shaderresourcemap.Clear();
            shaderresourcemap = null;

            // Clean up the sampler states
            smpClamp.Dispose();
            smpClamp = null;

            // Clean up the meshes
            mshrQuad.Dispose();
            mshrQuad = null;
            mshQuad = null;

            // Clean up the shaders
            shdBasic.Dispose();
            shdBasic = null;*/

            // Clean up the RT and depth buffer
            vwDepthBuffer.Dispose();
            vwDepthBuffer = null;
            texDepthBuffer.Dispose();
            texDepthBuffer = null;
            rtBackbuffer.Dispose();
            rtBackbuffer = null;

            // Clean up the depth states
            Depth_Disabled.Dispose();
            Depth_Disabled = null;
            Depth_Enabled.Dispose();
            Depth_Enabled = null;

            // Shutdown the swapchain and device
            swapchain.Dispose();
            swapchain = null;
            Device.Dispose();
            Device = null;

            // Cleanup other stuff
            context = null;

            // Cleanup the window
            window.Dispose();
            window = null;
        }
    }
}
