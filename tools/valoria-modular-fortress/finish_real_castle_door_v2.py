"""Eldoria Valoria: artist-finish real Poly Haven CC0 double castle door.
Single 3D asset only. Bakes subtle wood color and creates two flush, hinged ring pulls.
Door pivots, hierarchy, PBR atlas, UVs, normals preserved. Native Blender A/B.
"""
import bpy,sys,json,math,hashlib,traceback
import numpy as np
from pathlib import Path
from mathutils import Vector,Matrix
arg=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(arg)!=2:raise SystemExit('Expect current independent CC0 door GLB and result folder')
source,out=Path(arg[0]),Path(arg[1])
for d in ('before','after','glb','atlas','reports'):(out/d).mkdir(parents=True,exist_ok=True)
if not source.is_file():raise RuntimeError('Must have real donor CC0 gate GLB')
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
s.render.resolution_x=1050;s.render.resolution_y=1050
s.render.resolution_percentage=100;s.render.image_settings.file_format='PNG'
s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Valoria stand-alone hero door studio');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.18,.23,.30,1)
w.node_tree.nodes['Background'].inputs['Strength'].default_value=.82;s.world=w
def light(name,position,power,color):
 d=bpy.data.lights.new(name,'AREA');d.energy=power;d.shape='DISK';d.size=9;d.color=color
 o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o);o.location=position
 o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
light('golden raking wood grain',(-7,-9,13),2150,(1,.82,.63))
light('blue evening ambient',(8,4,12),1550,(.71,.82,1))
light('soft neutral metal rim',(-2,10,8),1000,(1,.95,.85))
camd=bpy.data.cameras.new('fixed same asset camera');cam=bpy.data.objects.new('fixed same asset camera',camd)
bpy.context.collection.objects.link(cam);s.camera=cam;camd.type='ORTHO'
rig=set(bpy.data.objects)
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def bbox(objs):
 bpy.context.view_layer.update()
 corners=[o.matrix_world@Vector(v) for o in objs for v in o.bound_box if o.type=='MESH']
 return Vector([min(p[i] for p in corners) for i in range(3)]),Vector([max(p[i] for p in corners) for i in range(3)])
def render(path):s.render.filepath=str(path);bpy.ops.render.render(write_still=True)
before=set(bpy.data.objects);bpy.ops.import_scene.gltf(filepath=str(source))
meshes=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
if len(meshes)!=3:raise RuntimeError('Expected original physical 3 pieces (frame, left, right); got '+str([o.name for o in meshes]))
lo,hi=bbox(meshes);center=(lo+hi)/2;size=hi-lo
cam.location=center+Vector((.85,-1.50,1.08)).normalized()*max(size.length,1)*3
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
camd.ortho_scale=max(size.x,size.y,size.z)*1.47
render(out/'before'/'reinforced_oak_gate.png')
original_materials={o.name:[m.name for m in o.data.materials if m] for o in meshes}
# Grade only the wood-brown pixels, leaving black iron hardware and pale light trim
# intact: avoids the uniformly bright synthetic resurface of rejected earlier tests.
images={}
for ob in meshes:
 for m in ob.data.materials:
  if m and m.use_nodes:
   for node in m.node_tree.nodes:
    if node.type=='TEX_IMAGE' and node.image and 'diff' in node.image.name.lower():
     images[node.image.name]=node.image
if not images:raise RuntimeError('Original CC0 albedo atlas absent')
images_changed=[]
for name,im in images.items():
 npx=im.size[0]*im.size[1]
 data=np.empty(npx*4,dtype=np.float32);im.pixels.foreach_get(data)
 rgba=data.reshape((npx,4));rgb=rgba[:,:3].copy()
 brown=(rgb[:,0]>rgb[:,1]*1.08)&(rgb[:,1]>rgb[:,2]*1.08)&(rgba[:,3]>.5)
 lum=rgb[:,0]*.30+rgb[:,1]*.60+rgb[:,2]*.10
 warm=np.stack([lum*1.16,lum*.91,lum*.69],axis=1)
 rgb[brown]=np.clip(rgb[brown]*.81+warm[brown]*.19,0,1)
 rgb[brown]=np.clip(rgb[brown]*1.13,0,1)
 rgba[:,:3]=rgb
 im.pixels.foreach_set(data);im.update()
 atlas=out/'atlas'/('warm_art_direction_'+name+'.png')
 im.filepath_raw=str(atlas);im.file_format='PNG';im.save()
 im.pack()
 images_changed.append({'image':name,'pixels_modified':int(brown.sum()),'total_pixels':npx,'artifact':str(atlas.name),'sha':sha(atlas)})
