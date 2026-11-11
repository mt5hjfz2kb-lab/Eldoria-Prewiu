// One-surface selective palette experiment: no additional UI graphic.
// Keep authored texture, UV, nine-slice, alpha, stencil and pointer geometry.
// Gamma-mode source art (Unity m_ActiveColorSpace: 0): dark wine-purple
// receives local bronze, original golden lion/specular highlights remain.
Shader "Eldoria/BreachWatch/AntiqueCrest"
{
    Properties
    {
        [PerRendererData] _MainTex ("Source 2K profile sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True"
               "RenderType"="Transparent" "PreviewType"="Plane"
               "CanUseSpriteAtlas"="True" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            sampler2D _MainTex;
            fixed4 _Color;
            float4 _TextureSampleAdd;
            float4 _ClipRect;
            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition=v.vertex;
                o.vertex=UnityObjectToClipPos(o.worldPosition);
                o.color=v.color*_Color;
                o.texcoord=v.texcoord;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col=(tex2D(_MainTex,i.texcoord)+_TextureSampleAdd)*i.color;
                float3 baseColor=col.rgb;
                // Only purple/magenta background: positive red>green AND
                // blue>green, moderately dark; not gold (blue<green),
                // neutral basalt or bright ivory. Existing 2K shading stays.
                float wine=smoothstep(.08,.19,baseColor.r-baseColor.g)
                         *smoothstep(.01,.04,baseColor.b-baseColor.g)
                         *(1.0-smoothstep(.56,.82,baseColor.r));
                col.rgb=lerp(baseColor,baseColor*float3(.63,.55,.20),wine);
                #ifdef UNITY_UI_CLIP_RECT
                col.a*=UnityGet2DClipping(i.worldPosition.xy,_ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a-.001);
                #endif
                return col;
            }
            ENDCG
        }
    }
}
