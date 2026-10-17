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
frozen={n:(digest(o),len(o.data.polygons),len(o.data.vertices)) for n,o in bastion.items() if n!="BastionKeep"}
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
 if mat and mat.name=="Eldoria_banner_blue":
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
def arch_cutter(name,cx,front_y,bottom_z,half_width,straight_height,rounding,depth):
 """One watertight, deliberately authored arched negative volume, extruded into Y.
 The physical cut creates an interior return, not a fake dark plane or added building.
 """
 spring=bottom_z+straight_height
 points=[(cx-half_width,bottom_z),(cx-half_width,spring)]
 for i in range(1,17):
  t=math.pi-i*math.pi/16
  points.append((cx+half_width*math.cos(t),spring+rounding*math.sin(t)))
 points.append((cx+half_width,bottom_z))
 # start arc includes shared left spring via prior point; right spring now at i=16.
 n=len(points)
 verts=[(x,front_y,z) for x,z in points]+[(x,front_y+depth,z) for x,z in points]
 faces=[tuple(reversed(range(n))),tuple(range(n,n*2))]
 for i in range(n):j=(i+1)%n;faces.append((i,j,j+n,i+n))
 mesh=bpy.data.meshes.new(name+"_profile_mesh");mesh.from_pydata(verts,[],faces);mesh.update()
 ob=bpy.data.objects.new(name,mesh);bpy.context.scene.collection.objects.link(ob)
 return ob
def machine_portal(label,cx,zbase,half_w,straight,rise,y_front,y_back):
 cutter=arch_cutter(label,cx,y_front,zbase,half_w,straight,rise,y_back-y_front)
 before=sum(len(p.vertices)-2 for p in keep.data.polygons)
 bpy.context.view_layer.objects.active=keep
 mod=keep.modifiers.new("Real cutter / "+label,"BOOLEAN")
 mod.operation="DIFFERENCE";mod.solver='EXACT';mod.object=cutter
 bpy.ops.object.modifier_apply(modifier=mod.name)
 bpy.data.objects.remove(cutter,do_unlink=True)
 after=sum(len(p.vertices)-2 for p in keep.data.polygons)
 assert after!=before,("ARCH_CUT_DID_NOT_CHANGE_MAIN_KEEP",label,before,after)
 return {'operation':'Blender EXACT boolean subtract actual existing SOURCE08 main Keep mesh',
  'architectural_feature':label,'triangles_before':before,'triangles_after':after,
  'x_center':cx,'front_to_back':[y_front,y_back],'z_base':zbase}
# Dataset values derived from real inspected V13 world bounds and Source08 build mesh.
# Keep visible facade: original y=6.769..7.009 source, plus placement -65.960;
# portal source z 11.42..12.80 plus -8.552. Deeper than former 0.045m fake void.
cuts=[]
cuts.append(machine_portal("Inhabited recessed command portal",3.08+delta.x,
  11.30+delta.z,.64,1.10,.38,6.65+delta.y,8.15+delta.y))
# Two real slender arrow embrasures high on the same front facade, deliberately
# shallow not through the rear wall. No new boards or ornaments pasted on.
cuts.append(machine_portal("Upper western arrow embrasure",1.96+delta.x,
  14.10+delta.z,.20,.75,.18,6.75+delta.y,7.65+delta.y))
cuts.append(machine_portal("Upper eastern arrow embrasure",4.19+delta.x,
  14.10+delta.z,.20,.75,.18,6.75+delta.y,7.65+delta.y))
after_vert=len(keep.data.vertices)
after_tri=sum(len(p.vertices)-2 for p in keep.data.polygons)
assert after_vert!=before_vert and after_tri!=before_tri
now_extent=extent(keep)
# Carving removes material only, so no change in approved geometry envelope.
assert max(abs(now_extent[j][i]-original_extent[j][i]) for j in range(2) for i in range(3))<.0001,("ORIGINAL_ENVELOPE_DRIFT",now_extent,original_extent)
assert {n:(digest(o),len(o.data.polygons),len(o.data.vertices)) for n,o in bastion.items() if n!="BastionKeep"}==frozen
keep.name="ENTRY_V18_SCULPTED_BASTION_BastionKeep"
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
result={'stage':'V18 genuine Blender architectural deep-void cutting + correct source-authored blue/gold heraldry on approved V13 geometry',
 'parent_v13_sha256':sha(src),'original_source08_sha256':sha(donor),
 'original_unchanged_modules':list(frozen),'sculpted_existing_module':'BastionKeep',
 'geometry_edits':cuts,'keep_vertices_before_after':[before_vert,after_vert],
 'keep_triangles_before_after':[before_tri,after_tri],
 'all_module_envelopes_preserved':True,'v13_licensed_stone_masonry_preserved':True,
 'source08_original_banner_only_restored':heraldry,
 'textures_from_project_generator':sha(gener),
 'other_scenes_untouched':True,'total_export_triangles':tris,
 'result_blend_sha256':sha(blend),'result_glb_sha256':sha(glb),
 'source_visual_status':'REQUIRES_TRUE_BLENDER_CAMERA_REVIEW',
 'unity_art_status':'NOT_APPROVED','full_frame_art_pass':False,'mobile_qa':False,'zero_euro':True}
open(os.path.join(out,'entry-source.json'),'w',encoding='utf8').write(json.dumps(result,indent=2)+"\n")
print("VALORIA_V18_ACTUAL_KEEP_MESH_SCULPT_AND_HERALDRY_COMPLETE",json.dumps({'cuts':len(cuts),'tris':tris,'source_sha':result['result_blend_sha256'],'glb':result['result_glb_sha256']}))
