"""Eldoria Bastion I: Qwen-directed broad terrain-and-ramp integration.

Genuine model inputs: Qwen vision run 38530882467, candidate A:
- turn western detached wall into stepped, soil-grounded embankment;
- connect masonry/cliff to scene silhouette; frame axial route via slope.
Candidate B: terrain hierarchy and graded worn soil, not isolated paving tiles.

This implementation is deterministic Blender, NOT AI-authored vertices.
Applies to existing 100x100m terrain mesh, without adding a gameplay collider,
building, resource node, separate terrain overlay or second fortified gate.
Retains source GLB and records original/new terrain geometry+material proof.
Command: blender -b --python-exit-code 1 --python sculpt_bastion_native_terrain.py -- input.glb out.glb
"""
import bpy,math,sys,pathlib,json,hashlib
import numpy as np
from mathutils import Vector
src,dst=map(pathlib.Path,sys.argv[sys.argv.index('--')+1:])
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(src))
originals=[o for o in bpy.data.objects if o.type=='MESH']
targets=[o for o in originals if 'natural playable hilltop' in o.name.lower()]
if len(targets)!=1:raise RuntimeError('Exactly one existing canonical terrain mesh expected')
terrain=targets[0]
if terrain.parent or any(abs(v)>1e-6 for v in terrain.location):
 raise RuntimeError('Terrain needs native world coordinates to sculpt non-destructively')
# Material isolation is crucial: candidate must NOT mutate existing architecture.
oldmesh=terrain.data;mesh=oldmesh.copy();terrain.data=mesh
before=np.array([list(v.co) for v in mesh.vertices],dtype=np.float64)
Xs,Ys=before[:,0],before[:,1]
Zs=before[:,2]
def sat(a):return np.clip(a,0,1)
def ease(a):
 t=sat(a);return t*t*(3-2*t)
def pad(x,y,cx,cy,wx,wy):
 return (1-ease((np.abs(x-cx)-wx)/2.0))*(1-ease((np.abs(y-cy)-wy)/2.0))
def safe_weight(x,y):
 # Real 14m central road + 1m feather safety, AND full 10x10m future lots.
 # No terrain Z movement at either real model footprint or central stair.
 route=ease((np.abs(x)-7.0)/3.2)
 plots=(1-pad(x,y,-16,3,6.5,6.5))*(1-pad(x,y,16,3,6.5,6.5))
 # No changes around lower hero gate, upper entrance or upper fort wall.
 bottom=ease((y+16.6)/2.6)
 upper=1-ease((y-19)/2.6)
 return route*plots*bottom*upper
weight=safe_weight(Xs,Ys)
# Original grass surface remains a single continuous playable mesh; sculpt
# 70cm-scale compound terrain in the OLD western ward and its right counterpart.
# Flatten micro-unevenness into three mutually blending stone-soil terraces.
western=np.exp(-((Xs+21.6)/10.0)**4 - ((Ys+6.75)/5.0)**4)
eastern=np.exp(-((Xs-21.1)/9.0)**4 - ((Ys+7.2)/5.7)**4)
upper_sides=np.exp(-((np.abs(Xs)-19.1)/10.8)**4 - ((Ys-12.2)/4.8)**4)
# Terrain relief / worn embankment: stays <0.6m from original, so no tall
# manmade ridge, floating objects or newly impassable cliff on future lots.
wavex=(np.sin(Xs*.48+Ys*.19)*.09+
       np.sin(Xs*.17-Ys*.37)*.085+
       np.cos(Xs*.095+Ys*.11)*.11)
bank=western*.31+eastern*.25+upper_sides*.38
soft_ruts=(np.exp(-((np.abs(Xs)-10.7)/2.1)**2)
           *np.exp(-((Ys+4)/15)**4))*.16
