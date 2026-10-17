Shader "Eldoria/ReferenceClean/River" {
 Properties { _BaseColor("Deep blue",Color)=(.018,.21,.34,1) _ShallowColor("Shore blue",Color)=(.045,.36,.47,1) }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  Pass {
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor; half4 _ShallowColor;
   CBUFFER_END
   struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
   struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float2 uv:TEXCOORD1;};
   V vert(A a){V o;o.world=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.uv=a.uv;return o;}
   half4 frag(V i):SV_Target {
    float t=_Time.y*.6;float w=sin(i.world.x*1.5+i.world.z*2.2+t);float w2=sin(i.world.z*3.4-i.world.x*.7-t*.8);
    float edge=abs(i.uv.y*2-1);half3 c=lerp(_BaseColor.rgb,_ShallowColor.rgb,saturate(edge*edge));
    float3 normal=normalize(float3(w*.10,1,w2*.08));float3 view=GetWorldSpaceNormalizeViewDir(i.world);Light light=GetMainLight();
    float spec=pow(saturate(dot(normal,normalize(light.direction+view))),80);c+=light.color*spec*.22;
    float ripple=pow(saturate(sin(i.world.x*.6+i.world.z*8+t*2)),20)*.065;c+=ripple;
    float foam=smoothstep(.91,.995,edge+w*w2*.018);c=lerp(c,half3(.62,.77,.73),foam*.65);
    return half4(c,1);
   }
   ENDHLSL
  }
 }
}
