Shader "Eldoria/Valoria25D/ArchitecturalShadow"
{
    Properties { _ShadowTex ("Building alpha silhouette", 2D) = "white" {} }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Pass
        {
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _ShadowTex;
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct Output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            Output vert(Input v)
            {
                Output o;
                o.vertex=UnityObjectToClipPos(v.vertex);
                o.uv=v.uv;
                o.color=v.color;
                return o;
            }
            fixed4 frag(Output i):SV_Target
            {
                fixed alpha=tex2D(_ShadowTex,i.uv).a;
                return fixed4(i.color.rgb,alpha*i.color.a);
            }
            ENDCG
        }
    }
}
