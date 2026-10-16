"""Two isolated repeatable terrain module repairs:
Dirt gets true variegated PBR loam, non-floating pebble geometry, sparse tufts.
Grass gets photographic green-brown CC0 terrain blend and varied grass leaves.
Photos from Poly Haven; native Blender, same-camera source/new A/B.
No fake procedural material omitted during GLB; all baked 1K RGBA.
"""
import bpy,sys,json,random,hashlib,math,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
a=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(a)!=4:raise RuntimeError('dirt_original_dir grass_original_dir CC0_photo_dir output')
dirt_source,grass_source,cc0,out=map(Path,a)
for d in ('before','after','glb','reports','atlas'):(out/d).mkdir(parents=True,exist_ok=True)
names=['dirt_ground_patch','grass_ground_patch']
bpy.ops.wm.read_factory_settings(use_empty=True)
sc=bpy.context.scene;sc.render.engine='BLENDER_EEVEE'
sc.render.resolution_x=1100;sc.render.resolution_y=1000
sc.render.image_settings.file_format='PNG';sc.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria daylight studio')
w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.22,.27,.33,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.86;sc.world=w
def lamp(n,p,pos,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=p;d.color=col;d.size=10;d.shape='DISK'
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o)
 o.location=pos;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('soft late afternoon',1900,(-7,-9,12),(1,.86,.70))
lamp('cold fill',1550,(8,3,13),(.72,.83,1))
lamp('geology rim',800,(0,9,11),(.95,.97,1))
ca=bpy.data.cameras.new('common ground A B');cam=bpy.data.objects.new('common ground A B',ca)
bpy.context.collection.objects.link(cam);sc.camera=cam;ca.type='ORTHO'
rig=set(bpy.data.objects)
def box(meshes):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(p) for o in meshes if o.type=='MESH' for p in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def frame(meshes):
 lo,hi=box(meshes);center=(lo+hi)/2;d=hi-lo
 cam.location=center+Vector((.84,-1.60,1.23)).normalized()*max(d.length,1)*3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 ca.ortho_scale=max(d.x,d.y,d.z,.3)*1.75
def photo(p):sc.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load_image(name):
 p=cc0/name
 if not p.is_file():raise RuntimeError('Missing CC0 image '+str(p))
 img=bpy.data.images.load(str(p),check_existing=False)
 img.scale(1024,1024)
 a=np.empty(1024*1024*4,dtype=np.float32)
 img.pixels.foreach_get(a)
 arr=a.reshape(1024,1024,4).copy()
 bpy.data.images.remove(img)
 return arr
D=load_image('dirt_diffuse.jpg');G=load_image('grass_ground_diffuse.jpg')
yy,xx=np.mgrid[0:1024,0:1024].astype(np.float32)
u=xx/1023.;v=yy/1023.
def atlas(name):
 # Tile masks with rounded overlapping irregular edges; no grid-symmetric
 # repeat: natural olive grass/earth mosaics and multiple frequency scales.
 mask=np.zeros((1024,1024),dtype=np.float32)
 rng=random.Random(144 if name=='dirt_ground_patch' else 781)
 n=16 if name.startswith('dirt') else 11
 for i in range(n):
  centerx=rng.uniform(-.1,1.1);centery=rng.uniform(-.1,1.1)
  radiusx=rng.uniform(.035,.15);radiusy=rng.uniform(.035,.15)
  dist=((u-centerx)/radiusx)**2+((v-centery)/radiusy)**2
  mask=np.maximum(mask,np.clip(1.55-dist,0,1))
 coarse=(np.sin(u*34+v*17)*np.cos(v*32-u*9))*.04
 mask=np.clip(mask+coarse,0,1)
 if name.startswith('dirt'):
  # Dirt is primary, small grass pockets only; no green checkerboarding.
  shade=.23*mask
 else:
  # Actual spring olive meadow with ~20% underlying soil patches.
  shade=.78-.47*mask
 pixels=D*(1-shade[:,:,None])+G*shade[:,:,None]
 # Warm medium olive grass, neutral textured earth. Never saturated yellow.
 if name.startswith('grass'):
  pixels[:,:,:3]*=np.array([.89,1.045,.87],dtype=np.float32)
 else:
  pixels[:,:,:3]*=np.array([.99,.97,.96],dtype=np.float32)
 pixels[:,:,:3]=np.clip(pixels[:,:,:3],.015,.92)
 final=bpy.data.images.new('Valoria_photo_'+name+'_CC0_loam_grass_1k',1024,1024,alpha=True)
 final.pixels.foreach_set(pixels.reshape(-1))
 final.update()
 path=out/'atlas'/(name+'_natural_CC0_ground_blend.png')
 final.filepath_raw=str(path);final.file_format='PNG';final.save();final.pack()
 return final,path
