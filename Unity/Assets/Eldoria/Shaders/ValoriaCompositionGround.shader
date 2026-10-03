Shader "Eldoria/ValoriaCompositionGround"
{
 Properties { _RockTex("Rock albedo",2D)="white"{} _GroundTex("Ground albedo",2D)="white"{} _RockNormal("Rock normal",2D)="bump"{} }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
  Pass
  {
   Tags { "LightMode"="UniversalForward" }
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_RockTex);SAMPLER(sampler_RockTex);
   TEXTURE2D(_GroundTex);SAMPLER(sampler_GroundTex);
   TEXTURE2D(_RockNormal);SAMPLER(sampler_RockNormal);
   struct A{float4 p:POSITION;float3 n:NORMAL;};
   struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float fog:TEXCOORD2;};
   V vert(A i){V o;VertexPositionInputs p=GetVertexPositionInputs(i.p.xyz);o.p=p.positionCS;o.w=p.positionWS;o.n=TransformObjectToWorldNormal(i.n);o.fog=ComputeFogFactor(p.positionCS.z);return o;}
   half4 frag(V i):SV_Target
   {
    float3 n=normalize(i.n);float3 weights=pow(abs(n),4);weights/=max(.001,weights.x+weights.y+weights.z);
    float3 p=i.w*.43;
    half3 r=SAMPLE_TEXTURE2D(_RockTex,sampler_RockTex,p.zy).rgb*weights.x+
            SAMPLE_TEXTURE2D(_RockTex,sampler_RockTex,p.xz).rgb*weights.y+
            SAMPLE_TEXTURE2D(_RockTex,sampler_RockTex,p.xy).rgb*weights.z;
    half3 e=SAMPLE_TEXTURE2D(_GroundTex,sampler_GroundTex,i.w.xz*.35).rgb;
    float macro=.5+.5*sin(i.w.x*.43+sin(i.w.z*.21))*sin(i.w.z*.34);
    float earth=smoothstep(.49,.82,n.y)*(.76+.18*macro);
    half3 albedo=lerp(r*half3(.63,.63,.59),e*half3(.66,.65,.59),earth);
    albedo*=lerp(.91,1.06,macro);
    float3 nx=UnpackNormal(SAMPLE_TEXTURE2D(_RockNormal,sampler_RockNormal,p.zy));
    float3 ny=UnpackNormal(SAMPLE_TEXTURE2D(_RockNormal,sampler_RockNormal,p.xz));
    float3 nz=UnpackNormal(SAMPLE_TEXTURE2D(_RockNormal,sampler_RockNormal,p.xy));
    float3 detail=float3(0,nx.y,nx.x)*weights.x+float3(ny.x,0,ny.y)*weights.y+float3(nz.x,nz.y,0)*weights.z;
    n=normalize(n+detail*.18*(1-earth));
    InputData data=(InputData)0;data.positionWS=i.w;data.normalWS=n;data.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.w);
    data.shadowCoord=TransformWorldToShadowCoord(i.w);data.bakedGI=SampleSH(n);data.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.p);data.shadowMask=half4(1,1,1,1);
    SurfaceData s=(SurfaceData)0;s.albedo=albedo;s.metallic=0;s.specular=half3(.04,.04,.04);s.smoothness=.025;s.normalTS=half3(0,0,1);s.occlusion=.88;s.alpha=1;
    half4 col=UniversalFragmentPBR(data,s);col.rgb=MixFog(col.rgb,i.fog);return col;
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
