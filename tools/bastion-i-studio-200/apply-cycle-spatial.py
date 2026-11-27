"""Second artistic approach: exact original blobs -> physical placement and road mesh edits.
Only isolated native candidate; export byte-verified replacement manifest."""
import sys,json,subprocess,hashlib
from pathlib import Path
root=Path(sys.argv[1]);settings=Path(sys.argv[2]);out=Path(sys.argv[3]);out.mkdir(exist_ok=True,parents=True)
data=json.loads(settings.read_text());v=data["values"];base="f27f4774e32efc5ad046478a07747eead4256c3e"
def git(*args):return subprocess.check_output(["git","-C",str(root),*args])
assert git("rev-parse","HEAD").decode().strip()==base==data["source_head"]
prefix="Unity/Assets/Experimentos/ValoriaModular25D/"
rows=[]
def replace(s,a,b):assert s.count(a)==1,a;return s.replace(a,b)
def f(k):return format(v[k],".4f")+"f"
def vec(x,y):return "new Vector2("+f(x)+","+f(y)+")"
def edit(path,fn):
    name=prefix+path;old=git("show","HEAD:"+name);before=git("rev-parse","HEAD:"+name).decode().strip()
    s=old.decode("utf-8-sig").replace("\r\n","\n");t=fn(s);assert t!=s
    content=t.encode("utf-8");after=hashlib.sha1(b"blob "+str(len(content)).encode()+bytes(1)+content).hexdigest()
    dest="replacement-"+str(len(rows))+".bin";(out/dest).write_bytes(content)
    rows.append(dict(path=name,file=dest,old_blob=before,new_blob=after,sha256=hashlib.sha256(content).hexdigest()))
    (root/name).write_bytes(content)
def plan(s):
    s=replace(s,"public static readonly ValoriaUrbanPlan City = new ValoriaUrbanPlan(true);","""public static readonly ValoriaUrbanPlan City = new ValoriaUrbanPlan(true);
        // Enabled in the candidate runtime; only the isolated Editor proof may disable it.
#if UNITY_EDITOR
        public static bool SpatialArtistEnabled = true;
#else
        public static bool SpatialArtistEnabled => true;
#endif
        public Vector2 PesoArtistOffset => SpatialArtistEnabled ? PESO : Vector2.zero;
        public Vector2 WeaverArtistOffset => SpatialArtistEnabled ? WEAVER : Vector2.zero;
        public Vector2 MillFenceArtistOffset => SpatialArtistEnabled ? FENCE : Vector2.zero;
        public float ArtistRoadWidth(string id,float original) {
            if(!SpatialArtistEnabled)return original;
            switch(id){
                case "bastion": return GATE;
                case "sawmill": return MILL;
                case "freight": return FREIGHT;
                case "postern": return POSTERN;
                case "branch": return BRANCH;
                default:return original;
            }
        }
        public float ArtistCoreInk => SpatialArtistEnabled ? INK : 0f;""".replace("PESO",vec("peso_dx","peso_dy")).replace("WEAVER",vec("weavers_dx","weavers_dy")).replace("FENCE",vec("fence_dx","fence_dy")).replace("GATE",f("gate_width")).replace("MILL",f("mill_width")).replace("FREIGHT",f("freight_width")).replace("POSTERN",f("postern_width")).replace("BRANCH",f("branch_width")).replace("INK",f("core_ink")))
    s=replace(s,'new Prop(3,"CasasDelPeso",MarketRow,0,0,1f,5)','new Prop(3,"CasasDelPeso",MarketRow+PesoArtistOffset,0,0,1f,5)')
    s=replace(s,'new Prop(3,"FrenteTejedores",Junction,-2.15f,-1.57f,.96f,4,4)','new Prop(3,"FrenteTejedores",Junction+WeaverArtistOffset,-2.15f,-1.57f,.96f,4,4)')
    return replace(s,'new Prop(1,"VallaLenador",Mill.Foot,2.03f,-1.34f,.65f)','new Prop(1,"VallaLenador",Mill.Foot+MillFenceArtistOffset,2.03f,-1.34f,.65f)')
