import bpy, os, json, hashlib, math
from mathutils import Vector

ROOT=os.environ.get("GITHUB_WORKSPACE",os.getcwd())
SRC=os.path.join(ROOT,"art-source","valoria","production","starter-family-v3")
OUT=os.path.join(ROOT,"Unity","Assets","Eldoria","Resources","Valoria","ProductionArt","StarterFamilyV3")
PRE=os.path.join(ROOT,"pipeline","evidence","valoria-production-art-starter-v3")
for p in (SRC,OUT,PRE): os.makedirs(p,exist_ok=True)

DONORS={
 "gate":"Unity/Assets/Bublik/Simple Modular Castle Assets/Meshes/Stone_Gate.fbx",
 "wall":"Unity/Assets/Bublik/Simple Modular Castle Assets/Meshes/Stone_Wall.fbx",
 "tower":"Unity/Assets/Bublik/Simple Modular Castle Assets/Meshes/Stone_Tower.fbx",
 "civic":"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Town_Building_Administrative _01a.fbx",
 "workshop":"Unity/Assets/EmaceArt/Slavic World Free/Meshes/EA03_Village_OutBuilding_Shed_03b.fbx"
}

def sha(p):
 h=hashlib.sha256()
 with open(p,"rb") as f:
  for c in iter(lambda:f.read(1024*1024),b""):h.update(c)
 return h.hexdigest()

def clear():
 bpy.ops.wm.read_factory_settings(use_empty=True)

def import_fbx(rel,prefix):
 p=os.path.join(ROOT,rel)
 before=set(bpy.context.scene.objects)
 bpy.ops.import_scene.fbx(filepath=p,automatic_bone_orientation=False)
 objs=[o for o in bpy.context.scene.objects if o not in before and o.type=="MESH"]
 if not objs: raise RuntimeError("No meshes imported "+rel)
 for i,o in enumerate(objs):o.name=f"{prefix} · {i:02d} · {o.name}"
 return objs

def bounds(objs):
 pts=[]
 for o in objs:
  for c in o.bound_box:pts.append(o.matrix_world@Vector(c))
 mn=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
 mx=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
 return mn,mx

def normalize(objs,target_span,target_height):
 mn,mx=bounds(objs);size=mx-mn
 s=min(target_span/max(size.x,size.y,.001),target_height/max(size.z,.001))
 for o in objs:o.scale*=s
 bpy.context.view_layer.update()
 mn,mx=bounds(objs)
 center=Vector(((mn.x+mx.x)*.5,(mn.y+mx.y)*.5,mn.z))
 for o in objs:o.location-=center
 bpy.context.view_layer.update()

def move_collection(objs,name):
 col=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(col)
 for o in objs:
  for c in list(o.users_collection):c.objects.unlink(o)
  col.objects.link(o)
 return col

def metrics(objs):
 tris=verts=0;mats=set()
 for o in objs:
  o.data.calc_loop_triangles();tris+=len(o.data.loop_triangles);verts+=len(o.data.vertices)
  for m in o.data.materials:
   if m:mats.add(m.name)
 mn,mx=bounds(objs)
 return {"objects":len(objs),"vertices":verts,"triangles":tris,"materials":sorted(mats),
         "bounds":[round(v,5) for v in (*mn,*mx)]}

def export(col,aid):
 bpy.ops.object.select_all(action="DESELECT")
 for o in col.objects:o.select_set(True)
 p=os.path.join(OUT,aid+".glb")
 bpy.ops.export_scene.gltf(filepath=p,export_format="GLB",use_selection=True,export_apply=True,export_yup=True,export_materials="EXPORT")
 return p

