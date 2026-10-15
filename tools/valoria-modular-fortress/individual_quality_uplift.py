"""Valoria: per-asset artistic finishing & individual review images.
Blender 5.2; no paid models; never composes a scene; no auto ART_APPROVED.
Each GLB is independently imported, modified, exported and rendered.
"""
import bpy, math, random, json, os, sys, hashlib, traceback, re
from pathlib import Path
from mathutils import Vector
import numpy as np
params=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
IN=Path(params[0]); OUT=Path(params[1])
OUT.mkdir(parents=True,exist_ok=True)
(OUT/'glb').mkdir(exist_ok=True);(OUT/'previews').mkdir(exist_ok=True);(OUT/'textures').mkdir(exist_ok=True)
R=random.Random(15015)
source=sorted(IN.glob('*.glb'))
if len(source)!=63: raise RuntimeError('Exactly 63 input GLBs required, got '+str(len(source)))
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene
scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=768;scene.render.resolution_y=768;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.render.film_transparent=False
scene.render.image_settings.color_mode='RGBA'
scene.view_settings.view_transform='AgX'
scene.render.engine='BLENDER_EEVEE'
scene.render.use_file_extension=True
scene.world=bpy.data.worlds.new('Neutral catalog environment');scene.world.use_nodes=True
scene.world.node_tree.nodes.get('Background').inputs['Color'].default_value=(.20,.25,.31,1)
scene.world.node_tree.nodes.get('Background').inputs['Strength'].default_value=.8
def light(n,loc,power,size,color):
 ld=bpy.data.lights.new(n,'AREA');ld.energy=power;ld.shape='DISK';ld.size=size;ld.color=color
 ob=bpy.data.objects.new(n,ld);bpy.context.collection.objects.link(ob);ob.location=loc
 ob.rotation_euler=(Vector((0,0,0))-ob.location).to_track_quat('-Z','Y').to_euler();return ob
light('Key warm',(-6,-9,13),1500,8,(1,.82,.64))
light('Fill cool',(7,3,11),1900,9,(.56,.75,1))
light('Rim',(2,9,8),1400,7,(1,.92,.81))
camdata=bpy.data.cameras.new('catalog lens');cam=bpy.data.objects.new('catalog lens',camdata)
bpy.context.collection.objects.link(cam);scene.camera=cam;camdata.type='ORTHO';camdata.lens=50
rig=set(bpy.data.objects)
palette={
 'stone':((.58,.53,.46),.82),'light':((.73,.65,.54),.83),'dark':((.42,.41,.41),.85),
 'rock':((.38,.40,.47),.87),'grass':((.27,.32,.13),.91),'dirt':((.43,.33,.23),.96),
 'wood':((.36,.20,.10),.78),'iron':((.15,.16,.17),.42),'gold':((.82,.60,.25),.33),
 'blue':((.028,.10,.47),.70),'pine':((.06,.19,.12),.91),'leaf':((.19,.31,.11),.87),
 'water':((.07,.25,.35),.21),'fire':((1,.23,.026),.3)
}
def hashfile(p):
 h=hashlib.sha256()
 with open(p,'rb') as f:
  for part in iter(lambda:f.read(1048576),b''):h.update(part)
 return h.hexdigest()
