#!/usr/bin/env python3
"""Zero-credit *candidate* for texture-density reduction of an existing LOD.
Blender PBR source stays frozen. Texture resampling is not ART_APPROVED.
"""
import argparse
import hashlib
import json
import os
import sys

import bpy

def parse():
    v = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    p = argparse.ArgumentParser()
    p.add_argument("--input", required=True)
    p.add_argument("--output", required=True)
    p.add_argument("--report", required=True)
    p.add_argument("--max-size", type=int, default=1024)
    a = p.parse_args(v)
    if a.max_size not in (512, 1024, 2048):
        raise ValueError("Only explicit texture QA sizes 512/1024/2048 are permitted")
    if not os.path.isfile(a.input):
        raise FileNotFoundError(a.input)
    if os.path.abspath(a.input) == os.path.abspath(a.output):
        raise ValueError("Never overwrite input LOD")
    if os.path.exists(a.output) or os.path.exists(a.report):
        raise FileExistsError("Never overwrite preexisting candidate/evidence")
    return a

def digest(p):
    h=hashlib.sha256()
    with open(p,"rb") as f:
        for chunk in iter(lambda: f.read(1024*1024),b""):
            h.update(chunk)
    return h.hexdigest()

def tris():
    total=0
    for obj in bpy.data.objects:
        if obj.type=="MESH":
            obj.data.calc_loop_triangles()
            total+=len(obj.data.loop_triangles)
    return total

def main():
    a=parse()
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=a.input)
    meshes=[o for o in bpy.data.objects if o.type=="MESH"]
    if not meshes or any(not o.data.uv_layers for o in meshes):
        raise ValueError("Missing source mesh / UV: abort texture QA")
    source_tris=tris()
    rows=[]
    touched=0
    for im in list(bpy.data.images):
        if im.type!="IMAGE" or im.source not in ("FILE","GENERATED") or min(im.size)<=0:
            continue
        w,h=im.size[0],im.size[1]
        factor=min(1.0,a.max_size/max(w,h))
        nw,nh=max(1,round(w*factor)),max(1,round(h*factor))
        if factor<1:
            im.scale(nw,nh)
            im.pack()
            touched+=1
        rows.append({"name":im.name,"original":[w,h],"resized":[int(im.size[0]),int(im.size[1])],
                     "colorspace":im.colorspace_settings.name,"changed":factor<1})
    if not rows or touched<1:
        raise ValueError("Texture profile changed no image; review source/caps")
    if source_tris!=tris():
        raise ValueError("Texture resample unexpectedly modified geometry")
    os.makedirs(os.path.dirname(a.output),exist_ok=True)
    bpy.ops.export_scene.gltf(filepath=a.output,export_format="GLB",
                               export_apply=True,export_materials="EXPORT",export_yup=True)
    result={
        "status":"TEXTURE_DENSITY_CANDIDATE_NOT_MOBILE_APPROVED",
        "source_sha256":digest(a.input),"output_sha256":digest(a.output),
        "source_bytes":os.path.getsize(a.input),"output_bytes":os.path.getsize(a.output),
        "source_triangles":source_tris,"output_triangles":tris(),
        "max_texture_dimension":a.max_size,
        "changed_images":touched,"images":rows,
        "new_tripo_credits":0,"unity_tested":False,"art_approved":False
    }
    os.makedirs(os.path.dirname(a.report),exist_ok=True)
    with open(a.report,"w",encoding="utf-8") as f:json.dump(result,f,indent=2)
    print("ELDORIA_ZERO_CREDIT_TEXTURE_CANDIDATE="+json.dumps(result))
if __name__=="__main__":main()
