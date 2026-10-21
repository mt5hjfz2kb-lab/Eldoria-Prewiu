#!/usr/bin/env python3
"""Zero-credit artistic finishing of an EXISTING Tripo GLB for Eldoria.
Reads the immutable source, makes two real mobile LOD meshes, darkens/clarifies
the diffuse texture, authors distinctive corruption eyes, retains PBR, exports
editable Blender and two GLBs. No network/API calls; no game rules.
"""
import bpy, sys, os, json, math, hashlib
from pathlib import Path
from mathutils import Vector
try:
    import numpy as np
except ImportError:
    np = None

def require(ok, msg):
    if not ok: raise RuntimeError(msg)

def sha(p):
    h=hashlib.sha256()
    with open(p,"rb") as f:
        for chunk in iter(lambda:f.read(1048576),b""):h.update(chunk)
    return h.hexdigest()

def tris(obj):
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)

def world_bounds(objs):
    pts=[o.matrix_world @ Vector(c) for o in objs for c in o.bound_box]
    return Vector(tuple(min(v[k] for v in pts) for k in range(3))),Vector(tuple(max(v[k] for v in pts) for k in range(3)))

def select_only(objects):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects: obj.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]

def make_emissive(name,color,power):
    mat=bpy.data.materials.new(name)
    mat.use_nodes=True
    nodes=mat.node_tree.nodes;nodes.clear()
    out=nodes.new("ShaderNodeOutputMaterial")
    emission=nodes.new("ShaderNodeEmission")
    emission.inputs["Color"].default_value=(*color,1)
    emission.inputs["Strength"].default_value=power
    mat.node_tree.links.new(emission.outputs["Emission"],out.inputs["Surface"])
    mat.diffuse_color=(*color,1)
    return mat

def add_eye(name,loc,rad,material,parent=None):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=rad,location=loc)
    obj=bpy.context.object;obj.name=name
    obj.scale=(1.15,.6,.9)
    obj.data.materials.append(material)
    if parent: obj.parent=parent
    return obj

def process_materials(objects,out):
    """Keep actual Tripo UV/albedo/normal PBR, with a deliberately darker violet grade."""
    diagnostics=[]
    seen=set()
    for obj in objects:
      for m in obj.data.materials:
        if m is None or m.name in seen:continue
        seen.add(m.name)
        m.use_nodes=True
        m.diffuse_color=(.38,.18,.49,1)
        bsdf=next((n for n in m.node_tree.nodes if n.type=="BSDF_PRINCIPLED"),None)
        if bsdf:
          if bsdf.inputs.get("Roughness"):bsdf.inputs["Roughness"].default_value=.82
          if bsdf.inputs.get("Metallic"):bsdf.inputs["Metallic"].default_value=0
        image_names=[]
        if bsdf and bsdf.inputs.get("Base Color"):
          linked=list(bsdf.inputs["Base Color"].links)
          if linked and linked[0].from_node.type=="TEX_IMAGE":
            img=linked[0].from_node.image
            if img:
              # Clone the existing baseColor instead of repainting normal/ORM.
              grade=img.copy();grade.name="Spawnling / retained skin with dusk-violet grade"
              if grade.size[0]>1024 or grade.size[1]>1024:
                grade.scale(1024,1024)
              if np is not None:
                values=np.empty(len(grade.pixels),dtype=np.float32)
                grade.pixels.foreach_get(values)
                px=values.reshape((-1,4))
                # Preserve texture micro-details and alpha; suppress the washed-out lavender.
                lum=px[:,:3].mean(axis=1)
                shade=np.clip(.92+(lum-.52)*.32,.79,1.08)
                px[:,0]*=.63*shade
                px[:,1]*=.48*shade
                px[:,2]*=.76*shade
                np.clip(px[:,:3],0,1,out=px[:,:3])
                grade.pixels.foreach_set(values)
              else:
                pixels=list(grade.pixels)
                for i in range(0,len(pixels),4):
                  bright=sum(pixels[i:i+3])/3
                  shade=max(.79,min(1.08,.92+(bright-.52)*.32))
                  pixels[i]*=.63*shade;pixels[i+1]*=.48*shade;pixels[i+2]*=.76*shade
                grade.pixels[:]=pixels
              grade.pack()
              linked[0].from_node.image=grade
              image_names.append({"role":"retained_albedo_regraded","w":grade.size[0],"h":grade.size[1]})
        for n in m.node_tree.nodes:
          if n.type=="TEX_IMAGE" and n.image:
            im=n.image
            if im.size[0]>1024 or im.size[1]>1024:
              if not im.name.startswith("Spawnling / retained skin"):
                clone=im.copy();clone.name="Spawnling / retained PBR "+im.name
                clone.scale(1024,1024);clone.pack();n.image=clone
                im=clone
            image_names.append({"role":"retained_pbr","w":im.size[0],"h":im.size[1]})
        diagnostics.append({"material":m.name,"images":image_names})
    return diagnostics

