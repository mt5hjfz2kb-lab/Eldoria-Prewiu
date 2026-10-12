"""Reference composition of immutable authored Valoria modules, not a new family.
Blender 4.0+: blender -b --python assemble.py -- --inputs DIR --output DIR
Terrain polygons are explicitly provisional supports, pending external artist gate.
"""
import argparse, hashlib, json, math, os, pathlib, random, sys
if os.environ.get('VALORIA_PYTHON_PACKAGES'):
 sys.path.append(os.environ['VALORIA_PYTHON_PACKAGES'])
if os.environ.get('VALORIA_PYTHON_DYNLOAD'):
 sys.path.append(os.environ['VALORIA_PYTHON_DYNLOAD'])
import bpy
from mathutils import Vector, Matrix
from mathutils.noise import fractal

args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
p=argparse.ArgumentParser();p.add_argument('--inputs',type=pathlib.Path,required=True);p.add_argument('--output',type=pathlib.Path,required=True);p.add_argument('--samples',type=int,default=24)
a=p.parse_args(args);a.output.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=a.samples
scene.cycles.use_denoising=os.environ.get('VALORIA_NO_DENOISE')!='1';scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX';scene.view_settings.look='AgX - Medium High Contrast'
scene.world=bpy.data.worlds.new('Blue grey valley ambience');scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.25,.34,1)
scene.world.node_tree.nodes['Background'].inputs['Strength'].default_value=.6
blueprints={};sources={};placements=[];materials={};images={}

def bounds(objs):
 pts=[o.matrix_world @ Vector(c) for o in objs for c in o.bound_box]
 return Vector(tuple(min(v[i] for v in pts) for i in range(3))),Vector(tuple(max(v[i] for v in pts) for i in range(3)))

def signature(m):
 if not m.use_nodes:return m.name
 n=m.node_tree.nodes.get('Principled BSDF')
 imagekeys=[]
 for node in m.node_tree.nodes:
  if node.type=='TEX_IMAGE' and node.image:
   im=node.image
   if im.packed_file:key=hashlib.sha256(im.packed_file.data).hexdigest()
   else:key=im.name.split('.')[0]+str(tuple(im.size))
   if key in images:node.image=images[key]
   else:
    if max(im.size)>1024:im.scale(int(im.size[0]*1024/max(im.size)),int(im.size[1]*1024/max(im.size)))
    images[key]=im
   imagekeys.append(key)
 vals=[tuple(n.inputs['Base Color'].default_value),float(n.inputs['Metallic'].default_value),float(n.inputs['Roughness'].default_value)] if n else []
 return json.dumps([vals,sorted(imagekeys)])

