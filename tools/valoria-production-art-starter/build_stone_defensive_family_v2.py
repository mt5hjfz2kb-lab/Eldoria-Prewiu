import bpy, os, json, hashlib, math
from mathutils import Vector

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
SRC=os.path.join(ROOT,"art-source","valoria","production","stone-defensive-family-v2")
OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","ProductionArt","StoneDefensiveFamilyV2")
PRE=os.path.join(ROOT,"pipeline","evidence","valoria-production-art-stone-defensive-v2")
for p in (SRC,OUT,PRE):os.makedirs(p,exist_ok=True)
DONOR_WALL=os.path.join(ROOT,"Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/HighStraightWall.glb")
DONOR_CORNER=os.path.join(ROOT,"Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/CornerWallL.glb")

def sha(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for b in iter(lambda:f.read(1048576),b""):h.update(b)
 return h.hexdigest()

def reset():
 bpy.ops.wm.read_factory_settings(use_empty=True)

def import_glb(path,prefix):
 before=set(bpy.context.scene.objects)
 bpy.ops.import_scene.gltf(filepath=path)
 objs=[o for o in bpy.context.scene.objects if o not in before and o.type=="MESH"]
 for i,o in enumerate(objs):o.name=f"{prefix} · {i:02d}"
 return objs

def bounds(objs):
 pts=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box]
 mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
 mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
 return mn,mx

def normalize(objs,target_x,target_z):
 mn,mx=bounds(objs);s=mx-mn
 scale=min(target_x/max(s.x,s.y,.001),target_z/max(s.z,.001))
 for o in objs:o.scale*=scale
 bpy.context.view_layer.update();mn,mx=bounds(objs)
 ctr=Vector(((mn.x+mx.x)*.5,(mn.y+mx.y)*.5,mn.z))
 for o in objs:o.location-=ctr
 bpy.context.view_layer.update()

def duplicate_group(objs,prefix,loc=(0,0,0),rot=0,scale=(1,1,1)):
 out=[]
 for i,o in enumerate(objs):
  n=o.copy();n.data=o.data.copy();bpy.context.scene.collection.objects.link(n)
  n.name=f"{prefix} · {i:02d}";n.location+=Vector(loc);n.rotation_euler[2]+=math.radians(rot)
  n.scale.x*=scale[0];n.scale.y*=scale[1];n.scale.z*=scale[2];out.append(n)
 return out

def export(aid,objs):
 bpy.ops.object.select_all(action="DESELECT")
 for o in objs:o.select_set(True)
 p=os.path.join(OUT,aid+".glb")
 bpy.ops.export_scene.gltf(filepath=p,export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT")
 return p

def preview(aid,objs):
 sc=bpy.context.scene
 if sc.world is None:sc.world=bpy.data.worlds.new("Stone Defensive Preview")
 sc.world.use_nodes=True;bg=sc.world.node_tree.nodes["Background"];bg.inputs["Color"].default_value=(.045,.05,.055,1);bg.inputs["Strength"].default_value=.65
 mn,mx=bounds(objs);ctr=(mn+mx)*.5;span=max(*(mx-mn),.5)
 bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.lens=58;cam.location=ctr+Vector((span*1.45,-span*1.8,span*1.05));cam.rotation_euler=(ctr-cam.location).to_track_quat("-Z","Y").to_euler();sc.camera=cam
 bpy.ops.object.light_add(type="AREA",location=ctr+Vector((span,-span,span*1.6)));key=bpy.context.object;key.data.energy=1100;key.data.size=span*1.4
 bpy.ops.object.light_add(type="AREA",location=ctr+Vector((-span*.8,-span*.1,span)));fill=bpy.context.object;fill.data.energy=450;fill.data.size=span
 sc.render.engine="BLENDER_EEVEE_NEXT";sc.render.resolution_x=720;sc.render.resolution_y=720;sc.render.resolution_percentage=100;sc.render.image_settings.file_format="PNG";sc.render.filepath=os.path.join(PRE,aid+".png");sc.view_settings.look="AgX - Medium High Contrast";bpy.ops.render.render(write_still=True)

def metrics(objs):
 t=v=0;m=set()
 for o in objs:
  o.data.calc_loop_triangles();t+=len(o.data.loop_triangles);v+=len(o.data.vertices)
  for x in o.data.materials:
   if x:m.add(x.name)
 return {"objects":len(objs),"vertices":v,"triangles":t,"materials":sorted(m)}

def wall():
 reset();base=import_glb(DONOR_WALL,"Wall donor");normalize(base,4.5,2.3)
 return base

def tower():
 reset();base=import_glb(DONOR_WALL,"Tower wall grammar");normalize(base,2.25,2.55)
 # Four real donor facades around a compact square tower. No primitive wall faces.
 parts=[]
 for rot,loc in [(0,(0,-.76,0)),(180,(0,.76,0)),(90,(-.76,0,0)),(-90,(.76,0,0))]:
  parts+=duplicate_group(base,"Tower facade",loc,rot,(1,.72,1.12))
 # upper crown from same stone grammar, stepped inward
 for rot,loc in [(0,(0,-.60,2.05)),(180,(0,.60,2.05)),(90,(-.60,0,2.05)),(-90,(.60,0,2.05))]:
  parts+=duplicate_group(base,"Tower crown",loc,rot,(.78,.58,.52))
 # donor source originals not part of final assembly
 for o in base:bpy.data.objects.remove(o,do_unlink=True)
 return parts

def gate():
 reset();base=import_glb(DONOR_WALL,"Gate wall grammar");normalize(base,2.05,2.45)
 parts=[]
 # flanking real stone pylons, leaving a true central opening
 for sx in (-1,1):
  for rot,loc in [(0,(sx*1.32,-.34,0)),(180,(sx*1.32,.34,0))]:
   parts+=duplicate_group(base,"Gate pylon",loc,rot,(.66,.72,1.15))
 # upper bridge and crown are donor geometry compressed vertically, not cubes
 bridge=duplicate_group(base,"Gate bridge",(0,0,2.08),0,(1.18,.68,.34));parts+=bridge
 crown=duplicate_group(base,"Gate crown",(0,0,2.72),0,(1.32,.72,.24));parts+=crown
 for o in base:bpy.data.objects.remove(o,do_unlink=True)
 return parts

built=[]
for aid,builder in [("Valoria_WallSegment_StoneDefV2",wall),("Valoria_Tower_StoneDefV2",tower),("Valoria_MainGate_StoneDefV2",gate)]:
 objs=builder();p=export(aid,objs);preview(aid,objs);built.append({"asset_id":aid,"glb":os.path.relpath(p,ROOT).replace("\\","/"),"sha256":sha(p),"geometry":metrics(objs)})

# canonical source by importing the three final GLBs with spacing
reset()
for i,b in enumerate(built):
 objs=import_glb(os.path.join(ROOT,b["glb"]),b["asset_id"])
 for o in objs:o.location.x+=(i-1)*6
blend=os.path.join(SRC,"Valoria_StoneDefensiveFamily_v2.blend");bpy.ops.wm.save_as_mainfile(filepath=blend)
report={"schema_version":1,"classification":"TEMPORARY","authoring_method":"source_reauthoring_from_certified_stone_architecture","donors":["HighStraightWall.glb"],"blend":os.path.relpath(blend,ROOT).replace("\\","/"),"blend_sha256":sha(blend),"assets":built}
with open(os.path.join(SRC,"stone-defensive-family-v2-build-report.json"),"w") as f:json.dump(report,f,indent=2)
print(json.dumps(report,indent=2))
