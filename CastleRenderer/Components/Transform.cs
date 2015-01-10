﻿using System;

using CastleRenderer.Structures;
using CastleRenderer.Messages;

using SlimDX;

namespace CastleRenderer.Components
{
    /// <summary>
    /// Represents a transform into the 3D world
    /// </summary>
    [ComponentPriority(0)]
    public class Transform : BaseComponent
    {
        public delegate void TransformChange(Transform sender);
        public event TransformChange OnTransformChange;

        private Vector3 localposition, localscale;
        private Quaternion localrotation;

        /// <summary>
        /// The position of this transform in local space
        /// </summary>
        public Vector3 LocalPosition
        {
            get
            {
                return localposition;
            }
            set
            {
                localposition = value;
                if (OnTransformChange != null) OnTransformChange(this);
            }
        }

        /// <summary>
        /// The rotation of this transform in local space
        /// </summary>
        public Quaternion LocalRotation
        {
            get
            {
                return localrotation;
            }
            set
            {
                localrotation = value;
                if (OnTransformChange != null) OnTransformChange(this);
            }
        }

        /// <summary>
        /// The scale of this transform in local space
        /// </summary>
        public Vector3 LocalScale
        {
            get
            {
                return localscale;
            }
            set
            {
                localscale = value;
                if (OnTransformChange != null) OnTransformChange(this);
            }
        }

        /// <summary>
        /// The forward vector of this transform in local space
        /// </summary>
        public Vector3 LocalForward
        {
            get
            {
                return Util.Vector3Transform(Vector3.UnitZ, LocalRotation);
            }
            set
            {
                Vector3 forward = value;
                forward.Normalize();
                if (forward == Vector3.UnitZ)
                {
                    LocalRotation = Quaternion.Identity;
                    return;
                }
                Vector3 normal = Vector3.Cross(forward, Vector3.UnitZ);
                normal.Normalize();
                float ang = (float)Math.Acos(forward.Z);
                LocalRotation = Quaternion.RotationAxis(normal, -ang);
                if (OnTransformChange != null) OnTransformChange(this);
            }
        }

        /// <summary>
        /// Gets the position of this transform in world space
        /// </summary>
        public Vector3 Position
        {
            get
            {
                // TODO: Optimise by pulling position right out of ObjectToLocal matrix
                return Util.Vector3Transform(Vector3.Zero, ObjectToWorld);
            }
        }

        /// <summary>
        /// Gets the forward vector of this transform in world space
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                // TODO: Optimise somehow
                return Util.Vector3Transform(Vector3.UnitZ, ObjectToWorld) - Util.Vector3Transform(Vector3.Zero, ObjectToWorld);;
            }
        }

        public Transform()
        {
            LocalPosition = Vector3.Zero;
            LocalRotation = Quaternion.Identity;
            LocalScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        /// <summary>
        /// Gets a matrix that transforms from object to local space
        /// </summary>
        public Matrix ObjectToLocal
        {
            get
            {
                return Matrix.RotationQuaternion(LocalRotation) * Matrix.Scaling(LocalScale) * Matrix.Translation(LocalPosition);
            }
        }

        /// <summary>
        /// Gets a matrix that transforms from local to object space
        /// </summary>
        public Matrix LocalToObject
        {
            get
            {
                Matrix tmp = ObjectToLocal;
                tmp.Invert();
                return tmp;
            }
        }

        /// <summary>
        /// Gets a matrix that transforms from object to world space
        /// </summary>
        public Matrix ObjectToWorld
        {
            get
            {
                Actor parent = Owner.Parent;
                if (parent == null) return ObjectToLocal;
                Transform parent_trans = parent.GetComponent<Transform>();
                if (parent_trans == null) return ObjectToLocal;
                return ObjectToLocal * parent_trans.ObjectToWorld;
            }
        }

        /// <summary>
        /// Gets a matrix that transforms from world to object space
        /// </summary>
        public Matrix WorldToObject
        {
            get
            {
                Matrix tmp = ObjectToWorld;
                tmp.Invert();
                return tmp;
            }
        }

    }
}
