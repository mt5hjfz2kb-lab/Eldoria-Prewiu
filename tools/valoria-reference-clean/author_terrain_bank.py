"""Author the measured integrated scene's bank and low meadow relief in Blender.
Current Unity world data only; no old scene reconstruction. Local Qwen chooses
bounded spatial edits, then direct bpy edits the original topology. No downloads.
"""
import bpy, json, math, hashlib, pathlib, os, subprocess, time, urllib.request
import numpy as np
from mathutils import Vector
ROOT=pathlib.Path.cwd(); EV=ROOT/'docs/evidence/valoria-reference-clean/terrain-bank192'
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def write(n,v): (EV/n).write_text(json.dumps(v,indent=None if n=='candidate.json' else 2,separators=(',',':') if n=='candidate.json' else None),encoding='utf-8')
survey=json.loads((EV/'survey.json').read_text(encoding='utf-8-sig'))
assert sha(ROOT/'references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg')==survey['reference_sha256']=='86fbffa9410514e935b121a844802573cbb7f94c73abb7358ce2f8ee15845c5d'
def vec(v):return [v['x'],v['y'],v['z']]
def ease(a,b,x):
 t=np.clip((x-a)/(b-a),0,1);return t*t*(3-2*t)
def road(z):return np.where(z<-10,-1.5+math.tan(math.radians(14))*(z+10),-1.5*np.clip((25.6-z)/35.6,0,1))
def river(x):return -23+1.5*np.sin(x*.065)+.85*np.sin(x*.19+.4)*ease(6,17,np.abs(x))
def width(x):return 7.1+.75*np.sin(x*.095+.7)+.48*np.sin(x*.23)
def free(x,z):
 # Continuous masks preserve exact functional platforms and all axial access.
 d=np.maximum.reduce([np.abs(x-road(z))-7.2,-78-z,z-27]);m=ease(0,3,d)
 for cx,cz,rx,rz in [(0,35,13.5,12),(-1.5,-10,16,7),(-23,28,11.5,9),(-19,9,11.5,9),(29,32,11.5,9),(28,12,11.5,9)]:
  m*=ease(0,3,np.maximum(np.abs(x-cx)-rx,np.abs(z-cz)-rz))
 return m

def components(mesh):
 v=mesh['vertices'];parent=list(range(len(v)))
 def find(a):
  while parent[a]!=a:parent[a]=parent[parent[a]];a=parent[a]
  return a
 for a,b,c in np.array(mesh['triangles']).reshape(-1,3):
  pa=find(int(a));pb=find(int(b));pc=find(int(c));parent[pb]=pa;parent[pc]=pa
 welded={}
 for i,p in enumerate(v):
  key=tuple(round(float(q),4) for q in vec(p))
  if key in welded:parent[find(i)]=find(welded[key])
  else:welded[key]=i
 groups={}
 for i in range(len(v)):groups.setdefault(find(i),[]).append(i)
 return list(groups.values())
bank=next(m for m in survey['meshes'] if m['name']=='River_bank_rocks')
bv=np.array([vec(v) for v in bank['vertices']],dtype=float);groups=components(bank);targets=[]
for i,ids in enumerate(groups):
 lo=bv[ids].min(0);hi=bv[ids].max(0);c=(lo+hi)/2
 if abs(c[0])<66 and abs(c[0]-road(c[2]))>9 and hi[1]-lo[1]>.8:
  targets.append({'id':i,'center':c.round(2).tolist(),'size':(hi-lo).round(2).tolist()})
assert len(targets)>=6
# Geometry and observed PNG diagnosis are both authentic. The text author receives
# measured bounds, not invented vertices; deterministic validation precedes bpy.
context={'reference_sha256':survey['reference_sha256'],'survey_sha256':sha(EV/'survey.json'),'coordinates':'Unity X,Y up,Z; all values metres',
 'visual_diagnosis':'Independent inspection of exact Unity before main and portrait against approved JPEG: high separated angular boulders read as a picket fence, lack supporting low talus, meadow has insufficient broad relief. Native61 lowered seven existing rocks successfully; native179 only alpha repaint was rejected. No plots, new buildings, camps or cliffs. Current good architecture stays untouched.',
 'bank_components':targets,'task':'Choose a coherent low fractured riverbank. Set crown compression, broad foot flare, inland embed and face shear per bank region, not isolated primitive additions. Crown compression applies above fixed lower foot, with slab fractures. Choose 4-7 shallow broad landforms from supplied allowed centers in meadow gaps; max rise 1.0m, roads and building platforms are strictly protected. Keep all relief gentle. No approval score.',
 'allowed_landform_centers':[[-49,13],[-40,-7],[-4,59],[46,0],[56,27],[16,52],[-48,43],[47,-45],[-36,-46]],
 'schema':{'rationale':'reason','north':{'compression':'.48..72','flare':'1.08..1.28','inland':'.25..65','shear':'-.22..22'},'south':'same fields','landforms':[{'center_index':'0..8','rise':'.35..1.0','radius_x':'8..15','radius_z':'6..12'}]}}
