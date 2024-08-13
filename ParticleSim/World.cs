using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace ParticleSim;

public class World
{
    public List<Particle> Particles { get; } = new List<Particle>();
    private Random Rand { get; } = new Random();
    private static readonly Vector2D<float> StartingPosition = new Vector2D<float>(5, 2);
    private bool IsSpacebarPressed { get; set; }
    private bool WasSpacebarPressed { get; set; }
    private int particleSpawnSpeed = 500;
    private const float ParticleLifeSpan = 1.0f;
    private const float ParticleRadius = 0.3f;
    public Vector2D<int> ScreenSize { get; set; } = new Vector2D<int>(800, 800);
    public List<Particle> CollisionParticles { get; } = new List<Particle>();

    public void Tick(double deltaTime, IInputContext inputContext)
    {
        object lockObject = new object();
        HandleMouseInput(inputContext, lockObject);
        HandleKeyboardInput(inputContext, lockObject);
        
        List<Particle> particlesToRemove = [];

        Vector2D<float> gravity = new Vector2D<float>(0, -5);
        
        // Update particle properties
        Parallel.ForEach(Particles, particle =>
        {
            particle.Lifespan -= (float)deltaTime;
            
            // Remove if lifespan is less than or equal to 0
            if (particle.Lifespan <= 0)
            {
                lock (lockObject)
                {
                    particlesToRemove.Add(particle);
                }
            }
            else
            {
                particle.Color = GetColor(particle.Lifespan / particle.MaxLifespan);

                if (particle.Lifespan < particle.MaxLifespan / 3)
                {
                    float radius = particle.Lifespan / particle.MaxLifespan * ParticleRadius * 3;
                    particle.Radius = Math.Clamp(radius, 0.01f, ParticleRadius);
                }

                particle.Acceleration += gravity * (float)deltaTime; // Apply gravity

                particle.Velocity += particle.Acceleration * (float)deltaTime; // Euler integration
                particle.Position += particle.Velocity * (float)deltaTime;

                foreach (Particle collisionParticle in CollisionParticles)
                {
                    if (Vector2D.Distance(particle.Position, collisionParticle.Position) < particle.Radius + collisionParticle.Radius - 0.15f)
                    {
                        particle.Position = collisionParticle.Position + Vector2D.Normalize(particle.Position - collisionParticle.Position) * (particle.Radius + CollisionParticles[0].Radius);
                    }
                }

                // Clamp velocity
                particle.Velocity =
                    new Vector2D<float>(Clamp(particle.Velocity.X, -5, 5), Clamp(particle.Velocity.Y, -5, 5));
            }
        });
        
        foreach (Particle particle in particlesToRemove)
        {
            Particles.Remove(particle);
        }
    }
    
    private void HandleMouseInput(IInputContext inputContext, object lockObject)
    {
        // Wind effect
        if (inputContext.Mice[0].IsButtonPressed(MouseButton.Right))
        {
            Vector2 mPosition = inputContext.Mice[0].Position;
            Vector2D<float> mousePos = ScreenToWorld(new Vector2D<float>(mPosition.X, mPosition.Y), ScreenSize, new Vector2D<float>(10, 10));
            Parallel.ForEach(Particles, particle =>
            {
                Vector2D<float> direction = Vector2D.Normalize(particle.Position - mousePos);
                float distance = Vector2D.Distance(particle.Position, mousePos);
                if (distance <= 4) // Only apply wind effect if distance is less than or equal to 2
                {
                    float windStrength = 2 / (distance > 0 ? distance : 1); // The closer the distance, the stronger the wind
                    particle.Acceleration += direction * windStrength;
                }
            });
        }
        
        // Create particles on left mouse click
        if (inputContext.Mice[0].IsButtonPressed(MouseButton.Left))
        {
            Vector2 mPosition = inputContext.Mice[0].Position;
            Vector2D<float> mousePos = ScreenToWorld(new Vector2D<float>(mPosition.X, mPosition.Y), ScreenSize, new Vector2D<float>(10, 10));
            // Console.WriteLine(mousePos);

            GenerateParticles(mousePos, lockObject);
        }
        
        // Spawn collision particle on middle mouse click
        if (inputContext.Mice[0].IsButtonPressed(MouseButton.Middle))
        {
            Vector2 mPosition = inputContext.Mice[0].Position;
            Vector2D<float> mousePos = ScreenToWorld(new Vector2D<float>(mPosition.X, mPosition.Y), ScreenSize, new Vector2D<float>(10, 10));
            Particle particle = new Particle
            {
                Position = mousePos,
                Color = new Vector4D<float>(0.1f, 0.1f, 0.1f, 1),
                // Color = new Vector4D<float>(0.0f, 0.0f, 0.0f, 1), // Cool effect
                Radius = 1,
            };
            CollisionParticles.Add(particle);
        }
    }
    
