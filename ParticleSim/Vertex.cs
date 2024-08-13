using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace ParticleSim;

[StructLayout(LayoutKind.Sequential)]

public struct Vertex
{
    public Vector2D<float> Position;
    public Vector4D<float> Color;
    public Vector2D<float> TextureCoordinates;
}