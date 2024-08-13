#version 460 core

layout (location = 0) in vec2 position;
layout (location = 1) in vec4 color;
layout (location = 2) in vec2 textureCoordinates;

uniform mat4 projection;

out vec4 vertexColor;
out vec2 texCoords; // Distance of fragment from center

void main()
{
    gl_Position = projection * vec4(position, 0.0, 1.0);
    vertexColor = color;
    texCoords = textureCoordinates;
}