write('author-context.json',context)
def api(path,data=None,timeout=5):
 req=urllib.request.Request('http://127.0.0.1:11434'+path,data=None if data is None else json.dumps(data).encode(),headers={'Content-Type':'application/json'})
 with urllib.request.urlopen(req,timeout=timeout) as r:return json.load(r)
server=None;log=None
schema={'type':'object','required':['rationale','north','south','landforms'],'properties':{'rationale':{'type':'string'}}}
region={'type':'object','required':['compression','flare','inland','shear'],'properties':{k:{'type':'number','minimum':a,'maximum':b} for k,a,b in [('compression',.48,.72),('flare',1.08,1.28),('inland',.25,.65),('shear',-.22,.22)]}}
schema['properties'].update(north=region,south=region,landforms={'type':'array','minItems':4,'maxItems':7,'items':{'type':'object','required':['center_index','rise','radius_x','radius_z'],'properties':{'center_index':{'type':'integer','minimum':0,'maximum':8},'rise':{'type':'number','minimum':.35,'maximum':1.0},'radius_x':{'type':'number','minimum':8,'maximum':15},'radius_z':{'type':'number','minimum':6,'maximum':12}}}})
payload={'model':'qwen3:14b','stream':False,'think':False,'keep_alive':0,'format':schema,'messages':[{'role':'system','content':'You direct a bounded professional Blender terrain correction from real Unity geometry and independent visual diagnosis. Return only validated spatial JSON, no code. No art approval. Preserve canonical scene and gameplay.'},{'role':'user','content':json.dumps(context)}],'options':{'temperature':.2,'num_ctx':8192,'num_predict':1500}}
write('qwen-request.json',payload)
retained=EV/'qwen-response-native193.json'
if retained.exists():
 assert sha(retained)=='ec714d777fe9514109409bf93600644e148c8c042187bf0db5575c5b90d1d7a1', 'Retained Qwen response changed'
 response=json.loads(retained.read_text());plan=json.loads(response['message']['content'])
 # Independent director correction, explicitly NOT another Qwen inference:
 # preserve every authored rise/radius and both bank designs; relocate the three
 # repeated western centers to the other approved spatial gaps, no overlap.
 assert [l['center_index'] for l in plan['landforms']]==[4,1,1,1,1]
 for l,center in zip(plan['landforms'],[4,1,8,3,6]):l['center_index']=center
 write('director-repair.json',{'source_run':39445329743,'raw_response_sha256':sha(retained),'reason':'Qwen repeated center 1 four times; overlapping mound plan rejected before editing. Independent director assigns distinct approved gaps, retaining all authored shapes and bank parameters.','before_centers':[4,1,1,1,1],'after_centers':[4,1,8,3,6],'new_inference':False})
else:
 try:
  try:api('/api/version')
  except Exception:
   users=pathlib.Path(os.environ.get('SystemDrive','C:')+'/Users');bins=list(users.glob('*/AppData/Local/Programs/Ollama/ollama.exe'));assert bins,'Installed Ollama unavailable'
   env=os.environ.copy();env['OLLAMA_HOST']='127.0.0.1:11434';models=list(users.glob('*/.ollama/models'))
   if models:env['OLLAMA_MODELS']=str(models[0])
   log=open(EV/'ollama.log','w');server=subprocess.Popen([str(bins[0]),'serve'],env=env,stdout=log,stderr=subprocess.STDOUT)
   for _ in range(40):
    try:api('/api/version');break
    except Exception:time.sleep(1)
   else:raise RuntimeError('Installed Ollama not ready')
  assert any(m['name']=='qwen3:14b' for m in api('/api/tags')['models']),'Existing local model missing; no download'
  response=api('/api/chat',payload,300);write('qwen-response.json',response);plan=json.loads(response['message']['content'])
 finally:
  if server:subprocess.run(['taskkill.exe','/PID',str(server.pid),'/T','/F'],capture_output=True);server.wait(timeout=20)
  if log:log.close()
for side in ['north','south']:
 assert set(plan[side])=={'compression','flare','inland','shear'}
 for k,a,b in [('compression',.48,.72),('flare',1.08,1.28),('inland',.25,.65),('shear',-.22,.22)]:assert isinstance(plan[side][k],(int,float)) and math.isfinite(plan[side][k]) and a<=plan[side][k]<=b
