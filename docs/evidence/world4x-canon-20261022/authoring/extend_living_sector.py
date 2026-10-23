"""AI: bounded northern grassland, on immutable restored AG. No gameplay baked into mesh."""
import bpy, bmesh, sys, pathlib, hashlib, json, math
import numpy as np
from mathutils import Vector
out,src=map(pathlib.Path,sys.argv[sys.argv.index('--')+1:]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='4b1a71c502302172017867d646f52d7abce0bcc53901264be2461da86e3f4a2d'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene
terrain=bpy.data.objects['Pilot_Terrain_1'];original=[tuple(v.co) for v in terrain.data.vertices]
boundary=sorted([v for v in terrain.data.vertices if abs(v.co.y-170)<.001],key=lambda v:v.co.x)
assert len(boundary)>40, len(boundary)
# Connect exactly to AG. Low eastern shoulder, broad western meadow, one dry swale.
# Hermite blend preserves the measured boundary value and derivative.
xs=np.array([v.co.x for v in boundary]);zs=np.array([v.co.z for v in boundary])
def oldground(x,y):
 ok,p,n,i=terrain.ray_cast(Vector((max(-95.999,min(95.999,x)),y,150)),Vector((0,0,-1)));assert ok;return p.z
slopes=np.array([(v.co.z-oldground(v.co.x,168))/2 for v in boundary])
cities=[(-62,204,15),(-18,250,15),(38,202,15),(67,250,15)]
def baseheight(x,y):
 return 3.8+3.6*math.exp(-((x-76)/23)**2-((y-219)/44)**2)+1.5*math.exp(-((x+76)/29)**2-((y-239)/24)**2)-.7*math.exp(-((x+4)/22)**2)
def height(x,y):
 t=(y-170)/80;u=min(1,(y-170)/22);s=u*u*(3-2*u)
 edge=float(np.interp(x,xs,zs));slope=float(np.interp(x,xs,slopes))
 base=baseheight(x,y)
 z=(edge+slope*(y-170)*(1-u))*(1-s)+base*s
 for cx,cy,r in cities:
  d=math.hypot(x-cx,y-cy);v=max(0,min(1,(d-r)/10));v=v*v*(3-2*v)
  if d<r+10:z=baseheight(cx,cy)+(z-baseheight(cx,cy))*v
 return z
# Continue the existing northern path at its actual endpoint, not a copied river.
ends=[]
for o in scene.objects:
 if o.type=='CURVE' and o.name.startswith('Editable P connected path'):
  for sp in o.data.splines:
   if sp.bezier_points:
    p=max(sp.bezier_points,key=lambda p:p.co.y)
    if p.co.y>160:ends.append((float(p.co.x),float(p.co.y)))
entry=min(ends,key=lambda p:abs(p[0]))[0] if ends else 0
routes=[[(entry,170),(entry+6,182),(8,198),(4,218),(12,270)],[(4,218),(-30,216),(-63,228),(-96,231)],[(8,198),(36,182),(66,185),(96,193)]]
# Smooth each road through its authored control points; retain exact junctions.
controls=routes
routes=[]
for points in controls:
 line=[]
 for i in range(len(points)-1):
  p0=np.array(points[max(0,i-1)],float);p1=np.array(points[i],float);p2=np.array(points[i+1],float);p3=np.array(points[min(len(points)-1,i+2)],float)
  for t in np.linspace(0,1,9,endpoint=False):
   p=.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t);line.append(tuple(p))
 line.append(points[-1]);routes.append(line)
