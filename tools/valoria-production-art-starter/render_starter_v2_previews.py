import bpy,os,math,json
from mathutils import Vector
ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
OUT=os.path.join(ROOT,"pipeline","evidence","valoria-production-art-starter-v2-previews");os.makedirs(OUT,exist_ok=True)
ASSETS=["Valoria_MainGate_v2","Valoria_WallSegment_v2","Valoria_Tower_v2","Valoria_CivicHouse_v2","Valoria_Workshop_v2"]

def bb(objs):
 pts=[o.matrix_world@Vector(c) for o in objs for c in o.bound_box]
 lo=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
 hi=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
 return lo,hi

def render(a):
 bpy.ops.wm.read_factory_settings(use_empty=True)
 sc=bpy.context.scene
 if sc.world is None: sc.world=bpy.data.worlds.new("V2 Preview World")
 sc.world.use_nodes=True
 bg=sc.world.node_tree.nodes["Background"];bg.inputs["Color"].default_value=(.025,.035,.05,1);bg.inputs["Strength"].default_value=.55
 p=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","ProductionArt","StarterFamilyV2",a+".glb")
 bpy.ops.import_scene.gltf(filepath=p)
 objs=[o for o in sc.objects if o.type=="MESH"];lo,hi=bb(objs);ctr=(lo+hi)*.5;size=hi-lo
 for o in objs:o.location-=Vector((ctr.x,ctr.y,lo.z))
 bpy.context.view_layer.update();lo,hi=bb(objs);ctr=(lo+hi)*.5;extent=max(size.x,size.y,size.z)
 bpy.ops.mesh.primitive_plane_add(size=max(extent*5,3),location=(0,0,-.002))
 plane=bpy.context.object;m=bpy.data.materials.new("Ground");m.diffuse_color=(.07,.08,.07,1);plane.data.materials.append(m)
 for typ,loc,energy,sizeL in [("AREA",(4,-5,7),1100,5.0),("AREA",(-4,-1,4),450,4.0),("AREA",(0,4,5),700,3.0)]:
  bpy.ops.object.light_add(type=typ,location=loc);L=bpy.context.object;L.data.energy=energy;L.data.shape='DISK';L.data.size=sizeL
 bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type="ORTHO";cam.data.ortho_scale=max(extent*1.55,.6)
 cam.location=ctr+Vector((extent*1.65,-extent*2.05,extent*1.45));cam.rotation_euler=(ctr-cam.location).to_track_quat("-Z","Y").to_euler();sc.camera=cam
 sc.render.engine="BLENDER_EEVEE_NEXT";sc.render.resolution_x=720;sc.render.resolution_y=720;sc.render.resolution_percentage=100
 sc.render.image_settings.file_format="PNG";sc.render.filepath=os.path.join(OUT,a+".png");sc.view_settings.look="AgX - Medium High Contrast"
 bpy.ops.render.render(write_still=True)
 return {"asset":a,"png":os.path.relpath(sc.render.filepath,ROOT).replace("\\","/")}
rep={"schema_version":1,"assets":[render(a) for a in ASSETS]}
with open(os.path.join(OUT,"manifest.json"),"w") as f:json.dump(rep,f,indent=2)
print(json.dumps(rep,indent=2))
