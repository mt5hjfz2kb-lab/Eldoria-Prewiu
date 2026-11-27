"""Deterministic implementation of studio cycle artist selections in real Unity source.
Runs only in disposable exact-SHA checkout, produces a git patch. No game repo ref changes.
"""
import sys,json,subprocess
from pathlib import Path
root=Path(sys.argv[1]);settings=Path(sys.argv[2]);out=Path(sys.argv[3]);out.mkdir(exist_ok=True,parents=True)
data=json.loads(settings.read_text());v=data["values"];base="f27f4774e32efc5ad046478a07747eead4256c3e"
assert subprocess.check_output(["git","-C",str(root),"rev-parse","HEAD"],text=True).strip()==base==data["source_head"]
prefix=root/"Unity/Assets/Experimentos/ValoriaModular25D"
def edit(path,fn):
    p=prefix/path;s=p.read_text(encoding="utf-8-sig");t=fn(s);assert t!=s;p.write_text(t,encoding="utf-8",newline="\n")
def replace(s,a,b):assert s.count(a)==1,a;return s.replace(a,b)
def val(k):return format(v[k],".4f")
def ground(s):
    s=replace(s,"float _Boroughs;","float _Boroughs;\nfloat _CycleLevel,_CycleMillBuilt;\nfloat _ValoriaCycleProofControl;")
    s=replace(s,"half3 ground=tex2D(_MainTex,i.uv).rgb;","""half3 ground=tex2D(_MainTex,i.uv).rgb;
 half cycle=1-saturate(_ValoriaCycleProofControl);
 // Broad world-space pigment modulation, not another city texture or decal.
 half macro=.52*sin(p.x*.31+p.y*.19)+.30*sin(p.x*.16-p.y*.37)+.18*sin(p.x*.59+p.y*.43);
 ground*=1+cycle*MEADOW*.14*macro;""".replace("MEADOW",val("meadow")))
    s=replace(s,"ground=lerp(ground,lerp(workEarth,ground,.23+.15*blades),millYard*.84);","""ground=lerp(ground,lerp(workEarth,ground,.23+.15*blades),millYard*.84);
 // C sawmill foot remains fixed. A worn yard and broken, muted sawdust strip
 // near the existing right-hand bench, only on the canonically built workshop.
 half millWear=millYard*cycle*MILL*saturate(_CycleMillBuilt);
 half irregular=.5+.5*sin(p.x*13.7+p.y*8.9)*sin(p.x*7.1-p.y*12.4);
 half sawdust=(1-smoothstep(.10,.58,distanceToLane(millGround,float2(-.85,-.22),float2(.9,-.63))+.10*irregular));
 half3 compact=lerp(dirt*.98,dirt*half3(1.22,1.13,.88),sawdust*(.40+.35*irregular));
 half wear=millWear*(.44+.22*irregular);
 ground=lerp(ground,compact,wear);""".replace("MILL",val("mill")))
    s=replace(s,"half stoneShare=lerp(.20,.56,max(avenueUse,1-smoothstep(.15,1.30,alongStreet)));","""half stoneShare=lerp(.20,.56,max(avenueUse,1-smoothstep(.15,1.30,alongStreet)));
 // Keep old street centre; distinguish mineral carriageway and earth shoulders.
 half centreBed=1-smoothstep(.22,.83,abs(side)/max(.2,width));
 stoneShare=lerp(stoneShare,lerp(.12,.74,centreBed),cycle*ROAD*.80);""".replace("ROAD",val("road")))
    s=replace(s,"ground=lerp(ground,lerp(dirt*.81,seam*.88,.33),saturate(urban*.25+crossing*.49));","""ground=lerp(ground,lerp(dirt*.81,seam*.88,.33),saturate(urban*.25+crossing*.49));
 // Keep three existing branches visually distinct instead of a broad opaque plaza.
 half junctionBed=(1-smoothstep(.13,.73,approach))*crossing;
 half junctionShoulder=crossing*(1-junctionBed)*smoothstep(.30,.85,approach);
 ground=lerp(ground,lerp(dirt*.96,seam*.97,.70),cycle*JUNCTION*junctionBed*.70);
 ground=lerp(ground,dirt*.97,cycle*JUNCTION*junctionShoulder*.42);
 // The accessible side of the actual well (Junction + .94,-.20).
 float2 well=_CivicAxis.zw+float2(.94,-.20);
 half wellUse=1-smoothstep(.12,.57,distanceToLane(p,well+float2(-.23,-.25),_CivicAxis.zw));
 ground=lerp(ground,lerp(dirt*.98,seam,.20),wellUse*cycle*DOORS*.58);""".replace("JUNCTION",val("junction")).replace("DOORS",val("doors")))
    s=replace(s,"ground=lerp(ground,flags,landing*.91);","""ground=lerp(ground,flags,landing*.91);
 // Concentrate mineral contact at the real threshold. Extend only a short
 // walkable, irregular taper towards the existing civic spine.
 half doorstepStone=1-smoothstep(.40,1.15,max(abs(gateSide+.12*run)/.84,abs(run-.34)/.82)+rough*.52);
 half gateTaper=(1-smoothstep(.18,.70,abs(gateSide-curve)+rough*.27))*smoothstep(-.18,.15,run)*(1-smoothstep(1.10,2.15,run));
 half3 settled=lerp(laidStone*half3(1.10,1.03,.91),dirt,.12);
 ground=lerp(ground,settled,cycle*GATE*max(doorstepStone*.87,gateTaper*.48));
 // Existing houses only: short open contact tongues, not a new pavement band.
 float2 westHouse=gate+float2(-1.62,-2.55);
 float2 eastHouse=gate+float2(2.38,-2.72);
 half westUse=(1-smoothstep(.09,.46,distanceToLane(p,westHouse+float2(.12,-.13),westHouse+float2(.85,-.12))+rough*.10))*(1-_Boroughs);
 half eastUse=(1-smoothstep(.09,.47,distanceToLane(p,eastHouse+float2(-.10,-.14),eastHouse+float2(-.87,-.10))+rough*.10))*(1-_Boroughs)*_Built.x;
 float2 textile=_CivicAxis.zw+float2(-2.15,-1.57);
 half textileUse=(1-smoothstep(.08,.45,distanceToLane(p,textile+float2(.15,-.15),textile+float2(.82,.07))))*_Built.w*(1-_Boroughs);
 ground=lerp(ground,lerp(dirt*.95,seam*.95,.36),cycle*DOORS*max(westUse,max(eastUse,textileUse))*.64);
 // Mature city: connect the existing Postigo path under its open arch, plus
 // small doorstep patches at the mapped frontage stations. Keep all paths.
 float2 arch=_UpperBlock.xy;
 half passage=1-smoothstep(.14,.52,distanceToLane(p,arch+float2(0,-.38),arch+float2(-.10,.88))+rough*.14);
 ground=lerp(ground,lerp(laidStone*.97,dirt,.20),cycle*POSTERN*passage*_Boroughs*.72);
 float2 portales=_LowerBlock.xy;
 half portalA=1-smoothstep(.08,.44,distanceToLane(p,portales+float2(-.72,-.02),portales+float2(-.65,-.58)));
 half portalB=1-smoothstep(.08,.44,distanceToLane(p,portales+float2(.77,-.13),portales+float2(.70,-.70)));
 half adarve=1-smoothstep(.10,.62,distanceToLane(p,_UpperBlock.zw+float2(-.42,-.28),_UpperBlock.zw+float2(-1.05,-.62))+rough*.18);
 half facade=max(max(portalA,portalB)*_Built.y,max(adarve,doorstep));
 ground=lerp(ground,lerp(dirt*.96,laidStone*.96,.44),cycle*DOORS*facade*_Boroughs*.60);
 // Freight approach ends on the existing market yard, with a graded shoulder.
 half freightBed=(1-smoothstep(.15,.86,distanceToLane(p,_WorkLane.zw,_MarketYard.xy)+rough*.17))*_Built.y;
 ground=lerp(ground,lerp(dirt*.92,seam*.94,.43),cycle*FREIGHT*freightBed*.53);""".replace("GATE",val("gate")).replace("DOORS",val("doors")).replace("POSTERN",val("postern")).replace("FREIGHT",val("freight")))
    return s
