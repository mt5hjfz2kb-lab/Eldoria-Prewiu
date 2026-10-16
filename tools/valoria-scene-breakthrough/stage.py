"""Immutable source staging; no original branch or asset mutation."""
import pathlib,subprocess,hashlib,json,shutil,struct
root=pathlib.Path.cwd();out=root/"Unity/Assets/Eldoria/ArtTests/SceneBreakthrough/Sources";out.mkdir(parents=True,exist_ok=True)
ref="0abf13870d21725c30acf3f3e3a8c86385470073"
subprocess.run(["git","fetch","--depth","1","origin",ref],check=True)
sources={
 "gate":("art-source/valoria/production/first-hero-gate-v1/quality-v3/ValoriaHeroGate.glb",ref),
 "upper":("art-source/valoria/production/bastion-family-v1/BastionFamilyV1.glb","HEAD"),
 "bridge":("art-source/valoria/production/bridge-family-v1/BridgeFamilyV1.glb","HEAD"),
 "stairs":("art-source/valoria/production/stair-family-v1/StairFamilyV1.glb",ref),
 "wall":("art-source/valoria/production/asset-family-v2/quality-v3/wall_straight/wall_straight.glb",ref),
 "cabin":("art-source/valoria/production/sawmill-family-v1/state2/SawmillState2V1.glb","HEAD"),
 "tower":("art-source/valoria/production/asset-family-v2/quality-v3/watchtower/watchtower.glb",ref),
 "camp":("art-source/valoria/production/camp-family-v1/CampFamilyV1.glb","HEAD")}
manifest=[]
for key,(path,commit) in sources.items():
 data=subprocess.check_output(["git","show",commit+":"+path]);dest=out/key;dest.mkdir(exist_ok=True);glb=dest/(key+".glb");glb.write_bytes(data)
 source=json.loads(data[20:20+struct.unpack_from("<I",data,12)[0]])
 mats=[]
 for item in source.get("materials",[]):
  m=dict(item);p=dict(m.get("pbrMetallicRoughness",{}))
  for n,v in [("baseColorFactor",[1,1,1,1]),("metallicFactor",1),("roughnessFactor",1)]:p.setdefault(n,v)
  for n in ["baseColorTexture","metallicRoughnessTexture"]:p.setdefault(n,{"index":-1})
  m["pbrMetallicRoughness"]=p;m.setdefault("normalTexture",{"index":-1,"scale":1});m["normalTexture"].setdefault("scale",1)
  for n,v in [("alphaMode","OPAQUE"),("alphaCutoff",.5),("emissiveFactor",[0,0,0]),("doubleSided",False)]:m.setdefault(n,v)
  mats.append(m)
 (dest/"material-bindings.json").write_text(json.dumps({"materials":mats}))
 manifest.append({"id":key,"path":path,"ref":commit,"sha256":hashlib.sha256(data).hexdigest(),"bytes":len(data)})
(root/"docs/evidence/valoria-scene-breakthrough-20261016/source-manifest.json").write_text(json.dumps(manifest,indent=2))

for key,relative,expected in [
 ("pine","eldoria-one-full-medium-pine/glb/pine_medium.glb","003d59a9d184cb6cb04b2b2b21a8cb35c2b92e838cf788575044e576f7846e3e"),
 ("cliff","eldoria-rockface-palette-02/glb/cliff_rock_column_stone_neutral.glb","614b2f1db74d66cfc6c3ffc759dbc62083e434de107806b7ee47f2669df70447")]:
 base=pathlib.Path(__import__("os").environ["RUNNER_TEMP"])/("breakthrough-"+key)
 existing=out/key/(key+".glb")
 found=[existing] if existing.exists() and hashlib.sha256(existing.read_bytes()).hexdigest()==expected else list(base.rglob(pathlib.Path(relative).name))
 if len(found)!=1:raise RuntimeError("Natural source ambiguous "+key)
 data=found[0].read_bytes()
 if hashlib.sha256(data).hexdigest()!=expected:raise RuntimeError("Natural source hash mismatch "+key)
 dest=out/key;dest.mkdir(exist_ok=True);(dest/(key+".glb")).write_bytes(data)
 source=json.loads(data[20:20+struct.unpack_from("<I",data,12)[0]]);mats=[]
 for item in source.get("materials",[]):
  m=dict(item);p=dict(m.get("pbrMetallicRoughness",{}))
  for n,v in [("baseColorFactor",[1,1,1,1]),("metallicFactor",1),("roughnessFactor",1)]:p.setdefault(n,v)
  for n in ["baseColorTexture","metallicRoughnessTexture"]:p.setdefault(n,{"index":-1})
  m["pbrMetallicRoughness"]=p;m.setdefault("normalTexture",{"index":-1,"scale":1});m["normalTexture"].setdefault("scale",1)
  for n,v in [("alphaMode","OPAQUE"),("alphaCutoff",.5),("emissiveFactor",[0,0,0]),("doubleSided",False)]:m.setdefault(n,v)
  mats.append(m)
 (dest/"material-bindings.json").write_text(json.dumps({"materials":mats}))
 manifest.append({"id":key,"sha256":expected,"file":relative,"provenance":"existing reviewed natural source, no reauthoring"})
(root/"docs/evidence/valoria-scene-breakthrough-20261016/source-manifest.json").write_text(json.dumps(manifest,indent=2))
