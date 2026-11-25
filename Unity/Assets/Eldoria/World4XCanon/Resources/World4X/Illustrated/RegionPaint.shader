Shader "Eldoria/Region1/Illustrated ground" {
 Properties {
  _FaultPigment("Painted fault arms registered to the relief",2D)="black"{}
  _CrownCover("Planted crown coverage",2D)="black"{}
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
 TEXTURE2D(_CrownCover); SAMPLER(sampler_CrownCover);
 TEXTURE2D(_NearFeatures); SAMPLER(sampler_NearFeatures);
 TEXTURE2D(_Region); SAMPLER(sampler_Region); TEXTURE2D(_Weights); SAMPLER(sampler_Weights);
 TEXTURE2D(_Grass); SAMPLER(sampler_Grass); TEXTURE2D(_Earth); SAMPLER(sampler_Earth);
 TEXTURE2D(_Water); SAMPLER(sampler_Water); TEXTURE2D(_Slate); SAMPLER(sampler_Slate);
 TEXTURE2D(_CityMeadow); SAMPLER(sampler_CityMeadow);
 TEXTURE2D(_CityEarth); SAMPLER(sampler_CityEarth);
 TEXTURE2D(_CityCobble); SAMPLER(sampler_CityCobble);
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
 struct In {float3 p:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
 struct Out {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;half3 slope:TEXCOORD2;};
 Out vert(In v){
  Out o;
  float2 metres=(v.uv-.5)*float2(1820,3600);
  float2 faultUV=(metres-float2(-416,0))/float2(320,288);
  if(all(faultUV>0) && all(faultUV<1)){
   half4 rock=SAMPLE_TEXTURE2D_LOD(_FaultPigment,sampler_FaultPigment,faultUV,2);
   half core=smoothstep(.002,.045,min(rock.b-rock.g,rock.r-rock.g));
   half shelf=smoothstep(.10,.43,dot(rock.rgb,half3(.24,.56,.20)));
   v.p.y+=rock.a*(shelf*2.65-core*5.7);
  }
  o.p=TransformObjectToHClip(v.p); o.uv=v.uv;
  o.slope=TransformObjectToWorldNormal(v.normal);
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
  if (_BeyondMap>.5) {
   // Carry only vegetation and geology through the chart edge. Curving
   // contours prevent the edge texels from stretching into straight bands.
   float2 fold=1-abs(frac(i.uv*.5)*2-1);
   float swing=smoothstep(0,360,beyondDistance);
   float2 bend=float2(sin(metre.y*.0027+metre.x*.0012),cos(metre.x*.0031+metre.y*.0014));
   i.uv=saturate(fold+bend*swing*float2(85.0/1820,115.0/3600));
  }
  // This is the only visible regional ground surface. The old imported
  // colour-map is NOT drawn as a background image beneath another landscape.
  // Canonical geography supplies masks and height, while the very same
  // Valoria paint sources used at Bastion I supply the actual meadow/road.
  half3 geographicLight=SAMPLE_TEXTURE2D(_Region,sampler_Region,i.uv).rgb;
  half4 mask=SAMPLE_TEXTURE2D(_Weights,sampler_Weights,i.uv);
  half coast=smoothstep(.70,1.0,mask.r)*smoothstep(545,925,metre.x);
  if (_BeyondMap>.5) {
   mask.g*=1-smoothstep(0,190,beyondDistance);
   // The same water mask continues across the outermost mesh vertices.
  }
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
  // Silt accumulates on the broad inside bends and breaks up the waterline
  // at grass-tuft scale. The signed river field remains the channel centre.
  float bankBend=sin(metre.y*.069+sin(metre.x*.041))*.86+sin(metre.x*.151+metre.y*.092)*.39;
  // Narrow stony runs alternate with broader pools along the same channel.
  // Keep the crossing aligned with its mapped road and retain the centreline.
  float bankRun=sin(metre.x*.065+metre.y*.043+1.57*sin(metre.y*.029));
  float pool=bankRun*(1.72+.41*cos(metre.x*.021));
  float bridgeClearance=1-smoothstep(.03,.43,features.g);
  shore+=(bankBend*.55+pool*bridgeClearance)*(1-smoothstep(3.0,5.9,abs(shore)));
  float aa=clamp(fwidth(shore),.32,1.4);
  // Real near-camera captures showed an abrupt, hard teal cut against the
  // approved Valoria grass near the canonical bridge. Blend over the EXISTING
  // signed bank field (in metres), not by adding a river decal on the terrain.
  half wet=smoothstep(-2.4-aa,2.1+aa,shore);
  mask.r=lerp(mask.r,wet,local);
  half landBank=(1-wet)*local;
  half sediment=.5+.5*sin(metre.y*.12+sin(metre.x*.23));
  half terrace=landBank*(1-smoothstep(.35,2.1+sediment,-shore));
  half fringe=landBank*(1-smoothstep(2.4,5.4,-shore));
  grass=lerp(grass,cityMeadow*half3(.81,.91,.76),fringe*.72);
  grass=lerp(grass,lerp(sharedEarth,slate,.34+.34*sediment),terrace*.89);
  // At strategic distance, individual tree crowns no longer describe the land.
  // The existing geographic mask mip spans about 32 metres: water proximity,
  // woodland and the fault can give adjacent ground a continuous colour family.
  // Height is relative to the painted mesh origin, including translated worlds.
  half4 district=SAMPLE_TEXTURE2D_LOD(_Weights,sampler_Weights,i.uv,3);
  half mapScale=smoothstep(80,410,unity_OrthoParams.y*.5);
  half valley=smoothstep(.035,.48,district.r)*(1-mask.r);
  float2 standBend=float2(sin(metre.y*.032+sin(metre.x*.019)),cos(metre.x*.028+metre.y*.011));
  half forestCover=SAMPLE_TEXTURE2D_LOD(_Weights,sampler_Weights,saturate(i.uv+standBend*float2(.009,.004)),2).a;
  // Feather the pigment beyond the stand with cover sampled across the
  // slope. Individual trees never acquire an isolated circular ground stamp.
  half outwardCover=SAMPLE_TEXTURE2D_LOD(_Weights,sampler_Weights,
      saturate(i.uv-standBend*float2(.013,.007)),3).a;
  half canopy=smoothstep(.05,.76,district.a*.24+forestCover*.46+outwardCover*.30);
  float reliefHeight=i.world.y-GetObjectToWorldMatrix()._m13;
  half upland=smoothstep(18,76,reliefHeight);
  // Sunlit clearings and cooler drainage hollows follow the continuous land cover.
  half glade=(1-canopy)*(1-valley)*(1-upland);
  half field=saturate(.5+.32*sin(metre.x*.014+sin(metre.y*.018))+.18*cos(metre.y*.031-metre.x*.009));
  half3 openMeadow=grass*lerp(half3(.93,1.11,1.05),half3(1.28,1.11,.80),field*glade);
  // Merge painted crowns into irregular stands at map scale. Fine ridges
  // follow the stand instead of lining up with the field or the atlas border.
  half crowns=SAMPLE_TEXTURE2D(_Grass,sampler_Grass,(metre+standBend*9)/float2(37,29)).g;
  half clearEdge=smoothstep(.22,.62,canopy+(crowns-.23)*.50);
  half3 woodedMeadow=lerp(sharedEarth*half3(.70,.75,.51),grass*half3(.43,.75,.67),
                           .42+.55*clearEdge);
  woodedMeadow*=lerp(.90,1.13,saturate(crowns*2.8));
  half3 districtGrass=lerp(openMeadow,woodedMeadow,canopy);
  districtGrass=lerp(districtGrass,grass*half3(.76,1.02,1.11),valley*.82);
  districtGrass=lerp(districtGrass,lerp(grass,slate,.68),upland*.94);
  // Shade glades, forest litter and exposed shelves at the close scale too.
  half litter=canopy*(.38+.23*saturate(dot(meadowRange,half3(.4,.5,.1))*2));
  districtGrass=lerp(districtGrass,lerp(sharedEarth,grass,.48)*half3(.72,.86,.66),litter*.78);
  grass=lerp(grass,districtGrass,.72+.28*mapScale);
  half faultWeight=saturate(mask.b);
  half3 fracturedSlate=slate*lerp(half3(1,1,1),half3(.84,.72,.96),
                                  smoothstep(.035,.45,district.b)*mapScale);
  // Forest litter extends into clearings. Break its edge with regional
  // slope and soil variation already sampled by this pass.
  half woodEdge=smoothstep(.10,.82,canopy+standBend.x*.14);
  half3 forestFloor=lerp(sharedEarth*half3(.81,.76,.62),grass*half3(.77,.89,.73),.52);
  grass=lerp(grass,forestFloor,woodEdge*(.44+.18*close));
  half shoulderStone=smoothstep(.36,.88,upland)*(1-woodEdge);
  grass=lerp(grass,lerp(slate,grass,.37),shoulderStone*.55);
  // Read a stand-sized footprint so a solitary tree cannot stamp a circle
  // onto the grass. The existing meadow pigment still controls fine detail.
  half standDensity=SAMPLE_TEXTURE2D_LOD(_CrownCover,sampler_CrownCover,i.uv,3).r;
  half innerStand=smoothstep(.15,.80,standDensity)*(.65+.35*canopy);
  half3 woodlandSoil=lerp(grass,sharedEarth*half3(.85,.91,.77),.48);
  grass=lerp(grass,woodlandSoil,innerStand*.42);
  // Straw-coloured high grasses share the original brush grain. Combine
  // a broad site exposure with the existing hill and drainage measurements.
  half pastureGrain=dot(meadowRange-meadowGlade,half3(.24,.57,.19));
  half exposure=saturate(.47+.17*sin(metre.x*.021+2.3*sin(metre.y*.015))
                   +.13*cos(metre.y*.031-metre.x*.012+standBend.x*1.35)
                   +.72*pastureGrain+.13*upland-.32*valley);
  half dryField=smoothstep(.34,.77,exposure)*(1-canopy*.82)*(1-mask.r);
  half3 dryPigment=lerp(grass*half3(1.20,1.04,.74),sharedEarth*half3(1.14,1.05,.79),.32);
  grass=lerp(grass,dryPigment,dryField*.79);
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
   half3 weathered=lerp(sharedEarth,slate,.66)*half3(.74,.67,.85);
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
   fault.rgb+=half3(.14,.022,.23)*seam*pulse;
   fault.rgb+=half3(.047,.006,.087)*smoothstep(.004,.045,violetExcess)*bounce;
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
  half3 channel=half3(.122,.267,.296)*(.77+pigment*.30+shoal*.15);
  half3 shallows=half3(.41,.49,.397)*(.85+pigment*.32);
  half3 riverColour=lerp(channel,shallows,shelf*.79);
  half glint=smoothstep(.36,.85,pigment)*lerp(.42,1,close);
  riverColour+=half3(.064,.078,.073)*glint;
  #ifndef UNITY_COLORSPACE_GAMMA
  riverColour=SRGBToLinear(riverColour);
  #endif
  half bank=(1-smoothstep(.15,1.35,abs(shore)))*local*close;
  colour=lerp(colour,lerp(earth,slate,.55),bank*.68);
  half floodplain=(1-smoothstep(.35,3.8+shoal*1.95,abs(shore)))*local;
  half distantBank=smoothstep(.016,.22,district.r)*(1-smoothstep(.36,.86,district.r));
  half marshEdge=max(floodplain*(1-mask.r),distantBank*.74);
  colour=lerp(colour,lerp(sharedEarth,grass*half3(.69,.86,.61),.65),marshEdge*.82);
  colour=lerp(colour,lerp(sharedEarth,slate,.34),floodplain*(1-mask.g)*.43);
  colour=lerp(colour,riverColour,mask.r);
  // The geographic mesh is REAL height-based terrain. Shade that geometry
  // directly with a restrained painterly key matching Valoria's baked
  // upper-left light. The previous unlit sampled colour looked like a flat
  // carpet when the camera tilted or crossed the Breach's hills.
  // No legacy PBR mesh, light rig, decal or additional art layer is drawn.
  float3 geometricNormal=cross(ddx(i.world),ddy(i.world));
  geometricNormal*=rsqrt(max(dot(geometricNormal,geometricNormal),1.e-8));
  if(geometricNormal.y<0)geometricNormal=-geometricNormal;
  float3 smoothSlope=normalize(i.slope);
  if(smoothSlope.y<0)smoothSlope=-smoothSlope;
  float3 lightNormal=normalize(lerp(smoothSlope,geometricNormal,faultWeight*.65));
  half geomLight=saturate(dot(lightNormal,normalize(float3(-.38,.85,.36))));
  // Horizontal meadow must retain exactly the city paint's apparent value.
  // Reserve the change for geographic slopes, not a global brightness boost.
  half paintedLight=clamp(1+(geomLight-.85)*.86,.59,1.16);
  colour*=lerp(1,paintedLight,(1-saturate(mask.r))*.82);
  colour*=1+.045*sin(metre.x*.018+sin(metre.y*.027))*cos(metre.y*.014);
  if (_BeyondMap>.5) {
   half shoulder=saturate((1-geometricNormal.y)*4.7);
   half wooded=canopy*(.70+.30*cos(metre.y*.018+sin(metre.x*.013)));
   half3 heath=lerp(districtGrass,grass*half3(.52,.73,.60),wooded);
   half3 ridge=lerp(heath,slate,saturate(.12*upland+shoulder*.70))*(.87+.18*geomLight);
   ridge=lerp(ridge,riverColour,mask.r);
   colour=lerp(colour,ridge,smoothstep(190,1150,beyondDistance)*.72);
   colour=lerp(colour,peripheralMist(metre),smoothstep(1850,6400,beyondDistance)*.28);
  }
  return half4(colour,1);
 }
 ENDHLSL
 } }
}