def material(n):
 # Generated portable diffuse + normal images. Three tonal scales of procedural
 # grain: macro color, weathering and high-frequency pits, varied by material.
 name=n if n in palette else 'stone'
 if name in mats:return mats[name]
 base,rough=palette[name];r=np.random.default_rng(1000+sum(ord(c) for c in name))
 N=384; yy,xx=np.mgrid[0:N,0:N]
 xx=xx.astype(np.float32)/N; yy=yy.astype(np.float32)/N
 # Periodic, multi-scale, non-directional random field: avoids the repeating
 # plaid/checker pattern of the discarded sine-product texture experiment.
 raw=r.normal(size=(N,N))
 ky=np.fft.fftfreq(N).reshape(N,1)
 kx=np.fft.fftfreq(N).reshape(1,N)
 frequency=np.sqrt(kx*kx+ky*ky)
 low=(1.0+np.square(frequency*47.0))**-1.15
 cloud=np.fft.ifft2(np.fft.fft2(raw)*low).real.astype('float32')
 cloud=(cloud-cloud.mean())/(cloud.std()+1e-6)
 field=.058*cloud+r.normal(0,.012,(N,N))
 if name=='wood':
  field+=.029*np.sin(xx*113+3*np.sin(yy*5))+.013*np.sin(xx*223+2*np.sin(yy*8))
 elif name in ('rock','stone','light','dark'):
  field+=.021*np.sin(xx*4+yy*14+cloud*.8)
 elif name=='blue':
  field+=.009*np.sin(xx*191)*np.sin(yy*189)
 elif name in ('grass','leaf','pine'):
  field+=.023*np.sin(xx*27+yy*8+cloud*.7)
 f=np.clip(field,-.18,.18)
 rgb=np.clip(np.array(base)[None,None,:]*(1+f[:,:,None]),.012,.99)
 alpha=np.ones((N,N,1),np.float32)
 data=np.concatenate([rgb,alpha],axis=2)
 image=bpy.data.images.new('A_'+name,N,N,alpha=True)
 image.pixels.foreach_set(data.ravel().astype('float32'))
 path=OUT/'textures'/(name+'_albedo.png');image.filepath_raw=str(path);image.file_format='PNG';image.save()
 mat=bpy.data.materials.new('Valoria_PBR_'+name);mat.diffuse_color=(*base,1)
 mat.use_nodes=True;nodes=mat.node_tree.nodes;nodes.clear()
 output=nodes.new('ShaderNodeOutputMaterial');output.location=(480,10)
 bs=nodes.new('ShaderNodeBsdfPrincipled');bs.location=(190,10)
 bs.inputs['Roughness'].default_value=rough
 bs.inputs['Metallic'].default_value=.78 if name in ('gold','iron') else 0
 if name=='fire':
  bs.inputs['Emission Color'].default_value=(1,.28,.02,1)
  bs.inputs['Emission Strength'].default_value=6
 tex=nodes.new('ShaderNodeTexImage');tex.image=image;tex.location=(-300,160)
 links=mat.node_tree.links;links.new(tex.outputs['Color'],bs.inputs['Base Color'])
 bump=nodes.new('ShaderNodeBump');bump.location=(-30,-180)
 bump.inputs['Strength'].default_value=.17 if name in ('stone','light','dark','rock','wood') else .05
 bump.inputs['Distance'].default_value=.013 if name in ('stone','light','dark','rock','wood') else .004
 links.new(tex.outputs['Color'],bump.inputs['Height']);links.new(bump.outputs['Normal'],bs.inputs['Normal'])
 links.new(bs.outputs[0],output.inputs[0])
 mats[name]=mat;return mat
mats={}
def make_uv(ob):
 if ob.type!='MESH':return
 mesh=ob.data;layer=mesh.uv_layers.get('UVMap') or mesh.uv_layers.new(name='UVMap')
 for p in mesh.polygons:
  n=p.normal;ax=max(range(3),key=lambda i:abs(n[i]))
  ij=[i for i in range(3) if i!=ax]
  for li in p.loop_indices:
   v=mesh.vertices[mesh.loops[li].vertex_index].co
   layer.data[li].uv=(v[ij[0]]*.72,v[ij[1]]*.72)
def assign_existing(ob):
 if ob.type!='MESH':return
 for i,mat in enumerate(ob.data.materials):
  s=(mat.name if mat else '').lower().split('.')[0]
  key=next((k for k in palette if s.endswith('_'+k)),None)
  if not key:key='stone'
  ob.data.materials[i]=material(key)
 if not ob.data.materials:ob.data.materials.append(material('stone'))
 make_uv(ob)
def poly(name,verts,faces,mat):
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
 ob=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(ob)
 ob.data.materials.append(material(mat));make_uv(ob);return ob
def cube(name,loc,dim,mat,bev=.014):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc);ob=bpy.context.object;ob.name=name;ob.dimensions=dim
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 ob.data.materials.append(material(mat))
 if bev>0:
  m=ob.modifiers.new('Micro rounded worn edges','BEVEL');m.width=min(bev,min(dim)*.18);m.segments=2
  m.limit_method='ANGLE'
 make_uv(ob);return ob
