"""Qwen-authored source-only Brecha landform: typed curved paths, no scripted terrain shapes."""
import argparse,base64,copy,hashlib,json,pathlib,sys,time,urllib.request
from PIL import Image
root=pathlib.Path(__file__).resolve().parents[3]
sys.path.insert(0,str(root/"tools/art-direction/ai-authoring"))
from author_brief import cpu_artist_schema
from artist_plan_validation import inspect_artist_plan
p=argparse.ArgumentParser()
p.add_argument("--out",type=pathlib.Path,required=True)
p.add_argument("--reference",type=pathlib.Path,required=True)
p.add_argument("--real-before",type=pathlib.Path,required=True)
p.add_argument("--mesh-context",type=pathlib.Path,required=True)
a=p.parse_args()
a.out.mkdir(parents=True,exist_ok=False)
sha=lambda f:hashlib.sha256(pathlib.Path(f).read_bytes()).hexdigest()
top=json.loads(a.mesh_context.read_text())
target="ZoneOne continuous prairie source"
assert top["target"]==target and top["vertices"]>=14000
role=(root/"tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md").read_text()
source_review=(root/"docs/world4x-zone1/GROUND_ART_REVIEW_20261014.json").read_text()
b64=[]
images=[]
for name,file in [("destination",a.reference),("actual_before",a.real_before)]:
 im=Image.open(file).convert("RGB");im.thumbnail((480,480))
 frame=a.out/(name+".jpg");im.save(frame,quality=82)
 images.append({"name":name,"raw_sha256":sha(file),"model_image_sha256":sha(frame)})
 b64.append(base64.b64encode(frame.read_bytes()).decode())
schema=cpu_artist_schema(target,"existing",2)
vec={"type":"array","minItems":3,"maxItems":3,"items":{"type":"number"}}
xyz={"type":"array","minItems":3,"maxItems":3,"prefixItems":[
 {"type":"number","minimum":-72,"maximum":72},
 {"type":"number","minimum":32,"maximum":86},
 {"type":"number","minimum":-5,"maximum":7}]}
delta={"type":"array","minItems":3,"maxItems":3,"prefixItems":[
 {"const":0},{"const":0},{"type":"number","minimum":-14,"maximum":12}]}
knot={"type":"object","additionalProperties":False,"properties":{
 "center":xyz,"delta":delta,"radius":{"type":"number","minimum":18,"maximum":30}},
 "required":["center","delta","radius"]}
op={"type":"object","additionalProperties":False,"properties":{
 "id":{"type":"string","minLength":4,"maxLength":32},
 "type":{"const":"sculpt_path"},"target":{"const":target},
 "rationale":{"type":"string","minLength":20,"maxLength":190},
 "parameters":{"type":"object","additionalProperties":False,"properties":{
  "knots":{"type":"array","minItems":3,"maxItems":4,"items":knot}},
  "required":["knots"]}},
 "required":["id","type","target","rationale","parameters"]}
schema["properties"]["planned_actions"]={"type":"array","minItems":2,"maxItems":2,"items":op}
prompt=("ELDORIA / ZONE ONE / BRECHA: two real images are owner's approved world reference and the current actual Blender A/B result. "
"The current image has flat uniform carpet grass and beige painted paths. Independent visual inspection REJECTED it, despite 1957 changed vertices. "
"The user wants a SMALL convincing terrain sample near the Brecha, built from the ground up. Preserve this existing 240x240m source, "
"all its textures/UVs, city building pad x=-40..4 y=-52..-8, and outer boundary. Do not make architecture, assets, objects, roads or another world. "
"YOU ARE THE ARTIST: author exactly TWO genuinely different CURVED sculpt_path landform decisions for the actual 'ZoneOne continuous prairie source' mesh. "
"One is a distinct winding asymmetric BRECHA depression/chasm channel with Z offsets reaching -9..-14m; "
"the other is an offset companion rising fractured shoulder/rim using Z offsets reaching +7..+12m. "
"Three or four artist-chosen knots per sculpt_path, 18..30 meter radii, center X=-72..72 Y=32..86 Z=-5..7; [dx,dy]=[0,0]. "
"Make visually connected, irregular long curves rather than circular pimples or ruler-straight mountains. "
"Land around it remains an open buildable meadow; do not reprofile city or edges. "
"Place centers >= radius+2m inside the +/-118m protected boundaries and away from city pad. "
"Paths should each bend noticeably (not all Y equal). Prefer different locations and widths for the chasm floor and its geological flank. "
"Use the source topology and past failures, but DESIGN ORIGINAL EXACT NUMERIC KNOTS, not a fixed formula. "
"This cycle is geometry only. If results still look artificial we must reject, not self-approve. "
"Use exactly valid JSON described by your schema, no chatter. Existing source AABB / topology: "+
json.dumps({k:top[k] for k in ("target","vertices","faces","local_aabb","sample_vertices") if k in top})[:4000]+
"\nPast independently rejected imagery: "+source_review[-2100:])
payload={"model":"qwen3-vl:4b-instruct","think":False,"stream":True,"format":schema,
 "options":{"num_ctx":12288,"num_predict":2400,"temperature":.45},
 "messages":[{"role":"system","content":role},{"role":"user","content":prompt,"images":b64}]}
