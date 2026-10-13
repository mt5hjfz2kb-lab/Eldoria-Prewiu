"""Bastion I courtyard composition pass: authored Blender geometry on the verified scene.

No new resource nodes, buildings, colliders or changes to original mesh vertices.
Only low-profile terrain contact, secondary pedestrian wear and sparse reused stone.
Usage: blender -b --python compose_bastion_courtyard.py -- baseline.glb candidate.glb
"""
import bpy, sys, math, random, pathlib, json, hashlib
from mathutils import Vector
from mathutils.bvhtree import BVHTree

argv=sys.argv[sys.argv.index('--')+1:]
if len(argv)!=2:raise SystemExit('Expected baseline.glb candidate.glb')
src,dst=map(pathlib.Path,argv)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(src))
meshes=[o for o in bpy.data.objects if o.type=='MESH']
terrain=[o for o in meshes if 'natural playable hilltop' in o.name.lower()]
if len(terrain)!=1:raise RuntimeError('Cannot identify original editable courtyard terrain')
ground=terrain[0]
bvh=BVHTree.FromObject(ground,bpy.context.evaluated_depsgraph_get())
def surface(x,y):
 # The original terrain remains unchanged, including the central axial road.
 hit,normal,index,distance=bvh.ray_cast(Vector((x,y,120)),Vector((0,0,-1)),250)
 if hit is None:raise RuntimeError('Terrain contact missing at '+str((x,y)))
 return hit.z
def material(name,rgb):
 m=bpy.data.materials.new(name);m.diffuse_color=(*rgb,1);m.use_nodes=True
 n=m.node_tree.nodes.get('Principled BSDF');n.inputs['Base Color'].default_value=(*rgb,1)
 n.inputs['Roughness'].default_value=.94
 return m
wear=material('Bastion I compacted ochre earth secondary foot traffic',(.43,.345,.225))
edge=material('Bastion I lichen olive grass transition',(.235,.30,.145))
stone=material('Bastion I reused grey limestone scree',(.39,.37,.315))
shade=material('Bastion I compacted brown loam',(.295,.255,.19))
# Two asymmetrical pedestrian arcs, set outside the protected axial 7m road.
# Secondary paths are strictly surface overlays (no gameplay terrain changes).
def ribbon(name,control,width,mat):
 vertices=[];faces=[];segments=56
 for i in range(segments+1):
  t=i/segments
  p0,p1,p2=control
  x=(1-t)**2*p0[0]+2*(1-t)*t*p1[0]+t*t*p2[0]
  y=(1-t)**2*p0[1]+2*(1-t)*t*p1[1]+t*t*p2[1]
  tx=2*((1-t)*(p1[0]-p0[0])+t*(p2[0]-p1[0]))
  ty=2*((1-t)*(p1[1]-p0[1])+t*(p2[1]-p1[1]))
  d=max(.001,math.hypot(tx,ty));nx=-ty/d;ny=tx/d
  taper=.30+.70*min(1,t*7,(1-t)*7)
  for s in (-1,1):
   xx=x+s*nx*width*taper*.5;yy=y+s*ny*width*taper*.5
   if abs(xx)<7:raise RuntimeError('Protected central route touched')
   vertices.append((xx,yy,surface(xx,yy)+.085))
  if i<segments:faces.append((2*i,2*i+1,2*i+3,2*i+2))
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.materials.append(mat)
 mesh.update();ob=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(ob)
 return len(faces)
ribbon('Bastion I west secondary worn-earth approach',[(-10,-13),(-18,-5),(-12,6)],2.4,wear)
ribbon('Bastion I east secondary worn-earth approach',[(10,-11),(17,-4),(11,6)],2.2,wear)
# The central yard stays open. Irregular worn-earth and grass islands are
# concentrated at the lateral perimeter, not at build plots or road.
def island(name,cx,cy,rx,ry,mat,seed):
 rng=random.Random(seed);v=[];count=18
 for i in range(count):
  theta=2*math.pi*i/count
  rr=.82+rng.uniform(-.13,.15)
  x=cx+rx*rr*math.cos(theta);y=cy+ry*rr*math.sin(theta)
  if abs(x)<7:raise RuntimeError('Island infringes protected axial corridor')
  v.append((x,y,surface(x,y)+.065))
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],[tuple(range(count))]);mesh.materials.append(mat)
 mesh.update();ob=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(ob)
 return ob
for i,(x,y,rx,ry,m) in enumerate([
 (-14,-9,3.2,2.1,edge),(-17,0,3.0,2.8,shade),(-13,7,2.8,2.2,edge),
 (14,-8,3.5,2.3,edge),(17,-1,2.6,2.2,shade),(14,7,3.0,2.0,edge)]):
 island('Bastion I lateral ground breakup '+str(i),x,y,rx,ry,m,610+i)
# Modest reused scree geometry at the lateral wall contacts only. Max 20 cm
# height: no new nodes, no obstacle in walking area and mobile-friendly.
rng=random.Random(9826);count=0
for side in (-1,1):
 for i in range(19):
  x=side*(19.1+rng.uniform(0,2.5))
  y=-11.5+i*.98+rng.uniform(-.3,.3)
  if y>8:continue
  z=surface(x,y)
  r=.18+rng.uniform(0,.33);h=.07+rng.uniform(0,.12)
  verts=[(x+math.cos(j*math.pi/4)*r,y+math.sin(j*math.pi/4)*r,z+.025) for j in range(8)]
  verts.append((x,y,z+h))
  faces=[(j,(j+1)%8,8) for j in range(8)]
  mesh=bpy.data.meshes.new('Reused contact limestone');mesh.from_pydata(verts,[],faces)
  mesh.materials.append(stone);mesh.update()
  obj=bpy.data.objects.new('Bastion I contact scree %02d'%count,mesh)
  bpy.context.collection.objects.link(obj);count+=1
if count<20:raise RuntimeError('Insufficient wall contact detail')
dst.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',export_apply=False)
proof={'source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),
 'output_sha256':hashlib.sha256(dst.read_bytes()).hexdigest(),
 'intervention':'courtyard-perimeter-secondary-paths-and-contact-scree',
 'secondary_paths':2,'ground_islands':6,'contact_scree':count,
 'protected_axial_corridor_m':7,'existing_mesh_coordinates_modified':False,
 'new_resource_nodes':0,'new_buildings':0,'art_approved':False}
dst.with_suffix('.proof.json').write_text(json.dumps(proof,indent=2))
print('BASTION1_COURTYARD_COMPOSITION',json.dumps(proof))
