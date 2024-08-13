using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using StbImageSharp;

namespace ParticleSim;

public class Renderer
{
    private uint VertexVAO { get; }
    private GL Gl { get; }
    private Shader Shader { get; }
    private Shader ShaderCollision { get; }
    private Shader ScreenShader { get; }
    private IWindow Window { get; }
    private float AspectRatio => Window.Size.X / (float)Window.Size.Y;
    private const float Height = 10;
    private readonly uint outId;
    private uint frameBuffer;
    private uint colorTexture;

    private GaussianFilter GaussianFilter { get; }
    private QuadTool QuadTool { get; }


    public Renderer(GL gl, IWindow window)
    {
        Gl = gl;
        Window = window;
        VertexVAO = Gl.CreateVertexArray();
        
        Gl.ClearColor(Color.Black);
        Gl.Enable(EnableCap.Blend);
        // Gl.Enable(EnableCap.DepthTest);
        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        Gl.EnableVertexArrayAttrib(VertexVAO, 0);
        Gl.EnableVertexArrayAttrib(VertexVAO, 1);
        Gl.EnableVertexArrayAttrib(VertexVAO, 2);
        Gl.VertexArrayAttribFormat(VertexVAO, 0, 2, VertexAttribType.Float, false, 0);
        Gl.VertexArrayAttribFormat(VertexVAO, 1, 4, VertexAttribType.Float, false, 2 * sizeof(float));
        Gl.VertexArrayAttribFormat(VertexVAO, 2, 2, VertexAttribType.Float, false, 6 * sizeof(float));
        Gl.VertexArrayAttribBinding(VertexVAO, 0, 0);
        Gl.VertexArrayAttribBinding(VertexVAO, 1, 0);
        Gl.VertexArrayAttribBinding(VertexVAO, 2, 0);

        Shader = new Shader(Gl, "Vertex.vert", "Vertex.frag");
        ShaderCollision = new Shader(Gl, "Vertex.vert", "Collision.frag");
        
        // Textures
        using FileStream fs = File.OpenRead("fire1.png");
        ImageResult image = ImageResult.FromStream(fs);
        ReadOnlySpan<byte> data = image.Data;

        outId = Gl.CreateTexture(TextureTarget.Texture2D);
        Gl.TextureStorage2D(outId, 1, SizedInternalFormat.Rgba8, (uint)image.Width, (uint)image.Height);
        Gl.TextureSubImage2D(outId, 0, 0, 0, (uint)image.Width, (uint)image.Height, PixelFormat.Rgba, PixelType.UnsignedByte, data);


        // Framebuffer
        BuildFramebuffer();

        QuadTool = new QuadTool(Gl);
        GaussianFilter = new GaussianFilter(Gl, QuadTool, Window.Size);
        
        ScreenShader = new Shader(Gl, "ScreenShader.vert", "ScreenShader.frag");
    }

    public void OnRender(double deltaTime, World world)
    {
        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        world.SortParticlesByColor();
        RenderWorld(world);
    }

