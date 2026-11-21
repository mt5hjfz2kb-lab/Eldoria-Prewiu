Shader "Eldoria/Region1/Workyard" {
 Properties { _MainTex("Local earth",2D)="white"{} }
 SubShader {Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent-20" "RenderType"="Transparent"}
 Pass {ZWrite Off Cull Off Blend SrcAlpha OneMinusSrcAlpha Offset -1,-1
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 V vert(float3 p:POSITION,float2 uv:TEXCOORD0){V o;o.p=TransformObjectToHClip(p);o.uv=uv;return o;}
 half4 frag(V i):SV_Target{
  float2 q=(i.uv-.5)*2;
  float uneven=.055*sin(q.x*8+q.y*11)+.04*sin(q.x*11-q.y*4);
  half edge=1-smoothstep(.43,1,length(q)+uneven);
  half shadow=1-smoothstep(.06,.76,length((q-float2(.12,.07))*float2(1,1.48)));
  half3 soil=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv*1.65).rgb;
  return half4(soil*lerp(1,.5,shadow),edge*(.47+.36*shadow));
 }
 ENDHLSL
 } }
}
