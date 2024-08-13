using Silk.NET.Maths;

namespace ParticleSim;

public class Particle
{
	public float Radius { get; set; } = 0.3f;
	public Vector2D<float> Position { get; set; }
	public Vector4D<float> Color { get; set; }
	public float Lifespan { get; set; }
	public float MaxLifespan { get; set; }
	public Vector2D<float> Velocity { get; set; }
	public Vector2D<float> Acceleration { get; set; }
}