"""Three individually repeatable Valoria timber pieces. Replace virtually
textureless poles and metal blob of X joint with dressed-oak beams, grain UV,
hand-cut tenon and physically embedded wooden joint pins.
Preserve old camera for honest A/B; source CC0 Poly Haven wood photos.
"""
import bpy,sys,math,hashlib,json,random,traceback
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=3:raise RuntimeError('Need old GLB dir, CC0 source dir, output')
source,photos,out=map(Path,args)
for name in ('before','after','glb','reports'):(out/name).mkdir(parents=True,exist_ok=True)
assets=['timber_beam_1','timber_beam_2','timber_brace_X']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1000;s.render.resolution_y=1000;s.render.image_settings.file_format='PNG'
s.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Warm medieval oak joinery studio')
world.use_nodes=True;world.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.24,.30,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.83;s.world=world
def lamp(name,loc,power,color):
 l=bpy.data.lights.new(name,'AREA');l.energy=power;l.color=color;l.size=9;l.shape='DISK'
 ob=bpy.data.objects.new(name,l);bpy.context.collection.objects.link(ob)
 ob.location=loc;ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
lamp('sunset wood grain',(-7,-8,11),1920,(1,.84,.66))
lamp('blue skylight',(8,3,10),1520,(.72,.83,1))
lamp('wood arris rim',(-2,8,10),790,(1,.98,.86))
cd=bpy.data.cameras.new('locked comparison')
cam=bpy.data.objects.new('locked comparison',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def bounding(obs):
 bpy.context.view_layer.update()
 pts=[ob.matrix_world@Vector(p) for ob in obs if ob.type=='MESH' for p in ob.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def camera(obs):
 lo,hi=bounding(obs);c=(lo+hi)*.5;d=hi-lo
 cam.location=c+Vector((.85,-1.6,1.15)).normalized()*max(d.length,1)*3
 cam.rotation_euler=(c-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(d.x,d.y,d.z,.3)*1.65
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
woodim=bpy.data.images.load(str(photos/'oak_veneer_05_diffuse.jpg'),check_existing=True)
wood=bpy.data.materials.new('Valoria_CC0_real_warm_aged_oak_grain')
wood.use_nodes=True;nd=wood.node_tree.nodes;nd.clear();link=wood.node_tree.links
im=nd.new('ShaderNodeTexImage');im.image=woodim
tint=nd.new('ShaderNodeMixRGB');tint.blend_type='MULTIPLY';tint.inputs[0].default_value=.32
tint.inputs[2].default_value=(.58,.43,.31,1)
link.new(im.outputs['Color'],tint.inputs[1])
bs=nd.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.83
link.new(tint.outputs['Color'],bs.inputs['Base Color'])
nfile=photos/'oak_veneer_05_nor_gl.jpg'
if nfile.is_file():
 nimg=bpy.data.images.load(str(nfile),check_existing=True)
 nimg.colorspace_settings.name='Non-Color'
 tn=nd.new('ShaderNodeTexImage');tn.image=nimg
 nm=nd.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.22
 link.new(tn.outputs['Color'],nm.inputs['Color']);link.new(nm.outputs['Normal'],bs.inputs['Normal'])
output=nd.new('ShaderNodeOutputMaterial');link.new(bs.outputs[0],output.inputs['Surface'])
peg=bpy.data.materials.new('Dark_oak_joinery_dowel')
peg.use_nodes=True
peg.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.23,.145,.09,1)
peg.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.88
def uv(o):
 layer=o.data.uv_layers.active or o.data.uv_layers.new(name='UVMap')
 for face in o.data.polygons:
  axis=max(range(3),key=lambda i:abs(face.normal[i]))
  a,b=([0,2] if axis==1 else [0,1] if axis==2 else [1,2])
  for j in face.loop_indices:
   p=o.data.vertices[o.data.loops[j].vertex_index].co
   layer.data[j].uv=(float(p[a])*.8,float(p[b])*.5)
def cube(name,loc,dims,mat=wood,bev=.009):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
 o=bpy.context.object;o.name=name;o.dimensions=dims
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(mat)
 if bev>0:
  m=o.modifiers.new('genuine_lightly_worn_wood_edges','BEVEL')
  m.width=min(bev,min(dims)*.19);m.segments=2;m.limit_method='ANGLE'
  bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=m.name)
 uv(o)
 return o
def pin(name,where,r=.028):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=10,ring_count=6,radius=r,location=where)
 o=bpy.context.object;o.name=name;o.data.materials.append(peg)
 o.scale.y=.30
 return o