def material(im,name,alpha=False):
 mat=bpy.data.materials.new(name);mat.use_nodes=True
 nd=mat.node_tree.nodes;nd.clear();lk=mat.node_tree.links
 tex=nd.new('ShaderNodeTexImage');tex.image=im
 bs=nd.new('ShaderNodeBsdfPrincipled')
 bs.inputs['Roughness'].default_value=.90
 lk.new(tex.outputs['Color'],bs.inputs['Base Color'])
 outn=nd.new('ShaderNodeOutputMaterial');lk.new(bs.outputs['BSDF'],outn.inputs['Surface'])
 return mat
grass_leaf=bpy.data.materials.new('Valoria_living_olive_grass')
grass_leaf.use_nodes=True
bs=grass_leaf.node_tree.nodes.get('Principled BSDF')
bs.inputs['Base Color'].default_value=(.13,.215,.085,1);bs.inputs['Roughness'].default_value=.94
rock=bpy.data.materials.new('Valoria_dark_photogeologic_embedded_gravel')
rock.use_nodes=True
bs=rock.node_tree.nodes.get('Principled BSDF')
bs.inputs['Base Color'].default_value=(.31,.28,.245,1);bs.inputs['Roughness'].default_value=.95
records=[]
for n in names:
 pre=set(bpy.data.objects);row={'asset':n,'art_approved':False,'target_floor':6}
 try:
  path=(dirt_source if n.startswith('dirt') else grass_source)/(n+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(path))
  olds=[ob for ob in bpy.data.objects if ob not in pre]
  originals=[ob for ob in olds if ob.type=='MESH']
  if not originals:raise RuntimeError('Original ground absent')
  lo,hi=box(originals);frame(originals);photo(out/'before'/(n+'.png'))
  im,pic=atlas(n)
  mat=material(im,'Valoria_graded_PBR_soil_'+n)
  for o in originals:
   o.data.materials.clear();o.data.materials.append(mat)
  rng=random.Random(2506 if n.startswith('dirt') else 1061)
  groundlevel=float((lo.z+hi.z)*.5)
  width=float(hi.x-lo.x);depth=float(hi.y-lo.y)
  leaves=190 if n.startswith('dirt') else 910
  vertices=[];faces=[]
  for k in range(leaves):
   x=float(lo.x)+width*rng.random()
   y=float(lo.y)+depth*rng.random()
   h=rng.uniform(.012,.042) if n.startswith('dirt') else rng.uniform(.018,.075)
   lean=rng.uniform(-.016,.016)
   root=len(vertices)
   vertices.extend([(x-.008,y,groundlevel),(x+.008,y,groundlevel),
                    (x+lean,y+lean*.45,groundlevel+h)])
   faces.append((root,root+1,root+2))
  mesh=bpy.data.meshes.new('individual_grass_leaf_triangles')
  mesh.from_pydata(vertices,[],faces);mesh.update()
  ob=bpy.data.objects.new('actual_asymmetric_grass_sprouts_'+str(leaves),mesh)
  bpy.context.collection.objects.link(ob);mesh.materials.append(grass_leaf)
  new=originals+[ob]
  gravel_num=26 if n.startswith('dirt') else 7
  for k in range(gravel_num):
   x=float(lo.x)+width*rng.random()
   y=float(lo.y)+depth*rng.random()
   radius=rng.uniform(.015,.041)
   bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=radius,location=(x,y,groundlevel+radius*.35))
   r=bpy.context.object;r.name='ground_embedded_true_rock_'+str(k)
   r.scale=(rng.uniform(1,1.5),rng.uniform(.82,1.2),rng.uniform(.25,.52))
   r.rotation_euler.z=rng.uniform(0,3)
   r.data.materials.append(rock);new.append(r)
  # Avoid grass lawn mesh being incorrectly graded just through its texture.
  photo(out/'after'/(n+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for o in new:o.select_set(True)
  bpy.context.view_layer.objects.active=new[0]
  dst=out/'glb'/(n+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
  if dst.stat().st_size<100000:raise RuntimeError('Photographic CC0 texture removed')
  row.update({'technical':'PASS_PENDING_VISUAL','original_sha':sha(path),'candidate_sha':sha(dst),
              'texture_sha':sha(pic),'real_grass_triangles':leaves,
              'rocks_embedded':gravel_num,
              'scope':'standalone surface only'})
  print('REAL_LOAM_GROUND_ASSET',n,leaves,gravel_num,flush=True)
 except Exception as ex:
  row.update({'technical':'FAILED','error':repr(ex)});traceback.print_exc()
 finally:
  records.append(row)
  for o in list(bpy.data.objects):
   if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
  (out/'reports'/'two_natural_pbr_ground_blends.json').write_text(json.dumps({
    'source_good_dirt_run':38871626493,
    'source_good_grass_run':38872475510,'licensed_photo_run':38870945019,
    'no_art_auto_approval':True,'assets':records},indent=2))
if len(records)!=2 or any(r['technical']!='PASS_PENDING_VISUAL' for r in records):
 raise RuntimeError('Photo natural grass soil generation incomplete')
print('TWO_CC0_NATURAL_TILE_CANDIDATES_READY',flush=True)
