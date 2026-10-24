Shader "Eldoria/RiberaWater" {
 Properties { _BaseMap("Pigment",2D)="white"{} _BaseColor("Water tint",Color)=(.22,.51,.57,.25) }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
 Pass { Name "ForwardLit" Tags {"LightMode"="UniversalForward"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor;float4 _BaseMap_ST;
 CBUFFER_END
 struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
 struct V{float4 p:SV_POSITION;float3 normal:TEXCOORD0;float2 uv:TEXCOORD1;float3 world:TEXCOORD2;};
 V vert(A a){V o;o.world=TransformObjectToWorld(a.positionOS.xyz);o.p=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(a.normalOS);o.uv=TRANSFORM_TEX(a.uv,_BaseMap);return o;}
 half4 frag(V i):SV_Target{
 half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv)*_BaseColor;
 Light l=GetMainLight(TransformWorldToShadowCoord(i.world));float3 n=normalize(i.normal);float3 v=GetWorldSpaceNormalizeViewDir(i.world);
 float fresnel=.04+.12*pow(1-saturate(dot(n,v)),4);
 c.rgb*=.65+.35*l.shadowAttenuation;
 c.rgb+=half3(.23,.30,.32)*fresnel;
 return half4(c.rgb,saturate(c.a+fresnel));
 }
 ENDHLSL
 }
 }
}
