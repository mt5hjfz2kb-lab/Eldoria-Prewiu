Shader "Eldoria/Valoria25D/StreetElevation"
{
    Properties {
        [PerRendererData] _MainTex ("Elevation", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("Renderer Color", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        _StreetAxis ("Projected street axis", Vector) = (1,0,0,0)
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "DisableBatching"="True" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off Blend One OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            #pragma vertex StreetVert
            #pragma fragment SpriteFrag
            #include "UnitySprites.cginc"
            float4 _StreetAxis;
            v2f StreetVert(appdata_t v) {
                float station = v.vertex.x;
                v.vertex.xy = float2(station * _StreetAxis.x, v.vertex.y + station * _StreetAxis.y);
                return SpriteVert(v);
            }
            ENDCG
        }
    }
}
