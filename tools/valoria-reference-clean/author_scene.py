"""Reference-directed authored geometry. New scene, original project donors preserved.
Coordinates Blender X east, Y inland, Z up; glTF/Unity conversion is applied at import.
Design: compact blue-gabled keep, lower access gate, two-arch bridge, four empty plots.
"""
import bpy, bmesh, math, os, json, random, hashlib, urllib.request, socket, numpy as np
from mathutils import Vector,Matrix
from math import sin,cos,pi
random.seed(817)
ROOT=os.getcwd(); OUT=os.path.join(ROOT,'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources');EV=os.path.join(ROOT,'docs/evidence/valoria-reference-clean')
os.makedirs(OUT,exist_ok=True);os.makedirs(EV,exist_ok=True)
tools={'blender_version':bpy.app.version_string,'blender_executed':True,'ollama':'NOT_REACHABLE','blendermcp_socket':'NOT_REACHABLE'}
try:
 with urllib.request.urlopen('http://127.0.0.1:11434/api/tags',timeout=3) as r:tools['ollama']=json.load(r)
except Exception as e:tools['ollama_error']=str(e)
try:
 with socket.create_connection(('127.0.0.1',9876),timeout=2):tools['blendermcp_socket']='TCP_REACHABLE_NOT_PROTOCOL_VALIDATED'
except Exception as e:tools['blendermcp_error']=str(e)
json.dump(tools,open(os.path.join(EV,'native-tool-check.json'),'w'),indent=2)
bpy.ops.wm.read_factory_settings(use_empty=True)
def mat(n,c,rough=.8,metal=0):
 m=bpy.data.materials.new(n);m.diffuse_color=(*c,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*c,1);p.inputs['Roughness'].default_value=rough;p.inputs['Metallic'].default_value=metal;return m
stone=[mat('Warm limestone %d'%i,(.52+i*.025,.45+i*.022,.35+i*.021)) for i in range(6)]
trim=mat('Carved limestone',(.70,.61,.47));mortar=mat('Recessed stone joints',(.31,.28,.22));wood=mat('Oak',(.23,.105,.036));iron=mat('Forged iron',(.075,.065,.045),.55,.6);gold=mat('Heraldic gold',(.94,.61,.10),.45,.25)
blue=[mat('Blue slate %d'%i,(.025+i*.01,.11+i*.017,.22+i*.025),.62) for i in range(5)]
soil=mat('Warm cultivated earth',(.40,.255,.125));rocks=[mat('River granite %d'%i,(.32+i*.045,.32+i*.039,.28+i*.029)) for i in range(4)]
flame=mat('Torch golden flame',(1,.39,.025));p=flame.node_tree.nodes.get('Principled BSDF');p.inputs['Emission Color'].default_value=(1,.24,.005,1);p.inputs['Emission Strength'].default_value=4
# Real baked texture detail from existing original project source; no external downloads.
npath=os.path.join(ROOT,'art-source/valoria/production/lower-gate-family-v1/stone_grain_normal.png')
if os.path.isfile(npath):
 im=bpy.data.images.load(npath,check_existing=True);im.colorspace_settings.name='Non-Color';im.pack()
 for m in stone+[trim]+rocks:
  n=m.node_tree.nodes.new('ShaderNodeTexImage');n.image=im;nm=m.node_tree.nodes.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.22;m.node_tree.links.new(n.outputs['Color'],nm.inputs['Color']);m.node_tree.links.new(nm.outputs['Normal'],m.node_tree.nodes.get('Principled BSDF').inputs['Normal'])
