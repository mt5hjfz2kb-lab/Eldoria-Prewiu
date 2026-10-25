Shader "Eldoria/RegionOneAuthoredSurface" {
 Properties { _BaseMap("Pigment",2D)="white"{} _BaseColor("Colour",Color)=(1,1,1,1) _Fade("Far weight",Range(0,1))=0 _Baked("Baked lighting",Float)=0 _BoundaryWeight("Peripheral atmosphere",Range(0,1))=0 _BoundaryColour("Peripheral linear colour",Vector)=(0,0,0,1) _RegionOrigin("Region origin",Vector)=(0,0,0,0) [PerRendererData] _InstanceTint("Instance tint",Color)=(1,1,1,1) _GroundDetail("AF derived grass grain",2D)="gray"{} _GroundDetailStrength("Grass grain weight",Float)=0 }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"}
 HLSLINCLUDE
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
 TEXTURE2D(_GroundDetail); SAMPLER(sampler_GroundDetail);
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor;float4 _BaseMap_ST;float4 _RegionOrigin;float4 _BoundaryColour;float _Fade;float _Baked;float _BoundaryWeight;float _GroundDetailStrength;
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
 Light l=GetMainLight(shadowCoord);
 // The AG sector camera is within 300 m; continental cameras are not.
 // Respect URP shadow distance instead of sampling beyond its cascade coverage.
 float shadow=lerp(l.shadowAttenuation,1,GetMainLightShadowFade(i.world));
 float diffuse=saturate(dot(normalize(i.normal),l.direction));
 float2 detailUV=(i.world.xz-_RegionOrigin.xz)/float2(21.1875,12.4921875);
 float2 rotatedDetail=float2(detailUV.x*.8-detailUV.y*.6,detailUV.x*.6+detailUV.y*.8)+float2(.37,.61);
 half3 grain=(SAMPLE_TEXTURE2D(_GroundDetail,sampler_GroundDetail,detailUV).rgb+SAMPLE_TEXTURE2D(_GroundDetail,sampler_GroundDetail,rotatedDetail).rgb);
 // The same AF grain must survive regional zoom beside the retained core.
 // The existing mip chain/aniso filter performs minification; an additional
 // early derivative fade erased detail only on the extension and exposed a square.
 c.rgb*=lerp(half3(1,1,1),grain,_GroundDetailStrength);
 if(_Baked<.5)c.rgb*=(half3(.30,.34,.33)+half3(.70,.66,.57)*diffuse*shadow);
 float2 region=i.world.xz-_RegionOrigin.xz;
 // Atmosphere at the actual neighbouring-ground edge, beyond the playable region.
 float west=1710+region.x, east=1710-region.x, north=2600-region.y, south=2600+region.y;
 float edge=min(min(west,east),min(north,south));
 float irregular=40*sin(region.y*.011)+20*cos(region.x*.017-region.y*.006);
 float atmosphere=(1-smoothstep(0,107+irregular,edge))*_BoundaryWeight;
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
 clip(.5-_Baked);clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).a*_BaseColor.a-.35);return 0;}
 ENDHLSL
 }
 }
}
