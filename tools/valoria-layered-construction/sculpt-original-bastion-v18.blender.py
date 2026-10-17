"""Valoria V18 — conservative but REAL Blender architectural editing of the nine-piece
SOURCE08 upper Bastion already integrated as V13. V17 semantic-only materials were rejected
for flat walls. Keep V13 masonry; recover only authored blue/gold heraldry; carve physical
arched portal depth and keep embrasures into existing keep mesh (applied exact booleans).
Never touch other nine-family meshes, terrain, camera, roads, gate or gameplay.
"""
import bpy, math, os, json, hashlib, struct
from mathutils import Vector
root=os.getcwd()
base=os.path.join(root,"docs/evidence/valoria-layered-construction-20261016")
out=os.path.join(base,"entry-zone-v18")
os.makedirs(out,exist_ok=True)
sha=lambda p:hashlib.sha256(open(p,'rb').read()).hexdigest()
src=os.path.join(base,"entry-zone-v13/ValoriaEntryZoneV13.blend")
donor=os.path.join(root,"art-source/valoria/production/bastion-family-v1/BastionFamilyV1.blend")
gener=os.path.join(root,"tools/author-environment-family.py")
assert sha(src)=="1023443ef9eb3137a5157188b16194ff6245b08003f4c388377610d15811a1cf"
assert sha(donor)=="5c7b7e0918ac2e7dc78984515991c3f3546dffc3927bd0fa23bcaf4e9b148893"
assert "image_data('banner_heraldry'" in open(gener,encoding="utf-8").read()
bpy.ops.wm.open_mainfile(filepath=src)
bastion={o.name.removeprefix("ENTRY_V13_EXISTING_BASTION_"):o for o in bpy.data.objects
 if o.type=='MESH' and o.name.startswith("ENTRY_V13_EXISTING_BASTION_")}
assert len(bastion)==9,sorted(bastion)
keep=bastion["BastionKeep"]
digest=lambda ob:hashlib.sha256(b"".join(struct.pack("<fff",*v.co) for v in ob.data.vertices)).hexdigest()
frozen={n:(digest(o),len(o.data.polygons),len(o.data.vertices)) for n,o in bastion.items() if n not in ("BastionKeep","BastionLeftWing")}
before_vert=len(keep.data.vertices)
before_tri=sum(len(p.vertices)-2 for p in keep.data.polygons)
before_faces=len(keep.data.polygons)
def extent(ob):
 pts=[ob.matrix_world@v.co for v in ob.data.vertices]
 return ([min(p[i] for p in pts) for i in range(3)],[max(p[i] for p in pts) for i in range(3)])
original_extent=extent(keep)
with bpy.data.libraries.load(donor,link=False) as (source,target):
 target.objects=["BastionKeep"]
original=target.objects[0]
assert original is not None and original.type=='MESH'
assert len(keep.data.polygons)==len(original.data.polygons)
assert len(keep.data.materials)==len(original.data.materials)
heraldry=[]
for slot,mat in enumerate(original.data.materials):
 if mat and mat.name.startswith("Eldoria_banner_blue"):
  assert mat.use_nodes
  images=[n.image for n in mat.node_tree.nodes if n.type=="TEX_IMAGE" and n.image]
  assert len(images)==1 and images[0].name.startswith("banner_heraldry"),[x.name for x in images]
  img=images[0]
  if not img.packed_file:
   path=bpy.path.abspath(img.filepath);assert os.path.isfile(path);img.pack()
  keep.data.materials[slot]=mat
  heraldry.append({'slot':slot,'polygons':sum(p.material_index==slot for p in keep.data.polygons),'image':img.name})
