"""Eldoria SIX weakest assets: unique shape-specific real Blender rebuilds.
Copy-protect 63-source V3 and use CC0 Poly Haven photos, no scene compositions.
Paired native camera, SHA manifest, candidate GLB and PNG; never auto approve art.
"""
import bpy,sys,math,random,json,hashlib,traceback
from pathlib import Path
from mathutils import Vector,noise
A=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(A)!=3:raise SystemExit('original_v3_glb_dir, polyhaven_cc0_dir, output')
source,cc0,out=map(Path,A)
names=['mortar_joint_recess','river_foam_line','torch_wall','cobble_pebbles','stair_step','brazier_pedestal']
for d in ['before','after','glb','reports']:(out/d).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=950;scene.render.resolution_y=950
scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('Valoria isolated dusk studio');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.23,.30,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.75;scene.world=world
def light(name,where,power,color):
 ld=bpy.data.lights.new(name,'AREA');ld.energy=power;ld.color=color;ld.shape='DISK';ld.size=8
 ob=bpy.data.objects.new(name,ld);bpy.context.collection.objects.link(ob)
 ob.location=where;ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
light('Warm crafted stone key',(-7,-8,12),1450,(1,.85,.68))
light('Cool evening skylight',(7,4,12),1480,(.7,.82,1))
light('Soft rim',(2,9,9),850,(1,.98,.91))
camera_data=bpy.data.cameras.new('identical_before_after_camera')
camera=bpy.data.objects.new('identical_before_after_camera',camera_data)
bpy.context.collection.objects.link(camera);scene.camera=camera;camera_data.type='ORTHO'
rig=set(bpy.data.objects)
def sha(path):
 h=hashlib.sha256()
 with path.open('rb') as f:
  for block in iter(lambda:f.read(1024*1024),b''):h.update(block)
 return h.hexdigest()
def bbox(objects):
 bpy.context.view_layer.update()
 pts=[ob.matrix_world@Vector(v) for ob in objects if ob.type=='MESH' for v in ob.bound_box]
 if not pts:raise RuntimeError('No mesh bounds')
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def camera_on(meshes):
 lo,hi=bbox(meshes);center=(lo+hi)*.5;ext=hi-lo
 v=Vector((.85,-1.52,1.14)).normalized()
 camera.location=center+v*max(ext.length,1)*3.1
 camera.rotation_euler=(center-camera.location).to_track_quat('-Z','Y').to_euler()
 camera_data.ortho_scale=max(ext.x,ext.y,ext.z,.35)*1.76
def render(path):scene.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def pbr(name,filepath,color,rough=.87,metallic=0,normalfile=None,ns=.25):
 m=bpy.data.materials.new(name);m.use_nodes=True
 nodes=m.node_tree.nodes;nodes.clear();link=m.node_tree.links
 im=bpy.data.images.load(str(cc0/filepath),check_existing=True)
 tex=nodes.new('ShaderNodeTexImage');tex.image=im
 mix=nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY'
 mix.inputs[0].default_value=.55;mix.inputs[2].default_value=(*color,1)
 link.new(tex.outputs['Color'],mix.inputs[1])
 bs=nodes.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=rough
 bs.inputs['Metallic'].default_value=metallic
 link.new(mix.outputs['Color'],bs.inputs['Base Color'])
 if normalfile and (cc0/normalfile).exists():
  norm=bpy.data.images.load(str(cc0/normalfile),check_existing=True)
  norm.colorspace_settings.name='Non-Color'
  ntex=nodes.new('ShaderNodeTexImage');ntex.image=norm
  n=nodes.new('ShaderNodeNormalMap');n.inputs['Strength'].default_value=ns
  link.new(ntex.outputs['Color'],n.inputs['Color']);link.new(n.outputs['Normal'],bs.inputs['Normal'])
 output=nodes.new('ShaderNodeOutputMaterial');link.new(bs.outputs['BSDF'],output.inputs['Surface'])
 return m