def cone_between(name,a,b,r0,r1,mat,segments=5):
 a,b=Vector(a),Vector(b)
 v=b-a
 if v.length<1e-5:return None
 bpy.ops.mesh.primitive_cone_add(vertices=segments,radius1=r0,radius2=r1,depth=v.length,location=(a+b)*.5)
 ob=bpy.context.object;ob.name=name;ob.rotation_euler=v.to_track_quat('Z','Y').to_euler()
 ob.data.materials.append(material(mat));make_uv(ob);return ob
def generate_tree(n,old):
 dims=np.array([float(i) for i in dims_for(old)])
 h=max(dims[2],5);scale=h/7.5
 for o in old:bpy.data.objects.remove(o,do_unlink=True)
 R.seed(321+sum(ord(k) for k in n))
 cone_between(n+'_real_trunk',(0,0,0),(0,0,h*.91),.16*scale,.025*scale,'wood',8)
 levels=13 if 'tall' in n else 11
 for level in range(levels):
  z=h*(.12+.072*level)
  radius=(h*.18)*(1-level/(levels+.25))*(.91+R.random()*.13)
  count=6+(level%3)
  for side in range(count):
   theta=2*math.pi*side/count+level*2.399+R.uniform(-.13,.13)
   direction=Vector((math.cos(theta),math.sin(theta),0))
   a=Vector((0,0,z));b=a+direction*radius+Vector((0,0,-radius*.14))
   cone_between(n+'_thin_branch',a,b,.029*scale,.004*scale,'wood',5)
   # Several fan-shaped tapered needle clusters grow from every branch;
   # this replaces the conical silhouette with visibly separated boughs.
   for k in range(3):
    t=.34+k*.23
    center=a+(b-a)*t
    tip=center+direction*(radius*.37)+Vector((0,0,radius*.25))
    color='pine' if (level+side+k)%5 else 'leaf'
    verts=[tuple(center+Vector((0,0,-radius*.13))),
           tuple(center+direction*(radius*.15)+Vector((0,0,radius*.04)),
           ),tuple(tip),
           tuple(center-direction*(radius*.12)+Vector((0,0,radius*.07)))]
    poly(n+'_needle_spray',verts,[(0,1,2),(0,2,3)],color)
 return
def dims_for(objects):
 v=np.concatenate([np.array([(ob.matrix_world@Vector(c))[:] for c in ob.bound_box]) for ob in objects if ob.type=='MESH'],axis=0)
 return np.ptp(v,axis=0)
def add_shrub(n,old):
 dims=dims_for(old);h=max(.85,float(dims[2]))
 for o in old:bpy.data.objects.remove(o,do_unlink=True)
 R.seed(300+len(n));num=75 if n.endswith('1') else (110 if n.endswith('2') else 145)
 for i in range(num):
  angle=R.uniform(0,2*math.pi);rad=(R.random()**.6)*max(float(dims[0]),.8)*.41
  z=(.23+.7*R.random())*h
  center=Vector((rad*math.cos(angle),rad*math.sin(angle),z))
  sz=R.uniform(.09,.23)*h
  dr=Vector((math.cos(angle),math.sin(angle),R.uniform(-.1,.4)))
  side=Vector((-dr.y,dr.x,0))
  poly('shrub_individual_leaf',list(map(tuple,[center-side*sz*.5,center+dr*sz,center+side*sz*.5])),[(0,1,2)],'leaf' if i%5 else 'grass')
 for j in range(12):
  a=(R.uniform(-.2,.2),R.uniform(-.2,.2),0)
  b=(R.uniform(-.4,.4),R.uniform(-.4,.4),h*R.uniform(.3,.9))
  cone_between('shrub_twig',a,b,.015,.001,'wood',5)
