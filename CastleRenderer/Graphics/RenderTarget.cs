using System;
using System.Collections.Generic;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;

using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace CastleRenderer.Graphics
{
    /// <summary>
    /// Represents a render target
    /// </summary>
    public class RenderTarget
    {
        /// <summary>
        /// The viewport to use for this render target
        /// </summary>
        public Viewport Viewport { get; private set; }

        /// <summary>
        /// The width of this render target
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of this render target
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The colour to clear this rendertarget to
        /// </summary>
        public Color4 ClearColour { get; set; }

        private class Component
        {
            public Texture2D Texture;
            public ResourceView View;
        }

        private Component depthcomponent;
        private List<Component> components;

        private RenderTargetView[] views;

        private Device device;
        private DeviceContext context;

        private int samplecount;
        private string name;

        public RenderTarget(Device device, DeviceContext context, int w, int h, int samplecount, string name = "rt")
        {
            // Store parameters
            this.device = device;
            this.context = context;
            this.samplecount = samplecount;
            this.name = name;
            Width = w;
            Height = h;

            // Setup viewport
            Viewport = new Viewport(0.0f, 0.0f, w, h);

            // Setup components
            components = new List<Component>();

            // Setup defaults
            ClearColour = new Color4(1.0f, 0.0f, 0.0f, 0.0f);
        }

        /// <summary>
        /// Adds a depth component to this render target
        /// </summary>
        /// <returns></returns>
        public void AddDepthComponent()
        {
            // Create the component
            depthcomponent = new Component();

            // Setup texture
            depthcomponent.Texture = new Texture2D(device, new Texture2DDescription()
            {
                ArraySize = 1,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.D32_Float,
                Width = Width,
                Height = Height,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default
            });
            depthcomponent.Texture.DebugName = name + "_depth";

            // Setup view
            depthcomponent.View = new DepthStencilView(device, depthcomponent.Texture, new DepthStencilViewDescription()
            {
                ArraySize = 0,
                Format = Format.D32_Float,
                Dimension = DepthStencilViewDimension.Texture2D,
                Flags = DepthStencilViewFlags.None,
                MipSlice = 0,
                FirstArraySlice = 0
            });
            depthcomponent.View.DebugName = string.Format("-> {0}", depthcomponent.Texture.DebugName);
        }

        /// <summary>
        /// Adds a texture component to this render target
        /// </summary>
        /// <returns></returns>
        public int AddTextureComponent(Format format = Format.R8G8B8A8_UNorm)
        {
            // Create the component
            Component c = new Component();
            components.Add(c);

            // Setup texture
            c.Texture = new Texture2D(device, new Texture2DDescription()
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                Width = Width,
                Height = Height,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(samplecount, 0),
                Usage = ResourceUsage.Default
            });
            c.Texture.DebugName = name + "_" + (components.Count - 1).ToString();

            // Setup view
            c.View = new RenderTargetView(device, c.Texture, new RenderTargetViewDescription()
            {
                ArraySize = 0,
                Format = format,
                Dimension = RenderTargetViewDimension.Texture2D,
                MipSlice = 0,
                FirstArraySlice = 0
            });
            c.View.DebugName = string.Format("-> {0}", c.Texture.DebugName);

            // Return the index
            return components.Count - 1;
        }

        /// <summary>
        /// Completes this render target
        /// </summary>
        public void Finish()
        {
            // Setup views array
            views = new RenderTargetView[components.Count];
            for (int i = 0; i < components.Count; i++)
                views[i] = components[i].View as RenderTargetView;
        }

        /// <summary>
        /// Makes this render target active
        /// </summary>
        public void Bind()
        {
            // Make viewport active
            context.Rasterizer.SetViewports(Viewport);

            // Set target to our views
            if (depthcomponent != null)
                context.OutputMerger.SetTargets(depthcomponent.View as DepthStencilView, views);
            else
                context.OutputMerger.SetTargets(views);
        }

        public void BindHybrid(DepthStencilView depthview)
        {
            // Make viewport active
            context.Rasterizer.SetViewports(Viewport);

            // Set target to our views
            context.OutputMerger.SetTargets(depthview, views);
        }

        /// <summary>
        /// Clears this render target
        /// </summary>
        public void Clear()
        {
            // Clear depth if we have it
            if (depthcomponent != null) context.ClearDepthStencilView(depthcomponent.View as DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

            // Clear views
            foreach (RenderTargetView view in views)
                context.ClearRenderTargetView(view, ClearColour);
        }

        /// <summary>
        /// Gets the texture at the given index for this render target
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public Texture2D GetTexture(int idx)
        {
            return components[idx].Texture;
        }

        /// <summary>
        /// Gets the depth texture for this render target
        /// </summary>
        /// <returns></returns>
        public Texture2D GetDepthTexture()
        {
            return depthcomponent.Texture;
        }
        public DepthStencilView GetDepthView()
        {
            return depthcomponent.View as DepthStencilView;
        }


    }
}
