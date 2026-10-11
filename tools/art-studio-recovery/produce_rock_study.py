"""Real reference-directed donor assembly. Preserves baseline mesh; edits only cliff faces.
Existing planner/donor audit/GLB preflight are reused. Output is a candidate, never auto ART PASS.
"""
import bpy,bmesh,math,random,json,os,hashlib
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'art-source/valoria/production/studio-rocks-v1';OUT.mkdir(parents=True,exist_ok=True)
DONOR=Path(os.environ.get('ELDORIA_CC0_ROCK_SOURCE','/tmp/eldoria-rock-source'))
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def height(x,y):
 t=max(0,min(1,(y-8)/18));return -.25+t*t*(3-2*t)*5.2+.35*math.sin(x*.18)*math.cos(y*.14)+.19*math.sin(x*.43+y*.37)
def radius(a):return 39+2.6*math.sin(a*7.3)+1.3*math.cos(a*13.2)+.65*math.sin(a*27.7)
def top(x,y):return height(x,y)+max(0,(math.hypot(x,y)-19)/22)*.38*(.42*math.sin(x*.37+y*.19)+.24*math.sin(x*.81-y*.51)+.12*math.sin(x*1.7+y*1.31))
bpy.ops.wm.read_factory_settings(use_empty=True)
baseline=ROOT/'Unity/Assets/Eldoria/ArtTests/ValoriaGatehouseV1/valoria_visual_slice_environment_v2.glb'
bpy.ops.import_scene.gltf(filepath=str(baseline));terrain=[o for o in bpy.data.objects if o.type=='MESH'];assert len(terrain)==1
# Architecture is a context anchor, unchanged between renders; no gameplay scene is touched.
gatepath=ROOT/'Unity/Assets/Eldoria/ArtTests/ValoriaGatehouseV1/valoria_gate_reference.glb'
pre=set(bpy.data.objects);bpy.ops.import_scene.gltf(filepath=str(gatepath));parts=[o for o in bpy.data.objects if o not in pre and o.type=='MESH']
coords=[o.matrix_world@v.co for o in parts for v in o.data.vertices];lo=Vector(tuple(min(v[k] for v in coords) for k in range(3)));hi=Vector(tuple(max(v[k] for v in coords) for k in range(3)));scale=17.3/(hi.x-lo.x)
for o in parts:
 world=[o.matrix_world@v.co for v in o.data.vertices];o.matrix_world.identity()
 for v,p in zip(o.data.vertices,world):v.co=(p-Vector(((hi.x+lo.x)/2,(hi.y+lo.y)/2,lo.z)))*scale
 u=o.copy();u.data=o.data;bpy.context.collection.objects.link(u);u.scale=(.79,)*3;u.location=(0,27,height(0,27))
s=bpy.context.scene;s.render.engine='CYCLES';s.cycles.samples=24;s.cycles.seed=2711;s.cycles.use_denoising=False;s.render.resolution_percentage=100;s.view_settings.view_transform='Standard';s.render.image_settings.file_format='PNG'
w=bpy.data.worlds.new('Fixed cool Valoria ambient');s.world=w;w.use_nodes=True;w.node_tree.nodes['Background'].inputs['Color'].default_value=(.24,.31,.41,1);w.node_tree.nodes['Background'].inputs['Strength'].default_value=.7
li=bpy.data.lights.new('Fixed soft key','SUN');ob=bpy.data.objects.new('Fixed soft key',li);bpy.context.collection.objects.link(ob);ob.rotation_euler=(.55,-.4,-.55);li.energy=1.8;li.angle=.1
camd=bpy.data.cameras.new('Matched strategic study');cam=bpy.data.objects.new('Matched strategic study',camd);bpy.context.collection.objects.link(cam);s.camera=cam;camd.type='ORTHO'
views={'overview':((50,-70,49),(0,4,0),108,1280,800),'cliff':((52,-64,18),(15,-24,-2),44,1280,800),'mobile':((50,-70,49),(0,4,0),108,640,400)}
def render(label):
 for name,(pos,target,scale,rx,ry) in views.items():
  cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();camd.ortho_scale=scale;s.render.resolution_x=rx;s.render.resolution_y=ry;s.render.filepath=str(OUT/f'{label}-{name}.png');bpy.ops.render.render(write_still=True)
render('before')
# Retain all top/grass/shrub faces; retire exposed stone facets only. No per-vertex noise patch.
removed=0
for o in terrain:
 bm=bmesh.new();bm.from_mesh(o.data)
 indices={i for i,m in enumerate(o.data.materials) if m and 'Weathered layered granite' in m.name}
 doomed=[f for f in bm.faces if f.material_index in indices and any(math.hypot(v.co.x,v.co.y)>35 for v in f.verts)]
 removed+=len(doomed);bmesh.ops.delete(bm,geom=doomed,context='FACES');bm.to_mesh(o.data);bm.free();o.data.update()
