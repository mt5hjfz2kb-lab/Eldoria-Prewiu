"""Valoria V13: place the existing coherent BastionFamilyV1 on the upper certified
Bastion plot in a COPY of V12. No new architecture, kit or external textures.
Preserve V12 source, camera, 7 interaction volumes, 12 stairs and game runtime.
Blender 5.2 WORLD matrices checked against native measurement V13-survey.
"""
import bpy,os,json,hashlib,struct
from mathutils import Vector,Matrix
from mathutils.bvhtree import BVHTree
root=os.getcwd()
base=os.path.join(root,"docs/evidence/valoria-layered-construction-20261016")
out=os.path.join(base,"entry-zone-v13")
os.makedirs(out,exist_ok=True)
sha=lambda p:hashlib.sha256(open(p,'rb').read()).hexdigest()
source=os.path.join(base,"entry-zone-v12/ValoriaEntryZoneV12.blend")
surveyfile=os.path.join(base,"entry-zone-v13-survey/bastion-source-world-bounds.json")
family=os.path.join(root,"art-source/valoria/production/bastion-family-v1/BastionFamilyV1.blend")
assert sha(source)=="21c5681e20a498bcd97925520fd621907b023704a119e65890f2ec62c99081de"
survey=json.load(open(surveyfile,encoding='utf8'))
assert survey['source_v12_sha256']==sha(source) and survey['families']['bastion-family-v1']['sha256']==sha(family)
bpy.ops.wm.open_mainfile(filepath=source)
original=set(bpy.data.objects.keys())
terrain=next(o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith("ValoriaEntryTerrainV12"))
road=next(m for m in bpy.data.materials if m.name.startswith("ENTRY_road_masonry_PBR"))
rock=next(m for m in bpy.data.materials if m.name.startswith("ENTRY_existing_rock_PBR"))
trim=bpy.data.materials.get("Eldoria_trim") or road
assert any(n.type=='TEX_IMAGE' and n.image for n in road.node_tree.nodes)
assert any(n.type=='TEX_IMAGE' and n.image for n in rock.node_tree.nodes)
terrain.name="ValoriaEntryTerrainV13"
with bpy.data.libraries.load(family,link=False) as (src,dst):
    dst.objects=[name for name in src.objects if name in {p['name'] for p in survey['families']['bastion-family-v1']['objects']}]
pieces=[o for o in dst.objects if o is not None and o.type=='MESH']
assert len(pieces)==9,len(pieces)
for o in pieces:bpy.context.scene.collection.objects.link(o)
bpy.context.view_layer.update()
def bounds(vertices):
 return (Vector([min(p[i] for p in vertices) for i in range(3)]),
         Vector([max(p[i] for p in vertices) for i in range(3)]))
cert={e['name']:e for e in survey['families']['bastion-family-v1']['objects']}
worlds={}
for o in pieces:
    vertices=[o.matrix_world@v.co for v in o.data.vertices]
    lo,hi=bounds(vertices)
    assert max(abs(lo[i]-cert[o.name]['world_min_xyz'][i]) for i in range(3))<.01,("WORLD_MIN_CHANGED",o.name)
    assert max(abs(hi[i]-cert[o.name]['world_max_xyz'][i]) for i in range(3))<.01,("WORLD_MAX_CHANGED",o.name)
    worlds[o.name]=vertices
# Reference certified original Bastion hotspot in Unity:
# [native x=-0.08004951477, native y=4.48000145, native z=57.211299896].
# glTFast mirrors X once, Blender authored (x=-native x, y=-native z).
# Do not infer origin zero from source; translate the WHOLE nine-object family
# rigidly after comparing every donor WORLD transform to survey.
site=Vector((0.0800495,-57.211300,4.4800014))
meshpts=[p for world in worlds.values() for p in world]
low,high=bounds(meshpts)
family_cx=(low.x+high.x)/2
family_cy=(low.y+high.y)/2
family_min_z=low.z
bvh=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
samples=[]
for x,y in [(site.x,site.y),(site.x-8,site.y),(site.x+8,site.y),(site.x,site.y-4),(site.x,site.y+4),
            (site.x-13,site.y-1),(site.x+13,site.y+1)]:
    hit,norm,face,dist=bvh.ray_cast(Vector((x,y,18)),Vector((0,0,-1)),40)
    if hit is not None:samples.append({'xy':[x,y],'height':float(hit.z),'normal_z':float(norm.z)})
