Shader "Eldoria/ArtReboot/KitSunsetSky"
{
 Properties {
  _Zenith("Blue violet twilight",Color)=(.23,.19,.43,1)
  _Horizon("Amber low horizon",Color)=(.89,.50,.29,1)
 }
 SubShader {
  Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline"}
  Cull Off ZWrite Off
  Pass {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
     half4 _Zenith;half4 _Horizon;
   CBUFFER_END
   struct A{float4 positionOS:POSITION;};
   struct V{float4 positionCS:SV_POSITION;float3 direction:TEXCOORD0;};
   V vert(A v) {
     V o;
     o.positionCS=TransformObjectToHClip(v.positionOS.xyz);
     o.direction=v.positionOS.xyz;
     return o;
   }
   half4 frag(V input):SV_Target {
     float y=normalize(input.direction).y;
     // Phase1 camera tilts ~42 degrees down. Remap its negative-Y rays
     // so the horizon is warm amber, top of frame blue violet (not a dark strip).
     float t=saturate((y+1.0)*4.2);
     t=t*t*(3.0-2.0*t);
     return half4(lerp(_Horizon.rgb,_Zenith.rgb,t),1.0);
   }
   ENDHLSL
  }
 }
 Fallback Off
}
