import json, os, shutil
from PIL import Image
from gradio_client import Client, handle_file

ROOT="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"
OUT=os.environ.get("OUT_DIR","/tmp/qwen-angles-proof")
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

client=Client("multimodalart/qwen-image-multiple-angles-3d-camera")
api=client.view_api(return_format="dict")
open(os.path.join(OUT,"api.json"),"w").write(json.dumps(api,indent=2,default=str))
eps=api.get("named_endpoints",{}) if isinstance(api,dict) else {}
print("ENDPOINTS",list(eps))

def build_args(spec, azimuth):
    args=[]
    for p in spec.get("parameters",[]):
        comp=p.get("component"); pname=str(p.get("parameter_name","")).lower(); label=str(p.get("label","")).lower()
        if comp=="Image" or ("image" in pname and "num" not in pname):
            args.append(handle_file(inp))
        elif "azimuth" in pname or "horizontal" in label or "yaw" in pname:
            args.append(azimuth)
        elif "elevation" in pname or "vertical" in label or "pitch" in pname:
            args.append(0)
        elif "distance" in pname or "zoom" in pname:
            args.append(1.0)
        elif "randomize" in pname:
            args.append(False)
        elif "seed" in pname:
            args.append(1234)
        elif p.get("parameter_has_default"):
            args.append(p.get("parameter_default"))
        elif comp=="Checkbox": args.append(False)
        elif comp in ("Slider","Number"): args.append(0)
        elif comp=="Textbox": args.append("")
        elif comp=="Dropdown":
            choices=p.get("choices") or []
            args.append(choices[0] if choices else None)
        else: args.append(None)
    return args

cands=[]
for name,spec in eps.items():
    ps=spec.get("parameters",[]); rs=spec.get("returns",[])
    if any(p.get("component")=="Image" for p in ps) and any(r.get("component")=="Image" for r in rs):
        cands.append((name,spec))
if not cands: raise RuntimeError("No image edit endpoint")

for az,label in [(45,"quarter"),(90,"side"),(180,"back")]:
    ok=False; last=None
    for name,spec in cands:
        try:
            args=build_args(spec,az)
            print("TRY",label,name,args)
            res=client.predict(*args,api_name=name)
            print("RESULT",label,repr(res))
            files=[]
            def walk(v):
                if isinstance(v,str) and os.path.exists(v) and os.path.isfile(v): files.append(v)
                elif isinstance(v,dict):
                    q=v.get("path") or v.get("name")
                    if isinstance(q,str) and os.path.exists(q): files.append(q)
                    for x in v.values(): walk(x)
                elif isinstance(v,(list,tuple)):
                    for x in v: walk(x)
            walk(res)
            p=next((x for x in files if os.path.splitext(x)[1].lower() in (".png",".jpg",".jpeg",".webp")),None)
            if p:
                ext=os.path.splitext(p)[1].lower()
                shutil.copy2(p,os.path.join(OUT,label+ext))
                ok=True; break
        except Exception as e:
            last=repr(e); print("FAILED",label,name,last)
    if not ok: raise RuntimeError(f"No {label} output: {last}")
