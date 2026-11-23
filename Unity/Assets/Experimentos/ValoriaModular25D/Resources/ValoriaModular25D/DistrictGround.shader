Shader "Eldoria/Valoria/District ground" {
Properties {
 _MainTex("Meadow",2D)="white"{} _Paving("Cobble",2D)="white"{}
 _Soil("Earth",2D)="white"{} _Built("Completed districts",Vector)=(0,0,0,0)
}
SubShader { Tags {"RenderType"="Opaque" "Queue"="Geometry"} Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _MainTex,_Paving,_Soil;
float4 _Built, _MilitaryYard, _MarketYard, _StoneFace;
struct Vertex { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
struct Varying { float4 position:SV_POSITION; float2 uv:TEXCOORD0; };
Varying vert(Vertex v) { Varying o; o.position=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
half yard(float2 p, float4 plot, half erosion) {
 float2 q=(p-plot.xy)/plot.zw;
 // Oblique working terraces inherit the perspective of the painted buildings.
 q.x+=q.y*.32;
 float d=pow(pow(abs(q.x),3)+pow(abs(q.y),3),1.0/3.0);
 return 1-smoothstep(.72,1.08,d+erosion*.15);
}
half4 frag(Varying i):SV_Target {
 float2 p=i.uv*6.5;
 half3 ground=tex2D(_MainTex,i.uv).rgb;
 half3 seam=tex2D(_Paving,float2(.47*p.x+.17*p.y,.59*p.y-.08*p.x)).rgb;
 half3 dirt=tex2D(_Soil,float2(.24+.44*frac(.29*p.x+.09*p.y),frac(.31*p.y))).rgb;
 half erosion=dot(ground,half3(.24,.56,.20))+seam.r-.6;
 half drill=yard(p,_MilitaryYard,erosion)*_Built.x;
 half market=yard(p,_MarketYard,erosion)*_Built.y;
 half rock=yard(p,_StoneFace,erosion)*_Built.z;
 half3 pad=lerp(dirt*.84,seam*half3(.87,.84,.77),.44);
 ground=lerp(ground,pad,drill*.91);
 ground=lerp(ground,lerp(dirt*.89,pad,.42),market*.92);
 half mineral=dot(seam,half3(.3,.55,.15));
 ground=lerp(ground,lerp(dirt*.77,half3(mineral*.93,mineral*.92,mineral*.87),.72),rock*.97);
 return half4(ground,1);
}
ENDCG
} } }