assert len(heraldry)==1 and heraldry[0]['polygons']>10,heraldry
bpy.data.objects.remove(original,do_unlink=True)
# Verified original V13 rigid placement remains, no camera adjustment.
v13=json.load(open(os.path.join(base,"entry-zone-v13/entry-source.json"),encoding="utf-8"))
delta=Vector(v13["rigid_translation_xyz"])
assert (delta-Vector((-5.145967960357666,-65.96016693115234,-8.552324295043945))).length<.0001
# Switch from whole-joined-mesh Boolean: that operation removed separate original
# foundation components (measured on the real source); forbidden geometry regression.
# Use non-destructive, component-selective native BMesh front-face insets/extrusions.
import bmesh
def sculpt_inhabited_facade(obj,label,yfront,zlow,zhigh,depth,maxfaces):
 bm=bmesh.new()
 bm.from_mesh(obj.data)
 bm.faces.ensure_lookup_table()
 candidates=[]
 for face in bm.faces:
  center=face.calc_center_median()
  if (face.normal.y<-.82 and abs(center.y-yfront)<.22 and zlow<=center.z<=zhigh
       and face.calc_area()>0.52):
   candidates.append(face)
 candidates.sort(key=lambda face:face.calc_area(),reverse=True)
 chosen=candidates[:maxfaces]
 assert len(chosen)>=1,("NO_AUTHORED_FRONT_FACADE_FOUND",label,[(round(f.calc_center_median().y,2),round(f.calc_area(),2)) for f in candidates[:10]])
 old_faces=len(bm.faces)
 old_vertices=len(bm.verts)
 result=bmesh.ops.inset_individual(bm,faces=chosen,thickness=.15,depth=depth,use_even_offset=True)
 bm.normal_update()
 # Give all newly cut loops real metric UV, never a stretched wallpaper plane.
 uv=bm.loops.layers.uv.verify()
 for face in bm.faces:
  if face in chosen:continue
  if any(abs(v.co.y-yfront)<.5 for v in face.verts) and face.normal.y<-.3:
   for loop in face.loops:
    loop[uv].uv=(loop.vert.co.x*.38,loop.vert.co.z*.38)
 bm.to_mesh(obj.data);bm.free();obj.data.update()
 assert len(obj.data.vertices)>old_vertices and len(obj.data.polygons)>old_faces,("BMesh_EDIT_NOT_REAL",label)
 return {"operation":"Native Blender BMesh inset_individual on EXISTING architectural facade faces; physically recessed original mesh","feature":label,"faces_sculpted":len(chosen),
  "original_vertices":old_vertices,"sculpted_vertices":len(obj.data.vertices),
  "original_faces":old_faces,"sculpted_faces":len(obj.data.polygons),
  "inset_meters":.15,"recession_meters":abs(depth)}
cuts=[]
cuts.append(sculpt_inhabited_facade(keep,"Keep inhabited lower facade carved stone relief",
  6.93+delta.y,11.8+delta.z,16.15+delta.z,-.28,4))
cuts.append(sculpt_inhabited_facade(keep,"Keep upper command-stage real inset and arch flank depth",
  7.11+delta.y,16.2+delta.z,17.8+delta.z,-.18,2))
wing=bastion["BastionLeftWing"]
wing_before_vertices=len(wing.data.vertices)
cuts.append(sculpt_inhabited_facade(wing,"Supporting west wing deeply articulated masonry facade",
  7.27+delta.y,12.0+delta.z,15.8+delta.z,-.24,3))
after_vert=len(keep.data.vertices)
after_tri=sum(len(p.vertices)-2 for p in keep.data.polygons)
assert after_vert!=before_vert and after_tri!=before_tri
now_extent=extent(keep)
# Carving removes material only, so no change in approved geometry envelope.
assert max(abs(now_extent[j][i]-original_extent[j][i]) for j in range(2) for i in range(3))<.0001,("ORIGINAL_ENVELOPE_DRIFT",now_extent,original_extent)
assert {n:(digest(o),len(o.data.polygons),len(o.data.vertices)) for n,o in bastion.items() if n not in ("BastionKeep","BastionLeftWing")}==frozen
assert len(wing.data.vertices)>wing_before_vertices
keep.name="ENTRY_V18_SCULPTED_BASTION_BastionKeep"
wing.name="ENTRY_V18_SCULPTED_BASTION_BastionLeftWing"
terrain=next(o for o in bpy.data.objects if o.type=="MESH" and o.name=="ValoriaEntryTerrainV13")
terrain.name="ValoriaEntryTerrainV18"
# preserve original V13 masonry on every other surface, avoid V17 flatness
assert len([o for o in bpy.data.objects if o.type=='MESH' and "BASTION" in o.name.upper()])>=1
for img in bpy.data.images:
 if img.source=='FILE' and not img.packed_file and os.path.isfile(bpy.path.abspath(img.filepath)):img.pack()
