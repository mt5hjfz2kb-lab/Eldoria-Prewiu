"""Directed repair of the actual Breach: ground-bound shale and fractured plates.
Operated through the existing BlenderMCP session; no terrain/source regeneration.
"""
import bpy, pathlib, math, json, hashlib, struct
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree

out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
source=pathlib.Path(bpy.app.driver_namespace['interactive_source'])
helpers=pathlib.Path(bpy.app.driver_namespace['interactive_helpers'])
scene=bpy.context.scene
terrain=bpy.data.objects['BLD_Terrain']
floor=bpy.data.objects['BLD_BreachFloor']
heart=bpy.data.objects['BLD_BreachMineralHeart']

def mesh_digest(mesh):
 h=hashlib.sha256()
 for collection,prop,width,dtype in [(mesh.vertices,'co',3,np.float32),(mesh.loops,'vertex_index',1,np.int32),(mesh.polygons,'material_index',1,np.int32)]:
  data=np.empty(len(collection)*width,dtype=dtype);collection.foreach_get(prop,data);h.update(data.tobytes())
 for layer in mesh.uv_layers:
  data=np.empty(len(layer.data)*2,np.float32);layer.data.foreach_get('uv',data);h.update(data.tobytes())
 return h.hexdigest()

terrain_before=mesh_digest(terrain.data)
images_before={im.name:hashlib.sha256(im.packed_file.data).hexdigest() for im in bpy.data.images if im.packed_file}
original_glb=source.parent/'RegionOneBlenderWorld.glb'
data=original_glb.read_bytes();size=struct.unpack_from('<I',data,12)[0]
gltf=json.loads(data[20:20+size]);names={node['name'] for node in gltf['nodes'] if 'name' in node}
exported=[ob for ob in scene.objects if ob.name in names]
assert terrain in exported and floor in exported and heart in exported
assert hashlib.sha256(data).hexdigest()=='8e1ba54bdf1708b3b725ab06d9084329ff3d41864277fbe91fbd8132dd4e38b6'
world_vertices=[terrain.matrix_world@v.co for v in terrain.data.vertices]
bvh=BVHTree.FromPolygons(world_vertices,[list(p.vertices) for p in terrain.data.polygons],all_triangles=False)
def height(x,y):
 hit=bvh.ray_cast(Vector((x,y,1000)),Vector((0,0,-1)),2000)[0]
 assert hit is not None,('No actual terrain',x,y)
 return hit.z
def footprint(ob):
 points=[ob.matrix_world@v.co for v in ob.data.vertices]
 return BVHTree.FromPolygons([(p.x,p.y,0) for p in points],[list(p.vertices) for p in ob.data.polygons],all_triangles=False)
old_floor_mask=footprint(floor);old_heart_mask=footprint(heart)
def inside(mask,x,y):return mask.ray_cast(Vector((x,y,100)),Vector((0,0,-1)),200)[0] is not None
old_floor_points=[floor.matrix_world@v.co for v in floor.data.vertices]
lo=np.min(np.array(old_floor_points),axis=0);hi=np.max(np.array(old_floor_points),axis=0)

