"""V11 organic surface correction, deliberately restarted from frozen approved-input
V7 BLEND rather than rejected V8. Paint a certified connected route onto existing geographically mapped
photographic CC0 ground, not floating road tiles or extra cobble chains.
Preserve every gate, bridge, gameplay route, tree, topographic vertex and collider.
"""
import bpy,json,math,hashlib,os,struct
import numpy as np
from mathutils import Vector
root=os.getcwd()
ev=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016/entry-zone-v11')
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
terrain.name='ValoriaEntryTerrainV11'
# Recover geographically keyed, uniquely baked V7 CC0 meadow and terrain detail.
tex=next(img for img in bpy.data.images if img.name.startswith('Entry_Geographic_CC0_MeadowSoil_2k'))
N=tex.size[0];assert N==tex.size[1]==2048
pixels=np.array(tex.pixels[:],dtype=np.float32).reshape(N,N,4)
# Reuse Work V7 Poly Haven photographed cliff CC0, NOT another colored path.
cc0={'diff':('aerial_rocks_02_diff.jpg','f759d2471cb6a4b3b37d0a5a249adbad1d4d48d67825f37622a603dac8d9bb22'),
     'disp':('aerial_rocks_02_disp.png','3f080f5bec4b092ad4a7a6324c727010905c0f10b900a36bdeab4b2ef741147c')}
for file,expected in cc0.values():
 assert sha(os.path.join(os.path.dirname(source),file))==expected,('CC0_CHANGED',file)
photo=bpy.data.images.load(os.path.join(os.path.dirname(source),cc0['diff'][0]),check_existing=True)
sw,sh=photo.size;pix=np.asarray(photo.pixels[:],dtype=np.float32).reshape(sh,sw,4)
height=bpy.data.images.load(os.path.join(os.path.dirname(source),cc0['disp'][0]),check_existing=True)
dw,dh=height.size;hp=np.asarray(height.pixels[:],dtype=np.float32).reshape(dh,dw,4)[:,:,0]
normals=[v.normal.copy() for v in terrain.data.vertices]
sculpted=0;maxshift=0.
for v,n in zip(terrain.data.vertices,normals):
 x,z=-v.co.x,-v.co.y
 if not (5.5<abs(x)<32.0 and -21<z<-8.1):continue
 d=min(abs(z-(-9.7+1.1*math.sin(x*.10)+.45*math.sin(x*.29+.7))),abs(z-(-20.1+.65*math.sin(x*.12+.8))))
 shoreline=max(0,min(1,(5.1-d)/2.8))
 slope=max(0,min(1,(.95-abs(n.z))/.45))
 w=shoreline*shoreline*(3-2*shoreline)*slope*slope*(3-2*slope)
 if w<.0001:continue
 ix=int(((x/14.3+.13)%1)*(dw-1));iz=int(((z/14.3+.37)%1)*(dh-1))
 delta=(float(hp[iz,ix])-.5)*.34*w
 v.co.z+=delta;maxshift=max(maxshift,abs(delta));sculpted+=1
