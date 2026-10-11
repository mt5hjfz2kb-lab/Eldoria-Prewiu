Shader "Eldoria/ValoriaLowerGateProjection"
{
    Properties
    {
        _ReferenceTex ("Canonical Reference", 2D) = "white" {}
        _FallbackColor ("Fallback Stone", Color) = (0.22,0.20,0.18,1)
        _ProjectionStrength ("Projection Strength", Range(0,1)) = 1
        _FacingStart ("Facing Start", Range(0,1)) = 0.12
        _FacingFull ("Facing Full", Range(0,1)) = 0.42
        _FlipY ("Flip Y", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Overlay+100" }
        Pass
        {
            Name "ValoriaProjection"
            ZWrite Off
            ZTest Always
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            TEXTURE2D(_ReferenceTex);
            SAMPLER(sampler_ReferenceTex);
            float4x4 _ProjectorVP;
            float4 _ProjectorPosition;
            float4 _FallbackColor;
            float _ProjectionStrength;
            float _FacingStart;
            float _FacingFull;
            float _FlipY;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 proj = mul(_ProjectorVP, float4(IN.positionWS,1));
                float invW = rcp(max(abs(proj.w), 1e-6));
                float2 uv = proj.xy * invW * 0.5 + 0.5;
                if (_FlipY > 0.5) uv.y = 1.0 - uv.y;

                float inside = step(0.0,uv.x)*step(uv.x,1.0)*step(0.0,uv.y)*step(uv.y,1.0)*step(0.0,proj.w);
                float3 toProjector = normalize(_ProjectorPosition.xyz - IN.positionWS);
                float facing = abs(dot(normalize(IN.normalWS),toProjector));
                float facingWeight = smoothstep(_FacingStart,_FacingFull,facing);
                float weight = saturate(inside * facingWeight * _ProjectionStrength);

                half4 reference = SAMPLE_TEXTURE2D(_ReferenceTex,sampler_ReferenceTex,uv);
                return lerp(_FallbackColor,reference,weight);
            }
            ENDHLSL
        }
    }
}