def load(name):
 path=a.inputs/(name+'.glb');before=set(bpy.data.objects)
 bpy.ops.import_scene.gltf(filepath=str(path))
 objs=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
 # Bake importer hierarchy to object coordinates before removing empties.
 for o in objs:
  mw=o.matrix_world.copy();o.parent=None;o.matrix_world=mw
  for i,m in enumerate(o.data.materials):
   if m:
    key=signature(m)
    if key in materials:o.data.materials[i]=materials[key]
    else:materials[key]=m
 lo,hi=bounds(objs);center=(lo+hi)/2;center.z=0
 for o in objs:o.location-=center
 # Collapse same-material parts in each immutable source copy. Gate goes from
 # 1,037 renderers to material groups; keep the untouched source SHA recorded.
 groups={}
 for o in objs:
  k=tuple(m.name if m else '' for m in o.data.materials);groups.setdefault(k,[]).append(o)
 joined=[]
 for i,group in enumerate(groups.values()):
  bpy.ops.object.select_all(action='DESELECT')
  for o in group:o.select_set(True)
  bpy.context.view_layer.objects.active=group[0]
  if len(group)>1:bpy.ops.object.join()
  ob=group[0];ob.name=name+'_material_group_'+str(i);joined.append(ob)
 lo,hi=bounds(joined)
 blueprints[name]=(joined,lo,hi)
 sources[name]={'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'bytes':path.stat().st_size,'mesh_groups':len(joined),'bounds':[[*lo],[*hi]]}
 for o in joined:
  for c in list(o.users_collection):c.objects.unlink(o)
 print('SOURCE_IMPORTED',name,len(joined),flush=True)

for n in ['hero_gate_v3','wall_straight','watchtower','wall_corner','wall_connections','stone_path_platform','cliff_rocks','defensive_props','bridge','stairs']:load(n)

def put(name,label,x,y,z=0,angle=0,scale=(1,1,1),anchor='foot',target_size=None):
 originals,lo,hi=blueprints[name];sz=hi-lo
 if target_size:scale=tuple(target_size[i]/sz[i] if target_size[i] is not None else scale[i] for i in range(3))
 root=bpy.data.objects.new(label,None);scene.collection.objects.link(root)
 root.location=(x,y,z);root.rotation_euler.z=math.radians(angle);root.scale=scale
 offset=lo.z if anchor=='foot' else hi.z if anchor=='crest' else 0
 for old in originals:
  o=old.copy();o.data=old.data;scene.collection.objects.link(o);o.parent=root
  o.matrix_parent_inverse=Matrix.Identity(4);o.matrix_basis=old.matrix_world.copy();o.location.z-=offset
 placements.append({'id':label,'asset':name,'position_blender':[x,y,z],'rotation_degrees':angle,'scale':[*scale],'anchor':anchor})
 return root

# Coordinate contract: Blender +Y goes inward; Unity +Z goes inward.
# Two elevations, broad playable negative space, an uninterrupted axial route.
put('hero_gate_v3','LowerGate',0,-17,0)
put('hero_gate_v3','UpperBastion',0,24,3.5,scale=(.88,.88,.88))
gatewidth=blueprints['hero_gate_v3'][2].x-blueprints['hero_gate_v3'][1].x
half=gatewidth/2
for side in [-1,1]:
 for i in range(2):put('wall_straight',f'UpperWall_{side}_{i}',side*(half*.88+4+i*7.85),24,3.5)
 put('watchtower',f'UpperFlankTower_{side}',side*(half*.88+15.8),24,3.5)
 put('wall_connections',f'LowerSocket_{side}',side*(half+.8),-17,0)
 put('wall_straight',f'LowerGateWall_{side}',side*(half+4.9),-17,0,scale=(1,.9,.68))
 put('wall_corner',f'OuterCorner_{side}',side*25,-10,0,angle=0 if side<0 else 90,scale=(1,1,.72))
 put('wall_straight',f'RemnantWall_{side}',side*23,-4,0,angle=90,scale=(.9,1,.72))
 put('defensive_props',f'GatePracticalSet_{side}',side*(half-1.25),-19,0,scale=(.65,.65,.65))
 put('defensive_props',f'UpperPracticalSet_{side}',side*4.5,21,3.5,scale=(.6,.6,.6))
put('bridge','StoneApproachBridge',0,-28,-3.5,target_size=(6.0,19.0,3.5))
put('stairs','TerraceStair',0,9,0,target_size=(6.0,7.0,3.5))
for y in [-12,-4,3]:put('stone_path_platform','CentralRoad_'+str(y),0,y,.02)
for y in [16,20]:put('stone_path_platform','UpperRoad_'+str(y),0,y,3.52)
# Geological authored modules carry the visible perimeter; caps are prototypes.
for x in [-23,-11,11,23]:put('cliff_rocks','FrontEscarpment_'+str(x),x,-18,0,anchor='crest')
for side in [-1,1]:
 for y in [-9,3,15]:put('cliff_rocks',f'SideEscarpment_{side}_{y}',side*29,y,3.5 if y>9 else 0,angle=90 if side<0 else -90,anchor='crest')
 for x in [10,22]:put('cliff_rocks',f'UpperTerraceRise_{side}_{x}',side*x,12,3.5,anchor='crest')

def mat(name,color,rough=1):
 m=bpy.data.materials.new(name);m.use_nodes=True;n=m.node_tree.nodes.get('Principled BSDF');n.inputs['Base Color'].default_value=(*color,1);n.inputs['Roughness'].default_value=rough;return m
grass=mat('PROTOTYPE terrace earth olive grass',(.20,.24,.085));water=mat('PROTOTYPE blue valley water',(.025,.15,.22),.28)
nt=grass.node_tree;bs=nt.nodes.get('Principled BSDF');noise=nt.nodes.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=2.1
ramp=nt.nodes.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].color=(.12,.16,.05,1);ramp.color_ramp.elements[1].color=(.34,.29,.12,1);nt.links.new(noise.outputs['Fac'],ramp.inputs['Fac']);nt.links.new(ramp.outputs['Color'],bs.inputs['Base Color'])
# Make portable albedo: avoid procedural shader loss in GLB export.
im=bpy.data.images.new('terrace-earth-portable',width=256,height=256)
pixels=[]
for v in range(256):
 for u in range(256):
  f=fractal(Vector((u*.045,v*.045,7.13)),.8,2.1,5)*.13
  pixels.extend((.26+f,.28+f,.11+f*.5,1))
