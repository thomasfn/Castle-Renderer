using System;

using CastleRenderer.Structures;

using SlimDX.Windows;

namespace CastleRenderer.Messages
{
    /// <summary>
    /// A message sent when the game window has been created
    /// </summary>
    public class WindowCreatedMessage : Message
    {
        public RenderForm Form { get; set; }
    }
}
