"""Reference-directed authored geometry. New scene, original project donors preserved.
Coordinates Blender X east, Y inland, Z up; glTF/Unity conversion is applied at import.
Design: compact blue-gabled keep, lower access gate, two-arch bridge, four empty plots.
"""
import bpy, bmesh, math, os, json, random, hashlib, urllib.request, socket, numpy as np
from mathutils import Vector,Matrix
from math import sin,cos,pi
random.seed(817)
def smoothstep(a,b,v):
 t=max(0,min(1,(v-a)/(b-a)));return t*t*(3-2*t)
def river_center(x):return -23+1.5*sin(x*.065)+.85*sin(x*.19+.4)*smoothstep(6,17,abs(x))
def river_width(x):return 7.1+.75*sin(x*.095+.7)+.48*sin(x*.23)
def terrain_height(x,y):
 t=smoothstep(river_width(x)-2,river_width(x)+2,abs(y-river_center(x)))
 h=.35+.65*sin(x*.08)*sin(y*.09)+.24*sin(x*.25+y*.17)+max(0,y-40)*.045
 flatten=1-smoothstep(0,2.5,max(abs(x)-11,-45-y,y-45))
 for px,py in [(-23,28),(-19,9),(29,32),(28,12)]:flatten=max(flatten,1-smoothstep(0,2,max(abs(x-px)-9,abs(y-py)-6.5)))
 h=h*(1-flatten)+.35*flatten
 natural=smoothstep(river_width(x)+1,river_width(x)+4,abs(y-river_center(x)));h=.35+(h-.35)*natural
 return h-4.9*(1-t)
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
stone=[mat('Warm limestone %d'%i,(.265+i*.020,.235+i*.018,.185+i*.016)) for i in range(6)]
trim=mat('Carved limestone',(.41,.355,.275));mortar=mat('Recessed stone joints',(.155,.135,.10));wood=mat('Oak',(.14,.060,.019));iron=mat('Forged iron',(.035,.031,.025),.55,.6);gold=mat('Heraldic gold',(.94,.61,.10),.45,.25)
blue=[mat('Blue slate %d'%i,(.014+i*.005,.045+i*.009,.11+i*.017),.62) for i in range(5)]
cloth=mat('Royal blue cloth',(.018,.11,.45))
soil=mat('Warm cultivated earth',(.40,.255,.125));rocks=[mat('River granite %d'%i,(.135+i*.023,.13+i*.023,.115+i*.022)) for i in range(5)]
# Original low-contrast surface pigmentation is baked to packed raster maps for glTF.
sy,sx=np.mgrid[0:256,0:256]/256;grng=np.random.default_rng(715)
grain=grng.random((256,256));weather=.94+.065*np.sin(sx*19+np.sin(sy*11))+.045*np.sin(sy*31+sx*9)+grain*.045
for material in stone+[trim]+rocks:
 rgba=np.ones((256,256,4),dtype=np.float32);base=np.array(material.diffuse_color[:3]);rgba[:,:,:3]=weather[:,:,None]*base[None,None,:]
 flecks=grain>.984;rgba[flecks,:3]*=.76
 # Packed generated image bytes are sampled as sRGB in glTF: encode the linear palette.
 rgba[:,:,:3]=np.where(rgba[:,:,:3]<=.0031308,rgba[:,:,:3]*12.92,1.055*np.maximum(rgba[:,:,:3],0)**(1/2.4)-.055)
 image=bpy.data.images.new(material.name+' original weathered albedo',width=256,height=256);image.pixels.foreach_set(rgba.reshape(-1));image.pack();node=material.node_tree.nodes.new('ShaderNodeTexImage');node.image=image;material.node_tree.links.new(node.outputs['Color'],material.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
flame=mat('Torch golden flame',(1,.39,.025));p=flame.node_tree.nodes.get('Principled BSDF');p.inputs['Emission Color'].default_value=(1,.24,.005,1);p.inputs['Emission Strength'].default_value=2
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
  me=bpy.data.meshes.new(self.n);me.from_pydata(self.v,[],self.f);me.update();bm=bmesh.new();bm.from_mesh(me);bmesh.ops.delete(bm,geom=[v for v in bm.verts if not v.link_faces],context='VERTS');bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free();o=bpy.data.objects.new(self.n,me);bpy.context.collection.objects.link(o)
  for m in self.ms:me.materials.append(m)
  for p,i in zip(me.polygons,self.mi):p.material_index=i
  # Explicit per-face metric UVs; no zero-UV inherited layers.
  uv=me.uv_layers.new(name='MetricUV')
  for p in me.polygons:
   n=p.normal;axis=max(range(3),key=lambda i:abs(n[i]));axes=[i for i in range(3) if i!=axis]
   for li in p.loop_indices:
    q=me.vertices[me.loops[li].vertex_index].co;uv.data[li].uv=(q[axes[0]]/3.5,q[axes[1]]/3.5)
  if bevel:
   mod=o.modifiers.new('Light-catching dressed edges','BEVEL');mod.width=bevel;mod.segments=1;mod.limit_method='ANGLE';bpy.context.view_layer.objects.active=o
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
  rows=max(4,round(d*1.15));cols=max(6,round(w*1.6))
  for row in range(rows):
   t0=row/rows;t1=min(1.025,(row+1.12)/rows);Y0=y+side*(d/2+.45)*(1-t0);Y1=y+side*(d/2+.45)*(1-t1);lift=random.uniform(-.016,.016);Z0=z+rise*t0+lift;Z1=z+rise*t1+lift
   for col in range(cols+1):
    X0=max(x-w/2-.4,x-w/2-.4+(col-.5*(row%2))*(w+.8)/cols)+.012;X1=min(x+w/2+.4,x-w/2-.4+(col+1-.5*(row%2))*(w+.8)/cols)-.012
    if X1-X0<.03:continue
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
 g.add(v,[(j*7+i,j*7+i+1,(j+1)*7+i+1,(j+1)*7+i) for j in range(7) for i in range(6)],cloth)
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
courses(g,0,28.2,.38,4.8,4.4,10.3);crown(g,0,28.2,10.8,4.8,4.4);roof(g,0,28.2,10.85,3.2,2.8,.65)
for x in [-8.4,8.4]:courses(g,x,29.5,.38,3.5,5.2,6.8);crown(g,x,29.5,7.3,3.5,5.2);banner(g,x,26.75,6.6,1.15,2.5)
# Deliberately narrow dark window with stone moulding on the main tower.
g.box((0,25.89,7.8),(.8,.08,1.8),wood);arch(g,0,25.78,8.25,.4,.18,.2,trim,segments=7)
portal(g,0,25.86,1.5,3.2,4.1)
for X in [-4.8,4.8]:
 g.box((X,26.84,3.7),(.65,.10,1.45),wood);arch(g,X,26.73,4.05,.33,.17,.24,trim,segments=7)
 for S in [-1,1]:g.box((X+S*.42,26.70,3.6),(.17,.3,1.1),trim)
for X in [-5.8,5.8]:courses(g,X,26.7,1.5,.7,.8,3.7)
# Entrance terrace and physically nested six stair treads.
courses(g,0,25,.38,18,5,1.15)
for row in range(7):
 for col in range(22):
  x=-8.85+(col+.5)*17.7/22;y=22.64+(row+.5)*4.7/7
  g.box((x,y,1.565),(17.7/22-.025,4.7/7-.025,.075),random.choice(stone))
for i in range(6):g.box((0,20.25+i*.46,.38+(i+1)*.095),(6.0,.49,(i+1)*.19),trim)
for x in [-3.6,3.6]:courses(g,x,22,.38,1.2,3.4,.9);torch(g,x,22.9,1.3)
for x in [-2.6,2.6]:
 line(g,(x,25.7,2.7),(x,25.1,3.05),.08,iron);line(g,(x,25.1,2.95),(x,25.1,3.8),.10,wood)
 for j in range(3):
  a=j*2*pi/3;line(g,(x+.12*cos(a),25.1+.12*sin(a),3.7),(x+.035*cos(a+.4),25.1+.035*sin(a+.4),4.2),.09,flame)
line(g,(0,28.2,11.35),(0,28.2,17),.075,wood);banner(g,1.2,28.2,16.6,2.4,1.5)
bastion=g.finish();bastion.location.y=5.5
# Reuse original gate geometry and original in-project procedural material sources.
donor=os.path.join(ROOT,'art-source/valoria/production/lower-gate-family-v1/LowerGateFamilyV1.blend')
with bpy.data.libraries.load(donor,link=False) as (s,d):d.objects=list(s.objects)
objs=[o for o in d.objects if o and o.type=='MESH']
for o in objs:bpy.context.collection.objects.link(o)
bpy.context.view_layer.update();pts=[o.matrix_world@v.co for o in objs for v in o.data.vertices];lo=Vector([min(p[i] for p in pts) for i in range(3)]);hi=Vector([max(p[i] for p in pts) for i in range(3)]);ctr=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z));scale=.95
for o in objs:
 m=o.matrix_world.copy();o.parent=None
 for v in o.data.vertices:v.co=(m@v.co-ctr)*scale+Vector((0,-10,.35))
 o.matrix_world=Matrix.Identity(4);o.name='Gate_'+o.name