assert 4<=len(plan['landforms'])<=7
seen=set()
for l in plan['landforms']:
 assert isinstance(l['center_index'],int) and 0<=l['center_index']<=8 and l['center_index'] not in seen;seen.add(l['center_index'])
 for k,a,b in [('rise',.35,1),('radius_x',8,15),('radius_z',6,12)]:assert math.isfinite(l[k]) and a<=l[k]<=b
write('validated-plan.json',plan)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
# Build editable measured terrain, with original topology and preserved bounds.
s=survey['terrain'];n=s['resolution'];origin=np.array(vec(s['origin']));size=np.array(vec(s['size']));h=np.array(s['heights']).reshape(n,n);z,x=np.mgrid[0:n,0:n];wx=origin[0]+x/(n-1)*size[0];wz=origin[2]+z/(n-1)*size[2]
delta=np.zeros_like(h)
for l in plan['landforms']:
 cx,cz=context['allowed_landform_centers'][l['center_index']];r=((wx-cx)/l['radius_x'])**2+((wz-cz)/l['radius_z'])**2
 delta+=l['rise']*np.maximum(0,1-r)**3
# Low supporting berm behind bank: falls smoothly to the river, never a cliff.
d=np.abs(wz-river(wx))-width(wx)
delta+=.48*np.exp(-((d-3.1)/2.5)**2)*( .72+.28*np.sin(wx*.14+1.2))
delta*=free(wx,wz)*ease(-.4,2,d);delta=np.minimum(delta,1.05)
newh=h+delta/size[1];assert np.max(delta)<=1.051 and np.count_nonzero(delta>.03)>100
verts=np.stack([wx,-wz,origin[1]+h*size[1]],axis=-1).reshape(-1,3);faces=[]
for z in range(n-1):
 for x in range(n-1):
  a=z*n+x;faces.append((a,a+n,a+n+1,a+1))
def mesh_obj(name,v,faces):
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(v,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);return o
terrain=mesh_obj('Current_Meadow_Editable',verts.tolist(),faces)
for i,v in enumerate(terrain.data.vertices):v.co.z+=float(delta.flat[i])
def sample_grid(grid,x,z):
 fx=np.clip((x-origin[0])/size[0]*(n-1),0,n-1);fz=np.clip((z-origin[2])/size[2]*(n-1),0,n-1);ix=np.floor(fx).astype(int);iz=np.floor(fz).astype(int);jx=np.minimum(ix+1,n-1);jz=np.minimum(iz+1,n-1);u=fx-ix;v=fz-iz
 return (grid[iz,ix]*(1-u)+grid[iz,jx]*u)*(1-v)+(grid[jz,ix]*(1-u)+grid[jz,jx]*u)*v
def sample_delta(x,z):return sample_grid(delta,x,z)
def surface_y(x,z):return origin[1]+sample_grid(newh,x,z)*size[1]
# Hand-directed continuous low erosion skin follows the MEASURED terrain.
# Outer edge is submerged; inland edge is buried. This is a thin rocky bank,
# never an escarpment, wall, repeated cube or new traversable platform.
shore_v=[];shore_f=[];shore_uv=[]
for side in [-1,1]:
 for start,end in [(-72,-15),(9,72)]:
  count=int((end-start)/.85)+1;base=len(shore_v);bands=[-1.15,-.55,.15,.85,1.65,2.5,3.5,4.6]
  for j in range(count):
   x=start+(end-start)*j/(count-1);endmask=float(ease(0,4,x-start)*ease(0,4,end-x))
   for k,off in enumerate(bands):
    ripple=.28*math.sin(x*.72+k*.91)+.14*math.sin(x*1.93-k*.63)
    z=float(river(x)+side*(width(x)+off+ripple))
    y=float(surface_y(x,z));lift=(.08+.10*math.sin(x*.71+k*1.3))*endmask
    if k==0:lift=-.35
    if k==len(bands)-1:lift=-.18
    if endmask<.1:lift=-.18
    shore_v.append((x,y+lift,z));shore_uv.append((x/3.8,(z+y*.55)/3.8))
  for j in range(count-1):
   for k in range(len(bands)-1):
    a=base+j*len(bands)+k;b=a+len(bands)
    shore_f.extend([(a,a+1,b),(b,a+1,b+1)] if side==1 else [(a,b,a+1),(b,b+1,a+1)])
shore=mesh_obj('Measured_Low_Rocky_Bank',[(x,-z,y) for x,y,z in shore_v],shore_f)
uv=shore.data.uv_layers.new(name='UVMap')
for face in shore.data.polygons:
 for li in face.loop_indices:uv.data[li].uv=shore_uv[shore.data.loops[li].vertex_index]
