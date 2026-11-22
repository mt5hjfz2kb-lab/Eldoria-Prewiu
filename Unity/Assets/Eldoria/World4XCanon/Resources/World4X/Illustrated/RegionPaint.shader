Shader "Eldoria/Region1/Illustrated ground" {
 Properties {
  _BeyondMap("Outside charted terrain",Float)=0
  _BoundaryWeight("Peripheral atmosphere",Range(0,1))=0
  _BoundaryColour("Peripheral linear colour",Vector)=(0,0,0,1)
  _Region("Geographic colour",2D)="white"{} _NearFeatures("Mapped footpaths and water",2D)="black"{} _Weights("Geographic classes",2D)="black"{}
  _Grass("Meadow",2D)="white"{} _Earth("Soil",2D)="white"{}
  _Water("River",2D)="white"{} _Slate("Broken slate",2D)="white"{}
 }
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Pass {
 Cull Back
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_NearFeatures); SAMPLER(sampler_NearFeatures);
 TEXTURE2D(_Region); SAMPLER(sampler_Region); TEXTURE2D(_Weights); SAMPLER(sampler_Weights);
 TEXTURE2D(_Grass); SAMPLER(sampler_Grass); TEXTURE2D(_Earth); SAMPLER(sampler_Earth);
 TEXTURE2D(_Water); SAMPLER(sampler_Water); TEXTURE2D(_Slate); SAMPLER(sampler_Slate);
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
 struct In {float3 p:POSITION;float2 uv:TEXCOORD0;};
 struct Out {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 Out vert(In v){Out o;o.p=TransformObjectToHClip(v.p);o.uv=v.uv;return o;}
 float _BeyondMap; float _BoundaryWeight; float4 _BoundaryColour;
 half3 borderMist(float2 metres) {
  float2 cloud=metres/float2(670,450);
  float veil=SAMPLE_TEXTURE2D(_Grass,sampler_Grass,cloud).g;
  float drift=SAMPLE_TEXTURE2D(_Slate,sampler_Slate,cloud*.49+float2(.37,.21)).g;
  #ifndef UNITY_COLORSPACE_GAMMA
  veil=LinearToSRGB(veil); drift=LinearToSRGB(drift);
  #endif
  half3 mist=lerp(half3(.33,.375,.385),half3(.66,.685,.64),saturate(.07+veil*.9+drift*.36));
  #ifndef UNITY_COLORSPACE_GAMMA
  mist=SRGBToLinear(mist);
  #endif
  return mist;
 }
 half3 peripheralMist(float2 metres) {
  // Retain a restrained painted variation while converging on the same
  // neutral panorama haze as the imported regional continuation.
  // At the final overview the fog backing must become the exact camera haze.
  // Retaining even a small amount of the painted variation produces a dark
  // vertical shelf where the angled camera reveals the backing east of the
  // raised geographic relief. Regional and close views keep the full paint.
  return lerp(borderMist(metres),_BoundaryColour.rgb,saturate(_BoundaryWeight));
 }
 half4 frag(Out i):SV_Target {
  float2 metre=(i.uv-.5)*float2(1820,3600);
  if (_BeyondMap>.5) return half4(peripheralMist(metre),1);
  half3 base=SAMPLE_TEXTURE2D(_Region,sampler_Region,i.uv).rgb;
  half4 mask=SAMPLE_TEXTURE2D(_Weights,sampler_Weights,i.uv);
  float2 localUV=(metre-float2(-200,-148))/float2(416,512);
  float2 localBorder=min(localUV,1-localUV)*float2(416,512);
  half local=smoothstep(0,8,min(localBorder.x,localBorder.y));
  half4 features=SAMPLE_TEXTURE2D(_NearFeatures,sampler_NearFeatures,saturate(localUV));
  mask.r=lerp(mask.r,features.r,local);
  mask.g=max(mask.g,features.g*local);
  float2 q=metre/15;
  half3 grass=SAMPLE_TEXTURE2D(_Grass,sampler_Grass,q).rgb;
  // A second rotated meadow sample breaks the repeated 15 m texture pattern.
  float2 meadowUV=float2(.8*q.x-.6*q.y,.6*q.x+.8*q.y)*.37;
  half3 meadow=SAMPLE_TEXTURE2D(_Grass,sampler_Grass,meadowUV).rgb;
  grass=lerp(grass,meadow,.32);
  half3 earth=SAMPLE_TEXTURE2D(_Earth,sampler_Earth,q*.94).rgb;
  half3 slate=SAMPLE_TEXTURE2D(_Slate,sampler_Slate,q*.61).rgb;
  half3 water=SAMPLE_TEXTURE2D(_Water,sampler_Water,q*.81+float2(0,_Time.y*.012)).rgb;
  float shore=(features.a-.5)*12;
  float aa=clamp(fwidth(shore),.32,1.4);
  half wet=smoothstep(-aa,aa,shore);
  mask.r=lerp(mask.r,wet,local);
  // Lay a dry shingle terrace and a damp meadow fringe along the existing
  // signed bank field. Fully dry texels saturate at -6 m; fade before that
  // limit so the shoreline treatment cannot tint the entire meadow.
  half landBank=(1-wet)*local;
  half terrace=landBank*(1-smoothstep(.35,2.4,-shore));
  half fringe=landBank*(1-smoothstep(2.4,5.4,-shore));
  half3 bankMeadow=meadow*half3(.81,.91,.76);
  grass=lerp(grass,bankMeadow,fringe*.72);
  grass=lerp(grass,lerp(earth,slate,.58),terrace*.84);
  half3 albedo=lerp(grass,earth,mask.g);
  albedo=lerp(albedo,slate,mask.b);
  albedo=lerp(albedo,water,mask.r);
  float footprint=max(length(ddx(metre)),length(ddy(metre)));
  half close=1-smoothstep(.35,4.2,footprint);
  half3 colour=lerp(base,albedo,close*.57);
  half3 pathColour=lerp(base*half3(1.20,.99,.73),earth,close*.56+.24);
  half wheelWear=saturate(.67+dot(earth-meadow,half3(.33,.48,.19))*.65);
  colour=lerp(colour,pathColour,mask.g*(1-mask.r)*lerp(1,wheelWear,close));
  colour=lerp(colour,lerp(earth,slate,.34),features.b*local*.53);
  half3 riverColour=lerp(half3(.035,.145,.16),water,.37+.40*close);
  half shelf=1-smoothstep(0,3.8,shore);
  half bank=(1-smoothstep(.15,2.1,abs(shore)))*local*close;
  colour=lerp(colour,lerp(earth,slate,.32),bank*.68);
  riverColour=lerp(riverColour,lerp(water,earth,.38),shelf*local*close*.72);
  colour=lerp(colour,riverColour,mask.r);
  colour*=1+.045*sin(metre.x*.018+sin(metre.y*.027))*cos(metre.y*.014);
  half panorama=smoothstep(880,1700,unity_OrthoParams.y);
  // SurfaceWeights contains the canonical eastern sea in the same red water
  // channel as the rivers. At the final overview its fully saturated river
  // pigment formed a dark, wavy vertical band before the map-edge feather.
  // Dissolve only that already-authored eastern water progressively into the
  // peripheral haze. This is presentation-only: masks, terrain, discovery,
  // routes and every gameplay coordinate remain untouched.
  half easternSea=smoothstep(500,900,metre.x)*smoothstep(.05,.90,mask.r)*panorama;
  colour=lerp(colour,peripheralMist(metre),easternSea);
  float edge=min(910-abs(metre.x),1800-abs(metre.y));
  // Height variation already gives the mapped relief a natural silhouette.
  // The former 37 m procedural wobble displaced this blend far enough to expose
  // a dark, scalloped strip of the baked overview pigment on the eastern side.
  // Feather a wider *render-only* band at panorama zoom; geography, fog of war,
  // colliders and discovery bounds remain exactly the canonical Region 1 data.
  edge+=7*sin(metre.x*.014+metre.y*.009)+4*cos(metre.y*.027-metre.x*.012);
  half mist=panorama*(1-smoothstep(72,286,edge));
  half3 haze=mist>.001 ? peripheralMist(metre) : colour;
  return half4(lerp(colour,haze,mist),1);
 }
 ENDHLSL
 } }
}