def roads(s):
    s=replace(s,"half _Stone;","half _Stone;\n float _ValoriaCycleProofControl;")
    s=replace(s,"half roadCoverage=i.alpha*(1-shoulder*(.35+.40*(1-chips)));","""half roadCoverage=i.alpha*(1-shoulder*(.35+.40*(1-chips)));
   half cycle=1-saturate(_ValoriaCycleProofControl);
   half strength=cycle*ROAD;
   half wornCore=1-smoothstep(.28,.86,edge);
   blend=lerp(blend,_Stone*(.19+.77*wornCore+.07*chips*shoulder),strength*.78);
   roadCoverage*=1-strength*shoulder*(.19+.16*(1-chips));""".replace("ROAD",val("road")))
    return s
def shadows(s):
    s=replace(s,"sampler2D _ShadowTex;","sampler2D _ShadowTex;\n            float _ValoriaCycleProofControl;")
    return replace(s,"return fixed4(i.color.rgb,alpha*i.color.a);","""// Existing near/far vertex gradient; strengthen only near contact.
                half nearFoot=smoothstep(.13,.22,i.color.a);
                half gain=(1-saturate(_ValoriaCycleProofControl))*CONTACT;
                return fixed4(i.color.rgb,alpha*saturate(i.color.a+nearFoot*.072*gain));""".replace("CONTACT",val("contacts")))
def ground_cs(s):
    s=replace(s,"bool previousBoroughs;","bool previousBoroughs;\n        public void SetCycleMillBuilt(bool built) { if(districtMaterial) districtMaterial.SetFloat(\"_CycleMillBuilt\",built?1f:0f); }")
    s=replace(s,"if (districtMaterial && built == previousDistricts && boroughs == previousBoroughs) return;","if (districtMaterial) districtMaterial.SetFloat(\"_CycleLevel\",level);\n            if (districtMaterial && built == previousDistricts && boroughs == previousBoroughs) return;")
    s=replace(s,"districtMaterial.SetVector(\"_Built\", built);","districtMaterial.SetFloat(\"_CycleLevel\",level);\n            districtMaterial.SetVector(\"_Built\", built);")
    return s
def world(s):
    return replace(s,'var stage=ParcelBuildingStates.For(snapshot,"sawmill");','var stage=ParcelBuildingStates.For(snapshot,"sawmill");\n            GetComponentInChildren<Modular25DGround>()?.SetCycleMillBuilt(stage==ParcelBuildingState.BUILT);')
edit("Resources/ValoriaModular25D/DistrictGround.shader",ground)
edit("Resources/ValoriaModular25D/UrbanRoad.shader",roads)
edit("Resources/ValoriaModular25D/ArchitecturalShadow.shader",shadows)
edit("Scripts/Modular25DGround.cs",ground_cs)
edit("Scripts/Modular25DWorld.cs",world)
patch=subprocess.check_output(["git","-C",str(root),"diff","--binary"],text=True)
assert patch.count("diff --git ")==5
(out/"cycle-production.patch").write_text(patch,encoding="utf-8")
(out/"parameters.json").write_text(json.dumps(data,indent=2))
print("APPLIED_SOURCE_PATCH",len(patch))