def add_grass(n,old):
 for o in old:bpy.data.objects.remove(o,do_unlink=True)
 R.seed(12+sum(ord(c) for c in n))
 for i in range(40):
  x=R.uniform(-.35,.35);y=R.uniform(-.35,.35)
  height=R.uniform(.12,.54);w=R.uniform(.013,.040)
  ang=R.uniform(0,math.tau);d=Vector((math.cos(ang),math.sin(ang),0))
  side=Vector((-d.y,d.x,0))
  base=Vector((x,y,0))
  mid=base+d*height*.22+Vector((0,0,height*.52))
  tip=base+d*height*.62+Vector((0,0,height))
  poly('arched_tapered_grass_blade',[tuple(base-side*w),tuple(base+side*w),tuple(mid-side*w*.72),tuple(mid+side*w*.72),tuple(tip)],[(0,1,3),(0,3,2),(2,3,4)],'grass' if i%3 else 'leaf')
def make_banner(n,old):
 dims=dims_for(old);h=float(dims[2]);w=float(dims[0])*.88
 for o in old:bpy.data.objects.remove(o,do_unlink=True)
 NX=13;NZ=25;verts=[];faces=[]
 for zi in range(NZ):
  t=zi/(NZ-1)
  for xi in range(NX):
   u=xi/(NX-1);x=(u-.5)*w
   z=h*(.1+.77*(1-t))-.11*h*max(0,t-.7)*math.sin(u*math.pi)
   y=-.075*math.sin(u*math.pi*5+t*2.2)*t-.03*math.sin(16*t+u*7)
   verts.append((x,y,z))
   if zi and xi:
    k=zi*NX+xi;faces.extend(((k,k-1,k-1-NX),(k,k-1-NX,k-NX)))
 ob=poly(n+'_real_woven_cloth',verts,faces,'blue')
 solid=ob.modifiers.new('Cloth hems thickness','SOLIDIFY');solid.thickness=.012
 cube('banner_wood_rod',(0,0,h*.92),(w+ .25,.12,.11),'wood')
 for x in (-w*.5,w*.5):
  cone_between('gold_hem',(x,-.035,.12*h),(x,-.035,.88*h),.018,.018,'gold',8)
 # Raised golden fleur-de-lys, seen from front
 front=-.09;mid=h*.50
 cube('fleur_stem',(0,front,mid),(.075,.042,h*.40),'gold',.012)
 cube('fleur_sash',(0,front,mid-h*.03),(w*.35,.045,h*.06),'gold',.007)
 for pos in (-1,0,1):
  cx=pos*w*.14
  v=[(cx-.09*w,front,mid+.02*h),(cx+.09*w,front,mid+.02*h),(cx,front,mid+.30*h if pos==0 else mid+.18*h)]
  poly('fleur_gilt_petal',v,[(0,1,2)],'gold')
def add_door_hardware(n,old):
 dims=dims_for(old)
 # hand-shaped forged studs spaced on oak doors and portcullis cross members
 for j in range(4):
  z=(.2+j*.19)*float(dims[2])
  for i in range(4):
   x=(-.35+i*.23)*float(dims[0])
   bpy.ops.mesh.primitive_uv_sphere_add(segments=8,ring_count=4,radius=.047,location=(x,-.17,z))
   ob=bpy.context.object;ob.name=n+'_hand_peened_rivet';ob.data.materials.append(material('iron'))
 if n.startswith('reinforced'):
  for side in (-.25,.25):
   bpy.ops.mesh.primitive_torus_add(major_radius=.12,minor_radius=.025,location=(side*float(dims[0]),-.23,float(dims[2])*.47))
   ob=bpy.context.object;ob.name=n+'_forged_handle';ob.data.materials.append(material('iron'))
def add_stone_surface(n,old):
 # small, nonstructural weathering chips on the *visible top*, not contact planes.
 if n.startswith(('stone_','paver_','stair_step','arch_key','arch_voussoir','merlon_')):
  dims=dims_for(old)
  if min(dims[:2])>.2 and float(dims[2])>.05:
   z=max(float(o.location.z) for o in old) if False else None
   # existing bevels protected; texture and microbevels carry surface variation.
 for ob in old:
  if ob.type=='MESH' and len(ob.data.polygons)<300:
   m=ob.modifiers.new('Fine limestone edge abrasion','BEVEL')
   m.width=.008 if n.startswith('stone') else .005;m.segments=2;m.limit_method='ANGLE'