# Bake weathered earth patches with irregular cell boundaries, rather than a flat brown fill.
N=512;yy,xx=np.mgrid[0:N,0:N]/N;rng=np.random.default_rng(892)
dists=[]
for cx,cy in rng.random((55,2)):dists.append((xx-cx)**2+(yy-cy)**2)
a=np.sort(np.stack(dists),axis=0);edge=np.clip((np.sqrt(a[1])-np.sqrt(a[0]))*90,0,1)
noise=rng.random((N,N))*.09;value=.79+.15*edge+noise;rgba=np.ones((N,N,4),dtype=np.float32);rgba[:,:,:3]=value[:,:,None]*np.array([.53,.345,.18]);image=bpy.data.images.new('Valoria original cracked earth',width=N,height=N);image.pixels.foreach_set(rgba.reshape(-1));image.pack();n=soil.node_tree.nodes.new('ShaderNodeTexImage');n.image=image;soil.node_tree.links.new(n.outputs['Color'],soil.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
# Shared mesh builders consolidate masonry by semantic structure, not thousands of GameObjects.
class Mesh:
 def __init__(self,n):self.n=n;self.v=[];self.f=[];self.mi=[];self.ms=[]
 def add(self,v,f,m):
  off=len(self.v);self.v.extend(v);self.f.extend([tuple(off+i for i in q) for q in f]);
  if m not in self.ms:self.ms.append(m)
  self.mi.extend([self.ms.index(m)]*len(f))
 def box(self,p,s,m,angle=0):
  x,y,z=p;a,b,c=[i/2 for i in s];v=[]
  for X,Y,Z in [(-a,-b,-c),(-a,-b,c),(-a,b,-c),(-a,b,c),(a,-b,-c),(a,-b,c),(a,b,-c),(a,b,c)]:v.append((x+X*cos(angle)-Y*sin(angle),y+X*sin(angle)+Y*cos(angle),z+Z))
  self.add(v,[(0,4,6,2),(1,3,7,5),(0,1,5,4),(2,6,7,3),(0,2,3,1),(4,5,7,6)],m)
 def finish(self,bevel=.035):
  me=bpy.data.meshes.new(self.n);me.from_pydata(self.v,[],self.f);me.update();bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free();o=bpy.data.objects.new(self.n,me);bpy.context.collection.objects.link(o)
  for m in self.ms:me.materials.append(m)
  for p,i in zip(me.polygons,self.mi):p.material_index=i
  # Explicit per-face metric UVs; no zero-UV inherited layers.
  uv=me.uv_layers.new(name='MetricUV')
  for p in me.polygons:
   n=p.normal;axis=max(range(3),key=lambda i:abs(n[i]));axes=[i for i in range(3) if i!=axis]
   for li in p.loop_indices:
    q=me.vertices[me.loops[li].vertex_index].co;uv.data[li].uv=(q[axes[0]]/3.5,q[axes[1]]/3.5)
  if bevel:
   mod=o.modifiers.new('Light-catching dressed edges','BEVEL');mod.width=bevel;mod.segments=2;mod.limit_method='ANGLE';bpy.context.view_layer.objects.active=o
   bpy.ops.object.modifier_apply(modifier=mod.name)
  return o

def courses(g,x,y,z,w,d,h,stone_size=1):
 # Interior shell plus real shallow-relief ashlar skins, staggered joints on four faces.
 g.box((x,y,z+h/2),(w-.08,d-.08,h),mortar)
 rows=max(1,round(h/.48));rh=h/rows
 for r in range(rows):
  zz=z+(r+.5)*rh
  for side,L,C in [('front',w,y-d/2),('back',w,y+d/2),('left',d,x-w/2),('right',d,x+w/2)]:
   start=-L/2;shift=stone_size*.47 if r%2 else 0;cuts=[start]+[q for q in [start+shift+i*stone_size for i in range(math.ceil(L/stone_size)+1)] if start+.08<q<L/2-.08]+[L/2]
   for a,b in zip(cuts,cuts[1:]):
    if side in ('front','back'):p=(x+(a+b)/2,C,zz);s=(b-a-.035,.18,rh-.035)
    else:p=(C,y+(a+b)/2,zz);s=(.18,b-a-.035,rh-.035)
    g.box(p,s,random.choice(stone))
 # foundation and continuous top coping
 g.box((x,y,z+.16),(w+.22,d+.22,.32),trim)
 g.box((x,y,z+h-.1),(w+.24,d+.24,.22),trim)

def crown(g,x,y,z,w,d):
 g.box((x,y,z),(w+.4,d+.4,.28),trim)
 for X in [-w/2,w/2]:
  for j in range(max(2,round(d/1.25))+1):g.box((x+X,y-d/2+j*d/max(2,round(d/1.25)),z+.47),(.65,.65,.8),random.choice(stone))
 for Y in [-d/2,d/2]:
  for j in range(1,max(2,round(w/1.25))):g.box((x-w/2+j*w/max(2,round(w/1.25)),y+Y,z+.47),(.65,.65,.8),random.choice(stone))

def arch(g,x,y,z,r,thick,depth,m,axis='y',segments=13):
 for i in range(segments):
  a=i*pi/segments+.008;b=(i+1)*pi/segments-.008;v=[]
  for D in [-depth/2,depth/2]:
   for R,A in [(r,a),(r,b),(r+thick,b),(r+thick,a)]:
    if axis=='y':v.append((x+R*cos(A),y+D,z+R*sin(A)))
    else:v.append((x+D,y+R*cos(A),z+R*sin(A)))
  g.add(v,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],m)

