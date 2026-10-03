Shader "Eldoria/Valoria Coherence"
{
 Properties {
  _BaseMap("Canonical albedo",2D)="white"{} _BumpMap("Canonical normal",2D)="bump"{}
  _BaseColor("Tint",Color)=(1,1,1,1) _BumpScale("Normal strength",Float)=0.8
  _Family("Family",Float)=0 _Bottom("World bottom",Float)=0 _Height("World height",Float)=1
  _RockMap("Shared stone transition",2D)="white"{} _Ground("Ground",Float)=0
  _Smoothness("Smoothness",Float)=0.06 _Metallic("Metallic",Float)=0
 }
 SubShader {
 Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
 Pass {
  Name "ForwardLit" Tags{"LightMode"="UniversalForward"}
  HLSLPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
  #pragma multi_compile_fragment _ _SHADOWS_SOFT
  #pragma multi_compile_fog
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
  TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
  TEXTURE2D(_BumpMap);SAMPLER(sampler_BumpMap);
  TEXTURE2D(_RockMap);SAMPLER(sampler_RockMap);
  CBUFFER_START(UnityPerMaterial)
   float4 _BaseMap_ST; half4 _BaseColor; float _Family,_Bottom,_Height,_Ground; half _BumpScale,_Smoothness,_Metallic;
  CBUFFER_END
  struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float4 tangentOS:TANGENT;float2 uv:TEXCOORD0;half4 color:COLOR;};
  struct V{float4 positionCS:SV_POSITION;float3 positionWS:TEXCOORD0;half3 normalWS:TEXCOORD1;half4 tangentWS:TEXCOORD2;float2 uv:TEXCOORD3;half4 color:TEXCOORD4;half fog:TEXCOORD5;};
  V vert(A a){V o;VertexPositionInputs p=GetVertexPositionInputs(a.positionOS.xyz);VertexNormalInputs n=GetVertexNormalInputs(a.normalOS,a.tangentOS);o.positionCS=p.positionCS;o.positionWS=p.positionWS;o.normalWS=n.normalWS;o.tangentWS=half4(n.tangentWS,a.tangentOS.w*GetOddNegativeScale());o.uv=TRANSFORM_TEX(a.uv,_BaseMap);o.color=a.color;o.fog=ComputeFogFactor(p.positionCS.z);return o;}
  half4 frag(V i):SV_Target{
   half3 tex=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).rgb;
   half3 n=normalize(i.normalWS);
   half3 nt=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,i.uv),_BumpScale);
   if(dot(i.tangentWS.xyz,i.tangentWS.xyz)>.1)n=normalize(nt.x*i.tangentWS.xyz+nt.y*cross(n,i.tangentWS.xyz)*i.tangentWS.w+nt.z*n);
   half lum=dot(tex,half3(.2126,.7152,.0722));
   half3 c=tex;
   float h=saturate((i.positionWS.y-_Bottom)/max(_Height,.001));
   if(_Family>1.5 && _Family<4.5){
    half gain=_Family<2.5?.92:(_Family<3.5?1.28:1.10);
    c=lerp(lum.xxx,tex,.56)*gain;
    c*=half3(1.04,1.02,.94);
    half roof=smoothstep(.46,.66,h)*smoothstep(.32,.62,abs(i.normalWS.y));
    c=lerp(c,half3(.24,.32,.39)*(lum*1.35+.28),roof*.64);
   }
   if(_Family>4.5){c=lerp(lum.xxx,tex,.28)*1.10*half3(1.10,1.07,.98);}
   if(_Family>.5 && _Family<1.5){
    c=lerp(lum.xxx,tex,.85)*.89;
    float rock=1-smoothstep(.14,.245,h);
    half3 weights=pow(abs(i.normalWS),4);weights/=max(dot(weights,1),.001);
    half3 r=SAMPLE_TEXTURE2D(_RockMap,sampler_RockMap,i.positionWS.zy*.52).rgb*weights.x+SAMPLE_TEXTURE2D(_RockMap,sampler_RockMap,i.positionWS.xz*.52).rgb*weights.y+SAMPLE_TEXTURE2D(_RockMap,sampler_RockMap,i.positionWS.xy*.52).rgb*weights.z;
    half rl=dot(r,half3(.2126,.7152,.0722));
    c=lerp(c,half3(.48,.465,.41)*(rl*.85+.48),rock*.85);
   }
   if(_Ground>.5){
    c=tex*i.color.rgb;
    float road=(1-smoothstep(1.2,2.3,abs(i.positionWS.x)))*(1-smoothstep(-7,-5,i.positionWS.z));
    c=lerp(c,half3(.36,.31,.23)*(.65+lum),road);
   }
   c*=_BaseColor.rgb;
   InputData d=(InputData)0;d.positionWS=i.positionWS;d.normalWS=n;d.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.positionWS);d.shadowCoord=TransformWorldToShadowCoord(i.positionWS);d.bakedGI=SampleSH(n);d.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.positionCS);d.shadowMask=half4(1,1,1,1);
   half4 outCol=UniversalFragmentPBR(d,c,_Metallic,half3(.04,.04,.04),_Smoothness,1,half3(0,0,0),1);
   outCol.rgb=MixFog(outCol.rgb,i.fog);
   if(_Ground>.5){float distanceToCity=length(i.positionWS.xz-float2(0,1.5));outCol.rgb=lerp(outCol.rgb,half3(.48,.53,.54),smoothstep(24,49,distanceToCity));}
   return outCol;
  }
  ENDHLSL
 }
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
