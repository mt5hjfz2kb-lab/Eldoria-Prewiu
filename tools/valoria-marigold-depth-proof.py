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

fd=handle_file(inp)
# Imageslider input is a before/after tuple. For first processing, the source occupies both
# slots safely; the backend replaces the processed side with its inferred modalities.
res=client.predict((fd,fd),None,None,api_name="/on_process_first")
print("RESULT",repr(res))
open(os.path.join(OUT,"result.json"),"w").write(json.dumps(res,indent=2,default=str))

files=[]
def walk(v):
    if isinstance(v,str) and os.path.exists(v) and os.path.isfile(v):
        files.append(v)
    elif isinstance(v,dict):
        for key in ("path","name"):
            q=v.get(key)
            if isinstance(q,str) and os.path.exists(q): files.append(q)
        for x in v.values(): walk(x)
    elif isinstance(v,(list,tuple)):
        for x in v: walk(x)
walk(res)
seen=set(); copied=0
for p in files:
    if p in seen: continue
    seen.add(p)
    ext=os.path.splitext(p)[1].lower()
    if ext in (".png",".jpg",".jpeg",".webp",".tif",".tiff",".exr",".npy",".npz"):
        shutil.copy2(p,os.path.join(OUT,f"marigold-{copied}{ext}")); copied+=1
print("COPIED",copied)
if copied<2:
    raise RuntimeError("Marigold returned insufficient image evidence")
