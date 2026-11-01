Shader "UI/Eldoria/HallPainterly"
{
    Properties
    {
        [PerRendererData] _MainTex ("Source architecture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Strength ("Brush reduction", Range(0,1)) = 0.94
        _PortraitFeather ("Original portrait edge dissolve", Range(0,0.3)) = 0
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
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _ClipRect;
            float4 _MainTex_TexelSize;
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Strength;
                half _PortraitFeather;
            CBUFFER_END
            struct Attributes { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; float4 uiPosition : TEXCOORD1; };
            Varyings vert (Attributes v)
            {
                Varyings o;
                o.pos=TransformObjectToHClip(v.vertex.xyz);
                o.uv=v.uv; o.color=v.color*_Color; o.uiPosition=v.vertex;
                return o;
            }
            half4 frag(Varyings i) : SV_Target
            {
                half4 source=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                // Nine-tap edge-preserving brush in SOURCE TEXTURE pixels.
                // Previously a hardcoded 1536x1024 architectural step was
                // reused for Lyra's genuine model RenderTexture (512x768).
                // That over-smoothed face/hair at mobile display sizes.
                // The native texel size now scales with the actual artwork.
                float2 brush=_MainTex_TexelSize.xy*1.15;
                half3 a=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+brush*float2(-1,-1)).rgb;
                half3 b0=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+brush*float2( 0,-1)).rgb;
                half3 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+brush*float2( 1,-1)).rgb;
                half3 d=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+brush*float2(-1, 0)).rgb;
                half3 e=source.rgb;
                half3 f=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+brush*float2( 1, 0)).rgb;
                half3 g=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+brush*float2(-1, 1)).rgb;
                half3 h=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+brush*float2( 0, 1)).rgb;
                half3 q=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv+brush*float2( 1, 1)).rgb;
                half3 m0=(a+b0+d+e)*.25h;
                half3 m1=(b0+c+e+f)*.25h;
                half3 m2=(d+e+g+h)*.25h;
                half3 m3=(e+f+h+q)*.25h;
                half v0=dot((a-m0)*(a-m0)+(b0-m0)*(b0-m0)+
                    (d-m0)*(d-m0)+(e-m0)*(e-m0),half3(.299h,.587h,.114h));
                half v1=dot((b0-m1)*(b0-m1)+(c-m1)*(c-m1)+
                    (e-m1)*(e-m1)+(f-m1)*(f-m1),half3(.299h,.587h,.114h));
                half v2=dot((d-m2)*(d-m2)+(e-m2)*(e-m2)+
                    (g-m2)*(g-m2)+(h-m2)*(h-m2),half3(.299h,.587h,.114h));
                half v3=dot((e-m3)*(e-m3)+(f-m3)*(f-m3)+
                    (h-m3)*(h-m3)+(q-m3)*(q-m3),half3(.299h,.587h,.114h));
                half3 calm=m0;
                half variance=v0;
                if(v1<variance){variance=v1;calm=m1;}
                if(v2<variance){variance=v2;calm=m2;}
                if(v3<variance){calm=m3;}
                // A 24-step palette retains illustrated fabrics and human faces;
                // the former 14-step palette visibly posterized Lyra.
                half3 quantized=floor(saturate(calm)*24.0h+0.5h)/24.0h;
                half luminance=dot(quantized,half3(.299h,.587h,.114h));
                half3 ink=lerp(quantized,
                    half3(luminance*1.11h,luminance*.96h,luminance*.81h),.16h);
                // Warm parchment highlights and inked shadows are anchored
                // to source luminance rather than arbitrary animated noise.
                half shadow=saturate(1.0h-luminance);
                ink=ink*(1.0h-.12h*shadow)+half3(.014h,.009h,.004h);
                half3 merged=lerp(source.rgb,saturate(ink),_Strength);
                half4 result=half4(merged,source.a)*i.color;
                #ifdef UNITY_UI_CLIP_RECT
                    float2 inside=step(_ClipRect.xy,i.uiPosition.xy)*
                        step(i.uiPosition.xy,_ClipRect.zw);
                    result.a*=inside.x*inside.y;
                #endif
                // The runtime-approved Aldric image includes a solid pale
                // rectangular sky. Dissolve its actual pixel alpha into the
                // separately drawn 2.5D architectural alcove without changing
                // any character geometry, labels, materials or save state.
                // The standard Hall background keeps _PortraitFeather=0.
                if(_PortraitFeather>.001h){
                    half2 uvEdge=min(i.uv.xy,1.0h-i.uv.xy);
                    half rim=min(uvEdge.x,uvEdge.y);
                    half blend=smoothstep(0.0h,_PortraitFeather,rim);
                    result.a*=blend;
                }
                return result;
            }
            ENDHLSL
        }
    }
    FallBack "UI/Default"
}
