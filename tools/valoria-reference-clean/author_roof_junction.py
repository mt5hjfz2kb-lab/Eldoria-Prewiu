"""Repair measured existing Bastion roof/tower intersections, preserving I–II.

Source: exact current Unity mesh survey. Fit the existing tiled hall roof between the established side towers,
clip the inherited roof out of tower interiors, retain II's original hip caps.
Editable Blender sources preserve original and candidate geometry/materials.
"""
import bpy, json, math, hashlib, pathlib, gzip, shutil
import numpy as np
ROOT=pathlib.Path.cwd()
SRC=ROOT/'docs/evidence/valoria-reference-clean/world-contact222/roof-source-survey.json.gz'
EV=ROOT/'docs/evidence/valoria-reference-clean/roof-junction226'
EV.mkdir(parents=True,exist_ok=True)
# Preserve the rejected editable hip source and proof once; reviewed screenshots
# remain in immutable native230 artifact12294402736.
old=EV/'BastionRoofJunctions.blend'
if old.exists() and hashlib.sha256(old.read_bytes()).hexdigest()=='ec9ca3c1471903782b4316abd1e4fa9b93a2d387148ba923c197a31af635529b':
 rejected=EV/'RejectedHip230';rejected.mkdir(exist_ok=True)
 for name in ['BastionRoofJunctions.blend','author-proof.json']:
  if not (rejected/name).exists():shutil.copy2(EV/name,rejected/name)

source=json.loads(gzip.decompress(SRC.read_bytes()).decode('utf-8-sig'))
assert {p['name'] for p in source['parts']}=={'Bastion_I','Bastion_II'}
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def vectors(a,n=3): return np.array([[v[k] for k in ('x','y','z')[:n]] for v in a],dtype=float)
def key(p): return tuple(np.round(p,4))
first=next(p for p in source['parts'] if p['name']=='Bastion_I')
iv=vectors(first['world_vertices'])
main=set()
for slot in [8,9,10]:
 for tri in np.array(first['submeshes'][slot]['indices']).reshape(-1,3):
  if iv[tri,1].max()<10: main.update(key(v) for v in iv[tri])
assert len(main)>1000
# World coordinates are measured from the current integrated mesh, and checked
# against the retained authoring dimensions before any shaping takes place.
assert -9.1<min(p[0] for p in main)<-8.8
assert 42.9<max(p[2] for p in main)<43.1
assert 9.7<max(p[1] for p in main)<9.9
def split(poly,axis,value,sign):
 inside=[];outside=[]
 for a,b in zip(poly,poly[1:]+poly[:1]):
  da=sign*(a[axis]-value);db=sign*(b[axis]-value)
  (inside if da>=0 else outside).append(a)
  if (da>=0)!=(db>=0):
   q=a+(b-a)*(da/(da-db));inside.append(q);outside.append(q)
 return inside,outside
def subtract(poly,box):
 result=[];remaining=poly
 for axis,value,sign in [(0,box[0],1),(0,box[1],-1),(2,box[2],1),(2,box[3],-1)]:
  if not remaining:break
  remaining,out=split(remaining,axis,value,sign)
  if len(out)>=3:result.append(out)
 return result