stone=pbr('Valoria_CC0_aged_warm_limestone','rock_3_diffuse.jpg',(.63,.56,.47),normalfile='rock_3_nor_gl.jpg')
dark_stone=pbr('Valoria_CC0_shadow_fissured_stone','quarry_wall_diffuse.jpg',(.31,.31,.32),normalfile='quarry_wall_nor_gl.jpg')
iron=pbr('Valoria_CC0_forged_dark_iron','rusty_metal_05_diffuse.jpg',(.25,.28,.30),.58,.66,'rusty_metal_05_nor_gl.jpg')
wood=pbr('Valoria_CC0_dark_roasted_oak','oak_veneer_05_diffuse.jpg',(.45,.30,.20),.78,0,'oak_veneer_05_nor_gl.jpg')
def plain(name,color,rough=.75,metal=0,emit=0):
 m=bpy.data.materials.new(name);m.use_nodes=True
 bs=m.node_tree.nodes.get('Principled BSDF')
 bs.inputs['Base Color'].default_value=(*color,1);bs.inputs['Roughness'].default_value=rough
 bs.inputs['Metallic'].default_value=metal
 if emit:
  bs.inputs['Emission Color'].default_value=(*color,1)
  bs.inputs['Emission Strength'].default_value=emit
 return m
mortar=plain('Mortar_aged_granular_recess',(.28,.25,.24),.97)
foam=plain('Cool_white_translucent_river_foam',(.67,.81,.88),.66)
foam_shine=plain('Small_foam_white_crest',(.9,.94,.93),.5)
coals=plain('Warm_coals',(.33,.055,.014),.83,emit=1.1)
flame1=plain('Warm_amber_flame',(.95,.33,.045),.4,emit=2.8)
flame2=plain('Bright_core_flame',(1,.75,.17),.4,emit=3.2)
smoke=plain('Charcoal_burnt_tip',(.06,.058,.05),.95)
def cube(name,xyz,size,mat,bevel=0.02):
 bpy.ops.mesh.primitive_cube_add(size=1,location=xyz)
 ob=bpy.context.object;ob.name=name;ob.dimensions=size
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 ob.data.materials.append(mat)
 if bevel:
  m=ob.modifiers.new('Subtle_hewn_hand_edge','BEVEL');m.width=min(bevel,min(size)*.20)
  m.segments=2;m.limit_method='ANGLE'
 return ob
def sphere(name,xyz,radius,material,scale=None,segments=12,rings=8):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=segments,ring_count=rings,radius=radius,location=xyz)
 ob=bpy.context.object;ob.name=name;ob.data.materials.append(material)
 if scale:ob.scale=scale
 return ob
def tube(name,coords,radius,material,style='POLY'):
 cr=bpy.data.curves.new(name,'CURVE');cr.dimensions='3D';cr.resolution_u=8
 sp=cr.splines.new(style);sp.points.add(len(coords)-1)
 for i,v in enumerate(coords):sp.points[i].co=(float(v[0]),float(v[1]),float(v[2]),1)
 cr.bevel_depth=radius;cr.bevel_resolution=2
 ob=bpy.data.objects.new(name,cr);bpy.context.collection.objects.link(ob)
 ob.data.materials.append(material)
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob
 bpy.ops.object.convert(target='MESH')
 return bpy.context.object
def mortar_rebuild(lo,hi):
 # Standalone physical recessed mortar profile; U-shaped aged backing plus
 # fine darker broken center line. Not a wall composition.
 w=float(hi.x-lo.x);depth=max(.045,float(hi.y-lo.y));height=float(hi.z-lo.z)
 x=float((lo.x+hi.x)/2);y=float((lo.y+hi.y)/2);z=float((lo.z+hi.z)/2)
 out=[]
 out.append(cube('recessed_dark_mortar_core',(x,y,z),(w,depth*.80,max(.02,height*.78)),mortar,.005))
 for side in (-1,1):
  out.append(cube('stone_recess_rough_cut_lip_'+str(side),(x,y,z+side*height*.44),
                  (w,depth,.015),dark_stone,.008))
 rng=random.Random(168)
 for i in range(8):
  xx=float(lo.x)+.12+i*(w-.24)/7
  out.append(sphere('embedded_microgravel_in_mortar_'+str(i),(xx,y+rng.uniform(-.012,.012),z+rng.uniform(-.11,.11)*height),
      rng.uniform(.012,.025),stone,(1.5,.8,.45),8,5))
 return out
def foam_rebuild(lo,hi):
 rng=random.Random(44);out=[]
 width=float(hi.x-lo.x)
 x0=float(lo.x)
 # Soft broken water-fringe ribbons, neighboring streaks and tiny bubbles.
 for band in range(3):
  pts=[]
  for i in range(29):
   t=i/28;x=x0+width*t
   y=math.sin(t*13.5+band*.72)*.055+math.sin(t*24+1.3)*.021+(band-1)*.11
   z=.012+math.sin(t*7.2+band*.8)*.008
   pts.append((x,y,z))
  out.append(tube('sinuous_broken_whitewater_ribbon_'+str(band),pts,.012 if band==1 else .007,
                  foam_shine if band==1 else foam))
 for j in range(32):
  x=x0+rng.random()*width;y=rng.gauss(0,.11)
  out.append(sphere('tiny_water_bubble_'+str(j),(x,y,.02),rng.uniform(.009,.025),
                    foam_shine,(1.2,.45,1),8,4))
 return out
