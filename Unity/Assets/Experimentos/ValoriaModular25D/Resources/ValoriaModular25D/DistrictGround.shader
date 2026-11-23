Shader "Eldoria/Valoria/District ground" {
Properties {
 _MainTex("Meadow",2D)="white"{} _Paving("Cobble",2D)="white"{}
 _Soil("Earth",2D)="white"{} _Plan("District coverage",2D)="black"{}
 _Built("Completed districts",Vector)=(0,0,0,0)
}
SubShader { Tags {"RenderType"="Opaque" "Queue"="Geometry"} Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _MainTex,_Paving,_Soil,_Plan;
float4 _Built;
struct Vertex { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
struct Varying { float4 position:SV_POSITION; float2 uv:TEXCOORD0; };
Varying vert(Vertex v) { Varying o; o.position=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
half4 frag(Varying i):SV_Target {
 float2 p=i.uv*6.5;
 half3 ground=tex2D(_MainTex,i.uv).rgb;
 half3 plan=tex2D(_Plan,(p+float2(8,15))/float2(16,18)).rgb*_Built.xyz;
 half worn=max(max(plan.r,plan.g),plan.b);
 float2 grain=float2(.36*p.x+.13*p.y,.30*p.y-.08*p.x);
 half3 dirt=tex2D(_Soil,float2(.28+.44*frac(grain.x),frac(grain.y))).rgb;
 half3 stone=tex2D(_Paving,float2(.30+.40*frac(grain.x),frac(grain.y))).rgb;
 half3 bed=lerp(dirt*.84,stone*half3(.94,.88,.79),saturate(.17+.62*plan.r+.40*plan.g+.18*plan.b));
 return half4(lerp(ground,bed,worn),1);
}
ENDCG
} } }