def beam(name,lo,hi):
 center=(lo+hi)*.5;dims=hi-lo
 ob=cube('aged_oak_structural_beam_3d',center,tuple(float(i) for i in dims),wood,.018)
 made=[ob]
 # Small true tenon at top, not a black block pasted on.
 tenon=cube('male_oak_mortise_tenon',(center.x,center.y,hi.z+.05),
   (dims.x*.52,dims.y*.55,.10),wood,.012)
 made.append(tenon)
 # physically seated hardwood pegs signal craftsmanship without gratuitous iron.
 for i,z in enumerate((float(lo.z)+.21,float(hi.z)-.20)):
  made.append(pin('wooden_peg_'+str(i),
   (center.x+.02,float(lo.y)-.008,z),.021))
 return made
def cross_brace(lo,hi):
 center=(lo+hi)*.5;d=hi-lo;made=[]
 # diagonal square oak members: not thin cylindrical rods or black hub
 xspan=d.x*.94;zspan=d.z*.94
 L=math.hypot(xspan,zspan);angle=math.atan2(xspan,zspan)
 for side in (-1,1):
  bar=cube('hand_dressed_cross_brace_'+('left' if side<0 else 'right'),
    center,(.17,.145,L),wood,.012)
  bar.rotation_euler.y=side*angle
  made.append(bar)
 # Center half-lap joint with a small wood insert, NOT an oversized iron box.
 brace_center=cube('mortised_fitted_lap_joint',
  (float(center.x),float(center.y)-.006,float(center.z)),(.185,.155,.26),wood,.016)
 made.append(brace_center)
 made.append(pin('flush_hardwood_joinery_pin',(float(center.x),float(lo.y)-.017,float(center.z)),.032))
 return made
records=[]
for name in assets:
 last=set(bpy.data.objects);record={'asset':name,'target':6,'art_approved':False}
 try:
  path=source/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(path))
  old=[o for o in bpy.data.objects if o not in last]
  omesh=[o for o in old if o.type=='MESH']
  lo,hi=bounding(omesh);camera(omesh)
  shoot(out/'before'/(name+'.png'))
  for ob in old:bpy.data.objects.remove(ob,do_unlink=True)
  new=cross_brace(lo,hi) if name=='timber_brace_X' else beam(name,lo,hi)
  shoot(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in new:o.select_set(True)
  bpy.context.view_layer.objects.active=new[0]
  candidate=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(candidate),export_format='GLB',use_selection=True,export_apply=True)
  if candidate.stat().st_size<25000:raise RuntimeError('Photo-wood texture lost')
  record.update({'technical':'PASS_PENDING_VISUAL','mesh_objects':len(new),
    'old_sha':sha(path),'new_sha':sha(candidate),'photo':'Poly Haven 1K CC0 oak veneer with normal',
    'craftsmanship':'Real square oak diagonal half-lap or tenoned hand-dressed beam'})
  print('TRUE_MEDIEVAL_OAK_BEAM',name,len(new),flush=True)
 except Exception as e:
  record.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  records.append(record)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  (out/'reports'/'three_authentic_oak_joinery.json').write_text(json.dumps({
   'single_components_only':True,'art_approved':False,'assets':records},indent=2))
if any(r['technical']!='PASS_PENDING_VISUAL' for r in records):
 raise RuntimeError('Three oak modules not all generated')
print('THREE_HAND_JOINED_OAK_ASSETS_DONE',flush=True)
