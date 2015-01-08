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
    }
}
