Shader "UI/Eldoria/HallPainterly"
{
    Properties
    {
        [PerRendererData] _MainTex ("Source architecture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Strength ("Brush reduction", Range(0,1)) = 0.94
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
                // A single photo filtered only by palette levels still read
                // like a glossy raster. This second, distinct paint method
                // removes photographic micro-detail using adjacent source
                // samples BEFORE flattening to a Valoria gold/ink palette.
                // It affects architectural pixels only: live character
                // models, labels, cards and raycasts stay untouched.
                float2 brush=float2(2.2/1536.0,2.2/1024.0);
                half3 l=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv-float2(brush.x,0)).rgb;
                half3 r=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+float2(brush.x,0)).rgb;
                half3 t=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+float2(0,brush.y)).rgb;
                half3 b=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv-float2(0,brush.y)).rgb;
                half3 softened=(src*2+l+r+t+b)/6;
                half3 quantized=floor(softened*5.0h+0.5h)/5.0h;
                half luminance=dot(quantized,half3(0.299h,0.587h,0.114h));
                half3 painted=lerp(quantized,half3(luminance*1.08h,luminance*.96h,luminance*.86h),.25h);
                half edges=dot(abs(l-r)+abs(t-b),half3(.18h,.36h,.10h));
                painted*=(1.0h-.18h*saturate(edges*2.5h));
                half grain=frac(sin(dot(floor(i.uv*half2(920,520)),half2(372.14h,138.26h)))*43758.545h)-.5h;
                painted+=grain*.014h;
                painted=lerp(painted,half3(painted.r*.88h+.046h,painted.g*.88h+.034h,painted.b*.88h+.025h),.36h);
                half3 merged=lerp(src,saturate(painted),_Strength);
                return half4(merged,source.a)*i.color;
            }
            ENDHLSL
        }
    }
    FallBack "UI/Default"
}
