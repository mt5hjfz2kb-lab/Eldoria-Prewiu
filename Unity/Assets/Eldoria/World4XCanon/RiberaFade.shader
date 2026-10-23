Shader "Eldoria/RiberaFade" {
 Properties { _BaseMap("Pigment",2D)="white"{} _BaseColor("Colour",Color)=(1,1,1,1) _Fade("Far weight",Range(0,1))=0 _Baked("Baked lighting",Float)=0 }
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest"}
 Pass { Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor; float4 _BaseMap_ST;float _Fade;float _Baked;
 CBUFFER_END
 struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;};
 struct V{float4 p:SV_POSITION;float3 normal:TEXCOORD0;float2 uv:TEXCOORD1;};
 V vert(A a){V o;o.p=TransformObjectToHClip(a.positionOS.xyz);o.normal=TransformObjectToWorldNormal(a.normalOS);o.uv=TRANSFORM_TEX(a.uv,_BaseMap);return o;}
 half4 frag(V i):SV_Target {half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv)*_BaseColor;clip(c.a-.35);float d=frac(52.9829189*frac(dot(floor(i.p.xy),float2(.06711056,.00583715))));clip(_Baked>.5?_Fade-d:d-_Fade);Light l=GetMainLight();float diffuse=saturate(dot(normalize(i.normal),l.direction));if(_Baked<.5)c.rgb*=SampleSH(normalize(i.normal))+.25+l.color*diffuse*.72;return half4(c.rgb,1);}
 ENDHLSL
 }
 }
}
