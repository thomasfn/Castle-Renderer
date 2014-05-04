using System;
using System.Windows.Forms;

using CastleRenderer.Structures;
using Message = CastleRenderer.Structures.Message;

namespace CastleRenderer.Messages
{
    /// <summary>
    /// A message sent when a key press has occured
    /// </summary>
    public class KeyPressMessage : Message
    {
        /// <summary>
        /// The key that has been pressed
        /// </summary>
        public Keys Key { get; set; }

        /// <summary>
        /// Whether the key was pressed or released
        /// </summary>
        public bool Depressed { get; set; }

    }
}
