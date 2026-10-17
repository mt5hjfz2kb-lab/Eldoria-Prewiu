Shader "Eldoria/ReferenceClean/Comparison/RiverBefore" {
 Properties { [MainColor] _BaseColor("Deep blue",Color)=(.012,.17,.31,1) _ShallowColor("Shore blue",Color)=(.055,.48,.65,1) }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  Pass {
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor; half4 _ShallowColor;
   CBUFFER_END
   struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
   struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float2 uv:TEXCOORD1;};
   V vert(A a){V o;o.world=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.uv=a.uv;return o;}
   float hash21(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise21(float2 p){float2 q=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash21(q),hash21(q+float2(1,0)),f.x),lerp(hash21(q+float2(0,1)),hash21(q+1),f.x),f.y);}
   half4 frag(V i):SV_Target {
    float t=_Time.y*.6;float w=sin(i.world.x*1.5+i.world.z*2.2+t);float w2=sin(i.world.z*3.4-i.world.x*.7-t*.8);
    float edge=abs(i.uv.y*2-1);float2 flow=i.world.xz*float2(.22,.54)+float2(-t*.045,t*.012);float n=noise21(flow)+noise21(flow*2.17)*.40+noise21(flow*5.23)*.16;half3 c=lerp(_BaseColor.rgb,_ShallowColor.rgb,saturate(edge*edge*edge*.74+n*.14));c*=.90+n*.16;
    float3 normal=normalize(float3(w*.10,1,w2*.08));float3 view=GetWorldSpaceNormalizeViewDir(i.world);Light light=GetMainLight(TransformWorldToShadowCoord(i.world));
    float spec=pow(saturate(dot(normal,normalize(light.direction+view))),80);c+=light.color*spec*.22;
    float streak=noise21(float2(i.world.x*.48-t*.045,i.world.z*4.2+noise21(flow)*2));float ripple=smoothstep(.64,.83,streak)*.032;c+=ripple*half3(.48,.78,1);c*=lerp(.78,1,light.shadowAttenuation);
    float foam=smoothstep(.945,.995,edge+(noise21(flow*3)-.5)*.05)*smoothstep(.32,.72,noise21(flow*1.5));c=lerp(c,half3(.50,.70,.66),foam*.70);
    return half4(c,1);
   }
   ENDHLSL
  }
 }
}
