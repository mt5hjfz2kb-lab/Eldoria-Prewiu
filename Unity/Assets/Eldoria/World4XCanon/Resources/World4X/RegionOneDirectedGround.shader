Shader "Eldoria/World4X/DirectedGeographicLayers"
{
 Properties { _Mask("Authored geography masks",2D)="black"{} _Pigment("Painted meadow",2D)="white"{} _Origin("World origin",Vector)=(0,0,0,0) _Near("Near detail",Float)=0 _Domain("Geographic domain",Vector)=(1820,1560,0,0) _Panorama("Panorama haze",Float)=0 _Blockout("Blockout",Float)=0 }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass
  {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   struct A{float3 positionOS:POSITION;float3 normalOS:NORMAL;};
   struct V{float4 positionCS:SV_POSITION;float2 p:TEXCOORD0;float3 n:TEXCOORD1;float3 world:TEXCOORD2;};
   TEXTURE2D(_Mask);SAMPLER(sampler_Mask);TEXTURE2D(_Pigment);SAMPLER(sampler_Pigment);
   CBUFFER_START(UnityPerMaterial) float4 _Origin;float4 _Domain;float _Near;float _Panorama;float _Blockout; CBUFFER_END
   V vert(A v){V o;float3 w=TransformObjectToWorld(v.positionOS);o.positionCS=TransformWorldToHClip(w);o.world=w;o.p=w.xz-_Origin.xy;o.n=TransformObjectToWorldNormal(v.normalOS);return o;}
   float hash21(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise21(float2 p){float2 k=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash21(k),hash21(k+float2(1,0)),f.x),lerp(hash21(k+float2(0,1)),hash21(k+1),f.x),f.y);}
   float cloudBank(float2 p,float2 c,float2 radius){float2 d=(p-c)/radius;return exp(-dot(d,d)*1.7);}
   half4 frag(V i):SV_Target
   {
    float3 face=normalize(cross(ddy(i.world),ddx(i.world)));
    if(dot(face,i.n)<0)face=-face;
    i.n=normalize(lerp(i.n,face,.65));
    float2 uv=i.p/_Domain.xy+.5;
    half4 m=SAMPLE_TEXTURE2D(_Mask,sampler_Mask,uv);
    half3 tex=SAMPLE_TEXTURE2D(_Pigment,sampler_Pigment,abs(frac((i.p+float2(17,31))/64)*2-1)).rgb;
    half3 earth=half3(.72,.66,.30),sand=half3(.80,.70,.43),sea=half3(.045,.25,.36),shallow=half3(.07,.46,.56),river=half3(.055,.36,.47),soil=half3(.80,.70,.43);
    #if !defined(UNITY_COLORSPACE_GAMMA)
     earth=SRGBToLinear(earth);sand=SRGBToLinear(sand);sea=SRGBToLinear(sea);shallow=SRGBToLinear(shallow);river=SRGBToLinear(river);soil=SRGBToLinear(soil);
    #endif
    half3 broad=SAMPLE_TEXTURE2D(_Pigment,sampler_Pigment,abs(frac((i.p+float2(43,7))/150)*2-1)).rgb;
    half meadow=noise21(i.p*.012)*.65h+noise21(i.p*.034+19)*.35h;
    // Broad quiet meadows, fine pigment subordinate; no repeated mirrored camouflage.
    half3 land=lerp(earth*.86h,earth*1.10h,meadow);
    land=lerp(land,broad,.13h);land=lerp(land,tex,.09h+_Near*.13h);
    half3 woodland=half3(.48,.57,.26);
    #if !defined(UNITY_COLORSPACE_GAMMA)
     woodland=SRGBToLinear(woodland);
    #endif
    half drainage=SAMPLE_TEXTURE2D_LOD(_Mask,sampler_Mask,uv,3).g;
    half wet=smoothstep(.015h,.12h,drainage)*(1-m.g);
    half irregular=smoothstep(.25h,.65h,noise21(i.p*.12)+wet*.28h);
    half growth=saturate(m.a*1.35h+wet*irregular*.55h);
    woodland*=.86h+.28h*noise21(i.p*.21);
    land=lerp(land,woodland,growth);land*=1-m.a*.23h;
    land=lerp(land,soil,m.b*.46h);
    Light sun=GetMainLight();
    half lambert=saturate(dot(normalize(i.n),sun.direction));
    half light=.60h+.48h*lambert;
    land*=light;
    // Geological material is tied to the actual sculpted surface normal, not scattered prefab rocks.
    half slope=smoothstep(.025h,.14h,1-saturate(normalize(i.n).y));
    half strata=.82h+.18h*noise21(i.p*float2(.13,.29));
    land=lerp(land,sand*light*strata,slope*.90h);
    // Fractured mineral bed belongs to this terrain, centred on the same immutable fault anchor.
    float2 fault=i.p-float2(8,61);
    float damage=saturate(1-length(fault*float2(1,.94))/48);
    if(damage>0)
    {
     float2 cell=floor(i.p/6),local=frac(i.p/6);float first=10,second=10;
     for(int cy=-1;cy<=1;cy++)for(int cx=-1;cx<=1;cx++)
     {float2 neighbour=float2(cx,cy);float seed=hash21(cell+neighbour);float2 site=neighbour+float2(seed,hash21(cell+neighbour+37))*.7+.15;float distance=length(site-local);if(distance<first){second=first;first=distance;}else second=min(second,distance);}
     half joint=1-smoothstep(.025h,.12h,second-first);
     half3 shale=half3(.35,.30,.35),faultDark=half3(.21,.13,.27);
     #if !defined(UNITY_COLORSPACE_GAMMA)
      shale=SRGBToLinear(shale);faultDark=SRGBToLinear(faultDark);
     #endif
     half damaged=smoothstep(0,.7,damage)*(.60h+.40h*noise21(i.p*.18));
     land=lerp(land,lerp(shale,faultDark,joint)*light,damaged*.90h);
    }
    // Fine grass and stony interruptions remain subordinate to the large fields.
    half fleck=smoothstep(.75h,.88h,noise21(i.p*.8));
    land=lerp(land,sand*light,fleck*(.08h+.12h*_Near));
    half bank=smoothstep(.01h,.16h,m.g)*(1-smoothstep(.30h,.72h,m.g));
    land=lerp(land,sand*light,bank*(.20h+.35h*noise21(i.p*.19)));
    half3 freshWater=lerp(shallow,river,smoothstep(.4h,.96h,m.g));
    freshWater*=.86h+.24h*noise21(i.p*float2(.18,.31));
    half flow=smoothstep(.83h,.94h,noise21(i.p*float2(.85,2.1)))*smoothstep(.60h,.90h,m.g);
    freshWater=lerp(freshWater,shallow*1.25h,flow*.32h*_Near);
    land=lerp(land,freshWater,smoothstep(.25h,.80h,m.g));
    half3 blocked=lerp(half3(.30,.34,.27),half3(.09,.18,.10),m.a);
    blocked=lerp(blocked,half3(.09,.25,.35),smoothstep(.25h,.80h,m.g));
    land=lerp(land,blocked*light,_Blockout);
    half3 coast=lerp(sand*.73h,sand,light);
    half landWeight=smoothstep(.40h,.49h,m.r);
    half3 water=lerp(sea,shallow,smoothstep(.38h,.49h,m.r));
    water=lerp(water,shallow,(.10h+.18h*noise21(i.p*.027)+.09h*noise21(i.p*.08))*(1-landWeight));
    half ripple=(noise21(i.p*float2(.10,.24))-.5h)*.032h;
    water+=ripple;
    half foam=smoothstep(.42h,.47h,m.r)*(1-smoothstep(.47h,.50h,m.r));
    water=lerp(water,sand,foam*.38h);
    half3 c=lerp(water,lerp(coast,land,smoothstep(.52h,.59h,m.r)),landWeight);
    half side=1-saturate(normalize(i.n).y);
    c=lerp(c,sand*(.49h+.30h*light),side*landWeight*.92h);
    // Atmospheric continuation lies beyond the readable region, never on regional terrain.
    half fog=max(cloudBank(i.p,float2(-610,60),float2(100,650)),
                 max(cloudBank(i.p,float2(-120,570),float2(650,95)),
                     cloudBank(i.p,float2(-150,-590),float2(650,85))));
    half3 mist=half3(.78,.83,.85);
    #if !defined(UNITY_COLORSPACE_GAMMA)
     mist=SRGBToLinear(mist);
    #endif
    half cloud=.48h*noise21(i.p*.021)+.32h*noise21(i.p*.049+17)+.20h*noise21(i.p*.105+41);
    half cloudAlpha=smoothstep(.13h,.68h,fog*(.40h+.95h*cloud));
    return half4(lerp(c,mist*(.88h+.12h*cloud),cloudAlpha*.65h*_Panorama*(1-_Blockout)),1);
   }
   ENDHLSL
  }
 }
}
