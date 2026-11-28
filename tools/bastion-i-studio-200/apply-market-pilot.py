"""Integrate the director-approved new market assets into isolated actual Unity runtime source.
Preserves original importer settings and building progression. No canonical ref writes."""
import sys,json,hashlib,subprocess,re,uuid,shutil
from pathlib import Path
root,art,out=map(Path,sys.argv[1:4]);out.mkdir(exist_ok=True)
p=json.loads((art/"parameters.json").read_text());v=p["values"];base=p["source_head"]
assert p["sprite_gate"]["merits_native"] is True and base=="f27f4774e32efc5ad046478a07747eead4256c3e"
def git(*args):return subprocess.check_output(["git","-C",str(root),*args])
assert git("rev-parse","HEAD").decode().strip()==base
prefix="Unity/Assets/Experimentos/ValoriaModular25D/"
rows=[]
def record(name,content,old):
    dest="exact-"+str(len(rows))+".bin";(out/dest).write_bytes(content)
    new=hashlib.sha1(b"blob "+str(len(content)).encode()+bytes(1)+content).hexdigest()
    rows.append(dict(path=name,file=dest,old_blob=old,new_blob=new,sha256=hashlib.sha256(content).hexdigest()))
    target=root/name;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(content)
def edit(path,fn):
    name=prefix+path;old=git("show","HEAD:"+name);before=git("rev-parse","HEAD:"+name).decode().strip()
    t=fn(old.decode("utf-8-sig").replace("\r\n","\n"));assert t.encode()!=old;record(name,t.encode(),before)
def replace(s,a,b):assert s.count(a)==1,a;return s.replace(a,b)
def f(k):return format(v[k],".4f")+"f"
def v2(x,y):return "new Vector2("+f(x)+","+f(y)+")"
def world(s):
    s=replace(s,"        public int DisplayedBastionLevel { get; private set; } = 1;","""        public int DisplayedBastionLevel { get; private set; } = 1;
        // Candidate runtime defaults to the authored district. Only the native
        // proof selects the untouched original for its matched control.
#if UNITY_EDITOR
        public static bool MarketPilotEnabled = true;
#else
        public static bool MarketPilotEnabled => true;
#endif
        readonly System.Collections.Generic.Dictionary<string,SpriteRenderer> marketPilotVisuals =
            new System.Collections.Generic.Dictionary<string,SpriteRenderer>();
        readonly System.Collections.Generic.Dictionary<string,Sprite> marketPilotOriginals =
            new System.Collections.Generic.Dictionary<string,Sprite>();
        readonly System.Collections.Generic.Dictionary<string,Vector3> marketPilotFeet =
            new System.Collections.Generic.Dictionary<string,Vector3>();
        readonly System.Collections.Generic.Dictionary<string,Sprite> marketPilotSprites =
            new System.Collections.Generic.Dictionary<string,Sprite>();
        bool? marketPilotWasApplied;
        public static Vector2 MarketPilotPesoOffset => PESO;
        public static Vector2 MarketPilotStallOffset => STALL;
        void ApplyMarketPilot(bool enabled)
        {
            if(marketPilotWasApplied==enabled)return;
            marketPilotWasApplied=enabled;
            foreach(var pair in marketPilotVisuals)
            {
                string name=pair.Key;var visual=pair.Value;
                if(enabled && !marketPilotSprites.ContainsKey(name))
                {
                    var art=Resources.Load<Sprite>("Issue200MarketPilot/"+name);
                    if(!art)throw new System.InvalidOperationException("Missing authored market sprite "+name);
                    marketPilotSprites.Add(name,art);
                }
                visual.sprite=enabled?marketPilotSprites[name]:marketPilotOriginals[name];
                Vector2 offset=name=="CasasDelPeso"?MarketPilotPesoOffset:MarketPilotStallOffset;
                visual.transform.localPosition=marketPilotFeet[name]+(enabled?(Vector3)offset:Vector3.zero);
                visual.sortingOrder=FootOrder(visual.transform.localPosition.y);
                visual.GetComponent<Modular25DArchitecturalShadow>()?.Refresh();
            }
        }""".replace("PESO",v2("peso_dx","peso_dy")).replace("STALL",v2("stall_dx","stall_dy")))
    s=replace(s,"            RecoveredPropsPlaced++;\n            return visual.transform;","""            if(name=="CasasDelPeso"||name=="PuestoDeAbastos")
            {
                marketPilotVisuals.Add(name,visual);
                marketPilotOriginals.Add(name,sprite);
                marketPilotFeet.Add(name,visual.transform.localPosition);
            }
            RecoveredPropsPlaced++;
            return visual.transform;""")
    s=replace(s,"            grainYard.gameObject.SetActive(GranaryRenderer.gameObject.activeSelf);","""            grainYard.gameObject.SetActive(GranaryRenderer.gameObject.activeSelf);
            bool marketPilot=MarketPilotEnabled && DisplayedBastionLevel==5 && GranaryRenderer.gameObject.activeSelf;
            ApplyMarketPilot(marketPilot);
            GetComponentInChildren<Modular25DGround>()?.SetMarketPilot(marketPilot);""")
    return s
