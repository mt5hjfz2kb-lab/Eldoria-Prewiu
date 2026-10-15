"""Single modular Eldoria cliff rock panel V6 (NOT a castle composition).
Repair V5 cliff's artificial stacked-block silhouette with an irregular connected
faceted Alpine stratified rock face, preserving Poly Haven CC0 PBR donor.
"""
import bpy,sys,math,random,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector,noise
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
source=Path(args[0]);out=Path(args[1])
for part in ('glb','before','after','reports'):(out/part).mkdir(exist_ok=True,parents=True)
n='cliff_rock_column'
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=980;s.render.resolution_y=980
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Eldoria cliff studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.16,.20,.27,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.8;s.world=w
def area(n,p,pow,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=pow;d.color=col;d.shape='DISK';d.size=11
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=p
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
area('Dusk limestone cool key',(-7,-9,13),1850,(.82,.9,1))
area('Amber raking light',(8,-3,11),1250,(1,.82,.61))
area('Edge top-light',(-2,8,12),1100,(.72,.85,1))
cd=bpy.data.cameras.new('AB lock');cam=bpy.data.objects.new('AB lock',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def ext(objects):
 points=[ob.matrix_world@Vector(b) for ob in objects for b in ob.bound_box if ob.type=='MESH']
 return Vector([min(p[k] for p in points) for k in range(3)]),Vector([max(p[k] for p in points) for k in range(3)])
def view(objects):
 lo,hi=ext(objects);center=(lo+hi)/2;dims=hi-lo
 cam.location=center+Vector((.87,-1.7,1.23)).normalized()*max(dims.length,1)*2.9
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(dims.x,dims.y,dims.z,.2)*1.8
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def cliff(mat,dark):
 R=random.Random(20261016)
 columns,levels=11,21
 verts=[];faces=[];indices=[]
 def field(x,z):
  # strong vertical bands with low slanted lamination and broken ridge spurs.
  ridge=.30*math.sin(2.4*x+.32*z)+.21*math.sin(5.0*x-.27*z)
  ridge+=.13*math.sin(10.7*x+1.24*z)+.11*noise.noise_vector(Vector((x*.9,z*.42,1.1))).x
  return -.68+ridge
 front=[]
 for row in range(levels):
  z=6.55*row/(levels-1)
  current=[]
  for col in range(columns):
   x=-1.75+3.5*col/(columns-1)
   # More jagged at cliff crest, less at bottom foot.
   if 0<col<columns-1:x+=R.uniform(-.11,.11)
   if 0<row<levels-1:zj=z+R.uniform(-.12,.12)
   else:zj=z
   x+=.14*math.sin(zj*1.6+col*.4)
   y=field(x,zj)+R.uniform(-.09,.09)
   if col==0:y+=.2
   if col==columns-1:y+=.16
   current.append(len(verts));verts.append((x,y,zj))
  front.append(current)
 for row in range(levels-1):
  for col in range(columns-1):
   a=front[row][col];b=front[row][col+1];c=front[row+1][col];d=front[row+1][col+1]
   # Deliberate irregular diagonal triangulation that avoids a brick-like rectangle grid.
   if R.random()<.5:tris=[(a,b,c),(b,d,c)]
   else:tris=[(a,b,d),(a,d,c)]
   for f in tris:
    faces.append(f)
    indices.append(1 if R.random()<.11 else 0)
 # Deep back: long vertical cliff panel is one watertight connected shell.
 back=[]
 for row in range(levels):
  rr=[]
  for col in range(columns):
   x,y,z=verts[front[row][col]]
   rr.append(len(verts));verts.append((x,1.0,z))
  back.append(rr)
 for row in range(levels-1):
  for col in range(columns-1):
   faces.append((back[row][col],back[row+1][col],back[row+1][col+1],back[row][col+1]));indices.append(1)
 for row in range(levels-1):
  for col in (0,columns-1):
   faces.append((front[row][col],front[row+1][col],back[row+1][col],back[row][col]));indices.append(0)
 for col in range(columns-1):
  for row in (0,levels-1):
   faces.append((front[row][col],back[row][col],back[row][col+1],front[row][col+1]));indices.append(0)
 m=bpy.data.meshes.new('Valoria_Continuous_Strata_Solid');m.from_pydata(verts,[],faces);m.update()
 ob=bpy.data.objects.new('VALORIA_ONE_REUSABLE_ALPINE_CLIFF_FACE',m);bpy.context.collection.objects.link(ob)
 m.materials.append(mat);m.materials.append(dark)
 uv=m.uv_layers.new(name='UVMap')
 for poly,mi in zip(m.polygons,indices):
  poly.material_index=mi
  for loop_index in poly.loop_indices:
   p=m.vertices[m.loops[loop_index].vertex_index].co
   uv.data[loop_index].uv=(p.x*.28,p.z*.28)
 # Add NO grid-like buttress, masonry, or stacked cubes to natural geology.
 return ob
data={'asset':n,'target':8.0,'art_approved':False}
try:
 original=source/(n+'.glb');bpy.ops.import_scene.gltf(filepath=str(original))
 old=[o for o in bpy.data.objects if o not in rig and o.type=='MESH']
 if not old:raise RuntimeError('No donor')
 view(old);render(out/'before'/(n+'.png'))
 donor=[m for ob in old for m in ob.data.materials if m and m.use_nodes and 'Rock_cliff' in m.name]
 if not donor:donor=[m for ob in old for m in ob.data.materials if m and m.use_nodes]
 if not donor:raise RuntimeError('No actual loaded CC0 PBR material')
 mat=donor[0]
 dark=bpy.data.materials.new('natural_crack_dark_rock');dark.use_nodes=True
 dark.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.19,.21,.26,1)
 dark.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value=.93
 for ob in old:bpy.data.objects.remove(ob,do_unlink=True)
 current=cliff(mat,dark)
 render(out/'after'/(n+'.png'))
 bpy.ops.object.select_all(action='DESELECT');current.select_set(True)
 bpy.context.view_layer.objects.active=current
 outglb=out/'glb'/(n+'.glb')
 bpy.ops.export_scene.gltf(filepath=str(outglb),export_format='GLB',use_selection=True,export_apply=True)
 if outglb.stat().st_size<10000:raise RuntimeError('GLB export failed')
 data.update({'technical':'VALID_PENDING_ART',
              'triangles':len(current.data.polygons),'shape':'watertight irregular single connected rock surface',
              'source_sha256':sha(original),'candidate_sha256':sha(outglb),
              'source_run':38824169625})
 print('NATURAL_CLIFF_CONNECTED_MESH',len(current.data.polygons),flush=True)
except Exception as e:
 data.update({'technical':'FAILED','error':repr(e)})
 traceback.print_exc()
finally:
 (out/'reports'/'cliff_continuous_sculpt.json').write_text(json.dumps(data,indent=2))
if data['technical']!='VALID_PENDING_ART':raise RuntimeError('Geology V6 failed')