im.pixels=pixels;im.pack();tex=nt.nodes.new('ShaderNodeTexImage');tex.image=im;nt.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
def polygon(name,verts,material):
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],[list(range(len(verts)))]);mesh.materials.append(material);mesh.update()
 uv=mesh.uv_layers.new(name='UVMap')
 for loop in mesh.loops:
  co=mesh.vertices[loop.vertex_index].co;uv.data[loop.index].uv=(co.x*.08,co.y*.08)
 o=bpy.data.objects.new(name,mesh);scene.collection.objects.link(o);return o
polygon('PROTOTYPE_MainTerrace',[(-29,-18,-.04),(29,-18,-.04),(30,10,-.04),(-29,10,-.04)],grass)
polygon('PROTOTYPE_UpperTerrace',[(-29,12,3.44),(29,12,3.44),(28,31,3.44),(-28,31,3.44)],grass)
polygon('PROTOTYPE_ValleyWater',[(-90,-65,-3.7),(90,-65,-3.7),(90,70,-3.7),(-90,70,-3.7)],water)

# Non-destructive shared variants, restricted to known architectural materials.
material_changes=[]
for m in list(materials.values()):
 n=m.node_tree.nodes.get('Principled BSDF') if m.use_nodes else None
 if not n:continue
 name=m.name.lower();c=n.inputs['Base Color'].default_value
 if any(w in name for w in ['stone','mortar','limestone','ashlar','masonry']):
  old=[*c];n.inputs['Base Color'].default_value=(c[0]*.76,c[1]*.72,c[2]*.67,c[3]);n.inputs['Roughness'].default_value=.88
  material_changes.append({'material':m.name,'old_factor':old,'new_factor':[*n.inputs['Base Color'].default_value]})

