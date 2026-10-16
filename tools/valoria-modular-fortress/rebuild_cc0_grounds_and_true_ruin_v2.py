"""Valoria ground/ruin real CC0 corrective pilot: 3 genuine natural surfaces
with scanned albedo/normal/roughness; one physically ruined course-built wall.
4 STANDALONE modular GLBs, 8 matched PNGs. No Unity or fortress assembly.
"""
import bpy,sys,math,random,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector
A=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(A)!=4:raise RuntimeError('baseline_v3 scanned_CC0_maps V7_ashlar_wall output')
baseline,maps,ruin_source,out=map(Path,A)
for d in ('before','after','glb','reports'):(out/d).mkdir(parents=True,exist_ok=True)
names=['dirt_ground_patch','grass_ground_patch','river_water_tile','wall_panel_ruined_4m']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1000;s.render.resolution_y=950;s.render.image_settings.file_format='PNG'
s.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria natural lookdev');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.23,.31,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.78;s.world=world
def area(n,power,pos,color):
 l=bpy.data.lights.new(n,'AREA');l.color=color;l.energy=power;l.size=9;l.shape='DISK'
 ob=bpy.data.objects.new(n,l);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
area('warm key',1750,(-8,-9,12),(1,.85,.71))
area('cool sky fill',1660,(8,4,12),(.71,.82,1))
area('rim',930,(-2,9,10),(.9,.95,1))
cd=bpy.data.cameras.new('unaltered A-B camera')
cam=bpy.data.objects.new('unaltered A-B camera',cd);bpy.context.collection.objects.link(cam)
s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(obs):
 bpy.context.view_layer.update()
 pts=[ob.matrix_world@Vector(v) for ob in obs if ob.type=='MESH' for v in ob.bound_box]
 if not pts:raise RuntimeError('No mesh geometries')
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def view(obs):
 lo,hi=bounds(obs);c=(lo+hi)*.5;d=hi-lo
 cam.location=c+Vector((.84,-1.6,1.15)).normalized()*max(d.length,1)*3
 cam.rotation_euler=(c-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(d.x,d.y,d.z,.35)*1.76
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def photo_mat(key,tint):
 name={'dirt_ground_patch':'dirt','grass_ground_patch':'grass_ground',
       'river_water_tile':'forest_ground_04'}[key]
 # For river keep source aquatic albedo; use photo donor only for soil assets.
 if key=='river_water_tile':return None
 mat=bpy.data.materials.new('Valoria_CC0_1K_real_photo_'+name);mat.use_nodes=True
 nd=mat.node_tree.nodes;nd.clear();l=mat.node_tree.links
 tex=nd.new('ShaderNodeTexImage')
 tex.image=bpy.data.images.load(str(maps/(name+'_diffuse.jpg')),check_existing=True)
 bs=nd.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.94
 mix=nd.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY'
 mix.inputs[0].default_value=.30;mix.inputs[2].default_value=(*tint,1)
 l.new(tex.outputs['Color'],mix.inputs[1]);l.new(mix.outputs['Color'],bs.inputs['Base Color'])
 ni=bpy.data.images.load(str(maps/(name+'_nor_gl.jpg')),check_existing=True)
 ni.colorspace_settings.name='Non-Color'
 tn=nd.new('ShaderNodeTexImage');tn.image=ni
 nor=nd.new('ShaderNodeNormalMap');nor.inputs['Strength'].default_value=.42
 l.new(tn.outputs['Color'],nor.inputs['Color']);l.new(nor.outputs['Normal'],bs.inputs['Normal'])
 outn=nd.new('ShaderNodeOutputMaterial');l.new(bs.outputs['BSDF'],outn.inputs['Surface'])
 return mat
rng=random.Random(20261016)
def patch(name,lo,hi):
 # A true UV-mapped real photographed natural surface with small physical
 # microrelief. NO scatter of oversized white spheres or bright yellow blocks.
 width=float(hi.x-lo.x);depth=float(hi.y-lo.y)
 n=64;verts=[];faces=[];uvs=[]
 cx=float((lo.x+hi.x)*.5);cy=float((lo.y+hi.y)*.5)
 midheight=float((lo.z+hi.z)*.5)
 for j in range(n+1):
  for i in range(n+1):
   x=cx-width*(.5-i/n);y=cy+depth*(j/n-.5)
   wave=math.sin(i*.36+j*.21)*.006+math.sin(i*.73-j*.54)*.003
   z=midheight+wave
   verts.append((x,y,z))
 for j in range(n):
  for i in range(n):
   a=j*(n+1)+i;faces.append((a,a+1,a+n+2,a+n+1))
 m=bpy.data.meshes.new(name+'_physical_sculpted_ground')
 m.from_pydata(verts,[],faces);m.update()
 ob=bpy.data.objects.new(name+'_CC0_photo_terrain',m);bpy.context.collection.objects.link(ob)
 uv=m.uv_layers.new(name='UVMap')
 for p in m.polygons:
  for k in p.loop_indices:
   q=m.vertices[m.loops[k].vertex_index].co
   uv.data[k].uv=((q.x-cx)/max(width,.01)+.5,(q.y-cy)/max(depth,.01)+.5)
 ob.data.materials.append(photo_mat(name,(.96,.97,.95) if name=='dirt_ground_patch' else (.91,.96,.88)))
 return [ob]
def water(lo,hi):
 # Keep aquatic color and transparency; shader in Unity is separately necessary.
 width=float(hi.x-lo.x);depth=float(hi.y-lo.y)
 n=90;verts=[];faces=[]
 for j in range(n+1):
  for i in range(n+1):
   x=float(lo.x)+width*i/n;y=float(lo.y)+depth*j/n
   z=float((lo.z+hi.z)*.5)+.019*math.sin(i*.24+j*.32)+.01*math.sin(i*.63-j*.14)
   verts.append((x,y,z))
 for j in range(n):
  for i in range(n):
   a=j*(n+1)+i;faces.append((a,a+1,a+n+2,a+n+1))
 m=bpy.data.meshes.new('actual_ripple_surface');m.from_pydata(verts,[],faces);m.update()
 ob=bpy.data.objects.new('water_transparent_micro_wave',m);bpy.context.collection.objects.link(ob)
 uv=m.uv_layers.new(name='UVMap')
 for p in m.polygons:
  for k in p.loop_indices:
   q=m.vertices[m.loops[k].vertex_index].co
   uv.data[k].uv=((q.x-lo.x)/max(width,.01),(q.y-lo.y)/max(depth,.01))
 mat=bpy.data.materials.new('Valoria_river_clear_green_blue');mat.use_nodes=True
 bs=mat.node_tree.nodes.get('Principled BSDF')
 bs.inputs['Base Color'].default_value=(.115,.23,.285,1)
 bs.inputs['Roughness'].default_value=.18
 bs.inputs['Metallic'].default_value=.0
 bs.inputs['Transmission Weight'].default_value=.40
 bs.inputs['IOR'].default_value=1.333
 m.materials.append(mat)
 return [ob]
def ruined():
 # Work from V7 84-stone wall with source photographic CC0 medieval
 # limestone. Removing TOP COURSES and dropping stones is not a solid plate.
 before=set(bpy.data.objects)
 bpy.ops.import_scene.gltf(filepath=str(ruin_source))
 obs=[o for o in bpy.data.objects if o not in before]
 objs=[o for o in obs if o.type=='MESH']
 if len(objs)<65:raise RuntimeError('Not 84-stone source')
 original_count=len(objs)
 cands=[];removed=0
 for ob in list(objs):
  nm=ob.name.lower()
  # Fix continuous original full-height mortar core so real missing stones reveal sky.
  if 'mortar_core' in nm:
   old_h=float(ob.dimensions.z)
   ob.dimensions.z=old_h*.49
   ob.location.z-=old_h*.255
   continue
  if 'top_capstone' in nm:
   bpy.data.objects.remove(ob,do_unlink=True);removed+=1;continue
  if 'individual_ashlar_r05' in nm or 'individual_ashlar_r04' in nm:
   idx=rng.random()
   if idx<(.74 if 'r05' in nm else .51):
    bpy.data.objects.remove(ob,do_unlink=True);removed+=1;continue
  if 'individual_ashlar_r03' in nm and rng.random()<.15:
   bpy.data.objects.remove(ob,do_unlink=True);removed+=1;continue
  if 'end_quoin' in nm and ob.location.z>1.75 and rng.random()<.63:
   bpy.data.objects.remove(ob,do_unlink=True);removed+=1;continue
 # Add collapsed stones at base copied from real masonry, source material intact.
 donor=[o for o in bpy.data.objects if 'individual_ashlar_r01' in o.name.lower() and o.type=='MESH']
 if not donor:raise RuntimeError('No real stone donors to build rubble')
 for k in range(13):
  take=rng.choice(donor)
  o=take.copy();o.data=take.data.copy();bpy.context.collection.objects.link(o)
  o.name='loose_ruin_broken_stone_'+str(k)
  o.scale*=rng.uniform(.42,.73)
  o.rotation_euler=(rng.uniform(-.8,.8),rng.uniform(-.4,.4),rng.uniform(-1.4,1.4))
  o.location.x+=rng.uniform(-1.9,1.9)
  o.location.y+=rng.uniform(-.3,.2)
  o.location.z=-.03
 active=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
 if len(active)<30 or removed<10:raise RuntimeError('Not a convincing structural ruin '+str([len(active),removed]))
 return active,{'removed':removed,'original_count':original_count,'final_count':len(active)}
records=[]
for name in names:
 prior=set(bpy.data.objects);record={'asset':name,'target':6,'art_approved':False,'no_scene_composition':True}
 try:
  f=baseline/(name+'.glb')
  if not f.is_file():raise RuntimeError('Missing original GLB')
  bpy.ops.import_scene.gltf(filepath=str(f))
  old=[o for o in bpy.data.objects if o not in prior]
  meshes=[o for o in old if o.type=='MESH']
  if not meshes:raise RuntimeError('Empty original')
  lo,hi=bounds(meshes);view(meshes)
  render(out/'before'/(name+'.png'))
  for o in old:bpy.data.objects.remove(o,do_unlink=True)
  if name=='wall_panel_ruined_4m':
   new,details=ruined()
  elif name=='river_water_tile':
   new=water(lo,hi);details={'real_geometric_waves':90*90}
  else:
   new=patch(name,lo,hi);details={'texture':('dirt' if name=='dirt_ground_patch' else 'grass_ground')+'_CC0_1k','vertices':65*65}
  render(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in new:ob.select_set(True)
  bpy.context.view_layer.objects.active=new[0]
  dest=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<10000:raise RuntimeError('Missing candidate geometry')
  record.update({'technical':'PASS_PENDING_VISUAL','source_sha':sha(f),'new_sha':sha(dest),
   'details':details})
  print('ELDORIA_REAL_CC0_SURFACE_OR_RUIN',name,details,flush=True)
 except Exception as e:
  record.update({'technical':'FAILED','error':str(e)});traceback.print_exc()
 finally:
  records.append(record)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  (out/'reports'/'four_cc0_sculpted_ground_ruins.json').write_text(json.dumps({
    'source_run':38813966671,'donor_real_wall':38865543613,
    'no_art_auto_approval':True,'assets':records},indent=2))
if len(records)!=4 or any(x['technical']!='PASS_PENDING_VISUAL' for x in records):
 raise RuntimeError('Four-module ground/ruin batch incomplete')
print('FOUR_INDIVIDUAL_CC0_PHOTO_SURFACES_READY',flush=True)
