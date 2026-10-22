Shader "Eldoria/World4X/DirectedGeographicLayers"
{
 Properties { _Mask("Authored geography masks",2D)="black"{} _Pigment("Painted meadow",2D)="white"{} _Origin("World origin",Vector)=(0,0,0,0) _Near("Near detail",Float)=0 }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass
  {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
   struct A{float3 positionOS:POSITION;float3 normalOS:NORMAL;};
   struct V{float4 positionCS:SV_POSITION;float2 p:TEXCOORD0;float3 n:TEXCOORD1;};
   TEXTURE2D(_Mask);SAMPLER(sampler_Mask);TEXTURE2D(_Pigment);SAMPLER(sampler_Pigment);
   CBUFFER_START(UnityPerMaterial) float4 _Origin;float _Near; CBUFFER_END
   V vert(A v){V o;float3 w=TransformObjectToWorld(v.positionOS);o.positionCS=TransformWorldToHClip(w);o.p=w.xz-_Origin.xy;o.n=TransformObjectToWorldNormal(v.normalOS);return o;}
   half4 frag(V i):SV_Target
   {
    float2 uv=i.p/float2(500,1040)+.5;
    half4 m=SAMPLE_TEXTURE2D(_Mask,sampler_Mask,uv);
    half3 tex=SAMPLE_TEXTURE2D(_Pigment,sampler_Pigment,abs(frac((i.p+float2(17,31))/64)*2-1)).rgb;
    half3 earth=half3(.69,.68,.33),sand=half3(.80,.70,.43),sea=half3(.045,.25,.36),shallow=half3(.07,.46,.56),river=half3(.06,.47,.64),soil=half3(.80,.70,.43);
    #if !defined(UNITY_COLORSPACE_GAMMA)
     earth=SRGBToLinear(earth);sand=SRGBToLinear(sand);sea=SRGBToLinear(sea);shallow=SRGBToLinear(shallow);river=SRGBToLinear(river);soil=SRGBToLinear(soil);
    #endif
    half3 land=lerp(earth,tex,.26h+_Near*.52h);
    land*=1-m.a*.16h;
    land=lerp(land,soil,m.b*.72h);
    land=lerp(land,river,m.g);
    half light=.82h+.18h*saturate(dot(normalize(i.n),normalize(float3(-.6,1,-.4))));
    half3 coast=lerp(sand*.73h,sand,light);
    half landWeight=smoothstep(.49h,.54h,m.r);
    half3 water=lerp(sea,shallow,smoothstep(.02h,.49h,m.r));
    half ripple=sin(i.p.x*.17+i.p.y*.1)*sin(i.p.y*.3)*.017;
    water+=ripple;
    half3 c=lerp(water,lerp(coast,land,smoothstep(.55h,.66h,m.r)),landWeight);
    float edgeWave=sin(i.p.y*.035)*8+sin(i.p.y*.087+i.p.x*.03)*4;
    half fog=max(smoothstep(164,232,-i.p.x+edgeWave),max(smoothstep(420,505,i.p.y+edgeWave),smoothstep(418,505,-i.p.y+edgeWave)));
    half3 mist=half3(.78,.83,.85);
    #if !defined(UNITY_COLORSPACE_GAMMA)
     mist=SRGBToLinear(mist);
    #endif
    return half4(lerp(c,mist,fog*.88h),1);
   }
   ENDHLSL
  }
 }
}
