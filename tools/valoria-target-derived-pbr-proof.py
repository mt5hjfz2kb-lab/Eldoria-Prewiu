import json, os, shutil
from PIL import Image
from gradio_client import Client, handle_file

ROOT="docs/evidence/valoria-golden-lookdev-slice-v1/camera-first-depth-shell-v1"
OUT=os.environ.get("OUT_DIR","/tmp/stablematerials-proof")
os.makedirs(OUT,exist_ok=True)
src=Image.open(os.path.join(ROOT,"gate-front.png")).convert("RGBA")
alpha=src.getchannel("A")
bbox=alpha.getbbox()
if not bbox: raise RuntimeError("empty gate alpha")
x0,y0,x1,y1=bbox
# Choose a dense masonry patch from upper-right gate body, avoiding transparent border.
w=x1-x0; h=y1-y0
cx=int(x0+w*0.72); cy=int(y0+h*0.37)
side=max(160,min(320,int(min(w,h)*0.38)))
patch=src.crop((max(x0,cx-side//2),max(y0,cy-side//2),min(x1,cx+side//2),min(y1,cy+side//2))).convert("RGB")
patch=patch.resize((512,512),Image.Resampling.LANCZOS)
inp=os.path.join(OUT,"gate-masonry-input.png"); patch.save(inp)

client=Client("gvecchio/StableMaterials")
api=client.view_api(return_format="dict")
open(os.path.join(OUT,"api.json"),"w").write(json.dumps(api,indent=2,default=str))
eps=api.get("named_endpoints",{}) if isinstance(api,dict) else {}
print("ENDPOINTS",list(eps))
candidate=None
for name,spec in eps.items():
    params=spec.get("parameters",[])
    rets=spec.get("returns",[])
    if len(rets)>=5 and any("image_prompt" in str(p.get("parameter_name","")) for p in params):
        candidate=(name,spec);break
if candidate is None:
    # Gradio may expose only one unnamed generation endpoint.
    for name,spec in eps.items():
        if len(spec.get("returns",[]))>=5:
            candidate=(name,spec);break
if candidate is None: raise RuntimeError("No StableMaterials generation endpoint")
name,spec=candidate
args=[]
for p in spec.get("parameters",[]):
    pname=str(p.get("parameter_name","")).lower()
    default=p.get("parameter_default")
    if pname=="prompt_type": args.append("Image")
    elif pname=="text_prompt": args.append("")
    elif "image_prompt" in pname: args.append(handle_file(inp))
    elif pname=="seed": args.append(42)
    elif pname=="resolution": args.append("512" if isinstance(default,str) else 512)
    elif pname=="refinement": args.append(False)
    elif p.get("parameter_has_default"): args.append(default)
    else: args.append(None)
print("USING",name,args)
res=client.predict(*args,api_name=name)
print("RESULT",repr(res))
open(os.path.join(OUT,"result.json"),"w").write(json.dumps(res,indent=2,default=str))
labels=["basecolor","normal","height","metallic","roughness"]
if not isinstance(res,(list,tuple)) or len(res)<5: raise RuntimeError("Expected five material maps")
for lab,val in zip(labels,res[:5]):
    p=val.get("path") if isinstance(val,dict) else val
    if not p or not os.path.exists(p): raise RuntimeError(f"missing {lab}: {val!r}")
    ext=os.path.splitext(p)[1] or ".png"
    shutil.copy2(p,os.path.join(OUT,lab+ext))
