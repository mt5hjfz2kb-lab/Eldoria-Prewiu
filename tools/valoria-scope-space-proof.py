import json, os, shutil
from PIL import Image
from gradio_client import Client, handle_file

ROOT="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"
OUT=os.environ.get("OUT_DIR","/tmp/scope-proof")
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

client=Client("TencentARC/scope-camera-video-generation")
api=client.view_api(return_format="dict")
open(os.path.join(OUT,"api.json"),"w").write(json.dumps(api,indent=2,default=str))
eps=api.get("named_endpoints",{}) if isinstance(api,dict) else {}
print("ENDPOINTS",list(eps))

candidates=[]
for name,spec in eps.items():
    ps=spec.get("parameters",[]); rs=spec.get("returns",[])
    has_img=any(p.get("component")=="Image" or "image" in str(p.get("parameter_name","")).lower() for p in ps)
    has_video=any(r.get("component")=="Video" or "video" in str(r.get("label","")).lower() for r in rs)
    if has_img and has_video: candidates.append((name,spec))
if not candidates:
    for name,spec in eps.items():
        ps=spec.get("parameters",[])
        if any(p.get("component")=="Image" or "image" in str(p.get("parameter_name","")).lower() for p in ps):
            candidates.append((name,spec))
print("CANDIDATES",[n for n,_ in candidates])

last=None
for name,spec in candidates:
    args=[]
    for p in spec.get("parameters",[]):
        comp=p.get("component"); pname=str(p.get("parameter_name","")).lower(); label=str(p.get("label","")).lower()
        if comp=="Image" or ("image" in pname and "num" not in pname):
            args.append(handle_file(inp))
        elif "prompt" in pname:
            args.append("A symmetrical medieval stone gatehouse with two square crenellated towers, a central rounded stone arch with a raised portcullis, blue fleur-de-lis banners, warm torchlight, realistic block masonry, preserve exact architecture and proportions.")
        elif "trajectory" in pname:
            args.append("truck_right")
        elif "motion_scale" in pname:
            args.append(0.65)
        elif "seed" in pname:
            args.append(1234)
        elif "randomize" in pname:
            args.append(False)
        elif p.get("parameter_has_default"):
            args.append(p.get("parameter_default"))
        elif comp=="Checkbox":
            args.append(False)
        elif comp in ("Slider","Number"):
            # conservative mild camera motion if no defaults
            args.append(0)
        elif comp in ("Textbox",):
            args.append("")
        elif comp=="Dropdown":
            choices=p.get("choices") or []
            args.append(choices[0] if choices else None)
        else:
            args.append(None)
    try:
        print("TRY",name,args)
        res=client.predict(*args,api_name=name)
        print("RESULT",repr(res))
        open(os.path.join(OUT,"result.json"),"w").write(json.dumps({"endpoint":name,"result":res},indent=2,default=str))
        files=[]
        def walk(v):
            if isinstance(v,str) and os.path.exists(v) and os.path.isfile(v): files.append(v)
            elif isinstance(v,dict):
                for key in ("path","name","video"):
                    q=v.get(key)
                    if isinstance(q,str) and os.path.exists(q): files.append(q)
                for x in v.values(): walk(x)
            elif isinstance(v,(list,tuple)):
                for x in v: walk(x)
        walk(res)
        copied=0
        for p in files:
            ext=os.path.splitext(p)[1].lower()
            if ext in (".mp4",".webm",".mov",".gif",".png",".jpg",".jpeg"):
                shutil.copy2(p,os.path.join(OUT,f"scope-{copied}{ext}")); copied+=1
        if any(x.endswith((".mp4",".webm",".mov")) for x in os.listdir(OUT)):
            raise SystemExit(0)
    except SystemExit:
        raise
    except Exception as e:
        last=repr(e); print("FAILED",name,last)
raise RuntimeError("No camera-controlled video produced; last="+str(last))
