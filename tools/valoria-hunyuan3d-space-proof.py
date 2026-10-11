import json, os, shutil
from PIL import Image
from gradio_client import Client, handle_file

ROOT="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"
OUT=os.environ.get("OUT_DIR","/tmp/hunyuan-proof")
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

client=Client("tencent/Hunyuan3D-2.1")
api=client.view_api(return_format="dict")
open(os.path.join(OUT,"api.json"),"w").write(json.dumps(api,indent=2,default=str))
names=list(api.get("named_endpoints",{}).keys()) if isinstance(api,dict) else []
print("ENDPOINTS",names)
ep=None
for cand in ["/generation_all","/shape_generation"]:
    if cand in names: ep=cand; break
if ep is None:
    for n in names:
        if "generation_all" in n or "shape_generation" in n:
            ep=n; break
if ep is None: raise RuntimeError("No Hunyuan generation endpoint exposed")
print("USING",ep)

args=[handle_file(inp),None,None,None,None,10,5.0,1234,256,True,8000,False]
res=client.predict(*args,api_name=ep)
print("RESULT",repr(res))
open(os.path.join(OUT,"result.json"),"w").write(json.dumps(res,indent=2,default=str))

files=[]
def walk(v):
    if isinstance(v,str):
        if os.path.exists(v) and os.path.isfile(v): files.append(v)
    elif isinstance(v,dict):
        p=v.get("path") or v.get("name")
        if isinstance(p,str) and os.path.exists(p): files.append(p)
        for x in v.values(): walk(x)
    elif isinstance(v,(list,tuple)):
        for x in v: walk(x)
walk(res)
seen=set()
for p in files:
    if p in seen: continue
    seen.add(p)
    ext=os.path.splitext(p)[1].lower()
    if ext in (".glb",".gltf",".obj",".ply",".stl"):
        dst=os.path.join(OUT,"hunyuan-gate"+ext)
        shutil.copy2(p,dst)
        print("MODEL",dst,os.path.getsize(dst))
        break
else:
    raise RuntimeError("No 3D model file returned")
