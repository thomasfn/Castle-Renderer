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
    /// Represents a particle system in 3D space
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    public class CPUParticleSystem : ParticleSystem
    {
        

        private List<uint> indices;
        private Particle[] particles;
        public Mesh Mesh { get; private set; }

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
        }

        protected override void SimulateSystem(float deltatime)
        {
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
                        p.Velocity += Acceleration * deltatime;
                        p.Position += p.Velocity * deltatime;
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

        /// <summary>
        /// Emits a single particle
        /// </summary>
        /// <returns></returns>
        public override bool EmitParticle()
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

        /// <summary>
        /// Draws this particle system
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="projview"></param>
        public override void Draw(Renderer renderer, GenericCamera camera)
        {
            renderer.DrawImmediate(Mesh, 0, camera.CameraTransformParameterBlock, ObjectTransformParameterBlock);
        }
    }
}
