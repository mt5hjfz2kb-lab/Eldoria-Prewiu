Shader "UI/Eldoria/HallPainterly"
{
    Properties
    {
        [PerRendererData] _MainTex ("Source architecture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Strength ("Brush reduction", Range(0,1)) = 0.82
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Cull Off Lighting Off ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Strength;
            CBUFFER_END
            struct Attributes { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            Varyings vert (Attributes v)
            {
                Varyings o;
                o.pos=TransformObjectToHClip(v.vertex.xyz);
                o.uv=v.uv; o.color=v.color*_Color;
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                half4 source=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                half3 src=source.rgb;
                // Palette simplification, atmospheric dark ink and warm paint.
                // Keep the source's actual architecture: never paint interface
                // lettering or fake hero cards into the backdrop.
                half3 quantized=floor(src*7.0h+0.5h)/7.0h;
                half luminance=dot(quantized,half3(0.299h,0.587h,0.114h));
                half3 painted=lerp(quantized,half3(luminance*1.06h,luminance*0.97h,luminance*.87h),.22h);
                half grain=frac(sin(dot(i.uv,half2(372.14h,138.26h)))*43758.545h)-.5h;
                painted += grain*.015h;
                painted=lerp(painted,half3(painted.r*.82h+.052h,painted.g*.82h+.038h,painted.b*.84h+.025h),.27h);
                half3 merged=lerp(src,saturate(painted),_Strength);
                return half4(merged,source.a)*i.color;
            }
            ENDHLSL
        }
    }
    FallBack "UI/Default"
}
