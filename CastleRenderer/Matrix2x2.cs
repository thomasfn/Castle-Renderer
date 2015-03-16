using System;

using SlimDX;

namespace CastleRenderer
{

    public struct Matrix2x2 : IEquatable<Matrix2x2>
    {
        public float M11;
        public float M12;
        public float M21;
        public float M22;

        public Vector2 Column1
        {
            get
            {
                return new Vector2(M11, M12);
            }
            set
            {
                M11 = value.X;
                M12 = value.Y;
            }
        }

        public Vector2 Column2
        {
            get
            {
                return new Vector2(M21, M22);
            }
            set
            {
                M21 = value.X;
                M22 = value.Y;
            }
        }

        public Matrix2x2(Vector2 col1, Vector2 col2)
        {
            M11 = col1.X;
            M12 = col1.Y;
            M21 = col2.X;
            M22 = col2.Y;
        }

        public static bool operator !=(Matrix2x2 left, Matrix2x2 right)
        {
            return left.M11 != right.M11 || left.M12 != right.M12 || left.M21 != right.M21 || left.M22 != right.M22;
        }
        
        public static bool operator ==(Matrix2x2 left, Matrix2x2 right)
        {
            return left.M11 == right.M11 && left.M12 == right.M12 && left.M21 == right.M21 && left.M22 == right.M22;
        }

        public static Matrix2x2 operator *(Matrix2x2 left, Matrix2x2 right)
        {
            // Don't ask
            Matrix2x2 matrixx = new Matrix2x2();
            float num4 = right.M21;
            float num10 = left.M12;
            float num9 = left.M11;
            float num3 = right.M11;
            matrixx.M11 = (num3 * num9) + (num10 * num4);
            float num2 = right.M22;
            float num = right.M12;
            matrixx.M12 = (num * num9) + (num2 * num10);
            float num8 = left.M22;
            float num7 = left.M21;
            matrixx.M21 = (num8 * num4) + (num7 * num3);
            matrixx.M22 = (num7 * num) + (num8 * num2);
            return matrixx;
        }


        public static Matrix2x2 Identity
        {
            get
            {
                return new Matrix2x2(Vector2.UnitX, Vector2.UnitY);
            }
        }
        public bool IsIdentity
        {
            get
            {
                return this == Identity;
            }
        }
        public bool IsInvertible
        {
            get
            {
                return (M11 - M22 != 0.0f) && (M12 - M21 != 0.0f);
            }
        }

        public float Determinant()
        {
            return (M11 * M22) - (M12 * M21);
        }
        public bool Equals(Matrix2x2 other)
        {
            return M11 == other.M11 && M12 == other.M12 && M21 == other.M21 && M22 == other.M22;
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Matrix2x2)) return false;
            Matrix2x2 other = (Matrix2x2)obj;
            return M11 == other.M11 && M12 == other.M12 && M21 == other.M21 && M22 == other.M22;
        }
        public static bool Equals(ref Matrix3x2 left, ref Matrix3x2 right)
        {
            return left.M11 == right.M11 && left.M12 == right.M12 && left.M21 == right.M21 && left.M22 == right.M22;
        }
        public override int GetHashCode()
        {
            return M11.GetHashCode() ^ M12.GetHashCode() ^ M21.GetHashCode() ^ M22.GetHashCode();
        }
        public bool Invert()
        {
            float det = Determinant();
            if (det == 0.0f) return false;
            M11 = M11 / det;
            M12 = M21 / det;
            M21 = M12 / det;
            M22 = M22 / det;
            return true;
        }
        public static Matrix2x2 Invert(Matrix2x2 mat)
        {
            if (!mat.Invert()) throw new InvalidOperationException();
            return mat;
        }
        public static void Invert(ref Matrix2x2 mat, out Matrix2x2 result)
        {
            float det = mat.Determinant();
            if (det == 0.0f) throw new InvalidOperationException();
            result = new Matrix2x2
            {
                M11 = mat.M11 / det,
                M12 = mat.M21 / det,
                M21 = mat.M12 / det,
                M22 = mat.M22 / det
            };
        }

        public static Matrix2x2 Rotation(float angle)
        {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);
            return new Matrix2x2(new Vector2(c, -s), new Vector2(s, c));
        }
        public static void Rotation(float angle, out Matrix2x2 result)
        {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);
            result = new Matrix2x2(new Vector2(c, -s), new Vector2(s, c));
        }

        public Vector2 Transform(Vector2 point)
        {
            return new Vector2(M11 * point.X + M12 * point.Y, M21 * point.X + M22 * point.Y);
        }

        public static Vector2 Transform(Matrix2x2 matrix, Vector2 point)
        {
            return matrix.Transform(point);
        }

        public static void Transform(ref Matrix2x2 matrix, ref Vector2 point, out Vector2 transformed)
        {
            transformed = new Vector2(matrix.M11 * point.X + matrix.M12 * point.Y, matrix.M21 * point.X + matrix.M22 * point.Y);
        }
    }
}
