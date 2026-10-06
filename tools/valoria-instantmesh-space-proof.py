import json, os, shutil
from PIL import Image
from gradio_client import Client, handle_file

ROOT="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"
OUT=os.environ.get("OUT_DIR","/tmp/instantmesh-proof")
os.makedirs(OUT,exist_ok=True)
back=Image.open(os.path.join(ROOT,"gate-back.png")).convert("RGBA")
front=Image.open(os.path.join(ROOT,"gate-front.png")).convert("RGBA")
img=Image.alpha_composite(back,front)
bbox=img.getbbox()
if not bbox: raise RuntimeError("empty gate")
img=img.crop(bbox)
side=max(img.size)
canvas=Image.new("RGBA",(side,side),(0,0,0,0))
canvas.alpha_composite(img,((side-img.width)//2,(side-img.height)//2))
inp=os.path.join(OUT,"gate-input.png"); canvas.save(inp)

client=Client("TencentARC/InstantMesh")
api=client.view_api(return_format="dict")
open(os.path.join(OUT,"api.json"),"w").write(json.dumps(api,indent=2,default=str))
names=list(api.get("named_endpoints",{}).keys()) if isinstance(api,dict) else []
print("ENDPOINTS",names)

def find(fragment):
    for n in names:
        if fragment in n: return n
    return None

ep_pre=find("preprocess")
ep_mvs=find("generate_mvs")
ep_3d=find("make3d")
if not (ep_pre and ep_mvs and ep_3d):
    raise RuntimeError(f"Missing endpoints preprocess={ep_pre} mvs={ep_mvs} make3d={ep_3d}")

processed=client.predict(handle_file(inp),False,api_name=ep_pre)
print("PROCESSED",repr(processed))
mv=client.predict(processed,30,42,api_name=ep_mvs)
print("MVS",repr(mv))
open(os.path.join(OUT,"mvs-result.json"),"w").write(json.dumps(mv,indent=2,default=str))
state=mv[0] if isinstance(mv,(list,tuple)) else mv
res=client.predict(state,api_name=ep_3d)
print("MESH",repr(res))
open(os.path.join(OUT,"result.json"),"w").write(json.dumps(res,indent=2,default=str))
files=[]
def walk(v):
    if isinstance(v,str) and os.path.exists(v) and os.path.isfile(v): files.append(v)
    elif isinstance(v,dict):
        p=v.get("path") or v.get("name")
        if isinstance(p,str) and os.path.exists(p): files.append(p)
        for x in v.values(): walk(x)
    elif isinstance(v,(list,tuple)):
        for x in v: walk(x)
walk(res)
for p in files:
    if os.path.splitext(p)[1].lower() in (".glb",".obj"):
        shutil.copy2(p,os.path.join(OUT,"instantmesh-gate"+os.path.splitext(p)[1].lower()))
        print("MODEL",p,os.path.getsize(p))
if not any(x.endswith((".glb",".obj")) for x in os.listdir(OUT)): raise RuntimeError("No model returned")
