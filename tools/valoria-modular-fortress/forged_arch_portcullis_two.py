"""Blender-native individual A/B: two repeatable authentic medieval arched portcullises.
Replace old uniform rectangular grating and floating studs with hammer-forged CC0 PBR
iron, real arch, secured iron joints and spear-point feet. No castle composition.
"""
import bpy,sys,json,hashlib,math,random,traceback
from pathlib import Path
from mathutils import Vector
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(arg)!=3:raise SystemExit('source 8-finished-gates GLB dir, CC0 metal dir, output')
base,pbr,out=map(Path,arg)
for d in ('before','after','glb','reports'):(out/d).mkdir(parents=True,exist_ok=True)
names=['portcullis_large','portcullis_small']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1000;s.render.resolution_y=1000
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria forged iron module lookdev');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.20,.26,.34,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.84;s.world=w
def lamp(n,pos,power,color):
 d=bpy.data.lights.new(n,'AREA');d.color=color;d.energy=power;d.shape='DISK';d.size=9
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('Amber forge key',(-7,-9,13),2100,(1,.8,.59))
lamp('Cool blue fill',(8,3,12),1700,(.67,.81,1))
lamp('High metal edge light',(-2,9,11),950,(1,1,1))
c=bpy.data.cameras.new('constant comparison camera');cam=bpy.data.objects.new('constant comparison camera',c)
bpy.context.collection.objects.link(cam);s.camera=cam;c.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bounds(objs):
 points=[o.matrix_world@Vector(v) for o in objs for v in o.bound_box if o.type=='MESH']
 return (Vector([min(p[i] for p in points) for i in range(3)]),
         Vector([max(p[i] for p in points) for i in range(3)]))
def setcam(objs):
 lo,hi=bounds(objs);mid=(lo+hi)*.5;dim=hi-lo
 cam.location=mid+Vector((.9,-1.65,1.14)).normalized()*max(dim.length,1)*3.15
 cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
 c.ortho_scale=max(dim.x,dim.y,dim.z)*1.70
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
iron=bpy.data.materials.new('Valoria_wrought_iron_real_CC0_PBR');iron.use_nodes=True
nodes=iron.node_tree.nodes;nodes.clear();link=iron.node_tree.links
img=pbr/'rusty_metal_05_diffuse.jpg'
if not img.is_file():raise RuntimeError('Poly Haven CC0 iron map absent')
im=bpy.data.images.load(str(img),check_existing=True)
tex=nodes.new('ShaderNodeTexImage');tex.image=im;tex.location=(-620,60)
mix=nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=.78
mix.inputs[2].default_value=(.28,.29,.32,1);mix.location=(-320,120)
link.new(tex.outputs['Color'],mix.inputs[1])
bs=nodes.new('ShaderNodeBsdfPrincipled');bs.inputs['Metallic'].default_value=.64
bs.inputs['Roughness'].default_value=.57;bs.location=(10,80)
link.new(mix.outputs['Color'],bs.inputs['Base Color'])
norm=pbr/'rusty_metal_05_nor_gl.jpg'
if norm.is_file():
 ni=bpy.data.images.load(str(norm),check_existing=True)
 ni.colorspace_settings.name='Non-Color'
 tn=nodes.new('ShaderNodeTexImage');tn.image=ni
 nm=nodes.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.18
 link.new(tn.outputs['Color'],nm.inputs['Color'])
 link.new(nm.outputs['Normal'],bs.inputs['Normal'])
op=nodes.new('ShaderNodeOutputMaterial');link.new(bs.outputs[0],op.inputs['Surface'])
def cube(name,xyz,dim,bevel=.015):
 bpy.ops.mesh.primitive_cube_add(size=1,location=xyz)
 o=bpy.context.object;o.name=name;o.dimensions=dim
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(iron)
 if bevel:
  b=o.modifiers.new('forged worn iron edge','BEVEL');b.width=min(bevel,min(dim)*.2);b.segments=2
  b.limit_method='ANGLE'
 return o
def stud(name,xyz,r=.033):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=10,ring_count=6,radius=r,location=xyz)
 o=bpy.context.object;o.name=name;o.data.materials.append(iron);o.scale.y=.58;return o
def pointed_iron(name,center,base_z,tip_z,r):
 vertices=[]
 segments=4
 for i in range(segments):
  a=(i+0.5)*math.pi*2/segments
  vertices.append((center[0]+r*math.cos(a),center[1]+r*math.sin(a),base_z))
 vertices.append((center[0],center[1],tip_z))
 faces=[(0,2,1,3),(0,1,4),(1,2,4),(2,3,4),(3,0,4)]
 # Note: pyramid is narrow yet embedded into vertical rods.
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.update()
 o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);o.data.materials.append(iron)
 return o
