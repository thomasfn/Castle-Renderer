using System;
using System.Collections.Generic;
using System.Diagnostics;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;
using CastleRenderer.Graphics.MaterialSystem;

using SlimDX;

namespace CastleRenderer.Components
{
    public enum ParticleTransferMode {  Add, Alpha  };

    public class ParticleSystemComparer : IComparer<ParticleSystem>
    {
        public int Compare(ParticleSystem x, ParticleSystem y)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents a particle system in 3D space
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    public abstract class ParticleSystem : BaseComponent
    {
        /// <summary>
        /// The maximum particle count that this system manages
        /// </summary>
        public int ParticleCount { get; set; }

        /// <summary>
        /// The blend mode to use when rendering this particle system
        /// </summary>
        public ParticleTransferMode TransferMode { get; set; }

        /// <summary>
        /// The material to use
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// How long in seconds each particle should live for
        /// </summary>
        public float ParticleLife { get; set; }

        /// <summary>
        /// The start colour of each particle
        /// </summary>
        public Color4 StartColour { get; set; }

        /// <summary>
        /// The end colour of each particle
        /// </summary>
        public Color4 EndColour { get; set; }

        /// <summary>
        /// The start size of each particle
        /// </summary>
        public float StartSize { get; set; }

        /// <summary>
        /// The end size of each particle
        /// </summary>
        public float EndSize { get; set; }

        /// <summary>
        /// The initial velocity of each particle
        /// </summary>
        public Vector3 InitialVelocity { get; set; }

        /// <summary>
        /// The amount of random velocity to give each particle
        /// </summary>
        public Vector3 RandomVelocity { get; set; }

        /// <summary>
        /// The amount of random position to give each particle
        /// </summary>
        public Vector3 RandomPosition { get; set; }

        /// <summary>
        /// The acceleration of each particle
        /// </summary>
        public Vector3 Acceleration { get; set; }

        /// <summary>
        /// The number of particles to emit a second
        /// </summary>
        public int EmissionRate { get; set; }

        protected struct Particle
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public float Born;
            public float Rotation;
            public Color4 Colour;
            public float Size;

            public bool Alive;
        }

        protected Stopwatch timer, globaltimer;

        protected Random rnd;

        public override void OnAttach()
        {
            // Attach base
            base.OnAttach();

            // Create timers
            timer = new Stopwatch();
            timer.Start();
            globaltimer = new Stopwatch();
            globaltimer.Start();

            // Create misc
            rnd = new Random();
        }

        [MessageHandler(typeof(UpdateMessage))]
        public void OnUpdate(UpdateMessage msg)
        {
            // Emit particle
            if (timer.Elapsed.TotalSeconds > (1.0f / EmissionRate))
            {
                timer.Restart();
                EmitParticle();
            }

            // Simulate
            SimulateSystem(msg.DeltaTime);
        }

        protected abstract void SimulateSystem(float deltatime);

        protected Vector3 RandomVector(Vector3 range)
        {
            return new Vector3((float)(rnd.NextDouble() - 0.5) * range.X, (float)(rnd.NextDouble() - 0.5) * range.Y, (float)(rnd.NextDouble() - 0.5) * range.Z);
        }

        /// <summary>
        /// Emits a single particle
        /// </summary>
        /// <returns></returns>
        public abstract bool EmitParticle();

        /// <summary>
        /// Draws this particle system
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="projview"></param>
        public abstract void Draw(Renderer renderer, Matrix projview);

        [MessageHandler(typeof(PopulateParticleSystemList))]
        public void OnPopulateParticleSystem(PopulateParticleSystemList msg)
        {
            msg.ParticleSystems.Add(this);
        }
    }
}