# Bridge with genuine through arches, solid spandrels and voussoir bands.
b=Mesh('Bridge_Masonry');b.box((0,-23,.10),(9,24,.70),stone[3])
# Side walls boolean cuts are applied below, retaining editable blend geometry.
def arch_lower(y):
 return max([ -3.675 ]+[ -3.75+math.sqrt(max(0,3.7**2-(y-c)**2)) for c in [-27.25,-18.75] if abs(y-c)<=3.7 ])
def extrude_yz(g,x,depth,poly,material):
 n=len(poly);verts=[(x+side*depth/2,y,z) for side in [-1,1] for y,z in poly]
 g.add(verts,[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)],material)
for X in [-4.12,4.12]:
 # Closed profile extrusion: no overlapping-solid boolean solver or residual veneer.
 samples=sorted(set([-35,-11,-30.95,-23.55,-22.45,-15.05]+[c+3.7*cos(i*pi/48) for c in [-27.25,-18.75] for i in range(49)]))
 side=Mesh('Bridge_ArchSpandrel');vertices=[]
 for y in samples:vertices.extend([(X-.375,y,arch_lower(y)),(X+.375,y,arch_lower(y)),(X+.375,y,-.025),(X-.375,y,-.025)])
 faces=[(3,2,1,0),tuple((len(samples)-1)*4+i for i in range(4))]
 for j in range(len(samples)-1):
  for k in range(4):faces.append((j*4+k,j*4+(k+1)%4,(j+1)*4+(k+1)%4,(j+1)*4+k))
 side.add(vertices,faces,stone[2]);ob=side.finish(.015)
 skin=Mesh('Bridge_ArchSpandrel_DressedCourses')
 for row in range(8):
  z0=-3.675+row*3.65/8+.016;z1=-3.675+(row+1)*3.65/8-.016
  for j in range(27):
   ya=-35+(j-.5*(row%2))*24/26+.016;yb=min(-11,-35+(j+1-.5*(row%2))*24/26)-.016;ya=max(-35,ya)
   if yb<=ya:continue
   pieces=[(ya,yb)]
   # Analytically subtract the arch void at the top of this row. Remaining polygons
   # follow the arch curve at their lower edge instead of stamping a closed block over it.
   for center in [-27.25,-18.75]:
    d=math.sqrt(max(0,3.7**2-(z1+3.75)**2));lo=center-d;hi=center+d;remaining=[]
    for a,c in pieces:
     if hi<=a or lo>=c:remaining.append((a,c))
     else:
      if lo>a:remaining.append((a,lo-.002))
      if hi<c:remaining.append((hi+.002,c))
    pieces=remaining
   for ya,yb in pieces:
    if yb-ya<.025:continue
    lower=[(ya+(yb-ya)*k/6,max(z0,arch_lower(ya+(yb-ya)*k/6))) for k in range(7)]
    if any(z>=z1 for y,z in lower):continue
    poly=lower+[(yb,z1),(ya,z1)]
    for side_sign in [-1,1]:extrude_yz(skin,X+side_sign*.40,.12,poly,random.choice(stone))
 veneer=skin.finish(.012)
 bpy.context.view_layer.update()
 for test in [ob,veneer]:
  for Y in [-27.25,-18.75]:assert not test.ray_cast(Vector((-7,Y,-1.2)),Vector((1,0,0)),distance=14)[0], 'Profile bridge arch is obstructed'
 assert ob.ray_cast(Vector((-7,-23,-1.2)),Vector((1,0,0)),distance=14)[0], 'Bridge central pier missing'
 for Y in [-27.25,-18.75]:arch(b,X,Y,-3.75,3.7,.48,.95,trim,axis='x',segments=19)
 for y in range(-34,-10,2):courses(b,X,y,.45,.7,1.96,.85)
 b.box((X,-23,1.36),(.93,24,.21),trim)
