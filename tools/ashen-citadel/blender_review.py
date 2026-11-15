#!/usr/bin/env python3
"""Actual Blender import/material/lighting review, native render, editable .blend and GLB.
No legacy mesh. Use: blender -b -t 4 --python blender_review.py -- --out artifacts/ashen
The render is ART QA, not Unity or mobile performance evidence.
"""
import bpy
import math
import re
import json
import os
import sys
from pathlib import Path
from mathutils import Vector

sys.path.insert(0,str(Path(__file__).resolve().parent))
from blender_environment import build_environment, meadow_material
ROOT = Path(__file__).resolve().parents[2]
MESHES = ROOT/"Unity/Assets/Eldoria/ExperimentalAshenCitadel/Meshes"

def reset():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)

def import_obj(path):
    before=set(bpy.data.objects)
    if hasattr(bpy.ops.wm, "obj_import"):
        bpy.ops.wm.obj_import(filepath=str(path), forward_axis="NEGATIVE_Z", up_axis="Y")
    else:
        bpy.ops.import_scene.obj(filepath=str(path), axis_forward="-Z", axis_up="Y")
    imported=[obj for obj in bpy.data.objects if obj not in before]
    result=[o for o in imported if o.type=="MESH"]
    if not result:
        raise RuntimeError("Blender imported no mesh: "+str(path))
    return result

def palette():
    values={}
    current=None
    for raw in (MESHES/"AshenPalette.mtl").read_text(encoding="utf-8").splitlines():
        line=raw.strip()
        if line.startswith("newmtl "):current=line[7:].strip()
        elif current and line.startswith("Kd "):
            values[current]=tuple(map(float,line[3:].split()))
    assert len(values)==18, ("Incomplete palette",len(values))
    return values

def pbr(material, rgb, material_key=None):
    material.diffuse_color=(rgb[0],rgb[1],rgb[2],1)
    material.use_nodes=True
    bsdf=material.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        color=bsdf.inputs.get("Base Color")
        if color:color.default_value=(rgb[0],rgb[1],rgb[2],1)
        rough=bsdf.inputs.get("Roughness")
        if rough:rough.default_value=.66
        metal=bsdf.inputs.get("Metallic")
        if metal:metal.default_value=.34 if "brass" in material.name.lower() else 0
        if material_key:
            groups={
                "Stone":["basalt","stone","stoneLight","cut","plaster"],
                "Roof":["roof","slate"],"Wood":["wood","oak"],
                "Moss":["soil","moss","leaf","leafLight"],
                "Metal":["iron","brass","warm"],"Cloth":["banner","violet"]
            }
            family=next((g for g,keys in groups.items() if material_key in keys),None)
            if not family:raise RuntimeError("No mobile texture group for "+material_key)
            path=MESHES.parent/"Textures"/("Ashen"+family+".png")
            if not path.is_file():raise FileNotFoundError(str(path))
            image=bpy.data.images.load(str(path),check_existing=True)
            nodes=material.node_tree.nodes;links=material.node_tree.links
            texture=nodes.new("ShaderNodeTexImage");texture.image=image
            multiply=nodes.new("ShaderNodeMixRGB");multiply.blend_type="MULTIPLY"
            multiply.inputs[0].default_value=1
            multiply.inputs[1].default_value=(rgb[0],rgb[1],rgb[2],1)
            for current in list(color.links):links.remove(current)
            links.new(texture.outputs["Color"],multiply.inputs[2])
            links.new(multiply.outputs["Color"],color)
    return material

def remap(objects,pal):
    count=0
    for obj in objects:
        for slot in obj.material_slots:
            orig=slot.material
            if orig is None:continue
            name=re.sub(r"\.\d{3}$", "", orig.name.lower())
            found=next((k for k in pal if k.lower()==name or name.endswith(":"+k.lower()) or name.startswith(k.lower()+" ")),None)
            if found:
                pbr(orig,pal[found],found);count+=1
            else:
                # Blender OBJ material assignments must be investigated, not silently replaced.
                raise RuntimeError("Unknown MTL material "+repr(orig.name)+" in "+obj.name)
    if count==0:raise RuntimeError("No imported MTL slots remapped")

