"""Valoria V8: real source-conserving Blender surface pass from the frozen V7 BLEND.
Only the lateral road/apron surface geometry and existing river material/mesh are
edited. Protected gate, bridge, terrain topology and source objects remain untouched.
Unity colliders and gameplay routes stay in their original scene.
"""
import bpy, math, os, json, hashlib, struct
from mathutils import Vector
from mathutils.bvhtree import BVHTree
root=os.getcwd()
old=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7')
out=os.path.join(root,'docs/evidence/valoria-layered-construction-20261016/entry-zone-v8')
os.makedirs(out,exist_ok=True)
sha=lambda p:hashlib.sha256(open(p,'rb').read()).hexdigest()
source=os.path.join(old,'ValoriaEntryZoneV7.blend')
assert sha(source)=='24e27862ae759e30c36b151cef4fd9baacb45d1103d25f132bbe054d65896381','Unrecognized V7 input'
bpy.ops.wm.open_mainfile(filepath=source)
terrain=next(o for o in bpy.data.objects if o.type=='MESH' and o.name.startswith('ValoriaEntryTerrainV7'))
water=next(o for o in bpy.data.objects if o.type=='MESH' and 'RIVER_CONTINUOUS_RIPPLED_WATER' in o.name)
foot=next(o for o in bpy.data.objects if o.type=='MESH' and 'ARCH_FOOTINGS' in o.name)
roadmat=next(m for m in bpy.data.materials if m.name.startswith('ENTRY_road_masonry_PBR'))
shoremat=next(m for m in bpy.data.materials if m.name.startswith('ENTRY_existing_shore_PBR'))
bpy.context.view_layer.update()
bvh=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
# Local Blender x,y = -Unity native x,z. V7 already applies one x mirror.
def earth(native_x,native_z):
 x,y=-native_x,-native_z
 hit,norm,idx,d=bvh.ray_cast(Vector((x,y,12)),Vector((0,0,-1)),25)
 assert hit is not None, ('Terrain contact missing',native_x,native_z)
 return float(hit.z)
# Assess actual lower bridge footing at its base, before any changes.
foot_samples=[]
for v in foot.data.vertices:
 p=foot.matrix_world@v.co
 if p.z < -3.9:
  native_x,native_z=-p.x,-p.y
  try:
   ground=earth(native_x,native_z)
   foot_samples.append(dict(native_x=round(native_x,3),native_z=round(native_z,3),
     footing_y=round(p.z,3),terrain_y=round(ground,3),gap_above_terrain=round(p.z-ground,3)))
  except AssertionError:pass
# Real contact measurement is reported, never substituted with cosmetic water.
# A broad irregular dirt scar replaces each old clean-edged beige route renderer.
def make_dirt_material():
 m=bpy.data.materials.new('ENTRY_V8_earth_organic_route_CC0_derived')
 m.use_nodes=True
 p=m.node_tree.nodes.get('Principled BSDF')
 p.inputs['Base Color'].default_value=(.25,.21,.155,1)
 p.inputs['Roughness'].default_value=.98
 return m
dirt=make_dirt_material()
# Use existing licensed PBR stone/shore materials, no new asset downloads.
materials=[dirt,roadmat,shoremat]
verts=[];faces=[];mats=[]
def face(ids,mat=0):
 faces.append(tuple(ids));mats.append(mat)
def point(native_x,native_z,y):
 verts.append((-native_x,-native_z,y))
 return len(verts)-1
tracks=[]
for sign in (-1,1):
 # East and west parcel routes, linked to terrain rather than flat rectangles.
 xmin,xmax=7.0,15.25
 row=[]
 for i in range(35):
  x=xmin+(xmax-xmin)*i/34
  center_z=(-1.1 if sign>0 else -1.25)+.13*math.sin(i*.30+sign)
  row.append((x,center_z))
 tracks.append((sign,row,1.52))
# Lower entry apron at the exact native source station, with a softened edge.
tracks.append((0,[(0.,-9.42+i*.092) for i in range(25)],3.1))
for sign,rows,width in tracks:
 previous=None
 for index,(along,cz) in enumerate(rows):
  cells=[]
  for k in range(9):
   lateral=(k-4)/4
   taper=(1-.15*abs(lateral)) if sign!=0 else 1.
   wobble=.18*math.sin(index*.73+k*.98+sign*2.1)*abs(lateral)
   d=width*lateral*taper+wobble
   nx=sign*along if sign else d
   nz=cz+d if sign else cz
   gy=earth(nx,nz)
   # At edges the mesh sinks into grass, avoiding a new painted-looking slab.
   y=gy+.018*(1-abs(lateral))-.012*abs(lateral)
   cells.append(point(nx,nz,y))
  if previous is not None:
   for k in range(8):
    face((previous[k],previous[k+1],cells[k+1],cells[k]),0)
  previous=cells
