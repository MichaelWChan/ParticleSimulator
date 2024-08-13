// Source code for this derived from https://learnopengl.com/Advanced-Lighting/Bloom

#version 460 core

in vec2 texCoords;

uniform sampler2D texture1;
uniform bool horizontal;
uniform float weight[5] = float[5](0.2270270270, 0.1945945946, 0.1216216216, 0.0540540541, 0.0162162162);

out vec4 outColor;

void main()
{
    vec2 tex_offset = 1.0 / textureSize(texture1, 0); // gets size of single texel
    vec3 result = texture(texture1, texCoords).rgb * weight[0]; // current fragment's contribution
    if(horizontal)
    {
        for(int i = 1; i < 5; ++i)
        {
            result += texture(texture1, texCoords + vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
            result += texture(texture1, texCoords - vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
        }
    }
    else
    {
        for(int i = 1; i < 5; ++i)
        {
            result += texture(texture1, texCoords + vec2(0.0, tex_offset.y * i)).rgb * weight[i];
            result += texture(texture1, texCoords - vec2(0.0, tex_offset.y * i)).rgb * weight[i];
        }
    }
    outColor = vec4(result, 1.0);
}