"""Production geography candidate from certified AI. Preserve source; never bake gameplay.
Director-authored prototype: requires subsequent Qwen/Blender authorship and independent Unity review.
"""
import bpy,bmesh,sys,pathlib,hashlib,json,math,gzip,struct
from collections import Counter
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree
out,src=map(pathlib.Path,sys.argv[sys.argv.index('--')+1:]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='28f5bc9665145244fb3b3ac400983e184ef664afdd6da0b8c04dac6fae5a23fc'
bpy.ops.wm.open_mainfile(filepath=str(src));bpy.context.preferences.filepaths.save_version=0;scene=bpy.context.scene
land=[o for o in scene.objects if o.name.startswith('Pilot_Terrain') and o.type=='MESH']
assert len(land)==2
original={o.name:[tuple(v.co) for v in o.data.vertices] for o in land}
oldtrees=[o for o in scene.objects if o.name.startswith('Pilot_Pine') and o.type=='MESH']
river=bpy.data.objects['Pilot_River'];ends=[];river_widths=[]
water_height=sum(v.co.z for v in river.data.vertices)/len(river.data.vertices)
for x in [-96,96]:
 pts=[v.co.y for v in river.data.vertices if abs(v.co.x-x)<.1];ends.append(sum(pts)/len(pts));river_widths.append((max(pts)-min(pts))/2)
def smooth(t):t=np.clip(t,0,1);return t*t*(3-2*t)
def coast(x,y):return 760+38*np.sin(y/157)+17*np.sin(y/57)-x
def rivercentre(x):
 side=0 if x<0 else 1;d=max(0,abs(x)-96)
 return ends[side]+36*math.sin(d/140)*float(smooth(d/65))
# Paths authored as connected corridors with explicit sector exits and urban approaches.
routes=[[(12,270),(42,350),(20,480),(58,650),(15,950)],[(96,193),(180,205),(320,260),(460,330),(710,360)],[(-96,231),(-220,250),(-350,330),(-520,390),(-860,420)],[(0,-78),(30,-180),(4,-350),(-80,-520),(-20,-940)],[(-520,390),(-580,160),(-530,-120),(-600,-400),(-480,-740)],[(460,330),(420,140),(480,-120),(390,-350),(440,-760)],[(-530,-120),(-320,-180),(-120,-210),(30,-180),(220,-150),(480,-120)],[(-350,330),(-270,560),(-60,720),(200,760),(380,680),(620,750)]]
# Smooth the authored road control points into connected, unequal bends.
def organic_route(points):
 if len(points)<3:return points
 result=[]
 for i in range(len(points)-1):
  p0=np.array(points[max(0,i-1)],float);p1=np.array(points[i],float);p2=np.array(points[i+1],float);p3=np.array(points[min(len(points)-1,i+2)],float)
  for t in np.linspace(0,1,7,endpoint=False):
   q=.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t);result.append(tuple(q))
 result.append(points[-1]);return result
