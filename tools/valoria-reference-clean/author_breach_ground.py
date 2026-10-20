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
colors=[(.10,.085,.105),(.18,.145,.18),(.31,.26,.30),(.37,.065,.49),(.76,.30,.87),(.38,.29,.18),(.48,.37,.23)]
mats=[]
for i,c in enumerate(colors):
 m=bpy.data.materials.new(['Altered bedrock','Fresh fracture','Weathered fracture edge','Violet corruption vein','Contained fissure core','Compacted workground','Sawdust workground'][i]);m.diffuse_color=(*c,1);m.use_nodes=True;p=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=.91
 if i in (3,4):p.inputs['Emission Color'].default_value=(*c,1);p.inputs['Emission Strength'].default_value=.65
 mats.append(m)
models=[]
for index,(ident,radius,uplift) in enumerate([('corrupt-scout',2.7,.48),('engendro-valoria',3.8,1.65)]):
 node=next(n for n in world['nodes'] if n['id']==ident)['position'];cx,cz=node['x'],node['z'];rng=random.Random(211+index);vv=[];ff=[];ss=[]
 def add(points,faces,slot):
  offset=len(vv);vv.extend(points);ff.extend([tuple(offset+j for j in f) for f in faces]);ss.extend([slot]*len(faces))
 # Polygon sectors are individually lifted fracture plates. Their irregular
 # stepped radial profiles close below the existing terrain, with no cliff rim.
 count=19
 for i in range(count):
  a=math.tau*i/count+.14*math.sin(i*1.7);b=math.tau*(i+1)/count+.14*math.sin((i+1)*1.7);gap=.018
  v=[]
  for ring,r in enumerate([.08,.32,.58,.83,1.0]):
   for angle in [a+gap,b-gap]:
    rr=radius*r*(1+.10*math.sin(angle*3+index));x=cx+math.cos(angle)*rr;z=cz+math.sin(angle)*rr
    height=gy(x,z)+(.045 if ring==4 else uplift*(1-r)**1.2*(.45+.55*abs(math.sin(angle*2.4+.7))))
    if ring==0:height-=.11
    v.append((x,-z,height))
  faces=[]
  for j in range(4):faces.extend([(j*2,j*2+1,j*2+2),(j*2+1,j*2+3,j*2+2)])
  add(v,faces,i%3)
  # Actual downturned broken plate sides meet the measured terrain.
  side=[v[j] for j in [0,2,4,6,8]];base=[(x,y,gy(x,-y)-.06) for x,y,h in side]
  add(side+base,[(j,j+1,j+6,j+5) for j in range(4)],0)
 # Branching light is restricted to cracks, with no particles/volumetric fog.
 for branch in range(3 if index==0 else 5):
  angle=branch*math.tau/(3 if index==0 else 5)+index*.4;points=[]
  for k in range(13):
   t=k/12;r=radius*(.06+.78*t);a=angle+.16*math.sin(k*1.6+branch);x=cx+math.cos(a)*r;z=cz+math.sin(a)*r;height=gy(x,z)+.065+uplift*(1-r/radius)**1.2*.28
   width=(.025 if index==0 else .045)*(1-.7*t);points.extend([(x-math.sin(a)*width,-z-math.cos(a)*width,height),(x+math.sin(a)*width,-z+math.cos(a)*width,height)])
  add(points,[(2*k,2*k+1,2*k+3,2*k+2) for k in range(12)],3 if branch%2 else 4)
 mesh=bpy.data.meshes.new(ident+' measured fractured ground');mesh.from_pydata(vv,[],ff);mesh.update()
 for m in mats:mesh.materials.append(m)
 for p,slot in zip(mesh.polygons,ss):p.material_index=slot
 bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.triangulate(bm,faces=bm.faces);bm.to_mesh(mesh);bm.free();mesh.update();obj=bpy.data.objects.new(mesh.name,mesh);bpy.context.collection.objects.link(obj)
 vertices=[];normals=[];uv=[];subs=[[] for m in mats]
 for p in mesh.polygons:
  for vi in p.vertices:
   v=mesh.vertices[vi].co;n=p.normal;vertices.append({'x':v.x,'y':v.z,'z':-v.y});normals.append({'x':n.x,'y':n.z,'z':-n.y});uv.append({'x':v.x*.7,'y':-v.y*.7});subs[p.material_index].append(len(vertices)-1)
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
(EV/'breach-author-proof.json').write_text(json.dumps({'source_world_survey_sha256':hashlib.sha256((ROOT/'docs/evidence/valoria-reference-clean/world204/survey.json').read_bytes()).hexdigest(),'blend_sha256':hashlib.sha256((EV/'BreachGround211.blend').read_bytes()).hexdigest(),'mesh_sha256':hashlib.sha256((EV/'breach-ground.json').read_bytes()).hexdigest(),'triangles':[len(m['vertices'])//3 for m in models],'new_settlements':False,'gameplay_nodes_untouched':True,'art_approved':False,'operations':['measured heightfield contact','irregular fracture plates','closed broken sides','narrow branching corruption seams'],'lore':'v0220 altered matter and violet/black fissure signature; no camps, monuments or new enemy rules'},indent=2))
print('VALORIA_BREACH_GROUND_AUTHOR_COMPLETE')
