Shader "Eldoria/RiberaFade" {
 Properties { _BaseMap("Pigment",2D)="white"{} _BaseColor("Colour",Color)=(1,1,1,1) _Fade("Far weight",Range(0,1))=0 _Baked("Baked lighting",Float)=0 _Foliage("Painted foliage response",Float)=0 }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"}
 HLSLINCLUDE
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor;float4 _BaseMap_ST;float _Fade;float _Baked;float _Foliage;
 CBUFFER_END
 struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
 struct V{float4 p:SV_POSITION;float3 normal:TEXCOORD0;float2 uv:TEXCOORD1;float3 world:TEXCOORD2;};
 V vert(A a){V o;o.world=TransformObjectToWorld(a.positionOS.xyz);o.p=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normalOS);o.uv=TRANSFORM_TEX(a.uv,_BaseMap);return o;}
 ENDHLSL
 Pass { Name "ForwardLit" Tags {"LightMode"="UniversalForward"} Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 half4 frag(V i):SV_Target {
 half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv)*_BaseColor;clip(c.a-.35);
 float d=frac(52.9829189*frac(dot(floor(i.p.xy),float2(.06711056,.00583715))));clip(_Baked>.5?_Fade-d:d-_Fade);
 Light l=GetMainLight(TransformWorldToShadowCoord(i.world));
 float diffuse=saturate(dot(normalize(i.normal),l.direction));
 if(_Baked<.5)c.rgb*=(half3(.30,.34,.33)+half3(.70,.66,.57)*diffuse*l.shadowAttenuation);
 if(_Baked<.5 && _Foliage>.5)c.rgb=lerp(half3(.025,.11,.075),half3(.26,.36,.075),smoothstep(.1,.9,diffuse))*lerp(.65,1,l.shadowAttenuation)*( .88+.12*saturate(c.g/.2));
 return half4(c.rgb,1);
 }
 ENDHLSL
 }
 Pass { Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} ZWrite On ZTest LEqual ColorMask 0 Cull Off
 HLSLPROGRAM
 #pragma vertex shadowVert
 #pragma fragment shadowFrag
 float3 _LightDirection;
 V shadowVert(A a){V o=vert(a);o.p=TransformWorldToHClip(ApplyShadowBias(o.world,o.normal,_LightDirection));
 #if UNITY_REVERSED_Z
 o.p.z=min(o.p.z,UNITY_NEAR_CLIP_VALUE);
 #else
 o.p.z=max(o.p.z,UNITY_NEAR_CLIP_VALUE);
 #endif
 return o;}
 half4 shadowFrag(V i):SV_Target {clip(.5-_Baked);clip(SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv).a*_BaseColor.a-.35);return 0;}
 ENDHLSL
 }
 }
}
