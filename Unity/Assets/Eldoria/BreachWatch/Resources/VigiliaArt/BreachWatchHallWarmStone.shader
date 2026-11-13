Shader "UI/Eldoria/BreachWatchHallWarmStone"
{
    Properties
    {
        [PerRendererData] _MainTex ("Approved illustrated hall", 2D) = "white" {}
        _Color ("Vertex color", Color) = (1,1,1,1)
        _Warmth ("Charcoal/brass grade", Range(0,1)) = 0.72
        _ShadowFloor ("Shadow floor", Range(0,0.1)) = 0.018
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent"
               "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Cull Off Lighting Off ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _ClipRect;
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Warmth;
                half _ShadowFloor;
            CBUFFER_END

            struct Input
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                half4 color : COLOR;
            };
            struct Interpolated
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                float4 uiLocal : TEXCOORD1;
            };

            Interpolated vert(Input input)
            {
                Interpolated v;
                v.position=TransformObjectToHClip(input.vertex.xyz);
                v.uiLocal=input.vertex;
                v.uv=input.texcoord;
                v.color=input.color*_Color;
                return v;
            }

            half4 frag(Interpolated input) : SV_Target
            {
                // One texture sample per pixel. Avoid the 9-tap portrait
                // shader cost for a full-screen animated mobile hall.
                half4 sampled=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv);
                half lum=dot(sampled.rgb,half3(.28h,.61h,.11h));
                // Keep architecture, texture, shadows and actual dynamic
                // scenery. Remove the old electric-blue cast without putting
                // an opaque extra panel on top of the scene.
                half3 agedStone=half3(lum*1.10h+.012h,
                                      lum*.88h+.009h,
                                      lum*.69h+.005h);
                // Preserve some original warm torch color and stone depth.
                half3 warm=lerp(sampled.rgb,agedStone,_Warmth);
                warm=max(warm,half3(_ShadowFloor,_ShadowFloor*.88h,
                                    _ShadowFloor*.73h));
                half4 result=half4(saturate(warm),sampled.a)*input.color;
                #ifdef UNITY_UI_CLIP_RECT
                    float2 m=step(_ClipRect.xy,input.uiLocal.xy)*
                             step(input.uiLocal.xy,_ClipRect.zw);
                    result.a*=m.x*m.y;
                #endif
                return result;
            }
            ENDHLSL
        }
    }
    FallBack "UI/Default"
}
