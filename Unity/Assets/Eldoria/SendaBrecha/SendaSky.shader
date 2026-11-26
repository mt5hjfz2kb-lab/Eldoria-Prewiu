Shader "Eldoria/Senda/Sky" {
 SubShader {Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;};struct v2f {float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.dir=v.vertex.xyz;return o;}
 float n2(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);float a=frac(sin(dot(i,float2(127.1,311.7)))*43758.5453);float b=frac(sin(dot(i+float2(1,0),float2(127.1,311.7)))*43758.5453);float c=frac(sin(dot(i+float2(0,1),float2(127.1,311.7)))*43758.5453);float d=frac(sin(dot(i+float2(1,1),float2(127.1,311.7)))*43758.5453);return lerp(lerp(a,b,f.x),lerp(c,d,f.x),f.y);}
 fixed4 frag(v2f i):SV_Target{float3 d=normalize(i.dir);float h=saturate(d.y*2+.15);float3 c=lerp(float3(.31,.35,.43),float3(.055,.08,.15),h);float2 p=d.xz*12/(abs(d.y)+.35);float n=n2(p)*.55+n2(p*2.1)*.30+n2(p*5)*.15;float cloud=smoothstep(.48,.78,n)*smoothstep(-.02,.25,d.y);c=lerp(c,float3(.13,.14,.24),cloud*.9);float glow=pow(max(dot(d,normalize(float3(.25,.18,1))),0),18);c+=float3(.22,.065,.30)*glow;return fixed4(c,1);}
 ENDCG
 }
 } Fallback Off
}