for X in [-4.2,4.2]:
 for Y in [-34,-12]:torch(b,X,Y,.5)
b.finish(.035)
# Paving has real seams, slight stone variation, follows continuous deck/ground levels.
p=Mesh('Axial_Paving')
for ya,yb,w,z in [(-54,-35,5,.40),(-35,-11,7.4,.49),(-8,25.6,5,.40)]:
 y=ya;row=0
 while y<yb:
  n=9 if w>5 else 7
  for i in range(n+1):
   xa=max(-w/2,-w/2+(i-.5*(row%2))*w/n)+.015;xb=min(w/2,-w/2+(i+1-.5*(row%2))*w/n)-.015
   if xb-xa>.04:p.box(((xa+xb)/2,y+.35,z+random.uniform(-.008,.008)),(xb-xa,.66,.10),random.choice(stone))
  y+=.71;row+=1
p.finish(.025)
# Four empty construction plots, true ground plane and dressed stone kerbs.
for x,y in [(-23,28),(-19,9),(29,32),(28,12)]:
 if True:
  p=Mesh('Plot_'+str(x)+'_'+str(y));p.box((x,y,.355),(17,12,.10),soil)
  for j in range(25):
   for yy in [-6,6]:p.box((x-8.5+(j+.5)*17/25,y+yy,.47),(.59,.31,.22),random.choice(stone))
  for j in range(18):
   for xx in [-8.5,8.5]:p.box((x+xx,y-6+(j+.5)*12/18,.47),(.31,.59,.22),random.choice(stone))
  p.finish(.025)