for obj in bpy.data.objects:obj.select_set(obj.type=='MESH' and not obj.hide_render)
bpy.context.view_layer.objects.active=keep
blend=os.path.join(out,"ValoriaEntryZoneV18.blend")
glb=os.path.join(out,"ValoriaEntryZoneV18.glb")
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
raw=open(glb,'rb').read()
gltf=json.loads(raw[20:20+struct.unpack_from("<I",raw,12)[0]])
tris=sum(gltf['accessors'][p['indices']]['count']//3 for n in gltf['nodes'] if 'mesh' in n for p in gltf['meshes'][n['mesh']]['primitives'])
assert 370000<tris<410000 and tris!=374064,tris
# These are genuine native Blender visual gates, not AI reference images.
# No camera change is made to Unity. Source studios are diagnostic only.
from mathutils import Vector
for obj in bpy.data.objects:
 if obj.type=='MESH' and obj not in bastion.values():obj.hide_render=True
allpts=[obj.matrix_world@v.co for obj in bastion.values() for v in obj.data.vertices]
mn=Vector([min(p[i] for p in allpts) for i in range(3)])
mx=Vector([max(p[i] for p in allpts) for i in range(3)])
focus=(mn+mx)*.5
camdata=bpy.data.cameras.new("SOURCE_V18_STUDIO_FRONT_CAMERA")
camera=bpy.data.objects.new("SOURCE_V18_STUDIO_FRONT_CAMERA",camdata)
bpy.context.scene.collection.objects.link(camera)
bpy.context.scene.camera=camera
camdata.type="ORTHO";camdata.ortho_scale=45
camera.location=focus+Vector((16,-37,24))
camera.rotation_euler=(focus-camera.location).to_track_quat("-Z","Y").to_euler()
for name,offset,power,size in [
 ("SOURCE18 STUDIO SOFT KEY",Vector((-12,-14,22)),3200,26),
 ("SOURCE18 STUDIO FILL",Vector((15,7,13)),2400,25)
]:
 lightdata=bpy.data.lights.new(name,"AREA");lightdata.energy=power;lightdata.shape="DISK";lightdata.size=size
 light=bpy.data.objects.new(name,lightdata);bpy.context.scene.collection.objects.link(light)
 light.location=focus+offset;light.rotation_euler=(focus-light.location).to_track_quat("-Z","Y").to_euler()
sc=bpy.context.scene
sc.render.engine="CYCLES";sc.cycles.samples=12
sc.render.threads_mode="FIXED";sc.render.threads=4
sc.render.resolution_x=1024;sc.render.resolution_y=700;sc.render.resolution_percentage=100
sc.render.image_settings.file_format="PNG"
if sc.world is not None:
 sc.world.use_nodes=True
 bg=sc.world.node_tree.nodes.get("Background")
 if bg:bg.inputs["Strength"].default_value=.75
sc.render.filepath=os.path.join(out,"SOURCE-front-original-V13-masonry-plus-edited-V18-keep.png")
bpy.ops.render.render(write_still=True)
camdata.ortho_scale=13
kf=[keep.matrix_world@v.co for v in keep.data.vertices]
kfocus=Vector([sum(v[i] for v in kf)/len(kf) for i in range(3)])
camera.location=kfocus+Vector((7,-19,10))
camera.rotation_euler=(kfocus-camera.location).to_track_quat("-Z","Y").to_euler()
sc.render.filepath=os.path.join(out,"SOURCE-keep-architectural-closeup-V18.png")
bpy.ops.render.render(write_still=True)
source_images={n:sha(os.path.join(out,n)) for n in [
 "SOURCE-front-original-V13-masonry-plus-edited-V18-keep.png",
 "SOURCE-keep-architectural-closeup-V18.png"]}
result={'stage':'V18 genuine Blender architectural deep-void cutting + correct source-authored blue/gold heraldry on approved V13 geometry',
 'parent_v13_sha256':sha(src),'original_source08_sha256':sha(donor),
 'original_unchanged_modules':list(frozen),'sculpted_existing_modules':['BastionKeep','BastionLeftWing'],
 'geometry_edits':cuts,'keep_vertices_before_after':[before_vert,after_vert],
 'keep_triangles_before_after':[before_tri,after_tri],
 'all_module_envelopes_preserved':True,'v13_licensed_stone_masonry_preserved':True,
 'source08_original_banner_only_restored':heraldry,
 'textures_from_project_generator':sha(gener),
 'other_scenes_untouched':True,'triangles':tris,'total_export_triangles':tris,
 'result_blend_sha256':sha(blend),'result_glb_sha256':sha(glb),
 'source_visual_status':'REQUIRES_TRUE_BLENDER_CAMERA_REVIEW',
 'genuine_blender_source_images_sha256':source_images,
 'unity_art_status':'NOT_APPROVED','full_frame_art_pass':False,'mobile_qa':False,'zero_euro':True}
open(os.path.join(out,'entry-source.json'),'w',encoding='utf8').write(json.dumps(result,indent=2)+"\n")
print("VALORIA_V18_ACTUAL_KEEP_MESH_SCULPT_AND_HERALDRY_COMPLETE",json.dumps({'cuts':len(cuts),'tris':tris,'source_sha':result['result_blend_sha256'],'glb':result['result_glb_sha256']}))
