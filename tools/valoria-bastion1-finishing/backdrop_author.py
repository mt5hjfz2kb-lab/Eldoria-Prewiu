"""Ask the existing zero-cost Qwen vision artist to author a bounded mountain and valley palette revision."""
import argparse,base64,hashlib,json,pathlib,time,urllib.request
from PIL import Image
p=argparse.ArgumentParser();p.add_argument('--root',type=pathlib.Path,required=True)
r=p.parse_args().root
pictures=[]
for name in ("reference","unity"):
 im=Image.open(r/(name+".png")).convert("RGB")
 im.thumbnail((640,480),Image.Resampling.LANCZOS)
 path=r/(name+"-vision.jpg")
 im.save(path,quality=85)
 pictures.append(base64.b64encode(path.read_bytes()).decode())
brief="""You are Eldoria's genuine 3D medieval landscape artist. Image 1: approved Valoria target. Image 2: REAL Unity before frame. A prior attempt simply placed distant existing mountains and trees and was VISUALLY REJECTED (floating trees and worse flat triangulated landscape). Do not request new buildings or more prefabs. Instead author an actual artistic modification to a precise existing source mesh, Valoria true mountain backdrop: 9896 vertices, 3300 triangles, bounds X -175..175, Y -195..-40, height -28..55 metres. The mountain is currently flat, ugly gray polygon background. Its existing textured CC0 rock PBR and forest valley PBR material sources may be reassigned on the SAME geometry, while adjusting a broad real mountain ridge and face smoothing. Also author a restrained water color revision for the two existing flat cyan water channel meshes. Bastion, cliffs, terrain, all buildings, road, UVs of non-target meshes and gameplay are IMMUTABLE. This needs realistic atmospheric depth and coherent cool mountains/forest colors against the warm stone fortress, without outshining the hero gate. Author one JSON tool plan; the executor will enact EXACT parameters and preserve your raw response, Blender source, hashes and matched Unity A/B. Do not claim ART_PASS. Return ONLY a JSON object with: diagnosis (at least 50 chars), rationale (at least 50 chars), operation exactly 'refine_existing_mountain_and_water', parameters with EXACT fields ridge_amplitude_m (2..16), ridge_wavelength_m (18..90), ridge_phase_radians (0..6.283), secondary_ridge_m (0.5..5), mountain_tree_line_ratio (0.22..0.65; fraction of vertical relief carrying existing forest-ground material), rock_uv_scale (0.65..3), forest_uv_scale (0.65..3), water_rgb (3 numbers; red 0.015..0.11, green 0.05..0.22, blue 0.09..0.32), water_roughness (0.12..0.6), smooth_mountain_faces (boolean). Values must be concrete choices made from BOTH images, not boilerplate. A real Unity screenshot and independent visual veto follow."""
bounds={"ridge_amplitude_m":(2,16),"ridge_wavelength_m":(18,90),"ridge_phase_radians":(0,6.283),"secondary_ridge_m":(.5,5),"mountain_tree_line_ratio":(.22,.65),"rock_uv_scale":(.65,3),"forest_uv_scale":(.65,3),"water_roughness":(.12,.6)}
for attempt in (1,2):
 payload={"model":"qwen3-vl:8b-instruct","stream":False,"format":"json","keep_alive":"3m","messages":[{"role":"user","content":brief,"images":pictures}],"options":{"num_ctx":8192,"num_predict":1000,"temperature":.2}}
 t=time.monotonic()
 req=urllib.request.Request("http://127.0.0.1:11434/api/chat",json.dumps(payload).encode(),{"Content-Type":"application/json"})
 with urllib.request.urlopen(req,timeout=540) as result:raw=result.read()
 (r/f"raw-mountain-attempt-{attempt}.json").write_bytes(raw)
 try:
  plan=json.loads(json.loads(raw)["message"]["content"])
  assert plan["operation"]=="refine_existing_mountain_and_water"
  assert len(plan["diagnosis"])>=50 and len(plan["rationale"])>=50
  q=plan["parameters"]
  assert set(q)==set(bounds)|{"water_rgb","smooth_mountain_faces"},list(q)
  assert all(type(q[k]) in (float,int) and lo<=q[k]<=hi for k,(lo,hi) in bounds.items())
  assert type(q["smooth_mountain_faces"]) is bool
  assert type(q["water_rgb"]) is list and len(q["water_rgb"])==3
  assert all(type(q["water_rgb"][i]) in (float,int) and a<=q["water_rgb"][i]<=b for i,(a,b) in enumerate(((.015,.11),(.05,.22),(.09,.32))))
  (r/"mountain-plan.json").write_text(json.dumps(plan,indent=2)+"\n")
  proof={"model":"qwen3-vl:8b-instruct","source":"genuine_realtime_vision","raw_sha256":hashlib.sha256(raw).hexdigest(),"plan_sha256":hashlib.sha256((r/"mountain-plan.json").read_bytes()).hexdigest(),"attempt":attempt,"duration_sec":time.monotonic()-t,"parameters_modified":False,"unity_pass":False,"art_pass":False}
  (r/"mountain-inference-proof.json").write_text(json.dumps(proof,indent=2)+"\n")
  print("BASTION1_MOUNTAIN_QWEN_PLAN_PASS",json.dumps(plan))
  break
 except (KeyError,ValueError,AssertionError,TypeError) as e:
  (r/f"mountain-reject-{attempt}.txt").write_text(str(e))
  brief+=" Previous response invalid: "+str(e)+". Repair the JSON without weakening scope or bounds."
else:raise RuntimeError("Two real Qwen replies failed strict plan validation; no substitute artist plan was invented")