# Ruins are intentionally small, with stepped broken silhouettes and fallen blocks.
for x,y in [(-22,46),(26,52),(-25,0),(46,-1)]:
 r=Mesh('Ruin_'+str(x)+'_'+str(y))
 for i in range(8):
  h=max(.6,3.5-i*.45+random.uniform(-.35,.35));courses(r,x-3+i*.85,y,.3,.83,1.15,h)
 for i in range(8):r.box((x+random.uniform(-4,4),y+random.uniform(-2,2),.5),(random.uniform(.4,.8),.6,.65),random.choice(stone),random.random())
 r.finish(.06)
# Reuse editable tree source family. Fit each donor to a tree-scale silhouette,
# preserve source topology and materials, reusing linked copies for instances.
vegd=os.path.join(ROOT,'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/inputs/pine_medium.glb')
assert hashlib.sha256(open(vegd,'rb').read()).hexdigest()=='003d59a9d184cb6cb04b2b2b21a8cb35c2b92e838cf788575044e576f7846e3e'
before=set(bpy.data.objects);bpy.ops.import_scene.gltf(filepath=vegd)
parts=[o for o in bpy.data.objects if o not in before and o.type=='MESH']
bpy.ops.object.select_all(action='DESELECT')
for o in parts:o.select_set(True)
bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();tree=bpy.context.object;tree.name='CC0_Pine_Template'
M=tree.matrix_world.copy();ps=[M@v.co for v in tree.data.vertices];lo=Vector([min(q[i] for q in ps) for i in range(3)]);hi=Vector([max(q[i] for q in ps) for i in range(3)]);ct=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z));height=hi.z-lo.z
for v,q in zip(tree.data.vertices,ps):v.co=q-ct
tree.matrix_world=Matrix.Identity(4)
mod=tree.modifiers.new('Strategic pine face budget','DECIMATE');mod.ratio=.80;bpy.ops.object.modifier_apply(modifier=mod.name)
positions=[(-53,-39),(-47,-38),(-57,-32),(-38,-43),(26,-42),(39,-43),(47,-39),(-48,-15),(-44,3),(-39,29),(-40,40),(-45,44),(-32,49),(-5,54),(5,57),(39,49),(48,29),(46,2),(42,-12)]
positions += [(-48,-38),(-45,-41),(-42,-43),(-40,-37),(15,-42),(20,-44),(24,-41),(-29,49),(-25,53),(-31,52),(32,59),(44,28),(48,22),(43,20),(-37,31),(-39,36),(-34,40)]
# Preserve the donor mesh but deepen the pale source foliage into forest-green pine.
for material in tree.data.materials:
 if not material or not material.use_nodes or 'Leaves' not in material.name:continue
 for node in material.node_tree.nodes:
  if node.type!='TEX_IMAGE' or not node.image:continue
  im=node.image.copy();im.name='Valoria deep pine '+im.name
  pixels=np.array(im.pixels[:],dtype=np.float32).reshape((-1,4));pixels[:,:3]*=np.array([.40,.61,.37]);im.pixels.foreach_set(pixels.reshape(-1));im.pack();node.image=im
