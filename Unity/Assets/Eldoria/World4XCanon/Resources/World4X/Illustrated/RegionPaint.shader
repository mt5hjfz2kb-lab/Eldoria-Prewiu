Shader "Eldoria/Region1/Illustrated ground" {
 Properties {
  _FaultPigment("Painted fault arms registered to the relief",2D)="black"{}
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
 #pragma target 3.0
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_FaultPigment); SAMPLER(sampler_FaultPigment);
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
 Out vert(In v){
  Out o;
  float2 metres=(v.uv-.5)*float2(1820,3600);
  float2 faultUV=(metres-float2(-416,0))/float2(320,288);
  if(all(faultUV>0) && all(faultUV<1)){
   half4 rock=SAMPLE_TEXTURE2D_LOD(_FaultPigment,sampler_FaultPigment,faultUV,2);
   half core=smoothstep(.002,.045,min(rock.b-rock.g,rock.r-rock.g));
   half shelf=smoothstep(.10,.43,dot(rock.rgb,half3(.24,.56,.20)));
   v.p.y+=rock.a*(shelf*2.7-core*9.5);
  }
  o.p=TransformObjectToHClip(v.p); o.uv=v.uv;
  o.world=TransformObjectToWorld(v.p);return o;
 }
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
  half3 painted=borderMist(metres);
  // Broad banks of mist cross the surveyed edge in world coordinates.
  // The atmosphere mesh and the terrain evaluate the same field at the join.
  float wind=sin(metres.x*.0034+sin(metres.y*.0041))*cos(metres.y*.0027);
  float distanceToMap=max(abs(metres.x)-910,abs(metres.y)-1800);
  half cloud=(1-smoothstep(360,1250,distanceToMap))*(.32+.17*wind);
  // Let the existing pigments recede into mist along elongated ridgelines.
  // Both the edge mesh and its backing sample the same world coordinates.
  float2 ridgeUV=float2(metres.x*.00088+metres.y*.00032,metres.y*.00145);
  half strata=SAMPLE_TEXTURE2D(_Slate,sampler_Slate,ridgeUV).g;
  half shoulder=smoothstep(.15,.83,.49+wind*.27+.20*sin(metres.y*.005+strata*5));
  half veil=(1-smoothstep(160,1050,distanceToMap))*shoulder;
  half3 ridge=lerp(painted,painted*half3(.60,.82,.74),.78);
  half3 overview=lerp(lerp(_BoundaryColour.rgb,painted,cloud),ridge,veil*.56);
  return lerp(painted,overview,saturate(_BoundaryWeight));
 }
 half4 frag(Out i):SV_Target {
  float2 metre=(i.uv-.5)*float2(1820,3600);
  float beyondDistance=max(0,max(abs(metre.x)-910,abs(metre.y)-1800));
  i.uv=saturate(i.uv);
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
  // Let the original painted meadow resolve into broad strokes as its
  // fine tile falls below one screen pixel. Rotate the larger sampling axes
  // to avoid rows of repeating speckles on long open stretches.
  half3 cityAtZoom=lerp(cityMeadow,farPainting,strategicScale*.67)*lerp(1,macroTint,strategicScale*.48);
  // Paint continuity from Bastion I to the near, regional and panoramic map.
  // This operates INSIDE the one regional terrain shader, not on new planes.
  half cityPaint=saturate(_CityFinish)*lerp(.84,.98,paintedScale);
  half3 grass=lerp(continentalGrass,cityAtZoom,cityPaint);
  // Canonical canopy coverage keeps woodland readable between distant crowns.
  half woodland=smoothstep(.12,.82,mask.a);
  grass*=lerp(half3(1,1,1),half3(.66,.83,.76),woodland*lerp(.60,.54,strategicScale));
  half3 earth=SAMPLE_TEXTURE2D(_Earth,sampler_Earth,terrainUV*.94).rgb;
  half3 slate=SAMPLE_TEXTURE2D(_Slate,sampler_Slate,terrainUV*.61).rgb;
  half3 water=SAMPLE_TEXTURE2D(_Water,sampler_Water,terrainUV*.81+float2(0,_Time.y*.012)).rgb;
  // City road pigment only replaces the road material already described by
  // the ORIGINAL mapped path mask. There are no raised photo decals.
  half4 cityDirt=SAMPLE_TEXTURE2D(_CityEarth,sampler_CityEarth,
                                   metre/6.5+float2(.17,.09));
  // The mapped lane supplies the road edge. Mirror the opaque interior of
  // the city strip so its two transparent shoulders never cross a lane.
  float2 pavingUV=metre/float2(5.7,11.4)+float2(.12,.24);
  pavingUV.x=.22+.56*abs(frac(pavingUV.x*.5)*2-1);
  half4 cityPaving=SAMPLE_TEXTURE2D(_CityCobble,sampler_CityCobble,pavingUV);
  half3 sharedEarth=lerp(earth,cityDirt.rgb,cityDirt.a*.75*_CityFinish);
  half cityNear=1-smoothstep(55,138,length(metre));
  half3 pathColour=lerp(sharedEarth,cityPaving.rgb,
                         cityNear*cityPaving.a*.65*_CityFinish);
  // The riverbanks come from the actual signed geographic shoreline.
  float shore=(features.a-.5)*12;
  float aa=clamp(fwidth(shore),.32,1.4);
  // Real near-camera captures showed an abrupt, hard teal cut against the
  // approved Valoria grass near the canonical bridge. Blend over the EXISTING
  // signed bank field (in metres), not by adding a river decal on the terrain.
  half wet=smoothstep(-2.4-aa,2.1+aa,shore);
  mask.r=lerp(mask.r,wet,local);
  half landBank=(1-wet)*local;
  half terrace=landBank*(1-smoothstep(.35,2.4,-shore));
  half fringe=landBank*(1-smoothstep(2.4,5.4,-shore));
  grass=lerp(grass,cityMeadow*half3(.81,.91,.76),fringe*.72);
  grass=lerp(grass,lerp(sharedEarth,slate,.58),terrace*.84);
  // At strategic distance, individual tree crowns no longer describe the land.
  // The existing geographic mask mip spans about 32 metres: water proximity,
  // woodland and the fault can give adjacent ground a continuous colour family.
  // Height is relative to the painted mesh origin, including translated worlds.
  half4 district=SAMPLE_TEXTURE2D_LOD(_Weights,sampler_Weights,i.uv,3);
  half mapScale=smoothstep(80,410,unity_OrthoParams.y*.5);
  half valley=smoothstep(.035,.48,district.r)*(1-mask.r);
  half canopy=smoothstep(.08,.65,district.a);
  float reliefHeight=i.world.y-GetObjectToWorldMatrix()._m13;
  half upland=smoothstep(18,76,reliefHeight);
  // Sunlit clearings and cooler drainage hollows follow the continuous land cover.
  half glade=(1-canopy)*(1-valley)*(1-upland);
  half field=saturate(.5+.32*sin(metre.x*.014+sin(metre.y*.018))+.18*cos(metre.y*.031-metre.x*.009));
  half3 openMeadow=grass*lerp(half3(.98,1.07,.98),half3(1.20,1.09,.79),field*glade);
  half3 woodedMeadow=grass*half3(.83,.94,.88);
  half3 districtGrass=lerp(openMeadow,woodedMeadow,canopy);
  districtGrass=lerp(districtGrass,grass*half3(.76,1.02,1.11),valley*.82);
  districtGrass=lerp(districtGrass,lerp(grass,slate,.56),upland*.88);
  // Shade glades, forest litter and exposed shelves at the close scale too.
  half litter=canopy*(.38+.23*saturate(dot(meadowRange,half3(.4,.5,.1))*2));
  districtGrass=lerp(districtGrass,lerp(sharedEarth,grass,.48)*half3(.72,.86,.66),litter*.78);
  grass=lerp(grass,districtGrass,.72+.28*mapScale);
  half faultWeight=saturate(mask.b);
  half3 fracturedSlate=slate*lerp(half3(1,1,1),half3(.84,.72,.96),
                                  smoothstep(.035,.45,district.b)*mapScale);
  half3 colour=lerp(grass,fracturedSlate,faultWeight*.87);
  // Sample the geological opening on the terrain triangles themselves.
  // A separately tessellated patch clipped into the steep fault walls in
  // near-camera captures even though its regional silhouette looked right.
  float2 faultUV=(metre-float2(-416,0))/float2(320,288);
  if(all(faultUV>=0) && all(faultUV<=1)){
   half4 fault=SAMPLE_TEXTURE2D(_FaultPigment,sampler_FaultPigment,faultUV);
   // Coarse pigment coverage describes the apron around each actual rock arm.
   // Grade the neighbouring soil continuously before shading the exposed walls.
   half talus=SAMPLE_TEXTURE2D_LOD(_FaultPigment,sampler_FaultPigment,faultUV,5).a;
   half shattered=smoothstep(.018,.39,talus)*(1-fault.a);
   half3 weathered=lerp(sharedEarth,slate,.66)*half3(.83,.78,.85);
   colour=lerp(colour,weathered,shattered*.79);
   half glow=smoothstep(.021,.43,min(fault.b-fault.g,fault.r-fault.g));
   // Pigment isolates the interior seam. Preserve the illuminated rock lip
   // while violet reflected light reaches the near wall of the opening.
   half violetExcess=max(0,min(fault.b-fault.g,fault.r-fault.g));
   half bounce=smoothstep(.002,.026,violetExcess);
   half pulse=.88+.12*sin(_Time.y*.71+faultUV.x*11-faultUV.y*5);
   fault.rgb=fault.rgb*lerp(half3(1,1,1),half3(.86,.81,1.08),bounce)
          +half3(.34,.069,.53)*glow*pulse+half3(.032,.009,.054)*bounce;
   half rockValue=dot(fault.rgb,half3(.29,.57,.14));
   half seam=smoothstep(.03,.33,violetExcess);
   fault.rgb*=lerp(.83,1.10,smoothstep(.018,.28,rockValue));
   fault.rgb+=half3(.065,.012,.12)*seam*pulse;
   colour=lerp(colour,fault.rgb,fault.a);
  }
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
  half3 longCurrent=SAMPLE_TEXTURE2D(_Water,sampler_Water,
      metre/72.0+float2(_Time.y*.0014,-_Time.y*.002)).rgb;
  half3 currentPaint=lerp(water,longCurrent,mapScale*.72);
  #ifndef UNITY_COLORSPACE_GAMMA
  currentPaint=LinearToSRGB(currentPaint);
  #endif
  half pigment=saturate((dot(currentPaint,half3(.24,.56,.20))-.16)*4.0);
  // The geographic mask and its existing broad mip also describe shallows
  // outside the near survey. Work in display pigment before converting once.
  half shoal=saturate(.5+.32*sin(metre.x*.085+metre.y*.062)+.18*(pigment-.5));
  half shelf=lerp(1-smoothstep(.17,.79,district.r),1-smoothstep(.1,3.8+shoal*2.4,shore),local);
  half3 channel=half3(.167,.276,.266)*(.77+pigment*.30+shoal*.15);
  half3 shallows=half3(.38,.435,.346)*(.85+pigment*.32);
  half3 riverColour=lerp(channel,shallows,shelf*.88);
  half glint=smoothstep(.36,.85,pigment)*lerp(.42,1,close);
  riverColour+=half3(.11,.12,.115)*glint;
  #ifndef UNITY_COLORSPACE_GAMMA
  riverColour=SRGBToLinear(riverColour);
  #endif
  half bank=(1-smoothstep(.15,1.35,abs(shore)))*local*close;
  colour=lerp(colour,lerp(earth,slate,.55),bank*.68);
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
  // Horizontal meadow must retain exactly the city paint's apparent value.
  // Reserve the change for geographic slopes, not a global brightness boost.
  half paintedLight=clamp(1+(geomLight-.85)*.54,.72,1.10);
  colour*=lerp(1,paintedLight,(1-saturate(mask.r))*.82);
  colour*=1+.045*sin(metre.x*.018+sin(metre.y*.027))*cos(metre.y*.014);
  if (_BeyondMap>.5) {
   half shoulder=saturate((1-geometricNormal.y)*4.7);
   half grain=SAMPLE_TEXTURE2D(_Grass,sampler_Grass,metre/310).g;
   half3 heath=lerp(grass*half3(.66,.83,.76),sharedEarth, saturate(.13+grain*.59));
   half3 ridge=lerp(heath,slate,shoulder*.86)*(.70+.38*geomLight);
   half coastalWater=smoothstep(.64,.97,mask.r)*smoothstep(600,910,metre.x);
   ridge=lerp(ridge,riverColour,coastalWater);
   colour=lerp(colour,ridge,smoothstep(0,185,beyondDistance));
   colour=lerp(colour,peripheralMist(metre),smoothstep(320,2700,beyondDistance)*.78);
  }
  return half4(colour,1);
 }
 ENDHLSL
 } }
}

