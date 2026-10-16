"""Standalone CC0 photographed ground assets: NO white scattered fake sphere dots.
Preserve authored 3D grass details and existing ground mesh; replace only
base ground PBR with authenticated grass/forest/dirt 1K albedo+normal.
Two independent grass/dirt modules each in two choices (4 candidate GLBs).
"""
import bpy,sys,json,math,hashlib,traceback
from pathlib import Path
from mathutils import Vector
A=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else []
if len(A)!=3:raise SystemExit('Original V3 GLB folder, CC0 soil 1K folder, output')
src,cc0,out=map(Path,A)
for sub in ('glb','before','after','reports'):(out/sub).mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
s=bpy.context.scene;s.render.engine='BLENDER_EEVEE';s.render.resolution_x=1050;s.render.resolution_y=1050
s.render.image_settings.file_format='PNG';s.view_settings.view_transform='AgX'
w=bpy.data.worlds.new('Alpine ground photo-review neutral');w.use_nodes=True
w.node_tree.nodes['Background'].inputs['Color'].default_value=(.19,.24,.30,1);w.node_tree.nodes['Background'].inputs['Strength'].default_value=.80;s.world=w
def area(n,pow,p,color):
 l=bpy.data.lights.new(n,'AREA');l.energy=pow;l.color=color;l.size=9;l.shape='DISK'
 ob=bpy.data.objects.new(n,l);bpy.context.collection.objects.link(ob)
 ob.location=p;ob.rotation_euler=(-ob.location).to_track_quat('-Z','Y').to_euler()
area('golden sky',1400,(-8,-9,12),(1,.86,.68))
area('cool fill',1300,(7,3,11),(.73,.85,1))
area('neutral rim',750,(1,8,9),(1,1,1))
cd=bpy.data.cameras.new('matched single ground camera');cam=bpy.data.objects.new('matched single ground camera',cd)
bpy.context.collection.objects.link(cam);s.camera=cam;cd.type='ORTHO'
rig=set(bpy.data.objects)
def bounds(mesh):
 pts=[o.matrix_world@Vector(v) for o in mesh for v in o.bound_box]
 return Vector([min(p[i] for p in pts) for i in range(3)]),Vector([max(p[i] for p in pts) for i in range(3)])
def view(mesh):
 low,high=bounds(mesh);c=(low+high)/2;e=high-low
 cam.location=c+Vector((.95,-1.5,1.1)).normalized()*max(e.length,1)*3
 cam.rotation_euler=(c-cam.location).to_track_quat('-Z','Y').to_euler()
 cd.ortho_scale=max(e.x,e.y,e.z,.4)*1.71
def photo(name):
 d=cc0/(name+'_diffuse.jpg')
 n=cc0/(name+'_nor_gl.jpg')
 if not d.is_file() or not n.is_file():raise RuntimeError('Missing certified 1K ground PBR source: '+name)
 mat=bpy.data.materials.new('ELDORIA_CC0_REAL_'+name)
 mat.use_nodes=True;ns=mat.node_tree.nodes;ns.clear();lk=mat.node_tree.links
 texture=ns.new('ShaderNodeTexImage');texture.image=bpy.data.images.load(str(d),check_existing=True)
 bs=ns.new('ShaderNodeBsdfPrincipled');bs.inputs['Roughness'].default_value=.96
 lk.new(texture.outputs['Color'],bs.inputs['Base Color'])
 normal=ns.new('ShaderNodeTexImage');normal.image=bpy.data.images.load(str(n),check_existing=True)
 normal.image.colorspace_settings.name='Non-Color'
 mapn=ns.new('ShaderNodeNormalMap');mapn.inputs['Strength'].default_value=.34
 lk.new(normal.outputs['Color'],mapn.inputs['Color']);lk.new(mapn.outputs['Normal'],bs.inputs['Normal'])
 output=ns.new('ShaderNodeOutputMaterial');lk.new(bs.outputs['BSDF'],output.inputs['Surface'])
 return mat
textures={n:photo(n) for n in ['grass_ground','dirt','forest_ground_04']}
def shoot(path):s.render.filepath=str(path);bpy.ops.render.render(write_still=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def largest_flat(meshes):
 # Big horizontal plane (actual Blender Z), not grass blade microobjects.
 return max(meshes,key=lambda o:o.dimensions.x*o.dimensions.y)
entries=[]
for name,styles in [('grass_ground_patch',['grass_ground','forest_ground_04']),
                     ('dirt_ground_patch',['dirt','forest_ground_04'])]:
 baseline=src/(name+'.glb')
 for style in styles:
  pre=set(bpy.data.objects)
  row={'asset':name,'variant':style,'art_approved':False,'target':6}
  try:
   bpy.ops.import_scene.gltf(filepath=str(baseline))
   objs=[o for o in bpy.data.objects if o not in pre]
   meshes=[o for o in objs if o.type=='MESH']
   if not meshes:raise RuntimeError('No ground geometry')
   view(meshes)
   if style==styles[0]:shoot(out/'before'/(name+'.png'))
   ground=largest_flat(meshes)
   if ground.dimensions.x<2.5 or ground.dimensions.y<2.5:raise RuntimeError('Cannot identify 4m ground slab')
   ground.data.materials.clear();ground.data.materials.append(textures[style])
   # UVs from original are retained; on a missing UV slab use planar projection.
   if not ground.data.uv_layers:
    uv=ground.data.uv_layers.new(name='UVMap')
    points=[v.co for v in ground.data.vertices]
    xmin=min(x.x for x in points);ymin=min(x.y for x in points)
    dx=max(x.x for x in points)-xmin;dy=max(x.y for x in points)-ymin
    for p in ground.data.polygons:
     for li in p.loop_indices:
      v=ground.data.vertices[ground.data.loops[li].vertex_index].co
      uv.data[li].uv=((v.x-xmin)/max(dx,.01),(v.y-ymin)/max(dy,.01))
   shoot(out/'after'/(name+'_'+style+'.png'))
   bpy.ops.object.select_all(action='DESELECT')
   for ob in objs:ob.select_set(True)
   bpy.context.view_layer.objects.active=ground
   target=out/'glb'/(name+'_'+style+'.glb')
   bpy.ops.export_scene.gltf(filepath=str(target),export_format='GLB',use_selection=True,export_apply=True)
   if target.stat().st_size<20000:raise RuntimeError('GLB too small')
   row.update({'technical':'PASS_PENDING_ART','source_sha':sha(baseline),
               'candidate_sha':sha(target),'ground_material':style,'source_license':'Poly Haven CC0 1.0',
               'original_3d_microplant_geometry_preserved':len(meshes)-1})
   print('TRUE_PHOTOGRAPHIC_GROUND',name,style,len(meshes),flush=True)
  except Exception as e:
   row.update({'technical':'FAILED','error':repr(e)});traceback.print_exc()
  finally:
   entries.append(row)
   for o in list(bpy.data.objects):
    if o not in rig:bpy.data.objects.remove(o,do_unlink=True)
   (out/'reports'/'four_real_ground_surface_options.json').write_text(json.dumps({
    'source_v3_run':38813966671,'cc0_source_run':38870945019,
    'all_art_approved':False,'candidates':entries,'no_scene':True},indent=2))
if len(entries)!=4 or any(x['technical']!='PASS_PENDING_ART' for x in entries):
 raise RuntimeError('Some authentic photo ground candidate failed')
print('FOUR_CC0_PHOTO_TERRAIN_CANDIDATES_REAL_BLENDER_OK')
