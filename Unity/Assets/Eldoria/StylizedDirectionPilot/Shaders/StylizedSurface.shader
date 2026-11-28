Shader "Eldoria/StylizedDirectionPilot/Surface"
{
    Properties
    {
        _BaseMap ("Existing surface texture", 2D) = "white" {}
        _BaseColor ("Existing material color", Color) = (1,1,1,1)
        _StyleStrength ("Style strength", Range(0,1)) = 0.8
        _Cutoff ("Alpha clip", Range(0,1)) = 0.35
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 150
        Pass
        {
            Name "StylizedWorldForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _StyleStrength;
                float _Cutoff;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;
                output.shadowCoord = TransformWorldToShadowCoord(pos.positionWS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float4 sampleColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                // Deliberately disable alpha discard for OPAQUE pilot renderers.\n                // A/B on the previous SHA proved that all meshes vanished in color pass.\n                // Cutout handling must use material classification in a future real implementation.
                float3 source = saturate(sampleColor.rgb * _BaseColor.rgb);
                float luminance = dot(source, float3(.2126,.7152,.0722));
                float maxRG = max(source.r, source.g);
                // Ultramarine props/readymade roofs become the same quiet slate family.
                float blue = smoothstep(.05,.28,source.b - maxRG);
                float3 slate = lerp(float3(.17,.24,.28),float3(.42,.49,.52),luminance);
                float3 albedo = lerp(source,slate,blue * .88 * _StyleStrength);
                // Reduce distracting chroma without erasing original stone/wood texture.
                float neutral = dot(albedo,float3(.2126,.7152,.0722));
                albedo = lerp(albedo,neutral.xxx,.16 * _StyleStrength);
                Light sun = GetMainLight(input.shadowCoord);
                float nDotL = saturate(dot(normalize(input.normalWS), normalize(sun.direction)) * .5 + .5);
                float lit = saturate(.28 + .72*nDotL*sun.shadowAttenuation);
                float stepped = floor(lit*5.0+.5)/5.0;
                lit = lerp(lit,stepped,.58*_StyleStrength);
                float3 skyFill = float3(.69,.74,.72);
                float3 color = albedo * (skyFill * .46 + sun.color * lit * .65);
                return half4(saturate(color),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
    FallBack Off
}
