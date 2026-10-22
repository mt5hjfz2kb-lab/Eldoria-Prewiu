Shader "Eldoria/World4X/WorldAlignedPaintedGround"
{
    Properties
    {
        [MainTexture] _BaseMap("Geographic pigment", 2D) = "white" {}
        _DetailMap("Painted meadow detail", 2D) = "gray" {}
        _EcologyMap("Actual-node soil / forest / meadow",2D)="black"{}
        _EcologySpan("Local field inverse extent",Float)=.002604167
        _EcologyStrength("Local scale ecology",Range(0,1))=1
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
                float _EcologyStrength;
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
                    half3 meadow=half3(.51,.56,.25),forest=half3(.31,.39,.17),soil=half3(.72,.59,.37);
                #else
                    half3 meadow=half3(.223,.274,.051),forest=half3(.078,.126,.025),soil=half3(.477,.307,.113);
                #endif
                half weight=ecology.a*dry*_EcologyStrength;
                // Authored land-cover hierarchy: exposed ochre earth between
                // coherent meadow islands, rooted shade, then actual-node paths.
                // Derivative antialiasing keeps edges readable at every scale.
                half meadowAA=max(.018h,fwidth(ecology.b)*.8h);
                half meadowCover=smoothstep(.48h-meadowAA,.48h+meadowAA,ecology.b);
                half rootAA=max(.025h,fwidth(ecology.g));
                half rootCover=smoothstep(.28h-rootAA,.28h+rootAA,ecology.g);
                half pathAA=max(.035h,fwidth(ecology.r));
                half pathCover=smoothstep(.32h-pathAA,.32h+pathAA,ecology.r);
                #if defined(UNITY_COLORSPACE_GAMMA)
                    half3 earth=half3(.69,.58,.33);
                    half3 grass=half3(.46,.53,.23);
                    half3 roots=half3(.32,.41,.18);
                    half3 path=half3(.76,.64,.39);
                #else
                    half3 earth=half3(.434,.296,.089);
                    half3 grass=half3(.179,.243,.043);
                    half3 roots=half3(.084,.140,.027);
                    half3 path=half3(.539,.367,.126);
                #endif
                half grassTone=lerp(.88h,1.12h,smoothstep(.48h,.93h,ecology.b));
                half3 local=lerp(earth,grass*grassTone,meadowCover);
                local=lerp(local,roots,rootCover*.66h);
                local=lerp(local,path,pathCover*.92h);
                // Retain a little authored pigment variation, without the
                // previous blurred atlas dominating the near land-cover shapes.
                local*=lerp(half3(1,1,1),variation,.18h);
                pigment=lerp(pigment,local,weight*.86h);
                half footprint=max(length(ddx(relative)),length(ddy(relative)));
                half detailFade=1-smoothstep(.16,.75,footprint);
                half detailAmount=lerp(_DetailStrength,.055h,weight);
                half3 color=pigment*lerp(half3(1,1,1),variation,detailAmount*dry*detailFade);
                Light sun=GetMainLight();
                half flat=.70h+.30h*saturate(sun.direction.y);
                half relief=(.70h+.30h*saturate(dot(normalize(i.normalWS),sun.direction)))/max(.7h,flat);
                return half4(color*lerp(1,clamp(relief,.78,1.15),dry),1);
            }
            ENDHLSL
        }
    }
}