# Reuse the stone surface authored for the approved quarry, recoloured as shale.
# No existing packed image or material is modified.
atlas=bpy.data.images.load(str(helpers/'ApprovedNodeMaterials.png'),check_existing=False)
w,h=atlas.size;pixels=np.array(atlas.pixels[:],np.float32).reshape(h,w,4)
crop=pixels[h//2:,:w//2].copy()
linear=np.where(crop[:,:,:3]<=.04045,crop[:,:,:3]/12.92,((crop[:,:,:3]+.055)/1.055)**2.4)
packed_maps=[]
for i,tint in enumerate([(.19,.17,.25),(.22,.20,.28),(.16,.17,.23),(.24,.19,.28)]):
 colour=crop.copy();colour[:,:,:3]=linear*np.array(tint)
 colour[:,:,:3]=np.where(colour[:,:,:3]<=.0031308,colour[:,:,:3]*12.92,1.055*colour[:,:,:3]**(1/2.4)-.055)
 image=bpy.data.images.new('Breach genuinely fractured shale grain '+str(i),crop.shape[1],crop.shape[0],alpha=True)
 image.colorspace_settings.name='Non-Color';image.pixels.foreach_set(colour.ravel())
 image.filepath_raw=str(out/('BreachShaleGrain'+str(i)+'.png'));image.file_format='PNG';image.save()
 packed=bpy.data.images.load(image.filepath_raw,check_existing=False);packed.pack();packed_maps.append(packed)
def material(name,colour):
 m=bpy.data.materials.new(name);m.use_nodes=True
 bs=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Base Color'].default_value=(*colour,1);bs.inputs['Roughness'].default_value=.96
 return m
slates=[]
for i,packed in enumerate(packed_maps):
 m=material('Breach fractured basalt '+str(i),(1,1,1));bs=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
 n=m.node_tree.nodes.new('ShaderNodeTexImage');n.image=packed;m.node_tree.links.new(n.outputs['Color'],bs.inputs['Base Color']);slates.append(m)
side=material('Breach shaded stratified slab sides',(.045,.038,.065))
dim=material('Breach naturally graded exposed mineral deep narrow fissures',(.085,.003,.19))
hot=material('Breach naturally graded exposed mineral central exposed veins',(.24,.004,.40))
edge=material('Breach nonluminous weathered outer substrate',(.075,.062,.095))

class Mesh:
 def __init__(self):self.v=[];self.f=[];self.m=[]
 def face(self,points,slot):
  n=len(self.v);self.v.extend([tuple(p) for p in points]);self.f.append(tuple(range(n,n+len(points))));self.m.append(slot)
 def install(self,name,ob=None):
  mesh=bpy.data.meshes.new(name);mesh.from_pydata(self.v,[],self.f);mesh.update()
  for m in slates+[side,dim,hot,edge]:mesh.materials.append(m)
  layer=mesh.uv_layers.new(name='Actual world metre shale grain')
  for poly,slot in zip(mesh.polygons,self.m):
   poly.material_index=slot
   for loop in poly.loop_indices:
    point=mesh.vertices[mesh.loops[loop].vertex_index].co;layer.data[loop].uv=(point.x/7.5,point.y/7.5)
  if ob:ob.data=mesh
  else:ob=bpy.data.objects.new(name,mesh);scene.collection.objects.link(ob)
  ob.matrix_world.identity();return ob

# The former ribbon bridged the actual incision with long flat facets. Copy the
# actual ground faces beneath its footprint, preserving their exact topology.
new_floor=Mesh();used_faces=0;main_face_ids=set()
for poly in terrain.data.polygons:
 p=terrain.matrix_world@poly.center
 if not(lo[0]-10<=p.x<=hi[0]+10 and lo[1]-10<=p.y<=hi[1]+10):continue
 points=[world_vertices[i] for i in poly.vertices]
 if not inside(old_floor_mask,p.x,p.y) and not any(inside(old_floor_mask,q.x,q.y) for q in points):continue
 new_floor.face([(q.x,q.y,q.z+.015) for q in points],used_faces%4);used_faces+=1;main_face_ids.add(poly.index)
assert used_faces>50
new_floor.install('Breach floor copied from actual incised terrain',floor)
outer=bpy.data.objects['BLD_BreachOutlyingFractures'];outer_mask=footprint(outer)
outer_points=[outer.matrix_world@v.co for v in outer.data.vertices]
outer_lo=np.min(np.array(outer_points),axis=0);outer_hi=np.max(np.array(outer_points),axis=0)
new_outer=Mesh();outer_faces=0
for poly in terrain.data.polygons:
 if poly.index in main_face_ids:continue
 p=terrain.matrix_world@poly.center
 if -96<p.x<96 and -78<p.y<270:continue
 if not(outer_lo[0]-10<=p.x<=outer_hi[0]+10 and outer_lo[1]-10<=p.y<=outer_hi[1]+10):continue
 points=[world_vertices[i] for i in poly.vertices]
 if not inside(outer_mask,p.x,p.y) and not any(inside(outer_mask,q.x,q.y) for q in points):continue
 new_outer.face([(q.x,q.y,q.z+.015) for q in points],outer_faces%4);outer_faces+=1
assert outer_faces>50
new_outer.install('Outer fault substrate copied from actual ground',outer)

# Voronoi fracture cells replace the smooth luminous puddle. Raised plates stay
# within the already excluded fault belt; outer staining remains ground-bound.
rng=np.random.default_rng(261025)
seeds=[(-260.,150.)]
for _ in range(110):seeds.append((-300+80*rng.random(),110+80*rng.random()))
def clip(poly,a,b,c):
 result=[]
 for p,q in zip(poly,poly[1:]+poly[:1]):
  dp=a*p[0]+b*p[1]-c;dq=a*q[0]+b*q[1]-c
  if dp<=0:result.append(p)
  if (dp<=0)!=(dq<=0):
   t=dp/(dp-dq);result.append((p[0]+t*(q[0]-p[0]),p[1]+t*(q[1]-p[1])))
 return result
layout=json.loads((source.parent/'production-region.json').read_text(encoding='utf-8-sig'))
def fault_distance(x,y):
 best=1e9
 for p in layout['faults']:
  ax,ay,bx,by=p['ax'],p['az'],p['bx'],p['bz'];dx,dy=bx-ax,by-ay
  t=max(0,min(1,((x-ax)*dx+(y-ay)*dy)/max(.00001,dx*dx+dy*dy)))
  best=min(best,math.hypot(x-ax-t*dx,y-ay-t*dy))
 return best
underlay=Mesh();plates=Mesh();cells=0;raised=0
for i,(sx,sy) in enumerate(seeds):
 if not inside(old_heart_mask,sx,sy):continue
 poly=[(-300.,110.),(-220.,110.),(-220.,190.),(-300.,190.)]
 for j,(tx,ty) in enumerate(seeds):
  if i==j:continue
  poly=clip(poly,tx-sx,ty-sy,(tx*tx+ty*ty-sx*sx-sy*sy)/2)
  if len(poly)<3:break
 if len(poly)<3:continue
 cx=sum(p[0] for p in poly)/len(poly);cy=sum(p[1] for p in poly)/len(poly)
 if not inside(old_heart_mask,cx,cy):continue
 core=math.hypot((cx+260)/31,(cy-150)/29)
 slot=6 if core<.29 else 5 if core<.76 else 7
 centre=(cx,cy,height(cx,cy)+.020)
 for a,b in zip(poly,poly[1:]+poly[:1]):underlay.face([centre,(a[0],a[1],height(*a)+.020),(b[0],b[1],height(*b)+.020)],slot)
 inset=.83+.10*rng.random();ring=[(cx+(x-cx)*inset,cy+(y-cy)*inset) for x,y in poly]
 safe=fault_distance(cx,cy)<23 and all(fault_distance(x,y)<26 for x,y in ring)
 lift=(.25+1.05*rng.random()) if safe else .055
 raised+=int(safe);cells+=1
 top=[Vector((x,y,height(x,y)+lift*(.8+.2*math.sin(x*.2+y*.1)))) for x,y in ring]
 bottom=[Vector((x,y,height(x,y)-.45)) for x,y in ring]
 midpoint=sum(top,Vector())/len(top);colour=cells%4
 for k in range(len(top)):
  n=(k+1)%len(top);plates.face([midpoint,top[k],top[n]],colour)
  plates.face([top[k],bottom[k],bottom[n],top[n]],4)
assert cells>35 and raised>15,(cells,raised)
underlay.install('Breach narrow exposed mineral beneath real fracture plates',heart)
slabs=plates.install('BLD_BreachBrokenBasaltPlates');exported.append(slabs)

assert mesh_digest(terrain.data)==terrain_before,'Protected terrain geometry, UV or material slots changed'
assert all(hashlib.sha256(bpy.data.images[name].packed_file.data).hexdigest()==sha for name,sha in images_before.items()),'An existing source image changed'
terrain_after=mesh_digest(terrain.data)
gap=max(abs(p[2]-height(p[0],p[1])-.015) for p in new_floor.v)
assert gap<.001,gap

# Actual Blender review of the edited world, not Unity evidence. Restore every
# existing camera/light/LOD visibility before saving the editable source.
camera_before=scene.camera;hidden={ob:ob.hide_render for ob in scene.objects}
for ob in scene.objects:
 if ob.name.startswith(('BLD_ForestFar_','Far_Pilot_Pine')):ob.hide_render=True
 if ob.type=='LIGHT':ob.hide_render=True
camdata=bpy.data.cameras.new('Breach workshop comparison');cam=bpy.data.objects.new('Breach workshop comparison',camdata);scene.collection.objects.link(cam);scene.camera=cam;camdata.type='ORTHO'
sun_data=bpy.data.lights.new('Breach workshop sun','SUN');sun=bpy.data.objects.new('Breach workshop sun',sun_data);scene.collection.objects.link(sun);sun_data.energy=2
sun.rotation_euler=Vector((.384,-.548,.743)).to_track_quat('Z','Y').to_euler()
scene.render.engine='BLENDER_EEVEE';scene.render.image_settings.file_format='PNG';scene.render.resolution_percentage=100
for label,scale,offset in [('near',128,(0,-104,94)),('regional',680,(0,-320,650))]:
 focus=Vector((-260,150,-3));cam.location=focus+Vector(offset);cam.rotation_euler=(focus-cam.location).to_track_quat('-Z','Y').to_euler();camdata.ortho_scale=scale
 scene.render.resolution_x=1280;scene.render.resolution_y=720;scene.render.filepath=str(out/('breach-ground-'+label+'-blender.png'));bpy.ops.render.render(write_still=True)
scene.camera=camera_before
bpy.data.objects.remove(cam,do_unlink=True);bpy.data.objects.remove(sun,do_unlink=True)
for ob,state in hidden.items():ob.hide_render=state
bpy.ops.object.select_all(action='DESELECT')
for ob in exported:ob.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True)
report={'method':'Actual BlenderMCP execute_code of directed ground repair','source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'terrain_before':terrain_before,'terrain_after':terrain_after,'original_packed_images_unchanged':len(images_before),'copied_actual_terrain_faces':used_faces,'copied_outer_fault_ground_faces':outer_faces,'fractured_plates':cells,'raised_only_inside_existing_fault_exclusion':raised,'floor_maximum_ground_gap_error_m':gap,'floor_z_bounds':[min(p[2] for p in new_floor.v),max(p[2] for p in new_floor.v)],'floor_xy_bounds':[[min(p[c] for p in new_floor.v),max(p[c] for p in new_floor.v)] for c in [0,1]],'new_gameplay_targets':0,'colliders_added':0,'preview_is_Unity':False,'art_approved':False,'output_files':{n:hashlib.sha256((out/n).read_bytes()).hexdigest() for n in ['RegionOneBlenderWorld.glb','Eldoria_Region1_Production.blend']}}
(out/'geological-ground-authoring.json').write_text(json.dumps(report,indent=2));print('BREACH_ACTUAL_GROUND_REAUTHORING_SAVED',json.dumps(report));(out/'stop.request').write_text('Directed ground repair saved; close owned Blender')
