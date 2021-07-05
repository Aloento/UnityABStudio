namespace SoarCraft.QYun.AssetReader.Math {
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector2 : IEquatable<Vector2> {
        public float X;
        public float Y;

        public Vector2(float x, float y) {
            X = x;
            Y = y;
        }

        public float this[int index] {
            get => index switch {
                0 => X,
                1 => Y,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Invalid Vector2 index!"),
            };

            set {
                switch (index) {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index), "Invalid Vector2 index!");
                }
            }
        }

        public override int GetHashCode() => X.GetHashCode() ^ (Y.GetHashCode() << 2);

        public override bool Equals(object obj) => obj is Vector2 vector2 && this.Equals(vector2);

        public bool Equals(Vector2 other) => X.Equals(other.X) && Y.Equals(other.Y);

        public void Normalize() {
            var length = Length();
            if (length > KEpsilon) {
                var invNorm = 1.0f / length;
                X *= invNorm;
                Y *= invNorm;
            } else {
                X = 0;
                Y = 0;
            }
        }

        public float Length() => (float)Math.Sqrt(LengthSquared());

        public float LengthSquared() => (this.X * this.X) + (this.Y * this.Y);

        public static Vector2 Zero => new();

        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);

        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);

        public static Vector2 operator *(Vector2 a, Vector2 b) => new(a.X * b.X, a.Y * b.Y);

        public static Vector2 operator /(Vector2 a, Vector2 b) => new(a.X / b.X, a.Y / b.Y);

        public static Vector2 operator -(Vector2 a) => new(-a.X, -a.Y);

        public static Vector2 operator *(Vector2 a, float d) => new(a.X * d, a.Y * d);

        public static Vector2 operator *(float d, Vector2 a) => new(a.X * d, a.Y * d);

        public static Vector2 operator /(Vector2 a, float d) => new(a.X / d, a.Y / d);

        public static bool operator ==(Vector2 lhs, Vector2 rhs) => (lhs - rhs).LengthSquared() < KEpsilon * KEpsilon;

        public static bool operator !=(Vector2 lhs, Vector2 rhs) => !(lhs == rhs);

        public static implicit operator Vector3(Vector2 v) => new(v.X, v.Y, 0);

        public static implicit operator Vector4(Vector2 v) => new(v.X, v.Y, 0.0F, 0.0F);

        private const float KEpsilon = 0.00001F;
    }
}
