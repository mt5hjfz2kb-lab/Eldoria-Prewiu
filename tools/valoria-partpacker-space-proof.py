import json, os, shutil
from PIL import Image
from gradio_client import Client, handle_file

ROOT="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"
OUT=os.environ.get("OUT_DIR","/tmp/partpacker-proof")
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

client=Client("nvidia/PartPacker")
api=client.view_api(return_format="dict")
open(os.path.join(OUT,"api.json"),"w").write(json.dumps(api,indent=2,default=str))
eps=api.get("named_endpoints",{}) if isinstance(api,dict) else {}
print("ENDPOINTS",list(eps))

candidates=[]
for name,spec in eps.items():
    params=spec.get("parameters",[])
    has_img=any(p.get("component")=="Image" or "image" in str(p.get("parameter_name","")).lower() for p in params)
    if has_img: candidates.append((name,spec))
if not candidates: raise RuntimeError("No image-taking endpoints")

last=None
for name,spec in candidates:
    args=[]
    for p in spec.get("parameters",[]):
        comp=p.get("component")
        pname=str(p.get("parameter_name","")).lower()
        if comp=="Image" or ("image" in pname and "num" not in pname):
            args.append(handle_file(inp))
        elif "seed" in pname:
            args.append(1234)
        elif p.get("parameter_has_default"):
            args.append(p.get("parameter_default"))
        elif comp=="Checkbox": args.append(False)
        elif comp in ("Slider","Number"): args.append(0)
        elif comp in ("Textbox",): args.append("")
        else: args.append(None)
    try:
        print("TRY",name,args)
        res=client.predict(*args,api_name=name)
        print("RESULT",repr(res))
        open(os.path.join(OUT,"result.json"),"w").write(json.dumps({"endpoint":name,"result":res},indent=2,default=str))
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
        copied=0
        for p in files:
            ext=os.path.splitext(p)[1].lower()
            if ext in (".glb",".gltf",".obj",".ply",".stl",".zip"):
                dst=os.path.join(OUT,f"partpacker-{copied}{ext}")
                shutil.copy2(p,dst); copied+=1
        if copied: break
    except Exception as e:
        last=repr(e); print("FAILED",name,last)
else:
    raise RuntimeError("No downloadable 3D output; last="+str(last))
