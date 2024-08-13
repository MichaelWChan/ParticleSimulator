using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace ParticleSim;

public class GaussianFilter
{
	private GL Gl { get; }
	private QuadTool QuadTool { get; }
	private Shader GaussianShader { get; }
	private Shader CopyShader { get; }
	private uint[] PingPongFramebuffers { get; } = new uint[2];
	private uint[] PingPongTextures { get; } = new uint[2];

	public GaussianFilter(GL gl, QuadTool quadTool, Vector2D<int> size)
	{
		Gl = gl;
		QuadTool = quadTool;
		GaussianShader = new Shader(Gl, "ScreenShader.vert", "Gaussian.frag");
		CopyShader = new Shader(Gl, "ScreenShader.vert", "Copy.frag");

		RegenerateFilter(size);
	}

	public void RegenerateFilter(Vector2D<int> size)
	{
		PingPongFramebuffers[0] = Gl.CreateFramebuffer();
		PingPongFramebuffers[1] = Gl.CreateFramebuffer();
		PingPongTextures[0] = Gl.CreateTexture(TextureTarget.Texture2D);
		PingPongTextures[1] = Gl.CreateTexture(TextureTarget.Texture2D);

		Gl.TextureStorage2D(PingPongTextures[0], 1, SizedInternalFormat.Rgb8, (uint)size.X, (uint)size.Y);
		Gl.TextureParameter(PingPongTextures[0], TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
		Gl.TextureParameter(PingPongTextures[0], TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
		Gl.TextureParameter(PingPongTextures[0], TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
		Gl.TextureParameter(PingPongTextures[0], TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

		Gl.TextureStorage2D(PingPongTextures[1], 1, SizedInternalFormat.Rgb8, (uint)size.X, (uint)size.Y);
		Gl.TextureParameter(PingPongTextures[1], TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
		Gl.TextureParameter(PingPongTextures[1], TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
		Gl.TextureParameter(PingPongTextures[1], TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
		Gl.TextureParameter(PingPongTextures[1], TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);

		Gl.NamedFramebufferTexture(PingPongFramebuffers[0], FramebufferAttachment.ColorAttachment0, PingPongTextures[0], 0);
		Gl.NamedFramebufferTexture(PingPongFramebuffers[1], FramebufferAttachment.ColorAttachment0, PingPongTextures[1], 0);
	}
	
	public void Apply(uint framebuffer, uint texture, int iterations)
	{
		bool horizontal = true;
		bool firstIteration = true;

		for (int i = 0; i < iterations; i++)
		{
			Gl.BindFramebuffer(FramebufferTarget.Framebuffer, PingPongFramebuffers[horizontal ? 1 : 0]);
			Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			QuadTool.Draw(firstIteration ? texture : PingPongTextures[horizontal ? 0 : 1], GaussianShader.ShaderName);

			horizontal = !horizontal;
			firstIteration = false;
		}

		// Copy filter data to the original framebuffer
		Gl.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
		Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		QuadTool.Draw(PingPongTextures[horizontal ? 0 : 1], CopyShader.ShaderName);
	}
}