def portal(g,x,y,z,w,h):
 r=w/2;spring=z+h-r
 # Oak planks have individually profiled arched tops, iron straps and rivets.
 for i in range(9):
  l=-r+i*w/9+.016;rr=-r+(i+1)*w/9-.016
  top=lambda X:spring+math.sqrt(max(0,r*r-X*X))
  v=[(x+l,y,z),(x+rr,y,z),(x+rr,y,top(rr)),(x+l,y,top(l))];g.add(v,[(0,1,2,3)],wood)
 for side in [-1,1]:
  courses(g,x+side*(r+.26),y+.12,z,.48,.5,h-r)
 arch(g,x,y-.1,spring,r,.5,.5,trim)
 for H in [z+.55,z+1.4]:g.box((x,y-.09,H),(w-.15,.12,.14),iron)
 g.box((x,y-.12,z+1.1),(.09,.13,2.1),iron)

def roof(g,x,y,z,w,d,rise):
 # Thick gable roof with overlapping slate courses rather than a blue cuboid.
 for side in [-1,1]:
  for row in range(9):
   t0=row/9;t1=(row+1.12)/9;Y0=y+side*(d/2+.45)*(1-t0);Y1=y+side*(d/2+.45)*(1-t1);Z0=z+rise*t0;Z1=z+rise*t1
   for col in range(18):
    X0=x-w/2-.4+col*(w+.8)/18+.015;X1=X0+(w+.8)/18-.03
    v=[(X0,Y0,Z0),(X1,Y0,Z0),(X1,Y1,Z1),(X0,Y1,Z1),(X0,Y0,Z0-.13),(X1,Y0,Z0-.13),(X1,Y1,Z1-.13),(X0,Y1,Z1-.13)]
    g.add(v,[(0,1,2,3),(4,7,6,5),(0,4,5,1),(3,2,6,7),(0,3,7,4),(1,5,6,2)],random.choice(blue))
  # timber bargeboards along sloping gable silhouette
  for X in [x-w/2-.5,x+w/2+.5]:
   a=Vector((X,y+side*(d/2+.5),z));b=Vector((X,y,z+rise));line(g,a,b,.15,wood)
 g.box((x,y,z+rise+.12),(w+1,.23,.23),blue[2])
def line(g,a,b,r,m):
 a=Vector(a);b=Vector(b);direction=(b-a).normalized();n=direction.cross(Vector((0,0,1)))
 if n.length<.01:n=Vector((1,0,0))
 n.normalize();q=direction.cross(n);v=[]
 for p in [a,b]:
  for i in range(8):v.append(tuple(p+r*(n*cos(2*pi*i/8)+q*sin(2*pi*i/8))))
 g.add(v,[tuple(range(7,-1,-1)),tuple(range(8,16))]+[(i,(i+1)%8,(i+1)%8+8,i+8) for i in range(8)],m)

def banner(g,x,y,z,w=1,h=2.1):
 g.box((x,y,z+.14),(w+.4,.13,.13),gold)
 v=[]
 for j in range(8):
  for i in range(7):
   X=-w/2+i*w/6;Z=z-j*h/7
   if j==7:Z-=.25*(1-abs(X)/(w/2))
   v.append((x+X,y-.07-.09*sin(i*pi/3+j*.4),Z))
 g.add(v,[(j*7+i,j*7+i+1,(j+1)*7+i+1,(j+1)*7+i) for j in range(7) for i in range(6)],blue[3])
 # Three-leaf heraldic device, actual gold mesh on cloth.
 line(g,(x,y-.2,z-.35),(x,y-.2,z-1.3),.045,gold)
 for side in [-1,1]:line(g,(x,y-.2,z-1),(x+side*.22,y-.2,z-.6),.06,gold)
 g.box((x,y-.21,z-1.05),(.52,.025,.065),gold)