for idx,(x,y) in enumerate(positions):
 o=tree.copy();o.data=tree.data.copy();bpy.context.collection.objects.link(o);o.name='Reused_Fir_%02d'%idx;o.location=(x,y,.3);o.scale=(1,1,1);sc=(6+random.random()*3)/height;o.scale=(sc,sc,sc);o.rotation_euler.z=random.uniform(-pi,pi)
bpy.data.objects.remove(tree,do_unlink=True)
# Closed authored crags replace the rejected open scan face. Four irregular lofted rings,
# broad fractured planes, bevels and metric UVs; no repeated spheres or cliff stamps.
rock_positions=[]
for side in [-1,1]:
 for cluster in range(17):
  cx=random.uniform(-65,65)
  if abs(cx)<6:continue
  for j in range(random.randint(3,7)):
   x=cx+random.uniform(-3.3,3.3)
   if abs(x)<5.4:continue
   y=river_center(x)+side*(river_width(x)+random.uniform(-.85,1.65))
   size=random.uniform(.38,.8) if j>2 else random.uniform(.95,1.65)
   if any((x-px)**2+(y-py)**2 < (.68*(size+ps))**2 for px,py,ps in rock_positions):continue
   rock_positions.append((x,y,size))
for i,(x,y,size) in enumerate(rock_positions):
 crag=Mesh('River_Rock_%03d'%i);verts=[];segments=9;rx=size*random.uniform(.8,1.5);ry=size*random.uniform(.65,1.05);height=size*random.uniform(.9,1.65);angle=random.uniform(-pi,pi)
 for ring,(z,radius) in enumerate([(-.5,.58),(-.24,.94),(.22,1.0),(.5,.5)]):
  phase=random.uniform(-.10,.10)
  for j in range(segments):
   t=angle+j*2*pi/segments+phase;r=radius*random.uniform(.86,1.13)
   verts.append((x+cos(t)*rx*r,y+sin(t)*ry*r,height*z+random.uniform(-.07,.07)))
 faces=[tuple(range(segments-1,-1,-1)),tuple(3*segments+j for j in range(segments))]
 faces += [(ring*segments+j,ring*segments+(j+1)%segments,(ring+1)*segments+(j+1)%segments,(ring+1)*segments+j) for ring in range(3) for j in range(segments)]
 # Different tones on fracture planes give volume without a noisy texture stamp.
 crag.add(verts,faces,rocks[2])
 for material in rocks:
  if material not in crag.ms:crag.ms.append(material)
 tone=random.randrange(len(rocks));crag.mi=[crag.ms.index(rocks[max(0,min(len(rocks)-1,tone+random.choice([-1,0,0,0,1])))]) for face in faces]
 crag.finish(.045)
# Grounded irregular stones around woodland and ruins give the open land scale cues.
for i,(X,Y) in enumerate([(-22,46),(26,52),(-25,0),(46,-1)]+positions[::3]):
 for j in range(3):
  x=X+random.uniform(-2,2);y=Y+random.uniform(-1.8,1.8)
  if any(abs(x-px)<9 and abs(y-py)<6.5 for px,py in [(-23,28),(-19,9),(29,32),(28,12)]):continue
  bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=(x,y,0));o=bpy.context.object;o.name='Ground_Rock'
  for v in o.data.vertices:v.co*=1+.13*sin(v.co.x*7+v.co.y*9+v.co.z*3+i)
  o.scale=(random.uniform(.4,.9),random.uniform(.35,.7),random.uniform(.4,.85));o.rotation_euler.z=random.uniform(-pi,pi);o.data.materials.append(random.choice(rocks));bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
