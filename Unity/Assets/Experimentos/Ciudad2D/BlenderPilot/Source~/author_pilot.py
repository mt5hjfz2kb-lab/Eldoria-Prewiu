"""Valoria: 2 actual Blender-authored buildings, pre-rendered, camera locked.
Reuses vetted *geometry only* from unapproved Phase2 kit. Originals unchanged.
Blender 4.x: blender -b -t 4 --python-exit-code 1 --python author_pilot.py --
  --kit-dir PATH --output PATH
"""
import argparse,sys,math,json,hashlib,pathlib,random
import bpy
from mathutils import Vector
parser=argparse.ArgumentParser()
parser.add_argument('--kit-dir',required=True,type=pathlib.Path)
parser.add_argument('--output',required=True,type=pathlib.Path)
parser.add_argument('--size',type=int,default=1050)
parser.add_argument('--samples',type=int,default=24)
a=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
assert a.size>=512 and a.size<=2048 and a.samples>=8 and a.samples<=96
assert a.kit_dir.is_dir()
a.output.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene
scene.render.engine='CYCLES';scene.cycles.samples=a.samples
scene.cycles.device='CPU'
# Ubuntu's packaged Blender omits OpenImageDenoiser; disabling denoising is
# necessary for reproducible Cycles renders on the open-source CI runner.
for view_layer in scene.view_layers:
 if hasattr(view_layer, 'cycles'): view_layer.cycles.use_denoising=False
if hasattr(scene.cycles, 'use_denoising'): scene.cycles.use_denoising=False
scene.render.resolution_x=a.size;scene.render.resolution_y=a.size
scene.render.resolution_percentage=100
scene.render.film_transparent=True
scene.render.image_settings.file_format='PNG'
scene.render.image_settings.color_mode='RGBA'
scene.render.image_settings.color_depth='8'
try:scene.view_settings.view_transform='AgX'
except Exception:scene.view_settings.view_transform='Standard'
world=bpy.data.worlds.new('Valoria Dusk');scene.world=world
world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.38,.43,.54,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.52
random.seed(417)
def mat(name, rgb, rough=.73,metal=0,variation=.14):
 m=bpy.data.materials.new(name);m.use_nodes=True;n=m.node_tree.nodes;l=m.node_tree.links
 bs=n.get('Principled BSDF')
 bs.inputs['Base Color'].default_value=(*rgb,1)
 bs.inputs['Roughness'].default_value=rough;bs.inputs['Metallic'].default_value=metal
 if variation:
  noise=n.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=19
  noise.inputs['Detail'].default_value=4
  ramp=n.new('ShaderNodeValToRGB')
  ramp.color_ramp.elements[0].color=(*(v*(1-variation) for v in rgb),1)
  ramp.color_ramp.elements[1].color=(*(min(1,v*(1+variation)) for v in rgb),1)
  l.new(noise.outputs['Fac'],ramp.inputs['Fac'])
  l.new(ramp.outputs['Color'],bs.inputs['Base Color'])
  bump=n.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.12
  bump.inputs['Distance'].default_value=.034
  l.new(noise.outputs['Fac'],bump.inputs['Height'])
  l.new(bump.outputs['Normal'],bs.inputs['Normal'])
 return m
stone=mat('Limestone_WarmGrey',(.41,.38,.31),variation=.24)
limestone=mat('Trim_Stone',(.59,.54,.46),variation=.15)
wood=mat('Oak_Old',(.28,.18,.11),variation=.26)
newwood=mat('Oak_Cut',(.52,.32,.17),variation=.17)
slate=mat('Slate_Blue',(.14,.22,.31),variation=.24)
gold=mat('Brass',(.48,.33,.15),.40,.65,.13)
iron=mat('Iron',(.14,.15,.16),.48,.8,.12)
navy=mat('Heraldry_Blue',(.075,.13,.27),.88,0,.10)
glow=mat('Window_Amber',(.9,.43,.11),.26,0,0)
glow.node_tree.nodes['Principled BSDF'].inputs['Emission Color'].default_value=(1,.37,.08,1)
glow.node_tree.nodes['Principled BSDF'].inputs['Emission Strength'].default_value=.9
collections={}
for name in ['Cuartel','Aserradero']:
 c=bpy.data.collections.new('BUILDING_'+name);scene.collection.children.link(c);collections[name]=c