assert len(samples)>=5,('UPPER_TERRAIN_NOT_PRESENT',samples)
near=sorted(t['height'] for t in samples)
# Prefer native original interaction reservation floor when terrain is below it,
# but never make the entire fortress float metres above the natural bank.
ground=near[len(near)//2]
assert abs(ground-site.z)<4,('UNEXPECTED_UPPER_FLOOR',ground,site.z)
landmark_elevation=ground-.15
delta=Vector((site.x-family_cx,site.y-family_cy,landmark_elevation-family_min_z))
assert abs(delta.x)<50 and abs(delta.y)<110
placement=[]
for o in pieces:
    copy=o.copy()
    copy.data=o.data.copy()
    bpy.context.scene.collection.objects.link(copy)
    copy.name="ENTRY_V13_EXISTING_BASTION_"+o.name
    for v,p in zip(copy.data.vertices,worlds[o.name]):
        v.co=p+delta
    copy.parent=None
    copy.matrix_world=Matrix.Identity(4)
    copy.hide_render=False
    # Historic material nodes are NOT licensed for redistribution. Do not use
    # donor textures at all; replace every material slot with V7's approved
    # already packed PBR from the real CC0 photographic surface library.
    for idx,material in enumerate(copy.data.materials):
        mname=material.name.lower() if material else ""
        if "trim" in mname or "metal" in mname or "herald" in mname:copy.data.materials[idx]=trim
        elif "shadow" in mname or "roof" in mname or "timber" in mname:copy.data.materials[idx]=rock
        else:copy.data.materials[idx]=road
    assert all(m in (road,rock,trim) for m in copy.data.materials)
    lo,hi=bounds([v.co for v in copy.data.vertices])
    assert lo.z>=ground-1.0 and hi.z<=ground+12.0,("HEIGHT_GATE",copy.name,lo,hi,ground)
    placement.append({'name':copy.name,'source':o.name,'min_xyz':list(lo),'max_xyz':list(hi),
      'triangles':sum(len(p.vertices)-2 for p in copy.data.polygons)})
for o in pieces:bpy.data.objects.remove(o,do_unlink=True)
expected={"ENTRY_V13_EXISTING_BASTION_"+name for name in worlds}|{terrain.name}
actual=set(bpy.data.objects.keys())-original
assert actual==expected,("UNEXPECTED_MESH_MUTATION",sorted(actual-expected),sorted(expected-actual))
# Export the complete V12 scene AND the new architecture: Unity imports V13
# as a replacement art copy, retaining the original certified colliders.
for img in bpy.data.images:
 if img.source=='FILE' and not img.packed_file and os.path.isfile(bpy.path.abspath(img.filepath)):
    img.pack()
for o in bpy.data.objects:o.select_set(o.type=='MESH' and not o.hide_render)
bpy.context.view_layer.objects.active=terrain
blend=os.path.join(out,"ValoriaEntryZoneV13.blend")
glb=os.path.join(out,"ValoriaEntryZoneV13.glb")
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
raw=open(glb,'rb').read()
gltf=json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])
triangles=sum(gltf['accessors'][p['indices']]['count']//3 for n in gltf['nodes'] if 'mesh' in n for p in gltf['meshes'][n['mesh']]['primitives'])
assert 351374<triangles<410000,triangles
result={'stage':'V13 upper fortress from existing BastionFamily V1; no new assets',
 'source_v12_blend_sha256':sha(source),'source_family_sha256':sha(family),
 'source_inspection':'entry-zone-v13-survey/bastion-source-world-bounds.json',
 'result_blend_sha256':sha(blend),'result_glb_sha256':sha(glb),
 'triangles':triangles,'source_triangles':351374,'reused_bastion_mesh_count':len(placement),
 'bastion_objects':placement,'native_original_hotspot_xyz':[-.0800495,4.4800014,57.2112999],
 'site_blender_xyz':list(site),'terrain_samples':samples,'site_median_elevation':ground,
 'rigid_translation_xyz':list(delta),'donor_textures_excluded':True,
 'old_v12_geometry_unchanged':True,'source_colliders_unchanged':True,
 'licensed_material_source':'V12 packed V7 PBR CC0 stone/rock; donor family image-bearing materials excluded',
 'art_status':'CANDIDATE_NOT_APPROVED','gameplay':'REQUIRES_V13_NATIVE_VALIDATION',
 'zero_paid_spend':True,'full_frame_art_pass':False}
open(os.path.join(out,"entry-source.json"),'w',encoding='utf8').write(json.dumps(result,indent=2)+'\n')
print("VALORIA_V13_SOURCE_COMPLETE",json.dumps({k:v for k,v in result.items() if k not in ('bastion_objects','terrain_samples')}))
