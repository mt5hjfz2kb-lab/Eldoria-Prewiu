Shader "Hidden/Eldoria/StylizedDirectionPilot/GraphicFinish"
{
    Properties
    {
        _MainTex ("Real Unity camera color", 2D) = "white" {}
        _StyleStrength ("Painted style", Range(0,1)) = 0.55
        _OutlineStrength ("Fine illustrative edge", Range(0,1)) = 0.18
        _Bands ("Soft light bands", Range(6,32)) = 18
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
        Pass
        {
            Name "OriginalGeometryIllustration"
            ZWrite Off ZTest Always Cull Off
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _StyleStrength;
            float _OutlineStrength;
            float _Bands;
            struct Attributes { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.positionCS=TransformObjectToHClip(v.positionOS.xyz);
                o.uv=v.uv;
                return o;
            }
            float Luma(float3 c) {return dot(c,float3(.2126,.7152,.0722));}
            half4 Frag(Varyings i):SV_Target
            {
                float2 uv=i.uv;
                float3 original=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv).rgb;
                float3 c=original;
                // Restrict warm grass, not stone roofs or the river.
                float yellow=smoothstep(.055,.25,c.r-c.b)*smoothstep(.055,.25,c.g-c.b);
                float brightGrass=1.0-smoothstep(.04,.20,c.r-c.g);
                float grass=yellow*brightGrass;
                float3 sage=float3(c.r*.83,c.g*.89,c.b*.99);
                c=lerp(c,sage,grass*.78*_StyleStrength);
                // A broad color script unifies dissimilar source asset packs.
                c=lerp(c,Luma(c).xxx,.085*_StyleStrength);
                float l=Luma(c);
                float bands=max(6,_Bands);
                float q=floor(l*bands+.5)/bands;
                float ratio=(q+.085)/(l+.085);
                c*=lerp(1.0,ratio,.40*_StyleStrength);
                // Fine edges from original camera, avoiding huge black outlines.
                float2 stepUv=_MainTex_TexelSize.xy*1.35;
                float3 left=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv-float2(stepUv.x,0)).rgb;
                float3 right=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv+float2(stepUv.x,0)).rgb;
                float3 up=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv+float2(0,stepUv.y)).rgb;
                float3 down=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv-float2(0,stepUv.y)).rgb;
                float edge=max(abs(Luma(left)-Luma(right)),abs(Luma(up)-Luma(down)));
                float ink=smoothstep(.15,.40,edge)*_OutlineStrength*_StyleStrength;
                c=lerp(c,float3(.13,.15,.13),ink);
                return half4(saturate(c),1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
