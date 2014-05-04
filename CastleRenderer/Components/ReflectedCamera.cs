using System;
using System.Collections.Generic;
using System.Diagnostics;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;

using SlimDX;
using SlimDX.Direct3D11;

namespace CastleRenderer.Components
{
    /// <summary>
    /// Represents a camera used to render reflections
    /// NOTE: The local space of the object this component is attached to should be world space
    /// </summary>
    [RequiresComponent(typeof(Camera))]
    [ComponentPriority(2)]
    public class ReflectedCamera : BaseComponent
    {
        /// <summary>
        /// The plane to reflect in (this should match the plane used to render the reflections)
        /// </summary>
        public Plane ReflectionPlane { get; set; }

        /// <summary>
        /// The main camera to mimic
        /// </summary>
        public Actor MainCamera { get; set; }

        /// <summary>
        /// The material that will utilise this reflection
        /// </summary>
        public Material ReflectionTarget { get; set; }

        private Stopwatch timer;

        public override void OnAttach()
        {
            // Attach base
            base.OnAttach();

            // Copy camera settings
            Camera mimic = MainCamera.GetComponent<Camera>();
            Camera me = Owner.GetComponent<Camera>();
            me.ProjectionType = mimic.ProjectionType;
            me.FoV = mimic.FoV;
            me.NearZ = mimic.NearZ;
            me.FarZ = mimic.FarZ;
            me.Background = mimic.Background;
            me.Skybox = mimic.Skybox;

            // This is a bit hacky, but it refreshes the camera settings
            me.OnDetach();
            me.OnAttach();
            
            // Apply our RT to the target
            ReflectionTarget.SetParameter("texReflection", me.Target.GetTexture(0));

            // Make timer
            timer = new Stopwatch();
            timer.Start();
        }

        private Vector3 ReflectPosition(Vector3 position)
        {
            return position - (ReflectionPlane.Normal * Plane.DotCoordinate(ReflectionPlane, position) * 2.0f);
        }

        /// <summary>
        /// Called when it's time to update the frame
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(UpdateMessage))]
        public void OnUpdate(UpdateMessage msg)
        {
            // Get transforms
            Transform me = Owner.GetComponent<Transform>();
            Transform mimic = MainCamera.GetComponent<Transform>();

            // Reflect position
            Vector3 mimicpos = mimic.Position;
            me.LocalPosition = ReflectPosition(mimicpos);

            // Reflect angle
            Vector3 axis = mimic.LocalRotation.Axis;
            float angle = mimic.LocalRotation.Angle;
            Matrix refl = Matrix.Reflection(ReflectionPlane);
            refl.M41 = 0.0f;
            refl.M42 = 0.0f;
            refl.M43 = 0.0f;
            Vector3 reflectedaxis = Util.Vector3Transform(axis, refl);
            me.LocalRotation = Quaternion.RotationAxis(reflectedaxis, -angle);

            // Update material
            ReflectionTarget.SetParameter("time", (float)timer.Elapsed.TotalSeconds);
            ReflectionTarget.SetParameter("camera_position", mimicpos);
        }
    }
}
