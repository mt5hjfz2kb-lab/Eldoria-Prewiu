Shader "Eldoria/ArtReboot/SoftParticle"
{
 Properties { _EmissionScale("Emission",Float)=1 }
 SubShader {
 Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
 Pass {
 Tags {"LightMode"="SRPDefaultUnlit"}
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off
 Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float _EmissionScale;
 CBUFFER_END
 struct A {float4 p:POSITION;half4 c:COLOR;float2 uv:TEXCOORD0;};
 struct V {float4 p:SV_POSITION;half4 c:COLOR;float2 uv:TEXCOORD0;};
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.c=i.c;o.uv=i.uv;return o;}
 half4 frag(V i):SV_Target {float r=length(i.uv-.5)*2;float a=saturate(1-r*r);return half4(i.c.rgb*_EmissionScale,i.c.a*a*a);}
 ENDHLSL
 }
 }
}
