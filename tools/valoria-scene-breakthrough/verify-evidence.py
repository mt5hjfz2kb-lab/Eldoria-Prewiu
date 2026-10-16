import pathlib,json,struct,hashlib,re,sys
root=pathlib.Path.cwd();study=root/"Unity/Assets/Eldoria/ArtTests/SceneBreakthrough";evidence=root/"docs/evidence/valoria-scene-breakthrough-20261016"
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
pairs=["main","landscape","portrait","pan-left","pan-right","zoom-in","zoom-out","portrait-left","portrait-right"]
images=[]
for view in pairs:
 cams=[]
 for variant in ["final3d","final25d","baseline-matched"]:
  name=variant+"-"+view;path=evidence/(name+".png");data=path.read_bytes()
  if data[:8]!=b"\x89PNG\r\n\x1a\n" or len(data)<10000:raise RuntimeError("Invalid native capture "+name)
  w,h=struct.unpack(">II",data[16:24]);cam=json.loads((evidence/("camera-"+name+".json")).read_text())
  if [w,h]!=[cam["width"],cam["height"]]:raise RuntimeError("Camera resolution mismatch "+name)
  cams.append(cam);images.append({"name":name,"width":w,"height":h,"sha256":sha(path),"source_commit":cam["source_commit"]})
 for key in ["position","rotation","view","projection","orthographic_size","pipeline","color_space","width","height"]:
  if cams[0][key]!=cams[1][key]:raise RuntimeError("A/B camera/render mismatch "+view+" "+key)
fg_a=evidence/"final3d-foreground-fingerprint.txt";fg_b=evidence/"final25d-foreground-fingerprint.txt"
if fg_a.read_bytes()!=fg_b.read_bytes():raise RuntimeError("Foreground geometry/material changed between A and B")
guids=set()
for meta in (p for search in [root/"Unity/Assets",root/"Unity/Library/PackageCache",root/"Unity/Packages"] if search.exists() for p in search.rglob("*.meta")):
 try:
  found=re.search(r"^guid: ([a-f0-9]{32})$",meta.read_text(errors="ignore"),re.M)
  if found:guids.add(found[1])
 except OSError:pass
scenes=[]
for filename in ["BastionI_Blockout.unity","BastionI_Hybrid3D_Final.unity","BastionI_Hybrid25D_Final.unity","BastionI_FrozenBaseline_MatchedReview.unity"]:
 p=study/filename;text=p.read_text();references=set(re.findall(r"guid: ([a-f0-9]{32})",text))
 missing=sorted(g for g in references if g not in guids and not g.startswith("0000000"))
 if missing:raise RuntimeError("Scene unresolved GUIDs "+filename+" "+str(missing))
 scenes.append({"file":filename,"sha256":sha(p),"asset_guids_resolved":True})
result={"schema":"eldoria.visual-breakthrough.exact-evidence.v1","native_pngs":images,"scenes":scenes,"identical_a_b_foreground":True,"foreground_fingerprint_sha256":sha(fg_a),"same_a_b_camera_and_pipeline":True,"art_approved":False,"physical_mobile_performance_measured":False,"paid_credits":0}
(evidence/"exact-evidence-manifest.json").write_text(json.dumps(result,indent=2))
print("EXACT_NATIVE_EVIDENCE_PASS",len(images),"frames",len(scenes),"portable scenes; NOT artistic acceptance")