log={'kit_ref':'art/visual-reboot@84bb8e7a83810f4b722c764938a5d36e20b906d5','materials':[], 'pieces':[], 'buildings':{},'camera':{},'shadows':'Cycles catcher separate pass; zero alpha fails QA','art_approved':False}
def parent(ob,group):
 for old in list(ob.users_collection):old.objects.unlink(ob)
 collections[group].objects.link(ob)
 return ob
def cube(name,loc,size,ma,group,bevel=.05):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
 o=bpy.context.object;o.name=name;o.dimensions=size
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(ma);parent(o,group)
 if bevel:
  b=o.modifiers.new('WornCornice','BEVEL');b.width=min(bevel,min(size)*.18);b.segments=2
  o.modifiers.new('FaceNormals','WEIGHTED_NORMAL')
 return o
def cylinder(name,loc,r,depth,ma,group,verts=14):
 bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=depth,location=loc)
 o=bpy.context.object;o.name=name;o.data.materials.append(ma)
 parent(o,group);return o
def beam(name,v0,v1,r,ma,group):
 p=Vector(v0);q=Vector(v1);d=q-p
 o=cylinder(name,(p+q)/2,r,d.length,ma,group,8)
 o.rotation_euler=d.to_track_quat('Z','Y').to_euler()
 return o
def imported(name,loc,extent,ma,group):
 """Read-only sibling kit module, preserve actual mesh and pivot at base."""
 path=a.kit_dir/(name+'.fbx')
 if not path.is_file():raise FileNotFoundError('Required Phase2 kit asset: '+str(path))
 before=set(bpy.data.objects)
 bpy.ops.import_scene.fbx(filepath=str(path.resolve()),use_anim=False)
 mesh=[x for x in bpy.data.objects if x not in before and x.type=='MESH']
 if not mesh:raise ValueError('Kit FBX imported no geometry: '+name)
 for x in mesh:
  for old in list(x.users_collection):old.objects.unlink(x)
  collections[group].objects.link(x)
  x.name='Kit_'+name+'_'+group
  x.data.materials.clear();x.data.materials.append(ma)
  # Normalize dimension against FBX pivot, preserve proportional geometry.
  dx=max(1e-6,x.dimensions.x);dy=max(1e-6,x.dimensions.y)
  dz=max(1e-6,x.dimensions.z)
  x.scale=(x.scale.x*extent[0]/dx,x.scale.y*extent[1]/dy,x.scale.z*extent[2]/dz)
  x.location=loc
 log['pieces'].append({'asset':name,'building':group,'geometry_provenance':'Phase2 unapproved Blender kit, no materials/atlas reused'})
 return mesh[0]
def target(x,y,z,group):
 cylinder('Archery_Round_Target',(x,y,z),.38,.12,newwood,group,36).rotation_euler[0]=math.pi/2
 cylinder('Archery_Inner',(x,y-.09,z),.25,.014,limestone,group,32).rotation_euler[0]=math.pi/2
 cylinder('Archery_Centre',(x,y-.107,z),.09,.015,navy,group,24).rotation_euler[0]=math.pi/2
 for v in (-1,1):beam('Archery_Rig',(x,y,z),(x+v*.39,y+.12,.02),.07,wood,group)
def flag(name,base,group):
 x,y,z=base
 beam(name+'_pole',(x,y,z),(x,y,z+1.65),.045,iron,group)
 verts=[];faces=[]
 for rr in range(5):
  for cc in range(5):
   t=rr/4;u=cc/4
   verts.append((x+u*1.08,y+.06*math.sin(u*4.5+t*2.5),z+1.52-rr*.26))
 for rr in range(4):
  for cc in range(4):
   p=rr*5+cc;faces.append((p,p+1,p+6,p+5))
 mesh=bpy.data.meshes.new('WavingCloth');mesh.from_pydata(verts,[],faces);mesh.update()
 o=bpy.data.objects.new(name+'_HeraldryFlag',mesh);collections[group].objects.link(o)
 o.data.materials.append(navy)
 o.modifiers.new('FlagThickness','SOLIDIFY').thickness=.018
 beam(name+'_trimming',(x+.04,y,z+1.48),(x+1.0,y,z+1.48),.022,gold,group)
