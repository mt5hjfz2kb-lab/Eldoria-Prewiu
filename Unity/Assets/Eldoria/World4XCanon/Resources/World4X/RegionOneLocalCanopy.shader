Shader "Eldoria/World4X/LocalCanopy"
{
 Properties { _MainTex("Canopy",2D)="white"{} }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
  Pass
  {
   Cull Off ZWrite On
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
   struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; float2 offset:TEXCOORD1; };
   struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
   Varyings vert(Attributes i)
   {
    Varyings o;
    float3 p=TransformObjectToWorld(i.positionOS);
    float3 right=float3(UNITY_MATRIX_I_V._m00,UNITY_MATRIX_I_V._m10,UNITY_MATRIX_I_V._m20);
    float3 up=float3(UNITY_MATRIX_I_V._m01,UNITY_MATRIX_I_V._m11,UNITY_MATRIX_I_V._m21);
    o.positionCS=TransformWorldToHClip(p+right*i.offset.x+up*i.offset.y);
    o.uv=i.uv; return o;
   }
   half4 frag(Varyings i):SV_Target
   {
    half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
    clip(c.a-.35h); return half4(c.rgb,1);
   }
   ENDHLSL
  }
 }
}
