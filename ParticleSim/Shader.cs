using Silk.NET.OpenGL;

namespace ParticleSim;

public class Shader
{
    private GL Gl { get; }
    public uint ShaderName { get; }

    public Shader(GL gl, string vertexFilePath, string fragmentFilePath)
    {
        Gl = gl;
        
        uint vertex = Gl.CreateShader(ShaderType.VertexShader);
        Gl.ShaderSource(vertex, File.ReadAllText(vertexFilePath));
        Gl.CompileShader(vertex);
        
        uint fragment = Gl.CreateShader(ShaderType.FragmentShader);
        Gl.ShaderSource(fragment, File.ReadAllText(fragmentFilePath));
        Gl.CompileShader(fragment);
        
        ShaderName = Gl.CreateProgram();
        Gl.AttachShader(ShaderName, vertex);
        Gl.AttachShader(ShaderName, fragment);
        Gl.LinkProgram(ShaderName);
        
        Gl.DeleteShader(vertex);
        Gl.DeleteShader(fragment);
    }
}