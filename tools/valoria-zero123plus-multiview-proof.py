import json, os, shutil
from PIL import Image
from gradio_client import Client, handle_file

ROOT="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"
OUT=os.environ.get("OUT_DIR","/tmp/zero123plus-mv")
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

spaces=["ysharma/Zero123PlusDemo","darshcoss/Zero123PlusDemo","sudo-ai/zero123plus-demo-space"]
last=None
for sid in spaces:
    try:
        client=Client(sid)
        api=client.view_api(return_format="dict")
        open(os.path.join(OUT,"api-"+sid.split("/")[-1]+".json"),"w").write(json.dumps(api,indent=2,default=str))
        eps=api.get("named_endpoints",{}) if isinstance(api,dict) else {}
        print("SPACE",sid,"ENDPOINTS",list(eps))
        for name,spec in eps.items():
            ps=spec.get("parameters",[]); rs=spec.get("returns",[])
            has_img=any(p.get("component")=="Image" or "image" in str(p.get("parameter_name","")).lower() for p in ps)
            returns_img=any(r.get("component")=="Image" for r in rs)
            if not (has_img and returns_img): continue
            args=[]
            for p in ps:
                comp=p.get("component"); pname=str(p.get("parameter_name","")).lower()
                if comp=="Image": args.append(handle_file(inp))
                elif "seed" in pname: args.append(1234)
                elif "step" in pname: args.append(40)
                elif "guidance" in pname: args.append(3.0)
                elif p.get("parameter_has_default"): args.append(p.get("parameter_default"))
                elif comp=="Checkbox": args.append(False)
                elif comp in ("Slider","Number"): args.append(0)
                elif comp=="Textbox": args.append("")
                else: args.append(None)
            try:
                print("TRY",name,args)
                res=client.predict(*args,api_name=name)
                print("RESULT",repr(res))
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
                    if ext in (".png",".jpg",".jpeg",".webp"):
                        shutil.copy2(p,os.path.join(OUT,"multiview"+ext))
                        open(os.path.join(OUT,"result.json"),"w").write(json.dumps({"space":sid,"endpoint":name,"result":res},indent=2,default=str))
                        raise SystemExit(0)
            except SystemExit: raise
            except Exception as e:
                last=repr(e); print("ENDPOINT_FAIL",name,last)
    except SystemExit: raise
    except Exception as e:
        last=repr(e); print("SPACE_FAIL",sid,last)
raise RuntimeError("No multiview output produced; last="+str(last))
