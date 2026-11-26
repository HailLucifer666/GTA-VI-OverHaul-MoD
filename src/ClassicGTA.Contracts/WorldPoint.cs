using System;

namespace ClassicGTA.Contracts
{
    /// <summary>
    /// Engine-agnostic 3D world coordinate.
    /// </summary>
    public struct WorldPoint
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public WorldPoint(float x, float y, float z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float DistanceTo(WorldPoint other)
        {
            float dx = X - other.X;
            float dy = Y - other.Y;
            float dz = Z - other.Z;
            return (float)Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }
    }
}