def ground(s):
    return replace(s,"        Vector4 previousDistricts;","""        public void SetMarketPilot(bool enabled)
        {
            if(!districtMaterial)return;
            var peso=Modular25DWorld.MarketPilotPesoOffset;
            var stall=Modular25DWorld.MarketPilotStallOffset;
            districtMaterial.SetFloat("_MarketPilot",enabled?COURT:0f);
            districtMaterial.SetVector("_MarketPilotOffsets",new Vector4(peso.x,peso.y,stall.x,stall.y));
        }
        Vector4 previousDistricts;""".replace("COURT",f("court_strength")))
def shader(s):
    s=replace(s,' _Soil("Earth",2D)="white"{} _Slate("Quarry strata",2D)="white"{} _Built("Completed districts",Vector)=(0,0,0,0)',' _Soil("Earth",2D)="white"{} _Slate("Quarry strata",2D)="white"{} _Built("Completed districts",Vector)=(0,0,0,0)\n _MarketPilot("Authored market court",Float)=0\n _MarketPilotOffsets("Registered frontage feet",Vector)=(0,0,0,0)')
    s=replace(s,"float _Boroughs;","float _Boroughs,_MarketPilot;\nfloat4 _MarketPilotOffsets;")
    s=replace(s,"ground=lerp(ground,lerp(dirt*.89,pad,.42),market*.92);","""ground=lerp(ground,lerp(dirt*.89,pad,.42),market*.92);
 // Local working courtyard: anchored to the actual new storefronts and
 // granary yard, not an image of a district or an opaque rectangle.
 float2 pesoFoot=_LowerBlock.zw+_MarketPilotOffsets.xy;
 float2 granaryFoot=_MarketYard.xy+float2(0,.35);
 float2 stallFoot=granaryFoot+float2(.73,-2.12)+_MarketPilotOffsets.zw;
 half frontCourt=yard(p,float4(pesoFoot+float2(-.03,-.43),1.54,.76),erosion);
 half loadingCourt=yard(p,float4(stallFoot+float2(-.10,.02),1.36,.78),erosion);
 float2 access=pesoFoot+float2(-.26,-.18);
 float2 lane=_LowerBlock.zw+float2(-1.45,-.55);
 half connection=(1-smoothstep(.20,.61,distanceToLane(p,access,lane)+rough*.13));
 half marketPath=max(frontCourt,connection*.82);
 half3 mineral=lerp(laidStone*half3(.98,.94,.85),dirt*.94,.23);
 half grain=(.5+.5*sin(p.x*10.1+p.y*6.4)*sin(p.x*4.3-p.y*9.1));
 half3 swept=lerp(dirt*.97,dirt*half3(1.16,1.06,.87),grain*.26);
 ground=lerp(ground,mineral,_MarketPilot*marketPath*.86);
 ground=lerp(ground,swept,_MarketPilot*loadingCourt*.66);""")
    return s
edit("Scripts/Modular25DWorld.cs",world)
edit("Scripts/Modular25DGround.cs",ground)
edit("Resources/ValoriaModular25D/DistrictGround.shader",shader)
manifest=json.loads((art/"asset-manifest.json").read_text())
folder=prefix+"Resources/Issue200MarketPilot"
def guid(name):return uuid.uuid5(uuid.NAMESPACE_URL,"eldoria/issue200/market-pilot/"+name).hex
record(folder+".meta",("fileFormatVersion: 2\nguid: "+guid("folder")+"\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n").encode(),None)
for n,m in manifest.items():
    assert n in ("CasasDelPeso","PuestoDeAbastos")
    original=prefix+"Resources/ValoriaModular25D/RecoveredProps/"+n+".png"
    donor=git("show","HEAD:"+original)
    assert hashlib.sha256(donor).hexdigest()==m["donor_sha256"]
    new=(art/(n+".png")).read_bytes();assert hashlib.sha256(new).hexdigest()==m["fitted_sha256"]
    meta=git("show","HEAD:"+original+".meta").decode("utf-8-sig")
    meta=re.sub(r"(?m)^guid: [a-f0-9]+$", "guid: "+guid(n),meta.replace("\r\n","\n"))
    record(folder+"/"+n+".png",new,None);record(folder+"/"+n+".png.meta",meta.encode(),None)
assert len(rows)==8
(out/"exact-source-manifest.json").write_text(json.dumps({"base":base,"files":rows,"sprite_manifest":manifest},indent=2))
shutil.copyfile(art/"parameters.json",out/"parameters.json")
shutil.copyfile(art/"ledger.json",out/"ledger.json")
shutil.copyfile(art/"asset-comparison.jpg",out/"asset-comparison.jpg")
print("MARKET_RUNTIME_FILES_EXPORTED",len(rows),flush=True)
