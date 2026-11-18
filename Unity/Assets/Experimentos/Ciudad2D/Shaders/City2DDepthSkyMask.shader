Shader "Eldoria/City2D/DepthSkyMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Original Terrain Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _MaskStart ("Far Sky Start", Range(0,1)) = 0.80
        _MaskEnd ("Far Sky End", Range(0,1)) = 0.90
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }
        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _MaskStart;
                float _MaskEnd;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * i.color;
                // Only the uppermost, distant landscape is permitted to drift.
                // Retained terrain, roads, walls and every building stay fixed.
                c.a *= smoothstep(_MaskStart, _MaskEnd, i.uv.y);
                return c;
            }
            ENDHLSL
        }
    }
}
