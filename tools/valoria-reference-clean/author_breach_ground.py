"""Author existing Corrupt/Engendro terrain scars, not camps or new lore structures.
Source ground is measured Unity geometry; native Unity trial approves the result.
"""
import bpy,bmesh,json,math,pathlib,hashlib,random
from mathutils import Vector
ROOT=pathlib.Path.cwd();EV=ROOT/'docs/evidence/valoria-reference-clean/finish211';EV.mkdir(parents=True,exist_ok=True)
world=json.loads((ROOT/'docs/evidence/valoria-reference-clean/world204/survey.json').read_text());ground=world['ground']['vertices'];samples={(round(v['x'],3),round(v['z'],3)):v['y'] for v in ground}
def gy(x,z):
 ix=math.floor((x-880)/2)*2+880;iz=math.floor((z+120)/2)*2-120;u=(x-ix)/2;v=(z-iz)/2
 return samples[(ix,iz)]*(1-u)*(1-v)+samples[(ix+2,iz)]*u*(1-v)+samples[(ix,iz+2)]*(1-u)*v+samples[(ix+2,iz+2)]*u*v
bpy.ops.wm.read_factory_settings(use_empty=True)
colors=[(.22,.19,.25),(.29,.25,.32),(.35,.31,.38),(.19,.035,.25),(.48,.12,.60),(.38,.29,.18),(.48,.37,.23)]
mats=[]
for i,c in enumerate(colors):
 m=bpy.data.materials.new(['Altered bedrock','Fresh fracture','Weathered fracture edge','Violet corruption vein','Contained fissure core','Compacted workground','Sawdust workground'][i]);m.diffuse_color=(*c,1);m.use_nodes=True;p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=.91
 if i in (3,4):p.inputs['Emission Color'].default_value=(*c,1);p.inputs['Emission Strength'].default_value=.65
 mats.append(m)
models=[]
for index,(ident,radius,uplift) in enumerate([('corrupt-scout',2.7,.30),('engendro-valoria',3.8,.72)]):
 node=next(n for n in world['nodes'] if n['id']==ident)['position'];cx,cz=node['x'],node['z'];rng=random.Random(211+index);vv=[];ff=[];ss=[]
 def add(points,faces,slot):
  offset=len(vv);vv.extend(points);ff.extend([tuple(offset+j for j in f) for f in faces]);ss.extend([slot]*len(faces))
 # Two fractured curved scarps flank an irregular narrow ground fissure.
 # The rejected native211 radial fan is replaced outright: no cone or disk.
 N=45;B=12
 crease=[rng.uniform(-.28,.28) for _ in range(12)]
 def line(t):
  u=(t+1)*5.5;i=min(10,int(u));return crease[i]+(crease[i+1]-crease[i])*(u-i)
 for side in (-1,1):
  points=[]
  for k in range(N):
   t=-1+2*k/(N-1);x=cx+radius*t;zmid=cz+line(t);fade=max(0,1-t*t)**.7
   for j in range(B):
    q=j/(B-1);width=(.48*radius+.15*math.sin(t*11))*fade
    z=zmid+side*(.24*fade+width*q)
    rise=uplift*(.24 if side==-1 else 1)*fade*(1-q)**1.3*(.68+.10*math.sin(k*.35+j*.8))
    fracture=.055*fade*math.sin(k*2.4+j*1.7)*math.sin(math.pi*q)
    height=gy(x,z)+rise+fracture+.035-.075*q*q
    points.append((x,-z,height))
  faces=[]
  for k in range(N-1):
   for j in range(B-1):
    a=k*B+j;b=a+1;c=a+B;d=c+1
    if side==-1:faces.extend([(a,c,b),(b,c,d)])
    else:faces.extend([(a,b,c),(b,d,c)])
  add(points,faces,1 if side==1 else 2)
  # Dark inner broken wall closes down to the actual source ground.
  top=[points[k*B] for k in range(N)];bottom=[(x,y,gy(x,-y)+.018) for x,y,h in top]
  add(top+bottom,[(k,k+1,k+1+N,k+N) for k in range(N-1)],0)
 # Recessed vein is wide enough to read at local zoom, remains contained.
 for slot,wide,lift in [(0,.24,.035),(3,.105,.052),(4,.027,.061)]:
  points=[]
  for k in range(N):
   t=-1+2*k/(N-1);fade=max(0,1-t*t)**.7;x=cx+radius*t;z=cz+line(t);w=wide*fade*(.75+.25*math.sin(k*.81)**2)
   points.extend([(x,-z-w,gy(x,z)+lift),(x,-z+w,gy(x,z)+lift)])
  add(points,[(2*k,2*k+1,2*k+3,2*k+2) for k in range(N-1) if slot==0 or k%11 not in (0,1)],slot)
 mesh=bpy.data.meshes.new(ident+' measured fractured ground');mesh.from_pydata(vv,[],ff);mesh.update()
 for m in mats:mesh.materials.append(m)
 for p,slot in zip(mesh.polygons,ss):p.material_index=slot
 bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.triangulate(bm,faces=bm.faces);bm.to_mesh(mesh);bm.free();mesh.update();obj=bpy.data.objects.new(mesh.name,mesh);bpy.context.collection.objects.link(obj)
 vertices=[];normals=[];uv=[];subs=[[] for m in mats]
 for p in mesh.polygons:
  for vi in p.vertices:
   v=mesh.vertices[vi].co;n=p.normal;vertices.append({'x':v.x,'y':v.z,'z':-v.y});normals.append({'x':n.x,'y':n.z,'z':-n.y});axis=max(range(3),key=lambda a:abs(n[a]));axes=[a for a in range(3) if a!=axis];uv.append({'x':v[axes[0]]*.7,'y':v[axes[1]]*.7});subs[p.material_index].append(len(vertices)-1)
 models.append({'name':ident,'vertices':vertices,'normals':normals,'uv':uv,'submeshes':[{'indices':s} for s in subs]})
