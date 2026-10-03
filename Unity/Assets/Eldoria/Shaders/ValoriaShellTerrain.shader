Shader "Eldoria/ValoriaShellTerrain"
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
    float3 n=normalize(i.n);float3 w=pow(abs(n),4);w/=max(.001,w.x+w.y+w.z);
    float3 p=i.w*.38;
    half3 rock=SAMPLE_TEXTURE2D(_RockTex,sampler_RockTex,p.zy).rgb*w.x+SAMPLE_TEXTURE2D(_RockTex,sampler_RockTex,p.xz).rgb*w.y+SAMPLE_TEXTURE2D(_RockTex,sampler_RockTex,p.xy).rgb*w.z;
    half3 ground=SAMPLE_TEXTURE2D(_GroundTex,sampler_GroundTex,i.w.xz*.30).rgb;
    float macro=.5+.5*sin(i.w.x*.37+sin(i.w.z*.23))*sin(i.w.z*.31);
    float slope=smoothstep(.56,.87,n.y);
    float grass=slope*smoothstep(.46,.78,macro);
    ground=lerp(ground*half3(.70,.66,.54),ground*half3(.40,.57,.25),grass*.65);
    half3 albedo=lerp(rock*half3(.70,.69,.62),ground,slope*.72);
    albedo*=lerp(.86,1.10,macro);
    // Triplanar normal detail uses projected gradients without UV seams.
    float3 nx=UnpackNormal(SAMPLE_TEXTURE2D(_RockNormal,sampler_RockNormal,p.zy));
    float3 ny=UnpackNormal(SAMPLE_TEXTURE2D(_RockNormal,sampler_RockNormal,p.xz));
    float3 nz=UnpackNormal(SAMPLE_TEXTURE2D(_RockNormal,sampler_RockNormal,p.xy));
    float3 detail=float3(0,nx.y,nx.x)*w.x+float3(ny.x,0,ny.y)*w.y+float3(nz.x,nz.y,0)*w.z;
    n=normalize(n+detail*.24);
    InputData data=(InputData)0;data.positionWS=i.w;data.normalWS=n;data.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.w);
    data.shadowCoord=TransformWorldToShadowCoord(i.w);data.bakedGI=SampleSH(n);data.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.p);data.shadowMask=half4(1,1,1,1);
    SurfaceData s=(SurfaceData)0;s.albedo=albedo;s.metallic=0;s.specular=half3(.04,.04,.04);s.smoothness=.035;s.normalTS=half3(0,0,1);s.occlusion=.88;s.alpha=1;
    half4 col=UniversalFragmentPBR(data,s);col.rgb=MixFog(col.rgb,i.fog);return col;
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