# Existing authored vegetation meshes; local preview palette only. No new tree art.
def foliage(name,color):
 path=a.inputs/(name+'.fbx')
 if not path.exists():return
 before=set(bpy.data.objects);bpy.ops.import_scene.fbx(filepath=str(path));obs=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
 for o in obs:mw=o.matrix_world.copy();o.parent=None;o.matrix_world=mw
 lo,hi=bounds(obs);h=max(.01,hi.z-lo.z)
 vegetation_materials={}
 def vegetation_material(old):
  key=old.name if old else 'fallback'
  if key in vegetation_materials:return vegetation_materials[key]
  isleaf='leaf' in key.lower()
  m=mat('Existing vegetation '+key,color if isleaf else (.16,.105,.058))
  file=a.inputs/('pine-leaf.png' if isleaf else 'pine-trunk.png')
  if name=='pine' and file.exists():
   tex=m.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(file),check_existing=True)
   if isleaf:
    # This package's leaf atlas is deliberately red: its Unity shader applies
    # a species color. Bake a portable green palette, preserving atlas alpha.
    src=list(tex.image.pixels);dst=[]
    for j in range(0,len(src),4):
     lum=max(src[j],src[j+1],src[j+2]);dst.extend((lum*.18,lum*.37,lum*.20,src[j+3]))
    tex.image.pixels=dst
   tex.image.pack()
   bs=m.node_tree.nodes.get('Principled BSDF');m.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
   if isleaf:
    m.node_tree.links.new(tex.outputs['Alpha'],bs.inputs['Alpha'])
    if hasattr(m,'blend_method'):m.blend_method='CLIP';m.alpha_threshold=.35
    else:
     # New Blender versions removed blend_method. Export thresholded alpha
     # instead of silently rendering the rectangular leaf cards opaque.
     threshold=m.node_tree.nodes.new('ShaderNodeMath');threshold.operation='GREATER_THAN';threshold.inputs[1].default_value=.35
     m.node_tree.links.new(tex.outputs['Alpha'],threshold.inputs[0]);m.node_tree.links.new(threshold.outputs[0],bs.inputs['Alpha'])
     if hasattr(m,'surface_render_method'):m.surface_render_method='DITHERED'
    m.use_backface_culling=False
  vegetation_materials[key]=m;return m
 for o in obs:
  o.location-=(lo+hi)/2;o.location.z+=h/2
  if name=='pine':
   for i,m in enumerate(list(o.data.materials)):o.data.materials[i]=vegetation_material(m)
  else:o.data.materials.clear();o.data.materials.append(mat('PREVIEW existing '+name+' palette',color))
 is_tree=name in ('tree','pine')
 for side in [-1,1]:
  for i in range(9 if is_tree else 14):
   x=side*(27+random.Random(i).uniform(-2,2));y=-14+i*(5 if is_tree else 3.1)
   height=(6.0+(i%3)*1.4) if is_tree else .75+(i%3)*.18
   for old in obs:
    ob=old.copy();ob.data=old.data;scene.collection.objects.link(ob);ob.scale*=height/h;ob.location*=height/h;ob.location+=Vector((x,y,3.5 if y>11 else 0))
 for o in obs:bpy.data.objects.remove(o,do_unlink=True)
foliage('pine' if (a.inputs/'pine.fbx').exists() else 'tree',(.075,.16,.12));foliage('bush',(.17,.23,.08))
sun=bpy.data.lights.new('Late afternoon sun','SUN');sun.energy=2.2;sun.color=(1,.86,.69);sun.angle=.12
ob=bpy.data.objects.new('Late afternoon sun',sun);scene.collection.objects.link(ob);ob.rotation_euler=(math.radians(28),math.radians(-23),math.radians(-38))
for x,y,z in [(-4,-20,2),(4,-20,2),(-4,21,5.5),(4,21,5.5)]:
 light=bpy.data.lights.new('Amber practical','POINT');light.energy=220;light.color=(1,.3,.055);light.shadow_soft_size=.7
 ob=bpy.data.objects.new('Amber practical',light);scene.collection.objects.link(ob);ob.location=(x,y,z)
camera=bpy.data.objects.new('Bastion I reference camera',bpy.data.cameras.new('Bastion I reference camera'));scene.collection.objects.link(camera);scene.camera=camera
camera.location=(48,-70,54);target=Vector((0,0,0));camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=77
scene.render.resolution_x=1440;scene.render.resolution_y=1000;scene.render.filepath=str(a.output/'bastion1-blender-composition.png')
scene.render.image_settings.file_format='PNG';bpy.ops.render.render(write_still=True)
scene.render.resolution_x=780;scene.render.resolution_y=520;scene.render.filepath=str(a.output/'bastion1-blender-mobile.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(a.output/'Bastion1ReferenceAssembly.blend'),compress=True)
bpy.ops.export_scene.gltf(filepath=str(a.output/'Bastion1ReferenceAssembly.glb'),export_format='GLB',export_cameras=False,export_lights=False)
manifest={'engine':'Blender 4.0.2 source composition, NOT Unity capture','sources':sources,'placements':placements,'material_changes':material_changes,'reference_camera':{'position':[*camera.location],'target':[*target],'ortho_scale':camera.data.ortho_scale},'source_geometry_modified':False,'prototype_supports':['main terrace','upper terrace','valley water'],'external_artist_inference_executed':False,'art_pass':False,'unity_pass':False,'gameplay_pass':False,'main_promotion_allowed':False,'sha256':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in a.output.iterdir() if p.suffix in ['.blend','.glb','.png']}}
(a.output/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n');print('BASTION1_REFERENCE_ASSEMBLED',flush=True)
