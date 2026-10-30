Shader "Eldoria/World4X/GeographicFacets"
{
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass
  {
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
   struct A{float3 p:POSITION;float3 n:NORMAL;half4 c:COLOR;};
   struct V{float4 p:SV_POSITION;half3 c:COLOR;};
   V vert(A i){V o;o.p=TransformObjectToHClip(i.p);half light=.76h+.24h*abs(dot(TransformObjectToWorldNormal(i.n),normalize(float3(-.6,1,-.4))));o.c=i.c.rgb;
    #if !defined(UNITY_COLORSPACE_GAMMA)
     o.c=SRGBToLinear(o.c);
    #endif
    o.c*=light;return o;}
   half4 frag(V i):SV_Target{return half4(i.c,1);}
   ENDHLSL
  }
 }
}