def torch(g,x,y,z):
 courses(g,x,y,z,.85,.85,1.0);g.box((x,y,z+1.05),(1,1,.15),trim)
 for i in range(3):
  a=i*2*pi/3;line(g,(x+cos(a)*.19,y+sin(a)*.19,z+1.16),(x+cos(a+.4)*.09,y+sin(a+.4)*.09,z+1.8),.12,flame)

# Compact main keep: one central tower, a gabled hall, two small flanks.
g=Mesh('Bastion_I');courses(g,0,32,.38,17,10,5.4);roof(g,0,32,5.9,17,10,3.1)
# Gables close the two roof ends.
for X in [-8.5,8.5]:g.add([(X,27,5.75),(X,37,5.75),(X,32,8.95)],[(0,1,2)],stone[3])
courses(g,0,28.2,.38,4.8,4.4,10.3);crown(g,0,28.2,10.8,4.8,4.4)
for x in [-8.4,8.4]:courses(g,x,29.5,.38,3.5,5.2,6.8);crown(g,x,29.5,7.3,3.5,5.2);banner(g,x,26.75,6.6,1.15,2.5)
# Deliberately narrow dark window with stone moulding on the main tower.
g.box((0,25.89,7.8),(.8,.08,1.8),wood);arch(g,0,25.78,8.25,.4,.18,.2,trim,segments=7)
portal(g,0,25.86,1.5,2.6,3.7)
# Entrance terrace and physically nested six stair treads.
courses(g,0,25,.38,18,5,1.15)
for i in range(6):g.box((0,20.25+i*.46,.38+(i+1)*.095),(6.0,.49,(i+1)*.19),trim)
for x in [-3.6,3.6]:courses(g,x,22,.38,1.2,3.4,.9);torch(g,x,22.9,1.3)
line(g,(0,28.2,11.35),(0,28.2,14),.075,wood);banner(g,.9,28.2,13.6,1.65,1.1)
g.finish()
# Reuse original gate geometry and original in-project procedural material sources.
donor=os.path.join(ROOT,'art-source/valoria/production/lower-gate-family-v1/LowerGateFamilyV1.blend')
with bpy.data.libraries.load(donor,link=False) as (s,d):d.objects=list(s.objects)
objs=[o for o in d.objects if o and o.type=='MESH']
for o in objs:bpy.context.collection.objects.link(o)
bpy.context.view_layer.update();pts=[o.matrix_world@v.co for o in objs for v in o.data.vertices];lo=Vector([min(p[i] for p in pts) for i in range(3)]);hi=Vector([max(p[i] for p in pts) for i in range(3)]);ctr=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z));scale=.72
for o in objs:
 m=o.matrix_world.copy();o.parent=None
 for v in o.data.vertices:v.co=(m@v.co-ctr)*scale+Vector((0,-10,.35))
 o.matrix_world=Matrix.Identity(4);o.name='Gate_'+o.name
# Bridge with genuine through arches, solid spandrels and voussoir bands.
b=Mesh('Bridge_Masonry');b.box((0,-23,.10),(7,24,.70),stone[3])
# Side walls boolean cuts are applied below, retaining editable blend geometry.
for X in [-3.12,3.12]:
 side=Mesh('Bridge_ArchSpandrel');side.box((X,-23,-1.85),(.75,24,3.65),stone[2]);ob=side.finish(.03)
 for Y in [-28.7,-17.3]:
  bpy.ops.mesh.primitive_cylinder_add(vertices=32,radius=2.55,depth=9,location=(0,Y,-2.7),rotation=(0,pi/2,0));cut=bpy.context.object
  mod=ob.modifiers.new('Structural through arch','BOOLEAN');mod.operation='DIFFERENCE';mod.solver='EXACT';mod.object=cut;bpy.context.view_layer.objects.active=ob;bpy.ops.object.modifier_apply(modifier=mod.name);bpy.data.objects.remove(cut,do_unlink=True)
  arch(b,X,-0+Y,-2.7,2.55,.48,.95,trim,axis='x',segments=15)
 for y in range(-34,-10,2):courses(b,X,y,.45,.7,1.96,.85)
 b.box((X,-23,1.36),(.93,24,.21),trim)
for X in [-3.2,3.2]:
 for Y in [-34,-12]:torch(b,X,Y,.5)