for attempt in range(3):
 (a.out/("request-"+str(attempt+1)+".json")).write_text(json.dumps(payload))
 parts=[];done=False;started=time.monotonic()
 request=urllib.request.Request("http://127.0.0.1:11434/api/chat",
  data=json.dumps(payload).encode(),headers={"Content-Type":"application/json"})
 with urllib.request.urlopen(request,timeout=540) as response, (a.out/("qwen-stream-"+str(attempt+1)+".jsonl")).open("w") as trace:
  for line in response:
   if not line.strip():continue
   trace.write(line.decode());trace.flush()
   chunk=json.loads(line)
   if chunk.get("error"):raise RuntimeError(chunk["error"])
   parts.append(chunk.get("message",{}).get("content",""))
   if chunk.get("done"):done=True;break
 if not done:raise RuntimeError("Incomplete Qwen plan stream")
 raw="".join(parts);(a.out/("qwen-raw-"+str(attempt+1)+".json")).write_text(raw)
 errors=[]
 try:
  plan=json.loads(raw)
  errors.extend(inspect_artist_plan(plan,target,"existing",2,topology=top))
  actions=plan["planned_actions"]
  if len(actions)!=2 or len({v.get("id") for v in actions})!=2: errors.append("Exactly two unique path operations")
  effects=[]
  for act in actions:
   if act["type"]!="sculpt_path" or act["target"]!=target:errors.append("Only source sculpt_path")
   knots=act["parameters"]["knots"]
   if not 3<=len(knots)<=4:errors.append("3..4 knots per path")
   cross=0
   for i in range(len(knots)-2):
    p0,p1,p2=[k["center"] for k in knots[i:i+3]]
    cross=max(cross,abs((p1[0]-p0[0])*(p2[1]-p0[1])-(p1[1]-p0[1])*(p2[0]-p0[0])))
   if cross<130:errors.append("Artist path must have meaningful curvature, not straight")
   for k in knots:
    x,y,z=k["center"];dx,dy,dz=k["delta"];rad=k["radius"]
    if not(-72<=x<=72 and 32<=y<=86 and -5<=z<=7 and 18<=rad<=30):errors.append("Knot center/radius out of ground-safe bounds")
    if dx!=0 or dy!=0 or not -14<=dz<=12:errors.append("Only vertical displacement")
    if abs(x)+rad>=118 or abs(y)+rad>=118:errors.append("Path intersects protected boundary")
    if -40-rad < x < 4+rad and -52-rad < y < -8+rad:errors.append("Path intersects city pad")
    effects.append(dz)
  if not any(z<=-9 for z in effects) or not any(z>=7 for z in effects):errors.append("Need one actual deep cut and one expressive shoulder")
 except Exception as e:
  errors.append("Malformed plan: "+str(e))
 (a.out/("validation-"+str(attempt+1)+".json")).write_text(json.dumps({"errors":errors,"qwen_sha256":hashlib.sha256(raw.encode()).hexdigest(),
  "elapsed_seconds":round(time.monotonic()-started,3)},indent=2))
 if not errors:
  (a.out/"artist-plan.json").write_text(json.dumps(plan["planned_actions"],indent=2))
  (a.out/"genuine-ai-evidence.json").write_text(json.dumps({"ai_model":payload["model"],"source_images":images,
   "plan_sha256":sha(a.out/"artist-plan.json"),"original_inference":plan,
   "model_weight_training":False,"blender_operations_executed_yet":False},indent=2))
  print("BRECHA_QWEN_ART_DIRECTOR_ACCEPTED",attempt+1,flush=True)
  break
 if attempt==2:raise RuntimeError("Qwen submitted 3 invalid original plans: "+str(errors))
 payload["messages"]=payload["messages"][:2]+[{"role":"user","content":"The previous ORIGINAL plan was rejected; do not repeat it. Repair your own design for these precise faults: "+json.dumps(errors)+". Two curved sculpt_path actions; truly deep and raised changes, protected city/border and source verified."}]