def camera_and_lights():
    sc=bpy.context.scene
    world=bpy.data.worlds.new("Smoke-blue overcast world")
    sc.world=world;world.use_nodes=True
    world.node_tree.nodes["Background"].inputs["Color"].default_value=(.23,.29,.29,1)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value=.70
    bpy.ops.object.camera_add(location=(23,29,27))
    cam=bpy.context.object;cam.name="Ashen City · Isometric 2.5D"
    direction=Vector((0,0,3))-cam.location
    cam.rotation_euler=direction.to_track_quat("-Z","Y").to_euler()
    cam.data.type="ORTHO";cam.data.ortho_scale=45
    sc.camera=cam
    bpy.ops.object.light_add(type="AREA", location=(-8,-13,25))
    light=bpy.context.object;light.name="Warm dusk · 3700K";light.data.energy=1900
    light.data.shape="DISK";light.data.size=18
    light.data.color=(1,.78,.55)
    bpy.ops.object.light_add(type="AREA", location=(11,10,18))
    fill=bpy.context.object;fill.name="Cold sky fill";fill.data.energy=1200
    fill.data.shape="DISK";fill.data.size=25;fill.data.color=(.57,.73,.88)
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0,0,-1.42))
    plane=bpy.context.object;plane.name="Dark meadow · QA plinth"
    plane.scale=(130,130,.50)
    mat=bpy.data.materials.new("Dusk dry moss")
    pbr(mat,(.32,.37,.31))
    plane.data.materials.append(mat)
    meadow_material(mat)
    return cam

def configure_render():
    sc=bpy.context.scene
    sc.render.resolution_x=1000;sc.render.resolution_y=700;sc.render.resolution_percentage=100
    sc.render.image_settings.file_format="PNG"
    sc.render.film_transparent=False
    try: sc.view_settings.view_transform = "AgX"
    except Exception: pass
    # Cycles is CPU-capable on GitHub's Linux runner without a GPU/display.
    sc.render.engine="CYCLES"
    sc.cycles.device="CPU"
    sc.cycles.samples=20
    try: sc.cycles.use_denoising=True
    except Exception: pass
    sc.render.image_settings.color_mode="RGBA"
    return sc.render.engine

def main():
    args=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    out=Path(args[args.index("--out")+1]) if "--out" in args else ROOT/"artifacts/ashen-blender"
    if not out.is_absolute():out=ROOT/out
    out.mkdir(parents=True,exist_ok=True)
    colors=palette()
    evidence=[]
    engine=None
    for level in range(1,6):
        reset()
        objs=import_obj(MESHES/f"AshenBastion_{level}.obj")
        remap(objs,colors)
        for obj in objs:
            if obj.type=="MESH":
                obj.name="Bastion %d · authored mesh" % level
                if hasattr(obj.data, "use_auto_smooth"): obj.data.use_auto_smooth = False
        cam=camera_and_lights()
        build_environment(level,colors,MESHES,import_obj,remap,pbr)
        engine=configure_render()
        outpath=out/f"Bastion_{level}_blender.png"
        bpy.context.scene.render.filepath=str(outpath)
        bpy.ops.render.render(write_still=True)
        # Save a real editable source and exchange model generated by Blender.
        bpy.ops.wm.save_as_mainfile(filepath=str(out/f"Bastion_{level}_source.blend"))
        # Export ONLY the authored Bastion model, never QA ground, lights or camera.
        bpy.ops.object.select_all(action="DESELECT")
        for mesh_obj in objs: mesh_obj.select_set(True)
        bpy.context.view_layer.objects.active=objs[0]
        bpy.ops.export_scene.gltf(filepath=str(out/f"Bastion_{level}_export.glb"),
                                  export_format="GLB",use_selection=True)
        tri_count=sum(len(p.vertices)-2 for ob in objs for p in ob.data.polygons)
        evidence.append({"level":level,"triangles":tri_count,"imported_meshes":len(objs),
                         "renderer":engine,"png":outpath.name,"blend":f"Bastion_{level}_source.blend",
                         "glb":f"Bastion_{level}_export.glb"})
        print("ASHEN_BLENDER_RENDERED",json.dumps(evidence[-1]),flush=True)
    (out/"report.json").write_text(json.dumps({"status":"Blender geometry/lighting production",
       "unityTested":False,"iphoneTested":False,"evidence":evidence},indent=2),encoding="utf-8")

if __name__=="__main__":
    main()
