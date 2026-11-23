Shader "Eldoria/Corona/TrainingPigment"
{
    Properties
    {
        [PerRendererData] _MainTex ("Illustration", 2D) = "white" {}
        _StencilComp ("Stencil comparison", Float) = 8
        _Stencil ("Stencil reference", Float) = 0
        _StencilOp ("Stencil operation", Float) = 0
        _StencilWriteMask ("Stencil write mask", Float) = 255
        _StencilReadMask ("Stencil read mask", Float) = 255
        _ColorMask ("Colour channels", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct Vertex { float4 position:POSITION; float2 uv:TEXCOORD0; half4 tint:COLOR; };
            struct Varying { float4 clip:SV_POSITION; float2 uv:TEXCOORD0; float2 local:TEXCOORD1; half4 tint:COLOR; };
            sampler2D _MainTex;
            float4 _MainTex_TexelSize, _ClipRect;
            Varying vert(Vertex v)
            {
                Varying o;
                o.clip=UnityObjectToClipPos(v.position);
                o.local=v.position.xy;
                o.uv=v.uv;
                o.tint=v.tint;
                return o;
            }
            half4 frag(Varying i):SV_Target
            {
                half4 source=tex2D(_MainTex,i.uv);
                float2 stroke=_MainTex_TexelSize.xy*float2(2.4,1.3);
                half4 a=tex2D(_MainTex,i.uv+stroke), b=tex2D(_MainTex,i.uv-stroke);
                // Average fine photographic grain inside a brush stroke, retaining
                // the archer's outline and the larger value changes in stonework.
                half aw=a.a*saturate(1-length(a.rgb-source.rgb)*3);
                half bw=b.a*saturate(1-length(b.rgb-source.rgb)*3);
                half3 paint=(source.rgb+aw*a.rgb+bw*b.rgb)/(1+aw+bw);
                #ifndef UNITY_COLORSPACE_GAMMA
                paint=LinearToGammaSpace(paint);
                #endif
                half value=dot(paint,half3(.28,.55,.17));
                half3 umber=half3(.16,.105,.13);
                half3 vellum=half3(.93,.80,.62);
                paint=lerp(paint,lerp(umber,vellum,value),.29);
                #ifndef UNITY_COLORSPACE_GAMMA
                paint=GammaToLinearSpace(paint);
                #endif
                half4 colour=half4(paint,source.a)*i.tint;
                #ifdef UNITY_UI_CLIP_RECT
                colour.a*=UnityGet2DClipping(i.local,_ClipRect);
                #endif
                return colour;
            }
            ENDCG
        }
    }
}
