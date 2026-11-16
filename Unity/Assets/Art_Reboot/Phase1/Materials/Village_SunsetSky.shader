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
 half4 frag(V i):SV_Target {float h=saturate(normalize(i.ray).y);return half4(lerp(_Horizon.rgb,_Zenith.rgb,pow(h,.55)),1);}
 ENDHLSL
 }
 }
}