def flame_layers(center,r,height,prefix):
 x,y,z=center;out=[]
 for i in range(5):
  a=i*2.4;rad=(.10 if i<3 else .062);bx=x+math.cos(a)*.047;by=y+math.sin(a)*.04
  points=[]
  for j in range(9):
   t=j/8
   points.append((bx+math.sin(t*8+i)*rad*(1-t)*.36,by+math.cos(t*6+i)*rad*.19*(1-t),
                  z+t*height*(.78+.11*(i%3))))
  out.append(tube(prefix+'_flame_tongue_'+str(i),points,.027 if i<3 else .019,
                  flame1 if i<3 else flame2))
 for i in range(7):
  out.append(sphere(prefix+'_glowing_coal_'+str(i),(x+random.Random(318+i).uniform(-.08,.08),
               y+random.Random(727+i).uniform(-.05,.05),z-.075),
               .027,coals,(1.5,.9,.5),8,5))
 return out
def torch_rebuild(lo,hi):
 height=float(hi.z-lo.z);cx=float((lo.x+hi.x)/2);cy=float((lo.y+hi.y)/2)
 base=float(lo.z);out=[]
 out.append(cube('carved_oak_torch_grip',(cx,cy,base+.36),( .11,.11,.72),wood,.018))
 for z in (.14,.38,.66):
  out.append(tube('wrought_hammered_iron_band_'+str(z),
     [(cx+.079*math.cos(i*math.pi/10),cy+.079*math.sin(i*math.pi/10),base+z) for i in range(21)],.013,iron))
 # genuine wrought-wall mounting bracket, seat integrated to handle.
 out.append(cube('wall_anchor_plate',(cx,cy+.125,base+.23),(.25,.05,.32),iron,.018))
 out.append(tube('bent_iron_wall_arm',[(cx,cy+.12,base+.32),
   (cx,cy+.28,base+.39),(cx,cy+.30,base+.52)],.032,iron))
 for angle in range(4):
  px=cx+.12*math.cos(angle*math.pi/2);py=cy+.12*math.sin(angle*math.pi/2)
  out.append(tube('basket_forged_finger_'+str(angle),[(cx+.065*math.cos(angle*math.pi/2),
            cy+.065*math.sin(angle*math.pi/2),base+.66),(px,py,base+.82)],.023,iron))
 out+=flame_layers((cx,cy,base+.85),.12,.50,'wall_torch')
 return out
def pebbles_rebuild(lo,hi):
 rng=random.Random(691);out=[]
 for i in range(28):
  ang=rng.uniform(0,2*math.pi)
  rr=.12+math.sqrt(rng.random())*.60
  x=float((lo.x+hi.x)/2)+rr*math.cos(ang)
  y=float((lo.y+hi.y)/2)+rr*math.sin(ang)
  z=float(lo.z)+.034+rng.uniform(.005,.04)
  r=rng.uniform(.043,.10)
  ob=sphere('sculpted_riverstone_'+str(i),(x,y,z),r,
     stone if rng.random()<.6 else dark_stone,
     (rng.uniform(.9,1.55),rng.uniform(.7,1.12),rng.uniform(.45,.73)),12,8)
  # no cuboid: sculpted glacial gravel, physically different silhouettes
  for v in ob.data.vertices:
   delta=noise.noise(v.co*7+Vector((i*.4,.2,.3)))*.045
   v.co+=v.normal*delta
  ob.rotation_euler=(rng.uniform(-.25,.25),rng.uniform(-.3,.3),rng.uniform(0,3))
  out.append(ob)
 return out
def step_rebuild(lo,hi):
 w=float(hi.x-lo.x);d=float(hi.y-lo.y);h=float(hi.z-lo.z)
 cx=(lo.x+hi.x)*.5;cy=(lo.y+hi.y)*.5;cz=(lo.z+hi.z)*.5
 out=[cube('one_real_dressed_stone_step_with_riser',(cx,cy,cz),(w,d,h),stone,.024)]
 # Actual shaped projecting lip and deeper shadow line on riser; does not become staircase composition.
 out.append(cube('individual_limestone_nosing',(cx,float(lo.y)+.035,float(hi.z)-.029),
                 (w-.05,.085,.054),stone,.019))
 out.append(cube('recessed_below_step_shadow_line',(cx,float(lo.y)-.002,float(lo.z)+.058),
                 (w-.08,.008,.011),mortar,.003))
 return out
