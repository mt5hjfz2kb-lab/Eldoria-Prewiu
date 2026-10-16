"""V6 individual 4m Valoria wall: repair exposed white slab at top with REAL
phototextured quoin capstone masonry, then compare TWO gentle CC0 color grades.
All older 84 blocks retained; same fixed review lens; no scene, no approval.
"""
import bpy,sys,math,hashlib,json,random,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=2:raise SystemExit('V5 GLB, output')
source,out=map(Path,args)
for d in ('glb','before','after','reports','atlas'):(out/d).mkdir(parents=True,exist_ok=True)
if not source.is_file():raise RuntimeError('No authentic V5 composed stone module')
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=1140;scene.render.resolution_y=950
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria limestone single-module studio');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.20,.25,.32,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.78;scene.world=world
def light(n,power,pos,color):
 data=bpy.data.lights.new(n,'AREA');data.energy=power;data.color=color;data.size=10;data.shape='DISK'
 ob=bpy.data.objects.new(n,data);bpy.context.collection.objects.link(ob);ob.location=pos
 ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
light('warm key',1800,(-8,-9,13),(1,.85,.72))
light('cool fill',1600,(8,2,11),(.70,.82,1))
light('rim',850,(-3,10,9),(.98,.95,.88))
cd=bpy.data.cameras.new('same review angle');cam=bpy.data.objects.new('same review angle',cd)
bpy.context.collection.objects.link(cam);scene.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
pre=set(bpy.data.objects)
bpy.ops.import_scene.gltf(filepath=str(source))
objects=[o for o in bpy.data.objects if o not in pre]
meshes=[o for o in objects if o.type=='MESH']
def bbox(objects):
 bpy.context.view_layer.update()
 v=[o.matrix_world@Vector(p) for o in objects for p in o.bound_box if o.type=='MESH']
 return Vector([min(p[i] for p in v) for i in range(3)]),Vector([max(p[i] for p in v) for i in range(3)])
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def shoot(p):scene.render.filepath=str(p);bpy.ops.render.render(write_still=True)
if len(meshes)<65:raise RuntimeError('Source is not V5 hand-laid stones')
lo,hi=bbox(meshes)
center=(lo+hi)*.5;extent=hi-lo
cam.location=center+Vector((.8,-1.56,1.11)).normalized()*max(extent.length,1)*3
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
cd.ortho_scale=max(extent.x,extent.y,extent.z)*1.62
shoot(out/'before'/'wall_panel_regular_4m.png')
albedos=set()
for ob in meshes:
 for mat in ob.data.materials:
  if not mat or not mat.use_nodes:continue
  for node in mat.node_tree.nodes:
   if node.type=='TEX_IMAGE' and node.image and 'dressed_stone' in node.image.name.lower():
    albedos.add(node.image)
if len(albedos)!=1:raise RuntimeError('Cannot trace single CC0 atlas '+str([x.name for x in albedos]))
atlas=next(iter(albedos))
arr=np.empty(atlas.size[0]*atlas.size[1]*4,dtype=np.float32)
atlas.pixels.foreach_get(arr)
original=arr.copy()
stone_materials=[]
for ob in meshes:
 if 'individual_ashlar_r05' in ob.name and 'front' in ob.name:
  for mat in ob.data.materials:
   if mat and mat not in stone_materials:stone_materials.append(mat)
if not stone_materials:raise RuntimeError('No verified top row front stones')
top=[o for o in meshes if 'individual_ashlar_r05' in o.name and 'front' in o.name]
if len(top)<4:raise RuntimeError('Top stone course name missing')
rng=random.Random(20261016)
patches=[(160,25,420,170),(505,21,760,166),(55,225,265,382),
         (322,230,500,371),(701,225,840,370),(38,432,315,594),
         (400,433,563,590),(620,443,911,592),(28,636,285,786),
         (325,654,742,790),(215,854,495,985),(695,847,900,984)]
