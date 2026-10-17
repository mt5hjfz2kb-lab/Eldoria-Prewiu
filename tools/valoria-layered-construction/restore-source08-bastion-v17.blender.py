"""Recover original SOURCE08 semantic Blender materials on the already-integrated nine meshes.
V13 reused professional SOURCE08 geometry, but replaced EVERY donor material with generic
road/rock/trim due to a mistaken unverified external-texture assumption. Original texture
images were generated in-project by tools/author-environment-family.py (numpy recipes).
No new objects or architecture, no external licenses, no new user-cost resources.
Candidate only; ART_APPROVAL requires actual identical-camera Unity A/B and human review.
"""
import bpy,os,json,hashlib,struct
ROOT=os.getcwd()
BASE=os.path.join(ROOT,"docs/evidence/valoria-layered-construction-20261016")
OUT=os.path.join(BASE,"entry-zone-v17")
os.makedirs(OUT,exist_ok=True)
sha=lambda f:hashlib.sha256(open(f,"rb").read()).hexdigest()
input_blend=os.path.join(BASE,"entry-zone-v13/ValoriaEntryZoneV13.blend")
donor_path=os.path.join(ROOT,"art-source/valoria/production/bastion-family-v1/BastionFamilyV1.blend")
generator_path=os.path.join(ROOT,"tools/author-environment-family.py")
assert sha(input_blend)=="1023443ef9eb3137a5157188b16194ff6245b08003f4c388377610d15811a1cf"
assert sha(donor_path)=="5c7b7e0918ac2e7dc78984515991c3f3546dffc3927bd0fa23bcaf4e9b148893"
generator=open(generator_path,encoding="utf-8").read()
for evidence in ("def surface_maps(kind)","image_data('banner_heraldry'","tex={k:surface_maps(k)","im.pack()"):
 assert evidence in generator,("GENERATOR_SOURCE_PROVENANCE_MISSING",evidence)
bpy.ops.wm.open_mainfile(filepath=input_blend)
copies={o.name.removeprefix("ENTRY_V13_EXISTING_BASTION_"):o for o in bpy.data.objects
 if o.type=="MESH" and o.name.startswith("ENTRY_V13_EXISTING_BASTION_")}
assert len(copies)==9,sorted(copies)
mesh_vertex_digest=lambda ob:hashlib.sha256(b"".join(struct.pack("<fff",*v.co) for v in ob.data.vertices)).hexdigest()
before={n:(mesh_vertex_digest(o),len(o.data.vertices),sum(len(p.vertices)-2 for p in o.data.polygons),
         tuple(p.material_index for p in o.data.polygons)) for n,o in copies.items()}
terrain=next(o for o in bpy.data.objects if o.type=="MESH" and o.name=="ValoriaEntryTerrainV13")
terrain.name="ValoriaEntryTerrainV17"
with bpy.data.libraries.load(donor_path,link=False) as (src,dst):
 dst.objects=[n for n in src.objects if n in copies]
donors={ob.name:ob for ob in dst.objects if ob is not None and ob.type=="MESH"}
assert set(donors)==set(copies),(sorted(donors),sorted(copies))
for name,donor in donors.items():
 target=copies[name]
 assert len(target.data.materials)==len(donor.data.materials),("MATERIAL_SLOT_DRIFT",name)
 assert len(target.data.polygons)==len(donor.data.polygons),("FACE_DRIFT",name)
 for slot,source_mat in enumerate(donor.data.materials):
  assert source_mat is not None
  assert source_mat.name.startswith("Eldoria_"),(name,source_mat.name)
  target.data.materials[slot]=source_mat
 target.name="ENTRY_V17_RECOVERED_SOURCE08_"+name
after={name:(mesh_vertex_digest(ob),len(ob.data.vertices),sum(len(p.vertices)-2 for p in ob.data.polygons),
       tuple(p.material_index for p in ob.data.polygons)) for name,ob in copies.items()}
assert after==before,"SOURCE08_GEOMETRY_OR_SEMANTIC_FACE_ASSIGNMENT_CHANGED"
allowed=("masonry_","stone_grain_","hero_masonry_","timber_","paving_","banner_heraldry")
images=set()
material_map={}
for name,ob in copies.items():
 ms=[]
 for mat in ob.data.materials:
  assert mat is not None and mat.use_nodes,("UNEXPECTED_NODELESS_MATERIAL",name)
  ms.append(mat.name)
  for node in mat.node_tree.nodes:
   if node.type!="TEX_IMAGE" or node.image is None:continue
   img=node.image
   assert img.name.startswith(allowed),("UNVERIFIED_DONOR_TEXTURE",img.name)
   if not img.packed_file:
    filename=bpy.path.abspath(img.filepath)
    assert os.path.isfile(filename),("UNPACKED_DONOR_IMAGE_MISSING",img.name)
    img.pack()
   assert img.packed_file,("SOURCE08_ORIGINAL_TEXTURE_NOT_PACKED",img.name)
   images.add(img.name)
 material_map[name]=ms
assert len(set(m for v in material_map.values() for m in v))>=9,material_map
for ob in donors.values():bpy.data.objects.remove(ob,do_unlink=True)
for im in bpy.data.images:
 if im.source=="FILE" and not im.packed_file and os.path.isfile(bpy.path.abspath(im.filepath)):im.pack()
