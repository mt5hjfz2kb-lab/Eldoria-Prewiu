import json, os, shutil
from PIL import Image
from gradio_client import Client, handle_file

ROOT="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"
OUT=os.environ.get("OUT_DIR","/tmp/craftsman-proof")
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

client=Client("multimodalart/CraftsMan3D-zerogpu")
api=client.view_api(return_format="dict")
open(os.path.join(OUT,"api.json"),"w").write(json.dumps(api,indent=2,default=str))
eps=api.get("named_endpoints",{}) if isinstance(api,dict) else {}
print("ENDPOINTS",list(eps))
ep="/generate_img2obj"
if ep not in eps:
    raise RuntimeError("CraftsMan /generate_img2obj endpoint unavailable")
res=client.predict(
    handle_file(inp),
    [],
    "DDIMScheduler",
    7.5,
    50,
    42,
    12000,
    8,
    api_name=ep,
)
print("RESULT",repr(res))
open(os.path.join(OUT,"result.json"),"w").write(json.dumps({"endpoint":ep,"result":res},indent=2,default=str))
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
    ext=os.path.splitext(p)[1].lower()
    if ext in (".glb",".gltf",".obj",".ply",".stl"):
        dst=os.path.join(OUT,"craftsman-gate"+ext)
        shutil.copy2(p,dst)
        print("MODEL",dst,os.path.getsize(dst))
        break
else:
    raise RuntimeError("CraftsMan returned no model file")
