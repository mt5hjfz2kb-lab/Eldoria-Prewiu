"""Reference-first V5: single 4m hand-dressed stone wall made from real individual
ashlars, alternating physical courses, recessed masonry mortar, correct UV on ALL
faces. Reuses official Poly Haven CC0 medieval_blocks_03 stone interiors (not
image-photo mortar lines). Separate no-assembly native Blender camera A/B.
Never ART_APPROVED without manual + Unity visual evidence.
"""
import bpy,sys,json,random,math,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(args)!=3:raise RuntimeError('required prior-4m-glb, medieval-CC0-dir, output-dir')
original,source,out=map(Path,args)
for p in ('before','after','glb','reports','atlas'):(out/p).mkdir(parents=True,exist_ok=True)
if not original.is_file() or not (source/'medieval_blocks_03_diffuse.jpg').is_file():
 raise RuntimeError('Need true original wall GLB and official CC0 photo texture')
bpy.ops.wm.read_factory_settings(use_empty=True)
sc=bpy.context.scene;sc.render.engine='BLENDER_EEVEE'
sc.render.resolution_x=1120;sc.render.resolution_y=920
sc.render.resolution_percentage=100;sc.render.image_settings.file_format='PNG'
sc.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Stone wall neutral alpine studio');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.20,.25,.32,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.78
sc.world=world
def lamp(name,power,pos,color):
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.color=color;d.size=10;d.shape='DISK'
 o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o)
 o.location=pos;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('warm handstone key',1800,(-8,-9,13),(1,.85,.72))
lamp('evening sky fill',1600,(8,2,11),(.70,.82,1))
lamp('stone bevel rim',850,(-3,10,9),(.98,.95,.88))
cd=bpy.data.cameras.new('repeatable locked camera');cam=bpy.data.objects.new('repeatable locked camera',cd)
bpy.context.collection.objects.link(cam);sc.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def box_of(objects):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in objects for v in o.bound_box if o.type=='MESH']
 return Vector([min(v[i] for v in pts) for i in range(3)]),Vector([max(v[i] for v in pts) for i in range(3)])
