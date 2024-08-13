#version 460 core

in vec4 vertexColor;
in vec2 texCoords;

uniform sampler2D texture1;

out vec4 outColor;

void main()
{
    outColor = texture(texture1, vec2((texCoords.x + 1) / 2, (texCoords.y + 1) / 2)) * vertexColor;
}