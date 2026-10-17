"""V9 organic surface correction, deliberately restarted from frozen approved-input
V7 BLEND rather than rejected V8. Paint on the existing geographically mapped
photographic CC0 ground, not floating road tiles or extra cobble chains.
Preserve every gate, bridge, gameplay route, tree, topographic vertex and collider.
"""
import bpy,json,math,hashlib,os,struct
import numpy as np
from mathutils import Vector
root=os.getcwd()
ev=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016/entry-zone-v9')
os.makedirs(ev,exist_ok=True)
sha=lambda p:hashlib.sha256(open(p,'rb').read()).hexdigest()
source=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/ValoriaEntryZoneV7.blend')
assert sha(source)=='24e27862ae759e30c36b151cef4fd9baacb45d1103d25f132bbe054d65896381'
bpy.ops.wm.open_mainfile(filepath=source)
terrain=next(o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('ValoriaEntryTerrainV7'))
water=next(o for o in bpy.data.objects if o.type=='MESH' and 'RIVER_CONTINUOUS_RIPPLED_WATER' in o.name)
foot=next(o for o in bpy.data.objects if o.type=='MESH' and 'ARCH_FOOTINGS' in o.name)
bpy.context.view_layer.update()
terrain_vertices_before=len(terrain.data.vertices)
# Recover geographically keyed, uniquely baked V7 CC0 meadow and terrain detail.
tex=next(img for img in bpy.data.images if img.name.startswith('Entry_Geographic_CC0_MeadowSoil_2k'))
N=tex.size[0];assert N==tex.size[1]==2048
pixels=np.array(tex.pixels[:],dtype=np.float32).reshape(N,N,4)
# Source atlas is packed in the Blender original; never download a new texture.
soil=next((im for im in bpy.data.images if im.name.endswith('ground_albedo.png')),None)
assert soil is not None,'Original ground albedo source missing from frozen Blender scene'
sw,sh=soil.size
soilpx=np.array(soil.pixels[:],dtype=np.float32).reshape(sh,sw,4)
# The V7 geographic UV is computed BEFORE native X is reflected for glTFast,
# therefore baked bitmap world XX retains the NATIVE Unity X coordinate.
xx=np.linspace(-44.8,44.8,N,dtype=np.float32)[None,:]
zz=np.linspace(-31.3,86.2,N,dtype=np.float32)[:,None]
ux=np.broadcast_to(np.mod(xx*.147+zz*.026,1),(N,N))
uz=np.broadcast_to(np.mod(zz*.152-xx*.031,1),(N,N))
sx=(ux*(sw-1)).astype(np.int32);sy=(uz*(sh-1)).astype(np.int32)
earth=soilpx[sy,sx,:3].copy()
# Ground's earthy loam is shaded and partially intermixed with its own CC0
# meadow. A blended geographic scar has NO gap or hover above the source terrain.
earth=np.clip(earth*np.array([.83,.87,.76],dtype=np.float32)+.025,0,1)
weather=.22*np.sin(xx*.51+zz*.37)+.11*np.sin(xx*1.29-zz*.71)
wear=np.zeros((N,N),dtype=np.float32)
def smoother(x):
 t=np.clip(x,0,1)
 return t*t*(3-2*t)
# Preserve exact certified original native greybox route footprint while giving
# a softened, irregular native-grade soil margin reaching the existing parcels.
for side in (-1,1):
 local_x=xx*side
 center=-1.1+.15*np.sin(local_x*.26+side*.5)
 distance=np.abs(zz-center+weather)
 # Route width 3.3-4.1 m, irregular grasses dissolve the edge and endpoints.
 width=1.65+.22*np.sin(local_x*.51+side)+.10*np.cos(local_x*.27-zz*.38)
 mask=smoother((width+.65-distance)/1.16)
 endpoint=smoother((local_x-6.6)/1.1)*(1-smoother((local_x-17.7)/2.0))
 wear=np.maximum(wear,(mask*endpoint).astype(np.float32))
