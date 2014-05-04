using System;
using System.Windows.Forms;

using CastleRenderer.Structures;
using Message = CastleRenderer.Structures.Message;

namespace CastleRenderer.Messages
{
    /// <summary>
    /// A message sent when the mouse has moved
    /// </summary>
    public class MouseMoveMessage : Message
    {
        /// <summary>
        /// The X coordinate of the mouse
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The Y coordinate of the mouse
        /// </summary>
        public int Y { get; set; }
    }
}
