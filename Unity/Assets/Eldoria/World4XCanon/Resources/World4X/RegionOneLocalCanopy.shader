Shader "Eldoria/World4X/LocalCanopy"
{
 Properties { _MainTex("Canopy",2D)="white"{} _Tint("Art tint",Color)=(1,1,1,1) _BillboardScale("World silhouette scale",Float)=1 _BoundaryWeight("Panorama boundary",Range(0,1))=0 _BoundaryColour("Panorama haze",Color)=(.63,.685,.665,1) _SoftEdges("Bastion I painterly alpha",Range(0,1))=0 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
  Pass
  {
   Cull Off ZWrite On Blend SrcAlpha OneMinusSrcAlpha
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
   CBUFFER_START(UnityPerMaterial) half4 _Tint; half4 _BoundaryColour; float _BillboardScale; float _BoundaryWeight; float _SoftEdges; CBUFFER_END
   struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; float2 offset:TEXCOORD1; };
   struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float2 metre:TEXCOORD1; };
   Varyings vert(Attributes i)
   {
    Varyings o;
    float3 p=TransformObjectToWorld(i.positionOS);
    float3 right=float3(UNITY_MATRIX_I_V._m00,UNITY_MATRIX_I_V._m10,UNITY_MATRIX_I_V._m20);
    float3 up=float3(UNITY_MATRIX_I_V._m01,UNITY_MATRIX_I_V._m11,UNITY_MATRIX_I_V._m21);
    o.positionCS=TransformWorldToHClip(p+(right*i.offset.x+up*i.offset.y)*_BillboardScale);
    // Batched illustrated vegetation is authored in canonical Region 1
    // metres. Preserve the foot position so the final overview can dissolve
    // crowns into the same peripheral atmosphere as the relief beneath them.
    o.uv=i.uv; o.metre=i.positionOS.xz; return o;
   }
   half4 frag(Varyings i):SV_Target
   {
    half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
    float edge=min(910-abs(i.metre.x),1800-abs(i.metre.y));
    half boundary=lerp(1,smoothstep(-16,150,edge),saturate(_BoundaryWeight));
    // Genuine alpha-blended city sprites have translucent fine edges.
    // In modular mode, keep their anti-aliased painted crowns and roofs
    // without adding another graphics layer or touching geographic data.
    half coverage=saturate(c.a*boundary);
    half softened=saturate(_SoftEdges);
    clip(coverage-lerp(.35h,.025h,softened));
    half3 colour=lerp(c.rgb*_Tint.rgb,_BoundaryColour.rgb,(1-boundary)*saturate(_BoundaryWeight));
    return half4(colour,lerp(1.0h,coverage,softened));
   }
   ENDHLSL
  }
 }
}
