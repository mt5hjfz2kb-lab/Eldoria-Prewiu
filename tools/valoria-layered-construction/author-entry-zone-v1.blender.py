"""GPT-directed, source-derived entry-zone authoring. No original file is modified.
Reprofile the existing V3 mesh to measured spatial V1; recover the authored gate family.
This is a candidate, never automatic art approval. Native Unity XYZ <-> Blender X,-Z,Y.
"""
import bpy,bmesh,sys,os,json,math,hashlib
from mathutils import Vector,Matrix
root=os.getcwd();out=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016/entry-zone-v1')
os.makedirs(out,exist_ok=True)
sha=lambda p:hashlib.sha256(open(p,'rb').read()).hexdigest()
source=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016/terrain-v3/ValoriaTerrainRiverCliffV3.blend')
gatefile=os.path.join(root,'art-source/valoria/production/lower-gate-family-v1/LowerGateFamilyV1.blend')
assert sha(source)=='768a08535f066a40c707fd52901a0c16efad91ec06ce91f35ca31cf625b5792f'
report=json.load(open('docs/evidence/valoria-layered-construction-20261016/spatial-v1/spatial-native.json'))
contacts={c['id']:c['native_xyz'] for c in report['contacts']}
sx,sy,sz=contacts['CentralStair'];stair_delta=sz-2.75
bpy.ops.wm.open_mainfile(filepath=source)
chosen=[o for o in bpy.data.objects if o.type=='MESH' and not o.hide_render]
assert len(chosen)==9,[(o.name,o.hide_render) for o in bpy.data.objects if o.type=='MESH']
for o in list(bpy.data.objects):
 if o not in chosen:bpy.data.objects.remove(o,do_unlink=True)
terrain=next(o for o in chosen if 'LOWER_REAL_TERRAIN_NATIVE_HEIGHTFIELD' in o.name)
terrain.name='ValoriaEntryTerrainV1'
water=next(o for o in chosen if 'RIVER_CONTINUOUS_RIPPLED_WATER' in o.name)
# Primary terrain reflow uses real observed positions, not a repeated decorative noise facade.
def smooth(a,b,v):
 t=max(0.,min(1.,(v-a)/(b-a)));return t*t*(3-2*t)
def map_z(z):
 keys=[(-40.,-45.),(-19.65,-19.65),(-9.35,-9.35),(3.01,sz+.26),(9.,contacts['Bastion'][2]),(40.,100.)]
 for (a,b),(c,d) in zip(keys,keys[1:]):
  if z<=c:return b+(z-a)*(d-b)/(c-a)
 return keys[-1][1]+z-keys[-1][0]
def width(z):return 29.+5.*math.sin(max(0,min(1,(z+8)/75))*math.pi)+1.2*math.sin(z*.115)
def ground(x,z):
 near=-9.7+1.1*math.sin(x*.10)+.45*math.sin(x*.29+.7)
 far=-20.1+.65*math.sin(x*.12+.8)
 riverbed=-4.5+.12*math.sin(x*.15+z*.09)
 north=smooth(near-1.8,near+.3,z)
 south=1-smooth(far-.35,far+1.65,z)
 relief=.28+.07*math.sin(x*.13+z*.055)*math.sin(x*.069-z*.10)
 upper=2.42*smooth(sz-1.8,sz-.8,z)
 # The real 12-step throat stays open; upper plateau starts behind last tread.
 throat=1-smooth(1.85,2.65,abs(x-sx))
 upper*=1-throat*(1-smooth(sz+3.0,sz+3.35,z))
 perimeter=1-smooth(width(z),width(z)+3.0,abs(x))
 north*=perimeter
 h=riverbed+north*(relief+upper-riverbed)+south*(relief-riverbed)
 # Exact measured bridge shore grades, flared actual banks. No collider offsets.
 for end in (-19.65,-9.35):
  band=(1-smooth(1.8,4.8,abs(x)))*(1-smooth(.15,1.25,abs(z-end)))
  h=h*(1-band)+.38*band
 # Main road contact: terrain lies just below the real road; no rock across passage.
 if -8.7<z<sz-2.6 and abs(x-sx*(z+8.7)/(sz-2.6+8.7))<2.1:h=min(h,.28)
 return h
# Add topology only around the two river banks, where the original coarse field
# cannot describe a 4m cliff. Keep the distant grid inexpensive and editable.
bm=bmesh.new();bm.from_mesh(terrain.data)
edges=[e for e in bm.edges if any(-24 < -v.co.y < -6 for v in e.verts)]
bmesh.ops.subdivide_edges(bm,edges=edges,cuts=2,use_grid_fill=True)
bm.to_mesh(terrain.data);bm.free()
for v in terrain.data.vertices:
 x=v.co.x*1.4;z=map_z(-v.co.y);v.co=Vector((x,-z,ground(x,z)))
terrain.data.update()
for o in chosen:
 if o==terrain:continue
 if o==water:
  for v in o.data.vertices:v.co.x*=2.6;v.co.z=-3.7+.025*math.sin(v.co.x*.8+v.co.y*.5)
 elif 'IRREGULAR_RETERRACED_CLIFF' in o.name:
  for v in o.data.vertices:
   x=v.co.x;z=-v.co.y
   v.co.x=sx+math.copysign(2.1+max(0,abs(x)-2.1)*2.2,x)
   v.co.y=-(z+stair_delta)
 elif 'REAL_BRIDGE_BANK_STONE' in o.name:
  pass # Frozen measured shore rocks remain at the exact bridge contacts.
 else:
  for v in o.data.vertices:
   x=v.co.x*1.4;z=map_z(-v.co.y);v.co.x=x;v.co.y=-z
   v.co.z=ground(x,z)+max(-.25,min(.45,v.co.z))
 o.data.update()
