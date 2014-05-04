using System;
using System.Windows.Forms;

using CastleRenderer.Structures;
using Message = CastleRenderer.Structures.Message;

namespace CastleRenderer.Messages
{
    /// <summary>
    /// A message sent when a mouse button press has occured
    /// </summary>
    public class MousePressMessage : Message
    {
        /// <summary>
        /// The button that has been pressed
        /// </summary>
        public MouseButtons Button { get; set; }

        /// <summary>
        /// Whether the button was pressed or released
        /// </summary>
        public bool Depressed { get; set; }

    }
}