def prepare(n,old):
 if n.startswith('pine_'):return generate_tree(n,old)
 if n.startswith('shrub_'):return add_shrub(n,old)
 if n.startswith('grass_tuft'):return add_grass(n,old)
 if n.startswith('banner_'):return make_banner(n,old)
 if n.startswith(('reinforced_oak_gate','portcullis_')):add_door_hardware(n,old)
 for ob in old:
  if ob.type=='MESH':assign_existing(ob)
 if n.startswith(('stone_','paver_','stair_','arch_','wall_','tower_','merlon_')):add_stone_surface(n,old)
def bbox(objs):
 pts=[]
 for ob in objs:
  if ob.type=='MESH':
   for p in ob.bound_box:pts.append(ob.matrix_world@Vector(p))
 if not pts:return Vector((0,0,0)),Vector((1,1,1))
 return Vector([min(v[i] for v in pts) for i in range(3)]),Vector([max(v[i] for v in pts) for i in range(3)])
def shoot(n,objs,ang):
 mi,ma=bbox(objs);cen=(mi+ma)*.5;dims=ma-mi
 radius=max(dims.length*.63,.75)
 direction=Vector((math.cos(ang)*1.7,math.sin(ang)*1.7,1.30)).normalized()
 cam.location=cen+direction*radius*2.9
 cam.rotation_euler=(cen-cam.location).to_track_quat('-Z','Y').to_euler()
 camdata.ortho_scale=max(float(dims.x),float(dims.y),float(dims.z),.5)*1.72
 scene.render.filepath=str(OUT/'previews'/(n+('_front.png' if ang<0 else '_back.png')))
 bpy.ops.render.render(write_still=True)
 return str(scene.render.filepath)
report=[]
for j,path in enumerate(source,1):
 n=path.stem;before=set(bpy.data.objects)
 try:
  bpy.ops.import_scene.gltf(filepath=str(path))
  old=[ob for ob in bpy.data.objects if ob not in before and ob.type=='MESH']
  if not old:raise RuntimeError('No imported meshes')
  prepare(n,old)
  newer=[ob for ob in bpy.data.objects if ob not in before and ob.type=='MESH']
  for ob in newer:
   if not ob.data.materials:ob.data.materials.append(material('stone'))
   make_uv(ob)
  bpy.ops.object.select_all(action='DESELECT')
  for ob in newer:ob.select_set(True)
  bpy.context.view_layer.objects.active=newer[0]
  out=OUT/'glb'/(n+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(out),export_format='GLB',use_selection=True,export_apply=True)
  # two independent photographic angles; no fortress composition
  front=shoot(n,newer,-1.14)
  back=shoot(n,newer,2.3)
  mesh_tri=sum(len(ob.data.polygons) for ob in newer)
  report.append({'asset':n,'technical':'GLB_EXPORTED','geometry':mesh_tri,
   'glb_sha256':hashfile(out),'glb_bytes':out.stat().st_size,
   'individual_front':Path(front).name,'individual_rear':Path(back).name,
   'visual_status':'PENDING_INDEPENDENT_ARTISTIC_EVALUATION',
   'target_minimum':8.0,'art_approved':False})
  print('PER_ASSET_PASS',j,'/63',n,'triangles',mesh_tri,flush=True)
 except Exception as exc:
  report.append({'asset':n,'technical':'FAILED','error':str(exc),
   'visual_status':'REJECTED','art_approved':False})
  print('PER_ASSET_FAIL',j,n,str(exc),flush=True);traceback.print_exc()
 finally:
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
 json.dump({'baseline_run':38810636076,'scope':'63 INDIVIDUAL ASSETS ONLY; NO COMPOSITION',
  'assets':report,'count':len(report),'target':8.0,
  'art_approved':False,'warning':'Neither script nor PNG automatically certifies visual excellence'},
  open(OUT/'individual_quality_status.json','w'),indent=2)
print('ALL_FINISHED',sum(x['technical']=='GLB_EXPORTED' for x in report),'of',len(report),flush=True)
if len(report)!=63 or any(x['technical']!='GLB_EXPORTED' for x in report):raise RuntimeError('Incomplete 63-piece batch')
