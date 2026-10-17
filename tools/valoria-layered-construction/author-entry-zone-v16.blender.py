"""Valoria V16: art-directed real CC0 masonry finishing of immutable V12 entry.
No new architecture or changes to terrain/camera/bridge/colliders. Specifically
treat 12 original gate/wing/WallFamily objects, preserve banner, trim and metal,
project masonry UVs consistently in WORLD metres and use archived Poly Haven
photo-scanned albedo + real normal/roughness maps in glTF-compatible PBR.
"""
import bpy,os,json,hashlib,struct,math
import numpy as np
from mathutils import Vector
root=os.getcwd()
base=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016')
out=os.path.join(base,'entry-zone-v16')
os.makedirs(out,exist_ok=True)
sha=lambda f:hashlib.sha256(open(f,'rb').read()).hexdigest()
source=os.path.join(base,'entry-zone-v12/ValoriaEntryZoneV12.blend')
assert sha(source)=='21c5681e20a498bcd97925520fd621907b023704a119e65890f2ec62c99081de'
cc0dir=os.path.join(base,'entry-zone-v7')
files={
 'diff':('aerial_rocks_02_diff.jpg','f759d2471cb6a4b3b37d0a5a249adbad1d4d48d67825f37622a603dac8d9bb22'),
 'normal':('aerial_rocks_02_nor_gl.png','53d4c2cac90801132a99054bd7d6132b230ddf9b12cc20c515caa7da0da6f01a'),
 'rough':('aerial_rocks_02_rough.jpg','f1808dec1ad6dd77881baa41813d9718bb93e9aa1dabdcd9d6dc9b7f45dd90da')}
for file,h in files.values():assert sha(os.path.join(cc0dir,file))==h,('UNLICENSED_RESOURCE_MISMATCH',file)
bpy.ops.wm.open_mainfile(filepath=source)
old_names={o.name for o in bpy.data.objects}
terrain=next(o for o in bpy.data.objects if o.type=='MESH' and o.name=='ValoriaEntryTerrainV12')
terrain.name='ValoriaEntryTerrainV16'
members=[o for o in bpy.data.objects if o.type=='MESH' and (
 o.name.startswith('ENTRY_LowerGate_') or o.name.startswith('ENTRY_V12_REUSED_WALLFAMILY_'))]
assert len(members)==12,[o.name for o in members]
# Decode authentic archive CC0 photogrammetry into a tileable, desaturated,
# neutral-warm dressed-stone albedo (not procedural recolor of object surfaces).
src=bpy.data.images.load(os.path.join(cc0dir,files['diff'][0]),check_existing=True)
w,h=src.size
assert w>=1024 and h>=1024
pix=np.asarray(src.pixels[:],dtype=np.float32).reshape(h,w,4)
orig=np.clip(pix[:,:,:3],0,1)
lum=orig[:,:,0]*.299+orig[:,:,1]*.587+orig[:,:,2]*.114
# Remove strong yellow grass/moss from rock scan, retain its actual geological
# occlusion and fine albedo variation. Warm-gray historic lime sandstone.
neutral=orig*.47+lum[:,:,None]*.53
tone=np.clip(.20+neutral*np.array([.58,.56,.54],dtype=np.float32),.15,.83)
tone=np.clip(tone,0,1)
rgba=np.concatenate((tone,np.ones((h,w,1),dtype=np.float32)),axis=2)
albedo=bpy.data.images.new('ENTRY_V16_CC0_DressedSandstone_2k',width=w,height=h,alpha=True)
albedo.colorspace_settings.name='sRGB'
albedo.pixels.foreach_set(rgba.ravel())
albedo.pack()
normal=bpy.data.images.load(os.path.join(cc0dir,files['normal'][0]),check_existing=True)
normal.colorspace_settings.name='Non-Color';normal.pack()
rough=bpy.data.images.load(os.path.join(cc0dir,files['rough'][0]),check_existing=True)
rough.colorspace_settings.name='Non-Color';rough.pack()
# The exporter/Unity understands regular glTF metallic-roughness and normal
# channels; avoid procedural shader nodes that get silently stripped on GLB.
pbr=bpy.data.materials.new('ENTRY_V16_CC0_ArchitecturalMasonry_UV_Metres')
pbr.use_nodes=True
nodes=pbr.node_tree.nodes;links=pbr.node_tree.links
principled=next(n for n in nodes if n.type=='BSDF_PRINCIPLED')
principled.inputs['Base Color'].default_value=(1,1,1,1)
principled.inputs['Metallic'].default_value=0.0
principled.inputs['Roughness'].default_value=.82
diffnode=nodes.new('ShaderNodeTexImage');diffnode.name='CC0 Dressed sandstone diffuse';diffnode.image=albedo
roughnode=nodes.new('ShaderNodeTexImage');roughnode.name='CC0 photographic stone roughness';roughnode.image=rough
normnode=nodes.new('ShaderNodeTexImage');normnode.name='CC0 photographic stone OpenGL normal';normnode.image=normal
normalmap=nodes.new('ShaderNodeNormalMap');normalmap.inputs['Strength'].default_value=.43
links.new(diffnode.outputs['Color'],principled.inputs['Base Color'])
links.new(roughnode.outputs['Color'],principled.inputs['Roughness'])
links.new(normnode.outputs['Color'],normalmap.inputs['Color'])
links.new(normalmap.outputs['Normal'],principled.inputs['Normal'])
# Preserve actual authored masonry block geometry and all six lower gate meshes,
# but replace stone-only material slots (never heraldry, trim, metal, timber).
reports=[];modified_uv_loops=0;changed_slots=0
for ob in members:
 mesh=ob.data
 old_mesh_verts=len(mesh.vertices)
 old_tri=sum(len(p.vertices)-2 for p in mesh.polygons)
 layer=mesh.uv_layers.get('ENTRY_V16_ArchitecturalMeters') or mesh.uv_layers.new(name='ENTRY_V16_ArchitecturalMeters')
 for face in mesh.polygons:
  n=face.normal
  ax,ay,az=abs(n.x),abs(n.y),abs(n.z)
  for li in face.loop_indices:
   vertex=mesh.vertices[mesh.loops[li].vertex_index]
   pos=ob.matrix_world@vertex.co
   # Area-weighted WORLD projection ensures 1 tiled rough stone map per 2.6m
   # on both sides of the gate, no scale discontinuity at wall joins.
   if az>=ax and az>=ay:u,v=pos.x*.38,pos.y*.38
   elif ay>=ax:u,v=pos.x*.38,pos.z*.38
   else:u,v=pos.y*.38,pos.z*.38
   layer.data[li].uv=(u,v)
   modified_uv_loops+=1
 mesh.uv_layers.active=layer
 replaced=[]
 for idx,old in enumerate(mesh.materials):
  if old is None:continue
  name=old.name
  if name in ('Eldoria_stone','ENTRY_road_masonry_PBR','ENTRY_existing_rock_PBR'):
   mesh.materials[idx]=pbr
   replaced.append(name)
   changed_slots+=1
 assert len(mesh.vertices)==old_mesh_verts
 assert sum(len(p.vertices)-2 for p in mesh.polygons)==old_tri
 reports.append({'object':ob.name,'changed_stone_slots':replaced,'triangles':old_tri,
     'world_texel_scale_u_per_m':.38})