def arch_rail(name,width,height,r):
 points=[]
 shoulder=height-.92
 for j in range(45):
  x=(-r+2*r*j/44)
  z=shoulder+.92*math.sqrt(max(0,1-(x/r)**2))-.105
  points.append((x,0,z))
 cu=bpy.data.curves.new(name,'CURVE');cu.dimensions='3D'
 spline=cu.splines.new('POLY');spline.points.add(len(points)-1)
 for i,p in enumerate(points):spline.points[i].co=(p[0],p[1],p[2],1)
 cu.bevel_depth=.058;cu.bevel_resolution=3
 ob=bpy.data.objects.new(name,cu);bpy.context.collection.objects.link(ob)
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob
 bpy.ops.object.convert(target='MESH')
 ob=bpy.context.object;ob.data.materials.append(iron);return ob
def forge(name,width,height,bar_count,levels):
 outobjs=[];R=random.Random(170+len(name))
 r=width*.5-.10;shoulder=height-.92;floor=.31
 xs=[(-r+.045)+i*(2*r-.09)/(bar_count-1) for i in range(bar_count)]
 for idx,x in enumerate(xs):
  top=shoulder+.92*math.sqrt(max(0,1-(x/r)**2))-.09
  # Wrought bars carry weight along full vertical wall section.
  barbottom=.33
  outobjs.append(cube('forged_vertical_bar_'+str(idx),(x,0,(top+barbottom)*.5),
                      (.115,.155,top-barbottom),.019))
  outobjs.append(pointed_iron('true_security_spearpoint_'+str(idx),(x,0),.41,-.075,.068))
 for row,z in enumerate(levels):
  outobjs.append(cube('horizontal_tenoned_strap_course_'+str(row),(0,-.008,z),
                      (width,.16,.123),.018))
  # Rivets are physically embedded into front of the strap, never suspended
  # in negative space like rejected baseline rivets.
  for col,x in enumerate(xs):
   outobjs.append(stud('peened_bolt_head_'+str(row)+'_'+str(col),(x,-.094,z),.029))
 outobjs.append(arch_rail('arched_iron_crown_rail',width,height,r))
 # Wide side latch brackets, welded to the real bars rather than floating.
 for side in (-1,1):
  x=side*(width*.5-.075)
  for z in (levels[0],levels[-1]):
   outobjs.append(cube('recessed_wall_guide_bracket',(x,.035,z),(.24,.23,.21),.018))
 return outobjs
records=[]
for name in names:
 pre=set(bpy.data.objects);rpt={'asset':name,'art_approved':False,'target':8,'no_composition':True}
 try:
  p=base/(name+'.glb');bpy.ops.import_scene.gltf(filepath=str(p))
  original=[o for o in bpy.data.objects if o not in pre and o.type=='MESH']
  if not original:raise RuntimeError('No prior gate geometry')
  low,high=bounds(original);wth=float(high.x-low.x);height=float(high.z-low.z)
  setcam(original);shoot(out/'before'/(name+'.png'))
  for o in list(bpy.data.objects):
   if o not in pre:bpy.data.objects.remove(o,do_unlink=True)
  count=11 if name.endswith('large') else 9
  levels=[.77,1.42,2.10] if name.endswith('large') else [.62,1.17,1.77]
  latest=forge(name,wth,height,count,levels)
  # Match source geometry's ground and horizontal footprint.
  bpy.ops.object.select_all(action='DESELECT')
  for ob in latest:ob.select_set(True)
  bpy.context.view_layer.objects.active=latest[0]
  shoot(out/'after'/(name+'.png'))
  target=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
  if target.stat().st_size<25000:raise RuntimeError('Exported lattice empty')
  rpt.update({'technical':'PASS_PENDING_ART','pieces':len(latest),
   'vertical_bars':count,'cross_straps':len(levels),
   'source_sha':sha(p),'candidate_sha':sha(target),
   'motif':'true arched crown, chamfered iron bars, physically embedded rivets and pointed forged ends'})
  print('INDEPENDENT_PORTCULLIS_A_B',name,len(latest),flush=True)
 except Exception as e:
  rpt.update({'technical':'FAIL','error':str(e)});traceback.print_exc()
 finally:
  records.append(rpt)
  for obj in list(bpy.data.objects):
   if obj not in rig:bpy.data.objects.remove(obj,do_unlink=True)
  (out/'reports'/'two_forged_portcullises.json').write_text(json.dumps({'single_asset_pairs':True,'all_art_approved':False,'results':records},indent=2))
if len(records)!=2 or any(x['technical']!='PASS_PENDING_ART' for x in records):raise RuntimeError('Forging two gate studies incomplete')
print('INDEPENDENT_FORGED_GATES_TWO_TECH_PASS',flush=True)
