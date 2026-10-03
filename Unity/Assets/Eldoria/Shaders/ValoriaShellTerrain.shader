Shader "Eldoria/Valoria Shell Terrain"
{
    Properties
    {
        _BackplateTex ("Backplate", 2D) = "white" {}
        _ContactStrength ("Contact Strength", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-10" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "ProjectedBackplate"
            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BackplateTex);
            SAMPLER(sampler_BackplateTex);
            float _ContactStrength;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float contact : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                VertexPositionInputs p=GetVertexPositionInputs(input.positionOS.xyz);
                o.positionHCS=p.positionCS;
                o.screenPos=ComputeScreenPos(p.positionCS);
                o.contact=input.color.r;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv=input.screenPos.xy/max(input.screenPos.w,1e-5);
                half4 c=SAMPLE_TEXTURE2D(_BackplateTex,sampler_BackplateTex,uv);
                float darken=saturate(input.contact*_ContactStrength);
                c.rgb*=lerp(1.0,0.82,darken);
                // Slightly warm only the contact zone so the shell reads as grounded earth,
                // while the outer shell remains pixel-matched to the photographed valley.
                c.rgb*=lerp(float3(1,1,1),float3(1.025,1.0,.965),darken*.55);
                c.a=1;
                return c;
            }
            ENDHLSL
        }
    }
}