def uv_project(o,patch):
 x1,y1,x2,y2=patch;u0,u1=x1/1024.,x2/1024.;v0,v1=1-y2/1024.,1-y1/1024.
 uv=o.data.uv_layers.active or o.data.uv_layers.new(name='UVMap')
 verts=[v.co for v in o.data.vertices]
 mins=[min(v[i] for v in verts) for i in range(3)]
 ranges=[max(v[i] for v in verts)-mins[i] for i in range(3)]
 for p in o.data.polygons:
  axis=max(range(3),key=lambda i:abs(p.normal[i]))
  a,b=([0,2] if axis==1 else ([0,1] if axis==2 else [1,2]))
  for li in p.loop_indices:
   co=o.data.vertices[o.data.loops[li].vertex_index].co
   u=(co[a]-mins[a])/max(ranges[a],.00001)
   v=(co[b]-mins[b])/max(ranges[b],.00001)
   uv.data[li].uv=(u0+u*(u1-u0),v0+v*(v1-v0))
caps=[]
for i,old in enumerate(sorted(top,key=lambda o:o.location.x)):
 mn,mx=bbox([old]);wide=mx.x-mn.x
 thick=float(extent.y)-.028
 # Cap overlaps the exposed central core, but extends neither left nor right
 # of the original modular envelope and covers its top.
 cy=(float(lo.y)+float(hi.y))*.5
 cz=float(hi.z)-.05
 bpy.ops.mesh.primitive_cube_add(size=1,location=((mn.x+mx.x)*.5,cy,cz))
 ob=bpy.context.object;ob.name='individually_dressed_top_capstone_%02d'%i
 ob.dimensions=(max(.14,float(wide)-.008),thick,.099)
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 ob.data.materials.append(stone_materials[i%len(stone_materials)])
 bevel=ob.modifiers.new('light dressed arris','BEVEL');bevel.width=.014;bevel.segments=3
 bpy.context.view_layer.objects.active=ob;bpy.ops.object.modifier_apply(modifier=bevel.name)
 uv_project(ob,rng.choice(patches));caps.append(ob)
if len(caps)<4:raise RuntimeError('Cap reconstruction empty')
results=[]
for version,brightness in [('warm_limestone',.35),('warm_limestone_bright',.55)]:
 try:
  modified=original.copy()
  rgb=modified.reshape((-1,4))[:,:3]
  # Physically still photographed weathered limestone, lightened via a gentle
  # gamma lift rather than greyscale replacement.
  rgb[:]=np.clip(rgb*(1-brightness)+np.sqrt(np.maximum(rgb,0))*brightness,0,1)
  rgb[:,0]=np.clip(rgb[:,0]*1.035,0,1)
  rgb[:,2]=np.clip(rgb[:,2]*.976,0,1)
  atlas.pixels.foreach_set(modified);atlas.update()
  atlas.filepath_raw=str(out/'atlas'/('dressed_stone_'+version+'.png'))
  atlas.file_format='PNG';atlas.save();atlas.pack()
  shoot(out/'after'/('wall_panel_regular_4m_'+version+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in objects+caps:ob.select_set(True)
  bpy.context.view_layer.objects.active=caps[0]
  dest=out/'glb'/('wall_panel_regular_4m_'+version+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<500000:raise RuntimeError('Missing stone source detail')
  results.append({'variant':version,'lift':brightness,'GLB_SHA256':sha(dest),
    'GLB_bytes':dest.stat().st_size,'source_sha256':sha(source),
    'top_capstone_count':len(caps),'art_approved':False,
    'technical':'PASS_PENDING_ART'})
  print('ARTISAN_WALL_TOP_REPAIR',version,len(caps),dest.stat().st_size,flush=True)
 except Exception as e:
  results.append({'variant':version,'technical':'FAILED','error':repr(e)})
  traceback.print_exc()
 (out/'reports'/'two_capstone_palette_studies.json').write_text(json.dumps({
  'reference':'Valoria official approved warm stone masonry','subject':'one individual 4m wall module',
  'source_run':38864740080,'source_masonry_stones':len(meshes),
  'real_capstone_count':len(caps),'variants':results,'art_approved':False,
  'unity_approved':False,'no_scene_assembly':True},indent=2))
if len(results)!=2 or any(r['technical']!='PASS_PENDING_ART' for r in results):
 raise RuntimeError('Color+top repair batch failed')
print('VALORIA_WALL_V6_TWO_REAL_OPTIONS_COMPLETE',flush=True)