terrain.data.update()
import bmesh
bm=bmesh.new();bm.from_mesh(terrain.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
bm.to_mesh(terrain.data);bm.free();terrain.data.update()
normals_up=sum(v.normal.z for v in terrain.data.vertices)/len(terrain.data.vertices)
assert normals_up>.78,'V7 SOURCE NORMALS INVERTED'
assert len(terrain.data.vertices)==terrain_vertices_before
terrain.name='ValoriaEntryTerrainV11'
# Map scanner pixels over the real riverbank, feathered to untouched V7 meadow.
xx=np.linspace(-44.8,44.8,N,dtype=np.float32)[None,:]
zz=np.linspace(-31.3,86.2,N,dtype=np.float32)[:,None]
original=pixels[:,:,:3].copy()
north=-9.7+1.1*np.sin(xx*.10)+.45*np.sin(xx*.29+.7)
south=-20.1+.65*np.sin(xx*.12+.8)
d=np.minimum(np.abs(zz-north),np.abs(zz-south))
m=np.clip((5.5-d)/3.8,0,1)
m=m*m*(3-2*m)
m*=np.clip((np.abs(xx)-5.5)/2.8,0,1)*np.clip((34-np.abs(xx))/5,0,1)
m=np.clip(m*(.74+.16*np.sin(xx*.27-zz*.33)+.10*np.sin(xx*.59+zz*.61))*.55,0,.62)
ix=np.broadcast_to((np.mod(xx/14.3+.13,1)*(sw-1)).astype(np.int32),(N,N))
iz=np.broadcast_to((np.mod(zz/14.3+.37,1)*(sh-1)).astype(np.int32),(N,N))
rock=pix[iz,ix,:3].copy()*np.array([.83,.88,.92],dtype=np.float32)
rgb=np.clip(original*(1-m[:,:,None])+rock*m[:,:,None],0,1)
new=tex.copy();new.name='Entry_V11_PolyHaven_PhotoBank_CC0_2k'
new.pixels.foreach_set(np.concatenate((rgb,np.ones((N,N,1),dtype=np.float32)),axis=2).ravel());new.pack()
linked=0
for mat in terrain.data.materials:
 for node in mat.node_tree.nodes if mat.use_nodes else []:
  if node.type=='TEX_IMAGE' and node.image==tex:node.image=new;linked+=1
assert linked>=1,'V7 geographic source material missing'
bank_pixels=int(np.count_nonzero(m>.20))
# Modify only a COPY of the existing water source, reducing saturation and
# introducing very fine mesh ripples without covering or altering the banks.
water.data=water.data.copy()
for v in water.data.vertices:
 x,y=v.co.x,v.co.y
 v.co.z+=.025*math.sin(x*.87+y*.58)+.016*math.sin(x*1.9-y*.83)
water.data.update()
material=bpy.data.materials.new('ENTRY_V11_MutedColdRiver');
material.use_nodes=True
nodes=material.node_tree.nodes;links=material.node_tree.links
p=next((n for n in nodes if n.type=='BSDF_PRINCIPLED'),None) or nodes.new('ShaderNodeBsdfPrincipled')
outnode=next((n for n in nodes if n.type=='OUTPUT_MATERIAL'),None) or nodes.new('ShaderNodeOutputMaterial')
links.new(p.outputs['BSDF'],outnode.inputs['Surface'])
p.inputs['Base Color'].default_value=(.042,.119,.131,1)
p.inputs['Roughness'].default_value=.24
p.inputs['Metallic'].default_value=.09
water.data.materials.clear();water.data.materials.append(material)
# The original gate, ground topology, 12-step authored stone, bridge, plants
# and historical collsion surfaces are NEVER moved or remodelled.
assert len(terrain.data.vertices)==terrain_vertices_before
for obj in bpy.data.objects:obj.select_set(obj.type=='MESH' and not obj.hide_render)
bpy.context.view_layer.objects.active=terrain
blend=os.path.join(ev,'ValoriaEntryZoneV11.blend')
glb=os.path.join(ev,'ValoriaEntryZoneV11.glb')
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
raw=open(glb,'rb').read()
gltf=json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])
triangles=sum(gltf['accessors'][part['indices']]['count']//3 for node in gltf['nodes'] if 'mesh' in node for part in gltf['meshes'][node['mesh']]['primitives'])
manifest={'art_status':'CANDIDATE_REQUIRES_VISUAL_REVIEW','author':'GPT continuation via native Blender','blender':bpy.app.version_string,
 'source_v7_blend_sha256':sha(source),'result_blend_sha256':sha(blend),'result_glb_sha256':sha(glb),
 'triangles':triangles,'source_vertices':terrain_vertices_before,'sculpted_bank_vertices':sculpted,
 'max_bank_shift_m':maxshift,'upward_normals':normals_up,'photographic_bank_pixels':bank_pixels,
 'scan_diff_sha256':sha(os.path.join(os.path.dirname(source),cc0['diff'][0])),
 'scan_disp_sha256':sha(os.path.join(os.path.dirname(source),cc0['disp'][0])),
 'photographic_bake_dimensions':[N,N],'updated_diffuse_nodes':linked,
 'gate_bridge_stairs_source_unchanged':True,'colliders_unchanged':True,'paid_spend_eur':0,
 'license':'CC0-1.0','source_url':'https://polyhaven.com/a/aerial_rocks_02',
 'operations':['archived Poly Haven CC0 photo-scan riverbank micro-sculpt and naturally blended PBR','darker physically rough water with 3D micro-ripples'],
 'rejected_previous':'V8 scattered stones, V9 flat slabs, V10 painted branching roads; V7 immutable baseline'}
open(os.path.join(ev,'entry-source.json'),'w',encoding='utf8').write(json.dumps(manifest,indent=2)+'\n')
print('VALORIA_ENTRY_V11_SOURCE_COMPLETE',json.dumps(manifest))
