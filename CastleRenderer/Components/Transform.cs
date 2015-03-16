using System;

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
                FireChangeEvent();
            }
        }

        /// <summary>
        /// The position of this transform in local 2D space
        /// </summary>
        public Vector2 LocalPosition2D
        {
            get
            {
                return new Vector2(localposition.X, localposition.Y);
            }
            set
            {
                localposition = new Vector3(value.X, value.Y, localposition.Z);
                FireChangeEvent();
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
                FireChangeEvent();
            }
        }

        /// <summary>
        /// The rotation of this transform in local 2D space
        /// </summary>
        public float LocalRotation2D
        {
            get
            {
                return (float)Math.Asin(2 * localrotation.X * localrotation.Y + 2 * localrotation.Z * localrotation.W);
                //return (float)Math.Atan2(2 * localrotation.X * localrotation.W - 2 * localrotation.Y * localrotation.Z, 1 - 2 * localrotation.X * localrotation.X - 2 * localrotation.Z * localrotation.Z);
            }
            set
            {
                localrotation = Quaternion.RotationYawPitchRoll(0.0f, 0.0f, value);
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
                FireChangeEvent();
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
                LocalRotation = Util.ForwardToRotation(value);
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
        /// Gets the rotation of this transform in world space
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                Actor parent = Owner.Parent;
                if (parent == null) return LocalRotation;
                Transform parent_trans = parent.GetComponent<Transform>();
                if (parent_trans == null) return LocalRotation;
                return LocalRotation * parent_trans.Rotation;
            }
        }

        /// <summary>
        /// Gets the forward vector of this transform in world space
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                // Rotate unit z
                return Util.Vector3Transform(Vector3.UnitZ, Rotation);
            }
        }

        public Transform()
        {
            LocalPosition = Vector3.Zero;
            LocalRotation = Quaternion.Identity;
            LocalScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        /// <summary>
        /// Fires the changed event
        /// </summary>
        private void FireChangeEvent()
        {
            if (OnTransformChange != null) OnTransformChange(this);
            if (Owner == null) return;
            foreach (Actor child in Owner.Children)
            {
                Transform transform = child.GetComponent<Transform>();
                if (transform != null)
                    transform.FireChangeEvent();
            }
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