def logpiece(x,y,z,length,r,group):
 c=cylinder('Worklog_Barked',(x,y,z),r,length,wood,group)
 c.rotation_euler[0]=math.pi/2
 for sign in (-1,1):
  e=cylinder('FreshEndgrain',(x,y+sign*length/2,z),r*.93,.02,newwood,group)
  e.rotation_euler[0]=math.pi/2

# Cuartel: masonry garrison, offset tower, genuine 3D pitched slate roof.
g='Cuartel'
for x in (-3.1,0,3.1):
 imported('Wall_Door_Intact' if x==0 else 'Wall_Window_Intact',(x,-1.9,0),(3.0,.70,3.65),limestone if x==0 else stone,g)
for x in (-3.1,0,3.1):
 imported('Wall_Straight_Intact',(x,2.1,0),(3.0,.70,3.65),stone,g)
for x in (-4.6,4.6):
 for yy in (-1.55,.2,1.55):
  cube('Buttress_Limestone',(x,yy,1.6),(.38,.5,3.2),limestone,g)
# Roof_Gable is detailed curving 3D module, better than a flat slab.
imported('Roof_Gable',(0,.0,3.55),(9.6,6.4,2.65),slate,g)
imported('Tower_Bell',(-3.1,1.45,0),(2.5,2.5,7.2),stone,g)
imported('Roof_Tower',(-3.1,1.45,7.10),(3.0,3.0,2.0),slate,g)
for px in (-2.55,2.35):
 target(px,-2.65,1.18,g)
for px in (-4.2,-4.0,-3.8,-3.6):
 beam('Spears',(px,-2.4,.12),(px,-2.4,2.28),.032,iron,g)
cube('Barracks_ErodedEntryStep',(0,-2.45,.09),(1.8,.5,.18),limestone,g)
flag('Barracks_Tower',(-3.3,1.4,8.6),g)
imported('Shield',(-1.5,-2.31,2.7),(.50,.28,.8),gold,g)
# Aserradero: unmistakably operational open structure, broad low roof, timber.
g='Aserradero'
for x in (-3.8,3.8):
 for y in (-1.8,1.8):
  cube('OakPost_Footing',(x,y,.22),(.57,.56,.44),stone,g)
  beam('OakColumn',(x,y,.28),(x,y,3.65),.18,wood,g)
for y in (-1.8,1.8):
 beam('OakHeader',(-4.08,y,3.65),(4.08,y,3.65),.15,newwood,g)
for x in (-3.8,3.8):
 beam('DiagonalBracket',(x,-1.85,2.85),(x,.2,3.65),.12,wood,g)
for yy in (-.55,1.55):
 imported('Wall_Window_Intact',(-3.8,yy,0),(.55,2.25,3.25),wood,g)
imported('Roof_Shed',(-1.85,.30,3.58),(4.9,5.9,1.45),slate,g)
imported('Roof_Shed',(2.0,.30,3.38),(4.9,5.8,1.25),slate,g)
imported('Chimney',(-3.3,1.10,3.40),(1.05,1.08,1.90),stone,g)
for y in (-2.30,-.65):
 cube('OpenCuttingBench',(-.2,y,1.19),(4.75,.72,.22),newwood,g)
 for x in (-2.1,1.8):beam('Trestle',(x,y,.09),(x,y,1.14),.11,wood,g)
# True rotating water-wheel geometry, 16 rim elements, 8 spokes.
cx,cy,cz,r=3.08,1.98,1.68,1.48
for side in (-1,1):
 yy=cy+side*.35
 for i in range(16):
  t=math.tau*i/16;u=math.tau*(i+1)/16
  beam('Waterwheel_Rim',(cx+r*math.cos(t),yy,cz+r*math.sin(t)),
      (cx+r*math.cos(u),yy,cz+r*math.sin(u)),.09,wood,g)
 for i in range(8):
  t=math.tau*i/8
  beam('Waterwheel_Spoke',(cx,yy,cz),(cx+r*math.cos(t),yy,cz+r*math.sin(t)),.061,newwood,g)
