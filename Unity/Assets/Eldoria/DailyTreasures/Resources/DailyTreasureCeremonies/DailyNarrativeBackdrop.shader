Shader "Eldoria/DailyNarrativeBackdrop"
{
    Properties
    {
        [PerRendererData] _MainTex ("Original Eldoria scenery", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _VisibleUv ("Visible original source UV", Vector) = (0,0,1,1)
        _PortraitReading ("Stacked report below scene", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True"
               "RenderType"="Transparent" "PreviewType"="Plane" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            fixed4 _Color;
            float4 _VisibleUv;
            float _PortraitReading;
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct v2f {float4 position:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            v2f vert(appdata v)
            {
                v2f o;
                o.position=UnityObjectToClipPos(v.vertex);
                o.uv=v.uv;
                o.color=v.color*_Color;
                return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                // The RGB stays genuine Eldoria. The artistically significant
                // change is a full-bleed visual theatre, instead of 50% blank
                // black space. A subtle sepia shadow ties it to worn bronze.
                fixed4 src=tex2D(_MainTex,i.uv);
                float lum=dot(src.rgb,float3(.299,.587,.114));
                float3 antique=float3(lum*1.08,lum*.91,lum*.71);
                src.rgb=lerp(src.rgb,antique,.27);
                // Shader reads screen position, not source pixels: the
                // panorama remains undistorted and text ink follows layout.
                float2 local=saturate((i.uv-_VisibleUv.xy)/
                    max(_VisibleUv.zw,float2(.0001,.0001)));
                float rightInk=smoothstep(.38,.96,local.x);
                float bottomInk=smoothstep(.38,.94,1.-local.y);
                float readingInk=lerp(rightInk,bottomInk,
                    saturate(_PortraitReading));
                float edge=length((local-.5)*float2(1.,1.12));
                float vignette=1.-.31*smoothstep(.30,.76,edge);
                // Keep the genuine painting legible in the reading field,
                // rather than turning the whole lower mobile theatre black.
                // The separate .67 basalt patch owns text contrast locally.
                // At full reading ink this carries 81% more scenery light
                // than the former .21 global multiplier (without new art).
                src.rgb *= lerp(.66,.38,readingInk)*vignette;
                src.a *= lerp(.80,.86,readingInk)*i.color.a;
                return src;
            }
            ENDCG
        }
    }
    Fallback Off
}