# Small irregular contact aprons belong to already-unlocked functional models.
# Their final vertices are projected onto the CURRENT city Terrain in Unity.
city=json.loads((EV/'survey.json').read_text())
for ident,slot in [('sawmill',6),('barracks',5)]:
 lo=city[ident+'_min'];hi=city[ident+'_max'];cx=(lo['x']+hi['x'])/2;cz=(lo['z']+hi['z'])/2
 rx=(hi['x']-lo['x'])*.57;rz=(hi['z']-lo['z'])*.62;v=[];f=[];N=36
 for j,r in enumerate([0,.24,.52,.78,1]):
  for k in range(N):
   a=k*math.tau/N;edge=1+.045*math.sin(k*2.4)+.035*math.cos(k*1.7);x=cx+math.cos(a)*rx*r*edge;z=cz+math.sin(a)*rz*r*edge;v.append((x,-z,lo['y']+.025 if j<4 else lo['y']-.015))
 for j in range(4):
  for k in range(N):
   a=j*N+k;b=j*N+(k+1)%N;c=a+N;d=b+N;f.extend([(a,b,c),(b,d,c)])
 mesh=bpy.data.meshes.new(ident+' irregular functional contact apron');mesh.from_pydata(v,[],f);mesh.update();obj=bpy.data.objects.new(mesh.name,mesh);bpy.context.collection.objects.link(obj)
 for m in mats:mesh.materials.append(m)
 vertices=[];normals=[];uv=[];subs=[[] for m in mats]
 for poly in mesh.polygons:
  poly.material_index=slot
  for vi in poly.vertices:
   x,y,z=mesh.vertices[vi].co;vertices.append(dict(x=x,y=z,z=-y));normals.append(dict(x=0,y=1,z=0));uv.append(dict(x=x*.4,y=-y*.4));subs[slot].append(len(vertices)-1)
 models.append(dict(name=ident,vertices=vertices,normals=normals,uv=uv,submeshes=[dict(indices=ss) for ss in subs]))
bpy.context.preferences.filepaths.save_version=0;bpy.ops.wm.save_as_mainfile(filepath=str(EV/'BreachGround211.blend'),compress=True)
(EV/'breach-ground.json').write_text(json.dumps({'variants':models,'colors':[dict(r=r,g=g,b=b,a=1) for r,g,b in colors]},separators=(',',':')))
(EV/'breach-author-proof.json').write_text(json.dumps({'source_world_survey_sha256':hashlib.sha256((ROOT/'docs/evidence/valoria-reference-clean/world204/survey.json').read_bytes()).hexdigest(),'blend_sha256':hashlib.sha256((EV/'BreachGround211.blend').read_bytes()).hexdigest(),'mesh_sha256':hashlib.sha256((EV/'breach-ground.json').read_bytes()).hexdigest(),'triangles':[len(m['vertices'])//3 for m in models],'new_settlements':False,'gameplay_nodes_untouched':True,'art_approved':False,'operations':['measured heightfield contact','curved opposed geological scarps','closed broken inner walls','recessed narrow corruption seam'],'lore':'v0220 altered matter and violet/black fissure signature; no camps, monuments or new enemy rules'},indent=2))
print('VALORIA_BREACH_GROUND_AUTHOR_COMPLETE')
