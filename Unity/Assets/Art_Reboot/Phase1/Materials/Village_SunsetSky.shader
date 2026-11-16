Shader "Eldoria/ArtReboot/SunsetSky"
{
 Properties {_Zenith("Zenith",Color)=(.16,.18,.32,1) _Horizon("Horizon",Color)=(.72,.34,.12,1)}
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
 struct A {float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 ray:TEXCOORD0;};
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.ray=i.p.xyz;return o;}
 half4 frag(V i):SV_Target {
   // In an orthographic camera tilted down ~42 degrees, all visible rays
   // have negative Y: saturate(ray.y) flattened the entire sky to Horizon.
   // Remap the below-horizontal range to make real violet/amber dusk gradient.
   float rayY=normalize(i.ray).y;
   float h=saturate((rayY + .88)*2.7);
   float t=h*h*(3.0-2.0*h);
   return half4(lerp(_Horizon.rgb,_Zenith.rgb,t),1);
 }
 ENDHLSL
 }
 }
}
