Shader "Eldoria/ReferenceClean/ShoreFoam" {
 Properties { [MainColor] _BaseColor("Foam tint",Color)=(.55,.78,.76,.52) }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
  Pass {
   Tags {"LightMode"="UniversalForward"}
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor;
   CBUFFER_END
   struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
   struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float2 uv:TEXCOORD1;};
   V vert(A a){V o;o.world=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.uv=a.uv;return o;}
   half4 frag(V i):SV_Target {float wave=.80+.20*sin(i.world.x*9+i.world.z*13+_Time.y*1.2);float edge=1-smoothstep(.12,1,i.uv.y);return half4(_BaseColor.rgb,_BaseColor.a*wave*edge);}
   ENDHLSL
  }
 }
}