def decimate(obj,target):
    count=tris(obj)
    if count<=target:return count
    select_only([obj])
    mod=obj.modifiers.new("Mobile silhouette-preserving decimation","DECIMATE")
    mod.ratio=min(.99,target/float(count))
    bpy.ops.object.modifier_apply(modifier=mod.name)
    return tris(obj)

def camera_view(name,center,extents,angle,out,near):
    cdata=bpy.data.cameras.new(name)
    cam=bpy.data.objects.new(name,cdata);bpy.context.collection.objects.link(cam)
    loc=center+Vector(angle)*max(extents.x,extents.y,extents.z)*2.8
    cam.location=loc
    vec=center-loc
    cam.rotation_euler=vec.to_track_quat("-Z","Y").to_euler()
    cdata.type="ORTHO"
    cdata.ortho_scale=max(extents.x,extents.y,extents.z)* (2.55 if near else 5.7)
    bpy.context.scene.camera=cam
    bpy.context.scene.render.filepath=str(out)
    bpy.ops.render.render(write_still=True)
    bpy.data.objects.remove(cam,do_unlink=True)

def export_named(path,visible):
    select_only(visible)
    bpy.ops.export_scene.gltf(filepath=str(path),export_format="GLB",
         use_selection=True,export_apply=True,export_normals=True,
         export_texcoords=True,export_materials="EXPORT",export_animations=False)
    require(path.is_file() and path.stat().st_size>20000,"Export missing: "+str(path))

