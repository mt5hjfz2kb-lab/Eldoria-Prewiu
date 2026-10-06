import json, os, shutil
from PIL import Image
from gradio_client import Client, handle_file

OUT=os.environ.get("OUT_DIR","/tmp/marigold-proof")
os.makedirs(OUT,exist_ok=True)
src="docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg"
im=Image.open(src).convert("RGB")
im.thumbnail((1024,1024),Image.LANCZOS)
inp=os.path.join(OUT,"canonical-target.png"); im.save(inp)

client=Client("toshas/Marigold-V2")
api=client.view_api(return_format="dict")
open(os.path.join(OUT,"api.json"),"w").write(json.dumps(api,indent=2,default=str))
eps=api.get("named_endpoints",{}) if isinstance(api,dict) else {}
print("ENDPOINTS",list(eps))

cands=[]
for name,spec in eps.items():
    ps=spec.get("parameters",[]); rs=spec.get("returns",[])
    has_img=any(p.get("component")=="Image" or "image" in str(p.get("parameter_name","")).lower() for p in ps)
    returns_img=any(r.get("component")=="Image" or "depth" in str(r.get("label","")).lower() or "normal" in str(r.get("label","")).lower() for r in rs)
    if has_img and returns_img: cands.append((name,spec))
if not cands:
    for name,spec in eps.items():
        if any(p.get("component")=="Image" for p in spec.get("parameters",[])): cands.append((name,spec))
print("CANDS",[x[0] for x in cands])

last=None
for name,spec in cands:
    args=[]
    for p in spec.get("parameters",[]):
        comp=p.get("component"); pname=str(p.get("parameter_name","")).lower()
        if comp=="Image" or ("image" in pname and "num" not in pname): args.append(handle_file(inp))
        elif p.get("parameter_has_default"): args.append(p.get("parameter_default"))
        elif comp=="Checkbox": args.append(False)
        elif comp in ("Slider","Number"): args.append(0)
        elif comp=="Textbox": args.append("")
        elif comp=="Dropdown":
            choices=p.get("choices") or []
            # prefer depth-like option
            pick=next((x for x in choices if "depth" in str(x).lower()), choices[0] if choices else None)
            args.append(pick)
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
                for key in ("path","name"):
                    q=v.get(key)
                    if isinstance(q,str) and os.path.exists(q): files.append(q)
                for x in v.values(): walk(x)
            elif isinstance(v,(list,tuple)):
                for x in v: walk(x)
        walk(res)
        copied=0
        for p in files:
            ext=os.path.splitext(p)[1].lower()
            if ext in (".png",".jpg",".jpeg",".webp",".tif",".tiff",".exr",".npy",".npz"):
                shutil.copy2(p,os.path.join(OUT,f"marigold-{copied}{ext}")); copied+=1
        if copied: raise SystemExit(0)
    except SystemExit: raise
    except Exception as e:
        last=repr(e); print("FAILED",name,last)
raise RuntimeError("No Marigold output; last="+str(last))
