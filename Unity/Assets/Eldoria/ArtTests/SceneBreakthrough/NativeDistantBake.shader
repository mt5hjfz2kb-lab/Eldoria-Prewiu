Shader "Eldoria/Breakthrough/NativeDistantBake" {
 Properties {_BaseMap("Native 3D distant environment bake",2D)="black"{}}
 SubShader {Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Background" "Queue"="Background"}
 Pass {Name "DistantEnvironment" Tags{"LightMode"="UniversalForward"} ZWrite Off ZTest Always Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct Input {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
 struct Output {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 Output vert(Input i){Output o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;return o;}
 half4 frag(Output i):SV_Target {half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(c.a-.02);return half4(c.rgb,1);}
 ENDHLSL
 }
 }
}