b.finish(.035)
# Paving has real seams, slight stone variation, follows continuous deck/ground levels.
p=Mesh('Axial_Paving')
for ya,yb,w,z in [(-54,-35,5,.40),(-35,-11,5.8,.49),(-8,20.1,5,.40)]:
 y=ya;row=0
 while y<yb:
  n=6 if w>5 else 5
  for i in range(n):p.box((-w/2+(i+.5)*w/n,y+.35,z),(w/n-.035,.66,.10),random.choice(stone))
  y+=.71;row+=1
p.finish(.025)
# Four empty construction plots, true ground plane and dressed stone kerbs.
for x in [-21,21]:
 for y in [2,22]:
  p=Mesh('Plot_'+str(x)+'_'+str(y));p.box((x,y,.355),(17,12,.10),soil)
  for j in range(25):
   for yy in [-6,6]:p.box((x-8.5+(j+.5)*17/25,y+yy,.47),(.59,.31,.22),random.choice(stone))
  for j in range(18):
   for xx in [-8.5,8.5]:p.box((x+xx,y-6+(j+.5)*12/18,.47),(.31,.59,.22),random.choice(stone))
  p.finish(.025)
# Ruins are intentionally small, with stepped broken silhouettes and fallen blocks.
for x,y in [(-42,39),(42,42),(-39,-3),(41,-9)]:
 r=Mesh('Ruin_'+str(x)+'_'+str(y))
 for i in range(8):
  h=max(.6,3.5-i*.45+random.uniform(-.35,.35));courses(r,x-3+i*.85,y,.3,.83,1.15,h)
 for i in range(8):r.box((x+random.uniform(-4,4),y+random.uniform(-2,2),.5),(random.uniform(.4,.8),.6,.65),random.choice(stone),random.random())
 r.finish(.06)
# Reuse editable tree source family. Fit each donor to a tree-scale silhouette,
# preserve source topology and materials, reusing linked copies for instances.
vegd=os.path.join(ROOT,'art-source/valoria/production/vegetation-family-v1/representative/VegetationRepresentativeFamilyV1.blend')
with bpy.data.libraries.load(vegd,link=False) as (s,d):d.objects=[n for n in s.objects if n.startswith('Tree_')]
trees=[o for o in d.objects if o and o.type=='MESH'];assert trees,'Existing tree donor missing'
# Exclude trees from city plots, axial road and main architecture.
positions=[(-53,-39),(-47,-38),(-57,-32),(-38,-43),(26,-42),(39,-43),(47,-39),(-48,-15),(-44,3),(-39,29),(-40,40),(-45,44),(-32,49),(-5,54),(5,57),(25,53),(39,49),(48,29),(46,2),(42,-12)]
for idx,(x,y) in enumerate(positions):
 src=trees[idx%len(trees)];matr=src.matrix_world.copy();ps=[matr@v.co for v in src.data.vertices];lo=Vector([min(q[i] for q in ps) for i in range(3)]);hi=Vector([max(q[i] for q in ps) for i in range(3)]);ct=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z));sc=(6+random.random()*3)/(hi.z-lo.z)
 o=src.copy();o.data=src.data.copy();bpy.context.collection.objects.link(o);o.parent=None;o.name='Reused_Fir_%02d'%idx
 for v,q in zip(o.data.vertices,ps):v.co=(q-ct)*sc+Vector((x,y,.30))
 o.matrix_world=Matrix.Identity(4)
 # Enrich the existing sparse foliage mesh, preserving its one trunk and donor identity.
 bm=bmesh.new();bm.from_mesh(o.data)
 foliage=[f for f in bm.faces if 'foliage' in o.data.materials[f.material_index].name.lower()]
 copied=bmesh.ops.duplicate(bm,geom=foliage)['geom'];verts=[v for v in copied if isinstance(v,bmesh.types.BMVert)]
 bmesh.ops.rotate(bm,verts=verts,cent=Vector((x,y,.3)),matrix=Matrix.Rotation(.62,3,'Z'))
 bmesh.ops.translate(bm,verts=verts,vec=Vector((0,0,-.16)))
 bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free()
# Natural river rock belt, faceted asymmetry; grouped meshes rather than repeated cubes.
for i in range(115):
 x=-65+(i%58)*2.25;bank=-23+1.5*sin(x*.065);y=bank+(-1 if i<58 else 1)*(7.6+random.uniform(-.3,.5))
 if abs(x)<4.5:continue
 bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=(x,y,-.25));o=bpy.context.object;o.name='River_Rock';o.scale=(random.uniform(.7,1.6),random.uniform(.7,1.2),random.uniform(.8,1.55));o.rotation_euler=(random.random()*.3,random.random()*.3,random.random()*pi);o.data.materials.append(random.choice(rocks));bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
