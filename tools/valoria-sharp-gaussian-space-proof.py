import json, os, shutil, requests
from PIL import Image
from gradio_client import Client, handle_file

OUT=os.environ.get("OUT_DIR","/tmp/sharp-proof")
os.makedirs(OUT,exist_ok=True)
src="references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg"
im=Image.open(src).convert("RGB")
im.thumbnail((1280,1280),Image.LANCZOS)
inp=os.path.join(OUT,"canonical-target.png"); im.save(inp)

spaces=requests.get("https://huggingface.co/api/spaces",params={"author":"gagndeep","limit":100},timeout=30).json()
ids=[]
for s in spaces:
    sid=s.get("id") or s.get("name") or ""
    title=(s.get("cardData") or {}).get("title","") if isinstance(s,dict) else ""
    if "sharp" in sid.lower() or "sharp" in str(title).lower():
        ids.append(sid)
open(os.path.join(OUT,"spaces.json"),"w").write(json.dumps(spaces,indent=2,default=str))
print("SHARP_CANDIDATES",ids)
if not ids: raise RuntimeError("No gagndeep SHARP Space discovered")

last=None
for sid in ids:
    try:
        client=Client(sid)
        api=client.view_api(return_format="dict")
        open(os.path.join(OUT,"api.json"),"w").write(json.dumps({"space":sid,"api":api},indent=2,default=str))
        eps=api.get("named_endpoints",{}) if isinstance(api,dict) else {}
        print("SPACE",sid,"ENDPOINTS",list(eps))
        candidates=[]
        for name,spec in eps.items():
            ps=spec.get("parameters",[])
            has_img=any(p.get("component")=="Image" or "image" in str(p.get("parameter_name","")).lower() for p in ps)
            if has_img: candidates.append((name,spec))
        for name,spec in candidates:
            args=[]
            for p in spec.get("parameters",[]):
                comp=p.get("component"); pname=str(p.get("parameter_name","")).lower()
                if comp=="Image" or ("image" in pname and "num" not in pname): args.append(handle_file(inp))
                elif "seed" in pname: args.append(1234)
                elif p.get("parameter_has_default"): args.append(p.get("parameter_default"))
                elif comp=="Checkbox": args.append(False)
                elif comp in ("Slider","Number"): args.append(0)
                elif comp in ("Textbox",): args.append("")
                else: args.append(None)
            try:
                print("TRY",name,args)
                res=client.predict(*args,api_name=name)
                print("RESULT",repr(res))
                open(os.path.join(OUT,"result.json"),"w").write(json.dumps({"space":sid,"endpoint":name,"result":res},indent=2,default=str))
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
                    if ext in (".ply",".splat",".spz",".zip",".mp4",".webm",".glb",".obj",".png",".jpg",".jpeg"):
                        shutil.copy2(p,os.path.join(OUT,f"sharp-{copied}{ext}")); copied+=1
                if copied:
                    print("COPIED",copied)
                    raise SystemExit(0)
            except SystemExit: raise
            except Exception as e:
                last=repr(e); print("ENDPOINT_FAILED",name,last)
    except SystemExit: raise
    except Exception as e:
        last=repr(e); print("SPACE_FAILED",sid,last)
raise RuntimeError("No SHARP output produced; last="+str(last))
