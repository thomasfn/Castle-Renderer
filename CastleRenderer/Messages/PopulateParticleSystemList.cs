using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Components;

namespace CastleRenderer.Messages
{
    /// <summary>
    /// A message sent when it's time to populate the particle system list
    /// </summary>
    public class PopulateParticleSystemList : Message
    {
        public List<ParticleSystem> ParticleSystems { get; set; }
    }
}
