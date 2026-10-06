import json, os, shutil
from gradio_client import Client, handle_file

OUT=os.environ.get("OUT_DIR","/tmp/geometrycrafter-proof")
video=os.path.join(OUT,"scope-0.mp4")
if not os.path.exists(video): raise RuntimeError("Missing SCoPE source video")

client=Client("TencentARC/GeometryCrafter")
api=client.view_api(return_format="dict")
open(os.path.join(OUT,"api.json"),"w").write(json.dumps(api,indent=2,default=str))
eps=api.get("named_endpoints",{}) if isinstance(api,dict) else {}
print("ENDPOINTS",list(eps))
cands=[]
for name,spec in eps.items():
    ps=spec.get("parameters",[]); rs=spec.get("returns",[])
    has_vid=any(p.get("component")=="Video" or "video" in str(p.get("parameter_name","")).lower() for p in ps)
    has_3d=any(r.get("component") in ("Model3D","File","Downloadbutton") or "point" in str(r.get("label","")).lower() for r in rs)
    if has_vid and has_3d: cands.append((name,spec))
if not cands:
    for name,spec in eps.items():
        if any(p.get("component")=="Video" for p in spec.get("parameters",[])): cands.append((name,spec))
print("CANDS",[x[0] for x in cands])

last=None
for name,spec in cands:
    args=[]
    for p in spec.get("parameters",[]):
        comp=p.get("component"); pname=str(p.get("parameter_name","")).lower()
        if comp=="Video" or "video" in pname:
            args.append(handle_file(video))
        elif "process_length" in pname or ("length" in pname and comp in ("Slider","Number")):
            args.append(60)
        elif "max_res" in pname or "resolution" in pname:
            args.append(640)
        elif "denoising" in pname or ("step" in pname and comp in ("Slider","Number")):
            args.append(5)
        elif "guidance" in pname or "cfg" in pname:
            args.append(1.0)
        elif "window" in pname:
            args.append(60)
        elif "decode" in pname:
            args.append(8)
        elif "overlap" in pname:
            args.append(20)
        elif p.get("parameter_has_default"):
            args.append(p.get("parameter_default"))
        elif comp=="Checkbox": args.append(False)
        elif comp in ("Slider","Number"): args.append(0)
        elif comp=="Textbox": args.append("")
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
            if ext in (".glb",".gltf",".ply",".obj",".npz",".npy",".mp4",".webm"):
                shutil.copy2(p,os.path.join(OUT,f"geometrycrafter-{copied}{ext}")); copied+=1
        if any(x.endswith((".glb",".gltf",".ply",".obj",".npz")) for x in os.listdir(OUT)):
            raise SystemExit(0)
    except SystemExit: raise
    except Exception as e:
        last=repr(e); print("FAILED",name,last)
raise RuntimeError("No 3D/point-map output; last="+str(last))
