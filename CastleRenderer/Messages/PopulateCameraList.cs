using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Components;

namespace CastleRenderer.Messages
{
    /// <summary>
    /// A message sent when it's time to populate the camera list
    /// </summary>
    public class PopulateCameraList : Message
    {
        public OrderedList<Camera> Cameras { get; set; }
        public HashSet<ShadowCaster> ShadowCasters { get; set; }
    }
}