for i in range(3):
 for j in range(2): logpiece(-3.25+i*.50,-2.68+j*.50,.22+i*.17,2.0,.20,g)
beam('SteelSaw',(1.35,-1.06,.14),(1.35,-1.06,3.14),.046,iron,g)
cube('SawBlade',(1.35,-1.12,1.8),(.11,.06,2.4),iron,g,.02)
flag('Miller',(-3.7,-1.8,3.1),g)
log['buildings']={
 'Cuartel':{'identity':'stone garrison + off-center tower + training targets','footprint':[9.6,6.0]},
 'Aserradero':{'identity':'open timber saw shop + working wheel + logs','footprint':[9.5,6.4]}
}
cam_data=bpy.data.cameras.new('MASTER_ORTHOGRAPHIC')
cam=bpy.data.objects.new('MASTER_ORTHOGRAPHIC',cam_data);scene.collection.objects.link(cam)
scene.camera=cam;cam_data.type='ORTHO'
look=Vector((0,0,3.1));cam.location=Vector((12.6,-15.0,11.1))
cam.rotation_euler=(look-cam.location).to_track_quat('-Z','Y').to_euler()
cam_data.ortho_scale=15.2
log['camera']={'projection':'orthographic','location':list(cam.location),
 'look_at':list(look),'ortho_scale':cam_data.ortho_scale,'image_size':a.size,
 'pivot':'bottom_center_after_alpha_crop','input_ground_level_m':0}
def area(name,loc,power,rgb):
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.color=rgb;d.shape='DISK';d.size=5
 obj=bpy.data.objects.new(name,d);scene.collection.objects.link(obj)
 obj.location=loc;obj.rotation_euler=(look-obj.location).to_track_quat('-Z','Y').to_euler()
area('Sun_Amber_Sunset',(-9,-8,13),1500,(1,.74,.53))
area('Sky_BlueFill',(9,7,10),840,(.53,.62,.94))
# Plane is Cycles shadow catcher, never baked as rectangular slab.
bpy.ops.mesh.primitive_plane_add(size=30,location=(0,0,-.015))
catcher=bpy.context.object;catcher.name='CYCLES_CONTACT_SHADOW_ONLY';catcher.is_shadow_catcher=True
catcher.hide_render=True
log['materials']=[m.name for m in [stone,limestone,wood,newwood,slate,gold,iron,navy,glow]]
for name in ('Cuartel','Aserradero'):
 for other,col in collections.items():col.hide_render=other!=name
 path=a.output/(name+'_Color.png')
 catcher.hide_render=True
 scene.render.filepath=str(path);bpy.ops.render.render(write_still=True)
 # Preserve same projection while isolating physical contact shadow pass.
 old=[]
 for obj in scene.objects:
  if obj.type=='MESH' and obj!=catcher:
   old.append((obj,obj.visible_camera));obj.visible_camera=False
 catcher.hide_render=False
 shadow=a.output/(name+'_ContactShadow.png')
 scene.render.filepath=str(shadow);bpy.ops.render.render(write_still=True)
 for obj,v in old:obj.visible_camera=v
 catcher.hide_render=True
 if not path.is_file() or path.stat().st_size<12000 or not shadow.is_file():
  raise RuntimeError('BLENDER_OUTPUT_MISSING '+name)
 log['buildings'][name].update({'color_sha256':hashlib.sha256(path.read_bytes()).hexdigest(),
  'shadow_sha256':hashlib.sha256(shadow.read_bytes()).hexdigest(),
  'mesh_objects':len([obj for obj in collections[name].objects if obj.type=='MESH'])})
for col in collections.values():col.hide_render=False
bpy.ops.wm.save_as_mainfile(filepath=str((a.output/'Valoria_Buildings_Master.blend').resolve()))
log['master_sha256']=hashlib.sha256((a.output/'Valoria_Buildings_Master.blend').read_bytes()).hexdigest()
(a.output/'blender-pilot-manifest.json').write_text(json.dumps(log,indent=2)+'\n')
print('BLENDER_REAL_MODEL_AND_PRERENDER_PASS',json.dumps(log))
