using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace ParticleSim;

public class QuadTool
{
	private GL Gl { get; }
	private uint ScreenVAO { get; }

	private readonly ScreenShaderVertex[] screenShaderVertexQuad =
	{
		// Lower left
		new ScreenShaderVertex
		{
			Position = new Vector2D<float>(-1,-1),
			TextureCoordinates = new Vector2D<float>(0, 0)
		},
		// Lower right
		new ScreenShaderVertex
		{
			Position = new Vector2D<float>(1,-1),
			TextureCoordinates = new Vector2D<float>(1, 0)
		},
		// Upper right
		new ScreenShaderVertex
		{
			Position = new Vector2D<float>(1,1),
			TextureCoordinates = new Vector2D<float>(1, 1)
		},
		// Upper left
		new ScreenShaderVertex
		{
			Position = new Vector2D<float>(-1,1),
			TextureCoordinates = new Vector2D<float>(0, 1)
		},
	};

	private readonly uint[] screenShaderVertexQuadElements = [0, 1, 2, 2, 3, 0];

	public QuadTool(GL gl)
	{
		Gl = gl;
		ScreenVAO = Gl.CreateVertexArray();

		Gl.EnableVertexArrayAttrib(ScreenVAO, 0);
		Gl.EnableVertexArrayAttrib(ScreenVAO, 1);
		Gl.VertexArrayAttribFormat(ScreenVAO, 0, 2, VertexAttribType.Float, false, 0);
		Gl.VertexArrayAttribFormat(ScreenVAO, 1, 2, VertexAttribType.Float, false, 2 * sizeof(float));
		Gl.VertexArrayAttribBinding(ScreenVAO, 0, 0);
		Gl.VertexArrayAttribBinding(ScreenVAO, 1, 0);
	}
	
	public void Draw(uint texture, uint shader)
	{
		Gl.UseProgram(shader);
		Gl.BindVertexArray(ScreenVAO);
		Gl.BindTextureUnit(1, texture);
		int uniformLocation3 = Gl.GetUniformLocation(shader, "texture1");
		Gl.Uniform1(uniformLocation3, 1);

		uint screenBuffer = Gl.CreateBuffer();
		Gl.NamedBufferStorage(screenBuffer, MemoryMarshal.AsBytes((ReadOnlySpan<ScreenShaderVertex>)screenShaderVertexQuad.AsSpan()), BufferStorageMask.None);

		uint indexBuffer = Gl.CreateBuffer();
		Gl.NamedBufferStorage(indexBuffer, MemoryMarshal.AsBytes((ReadOnlySpan<uint>)screenShaderVertexQuadElements.AsSpan()), BufferStorageMask.None);

		Gl.VertexArrayVertexBuffer(ScreenVAO, 0, screenBuffer, 0, (uint)Unsafe.SizeOf<ScreenShaderVertex>());
		Gl.VertexArrayElementBuffer(ScreenVAO, indexBuffer);

		unsafe { Gl.DrawElements(GLEnum.Triangles, (uint)screenShaderVertexQuadElements.Length, DrawElementsType.UnsignedInt, (void*)0); }

		Gl.DeleteBuffer(screenBuffer);
		Gl.DeleteBuffer(indexBuffer);
	}
}