def view(objects):
 lo,hi=box_of(objects);c=(lo+hi)*.5;d=hi-lo
 cam.location=c+Vector((.80,-1.56,1.11)).normalized()*max(d.length,1)*3
 cam.rotation_euler=(c-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(d.x,d.y,d.z,.2)*1.6
def shoot(p):sc.render.filepath=str(p);bpy.ops.render.render(write_still=True)
base_before=set(bpy.data.objects)
bpy.ops.import_scene.gltf(filepath=str(original))
olds=[o for o in bpy.data.objects if o not in base_before and o.type=='MESH']
if not olds:raise RuntimeError('Missing existing V4 4m wall GLB')
mn,mx=box_of(olds)
W=float(mx.x-mn.x);H=float(mx.z-mn.z);D=float(mx.y-mn.y)
if not 3.5<W<4.5 or not 2.1<H<3.2 or not .4<D<1:
 raise RuntimeError('Unexpected source modular dimensions '+str([W,H,D]))
view(olds)
shoot(out/'before'/'wall_panel_regular_4m.png')
for o in list(bpy.data.objects):
 if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
# Distinct stone faces are selected from INSIDE stone surfaces of real photo
# so a block face does not contain stray photographic mortar seams.
photo=bpy.data.images.load(str(source/'medieval_blocks_03_diffuse.jpg'),check_existing=False)
photo.scale(1024,1024)
array=np.empty(1024*1024*4,dtype=np.float32)
photo.pixels.foreach_get(array)
color=array.reshape(1024,1024,4)[:,:,:3]
intensity=color[:,:,0]*.30+color[:,:,1]*.59+color[:,:,2]*.11
tint=np.clip((intensity-.42)*.52+1.0,.78,1.19)
# Keep photographic grain and wide colour variation, gently warm the tone;
# absolutely NO 8-bit binary checker or uniformly gray procedural clay.
color[:]=np.clip(color*np.array([1.12,1.09,1.025],dtype=np.float32)[None,None,:]*tint[:,:,None],.04,.92)
atlas=bpy.data.images.new('Valoria_CC0_real_single_stone_surfaces',1024,1024,alpha=True)
atlas.pixels.foreach_set(array);atlas.update()
atlas.filepath_raw=str(out/'atlas'/'individual_dressed_stone_CC0.png');atlas.file_format='PNG';atlas.save()
bpy.data.images.remove(photo)
normal=bpy.data.images.load(str(source/'medieval_blocks_03_nor_gl.jpg'),check_existing=False)
normal.colorspace_settings.name='Non-Color'
def stone_material(name,mult):
 mat=bpy.data.materials.new(name);mat.use_nodes=True
 nd=mat.node_tree.nodes;nd.clear();links=mat.node_tree.links
 tex=nd.new('ShaderNodeTexImage');tex.image=atlas
 tex.interpolation='Linear';tex.extension='EXTEND'
 bs=nd.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.9
 mul=nd.new('ShaderNodeMixRGB');mul.blend_type='MULTIPLY';mul.inputs[0].default_value=.26
 mul.inputs[2].default_value=(*mult,1)
 links.new(tex.outputs['Color'],mul.inputs[1])
 links.new(mul.outputs['Color'],bs.inputs['Base Color'])
 tn=nd.new('ShaderNodeTexImage');tn.image=normal;tn.extension='EXTEND'
 nor=nd.new('ShaderNodeNormalMap');nor.inputs['Strength'].default_value=.25
 links.new(tn.outputs['Color'],nor.inputs['Color'])
 links.new(nor.outputs['Normal'],bs.inputs['Normal'])
 output=nd.new('ShaderNodeOutputMaterial')
 links.new(bs.outputs[0],output.inputs['Surface'])
 return mat
stone=[
 stone_material('Valoria_CC0_limestone_hand_chiselled_light',(1.0,.96,.91)),
 stone_material('Valoria_CC0_limestone_hand_chiselled_mid',(.92,.91,.89)),
 stone_material('Valoria_CC0_limestone_hand_chiselled_dark',(.86,.86,.85)),
]
mortar=bpy.data.materials.new('Valoria_recessed_mortar');mortar.use_nodes=True
bs=mortar.node_tree.nodes.get('Principled BSDF')
bs.inputs['Base Color'].default_value=(.34,.31,.285,1)
bs.inputs['Roughness'].default_value=.98
rng=random.Random(341200)
# UV patch interiors (top-left pixel rectangles): intentionally exclude real mortar.
# Source file y pixels top-down; Blender UV v increases bottom-up.
patches=[
 (160,25,420,170),(505,21,760,166),(55,225,265,382),
 (322,230,500,371),(701,225,840,370),(38,432,315,594),
 (400,433,563,590),(620,443,911,592),(28,636,285,786),
 (325,654,742,790),(215,854,495,985),(695,847,900,984)]
def stone_uv(o,patch):
 # Proper projection for front, top, END and back: each face cropped into a
 # valid stone INTERIOR rather than stretched across a 3D-extruded photo plate.
 x1,y1,x2,y2=patch
 u0,u1=(x1/1024.,x2/1024.);v0,v1=(1-y2/1024.,1-y1/1024.)
 uv=o.data.uv_layers.active or o.data.uv_layers.new(name='UVMap')
 verts=[v.co for v in o.data.vertices]
 mins=[min(v[i] for v in verts) for i in range(3)]
 spans=[max(v[i] for v in verts)-mins[i] for i in range(3)]
 for p in o.data.polygons:
  axis=max(range(3),key=lambda i:abs(p.normal[i]))
  a,b=([0,2] if axis==1 else ([0,1] if axis==2 else [1,2]))
  for i in p.loop_indices:
   co=o.data.vertices[o.data.loops[i].vertex_index].co
   uu=(co[a]-mins[a])/max(spans[a],.000001)
   vv=(co[b]-mins[b])/max(spans[b],.000001)
   uv.data[i].uv=(u0+uu*(u1-u0),v0+vv*(v1-v0))
def cube(name,position,dimensions,mat,bevel=0):
 bpy.ops.mesh.primitive_cube_add(size=1,location=position)
 o=bpy.context.object;o.name=name;o.dimensions=dimensions
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(mat)
 if bevel>0:
  mod=o.modifiers.new('hand-worn edges','BEVEL')
  mod.width=min(bevel,dimensions[2]*.08);mod.segments=3;mod.limit_method='ANGLE'
  # Apply BEFORE UV projection so newly created edge faces also get valid UVs.
  bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
 return o
# Slightly inset core; boundaries are true rounded stone modules, no stretched faces.
core=cube('real_mortar_core',(float((mn.x+mx.x)/2),float((mn.y+mx.y)/2),float((mn.z+mx.z)/2)),
           (W-.022,D-.14,H-.027),mortar,0)
height_margin=.017;rows=6;row_space=(H-2*height_margin)/rows
joint=.018;stones=[]
course_info=[]
for row in range(rows):
 z=float(mn.z)+height_margin+(row+.5)*row_space
 h=row_space-joint
 # Staggering is REAL geometry: vertical mortar contacts differ per course.
 cuts=[float(mn.x)]
 pos=float(mn.x)
 if row%2==0:
  desired=.56
 else:
  desired=.34
 while pos<float(mx.x)-.28:
  proposed=desired if len(cuts)==1 else rng.uniform(.56,.92)
  p=min(float(mx.x),pos+proposed)
  if float(mx.x)-p<.28:p=float(mx.x)
  if p-pos<.20:break
  cuts.append(p);pos=p
 if cuts[-1]<float(mx.x)-.01:cuts.append(float(mx.x))
 # Avoid truly flat end-caps: intentionally different side geometry edge and
 # separate stone front/back surfaces from the middle mortar.
 for k,(a,b) in enumerate(zip(cuts,cuts[1:])):
  centerx=(a+b)/2;ww=b-a
  for side in (-1,1):
   y=(float(mn.y) if side<0 else float(mx.y))
   dep=.225
   cy=y+side*(-1)*dep/2
   # For side<0 y=mn.y +dep/2; side>0 y=mx.y-dep/2.
   dim=(max(.12,ww-joint),dep,h)
   # Match exactly original footprint and avoid obvious overhangs.
   o=cube('individual_ashlar_r%02d_%02d_%s'%(row,k,'front' if side<0 else 'rear'),
          (centerx+ rng.uniform(-.002,.002),cy,z+rng.uniform(-.004,.004)),dim,
          stone[rng.choices([0,1,2],[5,4,1])[0]],rng.uniform(.012,.022))
   stone_uv(o,rng.choice(patches))
   stones.append(o)
 course_info.append({'row':row,'staggered_stones':len(cuts)-1,'widths':[round(v,3) for v in cuts]})
# Finish ends with dressed stones facing outward to hide continuous concrete
# backing; pattern follows the genuine physical courses.
for side in (-1,1):
 xx=float(mn.x) if side<0 else float(mx.x)
 for row in range(rows):
  z=float(mn.z)+height_margin+(row+.5)*row_space
  width=.17
  x=xx+(width/2 if side<0 else -width/2)
  o=cube('visible_end_quoin_%d_%d'%(side,row),
         (x,float((mn.y+mx.y)*.5),z),(width,D-.07,row_space-joint),
         stone[rng.randrange(3)],.013)
  stone_uv(o,rng.choice(patches));stones.append(o)
# Same physical dimensions as base. A/B images are SAME lens, lighting and GLB.
shoot(out/'after'/'wall_panel_regular_4m.png')
bpy.ops.object.select_all(action='DESELECT')
for o in stones+[core]:o.select_set(True)
bpy.context.view_layer.objects.active=stones[0]
glb=out/'glb'/'wall_panel_regular_4m.glb'
bpy.ops.export_scene.gltf(filepath=str(glb),export_format='GLB',use_selection=True,export_apply=True)
if glb.stat().st_size<150000:raise RuntimeError('Incomplete GLB')
uv_count=sum(len(o.data.uv_layers) for o in stones)
face_count=sum(len(o.data.polygons) for o in stones)
if uv_count!=len(stones):raise RuntimeError('Masonry with missing side UV faces')
if len(stones)<55 or face_count<1000:raise RuntimeError('Insufficient fully modeled stones')
report={
 'asset':'wall_panel_regular_4m','technical':'PASS_PENDING_ART',
 'source_v4_wall_run':38861363176,'license':'Poly Haven CC0 medieval_blocks_03',
 'source_sha256':sha(original),'candidate_sha256':sha(glb),
 'stone_count':len(stones),'stone_faces':face_count,
 'courses':course_info,'uv_stone_count':uv_count,'source_dimensions':[W,D,H],
 'side_uv':'separate per-face planar projections from real scanned stone interiors',
 'reference':'Warm hand-worked dressed ashlar, mortar only in PHYSICAL interstices',
 'art_approved':False,'unity_approved':False,'no_scene_composition':True,
 'manual_review':'Make sure real stone joint proportions and top/end faces are accurate; do not equate TECH_PASS with >=8/10.'
}
(out/'reports'/'stone_course_v5.json').write_text(json.dumps(report,indent=2))
print('ELDORIA_REAL_INDIVIDUAL_STONE_COURSE',len(stones),'stones',face_count,'faces',glb.stat().st_size,'bytes',flush=True)
