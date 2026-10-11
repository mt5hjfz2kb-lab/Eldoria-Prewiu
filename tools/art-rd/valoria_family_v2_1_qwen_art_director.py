"""Seven genuine Qwen vision decisions on actual V2 Blender renders before third artistic pass."""
import json,base64,io,urllib.request,time
from pathlib import Path
from PIL import Image
root=Path(__file__).resolve().parents[2]
out=root/"art-source/valoria/production/asset-family-v2"
ref=root/"art-source/valoria/production/first-hero-gate-v1/quality-v3/after-hero-matched.png"
assert ref.is_file()
options={
"wall_straight":("rebuild_buttress_proportions","cut_irregular_ashlar_courses"),
"watchtower":("reshape_crown_silhouette","carve_tower_relief"),
"wall_corner":("interlock_corner_stones","rebuild_corner_coping"),
"wall_connections":("carve_modular_contacts","integrate_socket_buttresses"),
"stone_path_platform":("erode_platform_perimeter","regrade_stone_path"),
"cliff_rocks":("sculpt_major_geological_shelves","cut_deep_erosion_gullies"),
"defensive_props":("resculpt_hanging_fabric","rebuild_torch_hardware")}
def encode(file):
 with Image.open(file) as image:
  image=image.convert("RGB");image.thumbnail((400,400))
  stream=io.BytesIO();image.save(stream,format="JPEG",quality=70)
  return base64.b64encode(stream.getvalue()).decode()
def ask(asset,choice):
 schema={"type":"object","properties":{
 "observation":{"type":"string"},"architectural_intent":{"type":"string"},
 "operation":{"type":"string","enum":list(choice)},
 "strength":{"type":"number","minimum":0.25,"maximum":0.85}},
 "required":["observation","architectural_intent","operation","strength"],"additionalProperties":False}
 prompt=("You are responsible for REWORK of ONE real 3D architectural asset: "+asset+
 ". Compare actual image 1 V3 historical reference quality, image 2 current hero, image 3 current strategic view. "+
 "The current model is visibly REJECTED as prototype and must be geometrically improved in Blender. "+
 "Choose the artistically stronger substantial change from "+choice[0]+" OR "+choice[1]+". "+
 "Specify a concrete visible defect and why this operation can solve it. "+
 "Avoid generic descriptions of medieval castles. Operation must affect 3D mesh shapes, not only color. "+
 "Return strict JSON observation, architectural_intent, operation, strength 0.25-0.85.")
 payload={"model":"qwen2.5vl:3b","stream":False,"think":False,"format":schema,
 "options":{"temperature":0.0,"num_predict":280,"num_ctx":4096},
 "messages":[{"role":"user","content":prompt,"images":[encode(ref),encode(out/asset/"v2-after-hero.png"),encode(out/asset/"v2-after-strategic.png")]}]}
 traces=[]
 for retry in range(2):
  try:
   request=urllib.request.Request("http://127.0.0.1:11434/api/chat",data=json.dumps(payload).encode(),headers={"Content-Type":"application/json"})
   with urllib.request.urlopen(request,timeout=500) as response:result=json.load(response)
   raw=result.get("message",{}).get("content","")
   candidate=json.JSONDecoder().raw_decode(raw[raw.index("{"):])[0]
   assert candidate["operation"] in choice
   assert .25<=float(candidate["strength"])<=.85
   assert len(candidate["observation"].strip())>=12
   assert len(candidate["architectural_intent"].strip())>=12
   traces.append({"retry":retry,"model_raw_response":raw,"accepted":True})
   return candidate,traces
  except Exception as exc:
   traces.append({"retry":retry,"model_raw_response":locals().get("raw",""),"error":str(exc)})
   (out/"qwen-v2-1-raw-inference-traces.json").write_text(json.dumps({asset:traces},indent=2))
 raise RuntimeError("Real vision agent could not propose valid "+asset+" geometry decision")
plans={};traces={}
for asset,choice in options.items():
 try:
  plan,trace=ask(asset,choice)
  plans[asset]=plan;traces[asset]=trace
  (out/"qwen-v2-1-art-directions.json").write_text(json.dumps({"plans":plans,"schema":"one geometric operation per seven assets"},indent=2))
  (out/"qwen-v2-1-raw-inference-traces.json").write_text(json.dumps(traces,indent=2))
  print("REAL_QWEN_V2_1_ART_DIRECTION",asset,plan["operation"],flush=True)
 except Exception:
  (out/"qwen-v2-1-raw-inference-traces.json").write_text(json.dumps(traces,indent=2))
  raise
assert len(plans)==7
