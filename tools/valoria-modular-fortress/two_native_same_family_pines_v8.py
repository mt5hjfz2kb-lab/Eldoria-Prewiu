"""Eldoria: preserve original pine_small/tall; derive two real same-family fine branch modules from already-art-reviewed pine_medium.
Uniform scale of an authentic native model, no new library or scene. Independent Blender A/B and original SHA."""
import bpy,sys,json,hashlib
from mathutils import Vector,Matrix
from pathlib import Path
a=sys.argv[sys.argv.index('--')+1:]
if len(a)!=3:raise RuntimeError('source_two_pines_dir verified_medium_pine_glb output')
src,donor,out=map(Path,a)
for n in ('before','after','glb','reports'):(out/n).mkdir(parents=True,exist_ok=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for name in ('pine_small','pine_tall'):
 bpy.ops.wm.read_factory_settings(use_empty=True);s=bpy.context.scene
 s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1040;s.render.resolution_y=950
 s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
 w=bpy.data.worlds.new('Neutral forest art studio');w.use_nodes=True;w.node_tree.nodes['Background'].inputs['Color'].default_value=(.40,.45,.52,1);w.node_tree.nodes['Background'].inputs['Strength'].default_value=.82;s.world=w
 for n,p,loc,col in [('main',1550,(-5,-8,10),(1,.91,.78)),('fill',1000,(7,6,10),(.80,.89,1))]:
  light=bpy.data.lights.new(n,'AREA');light.energy=p;light.size=11;light.color=col
  o=bpy.data.objects.new(n,light);bpy.context.collection.objects.link(o)
  o.location=loc;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
 data=bpy.data.cameras.new('A B identical');cam=bpy.data.objects.new('A B identical',data);bpy.context.collection.objects.link(cam);s.camera=cam;data.type='ORTHO'
 def meshes():return [o for o in s.objects if o.type=='MESH']
 def bounds(obs):
  bpy.context.view_layer.update()
  q=[o.matrix_world@Vector(v) for o in obs for v in o.bound_box]
  return Vector(tuple(min(v[i] for v in q) for i in range(3))),Vector(tuple(max(v[i] for v in q) for i in range(3)))
 def shoot(p):s.render.filepath=str(p);bpy.ops.render.render(write_still=True)
 old=src/(name+'.glb')
 if not old.exists() or not donor.exists():raise RuntimeError('Native source model missing '+name)
 bpy.ops.import_scene.gltf(filepath=str(old));existing=meshes()
 if not existing:raise RuntimeError('empty original '+name)
 low,high=bounds(existing);dim=high-low;center=(low+high)*.5
 cam.location=center+Vector((1,-1.55,1)).normalized()*dim.y*2.3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();data.ortho_scale=dim.y*1.32
 shoot(out/'before'/(name+'.png'))
 for ob in existing:bpy.data.objects.remove(ob,do_unlink=True)
 bpy.ops.import_scene.gltf(filepath=str(donor));new=meshes()
 if len(new)<3:raise RuntimeError('Real tree donor lacks separate branches and canopy')
 # Bake imported world matrices into all vertices, then use the SAME uniform
 # scale for trunk/branches: no distortions of needles or bark.
 for ob in new:
  mat=ob.matrix_world.copy();ob.data=ob.data.copy()
  for vertex in ob.data.vertices:vertex.co=mat@vertex.co
  ob.matrix_world=Matrix.Identity(4)
 dl,dh=bounds(new);d=dh-dl
 factor=float(dim.y)/float(d.y)
 if not (.5<factor<1.7):raise RuntimeError('Target pine scale inconsistent with donor')
 shift=Vector((-((dl.x+dh.x)*.5)*factor+center.x,-dl.y*factor+low.y,-((dl.z+dh.z)*.5)*factor+center.z))
 for ob in new:
  for vert in ob.data.vertices:vert.co=vert.co*factor+shift
 bpy.context.view_layer.update();nl,nh=bounds(new);nd=nh-nl
 if abs(nd.y-dim.y)>.015 or abs(nd.x-dim.x)/dim.x>.085:raise RuntimeError('Canopy exceeds modular expected envelope')
 shoot(out/'after'/(name+'.png'))
 bpy.ops.object.select_all(action='DESELECT')
 for ob in new:ob.select_set(True)
 bpy.context.view_layer.objects.active=new[0]
 target=out/'glb'/(name+'.glb')
 bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
 if target.stat().st_size<30000:raise RuntimeError('Baked natural pine export missing')
 j={'asset':name,'source_run':38819524063,'donor':'pine_medium (original same-family Quaternius-derived asset)','donor_run':38877895047,
 'source_sha':sha(old),'donor_sha':sha(donor),'candidate_sha':sha(target),
 'source_bounds_m':list(dim),'candidate_bounds_m':list(nd),'natural_scale':factor,
 'separate_native_meshes':len(new),'no_scene':True,'same_camera':True,'technical':'PASS_PENDING_VISUAL','art_approved':False,'unity_validated':False}
 (out/'reports'/(name+'.json')).write_text(json.dumps(j,indent=2))
 print('REAL_PINE_SAME_FAMILY_PENDING_VISUAL',name,sha(target),len(new))