    private void RenderWorld(World world)
    {
        // Framebuffer
        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        Gl.UseProgram(Shader.ShaderName);
        Gl.BindVertexArray(VertexVAO);
        Gl.BindTextureUnit(0, outId);
        
        Matrix4X4<float> ortho = Matrix4X4.CreateOrthographicOffCenter(0, Height * AspectRatio, 0, Height, -1, 1);
        int uniformLocation = Gl.GetUniformLocation(Shader.ShaderName, "projection");
        int uniformLocation2 = Gl.GetUniformLocation(Shader.ShaderName, "texture1");
        Gl.UniformMatrix4(uniformLocation, 1, false, 
            MemoryMarshal.Cast<Matrix4X4<float>, float>(new ReadOnlySpan<Matrix4X4<float>>(in ortho))); // Cast matrix to raw bytes
        Gl.Uniform1(uniformLocation2, 0);
        
        foreach (Particle particle in world.Particles)
        {
            DrawCircle(particle.Position, particle.Radius, particle.Color);
        }
        
        // Draw collision circle
        Gl.UseProgram(ShaderCollision.ShaderName);
        uniformLocation = Gl.GetUniformLocation(ShaderCollision.ShaderName, "projection");
        Gl.UniformMatrix4(uniformLocation, 1, false, 
            MemoryMarshal.Cast<Matrix4X4<float>, float>(new ReadOnlySpan<Matrix4X4<float>>(in ortho))); // Cast matrix to raw bytes
        foreach (Particle particle in world.CollisionParticles)
        {
            DrawCircle(particle.Position, particle.Radius, particle.Color);
        }

        // Gaussian blur
        GaussianFilter.Apply(frameBuffer, colorTexture, 8);

        // End of framebuffer
        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        QuadTool.Draw(colorTexture, ScreenShader.ShaderName);
    }

    private unsafe void DrawCircle(Vector2D<float> position, float radius, Vector4D<float> color)
    {
        Vertex[] vertexArray =
        {
            // Lower left
            new Vertex
            {
                Position = new Vector2D<float>(position.X - radius,position.Y - radius),
                Color = color,
                TextureCoordinates = new Vector2D<float>(-1, -1)
            },
            // Lower right
            new Vertex
            {
                Position = new Vector2D<float>(position.X + radius,position.Y - radius),
                Color = color,
                TextureCoordinates = new Vector2D<float>(1, -1)
            },
            // Upper right
            new Vertex
            {
                Position = new Vector2D<float>(position.X + radius,position.Y + radius),
                Color = color,
                TextureCoordinates = new Vector2D<float>(1, 1)
            },
            // Upper left
            new Vertex
            {
                Position = new Vector2D<float>(position.X - radius,position.Y + radius),
                Color = color,
                TextureCoordinates = new Vector2D<float>(-1, 1)
            },
        };

        uint[] ebo = [0, 1, 2, 2, 3, 0];

        uint vertexBuffer = Gl.CreateBuffer();
        Gl.NamedBufferStorage(vertexBuffer, MemoryMarshal.AsBytes((ReadOnlySpan<Vertex>)vertexArray.AsSpan()), BufferStorageMask.None);

        uint indexBuffer = Gl.CreateBuffer();
        Gl.NamedBufferStorage(indexBuffer, MemoryMarshal.AsBytes((ReadOnlySpan<uint>)ebo.AsSpan()), BufferStorageMask.None);

        Gl.VertexArrayVertexBuffer(VertexVAO, 0, vertexBuffer, 0, (uint)Unsafe.SizeOf<Vertex>());
        Gl.VertexArrayElementBuffer(VertexVAO, indexBuffer);

        Gl.DrawElements(GLEnum.Triangles, (uint)ebo.Length, DrawElementsType.UnsignedInt, (void*)0);
        
        Gl.DeleteBuffer(vertexBuffer);
        Gl.DeleteBuffer(indexBuffer);
    }

    private void BuildFramebuffer()
    {
        frameBuffer = Gl.CreateFramebuffer();

        colorTexture = Gl.CreateTexture(TextureTarget.Texture2D);
        Gl.TextureStorage2D(colorTexture, 1, SizedInternalFormat.Rgb8, (uint)Window.Size.X, (uint)Window.Size.Y);
        Gl.TextureParameter(colorTexture, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        Gl.TextureParameter(colorTexture, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        Gl.TextureParameter(colorTexture, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        Gl.TextureParameter(colorTexture, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

        Gl.NamedFramebufferTexture(frameBuffer, FramebufferAttachment.ColorAttachment0, colorTexture, 0);
    }

    public void OnFrameBufferResize(Vector2D<int> newSize)
    {
        Gl.Viewport(newSize);
        BuildFramebuffer();
        GaussianFilter.RegenerateFilter(newSize);
    }
}