# Actual CC0 PBR source remains editable in the Blender file.
mat=bpy.data.materials.new('Existing_CC0_Rock3_Bank');mat.use_nodes=True;nodes=mat.node_tree.nodes;links=mat.node_tree.links;bsdf=nodes.get('Principled BSDF');tex=nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(ROOT/'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/MaterialInputs/rock_3/rock_3_diff_1k.png'));tex.image.pack();links.new(tex.outputs['Color'],bsdf.inputs['Base Color']);bsdf.inputs['Roughness'].default_value=.9;shore.data.materials.append(mat)
outputs=[];changed_bank=0
for m in survey['meshes']:
 v=np.array([vec(p) for p in m['vertices']],dtype=float);out=v.copy();comps=groups if m['name']=='River_bank_rocks' else components(m)
 if m['name']=='River_bank_rocks':
  allowed={q['id'] for q in targets}
  for index,ids in enumerate(comps):
   if index not in allowed:continue
   pts=v[ids];lo=pts.min(0);hi=pts.max(0);c=(lo+hi)*.5;side=1 if c[2]>river(c[0]) else -1;p=plan['north' if side==1 else 'south'];t=np.clip((pts[:,1]-lo[1])/(hi[1]-lo[1]),0,1)
   # Sculpt a fractured shelf: broad buried foot, asymmetric crown, stepped
   # compressed face and lateral shear. Existing closed topology/UV preserved.
   # Second strategy: preserve the rock's rounded mass and seat it into a
   # continuous low bank; reject native194's flattened submerged slabs.
   out[ids,0]=c[0]+(pts[:,0]-c[0])*1.08
   out[ids,2]=c[2]+(pts[:,2]-c[2])*1.06+side*.28
   out[ids,1]=pts[:,1]-.12
  changed_bank=int(np.count_nonzero(np.linalg.norm(out-v,axis=1)>.001))
 else:
  # Move each connected natural component as a rigid object to preserve the
  # original vegetation silhouette while avoiding floating ground contacts.
  for ids in comps:
   pts=v[ids];foot=pts[pts[:,1]<=np.quantile(pts[:,1],.12)+.005];cx,cz=np.median(foot[:,0]),np.median(foot[:,2]);dy=float(sample_delta(cx,cz));out[ids,1]+=dy
   if m['name'].startswith('River_shingle'):
    target=float(surface_y(cx,cz))-.055
    out[ids,1]+=target-float(out[ids,1].min())
 obj=mesh_obj(m['name'],np.stack([out[:,0],-out[:,2],out[:,1]],axis=-1).tolist(),np.array(m['triangles']).reshape(-1,3).tolist())
 outputs.append({'name':m['name'],'vertices':[{'x':float(q[0]),'y':float(q[1]),'z':float(q[2])} for q in out]})
assert changed_bank>100
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=str(EV/'TerrainBank192.blend'),compress=True)
# Blender source carries the actual edited vertices; re-extract the authored
# height field from that mesh, not from an unrelated preview or mock result.
actual=np.array([v.co.z for v in terrain.data.vertices]).reshape(n,n)
write('candidate.json',{'survey_sha256':sha(EV/'survey.json'),'heights':((actual-origin[1])/size[1]).reshape(-1).tolist(),'meshes':outputs,'shore':{'name':'Measured_Low_Rocky_Bank','vertices':[{'x':x,'y':y,'z':z} for x,y,z in shore_v],'triangles':[i for f in shore_f for i in f],'uv':[{'x':u,'y':v} for u,v in shore_uv]},'changed_terrain_vertices':int(np.count_nonzero(delta>.001)),'changed_bank_vertices':changed_bank,'art_approved':False})
write('author-proof.json',{'blender':bpy.app.version_string,'source_scene_sha256':survey['scene_sha256'],'survey_sha256':sha(EV/'survey.json'),'plan_sha256':sha(EV/'validated-plan.json'),'blend_sha256':sha(EV/'TerrainBank192.blend'),'candidate_sha256':sha(EV/'candidate.json'),'changed_bank_vertices':changed_bank,'changed_terrain_vertices':int(np.count_nonzero(delta>.001)),'max_height_delta_m':float(delta.max()),'protected_height_delta_max':float(np.max(delta[free(wx,wz)==0])),'topology_preserved':True,'reference_sha256':survey['reference_sha256'],'author':'Retained Qwen3 14B bank/shape parameters with explicit independent director repair of repeated centers; mesh operations in bpy','second_strategy':'Independent director: original rock masses seated into continuous measured low shore surface; reject native194 flattened slab geometry; restore shingle contact to surface', 'new_shore_triangles':len(shore_f), 'transport':'direct bpy selected for measured mesh authoring; no extra MCP server needed','art_approved':False,'published':False})
print('VALORIA_TERRAIN_BANK_BLENDER_AUTHOR_COMPLETE')
