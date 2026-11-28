"""Export byte-verifiable runtime candidate without modifying canonical game files."""
import os,json,uuid,re,hashlib,shutil
from pathlib import Path
root=Path(os.environ["RUNNER_TEMP"]);src=root/"donors";art=root/"competition";out=root/"competition-source";out.mkdir(exist_ok=True)
params=json.loads((art/"parameters.json").read_text())
base=params["source_head"];assert base=="f27f4774e32efc5ad046478a07747eead4256c3e"
prefix="Unity/Assets/Experimentos/ValoriaModular25D/"
rows=[]
def blob(b):return hashlib.sha1(b"blob "+str(len(b)).encode()+bytes(1)+b).hexdigest()
def record(rel,b,old=None):
    dest="exact-"+str(len(rows))+".bin";(out/dest).write_bytes(b)
    rows.append({"path":prefix+rel,"file":dest,"old_blob":blob(old) if old else None,"new_blob":blob(b),"sha256":hashlib.sha256(b).hexdigest()})
def replace(s,a,b):assert s.count(a)==1,a;return s.replace(a,b)
old=(src/"Modular25DWorld.cs").read_bytes()
s=old.decode("utf-8-sig").replace("\r\n","\n")
s=replace(s,"        Transform millWorkYard, garrisonYard, grainYard, quarryYard;","        readonly Issue200CompetitionArt competitionArt=new Issue200CompetitionArt();\n        Transform millWorkYard, garrisonYard, grainYard, quarryYard;")
s=replace(s,"            RecoveredPropsPlaced++;","            competitionArt.Register(visual);\n            RecoveredPropsPlaced++;")
s=replace(s,"            quarryYard.gameObject.SetActive(StoneworksRenderer.gameObject.activeSelf);","            quarryYard.gameObject.SetActive(StoneworksRenderer.gameObject.activeSelf);\n            competitionArt.Apply(DisplayedBastionLevel);")
s=s.replace("            competitionArt.Apply(DisplayedBastionLevel);","            competitionArt.Apply(DisplayedBastionLevel);\n            Shader.SetGlobalFloat(\"_Issue200QuarryIntegration\",DisplayedBastionLevel==5 && (Issue200CompetitionArt.PreviewMask & 4)!=0 ? 1f : 0f);")
record("Scripts/Modular25DWorld.cs",s.encode(),old)
shader=(src/"DistrictGround.shader").read_bytes()
g=shader.decode("utf-8-sig").replace("\\r\\n","\\n")
g=replace(g,"float _Boroughs;","float _Boroughs,_Issue200QuarryIntegration;")
g=replace(g," ground=lerp(ground,lerp(dirt*.83,shale*half3(.98,.96,.93),.86),floorCover);",""" half3 oldQuarry=lerp(dirt*.83,shale*half3(.98,.96,.93),.86);
 // Keep the authored outcrop geometry, but break the dark slab into
 // mineral earth and exposed strata. Leave the meadow outside unchanged.
 half3 quarryScree=lerp(dirt*half3(1.08,1.04,.96),shale*half3(1.07,1.05,1.0),.30+.38*rock);
 half3 registeredQuarry=lerp(oldQuarry,quarryScree,_Issue200QuarryIntegration);
 half featheredCover=lerp(floorCover,smoothstep(.03,.98,floorCover),_Issue200QuarryIntegration*.55);
 ground=lerp(ground,registeredQuarry,featheredCover);""")
record("Resources/ValoriaModular25D/DistrictGround.shader",g.encode(),shader)
def guid(name):return uuid.uuid5(uuid.NAMESPACE_URL,"eldoria/issue200/competition/"+name).hex
record("Resources/Issue200Competition.meta",("fileFormatVersion: 2\nguid: "+guid("folder")+"\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n").encode())
for n,m in params["assets"].items():
    assert n in ("AceraZapateros","ArmeriaDelPatio","CorteCaliza")
    orig=(src/(n+".png")).read_bytes();new=(art/(n+"-final.png")).read_bytes()
    assert hashlib.sha256(orig).hexdigest()==m["original_sha256"] and hashlib.sha256(new).hexdigest()==m["final_sha256"]
    meta=(src/(n+".png.meta")).read_text();meta=re.sub(r"(?m)^guid: [a-f0-9]+$","guid: "+guid(n),meta)
    record("Resources/Issue200Competition/"+n+".png",new)
    record("Resources/Issue200Competition/"+n+".png.meta",meta.encode())
record("Scripts/Issue200CompetitionArt.cs",(src/"Issue200CompetitionArt.cs").read_bytes())
record("Editor/Issue200CompetitionProof.cs",(src/"Issue200CompetitionProof.cs").read_bytes())
assert len(rows)==11
(out/"exact-source-manifest.json").write_text(json.dumps({"base":base,"files":rows},indent=2))
shutil.copyfile(art/"parameters.json",out/"parameters.json")
shutil.copyfile(art/"ledger.json",out/"ledger.json")
print("COMPETITION_EXACT_SOURCE_READY",len(rows),flush=True)
