"""3 individual carpenter's oak components with REAL scanned CC0 cracked wood photo,
normal map and real lightly worn arrises and dark woody surface fissures.
No assembly, no Unity, no invented score. Constant Blender A/B.
"""
import bpy,sys,math,hashlib,json,random,traceback
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=3:raise SystemExit('original_three_wood_GLBS, CC0 rough_wood photos, output')
source,pbr,out=map(Path,args)
for d in ('before','after','glb','reports'):(out/d).mkdir(parents=True,exist_ok=True)
names=['timber_beam_1','timber_beam_2','timber_brace_X']
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1100;s.render.resolution_y=1100
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria high fidelity stand-alone oak studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.24,.31,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.81;s.world=w
def light(n,power,pos,color):
 ld=bpy.data.lights.new(n,'AREA');ld.energy=power;ld.color=color;ld.shape='DISK';ld.size=8
 ob=bpy.data.objects.new(n,ld);bpy.context.collection.objects.link(ob)
 ob.location=pos;ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
light('warm raking woodgrain',2000,(-6,-8,11),(1,.84,.66))
light('blue evening skylight',1550,(8,3,11),(.72,.83,1))
light('wood silhouette rim',850,(-2,9,10),(1,.98,.91))
cd=bpy.data.cameras.new('equal scale A-B individual timbers')
cam=bpy.data.objects.new('equal scale A-B individual timbers',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def box(obs):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in obs if o.type=='MESH' for v in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def frame(obs):
 lo,hi=box(obs);center=(lo+hi)*.5;d=hi-lo
 cam.location=center+Vector((.89,-1.59,1.16)).normalized()*max(d.length,1)*3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(d.x,d.y,d.z,.2)*1.68
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
im=bpy.data.images.load(str(pbr/'rough_wood_diffuse.jpg'),check_existing=True)
nfile=pbr/'rough_wood_nor_gl.jpg'
if not nfile.is_file():raise RuntimeError('CC0 wood normal map missing')
ni=bpy.data.images.load(str(nfile),check_existing=True);ni.colorspace_settings.name='Non-Color'
mat=bpy.data.materials.new('VALORIA_rough_dark_weathered_pbr_timber')
mat.use_nodes=True;nodes=mat.node_tree.nodes;nodes.clear();links=mat.node_tree.links
diff=nodes.new('ShaderNodeTexImage');diff.image=im
mix=nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=.38
mix.inputs[2].default_value=(.47,.34,.245,1)
links.new(diff.outputs['Color'],mix.inputs[1])
bs=nodes.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.85
links.new(mix.outputs['Color'],bs.inputs['Base Color'])
norm=nodes.new('ShaderNodeTexImage');norm.image=ni
nmap=nodes.new('ShaderNodeNormalMap');nmap.inputs['Strength'].default_value=.37
links.new(norm.outputs['Color'],nmap.inputs['Color']);links.new(nmap.outputs['Normal'],bs.inputs['Normal'])
finish=nodes.new('ShaderNodeOutputMaterial');links.new(bs.outputs['BSDF'],finish.inputs['Surface'])
fissure=bpy.data.materials.new('Subtle_deep_oak_split_in_open_weathering')
fissure.use_nodes=True
fb=fissure.node_tree.nodes.get('Principled BSDF')
fb.inputs['Base Color'].default_value=(.095,.064,.044,1);fb.inputs['Roughness'].default_value=.95
def box_uv(o):
 # 3D original UVs may sample one tiny region uniformly, projection on each
 # face restores proper photographic grain across local beam length.
 layer=o.data.uv_layers.active or o.data.uv_layers.new(name='UVMap')
 for face in o.data.polygons:
  axis=max(range(3),key=lambda i:abs(face.normal[i]))
  a,b=([0,2] if axis==1 else ([0,1] if axis==2 else [1,2]))
  for i in face.loop_indices:
   v=o.data.vertices[o.data.loops[i].vertex_index].co
   layer.data[i].uv=(v[a]*.65,v[b]*.5)
def wood_split(body,i,rng):
 # long dark narrow natural cracks flush against ACTUAL forward wood face;
 # back-face y measured in local coords, child follows rotation of X braces.
 mesh=body.data
 V=[v.co for v in mesh.vertices]
 xmin=min(p.x for p in V);xmax=max(p.x for p in V)
 ymin=min(p.y for p in V);zmin=min(p.z for p in V);zmax=max(p.z for p in V)
 span=zmax-zmin;width=xmax-xmin
 if span<.18 or width<.075:return None
 x=xmin+width*(.22+.22*(i%3))
 z0=zmin+.10*span+rng.random()*.15*span;length=span*(.38+.16*rng.random())
 verts=[];faces=[]
 n=7
 for j in range(n+1):
  t=j/n;xx=x+math.sin(t*10+i*2.2)*width*.032
  zz=z0+length*t
  thick=.001+min(width*.038,.012)*math.sin(t*math.pi)
  verts.extend([(xx-thick,ymin-.0006,zz),(xx+thick,ymin-.0006,zz)])
  if j<n:
   k=j*2;faces.append((k,k+1,k+3,k+2))
 crack=bpy.data.meshes.new('true_dark_split_curved_paper_thin')
 crack.from_pydata(verts,[],faces);crack.update()
 ob=bpy.data.objects.new('flush_lifetime_oak_fissure_'+str(i),crack)
 bpy.context.collection.objects.link(ob);ob.parent=body
 crack.materials.append(fissure)
 return ob
rows=[]
for idx,name in enumerate(names):
 pre=set(bpy.data.objects);r={'asset':name,'artist_target':6,'art_approved':False}
 try:
  source_glb=source/(name+'.glb')
  bpy.ops.import_scene.gltf(filepath=str(source_glb))
  imports=[o for o in bpy.data.objects if o not in pre]
  mesh=[o for o in imports if o.type=='MESH']
  if not mesh:raise RuntimeError('No timber source')
  frame(mesh);shoot(out/'before'/(name+'.png'))
  changed=[]
  rng=random.Random(5910+idx)
  for o in mesh:
   lname=o.name.lower()
   # Preserve true pegs/inlay smaller dark stain; replace only structural wood
   # with CC0 real photographed rough aged lumber.
   if any(term in lname for term in ('peg','dowel','pin')):continue
   for j in range(len(o.data.materials)):o.data.materials[j]=mat
   if not o.data.materials:o.data.materials.append(mat)
   box_uv(o);changed.append(o.name)
   if ('beam' in lname or 'brace' in lname) and len(o.data.polygons)>6:
    bevel=o.modifiers.new('hand_rounded_old_wood_arris','BEVEL')
    bevel.width=.014;bevel.segments=2;bevel.limit_method='ANGLE'
    bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=bevel.name)
  # Long cracks only on independent vertical wooden posts, not on short pegs
  # or the rotated X where orientation is harder to keep contact correct.
  new=[]
  if name!='timber_brace_X':
   structural=[o for o in mesh if 'beam' in o.name.lower()]
   for ob in structural[:1]:
    for i in range(3):
     fiss=wood_split(ob,i,rng)
     if fiss:new.append(fiss)
  if not changed:raise RuntimeError('No structural wood found')
  shoot(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in imports+new:ob.select_set(True)
  bpy.context.view_layer.objects.active=mesh[0]
  dst=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',use_selection=True,export_apply=True)
  if dst.stat().st_size<60000:raise RuntimeError('CC0 rough wood texture not packed')
  r.update({'technical':'PASS_PENDING_VISUAL','photo_source':'Poly Haven rough_wood CC0 1k diffuse and GL normal',
            'transformed_parts':changed,'real_wood_fissures':len(new),
            'old_sha':sha(source_glb),'candidate_sha':sha(dst),
            'note':'Textured true worn oak, physically chamfered arrises, optional natural flush splits.'})
  print('TRULY_WEATHERED_MEDIEVAL_WOOD',name,len(changed),len(new),flush=True)
 except Exception as e:
  r.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
 finally:
  rows.append(r)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  (out/'reports'/'three_pbr_worn_oak_modules.json').write_text(json.dumps({
   'source_run':38890199991,'no_scene':True,'items':rows},indent=2))
if len(rows)!=3 or any(x['technical']!='PASS_PENDING_VISUAL' for x in rows):
 raise RuntimeError('Three independent authentic timber finishes incomplete')
print('THREE_AGED_TIMBERS_NATIVE_BLENDER_COMPLETE',flush=True)
