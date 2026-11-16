#!/usr/bin/env python3
"""Merge static city meshes by material and geographic district for mobile draw-call reduction.
Input: authored glTF CityFinish stage 1..5; output: separate optimized GLB per stage.
Baking is off-line in Blender. No runtime procedural geometry and no change to gameplay.
A geometric proof only — actual Unity draw calls/memory/FPS still require testing.
"""
import bpy,bmesh,json,struct,math,os
from pathlib import Path
from collections import defaultdict
from mathutils import Matrix,Vector
ROOT=Path(__file__).resolve().parents[2]
SRC=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV2"
OUT=ROOT/"Unity/Assets/Art_Reboot/CC0System/DirectorV3Optimized"
OUT.mkdir(parents=True,exist_ok=True)
def zone(v):
    x,y,z=v
    if -14<x<14 and -11<y<16:return "Bastion"
    if -32<x<-14 and -18<y<1:return "Aserradero"
    if 15<x<33 and -20<y<1:return "Granero"
    if -34<x<-13 and 2<y<22:return "Cantera"
    if 15<x<34 and 3<y<22:return "Cuartel"
    return "TerrenoYEntorno"
def glbinfo(f):
    data=f.read_bytes()
    if data[:4]!=b"glTF":raise AssertionError(f)
    l=struct.unpack("<I",data[12:16])[0]
    return json.loads(data[20:20+l])
report={"source":"Director V2/V3 / one editable original Blender master",
        "all_geometry_author_owned":True,"runtime_generated_meshes":False,
        "unity_tested":False,"iphone_tested":False,"art_approved":False,
        "stages":[]}
for stage in range(1,6):
    source=SRC/("DirectorV2_ValoriaStage"+str(stage)+".glb")
    if not source.is_file():raise RuntimeError("Required physical unoptimized city GLB missing: "+str(source))
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.gltf(filepath=str(source),import_pack_images=True)
    original=[o for o in bpy.data.objects if o.type=="MESH"]
    if len(original)<400:raise RuntimeError("Too few source meshes")
    tris_before=sum(len(o.data.polygons) for o in original)
    mats_by_key={}
    groups={}
    positions={}
    input_mesh_count=len(original)
    for o in original:
        if len(o.data.materials)<1:
            raise RuntimeError("Source mesh lacks materials: "+o.name)
        pos=o.matrix_world.translation
        district=zone(pos)
        # Evaluated source may contain modifiers. Do not change originals.
        eval_o=o.evaluated_get(bpy.context.evaluated_depsgraph_get())
        temp=bpy.data.meshes.new_from_object(eval_o, preserve_all_data_layers=True,
                                             depsgraph=bpy.context.evaluated_depsgraph_get())
        temp.transform(o.matrix_world)
        for material_index in sorted({f.material_index for f in temp.polygons}):
            if material_index >= len(o.data.materials):
                raise RuntimeError("Bad material index in "+o.name)
            mat=o.data.materials[material_index]
            if mat is None:
                raise RuntimeError("Unassigned material in "+o.name)
            key=(district,mat.name)
            if key not in groups:
                groups[key]=bmesh.new()
                mats_by_key[key]=mat
            # Partition only polygons belonging to this material; one actual
            # merged mesh and one submesh per material/district, with UV kept.
            tmpbm=bmesh.new()
            tmpbm.from_mesh(temp)
            tmpbm.faces.ensure_lookup_table()
            for face in list(tmpbm.faces):
                if face.material_index!=material_index:
                    tmpbm.faces.remove(face)
                else:
                    face.material_index=0
            separated=bpy.data.meshes.new("batch_segment_"+o.name)
            tmpbm.to_mesh(separated)
            tmpbm.free()
            groups[key].from_mesh(separated)
            bpy.data.meshes.remove(separated)
        bpy.data.meshes.remove(temp)
    # Preserve original material objects during source deletion (Blender may otherwise orphan them).
    for m in mats_by_key.values():m.use_fake_user=True
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    output_objs=[]
    scene=bpy.context.scene
    roots={}
    for district in ("Bastion","Aserradero","Granero","Cantera","Cuartel","TerrenoYEntorno"):
        anchor=bpy.data.objects.new("ANCHOR_"+district,None)
        scene.collection.objects.link(anchor)
        anchor.empty_display_type="PLAIN_AXES"
        anchor.empty_display_size=.3
        roots[district]=anchor
    for (district,material_name),mesh_bm in groups.items():
        mesh=bpy.data.meshes.new("MESH_"+district+"_"+material_name)
        mesh_bm.to_mesh(mesh)
        mesh_bm.free()
        mesh.materials.append(mats_by_key[(district,material_name)])
        mesh.update(calc_edges=True)
        obj=bpy.data.objects.new("STATIC_"+district+"_"+material_name,mesh)
        scene.collection.objects.link(obj)
        obj.parent=roots[district]
        obj.matrix_world=Matrix.Identity(4)
        output_objs.append(obj)
    tris_after=sum(len(o.data.polygons) for o in output_objs)
    if tris_after!=tris_before:
        raise RuntimeError("Triangle/face count changed: "+str((stage,tris_before,tris_after)))
    # Data physically rebatched, no source references or vendor meshes.
    bpy.ops.object.select_all(action="DESELECT")
    for o in output_objs+[v for v in roots.values()]:
        o.select_set(True)
    path=OUT/("DirectorV3_BatchedStage"+str(stage)+".glb")
    bpy.ops.export_scene.gltf(filepath=str(path),export_format="GLB",
                              use_selection=True,export_apply=True)
    j=glbinfo(path)
    new_count=len(j.get("meshes",[]))
    if new_count>220:
        raise RuntimeError("Batching insufficient: "+str((stage,input_mesh_count,new_count)))
    entry={"stage":stage,"original_meshes":input_mesh_count,
           "optimized_meshes":new_count,"draw_mesh_reduction":round(1-new_count/input_mesh_count,4),
           "faces_before":tris_before,"faces_after":tris_after,
           "district_roots":list(roots.keys()),
           "materials":len(j.get("materials",[])),
           "file_bytes":path.stat().st_size}
    report["stages"].append(entry)
    print("ELDORIA_STATIC_CITY_BATCH_PASS",json.dumps(entry))
(OUT/"batch-report.json").write_text(json.dumps(report,indent=2)+"\n")
print("ELDORIA_STATIC_CITY_ALL_STAGES_PASS",len(report["stages"]))
