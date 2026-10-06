import json, os, shutil, colorsys
import numpy as np
from PIL import Image
from gradio_client import Client, handle_file

OUT=os.environ.get("OUT_DIR","/tmp/depthmesh-proof")
os.makedirs(OUT,exist_ok=True)
src="docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg"
im=Image.open(src).convert("RGB")
# Remove UI chrome: retain the world/city frame only.
im=im.crop((64,35,960,610))
im.thumbnail((896,576),Image.LANCZOS)
inp=os.path.join(OUT,"world-crop.png"); im.save(inp)

client=Client("toshas/Marigold-V2")
fd=handle_file(inp)
res=client.predict((fd,fd),None,None,api_name="/on_process_first")
gallery=res[0]
open(os.path.join(OUT,"marigold-result.json"),"w").write(json.dumps(res,indent=2,default=str))
by_caption={}
for item in gallery:
    cap=item.get("caption") if isinstance(item,dict) else None
    data=item.get("image") if isinstance(item,dict) else None
    if isinstance(data,str):
        p=data
    elif isinstance(data,dict):
        p=data.get("path") or data.get("name")
    else:
        p=None
    if cap and p and os.path.exists(p):
        ext=os.path.splitext(p)[1] or ".webp"
        dst=os.path.join(OUT,cap.lower().replace(" ","-")+ext)
        shutil.copy2(p,dst); by_caption[cap]=dst
if "Depth" not in by_caption: raise RuntimeError("Depth output missing")

dep=Image.open(by_caption["Depth"]).convert("RGB").resize(im.size,Image.BILINEAR)
rgb=np.asarray(dep,dtype=np.float32)/255.0
# Marigold display palette runs roughly blue/cyan -> yellow/orange -> red/magenta.
# Decode its ordinal depth through hue, unwrapping magenta past red.
import colorsys
flat=rgb.reshape(-1,3)
h=np.empty(len(flat),dtype=np.float32)
s=np.empty(len(flat),dtype=np.float32)
for i,(r,g,b) in enumerate(flat):
    hh,ss,vv=colorsys.rgb_to_hsv(float(r),float(g),float(b)); h[i]=hh*360.0; s[i]=ss
h=h.reshape(rgb.shape[:2]); s=s.reshape(rgb.shape[:2])
hu=np.where(h>300.0,h-360.0,h)
near=np.clip((205.0-hu)/235.0,0.0,1.0)
# suppress low-saturation palette noise by local median-ish fallback
gray=np.asarray(dep.convert("L"),dtype=np.float32)/255.0
near=np.where(s<0.08,0.5*near+0.5*(1-gray),near)
# mild smoothing without scipy
for _ in range(2):
    near=(near+np.roll(near,1,0)+np.roll(near,-1,0)+np.roll(near,1,1)+np.roll(near,-1,1))/5.0
np.save(os.path.join(OUT,"decoded-depth.npy"),near.astype(np.float32))
Image.fromarray((near*255).astype(np.uint8)).save(os.path.join(OUT,"decoded-depth.png"))

# Build UV textured continuous mesh sampled every 4 px.
W,H=im.size; step=4
xs=list(range(0,W,step)); ys=list(range(0,H,step))
if xs[-1]!=W-1: xs.append(W-1)
if ys[-1]!=H-1: ys.append(H-1)
verts=[]; uvs=[]
for y in ys:
    for x in xs:
        u=x/(W-1); v=1-y/(H-1)
        X=(u-0.5)*2.0
        Y=(v-0.5)*2.0*(H/W)
        Z=float(near[y,x])*0.55
        verts.append((X,Y,Z)); uvs.append((u,v))
nx=len(xs)
faces=[]
cut=0
for j in range(len(ys)-1):
    for i in range(len(xs)-1):
        ids=[j*nx+i,j*nx+i+1,(j+1)*nx+i,(j+1)*nx+i+1]
        zs=[verts[k][2] for k in ids]
        if max(zs)-min(zs)>0.12:
            cut+=1; continue
        a,b,c,d=ids
        faces.append((a,b,d)); faces.append((a,d,c))
obj=os.path.join(OUT,"valoria-depthmesh.obj")
with open(obj,"w") as f:
    f.write("mtllib valoria-depthmesh.mtl\n")
    for x,y,z in verts:f.write(f"v {x:.6f} {y:.6f} {z:.6f}\n")
    for u,v in uvs:f.write(f"vt {u:.6f} {v:.6f}\n")
    f.write("usemtl ValoriaImage\n")
    for tri in faces:
        f.write("f "+" ".join(f"{k+1}/{k+1}" for k in tri)+"\n")
open(os.path.join(OUT,"valoria-depthmesh.mtl"),"w").write("newmtl ValoriaImage\nKd 1 1 1\nmap_Kd world-crop.png\n")
open(os.path.join(OUT,"mesh-report.json"),"w").write(json.dumps({"vertices":len(verts),"triangles":len(faces),"cut_cells":cut,"crop":[64,35,960,610],"depth_span":0.55},indent=2))
