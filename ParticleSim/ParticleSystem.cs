using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace ParticleSim;

public class ParticleSystem
{
    public static ParticleSystem Instance { get; } = new ParticleSystem();
    private IWindow GameWindow { get; set; } = null!;
    private GL Gl { get; set; } = null!;
    private IInputContext InputContext { get; set; } = null!;
    private Renderer Renderer { get; set; } = null!;
    private World World { get; set; } = null!;
    private int frameCounter;
    private DateTime nextUpdateTime = DateTime.Now.AddSeconds(1);
    public void Run()
    {
        WindowOptions windowOptions = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 800),
            Title = "Particle System",
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible | ContextFlags.Debug, new APIVersion(4, 6)),
            VSync = false,
            FramesPerSecond = 200,
        };

        GameWindow = Window.Create(windowOptions);

        GameWindow.Load += OnLoad;
        GameWindow.Render += OnRender;
        GameWindow.Update += OnUpdate;
        GameWindow.FramebufferResize += OnFramebufferResize;
        GameWindow.Closing += OnClosing;
        
        GameWindow.Run();
    }

    private void OnLoad()
    {   
        Gl = GameWindow.CreateOpenGL();
        InputContext = GameWindow.CreateInput();

        Renderer = new Renderer(Gl, GameWindow);
        World = new World();
    }

    private void OnRender(double deltaTime)
    {
        Renderer.OnRender(deltaTime, World);
    }

    private void OnUpdate(double deltaTime)
    {
        World.Tick(deltaTime, InputContext);

        frameCounter++;
        
        if (DateTime.Now < nextUpdateTime) return;
        Console.WriteLine($"FPS: {frameCounter}");
        Console.WriteLine($"Particles: {Instance.World.Particles.Count}");
        Console.WriteLine($"Colliders: {Instance.World.CollisionParticles.Count}");
        
        frameCounter = 0;
        nextUpdateTime = DateTime.Now.AddSeconds(1);
    }
    
    private void OnFramebufferResize(Vector2D<int> newSize)
    {
        Gl.Viewport(newSize);
        World.ScreenSize = newSize;
        Renderer.OnFrameBufferResize(newSize);
    }
    
    private void OnClosing()
    {
        
    }
}

