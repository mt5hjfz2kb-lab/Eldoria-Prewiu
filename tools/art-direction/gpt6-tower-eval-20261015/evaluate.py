"""Independent local visual review and measurable A/B. Never self-approves art."""
from __future__ import annotations
import argparse,base64,hashlib,json,os,pathlib,time,urllib.request,urllib.error
from PIL import Image,ImageChops,ImageStat
p=argparse.ArgumentParser()
for k in ("out","reference","plan","source"):p.add_argument("--"+k,required=True)
p.add_argument("--model",default="qwen3-vl:8b-instruct-q4_K_M")
a=p.parse_args()
out=pathlib.Path(a.out);before=out/"before-blender.png";after=out/"after-blender.png"
source=pathlib.Path(a.source);reference=pathlib.Path(a.reference);plan=pathlib.Path(a.plan)
data=json.loads((out/"execution-proof.json").read_text())
from PIL import Image
with Image.open(before) as i,Image.open(after) as j:
 x=i.convert("RGB");y=j.convert("RGB")
 assert x.size==y.size,"CAMERA_RESOLUTION_MISMATCH"
 import numpy as np
 diff=np.abs(np.asarray(x,dtype=np.int16)-np.asarray(y,dtype=np.int16))
 meaningful=np.max(diff,axis=2)>12
 pixels=int(meaningful.sum());pct=round(100*pixels/meaningful.size,4)
 regions=[]
 for item in json.loads((pathlib.Path(__file__).resolve().parents[1]/"full-studio/tests/valoria_tower_hero_review.json").read_text())["protected_screen_regions"]:
  box=item["box_px"];m=meaningful[box[1]:box[3],box[0]:box[2]]
  regions.append({"name":item["name"],"pixel_changed_fraction":round(float(m.mean()),4),"max_changed_fraction":item["max_changed_fraction"],"pass":bool(m.mean()<=item["max_changed_fraction"])})
report={"source_sha256":hashlib.sha256(source.read_bytes()).hexdigest(),"plan_sha256":hashlib.sha256(plan.read_bytes()).hexdigest(),"before_sha256":hashlib.sha256(before.read_bytes()).hexdigest(),"after_sha256":hashlib.sha256(after.read_bytes()).hexdigest(),"same_camera":data.get("matched_review_camera"),"blender_execution":data.get("blender_execution"),"original_mesh_modified":data.get("inherited_mesh_data_changed"),"pixels_changed_gt12":pixels,"pixels_changed_pct":pct,"operational_visibility_gate_0_6pct":pct>=0.6,"protected_regions":regions,"contact_proof_present":(out/"actual-mesh-contact.json").is_file(),"independent_art_approved":False,"art_approved":False}
if (out/"actual-mesh-contact.json").is_file():
 report["contact_proof"]=json.loads((out/"actual-mesh-contact.json").read_text())
(out/"objective-review.json").write_text(json.dumps(report,indent=2)+"\n")
print("ELDORIA_GPT6_OBJECTIVE_REVIEW="+json.dumps({k:v for k,v in report.items() if k not in ("contact_proof",)}))
# A separate local VISUAL model, not the creator, reviews the true images.
# It is a provisional critic, never ART_APPROVED or Unity evidence.
def imgdata(path):
 from io import BytesIO
 im=Image.open(path).convert("RGB");im.thumbnail((700,700))
 stream=BytesIO();im.save(stream,format="JPEG",quality=76)
 return base64.b64encode(stream.getvalue()).decode("ascii")
q={"model":a.model,"stream":True,"think":False,"options":{"temperature":0.2,"num_ctx":8192,"num_predict":850},"messages":[{"role":"system","content":"You are an independent critical visual art reviewer. You did NOT create this asset. Images are authentic. NEVER self-approve; do not conflate change with quality. Judge fidelity to Valoria medieval style, timber contact, silhouette, compositional coherency, stair/door protection and game camera readability. Respond in JSON without markdown."},{"role":"user","content":"Three actual images: (1) owner's approved large-scale aesthetic reference, (2) original frozen wooden Valoria watchtower from fixed Blender review camera, (3) edited result at same camera. Do NOT confuse reference's stone castle with tower's timber material. Did new fascia and diagonal supports improve the tower? Are supports visibly physically attached or floating, does image show bad rectangular block or obstruction, are original stairs/roof safe? Give JSON object: decision one of IMPROVED,REJECTED,INCONCLUSIVE; reasons array; categories 0-10 geometry_silhouette,architectural_contact,materials,style,player_camera; flag_uncertainties array. This is provisional Blender visual opinion ONLY and NEVER Unity approval.","images":[imgdata(reference),imgdata(before),imgdata(after)]}]}
req=urllib.request.Request("http://127.0.0.1:11434/api/chat",headers={"Content-Type":"application/json"},data=json.dumps(q).encode())
chunks=[];done=False
try:
 with urllib.request.urlopen(req,timeout=550) as f:
  for line in f:
   if not line.strip():continue
   d=json.loads(line)
   if d.get("error"):raise RuntimeError(str(d["error"]))
   chunks.append(d.get("message",{}).get("content",""))
   if d.get("done"):
    done=True;break
except Exception as e:
 (out/"independent-critic-error.json").write_text(json.dumps({"error":str(e),"model":a.model,"art_approved":False},indent=2)+"\n")
 print("ELDORIA_INDEPENDENT_REVIEW_UNAVAILABLE "+str(e))
else:
 answer="".join(chunks)
 evidence={"model":a.model,"complete":done,"raw_text":answer,"image_sha256":report["before_sha256"]+" "+report["after_sha256"],"art_approved":False}
 (out/"independent-critic-raw.json").write_text(json.dumps(evidence,ensure_ascii=False,indent=2)+"\n")
 try:
  verdict=json.loads(answer)
  print("ELDORIA_INDEPENDENT_VISUAL_VERDICT="+json.dumps(verdict,ensure_ascii=True)[:4500])
 except ValueError:
  print("ELDORIA_INDEPENDENT_VISUAL_RAW="+answer[:2200].replace("\n"," "))
print("ELDORIA_COMPARISON_EVALUATION_COMPLETE_WITH_PROVISIONAL_ONLY")
