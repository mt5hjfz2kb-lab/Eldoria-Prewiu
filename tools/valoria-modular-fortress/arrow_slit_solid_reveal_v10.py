"""ELDORIA: improve EXACT source arrow_slit_frame real embrasure, not decorative black patch.
V10: 2cm physical volume on imported paper-thin splayed reveals; open-ray test,
same camera and actual 180 degree second viewpoint. No Unity or other assets.
"""
import bpy,sys,json,hashlib,math
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree
argv=sys.argv[sys.argv.index('--')+1:]
if len(argv)!=2:raise RuntimeError('source arrow_slit_frame GLB and output dir required')
source,out=map(Path,argv)
if not source.is_file():raise RuntimeError('Original verified genuine GLB missing')
for sub in ('before','after','glb','reports'):(out/sub).mkdir(parents=True,exist_ok=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
bpy.ops.wm.read_factory_settings(use_empty=True)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE'
scene.render.resolution_x=1080;scene.render.resolution_y=1070
scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='AgX'
world=bpy.data.worlds.new('neutral sandstone studio');world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.39,.45,.53,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.85;scene.world=world
for title,power,pos,color in (
 ('warm edge relief',1600,(-5,-7,10),(1,.89,.75)),
 ('cool opening interior',1350,(5,5,8),(.78,.89,1)),
 ('rear sunlight',550,(-2,8,6),(.9,.96,1))):
 lamp=bpy.data.lights.new(title,'AREA');lamp.energy=power;lamp.size=5;lamp.color=color
 node=bpy.data.objects.new(title,lamp);bpy.context.collection.objects.link(node)
 node.location=pos;node.rotation_euler=(-node.location).to_track_quat('-Z','Y').to_euler()
camdata=bpy.data.cameras.new('locked A B');cam=bpy.data.objects.new('locked A B',camdata)
bpy.context.collection.objects.link(cam);scene.camera=cam;camdata.type='ORTHO'
def allmeshes():return [ob for ob in scene.objects if ob.type=='MESH']
def bbox(obs):
 bpy.context.view_layer.update()
 pts=[ob.matrix_world@Vector(p) for ob in obs for p in ob.bound_box]
 return Vector(tuple(min(v[i] for v in pts) for i in range(3))),Vector(tuple(max(v[i] for v in pts) for i in range(3)))
def capture(p):scene.render.filepath=str(p);bpy.ops.render.render(write_still=True)
bpy.ops.import_scene.gltf(filepath=str(source))
original=allmeshes();startnames={o.name for o in original}
if len(original)<10:raise RuntimeError('Wrong source; modular physical quoin geometry absent')
low,high=bbox(original);dim=high-low;mid=(low+high)*.5
if not (.7<dim.x<1.2 and 1.0<dim.z<1.8):raise RuntimeError('Unknown source envelope '+str(dim))
cam.location=mid+Vector((.83,-1.56,1.12)).normalized()*max(dim.length,1)*3
cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
camdata.ortho_scale=dim.length*1.28
# The earlier black rectangle was a NON-EXPORTED studio backdrop, not the asset.
# A neutral pale examination sheet at y far beyond the rear improves visibility
# of a TRUE physical hole, and remains identical before vs after.
bpy.ops.mesh.primitive_plane_add(size=2,location=(mid.x,high.y+.8,mid.z),rotation=(math.pi/2,0,0))
back=bpy.context.object;back.name='STUDIO_NEUTRAL_CHECK_SHEET_NOT_EXPORTED'
back.scale=(1.3,1.3,1.3)
backmat=bpy.data.materials.new('sky-toned background for hole verification')
backmat.use_nodes=True
backmat.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=(.47,.62,.74,1)
back.data.materials.append(backmat)
capture(out/'before'/'arrow_slit_frame.png')
reveal=[ob for ob in original if 'REAL_INSIDE_SPLAY_' in ob.name]
if len(reveal)!=2:raise RuntimeError('Two real source slit reveals required')
# Replace only the two imported zero-thickness polygons.
# Each new reveal extrudes toward masonry exterior, AWAY from the clear slit;
# it retains the original front/back vertices and stone material/UV texture.
created=[]
for ob in reveal:
 side=-1 if '-1' in ob.name else 1
 me=ob.data
 points=[ob.matrix_world@vert.co for vert in me.vertices]
 if len(points)!=4:raise RuntimeError('Original reveal shape changed: '+ob.name)
 # Convert to ordered quad surfaces in world coordinates; back and front remain
 # untouched while their 20mm solid thickness is embedded in stone jamb.
 extr=Vector((side*.020,0,0))
 coords=[tuple(v) for v in points]+[tuple(v+extr) for v in points]
 polys=[(0,1,2,3),(7,6,5,4),(0,4,5,1),(1,5,6,2),
        (2,6,7,3),(3,7,4,0)]
 geo=bpy.data.meshes.new('genuine_splayed_20mm_stone_reveal')
 geo.from_pydata(coords,[],polys);geo.update()
 thick=bpy.data.objects.new(ob.name.replace('REAL_INSIDE_SPLAY','SOLID_LIMESTONE_REVEAL_20MM'),geo)
 bpy.context.collection.objects.link(thick)
 for m in ob.data.materials:geo.materials.append(m)
 # Individual side faces have UVs; use a local, coherent spatial projection,
 # no stretched photo from 1m slab into 2cm sliver.
 uv=geo.uv_layers.new(name='Real stone reveal uv')
 mn=Vector(tuple(min(v[i] for v in points) for i in range(3)))
 for poly in geo.polygons:
  for li in poly.loop_indices:
   v=geo.vertices[geo.loops[li].vertex_index].co
   uv.data[li].uv=((v.y-mn.y)*1.4,(v.z-mn.z)*1.3)
 bpy.data.objects.remove(ob,do_unlink=True);created.append(thick)
solid=[ob for ob in allmeshes() if ob!=back]
afterlow,afterhigh=bbox(solid);newdim=afterhigh-afterlow
if max(abs(dim[i]-newdim[i]) for i in range(3))>.002:
 raise RuntimeError('DO NOT PROMOTE: changed modular envelope')
# A real ray through front opening must NOT intersect any exported object.
deps=bpy.context.evaluated_depsgraph_get()
for rz in (.40,.55,.70):
 raystart=Vector((mid.x,low.y-.6,low.z+dim.z*rz))
 raydir=Vector((0,1,0))
 for ob in solid:
  tree=BVHTree.FromObject(ob,deps)
  inv=ob.matrix_world.inverted()
  origin_local=inv@raystart
  dir_local=(inv.to_3x3()@raydir).normalized()
  hit=tree.ray_cast(origin_local,dir_local,dim.y+1.2)
  if hit[0] is not None:raise RuntimeError('Hole obstructed at '+str(rz)+' by '+ob.name)
capture(out/'after'/'arrow_slit_frame.png')
front=cam.matrix_world.copy();orthoscale=camdata.ortho_scale
cam.location=mid+Vector((.15,-1.9,.35)).normalized()*2.1
cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
camdata.ortho_scale=dim.z*1.37
capture(out/'after'/'arrow_slit_frame_macro.png')
cam.matrix_world=front;camdata.ortho_scale=orthoscale
cam.location=mid+Vector((.65,1.7,.85)).normalized()*3
cam.rotation_euler=(mid-cam.location).to_track_quat('-Z','Y').to_euler()
# Backing sheet is for front-hole contrast only; it would obscure the actual
# posterior geometry. Hide it for genuine reverse/oblique verification.
back.hide_render=True
capture(out/'after'/'arrow_slit_frame_rear.png')
back.hide_render=False
# Export ONLY the 13 true 3D objects. NEVER export the study sheet.
bpy.ops.object.select_all(action='DESELECT')
for ob in solid:ob.select_set(True)
bpy.context.view_layer.objects.active=solid[0]
dest=out/'glb'/'arrow_slit_frame.glb'
bpy.ops.export_scene.gltf(filepath=str(dest),export_format='GLB',use_selection=True,export_apply=True)
if dest.stat().st_size<100000:raise RuntimeError('Native GLB absent')
report={'asset':'arrow_slit_frame','source_run':38866656756,
 'source_artifact':12031725939,'source_sha256':sha(source),
 'candidate_sha256':sha(dest),'source_meshes':len(original),
 'candidate_meshes':len(solid),'source_envelope_m':list(dim),
 'final_envelope_m':list(newdim),'physical_reveal_thickness_m':.020,
 'center_hole_rays_tested':3,'center_hole_unobstructed':True,
 'same_camera_before_after':True,'rear_inspection_png':True,
 'studio_backdrop_exported':False,'no_unity_scene_mutations':True,
 'art_approved':False,'unity_validated':False,'technical':'PASS_PENDING_VISUAL'}
(out/'reports'/'true_3d_splayed_slit_v10.json').write_text(json.dumps(report,indent=2))
print('SPLIT_STONE_REVEALS_TRUE_20MM_NO_OCCLUSION',sha(dest))
