Shader "Eldoria/Region1/Illustrated ground" {
 Properties {
  _BeyondMap("Outside charted terrain",Float)=0
  _BoundaryWeight("Peripheral atmosphere",Range(0,1))=0
  _BoundaryColour("Peripheral linear colour",Vector)=(0,0,0,1)
  _Region("Geographic colour",2D)="white"{} _NearFeatures("Mapped footpaths and water",2D)="black"{} _Weights("Geographic classes",2D)="black"{}
  _Grass("Meadow",2D)="white"{} _Earth("Soil",2D)="white"{}
  _Water("River",2D)="white"{} _Slate("Broken slate",2D)="white"{}
  _CityMeadow("Bastion I approved meadow",2D)="white"{}
  _CityEarth("Bastion I painted earth lanes",2D)="white"{}
  _CityCobble("Bastion I illustrated paving",2D)="white"{}
  _CityFinish("Shared modular paint",Range(0,1))=1
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
 TEXTURE2D(_CityMeadow); SAMPLER(sampler_CityMeadow);
 TEXTURE2D(_CityEarth); SAMPLER(sampler_CityEarth);
 TEXTURE2D(_CityCobble); SAMPLER(sampler_CityCobble);
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
 struct In {float3 p:POSITION;float2 uv:TEXCOORD0;};
 struct Out {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;};
 Out vert(In v){Out o;o.p=TransformObjectToHClip(v.p);o.uv=v.uv;
                 o.world=TransformObjectToWorld(v.p);return o;}
 float _BeyondMap; float _BoundaryWeight; float4 _BoundaryColour; float _CityFinish;
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
  // This is the only visible regional ground surface. The old imported
  // colour-map is NOT drawn as a background image beneath another landscape.
  // Canonical geography supplies masks and height, while the very same
  // Valoria paint sources used at Bastion I supply the actual meadow/road.
  half3 geographicLight=SAMPLE_TEXTURE2D(_Region,sampler_Region,i.uv).rgb;
  half4 mask=SAMPLE_TEXTURE2D(_Weights,sampler_Weights,i.uv);
  float2 localUV=(metre-float2(-200,-148))/float2(416,512);
  float2 localBorder=min(localUV,1-localUV)*float2(416,512);
  half local=smoothstep(0,8,min(localBorder.x,localBorder.y));
  half4 features=SAMPLE_TEXTURE2D(_NearFeatures,sampler_NearFeatures,saturate(localUV));
  mask.r=lerp(mask.r,features.r,local);
  mask.g=max(mask.g,features.g*local);

  // 6.5 m world-space tile: identical ground UV density to Modular25DGround.
  // A second rotated sample softens repeats without introducing another
  // surface or altering any canonical map coordinate.
  float2 cityUV=metre/6.5;
  float2 rotatedUV=float2(cityUV.x*.8-cityUV.y*.6,
                          cityUV.x*.6+cityUV.y*.8)*.59;
  half3 cityMeadow=SAMPLE_TEXTURE2D(_CityMeadow,sampler_CityMeadow,cityUV).rgb;
  half3 cityVariety=SAMPLE_TEXTURE2D(_CityMeadow,sampler_CityMeadow,rotatedUV).rgb;
  cityMeadow=lerp(cityMeadow,cityVariety,.16);
  // Keep the SAME approved painted meadow legible as 6.5m detail falls
  // beneath one strategic camera pixel: spatially enlarge its brush texture
  // instead of returning to an obsolete full-continent image.
  half3 meadowGlade=SAMPLE_TEXTURE2D(_CityMeadow,sampler_CityMeadow,
      metre/86+float2(.27,.43)).rgb;
  half3 meadowRange=SAMPLE_TEXTURE2D(_CityMeadow,sampler_CityMeadow,
      float2(metre.x*.80-metre.y*.60,metre.x*.60+metre.y*.80)/252
      +float2(.61,.19)).rgb;
  float2 terrainUV=metre/15;
  half3 continentalGrass=SAMPLE_TEXTURE2D(_Grass,sampler_Grass,terrainUV).rgb;
  half3 continentalVariety=SAMPLE_TEXTURE2D(_Grass,sampler_Grass,
      float2(.8*terrainUV.x-.6*terrainUV.y,
             .6*terrainUV.x+.8*terrainUV.y)*.37).rgb;
  continentalGrass=lerp(continentalGrass,continentalVariety,.29);
  float pixelMetres=max(length(ddx(metre)),length(ddy(metre)));
  half close=1-smoothstep(.35,4.2,pixelMetres);
  half paintedScale=1-smoothstep(1.4,8.0,pixelMetres);
  half strategicScale=smoothstep(.65,7.2,pixelMetres);
  half3 farPainting=lerp(meadowGlade,meadowRange,.25);
  // Do not magnify 1m grass blades into 35m billboard-like tufts in panorama.
  // The enlarged city tile controls only broad tone; the actual fine brush
  // strokes retain their world-space size and mip naturally with the camera.
  half luminanceVariation=dot(farPainting-cityMeadow,half3(.25,.64,.11));
  half macroTint=clamp(1+luminanceVariation*.36,.89,1.11);
  half3 cityAtZoom=cityMeadow*lerp(1,macroTint,strategicScale*.88);
  // Paint continuity from Bastion I to the near, regional and panoramic map.
  // This operates INSIDE the one regional terrain shader, not on new planes.
  half cityPaint=saturate(_CityFinish)*lerp(.84,.98,paintedScale);
  half3 grass=lerp(continentalGrass,cityAtZoom,cityPaint);
  half3 earth=SAMPLE_TEXTURE2D(_Earth,sampler_Earth,terrainUV*.94).rgb;
  half3 slate=SAMPLE_TEXTURE2D(_Slate,sampler_Slate,terrainUV*.61).rgb;
  half3 water=SAMPLE_TEXTURE2D(_Water,sampler_Water,terrainUV*.81+float2(0,_Time.y*.012)).rgb;
  // City road pigment only replaces the road material already described by
  // the ORIGINAL mapped path mask. There are no raised photo decals.
  half4 cityDirt=SAMPLE_TEXTURE2D(_CityEarth,sampler_CityEarth,
                                   metre/6.5+float2(.17,.09));
  half4 cityPaving=SAMPLE_TEXTURE2D(_CityCobble,sampler_CityCobble,
                                     metre/5.7+float2(.12,.24));
  half3 sharedEarth=lerp(earth,cityDirt.rgb,cityDirt.a*.75*_CityFinish);
  half cityNear=1-smoothstep(55,138,length(metre));
  half3 pathColour=lerp(sharedEarth,cityPaving.rgb,
                         cityNear*cityPaving.a*.65*_CityFinish);
  // The riverbanks come from the actual signed geographic shoreline.
  float shore=(features.a-.5)*12;
  float aa=clamp(fwidth(shore),.32,1.4);
  half wet=smoothstep(-aa,aa,shore);
  mask.r=lerp(mask.r,wet,local);
  half landBank=(1-wet)*local;
  half terrace=landBank*(1-smoothstep(.35,2.4,-shore));
  half fringe=landBank*(1-smoothstep(2.4,5.4,-shore));
  grass=lerp(grass,cityMeadow*half3(.81,.91,.76),fringe*.72);
  grass=lerp(grass,lerp(sharedEarth,slate,.58),terrace*.84);
  half3 colour=lerp(grass,slate,saturate(mask.b)*.87);
  // Use the archival region map only as a subtle broad RELIEF lighting
  // measurement, never as visible full-scene artwork.
  half terrainLight=dot(geographicLight,half3(.2126,.7152,.0722));
  colour*=clamp(.93+(.5-terrainLight)*-.16,.85,1.10);
  // Roads inherit the same organic masked geometry, but their material
  // matches Valoria's actual painted soil, not the previous flat ochre.
  half road=saturate(mask.g*(1-mask.r));
  half wear=saturate(.72+dot(sharedEarth-cityMeadow,half3(.30,.47,.23))*.30);
  colour=lerp(colour,pathColour,road*lerp(.96,wear,close));
  colour=lerp(colour,lerp(sharedEarth,slate,.34),features.b*local*.48);
  half3 riverColour=lerp(half3(.035,.145,.16),water,.37+.40*close);
  half shelf=1-smoothstep(0,3.8,shore);
  half bank=(1-smoothstep(.15,2.1,abs(shore)))*local*close;
  colour=lerp(colour,lerp(earth,slate,.32),bank*.68);
  riverColour=lerp(riverColour,lerp(water,earth,.38),shelf*local*close*.72);
  colour=lerp(colour,riverColour,mask.r);
  // The geographic mesh is REAL height-based terrain. Shade that geometry
  // directly with a restrained painterly key matching Valoria's baked
  // upper-left light. The previous unlit sampled colour looked like a flat
  // carpet when the camera tilted or crossed the Breach's hills.
  // No legacy PBR mesh, light rig, decal or additional art layer is drawn.
  float3 geometricNormal=cross(ddx(i.world),ddy(i.world));
  geometricNormal*=rsqrt(max(dot(geometricNormal,geometricNormal),1.e-8));
  if(geometricNormal.y<0)geometricNormal=-geometricNormal;
  half geomLight=saturate(dot(geometricNormal,normalize(float3(-.38,.85,.36))));
  half paintedLight=lerp(.915,1.085,geomLight);
  colour*=lerp(1,paintedLight,(1-saturate(mask.r))*.82);
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