def preview(col,aid):
 sc=bpy.context.scene
 if sc.world is None:sc.world=bpy.data.worlds.new("V3 Preview World")
 sc.world.use_nodes=True
 bg=sc.world.node_tree.nodes.get("Background");bg.inputs["Color"].default_value=(.035,.045,.06,1);bg.inputs["Strength"].default_value=.55
 for c in bpy.data.collections:
  c.hide_render=(c!=col)
 objs=[o for o in col.objects if o.type=="MESH"];mn,mx=bounds(objs);center=(mn+mx)*.5;span=max(*(mx-mn),.5)
 bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.lens=58;cam.location=center+Vector((span*1.45,-span*1.85,span*1.1));cam.rotation_euler=(center-cam.location).to_track_quat("-Z","Y").to_euler();sc.camera=cam
 bpy.ops.object.light_add(type="AREA",location=center+Vector((span,-span,span*1.7)));key=bpy.context.object;key.data.energy=850;key.data.size=span*1.2
 bpy.ops.object.light_add(type="AREA",location=center+Vector((-span*.8,-span*.2,span)));fill=bpy.context.object;fill.data.energy=420;fill.data.size=span
 bpy.ops.mesh.primitive_plane_add(size=span*5,location=(center.x,center.y,mn.z-.015));ground=bpy.context.object
 gm=bpy.data.materials.new("Preview Ground");gm.diffuse_color=(.08,.085,.075,1);ground.data.materials.append(gm)
 # ensure ground renders although outside source collection
 for c in ground.users_collection:c.hide_render=False
 sc.render.engine="BLENDER_EEVEE";sc.render.resolution_x=720;sc.render.resolution_y=720;sc.render.resolution_percentage=100;sc.render.image_settings.file_format="PNG"
 sc.view_settings.look="AgX - Medium High Contrast";sc.render.filepath=os.path.join(PRE,aid+".png")
 bpy.ops.render.render(write_still=True)
 bpy.data.objects.remove(cam,do_unlink=True);bpy.data.objects.remove(key,do_unlink=True);bpy.data.objects.remove(fill,do_unlink=True);bpy.data.objects.remove(ground,do_unlink=True)

clear()
specs=[
 ("Valoria_MainGate_v3","gate",4.2,3.2,"PRIMARY"),
 ("Valoria_WallSegment_v3","wall",4.6,2.2,"PRIMARY"),
 ("Valoria_Tower_v3","tower",2.8,4.2,"PRIMARY"),
 ("Valoria_CivicHouse_v3","civic",3.5,3.4,"SECONDARY"),
 ("Valoria_Workshop_v3","workshop",3.1,2.8,"SECONDARY")
]
built=[]
for aid,key,span,height,role in specs:
 objs=import_fbx(DONORS[key],aid);normalize(objs,span,height);col=move_collection(objs,aid);p=export(col,aid);preview(col,aid)
 built.append({"asset_id":aid,"role":role,"function":key,"donor":DONORS[key],"geometry":metrics(objs),"glb_path":os.path.relpath(p,ROOT).replace("\\","/"),"glb_sha256":sha(p)})
 # hide finished collection before next import
 col.hide_viewport=True;col.hide_render=True

# canonical source must preserve all five final assemblies
for c in bpy.data.collections:
 if c.name.startswith("Valoria_"):c.hide_viewport=False;c.hide_render=False
blend=os.path.join(SRC,"Valoria_StarterFamily_v3.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend);blend_sha=sha(blend)

for b in built:
 man={
  "asset_id":b["asset_id"],"classification":"TEMPORARY","role":b["role"],"function":b["function"],
  "source":{"tool":"Blender","tool_version":"4.0.2","path":os.path.relpath(blend,ROOT).replace("\\","/"),"sha256":blend_sha,"reproducible":True,"authoring_method":"coherent_family_source_reauthoring_v3","donor_source":b["donor"]},
  "geometry":{"triangles":b["geometry"]["triangles"],"materials":len(b["geometry"]["materials"]),"uv0":True,"normals":True},
  "materials":[{"slot":"preserved-source","family":"Eldoria_Stone","maps":["source-materials pending phase D harmonization"]}],
  "export":{"glb_path":b["glb_path"],"sha256":b["glb_sha256"],"scale":1.0,"forward_axis":"+Z","up_axis":"+Y"},
  "unity":{"resource_path":"Valoria/ProductionArt/StarterFamilyV3/"+b["asset_id"],"owns_gameplay_collider":False,"owns_hotspot":False},
  "evidence":{"isolated_preview":"pipeline/evidence/valoria-production-art-starter-v3/"+b["asset_id"]+".png","zoom9":"","mobile":"","technical_verdict":"PENDING_UNITY","visual_verdict":"PENDING_VISUAL","promotion_state":"CANDIDATE_NOT_PRODUCTION"}
 }
 with open(os.path.join(SRC,b["asset_id"]+".production-art.json"),"w") as f:json.dump(man,f,indent=2)
with open(os.path.join(SRC,"starter-family-v3-build-report.json"),"w") as f:json.dump({"blend_sha256":blend_sha,"assets":built},f,indent=2)
print(json.dumps({"blend_sha256":blend_sha,"assets":built},indent=2))