# Individual irregular cobblestones touch the real surface; one unified mesh.
# No colliders are moved or manufactured, and the protected gate is untouched.
stone_count=0
for sign,rows,width in tracks:
 for step,(along,cz) in enumerate(rows[::2]):
  for lane in range(-3,4):
   h=((step*71+lane*41+sign*19)&255)/255
   if h<.13:continue
   skew=.08*math.sin(step*2.14+lane*.91)
   side=lane*width/4.1+skew
   nx=sign*along if sign else side
   nz=cz+side if sign else cz
   if sign==0 and abs(nx)>2.7:continue
   ground=earth(nx,nz)
   # Footprint: tiny nonrectangular stone with bevelled-looking sloped sides.
   n=7;radius_x=.22+.045*math.sin(step*.7+lane);radius_z=.19+.025*math.cos(step*.41+lane)
   if sign==0:radius_x=.22;radius_z=.045
   bot=[];top=[]
   for j in range(n):
    angle=2*math.pi*j/n
    rx=radius_x*(.9+.13*math.sin(j*2.19+step))
    rz=radius_z*(.9+.11*math.cos(j*1.61+lane))
    dx=rx*math.cos(angle);dz=rz*math.sin(angle)
    bot.append(point(nx+dx,nz+dz,ground+.019))
    top.append(point(nx+dx*.88,nz+dz*.88,ground+.048+.015*h))
   face(tuple(top),1 if (step+lane)%5 else 2)
   for j in range(n):face((bot[j],bot[(j+1)%n],top[(j+1)%n],top[j]),1 if (step+lane)%5 else 2)
   stone_count+=1
mesh=bpy.data.meshes.new('Valoria_V8_OrganicRoadAndApron_TerrainContact')
mesh.from_pydata(verts,[],faces);mesh.update()
for m in materials:mesh.materials.append(m)
for poly,i in zip(mesh.polygons,mats):
 poly.material_index=i;poly.use_smooth=False
road=bpy.data.objects.new('ENTRY_V8_ORGANIC_PARCEL_PATHS_APRON',mesh)
bpy.context.scene.collection.objects.link(road)
# Basic world-space UV on stones; reuse the true existing PBR stone map.
uv=mesh.uv_layers.new(name='MetreRoadUV')
for poly in mesh.polygons:
 for li in poly.loop_indices:
  v=mesh.vertices[mesh.loops[li].vertex_index].co
  uv.data[li].uv=(v.x*.17,v.y*.17)
# Existing river surface: break uniform cyan through geometry and a physically
# rough, muted water material; do not cover bridge contact or float new slabs.
wat=water.data.copy();water.data=wat
for v in water.data.vertices:
 x,y=v.co.x,v.co.y
 v.co.z+=.020*math.sin(x*.87+y*.58)+.013*math.sin(x*1.91-y*.83)
wat.update()
watermat=bpy.data.materials.new('ENTRY_V8_ColdMutedRiver')
watermat.use_nodes=True
pr=watermat.node_tree.nodes.get('Principled BSDF')
pr.inputs['Base Color'].default_value=(.066,.145,.152,1)
pr.inputs['Metallic'].default_value=.13
pr.inputs['Roughness'].default_value=.47
water.data.materials.clear();water.data.materials.append(watermat)
# Export all genuine source scene meshes as a full replacement, not additive
# duplicates. Unity V8 scene disables old V7 artwork before importing this GLB.
for o in bpy.data.objects:o.select_set(o.type=='MESH' and not o.hide_render)
bpy.context.view_layer.objects.active=terrain
blend=os.path.join(out,'ValoriaEntryZoneV8.blend')
glb=os.path.join(out,'ValoriaEntryZoneV8.glb')
bpy.ops.wm.save_as_mainfile(filepath=blend,compress=True)
bpy.ops.export_scene.gltf(filepath=glb,export_format='GLB',use_selection=True,export_apply=True)
raw=open(glb,'rb').read()
gltf=json.loads(raw[20:20+struct.unpack_from('<I',raw,12)[0]])
triangles=sum(gltf['accessors'][p['indices']]['count']//3 for node in gltf['nodes'] if 'mesh' in node for p in gltf['meshes'][node['mesh']]['primitives'])
manifest={'author':'GPT handoff continuation','stage':'V8 isolated visual candidate','source_v7_blend_sha256':sha(source),
 'result_blend_sha256':sha(blend),'result_glb_sha256':sha(glb),'triangles':triangles,
 'source_triangles':342006,'new_organic_route_stones':stone_count,'new_route_faces':len(faces),
 'footing_terrain_samples_count':len(foot_samples),'footing_terrain_samples':foot_samples[:80],
 'source_bridge_transforms_unchanged':True,'original_unity_colliders_unchanged':True,
 'materials_origin':'existing V7 CC0 PBR stone/shore plus procedural original diffuse earth and muted river',
 'operations':['organic native-grade east and west parcel tracks','graded apron','genuine river material and micro-displacement'],
 'art_status':'REQUIRES_INDEPENDENT_GENUINE_UNITY_AB','full_frame_art_pass':False}
open(os.path.join(out,'entry-source.json'),'w',encoding='utf8').write(json.dumps(manifest,indent=2)+'\n')
print('VALORIA_ENTRY_V8_SOURCE_COMPLETE',json.dumps({k:v for k,v in manifest.items() if k!='footing_terrain_samples'}))