boxes=[(-10.2,-6.6,32.3,37.7),(6.6,10.2,32.3,37.7)]
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'docs/evidence/valoria-reference-clean/BastionII.blend'),use_scripts=False)
for o in bpy.context.scene.objects:o.hide_render=True
material_names=['Recessed stone joints','Trial sandstone 3','Trial sandstone 0','Trial sandstone 4','Trial sandstone 5','Trial sandstone 2','Trial sandstone 1','Carved limestone','Blue slate 2','Blue slate 3','Blue slate 1','Oak','Warm limestone 3','Heraldic gold','Royal blue cloth','Bastion recess shadow','Forged iron','Torch orange envelope','Torch golden core']
materials=[bpy.data.materials[n] for n in material_names]
results=[];proof=[];exports=[];editable=[]
for part in source['parts']:
 v=vectors(part['world_vertices']);norm=vectors(part['normals']);uv=vectors(part['uv'],2)
 uv2=vectors(part['uv2'],2) if len(part['uv2'])==len(v) else np.zeros((len(v),2))
 assert len(part['submeshes'])==19 and len(v)==len(norm)==len(uv)
 original=np.column_stack([v,norm,uv,uv2]);out=original.tolist();groups=[];changed=0;removed=0;protected=set()
 for slot,sub in enumerate(part['submeshes']):
  indices=[]
  for tri in np.array(sub['indices']).reshape(-1,3):
   pts=v[tri]
   slate=slot in [8,9,10] and all(key(p) in main for p in pts)
   timber=slot==11 and pts[:,1].min()>5.6 and pts[:,1].max()<10 and pts[:,2].min()>31.7 and pts[:,2].max()<43.3
   gable=slot==12 and np.ptp(pts[:,0])<.002 and abs(abs(pts[0,0])-8.5)<.002 and pts[:,1].min()>5.7 and pts[:,1].max()<10
   if not(slate or timber or gable):indices.extend(int(i) for i in tri);protected.update(int(i) for i in tri);continue
   poly=[a.copy() for a in original[tri]]
   for a in poly:a[0]*=.70
   polygons=[poly]
   changed+=1
   if not polygons:removed+=1
   for poly in polygons:
    for k in range(1,len(poly)-1):
     t=[poly[0].copy(),poly[k].copy(),poly[k+1].copy()]
     n=np.cross(t[1][:3]-t[0][:3],t[2][:3]-t[0][:3]);length=np.linalg.norm(n)
     if length<1e-9:continue
     n/=length
     for q in t:q[3:6]=n;indices.append(len(out));out.append(q.tolist())
  groups.append(indices)
 # A dressed walkable-looking terrace closes the existing hollow tower rim.
 # It is visual only, nested in both established I–II models; no new collider.
 decks=[(-9.95,-6.85,32.6,37.4,7.19,7.25),(6.85,9.95,32.6,37.4,7.19,7.25),(-8.5,-5.9,37.4,42.5,5.76,5.83),(5.9,8.5,37.4,42.5,5.76,5.83)]
 for x0,x1,z0,z1,y0,y1 in decks:
  corners=[(x0,y0,z0),(x1,y0,z0),(x1,y0,z1),(x0,y0,z1)]
  top=[(a,y1,c) for a,b,c in corners];points=corners+top
  for face in [(4,7,6,5),(0,1,2,3),(0,4,5,1),(1,5,6,2),(2,6,7,3),(3,7,4,0)]:
   for ids in [(face[0],face[1],face[2]),(face[0],face[2],face[3])]:
    p=np.array([points[i] for i in ids]);n=np.cross(p[1]-p[0],p[2]-p[0]);n/=np.linalg.norm(n)
    for a in p:groups[7].append(len(out));out.append(list(a)+list(n)+[a[0]/1.5,a[2]/1.5,0,0])
 used=sorted(set(i for g in groups for i in g));remap={old:new for new,old in enumerate(used)}
 out=np.array(out)[used];groups=[[remap[i] for i in g] for g in groups]
 protected_ids=sorted(protected);assert np.array_equal(original[protected_ids],out[[remap[i] for i in protected_ids]])
 protected_hash=hashlib.sha256(original[protected_ids].tobytes()).hexdigest()
 def js(a):return [dict(zip(('x','y','z')[:len(row)],map(float,row))) for row in a]
 result={'name':part['name'],'vertices':js(out[:,:3]),'normals':js(out[:,3:6]),'uv':js(out[:,6:8]),'uv2':js(out[:,8:10]),'submeshes':[{'indices':g} for g in groups]}
 results.append(result);proof.append({'name':part['name'],'protected_source_vertices':len(protected),'protected_source_sha256':protected_hash,'source_vertices':len(v),'candidate_vertices':len(out),'affected_source_triangles':changed,'removed_intersection_triangles':removed,'source_triangles':sum(len(s['indices'])//3 for s in part['submeshes']),'candidate_triangles':sum(len(g)//3 for g in groups)})
 assert changed>1000 and len(out)<len(v)*1.6
 for label,vertices,gg in [('SOURCE',v,[s['indices'] for s in part['submeshes']]),('CANDIDATE',out[:,:3],groups)]:
  mesh=bpy.data.meshes.new(part['name']+' '+label);faces=[tuple(t) for g in gg for t in np.array(g).reshape(-1,3)]
  mesh.from_pydata([(float(p[0]),float(-p[2]),float(p[1])) for p in vertices],[],faces);mesh.update();obj=bpy.data.objects.new(mesh.name,mesh);bpy.context.collection.objects.link(obj)
  for m in materials:mesh.materials.append(m)
  start=0
  for slot,g in enumerate(gg):
   for p in mesh.polygons[start:start+len(g)//3]:p.material_index=slot
   start+=len(g)//3
  layer=mesh.uv_layers.new(name='Retained source UV');uvs=uv if label=='SOURCE' else out[:,6:8]
  for loop in mesh.loops:layer.data[loop.index].uv=uvs[loop.vertex_index]
  editable.append(obj)
  obj.hide_render=label=='SOURCE' or part['name']=='Bastion_I'
  if label=='CANDIDATE':exports.append(obj)
for obj in list(bpy.data.objects):
 if obj not in editable:bpy.data.objects.remove(obj,do_unlink=True)
bpy.data.orphans_purge(do_recursive=True)
for image in bpy.data.images:
 if image.source=='FILE' and image.has_data:image.pack()
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(EV/'BastionRoofJunctions.blend'),compress=True)
for obj in exports:
 bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);bpy.context.view_layer.objects.active=obj
 bpy.ops.export_scene.gltf(filepath=str(EV/(obj.name.split()[0]+'_RoofJunction.glb')),export_format='GLB',use_selection=True)
(EV/'candidate.json.gz').write_bytes(gzip.compress(json.dumps({'survey_sha256':sha(SRC),'source_scene_sha256':source['scene_sha256'],'parts':results},separators=(',',':')).encode('utf-8'),mtime=0))
(EV/'author-proof.json').write_text(json.dumps({'source_survey_sha256':sha(SRC),'candidate_sha256':sha(EV/'candidate.json.gz'),'blend_sha256':sha(EV/'BastionRoofJunctions.blend'),'glb_sha256':{f.name:sha(f) for f in EV.glob('*_RoofJunction.glb')},'operations':['Measured affine hall-roof width fit between existing tower inner walls','Original slate slopes and closed tile geometry preserved','Existing gables fitted with roof; closed stone side seats on existing hall walls','Retained II hip caps','Closed dressed tower terrace','Original UV and protected geometry retention'],'parts':proof,'art_approved':False},indent=2))
print('VALORIA_REFERENCE_CLEAN_ROOF_JUNCTION_BLENDER_COMPLETE')
