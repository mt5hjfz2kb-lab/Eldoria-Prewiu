Shader "Eldoria/Valoria/District ground" {
Properties {
 _MainTex("Meadow",2D)="white"{} _Paving("Cobble",2D)="white"{}
 _Soil("Earth",2D)="white"{} _Slate("Quarry strata",2D)="white"{} _Built("Completed districts",Vector)=(0,0,0,0)
}
SubShader { Tags {"RenderType"="Opaque" "Queue"="Geometry"} Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _MainTex,_Paving,_Soil,_Slate;
float4 _CivicAxis,_WorkLane,_Forecourt;
float4 _UpperBlock,_LowerBlock,_Orchard,_QuarryMeadow;
float _Boroughs;
float4 _Built, _MilitaryYard, _MarketYard, _StoneFace;
struct Vertex { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
struct Varying { float4 position:SV_POSITION; float2 uv:TEXCOORD0; };
Varying vert(Vertex v) { Varying o; o.position=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
half yard(float2 p, float4 plot, half erosion) {
 float2 q=(p-plot.xy)/plot.zw;
 // Oblique working terraces inherit the perspective of the painted buildings.
 q.x+=q.y*.32;
 float d=pow(pow(abs(q.x),3)+pow(abs(q.y),3),1.0/3.0);
 return 1-smoothstep(.72,1.08,d+erosion*.15);
}
float distanceToLane(float2 p,float2 a,float2 b) {
 float2 axis=b-a;
 float t=saturate(dot(p-a,axis)/max(.001,dot(axis,axis)));
 return length(p-a-axis*t);
}
half4 frag(Varying i):SV_Target {
 float2 p=i.uv*6.5;
 half3 ground=tex2D(_MainTex,i.uv).rgb;
 half3 seam=tex2D(_Paving,float2(.47*p.x+.17*p.y,.59*p.y-.08*p.x)).rgb;
 half3 dirt=tex2D(_Soil,float2(.24+.44*frac(.29*p.x+.09*p.y),frac(.31*p.y))).rgb;
 half erosion=dot(ground,half3(.24,.56,.20))+seam.r-.6;
 // The old field lies above the orchard boundary. The open lower meadow
 // remains available for expansion, with a different soil at the quarry edge.
 float2 orchard=p-_Orchard.xy;
 float bankHeight=orchard.y-orchard.x*.17;
 half upperField=smoothstep(.15,1.12,bankHeight)*(1-smoothstep(3.2,5.3,bankHeight))*
                 (1-smoothstep(7.7,10.6,abs(orchard.x)));
 half blades=smoothstep(.23,.77,frac(2.4*(bankHeight+.035*orchard.x*orchard.x)+.31*erosion));
 half arable=yard(p,float4(_Orchard.zw+float2(0,.35),3.35,2.02),erosion);
 half stony=yard(p,_QuarryMeadow,erosion);
 ground=lerp(ground,lerp(ground*half3(1.11,1.00,.77),dirt*.91,.42),arable*_Boroughs*.76);
 ground=lerp(ground,lerp(ground*.94,dirt*1.03,.54),upperField*_Boroughs*(.47+.48*blades));
 ground=lerp(ground,lerp(ground*.91,seam*half3(.90,.93,.95),.36),stony*_Boroughs*.76);
 half drill=yard(p,_MilitaryYard,erosion)*_Built.x;
 half market=yard(p,_MarketYard,erosion)*_Built.y;
 half rock=yard(p,_StoneFace,erosion)*_Built.z;
 float approach=distanceToLane(p,_CivicAxis.xy,_CivicAxis.zw);
 float alley=distanceToLane(p,_WorkLane.xy,_CivicAxis.zw);
 float bend=distanceToLane(p,_CivicAxis.zw,_WorkLane.zw);
 half urban=1-smoothstep(.72,2.05,min(approach,alley)+erosion*.43);
 // The shoulders of the street are occupied land. The strip narrows toward the
 // gate and broadens at the well. Thresholds on both sides share its paving.
 float2 spine=_CivicAxis.zw-_CivicAxis.xy;
 float axisLength=length(spine);
 float2 tangent=spine/axisLength;
 float alongStreet=dot(p-_CivicAxis.xy,tangent);
 float side=dot(p-_CivicAxis.xy,float2(-tangent.y,tangent.x));
 float station=saturate(alongStreet/axisLength);
 // Only widen the paving opposite a built frontage. At the well the street
 // opens sideways into the existing crossing, with a short, worn lower edge.
 float doors=smoothstep(.22,.47,station)*(1-smoothstep(.78,1.0,station));
 float occupied=side<0 ? 1 : _Built.x;
 float width=lerp(.81,1.02,station)+doors*occupied*1.04;
 float crossEdge=abs(side)-width+erosion*.23;
 float endEdge=max(-alongStreet-.22,alongStreet-axisLength-.38)+erosion*.22;
 half street=1-smoothstep(-.13,.19,max(crossEdge,endEdge));
 half kerb=(1-smoothstep(.035,.12,abs(abs(side)-.79)+.025*erosion))
           *smoothstep(.26,.65,alongStreet)*(1-smoothstep(axisLength-.57,axisLength,alongStreet));
 half3 gravel=lerp(dirt*.81,seam*half3(.90,.90,.86),.56);
 // Shared thresholds and service courts take the shape of the occupied
 // block. Beyond the last house the original meadow takes over.
 half upper=yard(p,float4(_UpperBlock.xy+float2(0,-.28),3.03,1.06),erosion);
 half rear=yard(p,float4(_UpperBlock.zw+float2(.18,-.21),2.02,1.08),erosion);
 half looms=yard(p,float4(_LowerBlock.xy+float2(0,-.27),2.63,1.05),erosion);
 half scales=yard(p,float4(_LowerBlock.zw+float2(0,-.28),2.85,1.18),erosion);
 half apron=max(max(upper,rear),max(looms,scales)*_Built.y)*_Boroughs;
 ground=lerp(ground,lerp(dirt*.94,seam*.93,.42),apron*.88);
 ground=lerp(ground,gravel,street*.89);
 ground=lerp(ground,seam*1.12,street*kerb*.64);
 half crossing=1-smoothstep(.46,1.48,length((p-_CivicAxis.zw)*float2(.85,1.2))+erosion*.32);
 ground=lerp(ground,lerp(dirt*.81,seam*.88,.33),saturate(urban*.25+crossing*.49));
 ground=lerp(ground,dirt*.86,(1-smoothstep(.56,1.33,bend+erosion*.42))*max(_Built.y,_Built.z)*(.53+.28*_Built.w));
 // Broad stone landing tapers into the avenue. Its exposed lip stops on either
 // side of the entrance so the approach stays at the same walking level.
 float2 court=(p-_Forecourt.xy)/_Forecourt.zw;
 float edge=abs(court.x)+abs(court.y)*.54;
 float outline=max(edge,abs(court.y));
 half worn=erosion*.075;
 half landing=1-smoothstep(.87,1.07,outline+worn);
 half margin=(1-smoothstep(.035,.105,abs(outline-.91+worn)))*smoothstep(.25,.51,abs(court.x));
 half front=smoothstep(.1,.9,-court.y);
 half3 laidStone=tex2D(_Paving,float2(.94*p.x+.34*p.y,1.18*p.y-.16*p.x)).rgb;
 half3 flags=lerp(laidStone,dirt,.30)*half3(1.01,.98,.92);
 flags*=1-margin*(.10+.28*front);
 ground=lerp(ground,flags,landing*.91);
 half3 pad=lerp(dirt*.84,seam*half3(.87,.84,.77),.44);
 ground=lerp(ground,pad,drill*.91);
 ground=lerp(ground,lerp(dirt*.89,pad,.42),market*.92);
 // The working face continues into a geological outcrop along the northwest edge.
 float2 q=p-_StoneFace.xy;
 float along=dot(q,float2(-.88,.47)),across=dot(q,float2(.47,.88));
 half3 shale=tex2D(_Slate,float2(.27*q.x+.46*q.y,.69*q.y-.19*q.x)).rgb;
 half trace=abs(across+.24*sin(along*1.35))+.30*erosion;
 half seamWidth=lerp(1.38,.47,smoothstep(.4,5.2,along));
 half outcrop=(1-smoothstep(seamWidth*.42,seamWidth+.5,trace))*smoothstep(-2,.3,along)*(1-smoothstep(4,6.0,along));
 half ledge=(1-smoothstep(.08,.25,abs(frac(across*2.7+.23*sin(along*1.4))-.45)))*outcrop;
 shale*=lerp(.82,1.17,ledge);
 half floorCover=saturate(max(rock*.88,outcrop))*_Built.z;
 ground=lerp(ground,lerp(dirt*.83,shale*half3(.98,.96,.93),.86),floorCover);
 return half4(ground,1);
}
ENDCG
} } }
