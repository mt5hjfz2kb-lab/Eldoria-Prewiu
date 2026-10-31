Shader "Eldoria/Heroes/ContactShadow"
{
    Properties { _ShadowTint ("Shadow tint", Color) = (0.035,0.025,0.018,0.72) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _ShadowTint;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs position=GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS=position.positionCS;output.positionWS=position.positionWS;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                Light light=GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                return half4(_ShadowTint.rgb,(1.0h-light.shadowAttenuation)*_ShadowTint.a);
            }
            ENDHLSL
        }
    }
}
