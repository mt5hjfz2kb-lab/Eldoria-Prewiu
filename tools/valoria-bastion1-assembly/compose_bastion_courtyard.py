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
 # Bake portable, repeatable terrain-like color breakup into an actual GLB
 # texture, not a uniform overlay. World UV scale is shared across all patches.
 m=bpy.data.materials.new(name);m.diffuse_color=(*rgb,1);m.use_nodes=True
 nodes=m.node_tree.nodes;bs=nodes.get('Principled BSDF')
 bs.inputs['Base Color'].default_value=(1,1,1,1)
 bs.inputs['Roughness'].default_value=.98
 img=bpy.data.images.new(name+' baked soil microvariation',width=128,height=128)
 rng=random.Random(7725+len(name));pixels=[]
 for yy in range(128):
  for xx in range(128):
   wave=(math.sin(xx*.33+math.sin(yy*.13))*math.cos(yy*.28))*0.09
   grit=rng.uniform(-.13,.13)
   n=wave+grit
   green=math.sin(xx*.091+yy*.14)*math.cos(yy*.12)
   mix=max(0,min(1,.19+green*.12))
   pixels.extend((max(.05,min(.95,rgb[0]*(1+n)*(1-mix)+.28*mix)),
                  max(.05,min(.95,rgb[1]*(1+n)*(1-mix)+.32*mix)),
                  max(.05,min(.95,rgb[2]*(1+n)*(1-mix)+.17*mix)),1))
 img.pixels=pixels;img.pack()
 tex=nodes.new('ShaderNodeTexImage');tex.image=img;tex.interpolation='Linear'
 m.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
 return m
def world_uv(mesh):
 uv=mesh.uv_layers.new(name='UVMap')
 for loop in mesh.loops:
  p=mesh.vertices[loop.vertex_index].co
  uv.data[loop.index].uv=(p.x*.115,p.y*.115)
wear=material('Bastion I mottled muted earthen secondary foot traffic',(.355,.355,.24))
edge=material('Bastion I moss olive grass transition',(.29,.33,.19))
stone=material('Bastion I reused weathered grey limestone scree',(.29,.28,.255))
shade=material('Bastion I worn green-brown loam',(.30,.315,.205))
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
 mesh.update();world_uv(mesh);ob=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(ob)
 return len(faces)
ribbon('Bastion I west secondary worn-earth approach',[(-10,-13),(-18,-5),(-12,6)],1.45,wear)
ribbon('Bastion I east secondary worn-earth approach',[(10,-11),(17,-4),(11,6)],1.4,wear)
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
 mesh.update();world_uv(mesh);ob=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(ob)
 return ob
# Small, irregular material transitions rather than six conspicuous flat plates.
patches=[
 (-13,-10,1.1,.75,edge),(-17,-8,.8,1.1,shade),(-15,-3,1.0,.8,edge),
 (-19,1,.8,1.0,edge),(-13,4,1.15,.8,shade),(-17,8,.8,.9,edge),
 (-10,1,.8,.65,edge),
 (13,-10,1.2,.85,edge),(17,-7,.8,1.0,shade),(15,-3,1.1,.7,edge),
 (19,1,.85,.8,edge),(13,4,1.2,.9,shade),(17,8,.85,.8,edge),
 (10,1,.8,.65,edge)]
for i,(x,y,rx,ry,m) in enumerate(patches):
 island('Bastion I small mottled earth breakup '+str(i),x,y,rx,ry,m,610+i)
# Modest reused scree geometry at the lateral wall contacts only. Max 20 cm
# height: no new nodes, no obstacle in walking area and mobile-friendly.
rng=random.Random(9826);count=0
for side in (-1,1):
 for i in range(10):
  x=side*(19.1+rng.uniform(0,2.5))
  y=-10.5+i*1.85+rng.uniform(-.8,.8)
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
if count<16:raise RuntimeError('Insufficient wall contact detail')
dst.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',export_apply=False)
proof={'source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),
 'output_sha256':hashlib.sha256(dst.read_bytes()).hexdigest(),
 'intervention':'courtyard-perimeter-secondary-paths-and-contact-scree',
 'secondary_paths':2,'ground_islands':14,'contact_scree':count,
 'protected_axial_corridor_m':7,'existing_mesh_coordinates_modified':False,
 'new_resource_nodes':0,'new_buildings':0,'art_approved':False}
dst.with_suffix('.proof.json').write_text(json.dumps(proof,indent=2))
print('BASTION1_COURTYARD_COMPOSITION',json.dumps(proof))
