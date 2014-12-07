using System;
using System.Collections.Generic;
using System.Diagnostics;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;

using SlimDX;

namespace CastleRenderer.Components
{
    /// <summary>
    /// Represents a particle system in 3D space simulated on the GPU
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    public class GPUParticleSystem : ParticleSystem
    {


        public override void OnAttach()
        {
            base.OnAttach();
        }

        protected override void SimulateSystem(float deltatime)
        {
            throw new NotImplementedException();
        }

        public override bool EmitParticle()
        {
            throw new NotImplementedException();
        }

        public override void Draw(Renderer renderer, Matrix projview)
        {
            throw new NotImplementedException();
        }
    }
}