# Reserve a connected approach for every future city, ending outside its clear pad.
main_routes=list(routes)
for cx,cy,r in cities:
 closest=None;best=1e9
 for points in main_routes:
  for (ax,ay),(bx,by) in zip(points,points[1:]):
   t=max(0,min(1,((cx-ax)*(bx-ax)+(cy-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2)))
   q=np.array([ax+t*(bx-ax),ay+t*(by-ay)]);d=float(np.linalg.norm(np.array([cx,cy])-q))
   if d<best:best=d;closest=q
 if best>r+2:
  direction=(np.array([cx,cy])-closest)/best
  routes.append([tuple(closest),tuple(np.array([cx,cy])-direction*(r+2))])
def distance(x,y):
 best=1e9
 for points in routes:
  for (ax,ay),(bx,by) in zip(points,points[1:]):
   t=max(0,min(1,((x-ax)*(bx-ax)+(y-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2)))
   best=min(best,math.hypot(x-ax-t*(bx-ax),y-ay-t*(by-ay)))
 return best
# New UV material only on new faces: old loops, topology and all AG vertices survive.
W,H=2048,1024;X,Y=np.meshgrid(np.linspace(-96,96,W),np.linspace(170,270,H))
pigment=.028*np.sin(X/23+Y/31)+.018*np.sin(X/11-Y/25)
# Broken broad washes follow the eastern shoulder and western swale.
pigment+=.025*np.exp(-((X-65)/29)**2-((Y-217)/32)**2)-.02*np.exp(-((X+15)/24)**2-((Y-225)/20)**2)
rng_pigment=np.random.default_rng(781)
fine=rng_pigment.normal(0,.027,X.shape)+.005*np.sin(X*8+np.sin(Y*4))*np.sin(Y*7)
rgba=np.ones((H,W,4),np.float32);rgba[:,:,:3]=np.array([.545,.54,.265])+(pigment+fine)[:,:,None]
road=np.full(X.shape,1e9)
for points in routes:
 for (ax,ay),(bx,by) in zip(points,points[1:]):
  t=np.clip(((X-ax)*(bx-ax)+(Y-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2),0,1)
  road=np.minimum(road,np.hypot(X-ax-t*(bx-ax),Y-ay-t*(by-ay)))
mask=np.clip((1.85-road)/1.0,0,1);rgba[:,:,:3]=rgba[:,:,:3]*(1-mask[:,:,None])+(np.array([.55,.46,.27])+fine[:,:,None]*.35)*mask[:,:,None]
# Match the exact retained pigment at the join, then open into broad meadow tones.
prior=bpy.data.images.get('AF broad meadow flows')
if prior:
 pw,ph=prior.size;pp=np.array(prior.pixels[:]).reshape(ph,pw,4);edge=np.array([np.interp(np.linspace(0,pw-1,W),np.arange(pw),pp[-1,:,k]) for k in range(3)]).T
 # Match the retained boundary over one narrow weld band. Never mirror AG's old
 # road into the extension: its reflected paint made a false diagonal branch.
 blend=np.clip((Y-170)/3,0,1);blend=blend*blend*(3-2*blend)
 rgba[:,:,:3]=edge[None,:,:]*(1-blend[:,:,None])+rgba[:,:,:3]*blend[:,:,None]
 rgba[:,:,:3]=rgba[:,:,:3]*(1-mask[:,:,None])+(np.array([.55,.46,.27])+fine[:,:,None]*.35)*mask[:,:,None]

im=bpy.data.images.new('AI open northern grassland',W,H);im.pixels.foreach_set(rgba.ravel());im.filepath_raw=str(out/'AI_meadow.png');im.file_format='PNG';im.save();im.pack()
mat=bpy.data.materials.new('AI open meadow paths');mat.use_nodes=True;bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Roughness'].default_value=.92;tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
terrain.data.materials.append(mat);mi=len(terrain.data.materials)-1
bm=bmesh.new();bm.from_mesh(terrain.data);bm.verts.ensure_lookup_table();uv=bm.loops.layers.uv.active
prev=[bm.verts[v.index] for v in boundary]
for y in range(172,271,2):
 row=[bm.verts.new((float(x),y,height(float(x),y))) for x in xs]
 for i in range(len(row)-1):
  f=bm.faces.new((prev[i],prev[i+1],row[i+1],row[i]));f.material_index=mi;f.smooth=True
  for loop in f.loops:loop[uv].uv=((loop.vert.co.x+96)/192,(loop.vert.co.y-170)/100)
 prev=row
bm.to_mesh(terrain.data);bm.free();terrain.data.update();bpy.context.view_layer.update()
assert [tuple(v.co) for v in terrain.data.vertices[:len(original)]]==original
# Sparse asymmetrical perimeter groves, leaving city circles and wide circulation open.
rng=np.random.default_rng(164);donors=[o for o in scene.objects if o.name.startswith('Pilot_Pine')];added=[]
for cx,cy,rx,ry,count in [(-80,184,13,10,12),(-75,264,16,7,10),(82,213,10,18,14),(35,264,12,6,8)]:
 for attempt in range(500):
  if sum(o.get('grove')==str((cx,cy)) for o in added)>=count:break
  x=float(rng.uniform(cx-rx,cx+rx));y=float(rng.uniform(cy-ry,cy+ry))
  if not(-92<x<92 and 175<y<268) or distance(x,y)<5:continue
  if any(math.hypot(x-a,y-b)<r+5 for a,b,r in cities) or any(math.hypot(x-o.location.x,y-o.location.y)<3.5 for o in added):continue
  o=donors[len(added)%len(donors)].copy();o.data=donors[len(added)%len(donors)].data;scene.collection.objects.link(o);o.name='Pilot_Pine_AI_'+str(len(added));o.location=(x,y,height(x,y));o.scale*=.8;o['grove']=str((cx,cy));added.append(o)
bpy.context.view_layer.update()
def tree_radius(o):
 return max(math.hypot((o.matrix_world@Vector(c)).x-o.location.x,(o.matrix_world@Vector(c)).y-o.location.y) for c in o.bound_box)
# Export semantic authoring hints separately. Runtime must still raycast the real imported mesh.
meta={'stage':'AI','minX':-96,'maxX':96,'minZ':170,'maxZ':270,'cities':[{'x':x,'z':y,'radius':r} for x,y,r in cities], 'routes':[{'points':[{'x':x,'z':y} for x,y in line]} for line in routes], 'trees':[{'x':o.location.x,'z':o.location.y,'radius':tree_radius(o)} for o in added], 'source':'restored AG 40014366595','testPopulationOnly':True}
(out/'LivingSectorLayout.json').write_text(json.dumps(meta,indent=2))
for im in bpy.data.images:
 if im.size[0]>0 and im.source in {'GENERATED','FILE'}:im.pack()
scene['finishing_stage']='AI bounded northern extension: open 4X land; no baked dynamic content'
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'))
(out/'sector-authoring.json').write_text(json.dumps({'stage':'AI','source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'AG_vertices_unchanged':len(original),'added_vertices':len(terrain.data.vertices)-len(original),'extension_area_m2':192*100,'previous_extent_area_m2':192*248,'city_clearances':len(cities),'added_trees':len(added),'dynamic_objects_baked':0,'art_approved':False},indent=2))