for ob in bpy.data.objects:ob.select_set(ob.type=="MESH" and not ob.hide_render)
bpy.context.view_layer.objects.active=terrain
blend=os.path.join(OUT,"ValoriaEntryZoneV17.blend")
glb=os.path.join(OUT,"ValoriaEntryZoneV17.glb")
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
bpy.ops.export_scene.gltf(filepath=glb,export_format="GLB",use_selection=True,export_apply=True)
raw=open(glb,"rb").read()
gltf=json.loads(raw[20:20+struct.unpack_from("<I",raw,12)[0]])
tris=sum(gltf["accessors"][p["indices"]]["count"]//3
 for node in gltf["nodes"] if "mesh" in node for p in gltf["meshes"][node["mesh"]]["primitives"])
assert tris==374064,("V13_GEOMETRY_CHANGED",tris)
# Two honest SOURCE diagnostic renders, not claims about final Unity lighting.
# The original authored 9-mesh silhouette/materials are kept identical.
from mathutils import Vector
sitepoints=[o.matrix_world @ v.co for o in copies.values() for v in o.data.vertices]
bboxlo=Vector((min(v.x for v in sitepoints),min(v.y for v in sitepoints),min(v.z for v in sitepoints)))
bboxhi=Vector((max(v.x for v in sitepoints),max(v.y for v in sitepoints),max(v.z for v in sitepoints)))
focus=(bboxlo+bboxhi)*.5
for o in bpy.data.objects:
 if o.type=="MESH" and o not in list(copies.values()):o.hide_render=True
cam_data=bpy.data.cameras.new("SOURCE08_V17_STUDIO_DIAGNOSTIC_CAMERA")
cam=bpy.data.objects.new("SOURCE08_V17_STUDIO_DIAGNOSTIC_CAMERA",cam_data)
bpy.context.scene.collection.objects.link(cam)
cam.location=focus+Vector((22,39,22))
cam.rotation_euler=(focus-cam.location).to_track_quat("-Z","Y").to_euler()
cam_data.type="ORTHO";cam_data.ortho_scale=48
bpy.context.scene.camera=cam
for name,offset,power,size in [
 ("Source08 Studio Key",Vector((-18,15,36)),3000,28),
 ("Source08 Studio Fill",Vector((20,12,14)),1700,28)
]:
 ld=bpy.data.lights.new(name,"AREA");ld.energy=power;ld.shape="DISK";ld.size=size
 light=bpy.data.objects.new(name,ld);bpy.context.scene.collection.objects.link(light)
 light.location=focus+offset
 light.rotation_euler=(focus-light.location).to_track_quat("-Z","Y").to_euler()
scene=bpy.context.scene
scene.render.engine="CYCLES"
scene.cycles.samples=12
scene.render.threads_mode="FIXED";scene.render.threads=4
scene.render.resolution_x=960;scene.render.resolution_y=640;scene.render.resolution_percentage=100
scene.render.image_settings.file_format="PNG"
if scene.world is not None:
 scene.world.use_nodes=True
 bg=scene.world.node_tree.nodes.get("Background")
 if bg is not None:bg.inputs["Strength"].default_value=.6
neutral=bpy.data.materials.new("SOURCE08_V17_SOURCE_CLAY_DIAGNOSTIC")
neutral.diffuse_color=(.66,.67,.68,1)
neutral.use_nodes=True
bsdf=neutral.node_tree.nodes.get("Principled BSDF")
bsdf.inputs["Base Color"].default_value=(.66,.67,.68,1)
bsdf.inputs["Roughness"].default_value=.88
scene.view_layers[0].material_override=neutral
scene.render.filepath=os.path.join(OUT,"source-clay-threequarter.png")
bpy.ops.render.render(write_still=True)
scene.view_layers[0].material_override=None
scene.render.filepath=os.path.join(OUT,"source-source08-material-threequarter.png")
bpy.ops.render.render(write_still=True)
source_previews={
 "source-clay-threequarter.png":sha(os.path.join(OUT,"source-clay-threequarter.png")),
 "source-source08-material-threequarter.png":sha(os.path.join(OUT,"source-source08-material-threequarter.png"))
}
report={
 "stage":"V17 RECOVER original SOURCE08 nine-material semantic hierarchy (no new building/geometry)",
 "input_v13_blend_sha256":sha(input_blend),"original_family_blend_sha256":sha(donor_path),
 "source_texture_generator":"tools/author-environment-family.py",
 "source_texture_generator_sha256":sha(generator_path),
 "texture_provenance":"Actual source image_data/surface_maps numpy + generated banner, all generated in Eldoria and packed; no third-party texture relied upon for the Bastion",
 "reused_real_professional_model":True,"bastion_module_count":len(copies),
 "material_roles":material_map,"original_image_names":sorted(images),"source_previews_sha256":source_previews,
 "input_v13_geometry_unchanged":after==before,
 "result_blend_sha256":sha(blend),"result_glb_sha256":sha(glb),"triangles":tris,
 "budget_eur":0,"status":"BLENDER_SOURCE_CANDIDATE_REQUIRES_TRUE_UNITY_AB",
 "source_art_production_pass_in_2026_10_05_does_not_certify_new_SHARP_scene":True,
 "art_approved":False,"full_frame_art_pass":False,"gameplay_pass":False,"mobile_pass":False}
open(os.path.join(OUT,"entry-source.json"),"w",encoding="utf-8").write(json.dumps(report,indent=2)+"\n")
print("VALORIA_SOURCE08_MATERIAL_HIERARCHY_RECOVERED",json.dumps({"modules":len(copies),"roles":len(set(m for v in material_map.values() for m in v)),"images":len(images),"triangles":tris}))
