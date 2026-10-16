"""Single river_foam_line hand-designed water-edge VFX:
3D scalloped crest ribbon + translucent irregular alpha bubbles, not three
rigid wires. Use same camera and SAME separate dark-blue water studio plane
for A/B; studio plane excluded from GLB. Blender 5.2, 0 EUR.
No full fortress composition, no ART_APPROVED.
"""
import bpy,sys,json,math,hashlib,random,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=2:raise RuntimeError('Require previous river foam GLB and output')
source,out=map(Path,args)
for folder in ('before','after','glb','atlas','reports'):(out/folder).mkdir(parents=True,exist_ok=True)
if not source.is_file():raise RuntimeError('Missing original physical foam comparison')
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1200;s.render.resolution_y=900
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria alpine river studio')
world.use_nodes=True;world.node_tree.nodes['Background'].inputs['Color'].default_value=(.12,.19,.27,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.85;s.world=world
def lamp(n,pos,power,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=col;d.shape='DISK';d.size=9
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('sky crest',(-6,-8,11),2050,(.77,.9,1))
lamp('sunset specular',(6,4,10),1550,(1,.85,.67))
camera_data=bpy.data.cameras.new('locked foam camera')
cam=bpy.data.objects.new('locked foam camera',camera_data)
bpy.context.collection.objects.link(cam);s.camera=cam;camera_data.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(obs):
 bpy.context.view_layer.update()
 pts=[ob.matrix_world@Vector(v) for ob in obs if ob.type=='MESH' for v in ob.bound_box]
 return Vector([min(c[i] for c in pts) for i in range(3)]),Vector([max(c[i] for c in pts) for i in range(3)])
def view(obs):
 lo,hi=bounds(obs);center=(lo+hi)*.5;d=hi-lo
 cam.location=center+Vector((.2,-1.35,1.3)).normalized()*max(d.length,1)*2.3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 camera_data.ortho_scale=max(d.x,d.y,d.z)*1.48
def render(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
bpy.ops.import_scene.gltf(filepath=str(source))
old=[o for o in bpy.data.objects if o not in rig]
mesh=[o for o in old if o.type=='MESH']
if not mesh:raise RuntimeError('Original foam line has no GLB mesh')
lo,hi=bounds(mesh);view(mesh)
W=float(hi.x-lo.x);centerx=(lo.x+hi.x)*.5
if W<1.5:raise RuntimeError('Not a full length repeatable river asset')
# Only a studio color support. Excluded from exported GLB.
bpy.ops.mesh.primitive_plane_add(size=2,location=(centerx,(lo.y+hi.y)*.5,(lo.z+hi.z)*.5-.07))
back=bpy.context.object;back.name='NONEXPORT_RIVER_STUDIO_BACKDROP'
back.scale=(W*.78,.75,1)
backdrop=bpy.data.materials.new('blue_water_studio_only');backdrop.use_nodes=True
pbr=backdrop.node_tree.nodes.get('Principled BSDF')
pbr.inputs['Base Color'].default_value=(.10,.255,.35,1)
pbr.inputs['Roughness'].default_value=.3
back.data.materials.append(backdrop)
rig.add(back)
render(out/'before'/'river_foam_line.png')
for obj in old:bpy.data.objects.remove(obj,do_unlink=True)
# High contrast froth holes/cloud pockets entirely controlled by single alpha.
height=320;width=1536
yy,xx=np.mgrid[0:height,0:width].astype(np.float32)
u=xx/(width-1);v=yy/(height-1)
rng=np.random.default_rng(62016)
noise=np.zeros((height,width),dtype=np.float32)
for freq,weight in [(3,.17),(7,.10),(17,.063),(42,.029)]:
 phase=rng.uniform(-math.pi,math.pi,3)
 noise+=weight*np.sin(u*freq*2*math.pi+phase[0])*np.cos(v*(freq*.61)*2*math.pi+phase[1])
crest_center=.49+.09*np.sin(2*math.pi*u*2.5)+.04*np.sin(2*math.pi*u*10.3)
dist=np.abs(v-crest_center)
frac=np.clip(1-dist/(.38+.04*np.sin(u*19.4)),0,1)
ragged=np.clip(frac*1.6-.22+noise*2.2,0,1)
sparsity=np.clip(.45+.23*np.sin(u*83+v*29)+.18*np.sin(u*213-v*97),0,1)
alpha=np.clip(ragged*(.66+.32*sparsity),0,.88)
# Carve tiny open-water holes within the foamy ribbon (not painted stripes).
for i in range(140):
 x=rng.uniform(.01,.99);y=rng.uniform(.08,.92)
 rx=rng.uniform(.002,.011);ry=rng.uniform(.012,.065)
 hole=((u-x)/rx)**2+((v-y)/ry)**2
 alpha*=1-.65*np.exp(-hole*1.9)
alpha=np.clip(alpha,0,.85)
rgba=np.empty((height,width,4),dtype=np.float32)
rgba[:,:,0]=.77+.14*sparsity
rgba[:,:,1]=.84+.11*sparsity
rgba[:,:,2]=.87+.09*sparsity
rgba[:,:,3]=alpha
texture=bpy.data.images.new('Valoria_photo_styled_whitewater_alpha',width,height,alpha=True)
texture.pixels.foreach_set(rgba.reshape(-1));texture.update()
atlas=out/'atlas'/'whitewater_froth_irregular_alpha.png'
texture.filepath_raw=str(atlas);texture.file_format='PNG';texture.save();texture.pack()
mat=bpy.data.materials.new('Valoria_water_edge_3d_translucent_froth')
mat.use_nodes=True;mat.surface_render_method='DITHERED'
nodes=mat.node_tree.nodes;nodes.clear();lk=mat.node_tree.links
tex=nodes.new('ShaderNodeTexImage');tex.image=texture
bs=nodes.new('ShaderNodeBsdfPrincipled')
bs.inputs['Roughness'].default_value=.73
lk.new(tex.outputs['Color'],bs.inputs['Base Color'])
lk.new(tex.outputs['Alpha'],bs.inputs['Alpha'])
outnode=nodes.new('ShaderNodeOutputMaterial')
lk.new(bs.outputs['BSDF'],outnode.inputs['Surface'])
# Irregular physically scalloped triangle mesh not 3 razor thin parallel lines.
points=[];faces=[];N=146
ycenter=float((lo.y+hi.y)*.5)
zcenter=float((lo.z+hi.z)*.5)
for i in range(N+1):
 u0=i/N
 x=float(lo.x)+W*u0
 centerY=ycenter+.035*math.sin(2*math.pi*u0*3.7)+.018*math.sin(2*math.pi*u0*15)
 thick=.22+.07*math.sin(2*math.pi*u0*6.7)+.023*math.sin(2*math.pi*u0*19)
 for side in (-1,1):
  y=centerY+side*max(.12,thick)
  points.append((x,y,zcenter+.018*math.sin(2*math.pi*u0*4.1)))
 if i<N:
  a=i*2;faces.append((a,a+1,a+3,a+2))
geom=bpy.data.meshes.new('proper_foamy_crest_surface')
geom.from_pydata(points,[],faces);geom.update()
ob=bpy.data.objects.new('physically_scalloped_froth_crest',geom)
bpy.context.collection.objects.link(ob);geom.materials.append(mat)
uv=geom.uv_layers.new(name='UVMap')
for p in geom.polygons:
 for li in p.loop_indices:
  vertex=geom.loops[li].vertex_index
  uv.data[li].uv=(vertex//2/N,float(vertex%2))
new=[ob]
rng2=random.Random(7619)
# Additional tiny independent white bubbles cling to foam and visually break
# an otherwise straight geometrical silhouette.
bubble=bpy.data.materials.new('river_microaerated_white_bubbles')
bubble.use_nodes=True
b=bubble.node_tree.nodes.get('Principled BSDF')
b.inputs['Base Color'].default_value=(.75,.86,.93,1)
b.inputs['Roughness'].default_value=.51
for i in range(112):
 x=float(lo.x)+rng2.random()*W
 t=(x-float(lo.x))/W
 y=ycenter+.035*math.sin(2*math.pi*t*3.7)+rng2.uniform(-.25,.25)
 r=rng2.uniform(.008,.026)
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=r,location=(x,y,zcenter+r*.45))
 m=bpy.context.object;m.name='bonded_foam_cell_'+str(i)
 m.scale=(rng2.uniform(1.,2.1),rng2.uniform(.4,1.2),.25)
 m.data.materials.append(bubble);new.append(m)
render(out/'after'/'river_foam_line.png')
bpy.ops.object.select_all(action='DESELECT')
for o in new:o.select_set(True)
bpy.context.view_layer.objects.active=new[0]
dest=out/'glb'/'river_foam_line.glb'
bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
if dest.stat().st_size<130000:raise RuntimeError('Foam alpha GLB lost')
report={'asset':'river_foam_line','status':'PASS_PENDING_VISUAL','source_sha':sha(source),
 'candidate_sha':sha(dest),'photo_like_alpha_sha':sha(atlas),'foam_bubbles':len(new)-1,
 'approx_line_length_m':W,'real_foam_strip_width_m':.44,
 'alpha_transparency':True,'studio_blue_water_not_exported':True,'art_approved':False,
 'no_scene_composition':True,'caveat':'Validate thin-water alpha sort in Unity URP later; do not score from studio water backing alone'}
(out/'reports'/'water_edge_photo_foam_v5.json').write_text(json.dumps(report,indent=2))
print('RIVER_FOAM_TRUE_AERATED_FROTH_GLTF',len(new),dest.stat().st_size,flush=True)
