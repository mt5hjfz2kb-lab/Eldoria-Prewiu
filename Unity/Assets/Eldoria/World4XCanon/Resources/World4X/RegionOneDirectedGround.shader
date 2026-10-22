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
   float hash21(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise21(float2 p){float2 k=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash21(k),hash21(k+float2(1,0)),f.x),lerp(hash21(k+float2(0,1)),hash21(k+1),f.x),f.y);}
   float cloudBank(float2 p,float2 c,float2 radius){float2 d=(p-c)/radius;return exp(-dot(d,d)*1.7);}
   half4 frag(V i):SV_Target
   {
    float2 uv=i.p/float2(500,1040)+.5;
    half4 m=SAMPLE_TEXTURE2D(_Mask,sampler_Mask,uv);
    half3 tex=SAMPLE_TEXTURE2D(_Pigment,sampler_Pigment,abs(frac((i.p+float2(17,31))/64)*2-1)).rgb;
    half3 earth=half3(.72,.66,.30),sand=half3(.80,.70,.43),sea=half3(.045,.25,.36),shallow=half3(.07,.46,.56),river=half3(.06,.47,.64),soil=half3(.80,.70,.43);
    #if !defined(UNITY_COLORSPACE_GAMMA)
     earth=SRGBToLinear(earth);sand=SRGBToLinear(sand);sea=SRGBToLinear(sea);shallow=SRGBToLinear(shallow);river=SRGBToLinear(river);soil=SRGBToLinear(soil);
    #endif
    half3 broad=SAMPLE_TEXTURE2D(_Pigment,sampler_Pigment,abs(frac((i.p+float2(43,7))/150)*2-1)).rgb;
    half3 land=lerp(lerp(earth,broad,.48h),tex,.20h+_Near*.50h);
    land*=1-m.a*.28h;
    land=lerp(land,soil,m.b*.72h);
    half bank=smoothstep(.02h,.18h,m.g)*(1-smoothstep(.25h,.65h,m.g));
    land=lerp(land,sand,bank*.8h);
    half3 freshWater=lerp(shallow,river,smoothstep(.35h,.96h,m.g));
    freshWater*=.94h+.12h*noise21(i.p*.16);
    land=lerp(land,freshWater,smoothstep(.16h,.75h,m.g));
    half light=.82h+.18h*saturate(dot(normalize(i.n),normalize(float3(-.6,1,-.4))));
    half3 coast=lerp(sand*.73h,sand,light);
    half landWeight=smoothstep(.40h,.49h,m.r);
    half3 water=lerp(sea,shallow,smoothstep(.38h,.49h,m.r));
    half ripple=(noise21(i.p*.13)-.5h)*.012h;
    water+=ripple;
    half foam=smoothstep(.42h,.47h,m.r)*(1-smoothstep(.47h,.50h,m.r));
    water=lerp(water,sand,foam*.38h);
    half3 c=lerp(water,lerp(coast,land,smoothstep(.55h,.68h,m.r)),landWeight);
    half side=1-saturate(normalize(i.n).y);
    c=lerp(c,sand*(.49h+.30h*light),side*landWeight*.92h);
    half fog=max(max(cloudBank(i.p,float2(-195,290),float2(115,160)),cloudBank(i.p,float2(-75,390),float2(145,115))),max(cloudBank(i.p,float2(95,420),float2(150,120)),cloudBank(i.p,float2(-185,-50),float2(65,160))));
    fog=max(fog,max(cloudBank(i.p,float2(-165,-315),float2(90,120)),max(cloudBank(i.p,float2(-45,-400),float2(140,115)),cloudBank(i.p,float2(110,-405),float2(110,105)))));
    half3 mist=half3(.78,.83,.85);
    #if !defined(UNITY_COLORSPACE_GAMMA)
     mist=SRGBToLinear(mist);
    #endif
    half cloud=.48h*noise21(i.p*.021)+.32h*noise21(i.p*.049+17)+.20h*noise21(i.p*.105+41);
    half cloudAlpha=smoothstep(.13h,.68h,fog*(.40h+.95h*cloud));
    return half4(lerp(c,mist*(.88h+.12h*cloud),cloudAlpha*.94h),1);
   }
   ENDHLSL
  }
 }
}
