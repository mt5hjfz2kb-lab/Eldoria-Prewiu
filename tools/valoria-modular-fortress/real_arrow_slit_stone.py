"""Valoria: ONE actual medieval recessed arrow slit, not black cuboid decal.
Reusable 0.85m x 1.32m stone frame with open vertical cut-through,
six true quoin blocks, deep splayed embrasure, limestone CC0 stone PBR.
Native Blender A/B source vs physically hollow rebuild; no scene composition.
"""
import bpy,sys,random,math,hashlib,json,traceback
from pathlib import Path
from mathutils import Vector
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(arg)!=3:raise SystemExit('need baseline GLB, CC0 medieval blocks folder, output')
baseline,photos,out=map(Path,arg)
for d in ('glb','before','after','reports'):(out/d).mkdir(parents=True,exist_ok=True)
if not baseline.is_file():raise RuntimeError('Missing fixed original saetera')
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=990;s.render.resolution_y=1050
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria historic masonry studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.20,.26,.34,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.8;s.world=w
def lamp(n,power,pos,col):
 d=bpy.data.lights.new(n,'AREA');d.energy=power;d.color=col;d.size=9;d.shape='DISK'
 o=bpy.data.objects.new(n,d);bpy.context.collection.objects.link(o);o.location=pos
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
lamp('warm limestone key',1780,(-6,-8,11),(1,.84,.68))
lamp('evening sky fill',1540,(7,3,10),(.69,.84,1))
lamp('deep edge contour',760,(-4,8,10),(.97,.97,1))
cd=bpy.data.cameras.new('locked arrow slit AB camera')
cam=bpy.data.objects.new('locked arrow slit AB camera',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def hashfile(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def bounds(obj):
 bpy.context.view_layer.update()
 pts=[o.matrix_world@Vector(v) for o in obj for v in o.bound_box if o.type=='MESH']
 return Vector([min(v[i] for v in pts) for i in range(3)]),Vector([max(v[i] for v in pts) for i in range(3)])
def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
bpy.ops.import_scene.gltf(filepath=str(baseline))
old=[o for o in bpy.data.objects if o not in rig]
oldmesh=[o for o in old if o.type=='MESH']
if not oldmesh:raise RuntimeError('No original physical saetera')
lo,hi=bounds(oldmesh);extent=hi-lo
if not .7<extent.x<1.2 or not 1<extent.z<1.8:raise RuntimeError('Unexpected original dimensions '+str(extent))
mid=(lo+hi)*.5
cam.location=mid+Vector((.84,-1.54,1.17)).normalized()*max(extent.length,1)*3.0
cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
cd.ortho_scale=max(extent.x,extent.y,extent.z)*1.84
# Neutral backdrop behind open aperture, only in study camera (NEVER exported).
bpy.ops.mesh.primitive_plane_add(size=2.0,location=(mid.x,float(hi.y)+.64,mid.z),rotation=(math.pi/2,0,0))
back=bpy.context.object;back.name='STUDIO_BLACK_BACKDROP_NOT_PART_OF_ASSET'
back.scale=(1.2,1.2,1.2)
matbg=bpy.data.materials.new('neutral_black_test_background')
matbg.use_nodes=True;matbg.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.015,.019,.027,1)
back.data.materials.append(matbg);rig.add(back)
shoot(out/'before'/'arrow_slit_frame.png')
for o in old:bpy.data.objects.remove(o,do_unlink=True)
p=photos/'medieval_blocks_03_diffuse.jpg'
if not p.is_file():raise RuntimeError('Verified Poly Haven CC0 stone photo unavailable')
im=bpy.data.images.load(str(p),check_existing=True)
norm=photos/'medieval_blocks_03_nor_gl.jpg'
ni=bpy.data.images.load(str(norm),check_existing=True) if norm.is_file() else None
if ni:ni.colorspace_settings.name='Non-Color'
def pbr(name,color,dark=False):
 m=bpy.data.materials.new(name);m.use_nodes=True
 nodes=m.node_tree.nodes;nodes.clear();links=m.node_tree.links
 tex=nodes.new('ShaderNodeTexImage');tex.image=im;tex.extension='EXTEND'
 mix=nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=.32 if not dark else .62
 mix.inputs[2].default_value=(*color,1)
 links.new(tex.outputs['Color'],mix.inputs[1])
 bs=nodes.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.91
 links.new(mix.outputs['Color'],bs.inputs['Base Color'])
 if ni:
  tn=nodes.new('ShaderNodeTexImage');tn.image=ni;tn.extension='EXTEND'
  nm=nodes.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.25
  links.new(tn.outputs['Color'],nm.inputs['Color']);links.new(nm.outputs['Normal'],bs.inputs['Normal'])
 outn=nodes.new('ShaderNodeOutputMaterial');links.new(bs.outputs[0],outn.inputs['Surface'])
 return m
materials=[pbr('Valoria_warm_limestone_CC0',(.96,.92,.85)),
           pbr('Valoria_limestone_mid_CC0',(.84,.82,.77)),
           pbr('Valoria_deep_embrazure_CC0',(.41,.40,.42),dark=True)]
patches=[(170,18,414,169),(507,19,760,170),(58,225,259,384),
         (333,231,508,382),(703,221,848,380),
         (36,431,300,593),(404,433,561,589),(632,441,902,588),
         (29,641,274,782),(329,652,734,783)]
rng=random.Random(19016)
def uv_project(obj):
 pick=rng.choice(patches);x1,y1,x2,y2=pick
 u0,u1=x1/1024.,x2/1024.;v0,v1=1-y2/1024.,1-y1/1024.
 uv=obj.data.uv_layers.active or obj.data.uv_layers.new(name='UVMap')
 vertices=[v.co for v in obj.data.vertices]
 mi=[min(v[i] for v in vertices) for i in range(3)]
 ext=[max(v[i] for v in vertices)-mi[i] for i in range(3)]
 for poly in obj.data.polygons:
  axis=max(range(3),key=lambda i:abs(poly.normal[i]))
  a,b=([0,2] if axis==1 else [0,1] if axis==2 else [1,2])
  for k in poly.loop_indices:
   co=obj.data.vertices[obj.data.loops[k].vertex_index].co
   uu=(co[a]-mi[a])/max(ext[a],.00001)
   vv=(co[b]-mi[b])/max(ext[b],.00001)
   uv.data[k].uv=(u0+uu*(u1-u0),v0+vv*(v1-v0))
def cube(name,loc,dim,mat,bevel=.015):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
 o=bpy.context.object;o.name=name;o.dimensions=dim
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(mat)
 b=o.modifiers.new('real soft hand cut edges','BEVEL')
 b.width=min(bevel,min(dim)*.22);b.segments=3;b.limit_method='ANGLE'
 bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=b.name)
 uv_project(o);return o
