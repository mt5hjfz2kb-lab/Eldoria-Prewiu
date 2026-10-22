Shader "Eldoria/World4X/WorldAlignedPaintedGround"
{
    Properties
    {
        [MainTexture] _BaseMap("Geographic pigment", 2D) = "white" {}
        _DetailMap("Painted meadow detail", 2D) = "gray" {}
        _WorldMap("Origin XZ and inverse extents", Vector) = (0,0,1,1)
        _TileScale("Detail world scale", Float) = 0.0208333
        _DetailStrength("Detail strength", Range(0,1)) = 0.55
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 worldXZ : TEXCOORD0; };
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_DetailMap); SAMPLER(sampler_DetailMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _WorldMap;
                float _TileScale;
                float _DetailStrength;
            CBUFFER_END
            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 world=TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(world);
                o.worldXZ=world.xz;
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                float2 relative=i.worldXZ-_WorldMap.xy;
                float2 uv=relative*_WorldMap.zw+0.5;
                half3 pigment=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,uv).rgb;
                half3 detail=SAMPLE_TEXTURE2D(_DetailMap,sampler_DetailMap,relative*_TileScale).rgb;
                #if defined(UNITY_COLORSPACE_GAMMA)
                    half3 average=half3(.752,.600,.242);
                #else
                    half3 average=half3(.532491,.329092,.047880);
                #endif
                half3 variation=clamp(detail/average,half3(.65,.65,.65),half3(1.35,1.35,1.35));
                half dry=1-smoothstep(.01,.10,pigment.b-max(pigment.r,pigment.g));
                return half4(pigment*lerp(half3(1,1,1),variation,_DetailStrength*dry),1);
            }
            ENDHLSL
        }
    }
}
