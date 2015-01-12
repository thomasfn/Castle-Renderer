﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;

using SlimDX;

namespace CastleRenderer.Components
{
    /// <summary>
    /// A component that responds to user input and controls the object like a first-person fly mode
    /// The object will be moved in local space (aka, relative to it's parent)
    /// </summary>
    [RequiresComponent(typeof(Transform))]
    public class UserFlyable : BaseComponent
    {
        private bool move_forward, move_back, move_right, move_left;

        private bool rotate;
        private int oldmousex, oldmousey;

        private Vector3 velocity;

        /// <summary>
        /// The maximum speed this object can fly at
        /// </summary>
        public float MaxSpeed { get; set; }

        /// <summary>
        /// The rate at which to accelerate
        /// </summary>
        public float Acceleration { get; set; }

        /// <summary>
        /// The rate to slow down when no key is pressed
        /// </summary>
        public float Resistance { get; set; }

        /// <summary>
        /// Mouse sensitivity
        /// </summary>
        public float Sensitivity { get; set; }

        private Vector3[] recordedpositions;
        private Quaternion[] recordedrotations;
        private bool interpolating;
        private float dtime;
        private float dspeed;

        /// <summary>
        /// Called when this component is attached to an actor
        /// </summary>
        public override void OnAttach()
        {
            // Attach base
            base.OnAttach();

            // Initialise
            recordedpositions = new Vector3[2];
            recordedrotations = new Quaternion[2];
            interpolating = false;
            dtime = 0.0f;
            dspeed = 1.0f;
        }

        /// <summary>
        /// Called when it's time to update the frame
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(UpdateMessage))]
        public void OnUpdate(UpdateMessage msg)
        {
            // Get the transform
            Transform transform = Owner.GetComponent<Transform>();

            // Are we interpolating?
            if (interpolating)
            {
                // Reset velocity
                velocity = Vector3.Zero;

                // Increment dtime
                dtime += msg.DeltaTime * dspeed;
                if (dtime > 1.0f) dtime = 1.0f;

                // Compute position and rotation
                Vector3 position = Vector3.Lerp(recordedpositions[0], recordedpositions[1], dtime);
                Quaternion rotation = Quaternion.Slerp(recordedrotations[0], recordedrotations[1], dtime);

                // Apply
                transform.LocalPosition = position;
                transform.LocalRotation = rotation;

                // Done
                return;
            }

            // Apply drag
            float frameresistance = Resistance * msg.DeltaTime;
            velocity *= (1.0f - frameresistance);

            // Calculate direction vectors
            Matrix matrix = transform.ObjectToLocal;
            Vector4 forward = Vector3.Transform(Vector3.UnitZ, transform.LocalRotation);
            Vector4 right = Vector3.Transform(Vector3.UnitX, transform.LocalRotation);

            // Calculate force
            Vector3 force = Vector3.Zero;
            float forwardfactor = move_forward ? 1.0f : move_back ? -1.0f : 0.0f;
            float rightfactor = move_right ? 1.0f : move_left ? -1.0f : 0.0f;
            force.X = forward.X * forwardfactor + right.X * rightfactor;
            force.Y = forward.Y * forwardfactor + right.Y * rightfactor;
            force.Z = forward.Z * forwardfactor + right.Z * rightfactor;
            force.Normalize();

            // Apply force
            velocity += force * Acceleration * msg.DeltaTime;
            float speed = velocity.Length();
            if (speed > MaxSpeed)
                velocity *= MaxSpeed / speed;

            // Apply velocity
            transform.LocalPosition += velocity;
        }

        /// <summary>
        /// Called when a key has been pressed
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(KeyPressMessage))]
        public void OnKeyPress(KeyPressMessage msg)
        {
            // What key is it?
            switch (msg.Key)
            {
                case Keys.A:
                    move_left = msg.Depressed;
                    break;
                case Keys.W:
                    move_forward = msg.Depressed;
                    break;
                case Keys.S:
                    move_back = msg.Depressed;
                    break;
                case Keys.D:
                    move_right = msg.Depressed;
                    break;
                case Keys.D1:
                    if (msg.Depressed)
                    {
                        Transform transform = Owner.GetComponent<Transform>();
                        recordedpositions[0] = transform.LocalPosition;
                        recordedrotations[0] = transform.LocalRotation;
                        Console.WriteLine("Recorded camera transform into slot 1");
                    }
                    break;
                case Keys.D2:
                    if (msg.Depressed)
                    {
                        Transform transform = Owner.GetComponent<Transform>();
                        recordedpositions[1] = transform.LocalPosition;
                        recordedrotations[1] = transform.LocalRotation;
                        Console.WriteLine("Recorded camera transform into slot 2");
                    }
                    break;
                case Keys.Space:
                    if (msg.Depressed)
                    {
                        interpolating = !interpolating;
                        dtime = 0.0f;
                    }
                    break;
                case Keys.Add:
                    if (msg.Depressed)
                    {
                        dspeed *= 1.2f;
                        Console.WriteLine("dspeed = {0}", dspeed);
                    }
                    break;
                case Keys.Subtract:
                    if (msg.Depressed)
                    {
                        dspeed /= 1.2f;
                        Console.WriteLine("dspeed = {0}", dspeed);
                    }
                    break;
                case Keys.T:
                    if (msg.Depressed)
                    {
                        Vector3 pos = Owner.GetComponent<Transform>().Position;
                        System.IO.File.WriteAllText("debug.txt", string.Format("X={0}, Y={1}, Z={2}", pos.X, pos.Y, pos.Z));
                    }
                    break;
            }
        }

        /// <summary>
        /// Called when a mouse button has been pressed
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(MousePressMessage))]
        public void OnMousePress(MousePressMessage msg)
        {
            // Is it right button?
            if (msg.Button == MouseButtons.Right)
                rotate = msg.Depressed;
        }

        /// <summary>
        /// Called when a mouse has moved
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(MouseMoveMessage))]
        public void OnMouseMove(MouseMoveMessage msg)
        {
            // Are we rotating?
            if (rotate)
            {
                // Get delta
                int dx = msg.X - oldmousex;
                int dy = msg.Y - oldmousey;

                // Get transform and old rotation
                Transform transform = Owner.GetComponent<Transform>();
                Quaternion oldrotation = transform.LocalRotation;

                // Calculate pitch and yaw amounts
                Quaternion pitch = Quaternion.RotationAxis(Util.Vector3Transform(Vector3.UnitX, oldrotation), dy * Sensitivity);
                Quaternion yaw = Quaternion.RotationAxis(Vector3.UnitY, dx * Sensitivity);

                // Set new rotation
                transform.LocalRotation = oldrotation * pitch * yaw;
            }

            // Store new coords
            oldmousex = msg.X;
            oldmousey = msg.Y;
        }
    }
}
