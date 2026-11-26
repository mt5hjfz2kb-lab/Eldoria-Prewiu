Shader "Eldoria/Senda/WorldSprite" {
 Properties { _MainTex("World artwork",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _Cutoff("Alpha cutoff",Range(0,1))=.12 }
 SubShader { Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout" } Cull Off ZWrite On
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _Color;float _Cutoff;
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
   struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;UNITY_FOG_COORDS(1)};
   v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;UNITY_TRANSFER_FOG(o,o.pos);return o;}
   fixed4 frag(v2f i):SV_Target{fixed4 c=tex2D(_MainTex,i.uv)*_Color;clip(c.a-_Cutoff);UNITY_APPLY_FOG(i.fogCoord,c);return fixed4(c.rgb,1);}
   ENDCG
  }
 } Fallback Off
}