routes=[organic_route(r) for r in routes]
faults=[[(-385,260),(-345,218),(-308,206),(-280,165),(-260,150),(-222,137),(-195,86),(-135,40)],[(-308,206),(-350,140),(-365,110)],[(-222,137),(-175,187),(-150,230)]]
def fault_distance(X,Y):
 distance=np.full(np.shape(X),1e6)
 for path in faults:
  for (ax,ay),(bx,by) in zip(path,path[1:]):
   t=np.clip(((X-ax)*(bx-ax)+(Y-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2),0,1);distance=np.minimum(distance,np.hypot(X-ax-t*(bx-ax),Y-ay-t*(by-ay)))
 return distance
groves=[(-220,420,42,65),(-430,680,62,40),(-650,790,65,50),(-750,500,48,70),(-440,210,55,50),(-730,90,60,65),(-280,90,50,38),(-410,-280,55,45),(-750,-370,52,62),(-270,-470,60,45),(-610,-800,65,48),(-130,-800,45,60),(120,-610,60,50),(330,-820,45,60),(650,-650,45,70),(600,-370,45,50),(230,-320,55,48),(240,30,45,65),(540,70,55,45),(300,420,60,50),(580,540,55,65),(100,600,50,45),(-130,900,55,45),(390,910,65,45),(630,900,40,60)]
cities=[]
for y in [-740,-480,-250,330,530,810]:
 for x in [-650,-330,170,520]:
  if all(((x-a)/rx)**2+((y-b)/ry)**2>2.5 for a,b,rx,ry in groves) and abs(y-rivercentre(x))>35:cities.append((x,y,18))

def rawheight(x,y):
 h=5+1.4*math.sin(x/97+y/170)+.9*math.cos(y/83-x/145)
 for cx,cy,rx,ry,amp in [(-490,520,100,160,28),(380,680,90,145,23),(-580,-570,100,95,19)]:h+=amp*math.exp(-((x-cx)/rx)**2-((y-cy)/ry)**2)
 return h
# Broad measured-boundary blend; no second surface below AI.
edge_exact={}
for obj in land:
 for vertex in obj.data.vertices:
  x,y,z=vertex.co
  if abs(abs(x)-96)<.002 or abs(y+78)<.002 or abs(y-270)<.002:edge_exact[(round(x,5),round(y,5))]=z
def oldheight(x,y):
 key=(round(x,5),round(y,5))
 if key in edge_exact:return edge_exact[key]
 # Exact boundary rays can miss Blender triangles by floating-point rounding.
 # Use the measured edge vertices above, and an interior ray for intermediate samples.
 x=max(-95.999,min(95.999,x));y=max(-77.999,min(269.999,y))
 for o in land:
  hit,p,n,i=o.ray_cast(Vector((x,y,150)),Vector((0,0,-1)))
  if hit:return p.z
 return .02
boundary={}
def boundaryheight(x,y):
 key=(round(x,4),round(y,4))
 if key not in boundary:boundary[key]=oldheight(x,y)
 return boundary[key]
def height(x,y):
 if -96<=x<=96 and -78<=y<=270:return oldheight(x,y)
 qx=max(-95.999,min(95.999,x));qy=max(-77.999,min(269.999,y));d=math.hypot(x-qx,y-qy)
 blend=float(smooth(d/42));h=boundaryheight(qx,qy)*(1-blend)+rawheight(x,y)*blend
 if abs(x)>96:
  dr=abs(y-rivercentre(x));rivercut=float(smooth((9-dr)/5))*float(smooth((abs(x)-96)/60));h=h*(1-rivercut)+.02*rivercut
 for cx,cy,radius in cities:
  distance=math.hypot(x-cx,y-cy)
  if distance<radius+14:
   h=rawheight(cx,cy)+(h-rawheight(cx,cy))*float(smooth((distance-radius)/14))
 c=float(coast(x,y));h=h*float(smooth((c+5)/20))-.6*(1-float(smooth((c+5)/20)))
 return h
# Sample every actual perimeter vertex so the seam has matching edge coordinates.
xs=set(float(x) for x in np.arange(-910,911,10));ys=set(float(y) for y in np.arange(-1000,1001,10))
xs.update([-96.,96.]);ys.update([-78.,270.]);ys.update(float(y) for y in range(-120,121,2))
for o in land:
 for v in o.data.vertices:
  x,y,z=v.co
  if abs(abs(x)-96)<.002:ys.add(float(y))
  if abs(y+78)<.002 or abs(y-270)<.002:xs.add(float(x))
xs=sorted(xs);ys=sorted(ys);nx=len(xs);verts=[]
for y in ys:
 for x in xs:verts.append((x,y,height(x,y)))
faces=[]
for j in range(len(ys)-1):
 for i in range(nx-1):
  cx=(xs[i]+xs[i+1])/2;cy=(ys[j]+ys[j+1])/2
  if -96<cx<96 and -78<cy<270:continue
  a=j*nx+i;faces.append((a,a+1,a+nx+1,a+nx))
mesh=bpy.data.meshes.new('Region1 welded continental extension');mesh.from_pydata(verts,[],faces);mesh.update()
outer=bpy.data.objects.new('Region1 extension',mesh);scene.collection.objects.link(outer)
# The initial four AI reserves retain their existing geometry.
# New urban reservations are metadata only; never visible parcel plates.
# City access spurs meet the existing network and stop outside the reserved footprint.
for cx,cy,radius in cities:
 best=None;distance=1e9
 for route in list(routes):
  for (ax,ay),(bx,by) in zip(route,route[1:]):
   t=max(0,min(1,((cx-ax)*(bx-ax)+(cy-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2)))
   q=(ax+t*(bx-ax),ay+t*(by-ay));d=math.hypot(cx-q[0],cy-q[1])
   if d<distance:distance=d;best=q
 if distance>radius+3:routes.append([best,(cx+(best[0]-cx)/distance*(radius+3),cy+(best[1]-cy)/distance*(radius+3))])
# Editable route guides remain in Blender and are excluded from geometry exports.
for index,route in enumerate(routes):
 data=bpy.data.curves.new('Region1 editable road '+str(index),'CURVE');data.dimensions='3D';sp=data.splines.new('POLY');sp.points.add(len(route)-1)
 for pt,(x,y) in zip(sp.points,route):pt.co=(x,y,height(x,y)+.04,1)
 obj=bpy.data.objects.new(data.name,data);scene.collection.objects.link(obj);obj.hide_render=True
def line_distance(X,Y):
 d=np.full(np.shape(X),1e6)
 for route in routes:
  for (ax,ay),(bx,by) in zip(route,route[1:]):
   t=np.clip(((X-ax)*(bx-ax)+(Y-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2),0,1);d=np.minimum(d,np.hypot(X-ax-t*(bx-ax),Y-ay-t*(by-ay)))
 return d
S=2048;X,Y=np.meshgrid(np.linspace(-910,910,S),np.linspace(-1000,1000,S));road=line_distance(X,Y)
wash=.012*np.sin(X/93+Y/123)+.008*np.sin(Y/47-X/61)+.005*np.cos(X/21+Y/35)
rng=np.random.default_rng(791);grain=rng.normal(0,.006,X.shape)
rgba=np.ones((S,S,4),np.float32);rgba[:,:,:3]=np.array([.53,.55,.255])+(wash+grain)[:,:,None]
forest=np.zeros(X.shape)
for cx,cy,rx,ry in groves:forest=np.maximum(forest,np.clip(1-((X-cx)/rx)**2-((Y-cy)/ry)**2,0,1))
rgba[:,:,:3]*=(1-forest[:,:,None]*np.array([.18,.12,.12]))
fracture=smooth((48-fault_distance(X,Y))/38)*.7
rgba[:,:,:3]=rgba[:,:,:3]*(1-fracture[:,:,None])+np.array([.32,.265,.33])*fracture[:,:,None]
mask=smooth((4-road)/2);rgba[:,:,:3]=rgba[:,:,:3]*(1-mask[:,:,None])+(np.array([.58,.49,.29])+grain[:,:,None]) * mask[:,:,None]
# One pigment material for all meadow top faces eliminates source-tile boundaries.
# The certified source remains immutable; banks and original geometry keep their materials.
im=bpy.data.images.new('Region1 broad continental pigment',S,S);im.pixels.foreach_set(rgba.ravel());im.filepath_raw=str(out/'Region1_continental.png');im.file_format='PNG';im.save();im.pack()
mat=bpy.data.materials.new('Region1 open meadows');mat.use_nodes=True;bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Roughness'].default_value=.9;tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);mesh.materials.append(mat)
uv=mesh.uv_layers.new(name='Continental pigment')
for loop in mesh.loops:
 p=mesh.vertices[loop.vertex_index].co;uv.data[loop.index].uv=((p.x+910)/1820,(p.y+1000)/2000)
for p in mesh.polygons:p.use_smooth=True
crossing_banks={}
for bx in [-550.,0.,450.]:
 by=rivercentre(bx) if abs(bx)>96 else sum(v.co.y for v in river.data.vertices if abs(v.co.x)<.1)/len([v for v in river.data.vertices if abs(v.co.x)<.1])
 crossing_banks[bx]=(by,height(bx,by-22),height(bx,by+22))
# Preserve the authored AF/AG/AI material assignments and every active UV value.
# Joining differently named layers creates empty UV0s; normalize the layer NAME,
# never its coordinates. Original .blend remains pinned and immutable.
def face_signature(obj, poly):
 layer=obj.data.uv_layers.active
 corners=[]
 for li in poly.loop_indices:
  q=obj.data.vertices[obj.data.loops[li].vertex_index].co
  uv=layer.data[li].uv
  corners.append(tuple(round(float(v),4) for v in (*q,*uv)))
 return (obj.data.materials[poly.material_index].name,tuple(sorted(corners)))
core_faces=Counter(face_signature(obj,poly) for obj in land for poly in obj.data.polygons)
core_samples=[];sample_counts={}
for obj in land:
 for poly in obj.data.polygons:
  material=obj.data.materials[poly.material_index].name
  if sample_counts.get(material,0)>=32:continue
  li=poly.loop_indices[0];q=obj.data.vertices[obj.data.loops[li].vertex_index].co
  if abs(q.x)>94 or q.y<-76 or q.y>268:continue
  texcoord=obj.data.uv_layers.active.data[li].uv
  core_samples.append({'material':material,'x':q.x,'y':q.z,'z':q.y,'u':texcoord.x,'v':texcoord.y})
  sample_counts[material]=sample_counts.get(material,0)+1
assert len(core_samples)>=64,('Insufficient retained material samples',len(core_samples))
core_images={}
for obj in land:
 for material in obj.data.materials:
  if not material or not material.use_nodes:continue
  for node in material.node_tree.nodes:
   if node.type=='TEX_IMAGE' and node.image:
    image=node.image
    if image.name in core_images:continue
    pixels=np.array(image.pixels[:],dtype=np.float32)
    assert pixels.size>16 and np.isfinite(pixels).all(),('Invalid retained image',image.name)
    # AO may correctly be constant; unused historical texture nodes are not albedo.
    active_albedo=any(link.from_node==node and link.to_socket.name=='Base Color' for link in material.node_tree.links)
    if active_albedo:assert np.ptp(pixels[::4])>.01,('Missing active retained pigment',image.name)
    core_images[image.name]=hashlib.sha256(pixels.tobytes()).hexdigest()
for obj in land+[outer]:
 layer=obj.data.uv_layers.active
 assert layer is not None,('Missing source UV',obj.name)
 if obj.data.uv_layers.get('UVMap') and obj.data.uv_layers.get('UVMap')!=layer:
  obj.data.uv_layers['UVMap'].name='Preserved secondary UV'
 layer.name='UVMap'
 layer.active_render=True
# Join ground meshes, preserving AI material slots/UV; remove unused interior grid vertices.
bpy.ops.object.select_all(action='DESELECT')
for o in land+[outer]:o.select_set(True)
bpy.context.view_layer.objects.active=outer;bpy.ops.object.join();terrain=outer;terrain.name='BLD_Terrain'
bm=bmesh.new();bm.from_mesh(terrain.data);bmesh.ops.delete(bm,geom=[v for v in bm.verts if not v.link_faces],context='VERTS');bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.001);bm.to_mesh(terrain.data);bm.free();terrain.data.update()
# BMesh weld and join must not silently repaint or remap the historical sector.
terrain.data.uv_layers.active=terrain.data.uv_layers['UVMap']
joined_faces=Counter(face_signature(terrain,poly) for poly in terrain.data.polygons)
missing=core_faces-joined_faces
assert not missing,('Retained sector material/UV/geometry changed',len(missing))
core_proof={'source_ai_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),
 'retained_faces':sum(core_faces.values()),'missing_or_changed_faces':0,
 'image_float_pixel_sha256':core_images,'method':'original material slots and UV coordinates retained; common UVMap name for join',
 'art_approved':False}
(out/'retained-core-integrity.json').write_text(json.dumps(core_proof,indent=2))
print('AF_AG_CORE_MATERIAL_UV_INTEGRITY_PASS',sum(core_faces.values()),len(core_images))
# Certify a real welded source/extension seam outside the river-bank section.
seam_levels={}
for vertex in terrain.data.vertices:
 x,y,z=vertex.co
 if (abs(abs(x)-96)<.002 or abs(y+78)<.002 or abs(y-270)<.002) and abs(y-rivercentre(x))>14:
  seam_levels.setdefault((round(x,4),round(y,4)),[]).append(z)
seam_spread=max((max(values)-min(values) for values in seam_levels.values()),default=0)
assert seam_spread<.01,('Vertical seam discontinuity',seam_spread)
print('REGION1_SOURCE_SEAM_MAX_SPREAD_M',seam_spread)
# Three authored crossings connect both continental banks. Decks follow measured
# bank heights and carry visible planks; they are not invisible collider offsets.
bridge_specs=[]
bridge_mat=bpy.data.materials.new('Bridge weathered timber');bridge_mat.use_nodes=True
bridge_bs=next(n for n in bridge_mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bridge_bs.inputs['Base Color'].default_value=(.31,.22,.12,1);bridge_bs.inputs['Roughness'].default_value=.94
for cx in [-550.,0.,450.]:
 cy,z0,z1=crossing_banks[cx]
 length=44.;half=length/2;vv=[];ff=[]
 for i in range(45):
  t=i/44;z=z0*(1-t)+z1*t+.5*math.sin(t*math.pi)
  vv.extend([(cx-4,cy-half+i,z),(cx+4,cy-half+i,z)])
  if i:ff.append((2*i-2,2*i-1,2*i+1,2*i))
 me=bpy.data.meshes.new('True plank deck');me.from_pydata(vv,[],ff);me.materials.append(bridge_mat);plank=bridge_mat.copy();plank.name='Bridge alternate weathered plank';next(n for n in plank.node_tree.nodes if n.type=='BSDF_PRINCIPLED').inputs['Base Color'].default_value=(.25,.17,.09,1);me.materials.append(plank)
 for face in me.polygons:face.material_index=face.index%2
 obj=bpy.data.objects.new('Authored crossing '+str(cx),me);scene.collection.objects.link(obj)
 bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);terrain.select_set(True);bpy.context.view_layer.objects.active=terrain;bpy.ops.object.join()
 bridge_specs.append((cx,cy,8,length))
# Actual vegetation clearance at authored crossings and existing logical destinations.
# These source objects remain recoverable in the certified AI file and this .blend.
for tree in oldtrees:
 x,y=tree.location.x,tree.location.y
 crossing=any(abs(x-cx)<12 and abs(y-cy)<length/2+8 for cx,cy,w,length in bridge_specs)
 destination=any(math.hypot(x-cx,y-cy)<r for cx,cy,r in [(0,0,16),(47,12,9),(46.5,-62.891,10),(-85,-53,8),(-92,-57,8)])
 if crossing or destination:tree.hide_render=True;tree.hide_set(True)
# Integrated geological fracture, not an upright monument. Keep city/real targets south of it.
positions=np.array([tuple(v.co) for v in terrain.data.vertices]);distance=fault_distance(positions[:,0],positions[:,1]);incision=12*np.clip(1-distance/24,0,1)**2
for vertex,cut in zip(terrain.data.vertices,incision):vertex.co.z-=float(cut)
# Grounded fracture ribbons follow the authored main fault and two unequal branches.
def simplemat(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;b=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');b.inputs['Base Color'].default_value=(*color,1);b.inputs['Roughness'].default_value=.85;return m
shale=simplemat('Fracture dark violet shale',(.08,.045,.11));watermat=simplemat('Continental river teal',(.055,.25,.30))
bpy.context.view_layer.update();bv=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
def surface(x,y):
 p,n,i,d=bv.ray_cast(Vector((x,y,200)),Vector((0,0,-1)));return p.z if p else -.6
v=[];f=[]
for branch,path in enumerate(faults):
 points=[]
 for aa,bb in zip(path,path[1:]):
  length=math.dist(aa,bb);steps=max(2,int(length/5))
  for t in np.linspace(0,1,steps,endpoint=False):points.append((aa[0]+(bb[0]-aa[0])*t,aa[1]+(bb[1]-aa[1])*t))
 points.append(path[-1]);start=len(v)
 for i,(cx,cy) in enumerate(points):
  previous=points[max(0,i-1)];following=points[min(len(points)-1,i+1)];angle=math.atan2(following[1]-previous[1],following[0]-previous[0]);t=i/(len(points)-1)
  w=(.5+3.2*math.sin(math.pi*t)**.7)*(1 if branch==0 else .6)*(1+.15*math.sin(i*1.3+branch))
  for side in [-1,1]:
   x=cx+side*w*math.sin(angle);y=cy-side*w*math.cos(angle);v.append((x,y,surface(x,y)+.025))
  if i:f.append((len(v)-4,len(v)-3,len(v)-1,len(v)-2))
me=bpy.data.meshes.new('Ground fracture ribbons');me.from_pydata(v,[],f);me.materials.append(shale);o=bpy.data.objects.new('BLD_BreachFloor',me);scene.collection.objects.link(o)
# Anchor is recorded explicitly, rather than inferring it from asymmetrical bounds.
# Two connected river extensions, with no overlapping geography inside the retained sector.
for side in [-1,1]:
 vv=[];ff=[]
 for i in range(329):
  x=side*(96+i*2.5);cy=rivercentre(x);d=abs(x)-96
  width=river_widths[0 if side<0 else 1]+(.65+.35*math.sin(d/53))*float(smooth(d/80))
  for sign in [-1,1]:vv.append((x,cy+sign*width,water_height))
  if i:ff.append((2*i,2*i+1,2*i-1,2*i-2) if side>0 else (2*i-2,2*i-1,2*i+1,2*i))
 me=bpy.data.meshes.new('Connected water');me.from_pydata(vv,[],ff);me.materials.append(river.data.materials[0]);uv=me.uv_layers.new(name='River flow')
 for loop in me.loops:
  q=me.vertices[loop.vertex_index].co;uv.data[loop.index].uv=((q.x+96)/192,loop.vertex_index%2)
 for face in me.polygons:face.use_smooth=True
 o=bpy.data.objects.new('BLD_RiverExtension_'+str(side),me);scene.collection.objects.link(o)
# Sea only at the east edge, shaped by the same coastline rather than a rectangle beneath land.
vv=[];ff=[]
for i,y in enumerate(range(-1000,1001,10)):
 c=float(coast(0,y));vv.extend([(c,y,.12),(910,y,.12)])
 if i:ff.append((2*i-2,2*i-1,2*i+1,2*i))
me=bpy.data.meshes.new('Eastern sea');me.from_pydata(vv,[],ff);me.materials.append(watermat);o=bpy.data.objects.new('BLD_EasternSea',me);scene.collection.objects.link(o)
# Natural grouped canopies reuse AI editable tree topology, never old main donors.
masters=[bpy.data.objects['Editable Layered fir source'],bpy.data.objects['Editable Slender fir source']]
for index,(cx,cy,rx,ry) in enumerate(groves):
 parts=[]
 for k in range(27):
  a=k*2.39996;r=math.sqrt((k+.5)/27);x=cx+math.cos(a)*rx*r*.8;y=cy+math.sin(a)*ry*r*.8
  if float(line_distance(np.array(x),np.array(y)))<9 or float(fault_distance(np.array(x),np.array(y)))<36:continue
  donor=masters[k%2];o=donor.copy();o.data=donor.data.copy();scene.collection.objects.link(o);o.hide_render=False;o.hide_viewport=False;o.hide_set(False);o.location=(x,y,surface(x,y));scale=2.8+(k%5)*.2;o.scale=(scale,scale,scale);o.rotation_euler.z=k*.71;parts.append(o)
 if not parts:continue
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();parts[0].name='BLD_ForestMass_'+str(index).zfill(2)
 mod=parts[0].modifiers.new('Shared regional canopy simplification','DECIMATE');mod.ratio=.4;bpy.ops.object.modifier_apply(modifier=mod.name)
# Distant LODs are simplified copies of the same silhouettes; no canopy substitutions.
far_cache={}
for near in list(scene.objects):
 if near.type!='MESH' or not (near.name.startswith('BLD_ForestMass_') or near.name.startswith('Pilot_Pine')):continue
 if near.hide_render:continue
 far=near.copy();far.data=near.data.copy();scene.collection.objects.link(far)
 far.name=near.name.replace('BLD_ForestMass_','BLD_ForestFar_') if near.name.startswith('BLD_ForestMass_') else 'Far_'+near.name
 key=near.data.name
 if key in far_cache:far.data=far_cache[key]
 else:
  bpy.ops.object.select_all(action='DESELECT');far.select_set(True);bpy.context.view_layer.objects.active=far
  mod=far.modifiers.new('Mobile distant silhouette','DECIMATE');mod.ratio=.16;bpy.ops.object.modifier_apply(modifier=mod.name);far_cache[key]=far.data
# Source meshes remain editable but excluded from export; remove no historical file.
# Export includes only genuinely visible scene geometry and the assembled geography.
anchor=bpy.data.objects.new('BLD_BreachAnchor',None);scene.collection.objects.link(anchor);anchor.location=(-260,150,0)
selected=[anchor]
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render and not o.hide_get():selected.append(o)
bpy.ops.object.select_all(action='DESELECT')
for o in selected:o.select_set(True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'))
bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
# Query samples are taken from the actual exported mesh (not the height recipe).
NX,NZ=456,501;data=[]
QX,QY=np.meshgrid(np.linspace(-910,910,NX),np.linspace(-1000,1000,NZ));RD=line_distance(QX,QY);FO=np.zeros(QX.shape)
for a,b,rx,ry in groves:FO=np.maximum(FO,np.clip(1-((QX-a)/rx)**2-((QY-b)/ry)**2,0,1))
MM=np.zeros((NZ,NX,4),np.uint8);MM[:,:,0]=(np.clip(.5+coast(QX,QY)/32,0,1)*255).astype(np.uint8);MM[:,:,2]=(np.clip(1-RD/5,0,1)*255).astype(np.uint8);MM[:,:,3]=(FO*255).astype(np.uint8)
for j,y in enumerate(np.linspace(-1000,1000,NZ)):
 for i,x in enumerate(np.linspace(-910,910,NX)):
  data.append(surface(float(x),float(y)))
  riverwater=abs(x)>96 and abs(y-rivercentre(x))<5
  if -96<=x<=96 and -78<=y<=270:
   hit,point,normal,face=river.ray_cast(Vector((x,y,150)),Vector((0,0,-1)));riverwater=hit
  if any(abs(x-cx)<w/2 and abs(y-cy)<length/2 for cx,cy,w,length in bridge_specs):riverwater=False
  MM[j,i,1]=255 if riverwater else 0
masks=MM.tobytes()
with gzip.open(out/'RegionOneBlenderQueries.bytes','wb') as f:
 f.write(struct.pack('<iiiff',0x31424c45,NX,NZ,1820.,2000.));f.write(np.array(data,dtype='<f4').tobytes());f.write(bytes(masks));f.write(struct.pack('<ii',0x32564c45,len(terrain.data.vertices)))
 for v in terrain.data.vertices:f.write(struct.pack('<fff',v.co.x,v.co.z,v.co.y))
(out/'production-region.json').write_text(json.dumps({'source_ai_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'bounds':[-910,-1000,910,1000],'terrain_vertices':len(terrain.data.vertices),'retained_material_samples':core_samples,'faults':[{'ax':a[0],'az':a[1],'bx':b[0],'bz':b[1],'radius':28} for path in faults for a,b in zip(path,path[1:])],'city_reserves':cities,'cities':[{'x':x,'z':y,'radius':rad} for x,y,rad in cities+[(-62,204,15),(-18,250,15),(38,202,15),(67,250,15)]],'forest_groups':len(groves),'bridges':[{'x':x,'z':y,'width':w,'length':l} for x,y,w,l in bridge_specs],'source_seam_max_spread_m':seam_spread,'dynamic_gameplay_baked':0,'art_status':'PROTOTYPE_NO_QWEN_AUTHORSHIP_NOT_APPROVED','physical_device':False},indent=2))
print('REGION1_AI_CONTINENT_EXPORTED',len(terrain.data.vertices))
