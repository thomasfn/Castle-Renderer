using System;
using System.Collections.Generic;

using SlimDX;

namespace CastleRenderer.Graphics
{
    /// <summary>
    /// Represents an item of work to be rendered
    /// </summary>
    public class RenderWorkItem
    {
        public Mesh Mesh { get; set; }
        public int SubmeshIndex { get; set; }
        public Matrix Transform { get; set; }
        public Material Material { get; set; }

    }

    public class RenderWorkItemComparer : IComparer<RenderWorkItem>
    {
        public int Compare(RenderWorkItem x, RenderWorkItem y)
        {
            return Comparer<string>.Default.Compare(x.Material.Name, y.Material.Name);
        }
    }
}
