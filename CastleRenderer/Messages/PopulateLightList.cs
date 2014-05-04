using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Components;

namespace CastleRenderer.Messages
{
    /// <summary>
    /// A message sent when it's time to populate the light list
    /// </summary>
    public class PopulateLightList : Message
    {
        public OrderedList<Light> Lights { get; set; }
    }
}
