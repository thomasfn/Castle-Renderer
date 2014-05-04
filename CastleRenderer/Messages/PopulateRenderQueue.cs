using System;

using CastleRenderer.Structures;
using CastleRenderer.Components;

namespace CastleRenderer.Messages
{
    /// <summary>
    /// A message sent when it's time to populate the render queue
    /// </summary>
    public class PopulateRenderQueue : Message
    {
        public SceneManager SceneManager { get; set; }
    }
}
