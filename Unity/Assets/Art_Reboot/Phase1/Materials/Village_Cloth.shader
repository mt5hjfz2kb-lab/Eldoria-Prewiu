Shader "Eldoria/ArtReboot/Cloth"
{
 Properties {_Color("Color",Color)=(.42,.12,.065,1) _Phase("Phase",Float)=1.2}
 SubShader {
 Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
 Pass {
 Tags {"LightMode"="SRPDefaultUnlit"}
 Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _Color;float _Phase;
 CBUFFER_END
 struct A {float4 p:POSITION;float2 uv:TEXCOORD0;};
 struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float wave:TEXCOORD1;};
 V vert(A i){V o;float w=sin(_Time.y*1.8+_Phase+i.uv.x*5+i.uv.y*2);i.p.z+=w*.16*i.uv.x;i.p.y+=sin(_Time.y*1.3+i.uv.x*7+_Phase)*.045*i.uv.x;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.wave=w;return o;}
 half4 frag(V i):SV_Target {half stripe=step(.86,i.uv.x)+step(i.uv.y,.07)+step(.93,i.uv.y);return half4(lerp(_Color.rgb*(.85+.15*i.wave),half3(.64,.42,.16),saturate(stripe)),1);}
 ENDHLSL
 }
 }
}
