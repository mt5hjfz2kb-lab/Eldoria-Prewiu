Shader "Eldoria/World4X/WorldAlignedPaintedGround"
{
    Properties
    {
        [MainTexture] _BaseMap("Geographic pigment", 2D) = "white" {}
        _DetailMap("Painted meadow detail", 2D) = "gray" {}
        _EcologyMap("Actual-node soil / forest / meadow",2D)="black"{}
        _EcologySpan("Local field inverse extent",Float)=.002604167
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes { float4 positionOS : POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 worldXZ : TEXCOORD0; float3 normalWS:TEXCOORD1; };
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_DetailMap); SAMPLER(sampler_DetailMap);
            TEXTURE2D(_EcologyMap); SAMPLER(sampler_EcologyMap);
            CBUFFER_START(UnityPerMaterial)
                float _EcologySpan;
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
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
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
                half4 ecology=SAMPLE_TEXTURE2D(_EcologyMap,sampler_EcologyMap,relative*_EcologySpan+.5);
                #if defined(UNITY_COLORSPACE_GAMMA)
                    half3 meadow=half3(.51,.56,.25),forest=half3(.31,.39,.17),soil=half3(.65,.53,.32);
                #else
                    half3 meadow=half3(.223,.274,.051),forest=half3(.078,.126,.025),soil=half3(.380,.243,.084);
                #endif
                half weight=ecology.a*dry;
                pigment=lerp(pigment,meadow,ecology.b*weight*.62);
                pigment=lerp(pigment,forest,ecology.g*weight*.68);
                pigment=lerp(pigment,soil,ecology.r*weight*.88);
                half footprint=max(length(ddx(relative)),length(ddy(relative)));
                half detailFade=1-smoothstep(.8,2.8,footprint);
                half3 color=pigment*lerp(half3(1,1,1),variation,_DetailStrength*dry*detailFade);
                Light sun=GetMainLight();
                half flat=.70h+.30h*saturate(sun.direction.y);
                half relief=(.70h+.30h*saturate(dot(normalize(i.normalWS),sun.direction)))/max(.7h,flat);
                return half4(color*lerp(1,clamp(relief,.78,1.15),dry),1);
            }
            ENDHLSL
        }
    }
}
