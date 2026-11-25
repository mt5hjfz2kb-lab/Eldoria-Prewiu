Shader "Eldoria/Valoria/Urban road" {
Properties {
 _MainTex("Road paint",2D)="white"{} _Paving("Cobbles",2D)="white"{}
 _Stone("Stone fraction",Range(0,1))=0
}
SubShader { Tags {"Queue"="Transparent" "RenderType"="Transparent"} Pass {
 Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex,_Paving;
 half _Stone;
 struct V {float4 pos:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
 struct F {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float2 ground:TEXCOORD1;fixed alpha:COLOR;};
 F vert(V v) { F o;o.pos=UnityObjectToClipPos(v.pos);o.uv=v.uv;o.ground=mul(unity_ObjectToWorld,v.pos).xy;o.alpha=v.color.a;return o; }
 fixed4 frag(F i):SV_Target {
   fixed4 earth=tex2D(_MainTex,i.uv);
   float2 p=i.ground;
   fixed3 stone=tex2D(_Paving,float2(.85*p.x+.32*p.y,1.08*p.y-.25*p.x)).rgb;
   half variation=dot(stone,half3(.24,.56,.20));
   half middle=1-smoothstep(.22,.96,2*abs(i.uv.x-.5));
   // Paving settles into a compact wheel bed. On the outer shoulder,
   // individual exposed stones interrupt the edge of the road paint.
   half edge=abs(i.uv.x-.5)*2;
   half chips=saturate((variation-.24)*2.8);
   half shoulder=smoothstep(.50,.96,edge);
   half blend=_Stone*(.36+.55*middle+.13*chips*shoulder);
   half roadCoverage=i.alpha*(1-shoulder*(.35+.40*(1-chips)));
   return fixed4(lerp(earth.rgb*.88,stone*half3(.93,.88,.79),blend),
       roadCoverage*saturate(earth.a+.12)*(.76+.22*variation));
 }
 ENDCG
} } }