# Continuous world-scale UVs and existing PBR maps, never a color-only final claim.
surface='art-source/valoria/lookdev/golden-slice-v1/surface-v2'
def pbr(label,family,rough):
 m=bpy.data.materials.new(label);m.use_nodes=True;n=m.node_tree.nodes;l=m.node_tree.links;p=next((x for x in n if x.type=='BSDF_PRINCIPLED'),None) or n.new('ShaderNodeBsdfPrincipled');output=next((x for x in n if x.type=='OUTPUT_MATERIAL'),None) or n.new('ShaderNodeOutputMaterial');l.new(p.outputs['BSDF'],output.inputs['Surface']);p.inputs['Roughness'].default_value=rough
 for kind,socket in [('albedo','Base Color'),('normal','Normal')]:
  path=os.path.abspath(f'{surface}/{family}_{kind}.png');tex=n.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(path,check_existing=True)
  if kind=='normal':
   tex.image.colorspace_settings.name='Non-Color';normal=n.new('ShaderNodeNormalMap');normal.inputs['Strength'].default_value=.55;l.new(tex.outputs['Color'],normal.inputs['Color']);l.new(normal.outputs['Normal'],p.inputs[socket])
  else:l.new(tex.outputs['Color'],p.inputs[socket])
 return m
meadow=pbr('ENTRY_existing_ground_PBR','ground',.93);rock=pbr('ENTRY_existing_rock_PBR','rock',.94);shore=pbr('ENTRY_existing_shore_PBR','shore',.94)
terrain.data.materials.clear()
for m in (meadow,rock,shore):terrain.data.materials.append(m)
for p in terrain.data.polygons:
 p.material_index=1 if p.normal.z<.78 else 2 if p.center.z < -.1 else 0
 p.use_smooth=p.material_index==0
uv=terrain.data.uv_layers.active or terrain.data.uv_layers.new(name='Native_Metre_UV')
for p in terrain.data.polygons:
 for li in p.loop_indices:
  v=terrain.data.vertices[terrain.data.loops[li].vertex_index].co
  uv.data[li].uv=(v.x*.16,(v.z if p.normal.z<.78 else v.y)*.16)
for o in chosen:
 if 'IRREGULAR_RETERRACED_CLIFF' in o.name:
  o.data.materials.clear();o.data.materials.append(rock)
  for p in o.data.polygons:p.material_index=0
# Recover the actual authored asymmetric tall-tower gate, not another tower generator.
with bpy.data.libraries.load(gatefile,link=False) as (src,dst):dst.objects=[n for n in src.objects if n.startswith('LowerGate')]
gates=[o for o in dst.objects if o is not None and o.type=='MESH']
assert len(gates)>=6,[o.name for o in gates]
for o in gates:
 bpy.context.scene.collection.objects.link(o)
 o.hide_render=False;o.hide_set(False)
 # Bake source transforms once, then uniform scale .75: preserve mesh proportions.
 transform=o.matrix_world.copy()
 for v in o.data.vertices:
  w=transform@v.co;v.co=Vector((-(w.x-5.566)*.75,-(w.y+25.6)*.75+9.25,(w.z-7.1)*.75+.38))
 o.matrix_world=Matrix.Identity(4);o.name='ENTRY_'+o.name;o.data.update()
# Pack original texture bytes into this isolated, editable source.
for img in bpy.data.images:
 if img.source=='FILE' and img.packed_file is None:
  if not os.path.isfile(bpy.path.abspath(img.filepath)):
   alt=os.path.join(os.path.dirname(gatefile),os.path.basename(img.filepath))
   if os.path.isfile(alt):img.filepath=alt
  if os.path.isfile(bpy.path.abspath(img.filepath)):img.pack()
for o in bpy.data.objects:o.select_set(o.type=='MESH')
bpy.context.view_layer.objects.active=terrain
blend=os.path.join(out,'ValoriaEntryZoneV1.blend');glb=os.path.join(out,'ValoriaEntryZoneV1.glb')
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
manifest={'author':'GPT current-session art direction; Qwen auxiliary remains separately recorded','blender_version':bpy.app.version_string,
 'source_blend_sha256':sha(source),'source_gate_blend_sha256':sha(gatefile),'result_blend_sha256':sha(blend),'result_glb_sha256':sha(glb),
 'original_sources_unchanged':True,'canonical_modified':False,'terrain_target':terrain.name,'gate_uniform_scale':.75,
 'stair_contact_native':contacts['CentralStair'],'bridge_contact_y':{str(z):ground(0,z) for z in (-19.65,-9.35)},
 'mesh_objects':len(chosen)+len(gates),'triangles':sum(len(p.vertices)-2 for o in chosen+gates for p in o.data.polygons),
 'art_status':'CANDIDATE_REQUIRES_GENUINE_UNITY_AB','operations':['source-topology shore subdivision','direct source vertex reprofile','world-scale UV','existing PBR texture maps','reuse original Blender gate with uniform transform']}
open(os.path.join(out,'entry-source.json'),'w').write(json.dumps(manifest,indent=2)+'\n')
print('VALORIA_ENTRY_SOURCE_COMPLETE',json.dumps(manifest))
