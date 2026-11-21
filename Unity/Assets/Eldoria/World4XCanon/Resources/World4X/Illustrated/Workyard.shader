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
  // Break up the yard before it reaches the rectangular mesh border. The
  // previous broad, dark oval read as a platform beneath the illustrated
  // resource at phone scale even though its corners were transparent.
  float uneven=.09*sin(q.x*8+q.y*11)+.065*sin(q.x*11-q.y*4)+
   .035*sin(q.x*23+q.y*17);
  half edge=1-smoothstep(.34,.88,length(q)+uneven);
  half shadow=1-smoothstep(.06,.76,length((q-float2(.12,.07))*float2(1,1.48)));
  half3 soil=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv*1.65).rgb;
  // Keep a readable worked-earth centre but let the retained meadow remain
  // the dominant material around the silhouette contact.
  return half4(soil*lerp(1,.72,shadow),edge*(.24+.22*shadow));
 }
 ENDHLSL
 } }
}