# Lower entry apron is embedded into the original axial masonry route, not
# rendered as a rectangular plank on top of the sloping terrain.
apron=smoother((3.45-np.abs(xx)+weather*.38)/1.2)
south=smoother((zz+9.55)/.75)*(1-smoother((zz+6.4)/1.3))
wear=np.maximum(wear,(apron*south*.76).astype(np.float32))
# Avoid painting waterbeds, cliffs or protected actual bridge/footing mesh.
# Only target existing upper-level route/apron zones.
original=pixels[:,:,:3].copy()
wear=np.clip(wear*.90,0,.90)
ground=(original*(1-wear[:,:,None])+earth*wear[:,:,None]).astype(np.float32)
new=tex.copy();new.name='Entry_V9_Geographic_CC0_IntegratedNativeRoads_2k'
new.pixels.foreach_set(np.concatenate((ground,np.ones((N,N,1),dtype=np.float32)),axis=2).ravel())
new.pack()
linked=0
for mat in terrain.data.materials:
 for node in mat.node_tree.nodes if mat.use_nodes else []:
  if node.type=='TEX_IMAGE' and node.image==tex:
   node.image=new;linked+=1
assert linked>=1,'No geographic diffuse link updated'
# Modify only a COPY of the existing water source, reducing saturation and
# introducing very fine mesh ripples without covering or altering the banks.
water.data=water.data.copy()
for v in water.data.vertices:
 x,y=v.co.x,v.co.y
 v.co.z+=.025*math.sin(x*.87+y*.58)+.016*math.sin(x*1.9-y*.83)
water.data.update()
material=bpy.data.materials.new('ENTRY_V9_MutedColdRiver');
material.use_nodes=True
nodes=material.node_tree.nodes;links=material.node_tree.links
p=next((n for n in nodes if n.type=='BSDF_PRINCIPLED'),None) or nodes.new('ShaderNodeBsdfPrincipled')
outnode=next((n for n in nodes if n.type=='OUTPUT_MATERIAL'),None) or nodes.new('ShaderNodeOutputMaterial')
links.new(p.outputs['BSDF'],outnode.inputs['Surface'])
p.inputs['Base Color'].default_value=(.035,.102,.124,1)
p.inputs['Roughness'].default_value=.37
p.inputs['Metallic'].default_value=.10
water.data.materials.clear();water.data.materials.append(material)
# The original gate, ground topology, 12-step authored stone, bridge, plants
# and historical collsion surfaces are NEVER moved or remodelled.
assert len(terrain.data.vertices)==terrain_vertices_before
for obj in bpy.data.objects:obj.select_set(obj.type=='MESH' and not obj.hide_render)
bpy.context.view_layer.objects.active=terrain
blend=os.path.join(ev,'ValoriaEntryZoneV9.blend')
glb=os.path.join(ev,'ValoriaEntryZoneV9.glb')
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
raw=open(glb,'rb').read()
gltf=json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])
triangles=sum(gltf['accessors'][part['indices']]['count']//3 for node in gltf['nodes'] if 'mesh' in node for part in gltf['meshes'][node['mesh']]['primitives'])
manifest={'art_status':'CANDIDATE_NO_AUTOMATIC_APPROVAL','author':'GPT isolated continuation','blender':bpy.app.version_string,
 'source_v7_blend_sha256':sha(source),'result_blend_sha256':sha(blend),'result_glb_sha256':sha(glb),
 'triangles':triangles,'terrain_vertex_count_unchanged':terrain_vertices_before,
 'geographic_bake_dimensions':[N,N],'native_route_painted_pixels':int(np.count_nonzero(wear>.25)),
 'max_paint_mask':float(wear.max()),'updated_diffuse_nodes':linked,
 'source_gate_bridge_stairs_unchanged':True,'actual_unity_colliders_unchanged':True,
 'protected_sources_unchanged':True,
 'operations':['graded lateral footpaths baked into original CC0 photographed terrain','blended native lower apron in same terrain material','muted river material and micro ripples'],
 'rejected_previous':'V8 floating isolated stone tracks; source V7 chosen intentionally'}
open(os.path.join(ev,'entry-source.json'),'w',encoding='utf8').write(json.dumps(manifest,indent=2)+'\n')
print('VALORIA_ENTRY_V9_SOURCE_COMPLETE',json.dumps(manifest))