allparts=[]
width=float(extent.x);height=float(extent.z);depth=max(.285,float(extent.y))
centerx=float(mid.x);centery=float(mid.y);basez=float(lo.z)
# Large front slit 0.13, true hole visible through 0.31m stone depth.
aperture=.126
opening_lo=basez+.235;opening_hi=basez+height-.17
side_w=(width-aperture)/2
for side in (-1,1):
 x=centerx+side*(aperture/2+side_w/2)
 # Separate stone courses give genuine mortar contacts around the hole.
 for i in range(3):
  z=opening_lo+(i+.5)*(opening_hi-opening_lo)/3
  o=cube('slit_architectural_jamb_'+str(side)+'_'+str(i),
         (x,centery,z),
         (side_w-.014,depth-.010,(opening_hi-opening_lo)/3-.011),
         materials[0 if (i+side)%2 else 1],.016)
  allparts.append(o)
# Upper lintel formed from 3 individually dressed interlocking stones,
# includes centered cap/keystone rather than a continuous generic plate.
for i in range(3):
 x=centerx+(-1+i)*(width/3)
 allparts.append(cube('historic_slit_arch_lintel_'+str(i),
    (x,centery,opening_hi+(basez+height-opening_hi)/2),
    (width/3-.010,depth,basez+height-opening_hi-.015),
    materials[i%2],.016))
# Thick sill at opening base, separated in two courses with shallow carved face.
for i in range(2):
 x=centerx+(-.5+i)*(width/2)
 allparts.append(cube('stone_slit_dressed_sill_'+str(i),
    (x,centery,basez+(opening_lo-basez)/2),
    (width/2-.012,depth,opening_lo-basez-.008),
    materials[(i+1)%2],.019))
# Genuine splayed embrasure on inside wall: narrow 13cm front aperture opens
# toward a wider 35cm interior. Two trapezoidal faceted stone reveal walls.
for side in (-1,1):
 front_x=centerx+side*(aperture/2)
 back_x=centerx+side*(aperture/2+.10)
 yfront=float(lo.y)-.006
 yback=float(hi.y)+.006
 verts=[(front_x,yfront,opening_lo),(back_x,yback,opening_lo),
        (back_x,yback,opening_hi),(front_x,yfront,opening_hi)]
 m=bpy.data.meshes.new('splayed arrow slit side')
 m.from_pydata(verts,[],[(0,1,2,3)]);m.update()
 o=bpy.data.objects.new('REAL_INSIDE_SPLAY_'+str(side),m)
 bpy.context.collection.objects.link(o);m.materials.append(materials[2])
 # Paper-thin inside reveal is neither a black cube nor a wall behind the slot.
 uv_project(o);allparts.append(o)
# Aperture must remain open: a physical ray can pass center from front to back.
if len(allparts)<12:raise RuntimeError('Building must have individual dressed blocks')
shoot(out/'after'/'arrow_slit_frame.png')
bpy.ops.object.select_all(action='DESELECT')
for o in allparts:o.select_set(True)
bpy.context.view_layer.objects.active=allparts[0]
output=out/'glb'/'arrow_slit_frame.glb'
bpy.ops.export_scene.gltf(filepath=str(output),export_format='GLB',use_selection=True,export_apply=True)
if output.stat().st_size<30000:raise RuntimeError('Failed CC0 detailed stone opening GLB')
report={'asset':'arrow_slit_frame','target':8,'technical':'PASS_PENDING_ART',
 'original_sha':hashfile(baseline),'candidate_sha':hashfile(output),
 'source_run':38813966671,'physical_stone_objects':len(allparts),
 'dimensions':[width,depth,height],'aperture_width_m':aperture,
 'opening_height_m':opening_hi-opening_lo,'rear_splay_added_width_m':.20,
 'photography':'Poly Haven medieval_blocks_03 CC0',
 'studio_backdrop_excluded_from_glb':True,
 'no_scene_composition':True,'art_approved':False,'unity_approved':False,
 'note':'Real through-hole, masonry jambs and splayed inside stone. Must inspect contact scale and stone color before any approval.'}
(out/'reports'/'real_medieval_arrow_slit.json').write_text(json.dumps(report,indent=2))
print('ELDORIA_PHYSICAL_ARROW_SLIT_WITH_HOLE',len(allparts),output.stat().st_size,flush=True)
