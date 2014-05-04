using System;

using CastleRenderer.Structures;

namespace CastleRenderer.Messages
{
    /// <summary>
    /// A message sent every frame
    /// </summary>
    public class UpdateMessage : Message
    {
        /// <summary>
        /// The frame number
        /// </summary>
        public uint FrameNumber { get; set; }

        /// <summary>
        /// Time passed in seconds since last frame message was sent
        /// </summary>
        public float DeltaTime { get; set; }
    }
}
