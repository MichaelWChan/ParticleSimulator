#version 460 core

in vec2 texCoords;

uniform sampler2D texture1;

out vec4 outColor;

// Copies a texture to the output texture
void main()
{
    outColor = texture(texture1, texCoords);
}