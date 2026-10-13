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
# Previous surface ribbons and islands were ART REJECTED by authentic Unity A/B:
# their color remained dark and geometry read as pasted-on decals. Remove them
# completely. Improve the existing terrain albedo IN PLACE, without new planes.
grass_mats=[m for m in ground.data.materials if m and 'terrace earth olive grass' in m.name.lower()]
if len(grass_mats)!=1:raise RuntimeError('Original terrain grass material not uniquely found')
grass_mat=grass_mats[0]
tex_nodes=[n for n in grass_mat.node_tree.nodes if n.type=='TEX_IMAGE' and n.image]
if not tex_nodes:raise RuntimeError('Original grass material has no editable albedo image')
original=tex_nodes[0].image
# Duplicate image so the canonical source texture remains untouched in memory.
newimg=original.copy();newimg.name='Bastion I varied native meadow albedo'
w,h=newimg.size
source=list(original.pixels);updated=[]
for yy in range(h):
 for xx in range(w):
  k=(yy*w+xx)*4
  # Irregular broad grass/dirt tone variations, with a low-amplitude grain.
  n=(math.sin(xx*.071+math.sin(yy*.032)*2.1)*math.cos(yy*.057)
    +.48*math.sin(xx*.15-yy*.09))*0.5
  grit=math.sin(xx*.73+yy*.51)*math.sin(yy*.64-xx*.41)
  delta=n*.18+grit*.025
  # More olive earth, less flat saturated green; preserve alpha and texture.
  r=max(.03,min(.9,source[k]*(1+delta)+max(0,n)*.035))
  g=max(.03,min(.9,source[k+1]*(1+delta*.8)))
  b=max(.03,min(.9,source[k+2]*(1+delta*.55)))
  updated.extend((r,g,b,source[k+3]))
newimg.pixels=updated;newimg.pack()
tex_nodes[0].image=newimg
print('BASTION1_EXISTING_GRASS_ALBEDO_REBALANCED',w,h)
# No overlays or new resource nodes. Reuse only modest contact stones.
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
 'intervention':'native-courtyard-ground-albedo-and-contact-scree',
 'secondary_paths':0,'ground_islands':0,'contact_scree':count,
 'protected_axial_corridor_m':7,'existing_mesh_coordinates_modified':False,
 'new_resource_nodes':0,'new_buildings':0,'art_approved':False}
dst.with_suffix('.proof.json').write_text(json.dumps(proof,indent=2))
print('BASTION1_COURTYARD_COMPOSITION',json.dumps(proof))