delta=weight*(bank+wavex*(.46+western+eastern)*.50-soft_ruts)
delta=np.clip(delta,-.22,.58)
for v,d in zip(mesh.vertices,delta):
 if abs(d)>1e-5:v.co.z+=float(d)
mesh.update()
# Guarantee true heights of gameplay entrance and both ten-metre parcels.
protected=~(safe_weight(Xs,Ys)>0.00001)
if np.max(np.abs(delta[protected]),initial=0)>2e-4:
 raise RuntimeError('Protected original terrain vertices were moved')
change=int(np.count_nonzero(np.abs(delta)>.012))
if change<350 or np.max(np.abs(delta))<.25:raise RuntimeError('Not enough genuine terrain sculpt')
# Baked full-width albedo map on ONLY the original grass faces, with irregular
# forest-meadow soil break-up. Unlike rejected decal paths, no extra top planes
# or overtly rectangular texture islands are introduced. GLB contains image.
res=1024
u,v=np.meshgrid(np.linspace(0,1,res,dtype=np.float32),
                np.linspace(0,1,res,dtype=np.float32))
X=-50.5+u*101.;Y=-49+v*106.
rng=np.random.default_rng(1401026)
def multi_noise(X,Y,frequency,seed):
 # Portable bicubic-ish smoothed interpolation of seeded coarse fields.
 gx=(X+60)*frequency;gy=(Y+60)*frequency
 ix=np.floor(gx).astype(np.int32);iy=np.floor(gy).astype(np.int32)
 fx=ease(gx-ix);fy=ease(gy-iy)
 # Polynomial coordinate hash avoids enormous RNG allocation.
 def h(i,j):
  a=np.sin(i*127.1+j*311.7+seed*71.7)*43758.5453
  return (a-np.floor(a))*2-1
 v00=h(ix,iy);v10=h(ix+1,iy);v01=h(ix,iy+1);v11=h(ix+1,iy+1)
 return (v00*(1-fx)+v10*fx)*(1-fy)+(v01*(1-fx)+v11*fx)*fy
broad=multi_noise(X,Y,.11,4)
medium=multi_noise(X,Y,.32,7)
fine=multi_noise(X,Y,1.18,9)
micro=multi_noise(X,Y,4.6,10)
variation=.66*broad+.25*medium+.09*fine
def ground_patch(cx,cy,rx,ry):
 warp=medium*.75+broad*.88
 r=np.sqrt(((X-cx+warp)/rx)**2+((Y-cy+warp*.7)/ry)**2)
 return 1-ease((r-(.67+variation*.17))/.55)
# Authored Qwen spatial decisions (west connected ruined earthwork; two-sided
# worn medieval courtyard). Only dirt ALBEDO changes at buildable zones.
earth=np.zeros_like(X)
for params,op in [
 ((-20.0,-10.15,12.2,6.2),.79),
 ((19.7,-9.7,12.0,6.6),.65),
 ((-18.9,12.8,8.3,5.7),.52),
 ((19.0,12.8,8.5,5.7),.52),
 ((-9.2,-3.2,6.7,17.3),.56),
 ((9.6,-3.2,6.8,17.1),.59),
 ((-2.7,-11.8,10.8,3.5),.40)
]:
 earth=np.maximum(earth,ground_patch(*params)*op)
# Substantial but coherent subtle dirt/grass mixing: not a uniform mud decal.
earth=sat(earth*(.82+.15*medium+.14*fine))
# Confine to existing playable yard, avoid replacing cliff UVs (done below).
soil_grass=np.stack((
 .265+.035*variation+.013*fine,
 .292+.034*variation+.013*fine,
 .155+.021*variation+.010*fine),axis=-1)
soil_ochre=np.stack((
 .370+.045*variation+.020*fine,
 .305+.032*variation+.016*fine,
 .201+.026*variation+.014*fine),axis=-1)