assert changed_slots>=20 and modified_uv_loops>12000,(changed_slots,modified_uv_loops)
assert {o.name for o in bpy.data.objects}-old_names=={terrain.name}
for img in bpy.data.images:
 if img.source=='FILE' and not img.packed_file and os.path.isfile(bpy.path.abspath(img.filepath)):img.pack()
for o in bpy.data.objects:o.select_set(o.type=='MESH' and not o.hide_render)
bpy.context.view_layer.objects.active=terrain
blend=os.path.join(out,'ValoriaEntryZoneV16.blend')
glb=os.path.join(out,'ValoriaEntryZoneV16.glb')
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
raw=open(glb,'rb').read();gltf=json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])
triangles=sum(gltf['accessors'][p['indices']]['count']//3 for n in gltf['nodes'] if 'mesh' in n for p in gltf['meshes'][n['mesh']]['primitives'])
assert triangles==351374,triangles
manifest={'art_stage':'V16 real photogrammetry-based architectural PBR on original gate and six existing wall family parts',
 'source_v12_blend_sha256':sha(source),'result_blend_sha256':sha(blend),'result_glb_sha256':sha(glb),
 'triangles':triangles,'mesh_geometry_unchanged':True,
 'architecture_mesh_count':len(members),'changed_masonry_material_slots':changed_slots,
 'projected_actual_uv_loops':modified_uv_loops,'world_uv_units_per_m':.38,
 'cc0_source_files_sha256':{k:v[1] for k,v in files.items()},
 'pbr_albedo_resolution':[w,h],'photographic_normal_strength':.43,
 'preserved_gate_and_bridge_geometry':True,'source_gameplay_colliders_unchanged':True,
 'preserved_heraldry_trim_metal':True,'zero_paid_spend':True,
 'material_applied_to':reports,'full_frame_art_pass':False,
 'art_status':'CANDIDATE_VISUAL_UNAPPROVED_REQUIRES_UNITY_AB',
 'mobile_certified':False}
open(os.path.join(out,'entry-source.json'),'w',encoding='utf8').write(json.dumps(manifest,indent=2)+'\n')
print('VALORIA_V16_PROFESSIONAL_PBR_SOURCE_COMPLETE',json.dumps({'meshes':len(members),'slots':changed_slots,'uv':modified_uv_loops,'triangles':triangles}))