    private void HandleKeyboardInput(IInputContext inputContext, object lockObject)
    {
        // Toggle particle generation on spacebar press
        bool isSpacebarCurrentlyPressed = inputContext.Keyboards[0].IsKeyPressed(Key.Space);

        if (isSpacebarCurrentlyPressed && !WasSpacebarPressed)
        {
            IsSpacebarPressed = !IsSpacebarPressed;
        }

        WasSpacebarPressed = isSpacebarCurrentlyPressed;

        if (IsSpacebarPressed)
        {
            GenerateParticles(StartingPosition, lockObject);
        }
        
        // Clear particles on R key press
        if (inputContext.Keyboards[0].IsKeyPressed(Key.R))
        {
            CollisionParticles.Clear();
            Particles.Clear();
        }
        
        Dictionary<Key, int> keyToNumberMapping = new Dictionary<Key, int>
        {
            { Key.Number1, 1 }, 
            { Key.Number2, 100 },
            { Key.Number3, 200 },
            { Key.Number4, 300 },
            { Key.Number5, 400 },
            { Key.Number6, 500 },
            { Key.Number7, 1000 },
            { Key.Number8, 2000 },
            { Key.Number9, 5000 }
        };

        foreach (KeyValuePair<Key, int> keyNumberPair in keyToNumberMapping)
        {
            if (inputContext.Keyboards[0].IsKeyPressed(keyNumberPair.Key))
            {
                particleSpawnSpeed = keyNumberPair.Value;
            }
        }
    }
    
    private void GenerateParticles(Vector2D<float> spawnPosition, object lockObject)
    {   
        // Generate particles at a given position
        Parallel.For(0, particleSpawnSpeed, _ =>
        {
            float angle = (float)(Rand.NextDouble() * 2 * Math.PI);
            
            float accelerationX = 2 * (float)Math.Cos(angle) + (float)(Rand.NextDouble() * 2 - 1);
            float accelerationY = 4 * Math.Abs((float)Math.Sin(angle) + (float)(Rand.NextDouble() * 2 - 1));
            float scale = Math.Clamp(accelerationY / (Math.Abs(accelerationX) > 0 ? Math.Abs(accelerationX) : 1), ParticleLifeSpan, ParticleLifeSpan * 1.2f);
                
            Particle particle = new Particle
            {
                Position = spawnPosition + GenerateRandomPoint(0.1f),
                Color = new Vector4D<float>(1, 0, 0, 1),
                Lifespan = ParticleLifeSpan * ((float)Rand.NextDouble() * 2 - 1) * scale,
                Velocity = new Vector2D<float>(0, 0),
                Acceleration = new Vector2D<float>(accelerationX, accelerationY)
            };
                
            particle.MaxLifespan = particle.Lifespan;
            lock (lockObject)
            {
                Particles.Add(particle);
            }
        });
    }
    
    public void SortParticlesByColor()
    {
        // Sort particles based on the brightness of the color
        Particles.Sort((p1, p2) => 
        {
            float brightness1 = (p1.Color.X + p1.Color.Y + p1.Color.Z) / 3;
            float brightness2 = (p2.Color.X + p2.Color.Y + p2.Color.Z) / 3;
            return brightness1.CompareTo(brightness2);
        });
    }
    
    private static Vector2D<float> ScreenToWorld(Vector2D<float> screenPoint, Vector2D<int> screenSize, Vector2D<float> worldSize)
    {
        float aspectRatio = screenSize.X / (float)screenSize.Y;
        
        // Calculate scaling factors for X and Y coordinates
        float scaleX = worldSize.X / screenSize.X * aspectRatio;
        float scaleY = worldSize.Y / screenSize.Y;

        // Scale the coordinates
        float worldX = screenPoint.X * scaleX;
        float worldY = 10 - screenPoint.Y * scaleY;

        return new Vector2D<float>(worldX, worldY);
    }

    private Vector2D<float> GenerateRandomPoint(float radius)
    {
        // Generate random angle within [0, 2π)
        float angle = (float)(Rand.NextDouble() * 2 * Math.PI);
        // Generate random distance within [0, radius)
        float distance = (float)Math.Sqrt(Rand.NextDouble()) * radius;
        // Calculate the x and y coordinates using polar to cartesian conversion
        float x = distance * (float)Math.Cos(angle);
        float y = distance * (float)Math.Sin(angle);
    
        return new Vector2D<float>(x, y);
    }
    
    private static Vector4D<float> GetColor(float lifespanRatio)
    {

        float colorRed = Math.Clamp(lifespanRatio * 3, 0, 1);
        lifespanRatio -= 1 / 3f;
        float colorGreen = Math.Clamp(lifespanRatio * 3, 0, 1);
        lifespanRatio -= 1 / 3f;
        float colorBlue = Math.Clamp(lifespanRatio * 3, 0, 1);
        
        return new Vector4D<float>(colorRed, colorGreen, colorBlue, 1);

    }
    
    private static float Clamp(float value, float min, float max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
}