def landscape(s):
    s=replace(s,'plan.Approach(plot),paved?1.32f:.92f,','plan.Approach(plot),plan.ArtistRoadWidth(plot.Id,paved?1.32f:.92f),')
    s=replace(s,'plan.FreightRoad,1.15f,imperialMat,true','plan.FreightRoad,plan.ArtistRoadWidth("freight",1.15f),imperialMat,true')
    s=replace(s,'streets[i],i==5?.66f:i==2?1.18f:1.04f,imperialMat,true','streets[i],plan.ArtistRoadWidth(i==5?"postern":i==2?"freight":"branch",i==5?.66f:i==2?1.18f:1.04f),imperialMat,true')
    s=replace(s,'float alpha=Mathf.Clamp01(feather*ink);','''// Independent ribbons now have a clearly defined travelled core.
                    // Keep their route centers and eroded shoulders unchanged.
                    float art=ValoriaUrbanPlan.City.ArtistCoreInk;
                    float spineCore=1f-Sm(.26f,.74f,bank);
                    float alpha=Mathf.Clamp01(Mathf.Lerp(feather*ink,Mathf.Max(feather*ink,spineCore*art),art));''')
    return replace(s,'alpha*=Mathf.Lerp(.16f,1f,Sm(0,.23f,totalLength-along));','alpha*=Mathf.Lerp(art>0f?.42f:.16f,1f,Sm(0,.23f,totalLength-along));')
def groundcs(s):
    s=replace(s,'districtMaterial.SetVector("_LowerBlock",new Vector4(plan.WorkshopRow.x,plan.WorkshopRow.y,plan.MarketRow.x,plan.MarketRow.y));','''districtMaterial.SetVector("_LowerBlock",new Vector4(plan.WorkshopRow.x,plan.WorkshopRow.y,plan.MarketRow.x,plan.MarketRow.y));
                districtMaterial.SetFloat("_SpatialArt",ValoriaUrbanPlan.SpatialArtistEnabled?1f:0f);
                var peso=plan.PesoArtistOffset;var weaver=plan.WeaverArtistOffset;
                districtMaterial.SetVector("_SpatialFrontageOffsets",new Vector4(peso.x,peso.y,weaver.x,weaver.y));''')
    return s
def shader(s):
    s=replace(s,"float _Boroughs;","float _Boroughs,_SpatialArt;\nfloat4 _SpatialFrontageOffsets;")
    s=replace(s,"half scales=yard(p,float4(_LowerBlock.zw+float2(-.06,-.24),2.03,.62),erosion);","half scales=yard(p,float4(_LowerBlock.zw+_SpatialFrontageOffsets.xy+float2(-.06,-.24),2.03,.62),erosion);")
    s=replace(s,"half3 gravel=lerp(dirt*.86,seam*half3(.90,.90,.86),stoneShare);","""// Clear mineral carriageway, softer non-mineral shoulders.
 half travelCore=1-smoothstep(.38,.93,abs(side)/max(width,.1));
 half spatialStone=lerp(.18,.81,travelCore);
 stoneShare=lerp(stoneShare,spatialStone,_SpatialArt*avenueUse);
 half3 gravel=lerp(dirt*.86,seam*half3(.90,.90,.86),stoneShare);""")
    s=replace(s,"ground=lerp(ground,lerp(dirt*.81,seam*.88,.33),saturate(urban*.25+crossing*.49));","""ground=lerp(ground,lerp(dirt*.81,seam*.88,.33),saturate(urban*.25+crossing*.49));
 // Do not paint an undifferentiated stone circle over the well and street junction.
 half crossingLane=1-smoothstep(.26,.72,approach);
 ground=lerp(ground,lerp(dirt*.93,seam*.96,.70),_SpatialArt*crossing*crossingLane*.67);
 ground=lerp(ground,dirt*.96,_SpatialArt*crossing*(1-crossingLane)*.43);
 // Register contact to the MOVED weighing-house and weaver frontage feet.
 float2 peso=_LowerBlock.zw+_SpatialFrontageOffsets.xy;
 half pesoAccess=1-smoothstep(.15,.48,distanceToLane(p,peso+float2(-.1,-.22),_LowerBlock.zw+float2(-.46,-.57)));
 float2 weaver=_CivicAxis.zw+float2(-2.15,-1.57)+_SpatialFrontageOffsets.zw;
 half weaveAccess=1-smoothstep(.16,.47,distanceToLane(p,weaver+float2(.14,-.15),weaver+float2(.76,.12)));
 ground=lerp(ground,lerp(dirt*.96,seam*.97,.41),_SpatialArt*max(pesoAccess*_Boroughs*_Built.y,weaveAccess*_Built.w*(1-_Boroughs))*.72);""")
    return s
edit("Scripts/ValoriaUrbanPlan.cs",plan)
edit("Scripts/Modular25DLandscape.cs",landscape)
edit("Scripts/Modular25DGround.cs",groundcs)
edit("Resources/ValoriaModular25D/DistrictGround.shader",shader)
patch=git("diff","--binary");assert patch.count(b"diff --git ")==4
(out/"cycle-production.patch").write_bytes(patch)
(out/"exact-source-manifest.json").write_text(json.dumps(dict(base=base,patch_sha256=hashlib.sha256(patch).hexdigest(),files=rows),indent=2))
print("PHYSICAL_ARTIST_SOURCE_REPLACEMENTS",len(rows))
