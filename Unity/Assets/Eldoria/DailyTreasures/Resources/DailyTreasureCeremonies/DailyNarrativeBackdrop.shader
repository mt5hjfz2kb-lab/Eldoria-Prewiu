Shader "Eldoria/DailyNarrativeBackdrop"
{
    Properties
    {
        [PerRendererData] _MainTex ("Original Eldoria scenery", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)
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
                float u=saturate(i.uv.x);
                float v=saturate(i.uv.y);
                float rightInk=smoothstep(.38,.96,u);
                // Bounded: the right reading column is basalt-dark but still
                // contains genuine scenery detail behind the text. The scene
                // remains visible at the left of its original focal vignette.
                float edge=length((float2(u,v)-.5)*float2(1.,1.12));
                float vignette=1.-.31*smoothstep(.30,.76,edge);
                src.rgb *= lerp(.57,.21,rightInk)*vignette;
                src.a *= lerp(.80,.86,rightInk)*i.color.a;
                return src;
            }
            ENDCG
        }
    }
    Fallback Off
}
