using System;
using System.Collections.Generic;
using System.Diagnostics;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;

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
    public class ParticleSystem : BaseComponent
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

        private struct Particle
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public float Born;
            public float Rotation;
            public Color4 Colour;
            public float Size;

            public bool Alive;
        }

        private List<uint> indices;
        private Particle[] particles;
        public Mesh Mesh { get; private set; }

        private Stopwatch timer, globaltimer;

        private Random rnd;

        public override void OnAttach()
        {
            // Attach base
            base.OnAttach();

            // Create particle array
            particles = new Particle[ParticleCount];

            // Create mesh
            Mesh = new Mesh();
            Mesh.Positions = new Vector3[ParticleCount];
            Mesh.Normals = new Vector3[ParticleCount];
            Mesh.TextureCoordinates = new Vector2[ParticleCount];
            Mesh.Submeshes = new uint[1][];
            Mesh.Topology = MeshTopology.Points;

            // Create indices
            indices = new List<uint>();

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

            // Run simulation on each particle and update the mesh
            indices.Clear();
            float time = (float)globaltimer.Elapsed.TotalSeconds;
            for (int i = 0; i < ParticleCount; i++)
            {
                Particle p = particles[i];
                if (p.Alive)
                {
                    float age = (time - p.Born) / ParticleLife;
                    if (age >= 1.0f)
                        p.Alive = false;
                    else
                    {
                        p.Velocity += Acceleration * msg.DeltaTime;
                        p.Position += p.Velocity * msg.DeltaTime;
                        p.Colour = Color4.Lerp(StartColour, EndColour, age);
                        p.Size = (EndSize - StartSize) * age + StartSize;

                        Mesh.Positions[i] = p.Position;
                        Mesh.Normals[i] = new Vector3(p.Colour.Red, p.Colour.Green, p.Colour.Blue) * p.Colour.Alpha / 65536.0f;
                        Mesh.TextureCoordinates[i] = new Vector2(p.Size, p.Rotation);

                        indices.Add((uint)i);
                    }
                    particles[i] = p;
                }
            }
            Mesh.Submeshes[0] = indices.ToArray();
            Mesh.Iteration++;
        }

        private Vector3 RandomVector(Vector3 range)
        {
            return new Vector3((float)(rnd.NextDouble() - 0.5) * range.X, (float)(rnd.NextDouble() - 0.5) * range.Y, (float)(rnd.NextDouble() - 0.5) * range.Z);
        }

        /// <summary>
        /// Emits a single particle
        /// </summary>
        /// <returns></returns>
        public bool EmitParticle()
        {
            for (int i = 0; i < ParticleCount; i++)
            {
                if (!particles[i].Alive)
                {
                    Particle p = default(Particle);
                    p.Alive = true;
                    p.Born = (float)globaltimer.Elapsed.TotalSeconds;
                    p.Position = Owner.GetComponent<Transform>().Position + RandomVector(RandomPosition);
                    p.Velocity = InitialVelocity + RandomVector(RandomVelocity);
                    p.Rotation = (float)(rnd.NextDouble() * Math.PI * 2.0);
                    particles[i] = p;
                    return true;
                }
            }
            return false;
        }

        [MessageHandler(typeof(PopulateParticleSystemList))]
        public void OnPopulateParticleSystem(PopulateParticleSystemList msg)
        {
            msg.ParticleSystems.Add(this);
        }
    }
}