if not images_changed or max(x['pixels_modified'] for x in images_changed)<1000:
 raise RuntimeError('Wood color adjustment was a no-op')
# Forge deliberate, physically attached ring handles rather than floating studs.
black=bpy.data.materials.new('Valoria_forged_oxidized_iron');black.use_nodes=True
b=black.node_tree.nodes.get('Principled BSDF')
b.inputs['Base Color'].default_value=(.11,.105,.105,1)
b.inputs['Metallic'].default_value=.76;b.inputs['Roughness'].default_value=.53
def attach(o,parent):
 world=o.matrix_world.copy();o.parent=parent;o.matrix_world=world
def cube(name,position,dimensions,parent):
 bpy.ops.mesh.primitive_cube_add(size=1,location=position)
 o=bpy.context.object;o.name=name;o.dimensions=dimensions
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(black)
 bev=o.modifiers.new('small forged edge rounding','BEVEL');bev.width=.011;bev.segments=2
 attach(o,parent)
 return o
def stud(name,position,r,parent):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=10,ring_count=6,radius=r,location=position)
 o=bpy.context.object;o.name=name;o.data.materials.append(black);attach(o,parent);return o
def ring(name,position,parent):
 bpy.ops.mesh.primitive_torus_add(major_radius=.077,minor_radius=.016,location=position,rotation=(math.pi/2,0,0),
  major_segments=24,minor_segments=9)
 o=bpy.context.object;o.name=name;o.data.materials.append(black);attach(o,parent);return o
door_leaves=[o for o in meshes if ('left' in o.name.lower() or 'right' in o.name.lower())]
if len(door_leaves)!=2:raise RuntimeError('Must retain left and right door hinges')
additions=[]
for i,leaf in enumerate(door_leaves):
 bbox_min,bbox_max=bbox([leaf])
 x=(bbox_min.x+bbox_max.x)/2
 front=bbox_min.y
 z=bbox_min.z+(bbox_max.z-bbox_min.z)*.38
 o=cube('artisan_recessed_handle_backplate_'+str(i),(x,front-.009,z),(.18,.042,.25),leaf);additions.append(o)
 o=ring('wrought_iron_ring_pull_'+str(i),(x,front-.044,z-.018),leaf);additions.append(o)
 for sign in (-1,1):
  for sz in (-1,1):
   o=stud('flush_peened_attachment_'+str(i),(x+sign*.065,front-.034,z+sz*.092),.012,leaf)
   additions.append(o)
# Actual hinge leaf parents must remain independently articulated with their handles.
bpy.context.view_layer.update()
render(out/'after'/'reinforced_oak_gate.png')
cam_before=cam.matrix_world.copy()
cam.location=center+Vector((-.88,1.55,1.10)).normalized()*max(size.length,1)*3
cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
render(out/'after'/'reinforced_oak_gate_rear.png')
cam.matrix_world=cam_before
bpy.ops.object.select_all(action='DESELECT')
for ob in meshes+additions:ob.select_set(True)
bpy.context.view_layer.objects.active=meshes[0]
dest=out/'glb'/'reinforced_oak_gate.glb'
bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
if dest.stat().st_size<300000:raise RuntimeError('Lost CC0 geometry and maps')
glbsha=sha(dest)
report={
 'asset':'reinforced_oak_gate','license':'Poly Haven original CC0, derived modifications',
 'source_sha':sha(source),'result_sha':glbsha,
 'source_geometry_parts':[ob.name for ob in meshes],
 'modification':'Wood toned with preserved photos; two leaf-parented forged iron ring handles and 8 non-floating pins',
 'new_details':len(additions),'source_mats':original_materials,'atlas_changes':images_changed,
 'technical':'PASS_PENDING_ART','art_approved':False,'unity_approved':False,
 'checklist':['Wood dark-warm stylized consistent with Valoria','Door arch and leaf hinges still intact',
              'Handles attached to individual leaves','Actual contact and camera readability','No floating rivets'],
 'limitations':['Two front iron handles intentionally added; check against game distance and door overlap'],
 'no_scene_composition':True
}
(out/'reports'/'finished_gate_v2.json').write_text(json.dumps(report,indent=2))
print('ELDORIA_REAL_CC0_GATE_ART_FINISH',json.dumps({'sha':glbsha,'new_details':len(additions),'atlas_count':len(images_changed),'glb_bytes':dest.stat().st_size}),flush=True)
