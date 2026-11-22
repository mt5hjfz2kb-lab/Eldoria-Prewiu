Shader "Eldoria/World4X/LocalCanopy"
{
 Properties { _FaultLight("Open geological seam",Float)=0 _CreatureTempo("Painted creature idle tempo",Float)=0  _MainTex("Canopy",2D)="white"{} _Tint("Art tint",Color)=(1,1,1,1) _BillboardScale("World silhouette scale",Float)=1 _BoundaryWeight("Panorama boundary",Range(0,1))=0 _BoundaryColour("Panorama haze",Color)=(.63,.685,.665,1) _SoftEdges("Bastion I painterly alpha",Range(0,1))=0 _CameraFacingDepth("Modular orthographic foreground depth",Float)=0 }
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
   CBUFFER_START(UnityPerMaterial) half4 _Tint; half4 _BoundaryColour; float _BillboardScale; float _BoundaryWeight; float _FaultLight; float _CreatureTempo; float _SoftEdges; float _CameraFacingDepth; CBUFFER_END
   struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; float2 offset:TEXCOORD1; };
   struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float2 metre:TEXCOORD1; };
   Varyings vert(Attributes i)
   {
    Varyings o;
    float3 p=TransformObjectToWorld(i.positionOS);
    float3 right=float3(UNITY_MATRIX_I_V._m00,UNITY_MATRIX_I_V._m10,UNITY_MATRIX_I_V._m20);
    float3 up=float3(UNITY_MATRIX_I_V._m01,UNITY_MATRIX_I_V._m11,UNITY_MATRIX_I_V._m21);
    // Move ONLY the rasterized camera-facing art toward the camera along the
    // view-depth axis. This is orthographic depth ordering: all screen-space
    // positions, authoring feet, world geography, selection, routes and saves
    // remain unchanged. Without it the elevated geographic relief z-buffers
    // away the lower castle, tree trunks and resource sites.
    float3 towardCamera=float3(UNITY_MATRIX_I_V._m02,UNITY_MATRIX_I_V._m12,UNITY_MATRIX_I_V._m22);
    float3 painted=p+(right*i.offset.x+up*i.offset.y)*_BillboardScale
                   +towardCamera*_CameraFacingDepth;
    o.positionCS=TransformWorldToHClip(painted);
    // Batched illustrated vegetation is authored in canonical Region 1
    // metres. Preserve the foot position so the final overview can dissolve
    // crowns into the same peripheral atmosphere as the relief beneath them.
    o.uv=i.uv; o.metre=i.positionOS.xz; return o;
   }
   half4 frag(Varyings i):SV_Target
   {
    if (_CreatureTempo>0)
    {
     float3 anchor=float3(GetObjectToWorldMatrix()._m03,0,GetObjectToWorldMatrix()._m23);
     float t=_Time.y*_CreatureTempo+dot(anchor.xz,float2(.13,.21));
     // Inverse sprite warp: snout lowers to sniff, shoulder expands,
     // paws stay registered. Each encounter has its own resting phase.
     float body=smoothstep(.12,.54,i.uv.y);
     float head=(1-smoothstep(.12,.58,i.uv.x))*smoothstep(.10,.31,i.uv.y);
     float sniff=pow(saturate(.5+.5*sin(t*.71)),4);
     i.uv.y+=head*.040*sniff-body*.016*sin(t*2.1);
     i.uv.x+=body*.009*sin(t*1.3);
    }
    half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
    if (_FaultLight>.5)
    {
     // Light comes only from the painted violet interior. Rock lips, roots
     // and translucent earth keep the upper-left illumination of the city.
     half violet=saturate(min(c.b-c.g,c.r-c.g)*34);
     half pulse=.89+.11*sin(_Time.y*.86+i.uv.x*8);
     c.rgb+=half3(.24,.047,.43)*violet*pulse;
    }
    float edge=min(910-abs(i.metre.x),1800-abs(i.metre.y));
    if(_SoftEdges>.5 && _BoundaryWeight>0)
     edge+=7*sin(i.metre.x*.014+i.metre.y*.009)+4*cos(i.metre.y*.027-i.metre.x*.012);
    half boundary=lerp(1,lerp(smoothstep(-16,150,edge),smoothstep(72,286,edge),_SoftEdges),saturate(_BoundaryWeight));
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
