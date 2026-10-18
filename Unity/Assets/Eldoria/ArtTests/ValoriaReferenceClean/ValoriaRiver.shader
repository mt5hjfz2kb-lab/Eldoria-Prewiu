Shader "Eldoria/ReferenceClean/River" {
 Properties { [MainColor] _BaseColor("Deep blue",Color)=(.012,.17,.31,1) _ShallowColor("Shore blue",Color)=(.055,.48,.65,1) _WaveNormal("Authored flow normal and height",2D)="gray"{} _RiverReflection("Baked river environment",Cube)="black"{} _RiverReflection_HDR("Reflection HDR decode",Vector)=(1,1,0,0) _ReflectionStrength("Environment reflection",Range(0,1))=0 }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-50"}
  Pass {
   Tags {"LightMode"="UniversalForward"}
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
   TEXTURE2D(_WaveNormal);SAMPLER(sampler_WaveNormal);
   TEXTURECUBE(_RiverReflection);SAMPLER(sampler_RiverReflection);
   CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor; half4 _ShallowColor; half4 _RiverReflection_HDR; half _ReflectionStrength;
   CBUFFER_END
   struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
   struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float2 uv:TEXCOORD1;};
   V vert(A a){V o;o.world=TransformObjectToWorld(a.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.uv=a.uv;return o;}
   float hash21(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise21(float2 p){float2 q=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash21(q),hash21(q+float2(1,0)),f.x),lerp(hash21(q+float2(0,1)),hash21(q+1),f.x),f.y);}
   half4 frag(V i):SV_Target {
    float t=_Time.y*.6;float w=sin(i.world.x*1.5+i.world.z*2.2+t);float w2=sin(i.world.z*3.4-i.world.x*.7-t*.8);
    float edge=abs(i.uv.y*2-1);float2 flow=i.world.xz*float2(.22,.54)+float2(-t*.045,t*.012);float n0=noise21(flow),n1=noise21(flow*2.17),n2=noise21(flow*5.23);float n=n0+n1*.40+n2*.16;float rawDepth=SampleSceneDepth(GetNormalizedScreenSpaceUV(i.positionCS));float depth01=rawDepth;
    #if UNITY_REVERSED_Z
     depth01=1-rawDepth;
    #endif
    float sceneEye=lerp(_ProjectionParams.y,_ProjectionParams.z,depth01);if(unity_OrthoParams.w<.5)sceneEye=LinearEyeDepth(rawDepth,_ZBufferParams);
    float column=max(0,sceneEye+TransformWorldToView(i.world).z);float deep=smoothstep(.08,2.1,column);
    half3 c=lerp(_ShallowColor.rgb,_BaseColor.rgb,deep);c*=.88+n*.17;
    float2 distortion=(float2(n0,n1)-.5)*.75;float2 wuv=i.world.xz*.17+distortion;half4 wa=SAMPLE_TEXTURE2D(_WaveNormal,sampler_WaveNormal,wuv+float2(-t*.025,t*.006));half4 wb=SAMPLE_TEXTURE2D(_WaveNormal,sampler_WaveNormal,wuv*1.73+float2(t*.018,-t*.012));float2 slope=((wa.rg*2-1)*.6+(wb.rg*2-1)*.4)*.65;float3 normal=normalize(float3(slope.x,1,slope.y));float3 view=GetWorldSpaceNormalizeViewDir(i.world);Light light=GetMainLight(TransformWorldToShadowCoord(i.world));
    float spec=pow(saturate(dot(normal,normalize(light.direction+view))),80);c+=light.color*spec*.22;
    float ridge=smoothstep(.82,.95,1-abs((wa.b+wb.b)*.5-.5)*2);c+=half3(.4,.7,.9)*ridge*.010*(1-deep);c*=lerp(.78,1,light.shadowAttenuation);
    half4 encodedReflection=SAMPLE_TEXTURECUBE_LOD(_RiverReflection,sampler_RiverReflection,reflect(-view,normalize(float3(normal.x*.35,normal.y,normal.z*.35))),3);half3 environment=DecodeHDREnvironment(encodedReflection,_RiverReflection_HDR);float fresnel=pow(1-saturate(dot(normalize(float3(normal.x*.35,normal.y,normal.z*.35)),view)),4);c=lerp(c,environment,saturate((.08+.22*fresnel)*_ReflectionStrength));
    float contact=1-smoothstep(.035,.30,column);float foam=contact*smoothstep(.20,.65,noise21(i.world.xz*4+float2(t*.025,0)));c=lerp(c,half3(.57,.76,.73),foam*.78);
    float opacity=saturate(.38+column*.55);opacity=max(opacity,foam*.76);
    return half4(c,opacity);
   }
   ENDHLSL
  }
 }
}
