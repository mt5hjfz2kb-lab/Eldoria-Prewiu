Shader "Eldoria/ValoriaTerrainCliffBlend"
{
    Properties {
        _GrassLow("Grass shadows", Color) = (0.20,0.24,0.105,1)
        _GrassHigh("Grass highlights", Color) = (0.43,0.42,0.23,1)
        _RockLow("Weathered limestone shadows", Color) = (0.24,0.22,0.19,1)
        _RockHigh("Weathered limestone highlights", Color) = (0.53,0.49,0.40,1)
        _CliffThreshold("Cliff slope threshold", Range(0.3,0.98)) = 0.47
        _CliffFeather("Cliff slope feather", Range(0.05,0.7)) = 0.31
    }
    SubShader {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _GrassLow, _GrassHigh, _RockLow, _RockHigh;
                half _CliffThreshold, _CliffFeather;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings {
                float4 positionCS:SV_POSITION;
                float3 positionWS:TEXCOORD0;
                half3 normalWS:TEXCOORD1;
                float4 shadowCoord:TEXCOORD2;
            };
            float Hash21(float2 p) {
                p = frac(p*float2(123.34,345.45));
                p += dot(p,p+34.345);
                return frac(p.x*p.y);
            }
            float Noise(float2 p) {
                float2 c=floor(p),f=frac(p);
                f=f*f*(3.0-2.0*f);
                return lerp(lerp(Hash21(c),Hash21(c+float2(1,0)),f.x),
                    lerp(Hash21(c+float2(0,1)),Hash21(c+float2(1,1)),f.x),f.y);
            }
            Varyings Vert(Attributes v) {
                Varyings o;
                o.positionWS=TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.positionWS);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                o.shadowCoord=TransformWorldToShadowCoord(o.positionWS);
                return o;
            }
            half4 Frag(Varyings i):SV_Target {
                float3 n=normalize(i.normalWS);
                // A continuous normal-based transition, not the hard per-triangle
                // rock/grass selection that gave Valoria jagged green stripes.
                float cliff=smoothstep(_CliffThreshold-_CliffFeather,_CliffThreshold,
                    1.0-saturate(n.y));
                float2 flat=i.positionWS.xz;
                float grassN=Noise(flat*0.28)*0.65+Noise(flat*1.48)*0.35;
                float3 grass=lerp(_GrassLow.rgb,_GrassHigh.rgb,grassN);
                // Project coherent sediment patterns on vertical faces, not
                // stretched grass UVs across an abrupt 4 m elevation change.
                float2 face=abs(n.x)>abs(n.z)?i.positionWS.zy:i.positionWS.xy;
                float broad=Noise(face*0.40);
                float grain=Noise(face*2.15);
                float layers=sin(i.positionWS.y*4.6+Noise(face*0.23)*2.6)*0.06;
                float rockN=saturate(broad*0.63+grain*0.37+layers);
                float3 rock=lerp(_RockLow.rgb,_RockHigh.rgb,rockN);
                float3 albedo=lerp(grass,rock,cliff);
                Light sun=GetMainLight(i.shadowCoord);
                float ndl=saturate(dot(n,sun.direction));
                float diffuse=(0.80+0.20*ndl*sun.shadowAttenuation);
                return half4(albedo*diffuse,1);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
