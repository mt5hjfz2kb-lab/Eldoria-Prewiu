Shader "Eldoria/Region1/Illustrated ground" {
 Properties {
  _Region("Geographic colour",2D)="white"{} _Weights("Geographic classes",2D)="black"{}
  _Grass("Meadow",2D)="white"{} _Earth("Soil",2D)="white"{}
  _Water("River",2D)="white"{} _Slate("Broken slate",2D)="white"{}
 }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Pass {
 Cull Back
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_Region); SAMPLER(sampler_Region); TEXTURE2D(_Weights); SAMPLER(sampler_Weights);
 TEXTURE2D(_Grass); SAMPLER(sampler_Grass); TEXTURE2D(_Earth); SAMPLER(sampler_Earth);
 TEXTURE2D(_Water); SAMPLER(sampler_Water); TEXTURE2D(_Slate); SAMPLER(sampler_Slate);
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
 struct In {float3 p:POSITION;float2 uv:TEXCOORD0;};
 struct Out {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 Out vert(In v){Out o;o.p=TransformObjectToHClip(v.p);o.uv=v.uv;return o;}
 half4 frag(Out i):SV_Target {
  float2 metre=(i.uv-.5)*float2(1820,3600);
  half3 base=SAMPLE_TEXTURE2D(_Region,sampler_Region,i.uv).rgb;
  half4 mask=SAMPLE_TEXTURE2D(_Weights,sampler_Weights,i.uv);
  float2 q=metre/15;
  half3 grass=SAMPLE_TEXTURE2D(_Grass,sampler_Grass,q).rgb;
  half3 earth=SAMPLE_TEXTURE2D(_Earth,sampler_Earth,q*.94).rgb;
  half3 slate=SAMPLE_TEXTURE2D(_Slate,sampler_Slate,q*.61).rgb;
  half3 water=SAMPLE_TEXTURE2D(_Water,sampler_Water,q*.81+float2(0,_Time.y*.012)).rgb;
  half3 albedo=lerp(grass,earth,mask.g);
  albedo=lerp(albedo,slate,mask.b);
  albedo=lerp(albedo,water,mask.r);
  float footprint=max(length(ddx(metre)),length(ddy(metre)));
  half close=1-smoothstep(.35,4.2,footprint);
  half3 colour=lerp(base,albedo,close*.57);
  colour*=1+.045*sin(metre.x*.018+sin(metre.y*.027))*cos(metre.y*.014);
  float edge=min(910-abs(metre.x),1800-abs(metre.y));
  edge+=14*sin(metre.x*.014+metre.y*.009);
  half mist=smoothstep(2900,3900,unity_OrthoParams.y)*(1-smoothstep(0,125,edge));
  half3 haze=half3(.63,.685,.665);
  #ifndef UNITY_COLORSPACE_GAMMA
  haze=SRGBToLinear(haze);
  #endif
  return half4(lerp(colour,haze,mist),1);
 }
 ENDHLSL
 } }
}
