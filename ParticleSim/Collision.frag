#version 460 core

in vec4 vertexColor;
in vec2 texCoords;

uniform sampler2D texture1;

out vec4 outColor;

void main()
{
    outColor = vertexColor;

    // If length of texCoords is greater than 1, discard the fragment
    if (length(texCoords) > 1)
    {
        discard;
    }
}