# Dense, small ornamental shrubs keep architecture grounded and plots free.
leaf=[mat('Shrub leaf '+str(i),(.11+i*.028,.18+i*.031,.028+i*.01)) for i in range(4)]
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1)
template=bpy.context.object;iv=[v.co.copy() for v in template.data.vertices];iff=[tuple(p.vertices) for p in template.data.polygons];bpy.data.objects.remove(template,do_unlink=True)
sh=Mesh('Foundation_shrubs_and_groundcover')
centers=[(-10,31.5),(10,31.5),(-10,39.5),(10,39.5),(-5,-11),(5,-11),(-11,-9),(11,-9)]
centers+=positions+[(-22,46),(26,52),(-25,0),(46,-1)]
shrub_path=os.path.join(ROOT,'docs/evidence/valoria-layered-construction-20261016/entry-zone-v7/inputs/shrub_1.glb')
assert hashlib.sha256(open(shrub_path,'rb').read()).hexdigest()=='5da94bd40f1afc7b03dc1fb052352ca97b986b35aafb49a69249cebb876f14f0'
before=set(bpy.data.objects);bpy.ops.import_scene.gltf(filepath=shrub_path);shrubs=[o for o in bpy.data.objects if o not in before and o.type=='MESH'];assert shrubs
for idx,(x,y) in enumerate(centers[::2]):
 for donor_shrub in shrubs:
  o=donor_shrub.copy();o.data=donor_shrub.data.copy();bpy.context.collection.objects.link(o);o.parent=None;o.name='Reused_Shrub_%02d'%idx
  M=donor_shrub.matrix_world.copy();ps=[M@v.co for v in donor_shrub.data.vertices];lo=Vector([min(q[i] for q in ps) for i in range(3)]);hi=Vector([max(q[i] for q in ps) for i in range(3)]);ct=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z));sc=.75/max(.1,hi.z-lo.z)
  for v,q in zip(o.data.vertices,ps):v.co=(q-ct)*sc+Vector((x,y,.32))
  o.matrix_world=Matrix.Identity(4)
for o in shrubs:bpy.data.objects.remove(o,do_unlink=True)
# Small overlapping three-lobed bushes mass around foundations, never over plots/stairs.
for X,Y in [(-10,32),(10,32),(-10.8,35),(10.8,36),(-8,39),(8,39),(-6,29),(6,29),(-11,-10),(11,-10),(-22,46),(26,52),(-25,0),(46,-1)]:
 for j in range(random.randint(3,6)):
  x=X+random.uniform(-1.3,1.3);y=Y+random.uniform(-1,1);z=terrain_height(x,y)
  scale=random.uniform(.40,.85)
  for k in range(3):
   cx=x+random.uniform(-.35,.35);cy=y+random.uniform(-.3,.3);cz=z+scale*.55+k*.13
   verts=[(cx+v.x*scale,cy+v.y*scale,cz+v.z*scale*.85) for v in iv]
   sh.add(verts,iff,random.choice(leaf))
   for material in leaf:
    if material not in sh.ms:sh.ms.append(material)
   sh.mi[-len(iff):]=[sh.ms.index(random.choice(leaf)) for face in iff]
# Sparse closed tapered reeds in shoreline pockets, not a uniform band.
for side in [-1,1]:
 for X in [-55,-34,-18,16,32,49]:
  for j in range(8):
   x=X+random.uniform(-1.2,1.2);y=river_center(x)+side*(river_width(x)+random.uniform(.7,1.6));z=terrain_height(x,y)-.05
   h=random.uniform(.45,.95);a=random.random()*2*pi;r=.055
   verts=[(x+r*cos(a+k*2*pi/3),y+r*sin(a+k*2*pi/3),z) for k in range(3)]+[(x+cos(a)*h*.3,y+sin(a)*h*.3,z+h)]
   sh.add(verts,[(2,1,0),(0,1,3),(1,2,3),(2,0,3)],random.choice(leaf))
# Sporadic low clumps break broad grass fields without invading the construction areas.
for i in range(800):
 x=random.uniform(-60,60);y=random.uniform(-48,65)
 if abs(y-river_center(x))<river_width(x)+2 or abs(x)<11 and -40<y<42:continue
 if any(abs(x-px)<9.3 and abs(y-py)<7 for px,py in [(-23,28),(-19,9),(29,32),(28,12)]):continue
 for k in range(5):
  a=random.random()*2*pi;w=random.uniform(.045,.075);h=random.uniform(.18,.40);cx=x+random.uniform(-.22,.22);cy=y+random.uniform(-.22,.22);z=terrain_height(cx,cy)+.01
  verts=[(cx+w*cos(a+j*2*pi/3),cy+w*sin(a+j*2*pi/3),z) for j in range(3)]+[(cx+.12*cos(a),cy+.12*sin(a),z+h)]
  sh.add(verts,[(2,1,0),(0,1,3),(1,2,3),(2,0,3)],random.choice(leaf))
