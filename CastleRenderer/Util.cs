using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using SlimDX;

namespace CastleRenderer
{
    public static class Util
    {
        public static Vector3 Vector3Transform(Vector3 vector, Matrix matrix)
        {
            Vector4 vec;
            Vector3.Transform(ref vector, ref matrix, out vec);
            return new Vector3(vec.X, vec.Y, vec.Z);
        }
        public static Vector3 Vector3Transform(Vector3 vector, Quaternion quaternion)
        {
            Vector4 vec;
            Vector3.Transform(ref vector, ref quaternion, out vec);
            return new Vector3(vec.X, vec.Y, vec.Z);
        }

        private static Vector3[] corners_temp = new Vector3[8];
        public static BoundingBox BoundingBoxTransform(BoundingBox bbox, Matrix matrix)
        {
            Vector3[] corners = bbox.GetCorners();
            for (int i = 0; i < 8; i++)
            {
                Vector4 vec;
                Vector3.Transform(ref corners[i], ref matrix, out vec);
                corners[i] = new Vector3(vec.X, vec.Y, vec.Z);
            }
            return BoundingBox.FromPoints(corners);
        }

        public static string ReadNullTerminatedString(this System.IO.BinaryReader rdr)
        {
            StringBuilder sb = new StringBuilder();
            char c;
            while ((c = rdr.ReadChar()) != '\0')
                sb.Append(c);
            return sb.ToString();
        }

        const float PI = 3.141592654f;

        public static float ToRadians(this float degrees)
        {
            return degrees * PI / 180.0f;
        }

        public static float ToDegrees(this float radians)
        {
            return (radians / PI) * 180.0f;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }


        public delegate T ReflectionGetter<T>(int index);

        /// <summary>
        /// Gets all input parameters to the specified shader reflection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getter"></param>
        /// <returns></returns>
        public static T[] GetUnknownShaderReflectionArray<T>(ReflectionGetter<T> getter)
        {
            var list = new List<T>();
            int i = 0;
            bool exit = false;
            while (!exit)
            {
                try
                {
                    T obj = getter(i++);
                    list.Add(obj);
                }
                catch (Exception)
                {
                    exit = true;
                }
            }
            return list.ToArray();
        }

        public static float Clamp(this float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }

        public static void Shuffle<T>(T[] arr, Random rnd = null)
        {
            T[] copy = new T[arr.Length];
            Array.Copy(arr, copy, arr.Length);
            int[] indices = new int[arr.Length];
            for (int i = 0; i < arr.Length; i++) indices[i] = i;
            if (rnd == null) rnd = new Random();
            for (int i = 0; i < arr.Length; i++)
            {
                int idx = rnd.Next(0, arr.Length - i);
                arr[i] = copy[indices[idx]];
                indices[idx] = indices[arr.Length - (i + 1)];
            }
        }

        public static float Cross(Vector2 left, Vector2 right)
        {
            return left.X * right.Y - left.Y * right.X;
        }

        public static Vector2 Cross(Vector2 left, float s)
        {
            return new Vector2(s * left.Y, -s * left.X);
        }

        public static Vector2 Cross(float s, Vector2 right)
        {
            return new Vector2(-s * right.Y, s * right.X);
        }

        public static bool Validate(this Vector2 vec)
        {
            return !float.IsNaN(vec.X) && !float.IsNaN(vec.Y) && !float.IsInfinity(vec.X) && !float.IsInfinity(vec.Y);
        }

        public static float ClampAngle(float angle)
        {
            return (float)Math.IEEERemainder(angle, Math.PI * 2.0);
            /*const float pi = (float)Math.PI;
            const float pi2 = pi * 2.0f;
            while (angle > pi) angle -= pi2;
            while (angle < -pi) angle += pi2;
            return angle;*/
        }

        public static Quaternion ForwardToRotation(Vector3 forward)
        {
            forward.Normalize();
            if (forward == Vector3.UnitZ)
            {
                return Quaternion.Identity;
            }
            else if (forward == Vector3.UnitZ * -1.0f)
            {
                return Quaternion.RotationAxis(Vector3.UnitY, (float)Math.PI);
            }
            Vector3 normal = Vector3.Cross(forward, Vector3.UnitZ);
            normal.Normalize();
            float ang = (float)Math.Acos(forward.Z);
            return Quaternion.RotationAxis(normal, -ang);
        }

        public static float Lerp(float a, float b, float mu)
        {
            return (b - a) * mu + a;
        }
    }
}