def brazier_rebuild(lo,hi):
 centerx=float((lo.x+hi.x)/2);centery=float((lo.y+hi.y)/2);base=float(lo.z);h=float(hi.z-lo.z)
 out=[]
 # masonry 6-course shaft with staggered chiseled stone, and strong cornice.
 for i in range(5):
  z=base+.10+i*.198
  out.append(cube('stone_brazier_ashlar_course_'+str(i),
                 (centerx,centery,z),(.62-.008*(i%3),.57,.188),stone if i%3 else dark_stone,.02))
 out.append(cube('blacksmith_forged_brazier_rim',(centerx,centery,base+1.06),(.79,.78,.12),iron,.045))
 out.append(cube('metal_inner_burning_basin',(centerx,centery,base+1.125),(.63,.62,.09),smoke,.025))
 # Real metal bowl rim with 4 handles supporting flame.
 for side in range(4):
  theta=side*math.pi/2
  px=centerx+math.cos(theta)*.37
  py=centery+math.sin(theta)*.37
  out.append(tube('forged_bowl_edge_'+str(side),
      [(centerx+.36*math.cos(theta+t*.085),centery+.35*math.sin(theta+t*.085),base+1.16) for t in range(11)],.024,iron))
 out+=flame_layers((centerx,centery,base+1.18),.12,.55,'brazier')
 return out
builders={'mortar_joint_recess':mortar_rebuild,'river_foam_line':foam_rebuild,
 'torch_wall':torch_rebuild,'cobble_pebbles':pebbles_rebuild,
 'stair_step':step_rebuild,'brazier_pedestal':brazier_rebuild}
results=[]
for i,name in enumerate(names,1):
 before=set(bpy.data.objects)
 row={'asset':name,'baseline_run':38813966671,'target_artist':6,
      'art_approved':False,'unity_approved':False,'no_scene_assembly':True}
 try:
  path=source/(name+'.glb')
  if not path.is_file():raise RuntimeError('Original source missing')
  bpy.ops.import_scene.gltf(filepath=str(path))
  imported=[o for o in bpy.data.objects if o not in before]
  meshes=[o for o in imported if o.type=='MESH']
  if not meshes:raise RuntimeError('Empty baseline mesh')
  lo,hi=bbox(meshes)
  camera_on(meshes)
  render(out/'before'/(name+'.png'))
  for o in imported:bpy.data.objects.remove(o,do_unlink=True)
  created=builders[name](lo,hi)
  if not created:raise RuntimeError('No real candidate geometry')
  render(out/'after'/(name+'.png'))
  bpy.ops.object.select_all(action='DESELECT')
  for ob in created:ob.select_set(True)
  bpy.context.view_layer.objects.active=created[0]
  dest=out/'glb'/(name+'.glb')
  bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
  if dest.stat().st_size<4500:raise RuntimeError('Suspiciously tiny GLB')
  row.update({'technical':'PASS_PENDING_VISUAL','primitive_count':len(created),
    'triangles':sum(len(o.data.polygons) for o in created if o.type=='MESH'),
    'original_sha':sha(path),'candidate_sha':sha(dest),
    'origin':'Original geometry rebuilt using licensed Poly Haven CC0 PBR and Blender authored modules'})
  print('SIX_UNDER_SIX_BLENDER_A_B',i,name,len(created),flush=True)
 except Exception as exc:
  row.update({'technical':'FAILED','error':repr(exc)});traceback.print_exc()
 finally:
  results.append(row)
  for ob in list(bpy.data.objects):
   if ob not in rig:bpy.data.objects.remove(ob,do_unlink=True)
  (out/'reports'/'six_weakest_individuals.json').write_text(json.dumps({
   'purpose':'Raise current worst 6 toward >=6 visual only','no_claim_8':True,
   'source_run':38813966671,'all_art_approved':False,'assets':results},indent=2))
if len(results)!=len(names) or any(r['technical']!='PASS_PENDING_VISUAL' for r in results):
 raise RuntimeError('Not all 6 weakest asset prototypes rendered')
print('SIX_INDIVIDUAL_QUALITY_CANDIDATES_RENDERED',flush=True)