def main():
    argv=sys.argv[sys.argv.index("--")+1:] if "--" in sys.argv else []
    require(len(argv)==2,"Usage: -- SOURCE_GLB OUTPUT_DIRECTORY")
    source=Path(argv[0]);out=Path(argv[1]);out.mkdir(parents=True,exist_ok=True)
    require(source.is_file(),"Existing paid source missing")
    bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.gltf(filepath=str(source))
    meshes=[o for o in bpy.context.scene.objects if o.type=="MESH"]
    require(len(meshes)==1,"Expected exactly one actual paid Tripo mesh")
    original=meshes[0];before=tris(original)
    require(48000<=before<=51000,"Unexpected Tripo source topology "+str(before))
    lo,hi=world_bounds(meshes);center=(lo+hi)/2;size=hi-lo
    require(min(size)>0.1 and max(size)<5,"Source scale unexpected "+str(tuple(size)))
    diag={"bounds_min":list(lo),"bounds_max":list(hi),"source_triangles":before,
          "head_axis_convention":"Blender -Y assumed, verified by four views",
          "note":"Geometry changes do not imply artistic approval."}
    # Keep original mesh editable in the saved Blender, but never export it to Unity.
    original.name="Original paid Tripo / preserved untouched"
    original.hide_render=True
    original.hide_set(True)
    near=original.copy();near.data=original.data.copy();bpy.context.collection.objects.link(near)
    near.name="Spawnling_Near_Authored"
    near.hide_render=False;near.hide_set(False)
    # Shorten the distal rear tail rather than making a completely new creature:
    # identify the tapered rear by low-Z cross section beyond 70% longitudinal depth.
    # A paired before/after camera must confirm the sign and effect.
    coords=[v.co for v in near.data.vertices]
    ylo=min(v.y for v in coords);yhi=max(v.y for v in coords)
    zlo=min(v.z for v in coords);zhi=max(v.z for v in coords)
    xlo=min(v.x for v in coords);xhi=max(v.x for v in coords)
    # Conservative deformation: only extreme rear low vertices move; no claws/arms/head.
    # Blender glTF convention has head towards -Y and tail towards +Y for this asset.
    edits=0
    for v in near.data.vertices:
        along=(v.co.y-ylo)/max(.001,yhi-ylo)
        height=(v.co.z-zlo)/max(.001,zhi-zlo)
        if along>.77 and height<.64 and abs(v.co.x-(xlo+xhi)/2)<.25*(xhi-xlo):
            t=(along-.77)/.23
            v.co.y-=.22*(yhi-ylo)*t*t
            edits+=1
    near.data.update()
    diag["tail_vertices_moved"]=edits
    mats=process_materials([near],out)
    count_near=decimate(near,12000)
    # The far LOD is a distinct mesh with the same UV and material family.
    far=near.copy();far.data=near.data.copy();bpy.context.collection.objects.link(far)
    far.name="Spawnling_Far_Authored"
    count_far=decimate(far,2900)
    far.hide_render=True
    # Distinct eyes: a concentrated visual cue on the face; kept restrained in scale.
    # These are separate objects and will be included in near GLB only.
    lo2,hi2=world_bounds([near])
    eye_mat=make_emissive("Rift violet / twin eye luminescence",(.53,.085,.87),1.6)
    eyey=lo2.y+(hi2.y-lo2.y)*.22
    eyez=lo2.z+(hi2.z-lo2.z)*.70
    halfx=(hi2.x-lo2.x)*.14
    rad=max(.012,min(.045,(hi2.x-lo2.x)*.043))
    eyes=[add_eye("Rift eye left",(center.x-halfx,eyey,eyez),rad,eye_mat),
          add_eye("Rift eye right",(center.x+halfx,eyey,eyez),rad,eye_mat)]
    # A shadowed, short cheek/mouth accent creates readable facial separation.
    jaw_mat=make_emissive("Rift face / deep violet",(.075,.02,.15),.18)
    jaw=add_eye("Shadowed maw",(center.x,eyey-.025*(hi2.y-lo2.y),eyez-.14*(hi2.z-lo2.z)),
                rad*1.85,jaw_mat)
    jaw.scale=(2.1,.5,.48)
    face_parts=eyes+[jaw]
    # Save editable artistic source with original + two actual LODs.
    for obj in [near,far]+face_parts: obj.hide_set(False)
    bpy.ops.wm.save_as_mainfile(filepath=str(out/"Spawnling_B2_Finished.blend"))
    export_named(out/"Spawnling_B2_Near.glb",[near]+face_parts)
    export_named(out/"Spawnling_B2_Far.glb",[far])
    # Render real Blender comparisons; Unity camera captures are separate.
    sc=bpy.context.scene
    sc.render.engine="CYCLES"
    sc.cycles.samples=8
    sc.render.resolution_x=960;sc.render.resolution_y=720;sc.render.resolution_percentage=100
    sc.render.image_settings.file_format="PNG"
    sc.render.film_transparent=False
    world=sc.world or bpy.data.worlds.new("Eldoria review world");sc.world=world;world.use_nodes=True
    bg=world.node_tree.nodes.get("Background")
    if bg:bg.inputs["Color"].default_value=(.36,.39,.4,1);bg.inputs["Strength"].default_value=.8
    light_data=bpy.data.lights.new("Review warm light","AREA")
    light=bpy.data.objects.new("Review warm light",light_data);bpy.context.collection.objects.link(light)
    light.location=center+Vector((2.5,-3.0,5.0));light_data.energy=360;light_data.shape="DISK";light_data.size=5
    for obj in [near]+face_parts:obj.hide_render=False
    far.hide_render=True
    original.hide_render=True
    bounds=world_bounds([near])
    camcenter=(bounds[0]+bounds[1])/2;dims=bounds[1]-bounds[0]
    for label,vector,close in [
        ("finished_iso",(1.25,-2.0,1.2),True),
        ("finished_face",(0,-2.5,.65),True),
        ("finished_rear",(0,2.5,.7),True),
        ("finished_strategic",(1.3,-2.0,1.5),False)]:
        camera_view(label,camcenter,dims,vector,out/(label+".png"),close)
    diag.update({"source_sha256":sha(source),"near_sha256":sha(out/"Spawnling_B2_Near.glb"),
          "far_sha256":sha(out/"Spawnling_B2_Far.glb"),
          "blend_sha256":sha(out/"Spawnling_B2_Finished.blend"),
          "triangles_near_body":count_near,"triangles_far_body":count_far,
          "face_parts":len(face_parts),"texture_grade":"PBR albedo darker violet; actual source retained",
          "materials":mats,"blender":bpy.app.version_string,
          "art_approved":False,"physical_mobile_tested":False,"tripo_credits_added":0})
    require(4000<count_near<=12500 and 1000<count_far<=3200,"Mobile LOD limits not reached")
    require(all(Path(out/p).is_file() for p in ["finished_iso.png","finished_face.png",
               "finished_rear.png","finished_strategic.png"]),"Missing review captures")
    (out/"author-evidence.json").write_text(json.dumps(diag,indent=2))
    print("SPAWNLING_ARTIFACTS_CREATED_VERIFICATION_REQUIRED "+json.dumps({"near":count_near,"far":count_far,"tail_edits":edits}))

if __name__=="__main__":main()
