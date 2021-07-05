namespace SoarCraft.QYun.AssetReader.Math {
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector4 : IEquatable<Vector4> {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Vector4(float x, float y, float z, float w) {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vector4(Vector3 value, float w) {
            X = value.X;
            Y = value.Y;
            Z = value.Z;
            W = w;
        }

        public float this[int index] {
            get => index switch {
                0 => X,
                1 => Y,
                2 => Z,
                3 => W,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Invalid Vector4 index!"),
            };

            set {
                switch (index) {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    case 3:
                        W = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index), "Invalid Vector4 index!");
                }
            }
        }

        public override int GetHashCode() => X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2) ^ (W.GetHashCode() >> 1);

        public override bool Equals(object obj) => obj is Vector4 vector4 && this.Equals(vector4);

        public bool Equals(Vector4 other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);

        public void Normalize() {
            var length = Length();
            if (length > KEpsilon) {
                var invNorm = 1.0f / length;
                X *= invNorm;
                Y *= invNorm;
                Z *= invNorm;
                W *= invNorm;
            } else {
                X = 0;
                Y = 0;
                Z = 0;
                W = 0;
            }
        }

        public float Length() => (float)Math.Sqrt(LengthSquared());

        public float LengthSquared() => (this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z) + (this.W * this.W);

        public static Vector4 Zero => new();

        public static Vector4 operator +(Vector4 a, Vector4 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);

        public static Vector4 operator -(Vector4 a, Vector4 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);

        public static Vector4 operator -(Vector4 a) => new(-a.X, -a.Y, -a.Z, -a.W);

        public static Vector4 operator *(Vector4 a, float d) => new(a.X * d, a.Y * d, a.Z * d, a.W * d);

        public static Vector4 operator *(float d, Vector4 a) => new(a.X * d, a.Y * d, a.Z * d, a.W * d);

        public static Vector4 operator /(Vector4 a, float d) => new(a.X / d, a.Y / d, a.Z / d, a.W / d);

        public static bool operator ==(Vector4 lhs, Vector4 rhs) => (lhs - rhs).LengthSquared() < KEpsilon * KEpsilon;

        public static bool operator !=(Vector4 lhs, Vector4 rhs) => !(lhs == rhs);

        public static implicit operator Vector2(Vector4 v) => new(v.X, v.Y);

        public static implicit operator Vector3(Vector4 v) => new(v.X, v.Y, v.Z);

        public static implicit operator Color(Vector4 v) => new(v.X, v.Y, v.Z, v.W);

        private const float KEpsilon = 0.00001F;
    }
}
