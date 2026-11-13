Shader "Eldoria/DailyHeraldicRelief"
{
 Properties
 {
    [PerRendererData] _MainTex ("Authentic in-game emblem", 2D) = "white" {}
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
   float4 _MainTex_TexelSize;
   fixed4 _Color;
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   v2f vert(appdata v)
   { v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
   fixed4 frag(v2f i):SV_Target
   {
      // Old Unity screenshot: official flat blue 128px compass floated on
      // painterly forged shield. Preserve its actual alpha + silhouette;
      // relief derives ONLY from its existing sampled RGB, never a new icon.
      fixed4 src=tex2D(_MainTex,i.uv);
      float2 uvStep=_MainTex_TexelSize.xy*1.7;
      fixed4 l=tex2D(_MainTex,i.uv-float2(uvStep.x,0));
      fixed4 r=tex2D(_MainTex,i.uv+float2(uvStep.x,0));
      fixed4 b=tex2D(_MainTex,i.uv-float2(0,uvStep.y));
      fixed4 u=tex2D(_MainTex,i.uv+float2(0,uvStep.y));
      float3 Y=float3(.299,.587,.114);
      float lum=dot(src.rgb,Y)*src.a;
      float dx=dot(r.rgb,Y)*r.a-dot(l.rgb,Y)*l.a;
      float dy=dot(u.rgb,Y)*u.a-dot(b.rgb,Y)*b.a;
      float relief=saturate(.46+lum*.55-dx*.75+dy*.85);
      float3 copperDark=float3(.125,.091,.065);
      float3 bronzeHigh=float3(.83,.655,.385);
      float3 metal=lerp(copperDark,bronzeHigh,relief);
      metal+=saturate((dy-dx)*2.3)*float3(.09,.070,.035);
      return fixed4(metal*i.color.rgb,src.a*i.color.a);
   }
   ENDCG
  }
 }
 Fallback Off
}
