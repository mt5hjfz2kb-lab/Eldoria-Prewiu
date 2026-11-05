Shader "Eldoria/ConstructionDelight/ArchitecturalReveal"
{
    Properties
    {
        [HDR] _GlowColor ("Warm timber and masonry light", Color) = (1.8, 1.05, 0.53, 1)
        _Strength ("Moment intensity", Range(0,1)) = 0
        _Sweep ("Structural reveal progress", Range(-0.2,1.2)) = 0
        _Mode ("Working=0, Completed=1", Float) = 1
        _MinY ("World-space structure base", Float) = 0
        _Height ("World-space structure height", Float) = 4
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent+60"
            "IgnoreProjector"="True"
        }
        Pass
        {
            Name "Authentic timber and stone reveal"
            Tags {"LightMode"="UniversalForward"}
            ZWrite Off
            ZTest LEqual
            Cull Back
            Offset -1, -1
            Blend SrcAlpha One

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _GlowColor;
                half _Strength;
                half _Sweep;
                half _Mode;
                float _MinY;
                float _Height;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings o;
                float3 world=TransformObjectToWorld(input.positionOS.xyz);
                o.positionWS=world;
                o.positionCS=TransformWorldToHClip(world);
                o.normalWS=normalize(TransformObjectToWorldNormal(input.normalOS));
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float height=saturate((i.positionWS.y-_MinY)/max(.25,_Height));
                // Light rises up the actual structural geometry: foundations,
                // facades and finally roof. No screen-space pasted halo.
                half distanceToSweep=(half)(height-_Sweep);
                half centerBand=exp(-distanceToSweep*distanceToSweep*49.0h);
                half softBand=exp(-distanceToSweep*distanceToSweep*13.5h);
                half3 normal=normalize(i.normalWS);
                half3 toward=normalize((half3)(_WorldSpaceCameraPos-i.positionWS));
                half rim=pow(saturate(1.0h-abs(dot(normal,toward))),2.0h);
                half overhead=saturate(dot(normal,half3(.22h,.90h,.32h))*.5h+.5h);
                half alpha=0.0h;
                if(_Mode<.5h)
                {
                    // Subtle moving glints along genuine scaffold timbers
                    // between intermittent sawdust/pounding bursts.
                    alpha=_Strength*(centerBand*.30h+softBand*rim*.32h);
                }
                else
                {
                    // A recognisable, restrained gilded wave on real roofing
                    // and carved masonry, not extra golden dots on the lawn.
                    half roofWeight=lerp(.88h,1.25h,overhead);
                    alpha=_Strength*(centerBand*.31h+softBand*.10h+
                                      centerBand*rim*.24h)*roofWeight;
                }
                half3 lightColor=_GlowColor.rgb*(1.08h+rim*.32h);
                return half4(lightColor,saturate(alpha));
            }
            ENDHLSL
        }
    }
    FallBack Off
}