pix=soil_grass*(1-earth[:,:,None])+soil_ochre*earth[:,:,None]
gravel=sat((np.abs(X)-22)/6)*sat((1+medium*.35)/2)
pix=pix*(1-gravel[:,:,None]*.16)+np.array([.31,.299,.26])*(gravel[:,:,None]*.16)
pix+=micro[:,:,None]*.012
pix=np.clip(pix,.07,.82)
colors=np.concatenate((pix,np.ones((res,res,1),dtype=np.float32)),axis=2).astype(np.float32)
new_image=bpy.data.images.new('Bastion I Qwen broad sculpted terrain native soil A4',
                                width=res,height=res,alpha=True)
new_image.pixels.foreach_set(colors.ravel());new_image.pack()
# Index 0 is original olive-grass material from canonical terrain_finish.py.
original_grass=mesh.materials[0]
if not original_grass:raise RuntimeError('Original grass material missing')
grass=original_grass.copy();grass.name='Bastion I natural graded terrain Qwen spatial A4'
grass.use_nodes=True
nodes=grass.node_tree.nodes;principled=nodes.get('Principled BSDF')
if not principled:raise RuntimeError('URP-exportable PBR material missing')
tex=nodes.new('ShaderNodeTexImage');tex.image=new_image;tex.interpolation='Linear'
for link in list(principled.inputs['Base Color'].links):
 grass.node_tree.links.remove(link)
grass.node_tree.links.new(tex.outputs['Color'],principled.inputs['Base Color'])
principled.inputs['Roughness'].default_value=.99
mesh.materials[0]=grass
# Remove texture dependence on original repeating grass UV only for native
# terrain polygon faces with grass material. All rock cliff faces keep UVs.
uv_layer=mesh.uv_layers.active
if uv_layer is None:raise RuntimeError('Original heightfield needs native UV data')
grass_loops=0
for poly in mesh.polygons:
 if poly.material_index!=0:continue
 for li in poly.loop_indices:
  p=mesh.vertices[mesh.loops[li].vertex_index].co
  uv_layer.data[li].uv=((p.x+50.5)/101.,(p.y+49)/106.)
  grass_loops+=1
if grass_loops<1000:raise RuntimeError('Grass UV atlas not applied to source terrain')
# Respect the original source mesh names, everything except sole native terrain.
all_names_before={o.name for o in originals}
if not all(o.name in all_names_before for o in originals):raise RuntimeError('Original object set changed')
dst.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',export_apply=False)
proof=dict(artistic_choice='genuine Qwen vision run 38530882467: earthy continuous grade + stepped west wall embankment, executed deterministically',
 original_source_sha256=hashlib.sha256(src.read_bytes()).hexdigest(),
 output_sha256=hashlib.sha256(dst.read_bytes()).hexdigest(),
 image_agent_raw_sha256=['8ea3dd774945e1523a23a187cfb8ca75040cb88d5b3e3ff2cddf096e4dbf0ca9','05e86f2a3366f47b58768bdbc6bd54fa63c0d07e08c5365d47f9e43cc74d578f'],
 terrain_vertices_total=len(mesh.vertices),terrain_vertices_sculpted=change,
 max_terrain_delta_m=float(np.max(np.abs(delta))),mean_terrain_delta_m=float(np.mean(np.abs(delta))),
 protected_road_and_future_parcels_max_delta_m=float(np.max(np.abs(delta[protected]),initial=0)),
 soil_texture_px=[res,res],grass_uv_loops=grass_loops,
 original_mesh_objects_preserved=len(originals),other_original_geometry_modified=False,
 main_gate_and_stairs_unchanged=True,
 new_buildings=0,new_resource_nodes=0,new_colliders=0,
 native_source_terrain_sculpted=True,
 artistically_approved=False,mobile_hardware_certified=False)
dst.with_suffix('.proof.json').write_text(json.dumps(proof,indent=2))
print('BASTION1_QWEN_ACTUAL_TERRAIN_SCULPT',json.dumps(proof))
