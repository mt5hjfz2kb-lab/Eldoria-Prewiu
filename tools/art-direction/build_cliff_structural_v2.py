"""Structural authored cliff candidate: modify the actual cliff silhouette, not add a talus carpet.

Runs inside Blender on the prior Qwen-directed .blend variant. The approved source
and first-stage Qwen/BlenderMCP evidence remain untouched.
"""
import bpy, math, json, pathlib, hashlib
from mathutils import Vector
ROOT=pathlib.Path(__file__).resolve().parents[2]
BASE=ROOT/"art-source/valoria/variants/cliff_talus_transition_v1"
OUT=ROOT/"art-source/valoria/variants/cliff_structural_v2"
OUT.mkdir(parents=True,exist_ok=True)
direction=json.loads((BASE/"direction.json").read_text(encoding="utf-8"))
plan=direction["plan"]
source=BASE/"cliff_talus_transition_v1.blend"
assert source.exists(), "Qwen-directed Blender source from previous pass required"
bpy.ops.wm.open_mainfile(filepath=str(source))
bed=next(o for o in bpy.data.objects if o.type=="MESH" and o.name.startswith("Continuous angular escarpment bedrock"))
crest=next(o for o in bpy.data.objects if o.type=="MESH" and o.name.startswith("Continuous soil and grass cliff crest"))
assert len(bed.data.vertices)>1000
# The older stage shifted hidden underside only. Fix the dominant skyline and
# stone wall under it as one joint geological displacement, keeping x seams.
facet_count=max(4,min(9,int(plan["large_facets"])))
amplitude=max(.4,min(1.3,float(plan["fault_amplitude_m"])))
def field(x,y):
    edge=max(0.,min(1.,(6.-abs(x))/.85))
    sx=(x+6.)/12.
    # Low-frequency asymmetric silhouette, no periodic uniform sawtooth.
    rise=(.34*math.sin(sx*math.pi*2.7+.28)
          +.17*math.sin(sx*math.pi*6.1+1.3)
          +.12*math.sin(sx*math.pi*11.3+.9))
    return edge*rise*max(.65,min(1.,amplitude))
def front_offset(x,z):
    seam=max(0.,min(1.,(6.-abs(x))/.8))
    depth=max(0.,min(1.,-z/4.2))
    broad=(.50*math.sin(x*.79+1.1)+.23*math.sin(x*1.73-.2)
           +.17*math.cos(x*2.61+.3))
    return seam*depth*(.42+.42*amplitude)*broad
stats={}
for obj in (bed,crest):
    changed=0
    low=1e9;high=-1e9
    # x is along the 12m modular seam; z is world up in Blender.
    for v in obj.data.vertices:
        x,y,z=v.co
        if abs(x)>=5.98: continue
        dz=field(x,y)
        # Material continuity: grass and bedrock share the same rise at lip;
        # fade towards the bottom to protect the modular vertical anchor.
        vertical=max(.0,min(1.,(z+4.2)/1.5))
        v.co.z=z+dz*vertical
        if obj is bed and y < -.7:
            v.co.y=y-front_offset(x,z)
        changed+=1;low=min(low,dz);high=max(high,dz)
    obj.data.update()
    stats[obj.name]={"changed_vertices":changed,"crest_displacement_span":high-low}
# Do not export the old standalone talus apron: it was visually rejected.
reject=[o for o in bpy.data.objects if o.name.startswith("cliff_talus_transition_v1__")]
for obj in reject:bpy.data.objects.remove(obj,do_unlink=True)
# Reuse the textured 42 source talus parts, but group them into authored
# fractured flank zones rather than a line of isolated white spheres.
talus=[o for o in bpy.context.scene.objects if o.type=="MESH" and "talus" in o.name.lower()]
for index,obj in enumerate(talus):
    x,y,z=obj.location
    if abs(x)<5.6:
        obj.location.z=z+field(x,y)*max(0,min(1,(z+4.2)/1.5))
        obj.location.y=y-.13*math.sin(index*1.37)
# Preserve meaningful source materials, origin and 12m joins.
if not bpy.context.scene.camera:
    camdata=bpy.data.cameras.new("Structural cliff review");cam=bpy.data.objects.new("Structural cliff review",camdata)
    bpy.context.scene.collection.objects.link(cam);bpy.context.scene.camera=cam
cam=bpy.context.scene.camera
cam.location=(15,-22,14)
cam.rotation_euler=(Vector((0,0,-1.7))-cam.location).to_track_quat("-Z","Y").to_euler()
cam.data.type="ORTHO";cam.data.ortho_scale=19
sc=bpy.context.scene
sc.render.resolution_x=1200;sc.render.resolution_y=800;sc.render.resolution_percentage=100
sc.render.filepath=str(OUT/"after-structural-blender.png")
bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"cliff_structural_v2.blend"))
bpy.ops.object.select_all(action="DESELECT")
selected=[]
for obj in bpy.context.scene.objects:
    if obj.type=="MESH":
        obj.select_set(True);selected.append(obj)
assert bed in selected and crest in selected and len(selected)>10
bpy.context.view_layer.objects.active=bed
target=OUT/"cliff_structural_v2.glb"
bpy.ops.export_scene.gltf(filepath=str(target),export_format="GLB",use_selection=True,export_materials="EXPORT")
assert target.stat().st_size>10000
stats["candidate_sha256"]=hashlib.sha256(target.read_bytes()).hexdigest()
stats["source_blend_sha256"]=hashlib.sha256(source.read_bytes()).hexdigest()
stats["system_prompt_sha256"]=direction["artist_system_prompt_sha256"]
stats["selected_meshes"]=len(selected)
stats["visual_approval"]=False
stats["mobile_approval"]=False
stats["next_gate"]="Unity side-by-side replacement, same camera with neutral clay and actual materials"
(OUT/"structural-proof.json").write_text(json.dumps(stats,indent=2),encoding="utf-8")
print("ELDORIA_STRUCTURAL_CLIFF_CANDIDATE_PASS",json.dumps(stats),flush=True)