obj=next(DONOR.rglob('Rock_LOD4-2k.obj'));pre=set(bpy.data.objects);bpy.ops.wm.obj_import(filepath=str(obj));rocks=[o for o in bpy.data.objects if o not in pre and o.type=='MESH'];assert len(rocks)==1
master=rocks[0];coords=[master.matrix_world@v.co for v in master.data.vertices];lo=Vector(tuple(min(v[k] for v in coords) for k in range(3)));hi=Vector(tuple(max(v[k] for v in coords) for k in range(3)));size=hi-lo;master.matrix_world.identity()
for v,p in zip(master.data.vertices,coords):v.co=((p.x-(lo.x+hi.x)/2)/size.x,(p.y-(lo.y+hi.y)/2)/size.y,(p.z-lo.z)/size.z)
mat=bpy.data.materials.new('CC0 scanned slate donor - exportable PBR');mat.use_nodes=True;bs=mat.node_tree.nodes.get('Principled BSDF');bs.inputs['Roughness'].default_value=.88
images=[]
for kind,suffix in [('albedo','diffuse'),('normal','normals')]:
 p=obj.parent/f'Rock_LOD4_{suffix}.tga';im=bpy.data.images.load(str(p));im.scale(1024,1024);im.filepath_raw=str(OUT/f'scanned-rock-{kind}.png');im.file_format='PNG';im.save();images.append(im)
 tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im
 if kind=='albedo':mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
 else:
  im.colorspace_settings.name='Non-Color';n=mat.node_tree.nodes.new('ShaderNodeNormalMap');n.inputs['Strength'].default_value=.6;mat.node_tree.links.new(tex.outputs['Color'],n.inputs['Color']);mat.node_tree.links.new(n.outputs['Normal'],bs.inputs['Normal'])
master.data.materials.clear();master.data.materials.append(mat)
# Cut donor summit below its upper third: a cliff receiver needs a broad crown,
# rather than the narrow boulder tip which produced hanging grass and picket repetition.
bm=bmesh.new();bm.from_mesh(master.data)
bmesh.ops.bisect_plane(bm,geom=list(bm.verts)+list(bm.edges)+list(bm.faces),plane_co=(0,0,.68),plane_no=(0,0,1),clear_outer=True,clear_inner=False)
bm.to_mesh(master.data);bm.free();master.data.update()
for p in master.data.polygons:p.use_smooth=True
# Directed bedrock course: consistent geological orientation, uneven spacing and embedded tops.
rock_objects=[];rng=random.Random(102611)
for i in range(52):
 a=math.tau*i/52+.015*math.sin(i*2.31);r=radius(a)-.35;x=r*math.cos(a);y=r*math.sin(a)
 o=master.copy();o.data=master.data;bpy.context.collection.objects.link(o);o.name=f'CC0 directed cliff course {i:02d}'
 o.rotation_euler=(0,.07*math.sin(i*1.7),a+math.pi/2+.12*math.sin(i*.91));o.scale=(10.6+rng.random()*1.2,6.2+rng.random()*1.1,12.2+rng.random()*1.5)
 o.location=(x,y,top(x,y)-.68*o.scale.z+.16);rock_objects.append(o)
# Bedrock feet as a discontinuous talus course, not a second repeated wall.
for i in range(16):
 a=math.tau*(i+.27)/16;r=radius(a)+2.7;x=r*math.cos(a);y=r*math.sin(a)
 o=master.copy();o.data=master.data;bpy.context.collection.objects.link(o);o.name=f'CC0 talus foot {i:02d}';o.rotation_euler=(.23,.15,a+.42);o.scale=(3.1+rng.random(),3.2+rng.random(),2+rng.random()*1.8);o.location=(x,y,top(x,y)-9.5);rock_objects.append(o)
bpy.data.objects.remove(master,do_unlink=True)
render('after')
# Save genuine editable source; share the same normalized mobile donor mesh among instances.
for im in images:im.pack()
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'ValoriaRockStudy.blend'))
bpy.ops.object.select_all(action='DESELECT')
for o in rock_objects:o.select_set(True)
bpy.context.view_layer.objects.active=rock_objects[0]
bpy.ops.export_scene.gltf(filepath=str(OUT/'ValoriaScannedCliffCandidate.glb'),export_format='GLB',use_selection=True,export_apply=True)
# Also provide a replacement environment with top height/macro footprint retained.
bpy.ops.object.select_all(action='DESELECT')
for o in terrain+rock_objects:o.select_set(True)
bpy.context.view_layer.objects.active=terrain[0]
bpy.ops.export_scene.gltf(filepath=str(OUT/'ValoriaEnvironmentCandidate.glb'),export_format='GLB',use_selection=True,export_apply=True)
report={'method':'existing_cc0_photogrammetry_donor_directed_assembly','baseline_sha256':sha(baseline),'donor_obj_sha256':sha(obj),'donor_archive_sha256':sha('/tmp/eldoria-rock.7z') if Path('/tmp/eldoria-rock.7z').exists() else None,'donor_triangles':sum(len(p.vertices)-2 for p in rock_objects[0].data.polygons),'cliff_instances':52,'talus_instances':16,'removed_facets':removed,'fixed_camera_lighting_samples':True,'geometry_modified':True,'pbr_albedo_and_normal_exportable':True,'model_used':'assistant visual review; Ollama runtime connection pending','mcp_used':False,'visual_pass':False,'unity_tested':False,'credits_spent':0,'exports':{p.name:{'bytes':p.stat().st_size,'sha256':sha(p)} for p in OUT.glob('*.glb')},'limitations':['No Unity certification; isolated Blender context differs from full Unity scene','Artist acceptance requires actual before/after inspection','Draw calls/LOD/device performance not certified']}
(OUT/'production-report.json').write_text(json.dumps(report,indent=2))
