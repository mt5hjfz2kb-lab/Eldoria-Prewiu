Shader "Eldoria/Valoria Shell Terrain"
{
    Properties
    {
        _BackplateTex ("Backplate", 2D) = "white" {}
        _BackplateTint ("Backplate Tint", Color) = (0.84,0.88,0.91,1)
        _ContactStrength ("Contact Strength", Range(0,1)) = 0
        _UseMeshUv ("Use Mesh UV", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-20" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "PhotographicTerrain"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BackplateTex);
            SAMPLER(sampler_BackplateTex);
            float4 _BackplateTint;
            float _ContactStrength;
            float _UseMeshUv;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                VertexPositionInputs p=GetVertexPositionInputs(input.positionOS.xyz);
                o.positionHCS=p.positionCS;
                o.screenPos=ComputeScreenPos(p.positionCS);
                o.uv=input.uv;
                o.color=input.color;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUv=input.screenPos.xy/max(input.screenPos.w,1e-5);
                float2 sampleUv=lerp(screenUv,input.uv,saturate(_UseMeshUv));
                half4 c=SAMPLE_TEXTURE2D(_BackplateTex,sampler_BackplateTex,sampleUv);
                c.rgb*=_BackplateTint.rgb;
                float darken=saturate(input.color.r*_ContactStrength);
                c.rgb*=lerp(1.0,0.92,darken);
                c.a=saturate(input.color.a);
                return c;
            }
            ENDHLSL
        }
    }
}
