#include "../common.h"

sampler2D LogoTexture : register(s0);

float4 Source;

float4 LogoFadeShaderFragment(float2 svPos : SV_POSITION0, float2 uv : TEXCOORD0, float4 baseColor : COLOR0) : COLOR0
{
    float4 color = tex2D(LogoTexture, uv);
    
    float2 fadeUv = svPos - Source.zw;
    fadeUv /= Source.xy;
    
    float fade = 1 - fadeUv.x;
    
    fade = 1 - pow(1 - fade, 7);
    
    fade *= 1 - pow((abs(fadeUv.y - 0.5) * 2), 6.5);
    
    return color * baseColor * fade;
}

BEGIN_TECHNIQUE(Technique1)
    BEGIN_PASS(LogoFadeShader) 
        PIXEL_SHADER(compile ps_3_0 LogoFadeShaderFragment()) 
    END_PASS
END_TECHNIQUE
