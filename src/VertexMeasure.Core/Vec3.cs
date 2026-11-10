// MD Vertex Measure - lightweight 3D vector (no NW dependency)
// Author: MD (Mayank Dholariya)
using System;

namespace VertexMeasure.Core
{
    public struct Vec3 : IEquatable<Vec3>
    {
        public double X;
        public double Y;
        public double Z;

        public Vec3(double x, double y, double z) { X = x; Y = y; Z = z; }

        public static Vec3 operator +(Vec3 a, Vec3 b) { return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
        public static Vec3 operator -(Vec3 a, Vec3 b) { return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }
        public static Vec3 operator *(Vec3 a, double s) { return new Vec3(a.X * s, a.Y * s, a.Z * s); }
        public static Vec3 operator *(double s, Vec3 a) { return a * s; }
        public static Vec3 operator /(Vec3 a, double s) { return new Vec3(a.X / s, a.Y / s, a.Z / s); }

        public double LengthSquared { get { return X * X + Y * Y + Z * Z; } }
        public double Length { get { return Math.Sqrt(LengthSquared); } }

        public Vec3 Normalized()
        {
            double len = Length;
            if (len < 1e-12) return new Vec3(0, 0, 0);
            return this / len;
        }

        public static double Dot(Vec3 a, Vec3 b) { return a.X * b.X + a.Y * b.Y + a.Z * b.Z; }

        public static Vec3 Cross(Vec3 a, Vec3 b)
        {
            return new Vec3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        public static double Distance(Vec3 a, Vec3 b) { return (a - b).Length; }

        public bool Equals(Vec3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj) { return obj is Vec3 && Equals((Vec3)obj); }
        public override int GetHashCode() { return X.GetHashCode() ^ (Y.GetHashCode() << 1) ^ (Z.GetHashCode() << 2); }
        public override string ToString() { return string.Format(System.Globalization.CultureInfo.InvariantCulture, "({0:G6},{1:G6},{2:G6})", X, Y, Z); }
    }
}
