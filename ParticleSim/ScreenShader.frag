#version 460 core

in vec2 texCoords;

uniform sampler2D texture1;

out vec4 outColor;

void main()
{
    vec4 textureSampledColor = texture(texture1, texCoords);
    outColor = textureSampledColor;
}