sh.finish(0)
# Seat actual low foot vertices against exactly the terrain equation used by Unity.
for o in bpy.context.scene.objects:
 if o.type!='MESH' or not o.name.startswith(('River_Rock','Ground_Rock','Reused_Fir_','Reused_Shrub_')):continue
 pts=[o.matrix_world@v.co for v in o.data.vertices];low=min(p.z for p in pts);high=max(p.z for p in pts)
 fraction=.15 if o.name.startswith(('River_Rock','Ground_Rock')) else .035
 foot=[p for p in pts if p.z<=low+(high-low)*fraction]
 offsets=sorted(terrain_height(p.x,p.y)-p.z for p in foot)
 o.location.z+=offsets[len(offsets)//2]-.10
# Shared static environmental meshes reduce material submissions; source islands stay editable.
for prefix,label in [('River_Rock','River_bank_rocks'),('Ground_Rock','Woodland_ground_rocks'),('Reused_Fir_','Fir_tree_group'),('Reused_Shrub_','Shrub_group')]:
 group=[o for o in bpy.context.scene.objects if o.type=='MESH' and o.name.startswith(prefix)]
 bpy.ops.object.select_all(action='DESELECT')
 for o in group:o.select_set(True)
 if group:bpy.context.view_layer.objects.active=group[0];bpy.ops.object.join();bpy.context.object.name=label
# Rebase each editable semantic mesh to its base-centre pivot without moving geometry.
for o in bpy.context.scene.objects:
 if o.type!='MESH':continue
 M=o.matrix_world.copy();points=[M@v.co for v in o.data.vertices]
 center=Vector(((min(p.x for p in points)+max(p.x for p in points))/2,(min(p.y for p in points)+max(p.y for p in points))/2,min(p.z for p in points)))
 for v,p in zip(o.data.vertices,points):v.co=p-center
 o.matrix_world=Matrix.Translation(center)
# Texture copies are sized for this strategic camera; original donor files stay unchanged.
for image in bpy.data.images:
 if image.size[0]>1024 or image.size[1]>1024:
  factor=1024/max(image.size);image.scale(max(1,int(image.size[0]*factor)),max(1,int(image.size[1]*factor)));image.pack()
# Source hashes and editability are evidence, never an artistic approval.
for o in bpy.context.scene.objects:
 if o.type=='MESH':
  for slot in o.material_slots:
   if slot.material and slot.material.use_nodes:
    for n in slot.material.node_tree.nodes:
     if n.type=='TEX_IMAGE' and n.image and not n.image.packed_file:
      try:n.image.pack()
      except Exception:pass
bpy.context.preferences.filepaths.save_version=0
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(EV,'Valoria_Reference_Clean.blend'))
bpy.ops.export_scene.gltf(filepath=os.path.join(OUT,'Valoria_Reference_Clean.glb'),export_format='GLB',export_apply=True,export_yup=True,export_texcoords=True,export_normals=True,export_materials='EXPORT')
sha=lambda p:hashlib.sha256(open(p,'rb').read()).hexdigest()
json.dump({'author':'Reference analysis and spatial/model decisions by Codex; Blender bpy mesh operations executed natively','donors':[{'path':p,'sha256':sha(p)} for p in [donor,vegd,shrub_path]],'scene_new_empty':True,'external_downloads':[],'art_approved':False,'per_mesh_triangles':{o.name:sum(len(p.vertices)-2 for p in o.data.polygons) for o in bpy.context.scene.objects if o.type=='MESH'},'mesh_objects':len([o for o in bpy.context.scene.objects if o.type=='MESH']),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in bpy.context.scene.objects if o.type=='MESH'),'blend_sha256':sha(os.path.join(EV,'Valoria_Reference_Clean.blend')),'glb_sha256':sha(os.path.join(OUT,'Valoria_Reference_Clean.glb'))},open(os.path.join(EV,'source-art.json'),'w'),indent=2)
print('VALORIA_REFERENCE_CLEAN_BLENDER_COMPLETE')
