Shader "Eldoria/ConsolidationTrial/Breach" {
 Properties { _BaseMap("Pigment",2D)="white"{} _BaseColor("Colour",Color)=(1,1,1,1) _Fade("Far weight",Range(0,1))=0 _Baked("Baked lighting",Float)=0 _BoundaryWeight("Peripheral atmosphere",Range(0,1))=0 _BoundaryColour("Peripheral linear colour",Vector)=(0,0,0,1) _RegionOrigin("Region origin",Vector)=(0,0,0,0) [PerRendererData] _InstanceTint("Instance tint",Color)=(1,1,1,1) _GroundDetail("AF derived grass grain",2D)="gray"{} _GroundDetailStrength("Grass grain weight",Float)=0 _MineralEmission("Central mineral emission",Float)=0 _MeadowFinish("Warm meadow pigment finish",Float)=0 _WaterSurface("Two sided water lighting",Float)=0 _GeologicalRock("Physical stratification",Float)=0 _QuarryFloor("Grounded extraction edge",Float)=0 _MeadowBrush("Painted grass and earth",2D)="gray"{} _MeadowBrushEnabled("Painted brush weight",Float)=0 _BasinPigment("Same authored basin pigment",2D)="white"{} _BasinPigmentEnabled("Continuous geological pigment",Float)=0 _MeadowSourceMean("Measured source pigment mean",Vector)=(.5,.5,.25,1) _MeadowBrushMean("Neutral pigment mean",Vector)=(.51184069,.47712201,.18659264,1) _ExistingPathDistance("Existing authored earth distance",2D)="white"{} _ExistingPathEnabled("Path distance readability",Float)=0 }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"}
 HLSLINCLUDE
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
 TEXTURE2D(_GroundDetail); SAMPLER(sampler_GroundDetail);
 TEXTURE2D(_MeadowBrush); SAMPLER(sampler_MeadowBrush);
 TEXTURE2D(_BasinPigment); SAMPLER(sampler_BasinPigment);
 TEXTURE2D(_ExistingPathDistance); SAMPLER(sampler_ExistingPathDistance);
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor;float4 _BaseMap_ST;float4 _RegionOrigin;float4 _BoundaryColour;float _Fade;float _Baked;float _BoundaryWeight;float _GroundDetailStrength;float _MineralEmission;float _MeadowFinish;float _WaterSurface;float _GeologicalRock;float _QuarryFloor;float _MeadowBrushEnabled;float4 _MeadowBrushMean;float4 _MeadowSourceMean;float _ExistingPathEnabled;float _BasinPigmentEnabled;
 CBUFFER_END
 UNITY_INSTANCING_BUFFER_START(PerTree)
 UNITY_DEFINE_INSTANCED_PROP(float4, _InstanceTint)
 UNITY_INSTANCING_BUFFER_END(PerTree)
 struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V{float4 p:SV_POSITION;float3 normal:TEXCOORD0;float2 uv:TEXCOORD1;float3 world:TEXCOORD2;UNITY_VERTEX_INPUT_INSTANCE_ID};
 V vert(A a){V o;UNITY_SETUP_INSTANCE_ID(a);UNITY_TRANSFER_INSTANCE_ID(a,o);o.world=TransformObjectToWorld(a.positionOS.xyz);o.p=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normalOS);o.uv=TRANSFORM_TEX(a.uv,_BaseMap);return o;}
 ENDHLSL
 Pass { Name "ForwardLit" Tags {"LightMode"="UniversalForward"} Cull Off Blend SrcAlpha OneMinusSrcAlpha
 HLSLPROGRAM
 #pragma vertex vert
 #pragma multi_compile_instancing
 #pragma fragment frag
 #pragma multi_compile _ LOD_FADE_CROSSFADE
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
 #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
 half4 frag(V i):SV_Target {
 UNITY_SETUP_INSTANCE_ID(i);
 #ifdef LOD_FADE_CROSSFADE
 float lodDither=frac(52.9829189*frac(dot(floor(i.p.xy),float2(.06711056,.00583715))));
 clip(unity_LODFade.x-(unity_LODFade.x<0?-lodDither:lodDither));
 #endif
 half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv)*_BaseColor*UNITY_ACCESS_INSTANCED_PROP(PerTree,_InstanceTint);clip(c.a-.35);
 float d=frac(52.9829189*frac(dot(floor(i.p.xy),float2(.06711056,.00583715))));clip(_Baked>.5?_Fade-d:d-_Fade);
 #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
 float4 shadowCoord=ComputeScreenPos(TransformWorldToHClip(i.world));
 #else
 float4 shadowCoord=TransformWorldToShadowCoord(i.world);
 #endif
 if(_QuarryFloor>.5){
     float2 q=(i.uv-.5)*2;
     float edge=length(q)+.045*sin(q.x*19+q.y*11)+.03*sin(q.y*27-q.x*9);
     clip(1-smoothstep(.83,1.03,edge)-d);
 }
 Light l=GetMainLight(shadowCoord);
 // The AG sector camera is within 300 m; continental cameras are not.
 // Respect URP shadow distance instead of sampling beyond its cascade coverage.
 float shadow=lerp(l.shadowAttenuation,1,GetMainLightShadowFade(i.world));
 float ndl=dot(normalize(i.normal),l.direction);
 float diffuse=_WaterSurface>.5?abs(ndl):saturate(ndl);
 // Geological pigment is continuous across material slots, on the same
 // physical source surface. Unity imported UVs use Blender's original V.
 float2 groundPosition=i.world.xz-_RegionOrigin.xz;
 float2 fault=groundPosition-float2(-260,150);
 float radial=length(fault);
 float angle=atan2(fault.y,fault.x);
 float basinRadius=radial/(1+.15*sin(angle*3+.8)+.10*cos(angle*5-.4))*lerp(1,1.12,smoothstep(45,130,radial));
 if(_BasinPigmentEnabled>.5&&_MeadowFinish>.5&&basinRadius<130){
     float2 basinUV=(groundPosition-float2(-430,15))/float2(320,275);
     half3 basin=SAMPLE_TEXTURE2D(_BasinPigment,sampler_BasinPigment,basinUV).rgb;
     c.rgb=lerp(c.rgb,basin,1-smoothstep(115,130,basinRadius));
 }
 half3 sourcePigment=c.rgb;
 float2 detailUV=(i.world.xz-_RegionOrigin.xz)/float2(21.1875,12.4921875);
 float2 rotatedDetail=float2(detailUV.x*.8-detailUV.y*.6,detailUV.x*.6+detailUV.y*.8)+float2(.37,.61);
 half4 detailA=SAMPLE_TEXTURE2D(_GroundDetail,sampler_GroundDetail,detailUV);
 half4 detailB=SAMPLE_TEXTURE2D(_GroundDetail,sampler_GroundDetail,rotatedDetail);
 half3 grain=detailA.rgb+detailB.rgb;
 // Alpha carries the AF-derived 2m band, RGB the unchanged close micro-grain.
 // Keep the regional signal instead of asking sub-pixel detail to survive mipmapping.
 float footprint=max(length(ddx(detailUV)),length(ddy(detailUV)));
 half meso=detailA.a+detailB.a;
 grain=lerp(grain,half3(meso,meso,meso),smoothstep(.015,.07,footprint));
 // The same AF grain must survive regional zoom beside the retained core.
 // The existing mip chain/aniso filter performs minification; an additional
 // early derivative fade erased detail only on the extension and exposed a square.
 c.rgb*=lerp(half3(1,1,1),grain,_GroundDetailStrength);
 // Preserve the source's close grass grain and paths. Compress the broad
 // colour contrast with pixel footprint so overview never becomes camouflage.
 if(_MeadowFinish>.5){
     float2 q=i.world.xz-_RegionOrigin.xz;
     float metresPerPixel=max(length(ddx(i.world.xz)),length(ddy(i.world.xz)));
     // The four actual source textures have different colour/value means.
     // Normalise classification; display one common grass/detail field.
     half3 relative=sourcePigment/max(_MeadowSourceMean.rgb,half3(.01,.01,.01));
     half grass=1-smoothstep(1.04,1.20,relative.r/max(.001,relative.g));
     half3 palette=half3(.60,.64,.28);
     #ifndef UNITY_COLORSPACE_GAMMA
     palette=SRGBToLinear(palette);
     #endif
     float groundWeight=grass;
     if(_BasinPigmentEnabled>.5)groundWeight*=smoothstep(55,125,basinRadius);
     half3 unified=relative*palette;
     if(_MeadowBrushEnabled>.5){
         float2 brushUV=q/16;
         half3 brushA=SAMPLE_TEXTURE2D(_MeadowBrush,sampler_MeadowBrush,brushUV).rgb;
         half3 brushB=SAMPLE_TEXTURE2D(_MeadowBrush,sampler_MeadowBrush,float2(brushUV.x*.8-brushUV.y*.6,brushUV.x*.6+brushUV.y*.8)+float2(.37,.61)).rgb;
         half3 brush=lerp(brushA,brushB,.3)/max(_MeadowBrushMean.rgb,half3(.01,.01,.01));
         // One authored grass albedo AND one AF-derived neutral detail field.
         // Source-specific grain/value may classify paths, never draw a sector.
         half value=clamp(dot(grain,half3(.2126,.7152,.0722)),.92,1.08);
         unified=brush*palette*value;
     }
     c.rgb=lerp(c.rgb,unified,groundWeight);
     // Use one soft meadow light response. Keep real cast shadows and the
     // slope's highlights; dark macro bands must not make a second map.
     diffuse=lerp(diffuse,max(diffuse,saturate(l.direction.y)),groundWeight*.85);
     if(_ExistingPathEnabled>.5){
         float distanceMetres=SAMPLE_TEXTURE2D(_ExistingPathDistance,sampler_ExistingPathDistance,i.uv).r*16;
         float width=lerp(.35,3.3,smoothstep(.3,4.5,metresPerPixel));
         float readable=(1-smoothstep(width,width+1.8,distanceMetres))*smoothstep(.15,.7,metresPerPixel)*.32;
         half3 earth=half3(.70,.59,.34);
         #ifndef UNITY_COLORSPACE_GAMMA
         earth=SRGBToLinear(earth);
         #endif
         c.rgb=lerp(c.rgb,earth,readable);
     }
 }
 if(_GeologicalRock>.5||(_BasinPigmentEnabled>.5&&_MeadowFinish>.5)){
     // Flat sediment belongs to the dark interior; deposited layers describe
     // the sloping walls. This slope distinction keeps the wound below ground
     // instead of reading as a pale striped ribbon across the landscape.
     float wall=1-smoothstep(.48,.86,abs(normalize(i.normal).y));
     // Angular variation follows the newly cut facets; no sinusoidal contour lines.
     float2 facet=floor((i.world.xz-_RegionOrigin.xz)/3.7);
     float fleck=frac(sin(dot(facet,float2(12.9898,78.233)))*43758.5453);
     float wound=1-smoothstep(55,125,basinRadius);
     c.rgb*=lerp(1,lerp(.84,1.02,wall)*lerp(.91,1.08,fleck),wound);


 }
 half3 originalPigment=c.rgb;
 if(_Baked<.5)c.rgb*=(half3(.40,.43,.46)+half3(.60,.57,.54)*diffuse*shadow);
 #if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX) || USE_CLUSTER_LIGHT_LOOP
 if(_Baked<.5&&basinRadius<180){
     InputData inputData=(InputData)0;
     inputData.positionWS=i.world;inputData.normalWS=normalize(i.normal);
     inputData.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.world);
     inputData.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.p);
     uint pixelLightCount=GetAdditionalLightsCount();
     LIGHT_LOOP_BEGIN(pixelLightCount)
         Light interior=GetAdditionalLight(lightIndex,inputData.positionWS,half4(1,1,1,1));
         // Leave the established cool sky fill unchanged. Only the authored
         // mineral sources light the actual inward-facing geological walls.
         if(interior.color.b>interior.color.g*1.2&&interior.color.r>interior.color.g*1.2)
             c.rgb+=originalPigment*interior.color*interior.distanceAttenuation*interior.shadowAttenuation*saturate(dot(inputData.normalWS,interior.direction));
     LIGHT_LOOP_END
 }
 #endif
 // Only the existing exposed central mineral receives this material value.
 // This is emissive geology on the actual ground, never a portal or icon.
 c.rgb+=originalPigment*(_MineralEmission*smoothstep(.004,.04,originalPigment.b-originalPigment.g));
 if(_GeologicalRock>.5){
     float radius=basinRadius;
     float vein=smoothstep(1.8,2.8,originalPigment.b/max(.02,originalPigment.g))*smoothstep(.035,.12,originalPigment.b-originalPigment.g);
     vein*=1-smoothstep(18,75,radius);
     c.rgb+=half3(.30,.045,.48)*vein*.75;
     c.rgb+=half3(.65,.30,.85)*(1-smoothstep(1,5,radius));
 }
 float2 region=i.world.xz-_RegionOrigin.xz;
 // Minimal peripheral atmosphere at the region, hiding the neighbouring continuation.
 // Rounded, irregular peripheral falloff. Vary the boundary itself, not
 // merely the blur width of four perfectly straight map edges.
 float2 normalizedRegion=abs(region/float2(910,1800));
 float perimeter=pow(pow(normalizedRegion.x,4)+pow(normalizedRegion.y,4),.25);
 float irregular=.025*sin(region.y*.009)+.018*cos(region.x*.013-region.y*.006);
 // Keep haze only in the last continental rim: the earlier .88
 // start veiled too much real strategic terrain on a phone.
 float atmosphere=smoothstep(.925,1.055,perimeter+irregular)*_BoundaryWeight;
 return half4(lerp(c.rgb,_BoundaryColour.rgb,atmosphere),1);
 }
 ENDHLSL
 }
 Pass { Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} ZWrite On ZTest LEqual ColorMask 0 Cull Off
 HLSLPROGRAM
 #pragma vertex shadowVert
 #pragma multi_compile_instancing
 #pragma multi_compile _ LOD_FADE_CROSSFADE
 #pragma fragment shadowFrag
 float3 _LightDirection;
 V shadowVert(A a){V o=vert(a);o.p=TransformWorldToHClip(ApplyShadowBias(o.world,o.normal,_LightDirection));
 #if UNITY_REVERSED_Z
 o.p.z=min(o.p.z,o.p.w*UNITY_NEAR_CLIP_VALUE);
 #else
 o.p.z=max(o.p.z,o.p.w*UNITY_NEAR_CLIP_VALUE);
 #endif
 return o;}
 half4 shadowFrag(V i):SV_Target {
 UNITY_SETUP_INSTANCE_ID(i);
 #ifdef LOD_FADE_CROSSFADE
 float lodDither=frac(52.9829189*frac(dot(floor(i.p.xy),float2(.06711056,.00583715))));
 clip(unity_LODFade.x-(unity_LODFade.x<0?-lodDither:lodDither));
 #endif
 float detailDither=frac(52.9829189*frac(dot(floor(i.p.xy),float2(.06711056,.00583715))));clip(detailDither-_Fade);
 clip(.5-_Baked);clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).a*_BaseColor.a-.35);return 0;}
 ENDHLSL
 }
 }
}

