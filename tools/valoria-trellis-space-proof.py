import json, os, shutil
from PIL import Image
from gradio_client import Client, handle_file

ROOT="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"
OUT=os.environ.get("OUT_DIR","/tmp/trellis-proof")
os.makedirs(OUT,exist_ok=True)
back=Image.open(os.path.join(ROOT,"gate-back.png")).convert("RGBA")
front=Image.open(os.path.join(ROOT,"gate-front.png")).convert("RGBA")
img=Image.alpha_composite(back,front)
bbox=img.getbbox()
if not bbox: raise RuntimeError("empty Gate alpha")
img=img.crop(bbox)
side=max(img.size)
canvas=Image.new("RGBA",(side,side),(0,0,0,0))
canvas.alpha_composite(img,((side-img.width)//2,(side-img.height)//2))
inp=os.path.join(OUT,"gate-input.png"); canvas.save(inp)

client=Client("trellis-community/TRELLIS")
api=client.view_api(return_format="dict")
open(os.path.join(OUT,"api.json"),"w").write(json.dumps(api,indent=2,default=str))
names=list(api.get("named_endpoints",{}).keys())
print("ENDPOINTS",names)
ep=next((n for n in names if "generate" in n.lower() and ("glb" in n.lower() or "extract" in n.lower())),None)
if ep is None:
    ep=next((n for n in names if "generate" in n.lower()),None)
if ep is None: raise RuntimeError("No TRELLIS generation endpoint")
spec=api["named_endpoints"][ep]
print("USING",ep)
print(json.dumps(spec,indent=2,default=str))
args=[]
for p in spec.get("parameters",[]):
    name=(p.get("parameter_name") or p.get("label") or "").lower()
    default=p.get("parameter_default")
    has=p.get("parameter_has_default",False)
    if name in ("image","image_prompt") or ("image" in name and "multi" not in name):
        args.append(handle_file(inp))
    elif "is_multi" in name:
        args.append(False)
    elif "multiimage" in name or "multi_image" in name or "gallery" in name:
        args.append([])
    elif "seed"==name or name.endswith("_seed"):
        args.append(0)
    elif "random" in name:
        args.append(False)
    elif "ss_guidance" in name:
        args.append(7.5)
    elif "ss_sampling" in name or ("sparse" in name and "step" in name):
        args.append(12)
    elif "slat_guidance" in name:
        args.append(3.0)
    elif "slat_sampling" in name or ("latent" in name and "step" in name):
        args.append(12)
    elif "algo" in name:
        args.append("stochastic")
    elif "simpl" in name:
        args.append(0.95)
    elif "texture" in name:
        args.append(1024)
    elif has:
        args.append(default)
    else:
        raise RuntimeError(f"Unhandled required param: {p}")
print("ARGS",[(type(a).__name__,str(a)[:80]) for a in args])
res=client.predict(*args,api_name=ep)
print("RESULT",repr(res))
open(os.path.join(OUT,"result.json"),"w").write(json.dumps(res,indent=2,default=str))
models=[]
def walk(v):
    if isinstance(v,str) and v.lower().endswith(".glb") and os.path.exists(v): models.append(v)
    elif isinstance(v,dict):
        p=v.get("path") or v.get("name")
        if isinstance(p,str) and p.lower().endswith(".glb") and os.path.exists(p): models.append(p)
        for x in v.values(): walk(x)
    elif isinstance(v,(list,tuple)):
        for x in v: walk(x)
walk(res)
if not models: raise RuntimeError("No local GLB returned by TRELLIS")
shutil.copy2(models[0],os.path.join(OUT,"trellis-gate.glb"))
print("GLB",os.path.getsize(os.path.join(OUT,"trellis-gate.glb")))
