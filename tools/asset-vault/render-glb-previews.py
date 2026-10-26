#!/usr/bin/env python3
"""Render geometry-only GLB contact-sheet thumbnails. Never claim Unity/URP art approval."""
import argparse,json,math
from pathlib import Path
import numpy as np
from PIL import Image,ImageDraw
import trimesh

ROOT=Path(__file__).resolve().parents[2]
CATALOG=ROOT/"pipeline/asset-vault/master-catalog-v1.json"

def preview(glb,out,size=512,max_faces=45000):
    scene=trimesh.load(str(glb),force="scene",process=False)
    meshes=[m for m in scene.dump(concatenate=False)
            if isinstance(m,trimesh.Trimesh) and len(m.faces)]
    if not meshes:raise ValueError("GLB has no triangle meshes")
    vertices=np.vstack([m.vertices for m in meshes])
    center=(vertices.min(axis=0)+vertices.max(axis=0))*.5
    extent=max(float(np.max(vertices.max(axis=0)-vertices.min(axis=0))),1e-4)
    front=np.array([1.,.75,1.15]);front/=np.linalg.norm(front)
    up=np.array([0.,1.,0.]);right=np.cross(up,front)
    right/=np.linalg.norm(right);up=np.cross(front,right)
    light=np.array([.4,.85,.46]);light/=np.linalg.norm(light)
    polygons=[]
    budget=max(1,int(max_faces/max(1,len(meshes))))
    for mesh in meshes:
        faces=mesh.faces
        if len(faces)>budget:faces=faces[::math.ceil(len(faces)/budget)]
        tri=mesh.vertices[faces]
        n=np.cross(tri[:,1]-tri[:,0],tri[:,2]-tri[:,0])
        norms=np.linalg.norm(n,axis=1);good=norms>1e-8
        n[good]/=norms[good,None]
        luminance=np.clip(.54+.34*(n@light),.20,1.)
        depth=tri.mean(axis=1)@front
        x=((tri-center)@right)*(.81*size/extent)+size/2
        y=-((tri-center)@up)*(.81*size/extent)+size/2
        for i in range(len(tri)):
            v=int(luminance[i]*195)
            points=[(float(x[i,j]),float(y[i,j])) for j in range(3)]
            polygons.append((depth[i],points,(v,int(v*.95),int(v*.84))))
    polygons.sort(key=lambda z:-z[0])
    im=Image.new("RGB",(size,size),"#eef1f0")
    draw=ImageDraw.Draw(im)
    for _,points,colour in polygons:draw.polygon(points,fill=colour)
    draw.rectangle((0,size-29,size,size),fill="#22252b")
    draw.text((7,size-21),"Geometry preview; NOT in-game quality",fill="#fff")
    out.parent.mkdir(parents=True,exist_ok=True)
    im.save(out,optimize=True)
    return {"meshes":len(meshes),"triangles_displayed":len(polygons)}

def main():
    p=argparse.ArgumentParser()
    p.add_argument("--out",required=True,type=Path)
    p.add_argument("--manifest",required=True,type=Path)
    p.add_argument("--limit",type=int,default=26)
    args=p.parse_args()
    catalog=json.loads(CATALOG.read_text(encoding="utf-8-sig"))
    records=[a for a in catalog["records"] if a["id"].startswith("canon-")][:args.limit]
    results=[]
    for asset in records:
        infile=ROOT/asset["path"];dst=args.out/(asset["name"]+".png")
        try:
            evidence=preview(infile,dst)
            results.append({"name":asset["name"],"path":asset["path"],
                "preview":str(dst.resolve().relative_to(ROOT)),**evidence,"status":"GEOMETRY_ONLY"})
            print("PREVIEW_OK",asset["name"])
        except Exception as e:
            results.append({"name":asset["name"],"path":asset["path"],
                "status":"RENDER_FAILURE","error":str(e)[:400]})
            print("PREVIEW_FAILURE",asset["name"],str(e)[:240])
    args.manifest.parent.mkdir(parents=True,exist_ok=True)
    args.manifest.write_text(json.dumps({"schema_version":1,"art_approved":False,
        "source":"tracked canonical GLB","previews":results},ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    if len(results)!=args.limit or any(x["status"]=="RENDER_FAILURE" for x in results):
        raise SystemExit("Thumbnail render incomplete; preserve manifest for diagnosis")
if __name__=="__main__":main()
