Shader "Eldoria/Senda/PortalEnergy" {
 Properties { _Tint("Gate color",Color)=(.2,.7,.6,1) _Phase("Animated phase",Float)=0 }
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 fixed4 _Tint;float _Phase;
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 fixed4 frag(v2f i):SV_Target{float2 p=float2((i.uv.x-.5)*1.38,i.uv.y*2.56);float vein=pow(max(sin(p.x*15+sin(p.y*6-_Phase*1.7)*2.4+_Phase*1.3),0),7);
 float depth=.5+.5*sin(p.x*5+p.y*4-_Phase*1.2);float edge=smoothstep(.39,.68,abs(p.x)),base=exp(-p.y*2.2);
 return fixed4(_Tint.rgb*(.85+.8*vein+edge+base*.5),saturate(.15+.22*depth+.25*vein+.30*edge+.20*base));}
 ENDCG
 }
 } Fallback Off
}
