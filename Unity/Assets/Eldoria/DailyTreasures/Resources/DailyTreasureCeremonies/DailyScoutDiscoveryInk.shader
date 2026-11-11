Shader "Eldoria/DailyScoutDiscoveryInk"
{
    Properties
    {
        [PerRendererData] _MainTex ("Original atlas", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _AtlasUv ("Exact original family tile XYWH", Vector) = (0.01,0.51,0.48,0.48)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True"
               "RenderType"="Transparent" "PreviewType"="Plane"
               "CanUseSpriteAtlas"="True" }
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
            float4 _AtlasUv;
            fixed4 _Color;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };
            v2f vert(appdata v)
            {
                v2f o;
                o.pos=UnityObjectToClipPos(v.vertex);
                o.uv=v.uv;
                o.color=v.color*_Color;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                // The source is an EXISTING approved four-family atlas.
                // Local tile coordinates feather its edges INTO the genuine
                // aged parchment, never replace geography with fiction.
                float2 local=saturate((i.uv-_AtlasUv.xy)/
                    max(_AtlasUv.zw,float2(.0001,.0001)));
                float2 distanceToEdge=min(local,1.0-local);
                float edge=min(distanceToEdge.x,distanceToEdge.y);
                float matte=smoothstep(.025,.195,edge);
                fixed4 original=tex2D(_MainTex,i.uv);
                float brightness=dot(original.rgb,float3(.299,.587,.114));
                float3 ink=lerp(original.rgb,
                    float3(brightness*.96,brightness*.82,
                           brightness*.68),.31);
                original.rgb=ink*i.color.rgb;
                original.a*=matte*i.color.a;
                return original;
            }
            ENDCG
        }
    }
    Fallback Off
}