# Dense, small ornamental shrubs keep architecture grounded and plots free.
leaf=[mat('Shrub leaf '+str(i),(.16+i*.035,.25+i*.035,.045+i*.012)) for i in range(4)]
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1)
template=bpy.context.object;iv=[v.co.copy() for v in template.data.vertices];iff=[tuple(p.vertices) for p in template.data.polygons];bpy.data.objects.remove(template,do_unlink=True)
sh=Mesh('Foundation_shrubs_and_groundcover')
centers=[(-10,26),(10,26),(-10,34),(10,34),(-5,-11),(5,-11),(-11,-9),(11,-9)]
centers+=positions+[(-42,39),(42,42),(-39,-3),(41,-9)]
for x,y in centers:
 for i in range(7):
  X=x+random.uniform(-1.6,1.6);Y=y+random.uniform(-1.4,1.4);r=random.uniform(.28,.65)
  sh.add([(X+v.x*r,Y+v.y*r,.38+r*.6+v.z*r*.8) for v in iv],iff,random.choice(leaf))
# Sporadic low clumps break broad grass fields without invading the construction areas.
for i in range(1000):
 x=random.uniform(-60,60);y=random.uniform(-48,65)
 if abs(y+23-1.5*sin(x*.065))<9 or abs(x)<11 and -40<y<42:continue
 if any(abs(x-px)<9.3 and abs(y-py)<7 for px in [-21,21] for py in [2,22]):continue
 h=.3+random.random()*.25
 for k in range(3):
  a=random.random()*pi;w=.07
  sh.add([(x-w*cos(a),y-w*sin(a),.37),(x+w*cos(a),y+w*sin(a),.37),(x+random.uniform(-.2,.2),y+random.uniform(-.2,.2),.37+h)],[(0,1,2),(2,1,0)],random.choice(leaf))
sh.finish(0)
# Snap bottom of each reused/generated tree, bush-root centre and river boulder
# to the identical analytic terrain used by Unity, never the source object pivot.
for o in bpy.context.scene.objects:
 if o.type!='MESH' or not o.name.startswith(('River_Rock','Reused_Fir_')):continue
 pts=[o.matrix_world@v.co for v in o.data.vertices];x=sum(p.x for p in pts)/len(pts);y=sum(p.y for p in pts)/len(pts)
 t=max(0,min(1,(abs(y-(-23+1.5*sin(x*.065)))-5.2)/3.8));bank=t*t*(3-2*t)
 h=.35+.12*sin(x*.12)*sin(y*.1)+(max(0,y-40)*.045)-4.9*(1-bank)
 o.location.z+=h-min(p.z for p in pts)-.14
# Source hashes and editability are evidence, never an artistic approval.
for o in bpy.context.scene.objects:
 if o.type=='MESH':
  for slot in o.material_slots:
   if slot.material and slot.material.use_nodes:
    for n in slot.material.node_tree.nodes:
     if n.type=='TEX_IMAGE' and n.image and not n.image.packed_file:
      try:n.image.pack()
      except Exception:pass
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(EV,'Valoria_Reference_Clean.blend'))
bpy.ops.export_scene.gltf(filepath=os.path.join(OUT,'Valoria_Reference_Clean.glb'),export_format='GLB',export_apply=True,export_yup=True,export_texcoords=True,export_normals=True,export_materials='EXPORT')
sha=lambda p:hashlib.sha256(open(p,'rb').read()).hexdigest()
json.dump({'author':'Reference analysis and spatial/model decisions by Codex; Blender bpy mesh operations executed natively','donors':[{'path':p,'sha256':sha(p)} for p in [donor,vegd]],'scene_new_empty':True,'external_downloads':[],'art_approved':False,'mesh_objects':len([o for o in bpy.context.scene.objects if o.type=='MESH']),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in bpy.context.scene.objects if o.type=='MESH'),'blend_sha256':sha(os.path.join(EV,'Valoria_Reference_Clean.blend')),'glb_sha256':sha(os.path.join(OUT,'Valoria_Reference_Clean.glb'))},open(os.path.join(EV,'source-art.json'),'w'),indent=2)
print('VALORIA_REFERENCE_CLEAN_BLENDER_COMPLETE')
