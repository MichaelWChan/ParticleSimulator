using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace ParticleSim;

[StructLayout(LayoutKind.Sequential)]

public struct ScreenShaderVertex
{
    public Vector2D<float> Position;
    public Vector2D<float> TextureCoordinates;
}