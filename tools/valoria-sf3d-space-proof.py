import json, os, shutil, sys
from PIL import Image
from gradio_client import Client, handle_file

ROOT="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"
OUT=os.environ.get("OUT_DIR","/tmp/sf3d-proof")
os.makedirs(OUT,exist_ok=True)

back=Image.open(os.path.join(ROOT,"gate-back.png")).convert("RGBA")
front=Image.open(os.path.join(ROOT,"gate-front.png")).convert("RGBA")
img=Image.alpha_composite(back,front)
bbox=img.getbbox()
if not bbox:
    raise SystemExit("Gate semantic input has empty alpha")
img=img.crop(bbox)
w,h=img.size
side=max(w,h)
canvas=Image.new("RGBA",(side,side),(0,0,0,0))
canvas.alpha_composite(img,((side-w)//2,(side-h)//2))
inp=os.path.join(OUT,"gate-input.png")
canvas.save(inp)
print("INPUT",inp,canvas.size,bbox)

client=Client("stabilityai/stable-fast-3d")
api=client.view_api(return_format="dict")
open(os.path.join(OUT,"api.json"),"w").write(json.dumps(api,indent=2,default=str))
print(json.dumps(api,indent=2,default=str)[:12000])

prep=client.predict(handle_file(inp),0.85,api_name="/requires_bg_remove")
print("PREP",repr(prep))
if not isinstance(prep,(list,tuple)) or len(prep)<3:
    raise RuntimeError("Unexpected requires_bg_remove output")
bg=prep[2]
def as_file(v):
    if isinstance(v,str) and os.path.exists(v): return handle_file(v)
    if isinstance(v,dict):
        p=v.get("path") or v.get("name")
        if p and os.path.exists(p): return handle_file(p)
    return handle_file(inp)

res=client.predict("Run",handle_file(inp),as_file(bg),0.85,"Triangle",-1,1024,api_name="/run_button")
print("RUN",repr(res))
open(os.path.join(OUT,"result.json"),"w").write(json.dumps(res,indent=2,default=str))
cands=[]
def walk(v):
    if isinstance(v,str):
        if v.endswith(".glb") and os.path.exists(v): cands.append(v)
    elif isinstance(v,dict):
        for x in v.values(): walk(x)
    elif isinstance(v,(list,tuple)):
        for x in v: walk(x)
walk(res)
if not cands:
    raise RuntimeError("No local GLB returned by Space")
shutil.copy2(cands[0],os.path.join(OUT,"sf3d-gate.glb"))
print("GLB",os.path.getsize(os.path.join(OUT,"sf3d-gate.glb")))
