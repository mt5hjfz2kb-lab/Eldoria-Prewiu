import bpy,sys,json,hashlib
from pathlib import Path
from mathutils import Vector,Matrix
a=sys.argv[sys.argv.index('--')+1:];oldroot,donor,out=map(Path,a)
for d in ['before','after','glb','reports']:(out/d).mkdir(parents=True,exist_ok=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for name in ('merlon_low','merlon_tall'):
 bpy.ops.wm.read_factory_settings(use_empty=True);s=bpy.context.scene;s.render.engine='BLENDER_EEVEE'
 s.render.resolution_x=1150;s.render.resolution_y=900;s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
 w=bpy.data.worlds.new('Valoria neutral limestone');w.use_nodes=True;w.node_tree.nodes['Background'].inputs['Color'].default_value=(.4,.45,.49,1);w.node_tree.nodes['Background'].inputs['Strength'].default_value=.85;s.world=w
 for n,p,loc,col in [('warm',1600,(-6,-8,10),(1,.89,.78)),('fill',1200,(7,5,11),(.78,.89,1))]:
  l=bpy.data.lights.new(n,'AREA');l.energy=p;l.size=9;l.color=col;o=bpy.data.objects.new(n,l);bpy.context.collection.objects.link(o);o.location=loc;o.rotation_euler=(-o.location).to_track_quat('-Z','Y').to_euler()
 cd=bpy.data.cameras.new('unchanged A B');cam=bpy.data.objects.new('unchanged A B',cd);bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
 def meshes():return [x for x in s.objects if x.type=='MESH']
 def bb(os):
  bpy.context.view_layer.update();ps=[o.matrix_world@Vector(v) for o in os for v in o.bound_box]
  return Vector(tuple(min(p[i] for p in ps) for i in range(3))),Vector(tuple(max(p[i] for p in ps) for i in range(3)))
 def shoot(path):s.render.filepath=str(path);bpy.ops.render.render(write_still=True)
 original=oldroot/(name+'.glb');bpy.ops.import_scene.gltf(filepath=str(original))
 prev=meshes();assert prev
 lo,hi=bb(prev);size=hi-lo;center=(lo+hi)*.5
 if not (.35<size.z<.46 if name=='merlon_low' else 1.1<size.z<1.3):raise RuntimeError('Wrong vertical Blender-Z geometry')
 cam.location=center+Vector((1,-1.5,1.2)).normalized()*max(size.length,1)*3
 cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();cd.ortho_scale=max(size.length*1.3,1.45)
 shoot(out/'before'/(name+'.png'))
 for ob in prev:bpy.data.objects.remove(ob,do_unlink=True)
 bpy.ops.import_scene.gltf(filepath=str(donor));source=meshes()
 if len(source)!=1:raise RuntimeError('Expect one natural stone ashlar donor')
 base=source[0];base.data=base.data.copy();world=base.matrix_world.copy()
 for v in base.data.vertices:v.co=world@v.co
 base.matrix_world=Matrix.Identity(4);bl,bh=bb([base]);d=bh-bl
 n=1 if name=='merlon_low' else 3;gap=.009
 targeth=(size.z-(n-1)*gap)/n
 sc=Vector((size.x/d.x,size.y/d.y,targeth/d.z))
 if max(sc)/min(sc)>2:raise RuntimeError('Stone photograph too stretched')
 items=[]
 for k in range(n):
  ob=base if k==0 else base.copy()
  if k:ob.data=base.data.copy();bpy.context.collection.objects.link(ob)
  ob.name=name+'_one_physical_stone_'+str(k)
  ob.scale=sc;bpy.context.view_layer.update();low,high=bb([ob])
  target=Vector((center.x,center.y,lo.z+(k+.5)*targeth+k*gap))
  ob.location+=target-(low+high)*.5;items.append(ob)
 newlo,newhi=bb(items);actual=newhi-newlo
 if max(abs(actual[i]-size[i]) for i in range(3))>.02:raise RuntimeError('Merlon changed envelope '+str(actual))
 shoot(out/'after'/(name+'.png'))
 cd.ortho_scale*=.77;shoot(out/'after'/(name+'_macro.png'))
 bpy.ops.object.select_all(action='DESELECT')
 for ob in items:ob.select_set(True)
 bpy.context.view_layer.objects.active=items[0]
 file=out/'glb'/(name+'.glb');bpy.ops.export_scene.gltf(filepath=str(file),export_format='GLB',use_selection=True,export_apply=True)
 if file.stat().st_size<10000:raise RuntimeError('GLB absent')
 report={'asset':name,'source_run':38871632375,'donor_run':38825364463,'donor':'stone_ashlar_04','before_sha':sha(original),'donor_sha':sha(donor),'candidate_sha':sha(file),'independent_courses':n,'same_camera':True,'envelope_fit':True,'before_dimensions':list(size),'after_dimensions':list(actual),'license':'original donator CC0 status pending source verification','art_approved':False,'unity_validated':False}
 (out/'reports'/(name+'.json')).write_text(json.dumps(report,indent=2))
 print('TWO_REAL_STONE_MERLONS_PENDING_VISUAL',name,sha(file))
