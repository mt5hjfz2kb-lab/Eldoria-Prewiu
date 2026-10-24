"""Production geography candidate from certified AI. Preserve source; never bake gameplay.
ChatGPT-directed Blender authoring under the current owner exception; independent Unity review required.
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
def coast(x,y):return 720+49*np.sin(y/173)+27*np.sin(y/67)+18*np.cos(y/229)-x
def rivercentre(x):
 side=0 if x<0 else 1;d=max(0,abs(x)-96)
 return ends[side]+(54*math.sin(d/145)+19*math.sin(d/57+.4)-19*math.sin(.4))*float(smooth(d/65))
def riverwidth(x):
 d=max(0,abs(x)-96);return river_widths[0 if x<0 else 1]+(.65+.35*math.sin(d/53))*float(smooth(d/80))
# Paths authored as connected corridors with explicit sector exits and urban approaches.
routes=[[(12,270),(42,350),(20,480),(58,650),(15,950)],[(96,193),(180,205),(320,260),(460,330),(710,360)],[(-96,231),(-220,250),(-350,330),(-520,390),(-860,420)],[(0,-78),(30,-180),(4,-350),(-80,-520),(-20,-940)],[(-520,390),(-580,160),(-530,-120),(-600,-400),(-480,-740)],[(460,330),(420,140),(480,-120),(390,-350),(440,-760)],[(-530,-120),(-320,-180),(-120,-210),(30,-180),(220,-150),(480,-120)],[(-350,330),(-270,560),(-60,720),(200,760),(380,680),(620,750)]]
# Real north/south continental continuation; existing geography coordinates stay fixed.
routes += [[(15,950),(-35,1120),(20,1380),(-55,1720)],[(620,750),(550,1020),(590,1290),(520,1690)],[(-860,420),(-720,880),(-650,1180),(-740,1660)],[(-650,1180),(-340,1280),(20,1380),(300,1320),(590,1290)],[(-20,-940),(65,-1150),(10,-1400),(80,-1740)],[(-480,-740),(-560,-1050),(-520,-1340),(-650,-1700)],[(440,-760),(520,-1090),(420,-1390),(510,-1690)],[(-520,-1340),(-230,-1430),(10,-1400),(220,-1300),(420,-1390)]]
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
def scalar_path_distance(x,y,paths):
 result=1e6
 for path in paths:
  for (ax,ay),(bx,by) in zip(path,path[1:]):
   dx=bx-ax;dy=by-ay;t=max(0.,min(1.,((x-ax)*dx+(y-ay)*dy)/(dx*dx+dy*dy)))
   result=min(result,math.hypot(x-ax-t*dx,y-ay-t*dy))
 return result
def fault_distance(X,Y):
 if np.ndim(X)==0:return scalar_path_distance(float(X),float(Y),faults)
 distance=np.full(np.shape(X),1e6)
 for path in faults:
  for (ax,ay),(bx,by) in zip(path,path[1:]):
   t=np.clip(((X-ax)*(bx-ax)+(Y-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2),0,1);distance=np.minimum(distance,np.hypot(X-ax-t*(bx-ax),Y-ay-t*(by-ay)))
 return distance
groves=[(-220,420,42,65),(-430,680,62,40),(-650,790,65,50),(-750,500,48,70),(-440,210,55,50),(-730,90,60,65),(-280,90,50,38),(-410,-280,55,45),(-750,-370,52,62),(-270,-470,60,45),(-610,-800,65,48),(-130,-800,45,60),(120,-610,60,50),(330,-820,45,60),(650,-650,45,70),(600,-370,45,50),(230,-320,55,48),(240,30,45,65),(540,70,55,45),(300,420,60,50),(580,540,55,65),(100,600,50,45),(-130,900,55,45),(390,910,65,45),(630,900,40,60)]
groves += [(-620,1430,68,95),(-270,1120,70,65),(270,1550,85,70),(620,1470,60,90),(-740,-1160,55,85),(-300,-1660,85,65),(240,-1570,80,80),(650,-1300,55,90),(-130,140,20,40),(136,150,20,38)]
# Designed connected forest lobes, with unequal clearings and broken margins.
# One field drives pigment, placement AND route exclusion.
groves=[(x,y,rx*2.15,ry*2.15) for x,y,rx,ry in groves]
# Unequal connected woodland belts, retaining the explicit city/road clearings.
for i,sx,sy in [(1,1.65,.85),(2,1.3,1.1),(5,.85,1.6),(8,.85,1.55),(10,1.6,.9),(12,.9,1.45),(17,1.4,.85),(19,1.55,.85),(23,1.6,.85)]:
 x,y,rx,ry=groves[i];groves[i]=(x,y,rx*sx,ry*sy)
def grove_field(X,Y,index):
 cx,cy,rx,ry=groves[index]
 wx=X-cx+(26+index%4*4)*np.sin((Y-cy)/67+index*.8)+13*np.sin((Y-cy)/143-index*.2)
 wy=Y-cy+(24+index%3*5)*np.sin((X-cx)/79+index*.6)
 angle=np.arctan2(wy/ry,wx/rx)
 rim=1+.23*np.sin(3*angle+index)+.11*np.sin(5*angle-index*.7)+.055*np.cos(7*angle+index*.4)
 d=(wx/rx)**2+(wy/ry)**2
 return np.clip((rim-d)*1.8,0,1)
def forest_field(X,Y):
 result=np.zeros(np.shape(X))
 for index in range(len(groves)):result=np.maximum(result,grove_field(X,Y,index))
 return result
cities=[]
for y in [-1500,-1180,-740,-480,-250,330,530,810,1120,1530]:
 for x in [-650,-330,170,520]:
  if all(((x-a)/rx)**2+((y-b)/ry)**2>2.5 for a,b,rx,ry in [(a,b,rx/2.15,ry/2.15) for a,b,rx,ry in groves]) and abs(y-rivercentre(x))>35:cities.append((x,y,18))

def rawheight(x,y):
 h=5+1.4*math.sin(x/97+y/170)+.9*math.cos(y/83-x/145)
 for cx,cy,rx,ry,amp in [(-490,520,100,160,28),(380,680,90,145,23),(-580,-570,100,95,19),(-390,1470,150,210,32),(320,-1510,180,160,26)]:h+=amp*math.exp(-((x-cx)/rx)**2-((y-cy)/ry)**2)
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
 blend=float(smooth(d/110));edge=boundaryheight(qx,qy);tangent=0.
 # Continue measured outward slopes as well as positions, then merge into the
 # continental relief. A constant-height collar made the retained tile readable.
 if x<-96:tangent+=max(-.6,min(.6,(edge-boundaryheight(qx+2,qy))/2))*(-96-x)
 elif x>96:tangent+=max(-.6,min(.6,(edge-boundaryheight(qx-2,qy))/2))*(x-96)
 if y<-78:tangent+=max(-.6,min(.6,(edge-boundaryheight(qx,qy+2))/2))*(-78-y)
 elif y>270:tangent+=max(-.6,min(.6,(edge-boundaryheight(qx,qy-2))/2))*(y-270)
 # Do not extrude the old edge river incision straight through the new meander.
 # Replace that abandoned cross-section with its measured bank shoulders; the
 # actual curved channel below is then carved once along rivercentre(x).
 if abs(x)>96:
  side=0 if x<0 else 1;axis=ends[side];rw0=river_widths[side];shoulder=rw0+14
  old_channel=(1-float(smooth((abs(qy-axis)-rw0-5)/10)))*float(smooth(d/24))
  lo=boundaryheight(qx,axis-shoulder);hi=boundaryheight(qx,axis+shoulder)
  fraction=max(0,min(1,(qy-axis+shoulder)/(2*shoulder)))
  edge=edge*(1-old_channel)+(lo*(1-fraction)+hi*fraction)*old_channel
  tangent*=1-old_channel
 h=(edge+tangent*math.exp(-d/48))*(1-blend)+rawheight(x,y)*blend
 if abs(x)>96:
  dr=abs(y-rivercentre(x));rw=riverwidth(x)
  # Continue the measured source cross-section from the very first extension row.
  # The previous 60 m delayed incision left solid ground occluding the water.
  bank=float(smooth((rw+10-dr)/9));bed=water_height-.55+1.6*float(smooth((dr-rw+1)/4))
  edge_profile=boundaryheight(-95.999 if x<0 else 95.999,ends[0 if x<0 else 1]+y-rivercentre(x))
  channel=edge_profile*(1-float(smooth((abs(x)-96)/24)))+bed*float(smooth((abs(x)-96)/24))
  h=h*(1-bank)+channel*bank
 for cx,cy,radius in cities:
  distance=math.hypot(x-cx,y-cy)
  if distance<radius+14:
   h=rawheight(cx,cy)+(h-rawheight(cx,cy))*float(smooth((distance-radius)/14))
 c=float(coast(x,y));h=h*float(smooth((c+5)/20))-.6*(1-float(smooth((c+5)/20)))
 return h
# Sample every actual perimeter vertex so the seam has matching edge coordinates.
xs=set(float(x) for x in np.arange(-910,911,10));ys=set(float(y) for y in np.arange(-1800,1801,10))
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
 if np.ndim(X)==0:return scalar_path_distance(float(X),float(Y),routes)
 d=np.full(np.shape(X),1e6)
 for route in routes:
  for (ax,ay),(bx,by) in zip(route,route[1:]):
   t=np.clip(((X-ax)*(bx-ax)+(Y-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2),0,1);d=np.minimum(d,np.hypot(X-ax-t*(bx-ax),Y-ay-t*(by-ay)))
 return d
# Reviewed clear-grass patch from the original AF atlas (no path/forest features).
# Reuse its measured mean and fine pigment; mirror into a seamless data tile.
af=bpy.data.images['AF broad meadow flows'];af_values=np.empty(len(af.pixels),np.float32);af.pixels.foreach_get(af_values);af_pixels=af_values.reshape(af.size[1],af.size[0],4)
patch=af_pixels[int(af.size[1]*.90):int(af.size[1]*.98),int(af.size[0]*.02):int(af.size[0]*.075),:3].copy()
meadow_mean=patch.mean(axis=(0,1))
# Remove only the patch's low-frequency illumination before repeating. Preserving
# that gradient made a visible rectangular weave at regional scale.
fy=np.fft.fftfreq(patch.shape[0])[:,None];fx=np.fft.fftfreq(patch.shape[1])[None,:]
gaussian=np.exp(-2*math.pi**2*4.0**2*(fx*fx+fy*fy))
# The original pigment is sRGB; the Unity detail texture is linear data.
# Normalize in linear light, otherwise the added contrast is about 2.2x too
# weak and exposes a rectangular texture-density seam beside the retained I/AF.
linear_patch=np.where(patch<=.04045,patch/12.92,((patch+.055)/1.055)**2.4)
linear_mean=linear_patch.mean(axis=(0,1))
low=np.stack([np.fft.ifft2(np.fft.fft2(linear_patch[:,:,c])*gaussian).real for c in range(3)],axis=2)
relative_grain=(linear_patch-low)/np.maximum(linear_mean,.001)
detail=np.clip(.5+relative_grain*.5,.1,.9)
print('AF_LINEAR_GRAIN_RELATIVE_STD',relative_grain.std(axis=(0,1)).tolist())
detail=np.concatenate([detail,detail[:,::-1]],axis=1);detail=np.concatenate([detail,detail[::-1]],axis=0)
detail_pixels=np.ones((*detail.shape[:2],4),np.float32);detail_pixels[:,:,:3]=detail
detail_image=bpy.data.images.new('AF derived continental grass detail',detail.shape[1],detail.shape[0]);detail_image.colorspace_settings.name='Non-Color';detail_image.use_fake_user=True;detail_image.pixels.foreach_set(detail_pixels.ravel());detail_image.filepath_raw=str(out/'RegionOneGrassDetail.png');detail_image.file_format='PNG';detail_image.save();detail_image.pack()
print('AF_DERIVED_MEADOW_PALETTE',meadow_mean.tolist())
S=3072;X,Y=np.meshgrid(np.linspace(-910,910,S),np.linspace(-1800,1800,S));road=line_distance(X,Y)
rng=np.random.default_rng(791)
# Independent editable masks: designed meadow flows, ridge glaze, litter, roads, banks.
# No change to a single pixel/material/UV of the retained core.
wash=.004*np.sin(X/93+Y/123)+.003*np.sin(Y/47-X/61)+.002*np.sin(X/19+Y/37)
grain=rng.normal(0,.008,X.shape)
rgba=np.ones((S,S,4),np.float32);rgba[:,:,:3]=meadow_mean+(wash+grain*.25)[:,:,None]
meadow=np.zeros(X.shape);ridge=np.zeros(X.shape)
for n,(cx,cy,rx,ry) in enumerate([(-620,680,330,105),(-350,430,290,90),(260,610,340,120),(410,880,270,100),(-540,-370,340,100),(-280,-680,350,150),(280,-520,330,110),(480,-170,220,100),(-220,45,290,60),(-330,1480,390,170),(350,1270,370,130),(-400,-1240,350,150),(310,-1580,360,180)]):
 crest=cy+35*np.sin((X-cx)/120+n*.7)+14*np.sin((X-cx)/53+n)
 mass=np.exp(-((X-cx)/rx)**4)*np.exp(-((Y-crest)/ry)**4)
 weight=mass*(.86+.14*np.sin(X*.045+Y*.019+n)*np.cos(Y*.039-X*.021))*.72
 tone=meadow_mean*np.array([1.18,1.09,.91]) if n%3 else meadow_mean*np.array([.72,.94,.86])
 rgba[:,:,:3]=rgba[:,:,:3]*(1-weight[:,:,None])+tone*weight[:,:,None]
 meadow=np.maximum(meadow,weight)
for cx,cy,rx,ry in [(-490,520,100,160),(380,680,90,145),(-580,-570,100,95)]:
 ridge=np.maximum(ridge,np.exp(-((X-cx)/rx)**2-((Y-cy)/ry)**2))
ridge*=.38*(.87+.13*np.sin(X*.08+Y*.05))
rgba[:,:,:3]=rgba[:,:,:3]*(1-ridge[:,:,None])+np.array([.61,.56,.39])*ridge[:,:,None]
# Paint authored, irregular grassland-scale fields on NEW extension ground only.
exec(compile(pathlib.Path(__file__).with_name('author_continental_macro_pigment.py').read_text(encoding='utf-8'),'author_continental_macro_pigment.py','exec'),globals())
paint_canonical_macro_pigment(rgba,X,Y,(-910,-1800,910,1800),meadow_mean)
# Directed AF strokes at terrain scale, rather than soft repeating continental
# waves. These are pigment only: no invented fields, props or resource markers.
# Their centres follow the authored route corridors, but stay off the worn path.
prairie_paint=np.zeros(X.shape,np.float32)
for route_index,route in enumerate(routes[:16]):
 for node_index in range(1,len(route)-1,3):
  ax,ay=route[node_index-1];bx,by=route[node_index+1]
  tangent=np.array([bx-ax,by-ay]);tangent/=max(np.linalg.norm(tangent),.001)
  normal=np.array([-tangent[1],tangent[0]])
  for side in (-1,1):
   n=route_index*97+node_index*11+(side+1)
   cx,cy=np.array(route[node_index])+normal*side*(42+n%53)
   rx=37+n%47;ry=14+(n*7)%23;extent=rx+ry+15
   ix0=max(0,int((cx-extent+910)/1820*(S-1)));ix1=min(S,int((cx+extent+910)/1820*(S-1))+2)
   iy0=max(0,int((cy-extent+1800)/3600*(S-1)));iy1=min(S,int((cy+extent+1800)/3600*(S-1))+2)
   if ix1<=ix0 or iy1<=iy0:continue
   area=np.s_[iy0:iy1,ix0:ix1];px=X[area]-cx;py=Y[area]-cy
   u=px*tangent[0]+py*tangent[1];v=px*normal[0]+py*normal[1]
   crest=4.5*np.sin(u/19+n*.7)+2.5*np.sin(u/9+n*1.3)
   spine=np.exp(-(u/rx)**4)*np.exp(-((v-crest)/ry)**4)
   edges=.86+.14*np.sin(u*.24+v*.11+n*1.1)*np.cos(v*.29-u*.08)
   weight=np.clip(spine*edges*.47,0,.47)*smooth((road[area]-8)/10)
   tone=np.array([(.66,.59,.34),(.44,.50,.27),(.56,.52,.30)][n%3])
   rgba[area][:,:,:3]=rgba[area][:,:,:3]*(1-weight[:,:,None])+tone*weight[:,:,None]
   prairie_paint[area]=np.maximum(prairie_paint[area],weight)
forest=forest_field(X,Y)
# Forest clearings retain the real road network and city access footprints.
for cx,cy,r in cities:forest*=smooth((np.hypot(X-cx,Y-cy)-r-8)/14)
forest*=smooth((road-8)/8)
river_axis=np.array([rivercentre(float(x)) for x in X[0]])[None,:]
river_half=np.array([riverwidth(float(x)) for x in X[0]])[None,:]
dr=np.abs(Y-river_axis)
forest*=smooth((dr-river_half-8)/12)
for bx in [-550.,0.,450.]:forest*=smooth(np.maximum(np.abs(X-bx)-12,np.abs(Y-rivercentre(bx))-30)/6)
litter=forest*.69
rgba[:,:,:3]=rgba[:,:,:3]*(1-litter[:,:,None])+np.array([.25,.34,.19])*litter[:,:,None]
banks=(1-smooth((dr-river_half-1)/10))*.5
rgba[:,:,:3]=rgba[:,:,:3]*(1-banks[:,:,None])+np.array([.55,.51,.34])*banks[:,:,None]
coastal=smooth((26-coast(X,Y))/24)*.45
rgba[:,:,:3]=rgba[:,:,:3]*(1-coastal[:,:,None])+np.array([.59,.54,.37])*coastal[:,:,None]
fracture=smooth((36-fault_distance(X,Y))/28)*.58
rgba[:,:,:3]=rgba[:,:,:3]*(1-fracture[:,:,None])+np.array([.37,.32,.36])*fracture[:,:,None]
# Exposed shale facets follow only the already incised geological corridor.
# Independent bounded UV mask: no invented boulders, portal or navigation change.
shale_facets=np.zeros(X.shape,np.float32)
area=np.s_[max(0,int((5+1800)/3600*(S-1))):int((295+1800)/3600*(S-1))+2,
           int((-420+910)/1820*(S-1)):int((-100+910)/1820*(S-1))+2]
fx=X[area];fy=Y[area];fd=fault_distance(fx,fy)
first=np.full(fx.shape,np.inf);second=first.copy();cell=np.zeros(fx.shape,np.int32)
rock_rng=np.random.default_rng(241017)
seeds=[]
for sy in np.arange(0,312,16):
 for sx in np.arange(-430,-83,16):
  seeds.append((sx+rock_rng.uniform(-5,5),sy+rock_rng.uniform(-5,5)))
for index,(sx,sy) in enumerate(seeds):
 d=np.hypot(fx-sx,fy-sy);near=d<first
 second=np.where(near,first,np.minimum(second,d))
 cell=np.where(near,index,cell);first=np.minimum(first,d)
joint=1-smooth((second-first)/1.2)
rock_weight=(1-smooth((fd-13)/11))*.66
rock_tone=np.array([[.43,.40,.42],[.49,.45,.44],[.37,.34,.39],[.54,.49,.44]])[cell%4]
rock_tone=rock_tone*(1-joint[:,:,None]*.62)
rgba[area][:,:,:3]=rgba[area][:,:,:3]*(1-rock_weight[:,:,None])+rock_tone*rock_weight[:,:,None]
shale_facets[area]=rock_weight
# Grounded violet mineral stain at the actual fracture anchor, not a portal.
fracture_heart=np.exp(-((X+260)/55)**2-((Y-150)/65)**2)*.65
fracture_heart*=.85+.15*np.sin(X*.23+Y*.19)*np.cos(Y*.17-X*.11)
rgba[:,:,:3]=rgba[:,:,:3]*(1-fracture_heart[:,:,None])+np.array([.22,.09,.29])*fracture_heart[:,:,None]
mask=smooth((3.8-road+.3*np.sin(X*.72+Y*.29))/2.1)*(.73+.17*np.sin(X*.31+Y*.49)+.10*np.cos(Y*.64-X*.28))
rgba[:,:,:3]=rgba[:,:,:3]*(1-mask[:,:,None])+(np.array([.56,.49,.33])+grain[:,:,None])*mask[:,:,None]
# Transfer AF's broken middle-scale brush language, between macro washes and
# source-derived fine grass. Keep roads, forest litter and original UVs separate.
brush=(.5+.5*np.sin(X*.37+Y*.21+2*np.sin(Y*.053))*np.sin(Y*.29-X*.16))**2
grass_ink=brush*(1-forest)*(1-mask)*.12
rgba[:,:,:3]=rgba[:,:,:3]*(1-grass_ink[:,:,None])+np.array([.62,.59,.33])*grass_ink[:,:,None]
# Reuse AF's accepted directed meadow strokes at actual woodland clearings.
# Evaluate small bounded patches, not a global random/noise overlay or repeated tile.
habitat_paint=np.zeros(X.shape,np.float32)
for n,(gx,gy,grx,gry) in enumerate(groves):
 for side in range(2):
  angle=n*.73+side*2.37
  cx=gx+math.cos(angle)*grx*1.04;cy=gy+math.sin(angle)*gry*1.04
  rx=38+(n*17+side*13)%34;ry=19+(n*7+side*11)%19
  ix0=max(0,int((cx-rx*1.8+910)/1820*(S-1)));ix1=min(S,int((cx+rx*1.8+910)/1820*(S-1))+2)
  iy0=max(0,int((cy-ry*2.5+1800)/3600*(S-1)));iy1=min(S,int((cy+ry*2.5+1800)/3600*(S-1))+2)
  if ix1<=ix0 or iy1<=iy0:continue
  area=np.s_[iy0:iy1,ix0:ix1];px=X[area];py=Y[area]
  crest=cy+4.5*np.sin((px-cx)/19+n*.7)+2.5*np.sin((px-cx)/9+n*1.3)
  spine=np.exp(-((px-cx)/rx)**4)*np.exp(-((py-crest)/ry)**4)
  broken=.86+.14*np.sin(px*.24+py*.11+n*1.1)*np.cos(py*.29-px*.08)
  weight=np.clip(spine*broken*.43*(1-mask[area])*(1-forest[area]*.52),0,.47)
  weight*=smooth((dr[area]-np.broadcast_to(river_half,X.shape)[area]-3)/9)*smooth(coast(px,py)/20)
  tone=np.array([( .66,.59,.34),(.44,.50,.27),(.56,.52,.30)][(n+side)%3])
  rgba[area][:,:,:3]=rgba[area][:,:,:3]*(1-weight[:,:,None])+tone*weight[:,:,None]
  habitat_paint[area]=np.maximum(habitat_paint[area],weight)
# Raycast the actual original polygons, interpolate their original UVs, then read
# their active image. Only NEW extension pixels receive this 110 m boundary glaze.
image_cache={}
def source_pigment(x,y):
 for ob in land:
  hit,q,normal,fi=ob.ray_cast(Vector((x,y,150)),Vector((0,0,-1)))
  if not hit:continue
  poly=ob.data.polygons[fi];material=ob.data.materials[poly.material_index]
  bs=next(n for n in material.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
  links=bs.inputs['Base Color'].links
  if not links or links[0].from_node.type!='TEX_IMAGE':return np.array(bs.inputs['Base Color'].default_value[:3])
  image=links[0].from_node.image
  if image.name not in image_cache:
   values=np.empty(len(image.pixels),np.float32);image.pixels.foreach_get(values);image_cache[image.name]=values.reshape(image.size[1],image.size[0],4)
  loops=list(poly.loop_indices);layer=ob.data.uv_layers.active
  for k in range(1,len(loops)-1):
   ids=[loops[0],loops[k],loops[k+1]];pts=[ob.data.vertices[ob.data.loops[i].vertex_index].co for i in ids]
   a,b,c=pts;den=(b.y-c.y)*(a.x-c.x)+(c.x-b.x)*(a.y-c.y)
   if abs(den)<1e-10:continue
   u=((b.y-c.y)*(x-c.x)+(c.x-b.x)*(y-c.y))/den;v=((c.y-a.y)*(x-c.x)+(a.x-c.x)*(y-c.y))/den
   if min(u,v,1-u-v)<-.001:continue
   uv=layer.data[ids[0]].uv*u+layer.data[ids[1]].uv*v+layer.data[ids[2]].uv*(1-u-v)
   pixels=image_cache[image.name];iy=np.clip(uv.y*(pixels.shape[0]-1),0,pixels.shape[0]-1);ix=np.clip(uv.x*(pixels.shape[1]-1),0,pixels.shape[1]-1)
   return pixels[int(iy),int(ix),:3]
 return np.array([.44,.49,.265])
edgeY=np.linspace(-77.99,269.99,1024);edgeX=np.linspace(-95.99,95.99,1024)
edges=[np.array([source_pigment(x,float(y)) for y in edgeY]) for x in [-95.99,95.99]]
edges += [np.array([source_pigment(float(x),y) for x in edgeX]) for y in [-77.99,269.99]]
# Do not extrude one-dimensional edge grain into visible radial stripes.
# Keep exact fine pigment only at contact; extend the broad palette separately.
kernel=np.exp(-.5*(np.arange(-96,97)/32)**2);kernel/=kernel.sum()
soft_edges=[np.stack([np.convolve(np.pad(edge[:,c],96,mode='edge'),kernel,mode='valid') for c in range(3)],axis=1) for edge in edges]
qx=np.clip(X,-96,96);qy=np.clip(Y,-78,270);distance=np.hypot(X-qx,Y-qy);glaze=(1-smooth(distance/110))*(distance>0)
for side in range(4):
 choose=((X<-96) if side==0 else (X>96) if side==1 else (Y<-78)&(np.abs(X)<=96) if side==2 else (Y>270)&(np.abs(X)<=96))
 weight=glaze*choose*(1-mask*smooth(distance/3));axis=qy if side<2 else qx;grid=edgeY if side<2 else edgeX
 if side<2:
  old_channel=(1-smooth((np.abs(qy-ends[side])-river_widths[side]-5)/10))*smooth(distance/24)
  weight*=1-old_channel
 for channel in range(3):
  broad=np.interp(axis,grid,soft_edges[side][:,channel]);fine=np.interp(axis,grid,edges[side][:,channel])
  colour=broad+(fine-broad)*(1-smooth(distance/2.5))
  rgba[:,:,channel]=rgba[:,:,channel]*(1-weight)+colour*weight
for name,data in [('Continental meadow flow mask',meadow),('Continental ridge glaze mask',ridge),('Continental connected litter mask',litter),('Continental worn route mask',mask),('Continental broken grass brush mask',grass_ink),('Continental original-edge glaze',glaze),('AF directed woodland clearing strokes',habitat_paint),('AF directed continental prairie strokes',prairie_paint),('Breach incised shale facet mask',shale_facets)]:
 edit=bpy.data.images.new(name,S,S);edit.colorspace_settings.name='Non-Color';edit.use_fake_user=True
 values=np.ones((S,S,4),np.float32);values[:,:,:3]=data[:,:,None];edit.pixels.foreach_set(values.ravel());edit.pack()
im=bpy.data.images.new('Region1 authored meadow and woodland pigment',S,S);im.pixels.foreach_set(rgba.ravel());im.filepath_raw=str(out/'Region1_continental.png');im.file_format='PNG';im.save();im.pack()
mat=bpy.data.materials.new('Region1 open meadows');mat.use_nodes=True;bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Roughness'].default_value=.9;tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);mesh.materials.append(mat)
uv=mesh.uv_layers.new(name='Continental pigment')
for loop in mesh.loops:
 p=mesh.vertices[loop.vertex_index].co;uv.data[loop.index].uv=((p.x+910)/1820,(p.y+1800)/3600)
for p in mesh.polygons:p.use_smooth=True
# Author real neighbouring continental ground while the retained height source is alive.
exec(compile(pathlib.Path(__file__).with_name("author_continental_continuation.py").read_text(),"author_continental_continuation.py","exec"),globals())
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
    pixels=np.empty(len(image.pixels),dtype=np.float32);image.pixels.foreach_get(pixels)
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
core_proof['continental_continuation']=context_proof
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
# Grain follows real plank dimensions; it is a new bridge material, never an AF repaint.
BU,BV=np.meshgrid(np.linspace(0,8,512),np.linspace(0,44,1024))
wood_grain=.012*np.sin(BV*52+2*np.sin(BU*.9))+.008*np.sin(BV*109+BU*.25)
plank_gap=(np.abs((BV+.5)%1-.5)<.035).astype(float)*.065
wood_pixels=np.ones((1024,512,4),np.float32)
# Original flat bridge factor (.31,.22,.12) is linear glTF colour.
# Generated PNG pixels are encoded sRGB: convert explicitly, not by guessing
# a darker encoded palette. Native GLB inspection disproved double tint.
plank_tone=.92+.08*(np.floor(BV)%2)
wood_linear=np.clip((np.array([.31,.22,.12])+(wood_grain-plank_gap)[:,:,None])*plank_tone[:,:,None],0,1)
wood_pixels[:,:,:3]=np.where(wood_linear<=.0031308,wood_linear*12.92,1.055*wood_linear**(1/2.4)-.055)
wood_image=bpy.data.images.new('Authored weathered crossing planks',512,1024)
wood_image.pixels.foreach_set(wood_pixels.ravel());wood_image.pack()
wood_tex=bridge_mat.node_tree.nodes.new('ShaderNodeTexImage');wood_tex.image=wood_image
bridge_mat.node_tree.links.new(wood_tex.outputs['Color'],bridge_bs.inputs['Base Color'])
bridge_bs.inputs['Base Color'].default_value=(1,1,1,1) # Explicit neutral factor; glTF inspection confirms texture colour is authoritative.
for cx in [-550.,0.,450.]:
 cy,z0,z1=crossing_banks[cx]
 # The source-sector castle occupies the same close camera as this crossing.
 # Match the actual measured river width rather than spanning 44 metres of
 # dry grass like a ramp into the castle gate. Other crossing profiles stay.
 # Keep the north end of the castle bridge at its previously proven
 # approach (cy-22m). Shorten only excess southern approach, with an
 # explicit bank-safe allowance beyond the retained river half-width.
 length=44. if abs(cx)>1 else max(34.,min(40.,22.+float(riverwidth(cx))+4.))
 half=length/2
 # Exact 1280×720 Unity A/B shows that decreasing Y moved the bridge
 # AWAY from the gate. Higher Blender Y projects upward at heading 0.
 # Move the deck 10.5 m toward the castle versus rejected -5.5 m test.
 # Align the deck centre horizontally with the gate (~3.3 m rightward).
 deck_centre=cy if abs(cx)>1 else cy+(length-44.)/2+5.0
 bridge_x=cx if abs(cx)>1 else cx+3.3
 segments=int(round(length));vv=[];ff=[];deck_heights=[]
 if abs(cx)<=1:
  # Use the joined physical mesh, not the deleted source Blender objects.
  # The end at positive Y faces the castle; z1 is its original AF/AI
  # bank/gate elevation. Negative Y meets the distant bank.
  terrain_inverse=terrain.matrix_world.inverted()
  def joined_land_height(x,y):
   origin_local=terrain_inverse @ Vector((x,y,150))
   direction_local=terrain_inverse.to_3x3() @ Vector((0,0,-1))
   hit,point,normal,index=terrain.ray_cast(origin_local,direction_local,distance=400)
   assert hit,('Bridge must meet actual joined terrain',x,y)
   return (terrain.matrix_world @ point).z
  z0=joined_land_height(bridge_x,deck_centre-half)+.15
  z1=max(z1,joined_land_height(bridge_x,deck_centre+half)+.15)
 for i in range(segments+1):
  t=i/segments;yy=deck_centre-half+length*t
  z=z0*(1-t)+z1*t+.5*math.sin(t*math.pi)
  # Clamp the deck to real ground: previous trial let terrain occlude
  # the middle planks, visually splitting the crossing into two pieces.
  if abs(cx)<=1:z=max(z,joined_land_height(bridge_x,yy)+.18)
  deck_heights.append(z)
  vv.extend([(bridge_x-4,yy,z),(bridge_x+4,yy,z)])
  if i:ff.append((2*i-2,2*i-1,2*i+1,2*i))
 me=bpy.data.meshes.new('True plank deck');me.from_pydata(vv,[],ff);me.materials.append(bridge_mat);plank=bridge_mat.copy();plank.name='Bridge alternate weathered plank';next(n for n in plank.node_tree.nodes if n.type=='BSDF_PRINCIPLED').inputs['Base Color'].default_value=(.92,.92,.92,1);me.materials.append(plank)
 for face in me.polygons:face.material_index=face.index%2
 uv=me.uv_layers.new(name='UVMap') # Same active layer name as the joined certified terrain
 for loop in me.loops:
  q=me.vertices[loop.vertex_index].co;uv.data[loop.index].uv=((q.x-bridge_x+4)/8,(q.y-deck_centre+half)/length)
 obj=bpy.data.objects.new('Authored crossing '+str(cx),me);scene.collection.objects.link(obj)
 bpy.ops.object.select_all(action='DESELECT');obj.select_set(True);terrain.select_set(True);bpy.context.view_layer.objects.active=terrain;bpy.ops.object.join()
 # Simple measured timber rails beside the river span, outside the full 8 m route.
 rail_material=bridge_mat.copy();rail_material.name='Weathered crossing rail timber'
 rail_vertices=[];rail_faces=[]
 def rail_box(x0,x1,y0,y1,z0,z1):
  start=len(rail_vertices)
  rail_vertices.extend([(x0,y0,z0),(x1,y0,z0),(x1,y1,z0),(x0,y1,z0),(x0,y0,z1),(x1,y0,z1),(x1,y1,z1),(x0,y1,z1)])
  rail_faces.extend([tuple(start+i for i in face) for face in [(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]])
 def deck_at(local_y):
  # Rail posts follow the actual corrected deck heights, not a second
  # analytical surface that would float above/below the corrected planks.
  q=max(0.,min(float(segments),(local_y+half)/length*segments))
  lo=min(segments-1,int(q));f=q-lo
  return deck_heights[lo]*(1-f)+deck_heights[lo+1]*f
 rail_extent=int(min(12,half-2))
 for side in (-1,1):
  for y in range(-rail_extent,rail_extent+1,4):
   z=deck_at(y);rail_box(bridge_x+side*4.18-.12,bridge_x+side*4.18+.12,deck_centre+y-.12,deck_centre+y+.12,z,z+1.25)
  for y in range(-rail_extent,rail_extent,2):
   z=deck_at(y+1)+1.05;rail_box(bridge_x+side*4.18-.09,bridge_x+side*4.18+.09,deck_centre+y,deck_centre+y+2,z,z+.18)
 rail_mesh=bpy.data.meshes.new('Measured river crossing rails');rail_mesh.from_pydata(rail_vertices,[],rail_faces);rail_mesh.materials.append(rail_material)
 rail_uv=rail_mesh.uv_layers.new(name='Weathered timber grain')
 for loop in rail_mesh.loops:
  q=rail_mesh.vertices[loop.vertex_index].co;rail_uv.data[loop.index].uv=((q.x-bridge_x+4.5)/9,(q.y-deck_centre+half)/length)
 rail_object=bpy.data.objects.new('BLD_CrossingRails_'+str(cx),rail_mesh);scene.collection.objects.link(rail_object)
 bridge_specs.append((bridge_x,deck_centre,8,length))
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
 p,n,i,d=bv.ray_cast(Vector((x,y,200)),Vector((0,0,-1)))
 if p:return p.z
 hit,p,n,i=context_ground.ray_cast(Vector((x,y,200)),Vector((0,0,-1)))
 return p.z if hit else -.6
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
 for i in range(647):
  x=side*(96+i*2.5);cy=rivercentre(x);d=abs(x)-96
  if side>0 and float(coast(x,cy))<0:break
  width=riverwidth(x)
  for sign in [-1,1]:vv.append((x,cy+sign*width,water_height))
  if i:ff.append((2*i,2*i+1,2*i-1,2*i-2) if side>0 else (2*i-2,2*i-1,2*i+1,2*i))
 me=bpy.data.meshes.new('Connected water');me.from_pydata(vv,[],ff);me.materials.append(river.data.materials[0]);uv=me.uv_layers.new(name='River flow')
 for loop in me.loops:
  q=me.vertices[loop.vertex_index].co;uv.data[loop.index].uv=((q.x+96)/192,loop.vertex_index%2)
 for face in me.polygons:face.use_smooth=True
 o=bpy.data.objects.new('BLD_RiverExtension_'+str(side),me);scene.collection.objects.link(o)
# Continue the actual O cutbank profiles and their original painted grass/clay/wet-toe
# atlases. These are surface-bound strips, not a generic rectangular shoreline wall.
for direction in [-1,1]:
 for bank_side in [-1,1]:
  original_bank=bpy.data.objects['Pilot_Cutbank_'+str(bank_side)]
  sections=np.array([tuple(v.co) for v in original_bank.data.vertices]).reshape(193,5,3)
  profile=sections[0 if direction<0 else -1];source_axis=ends[0 if direction<0 else 1]
  vv=[];ff=[]
  for row in range(647):
   x=direction*(96+row*2.5);cy=rivercentre(x);d=abs(x)-96
   if direction>0 and float(coast(x,cy))<0:break
   blend=float(smooth(d/42));rw=riverwidth(x)
   for j in range(5):
    t=j/4;offset=bank_side*(rw-.3+4.7*(1-t)**1.35)
    y=cy+(profile[j,1]-source_axis)*(1-blend)+offset*blend
    z=profile[j,2]*(1-blend)+(surface(x,y)+.02)*blend
    vv.append((x,y,z))
   if row:
    for j in range(4):
     a=(row-1)*5+j;face=(a,a+1,a+6,a+5)
     ff.append(face if direction*bank_side>0 else tuple(reversed(face)))
  me=bpy.data.meshes.new('O continued sculpted bank');me.from_pydata(vv,[],ff);me.materials.append(original_bank.data.materials[0])
  uv=me.uv_layers.new(name='Original O bank pigment')
  for loop in me.loops:
   k=loop.vertex_index;q=me.vertices[k].co;uv.data[loop.index].uv=((q.x+96)/192,(k%5)/4)
  for face in me.polygons:face.use_smooth=True
  ob=bpy.data.objects.new('BLD_ConnectedCutbank_'+str(direction)+'_'+str(bank_side),me);scene.collection.objects.link(ob)
# Sea only at the east edge, shaped by the same coastline rather than a rectangle beneath land.
vv=[];ff=[]
for i,y in enumerate(range(-1800,1801,10)):
 c=float(coast(0,y));vv.extend([(c,y,.12),(910,y,.12)])
 if i:ff.append((2*i-2,2*i-1,2*i+1,2*i))
me=bpy.data.meshes.new('Eastern sea');me.from_pydata(vv,[],ff);me.materials.append(watermat);o=bpy.data.objects.new('BLD_EasternSea',me);scene.collection.objects.link(o)
uv=me.uv_layers.new(name='Coastal depth and current')
for loop in me.loops:
 q=me.vertices[loop.vertex_index].co;uv.data[loop.index].uv=(loop.vertex_index%2,(q.y+1800)/3600)
U,V=np.meshgrid(np.linspace(0,1,512),np.linspace(0,1,1024));depth=smooth(U*2.7)
sea_pixels=np.ones((1024,512,4),np.float32)
sea_pixels[:,:,:3]=np.array([.12,.32,.34])*(1-depth[:,:,None])+np.array([.045,.22,.29])*depth[:,:,None]
current=(.007*np.sin(V*55+U*8)+.004*np.sin(V*137-U*21))*(1-U)
sea_pixels[:,:,:3]+=current[:,:,None]
sea_image=bpy.data.images.new('Eastern shelf water pigment',512,1024);sea_image.pixels.foreach_set(sea_pixels.ravel());sea_image.pack()
node=watermat.node_tree.nodes.new('ShaderNodeTexImage');node.image=sea_image;node.extension='EXTEND'
water_bs=next(n for n in watermat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');water_bs.inputs['Base Color'].default_value=(1,1,1,1);watermat.node_tree.links.new(node.outputs['Color'],water_bs.inputs['Base Color'])
# The same real eastern coast continues outside the playable regional rectangle.
vv=[];ff=[]
for y0 in range(-2600,2600,10):
 y1=y0+10
 if -1800<=y0 and y1<=1800:row0=row1=[910.,1710.]
 else:
  # Preserve the same coastal gradient span as the central sea. A single
  # shore-to-1710 quad stretches its 0..1 UV across the whole neighbouring sea.
  row0=[float(coast(0,y0)),910.,1710.];row1=[float(coast(0,y1)),910.,1710.]
 for j in range(len(row0)-1):
  start=len(vv);vv.extend([(row0[j],y0,.12),(row0[j+1],y0,.12),(row1[j+1],y1,.12),(row1[j],y1,.12)])
  ff.append((start,start+1,start+2,start+3))
me=bpy.data.meshes.new('Continuous eastern shelf');me.from_pydata(vv,[],ff);me.materials.append(watermat)
o=bpy.data.objects.new('BLD_EasternSeaContinuation',me);scene.collection.objects.link(o)
uv=me.uv_layers.new(name='Same coastal depth and current')
for loop in me.loops:
 q=me.vertices[loop.vertex_index].co;shore=float(coast(0,q.y))
 uv.data[loop.index].uv=(min(.999,max(0,(q.x-shore)/max(1,910-shore))),(q.y+1800)/3600)

# Natural grouped canopies reuse AI editable tree topology, never old main donors.
masters=[bpy.data.objects['Editable Layered fir source'],bpy.data.objects['Editable Slender fir source']]
# Simplify each derived continental donor ONCE, then share its topology across
# instances. Original masters/core remain unchanged and editable.
shared_masters=[]
for master in masters:
 derived=master.copy();derived.data=master.data.copy();scene.collection.objects.link(derived)
 derived.hide_render=True;derived.hide_set(False);bpy.ops.object.select_all(action='DESELECT');derived.select_set(True);bpy.context.view_layer.objects.active=derived
 mod=derived.modifiers.new('Continental shared crown LOD0','DECIMATE');mod.ratio=.7;bpy.ops.object.modifier_apply(modifier=mod.name)
 shared_masters.append(derived)
masters=shared_masters
tree_total=0
for index,(cx,cy,rx,ry) in enumerate(groves):
 parts=[];positions=[];random=np.random.default_rng(61024+index)
 for attempt in range(2600):
  x=cx+random.uniform(-rx*1.2,rx*1.2);y=cy+random.uniform(-ry*1.2,ry*1.2)
  if float(grove_field(np.array(x),np.array(y),index))<random.uniform(.08,.55):continue
  if -99<x<99 and -81<y<273:continue
  if abs(x)>875 or abs(y)>1770 or float(coast(x,y))<22:continue
  if float(line_distance(np.array(x),np.array(y)))<16 or float(fault_distance(np.array(x),np.array(y)))<34:continue
  if abs(y-rivercentre(x))<riverwidth(x)+9:continue
  if any(abs(x-bx)<15 and abs(y-by)<length/2+12 for bx,by,width,length in bridge_specs):continue
  if any(math.hypot(x-a,y-b)<r+10 for a,b,r in cities):continue
  if any((x-a)**2+(y-b)**2<9.0**2 for a,b in positions):continue
  positions.append((x,y));k=len(positions)
  donor=masters[k%2];o=donor.copy();o.data=donor.data;scene.collection.objects.link(o);o.hide_render=False;o.hide_viewport=False;o.hide_set(False);o.location=(x,y,surface(x,y));o.name='BLD_ForestMass_'+str(index).zfill(2)+'_'+str(k).zfill(3)
  scale=random.uniform(1.3,3.9)*( .85+.15*float(grove_field(np.array(x),np.array(y),index)));core_distance=math.hypot(max(0,abs(x)-96),max(0,-78-y,y-270));scale=1.05+(scale-1.05)*float(smooth(core_distance/160));canopy=1+.35*float(smooth(core_distance/220));o.scale=(scale*random.uniform(1.15,1.5)*canopy,scale*1.25*canopy,scale*random.uniform(.78,1.05));o.rotation_euler.z=random.uniform(0,math.tau);parts.append(o)
  if len(parts)>=(240 if index<25 else 155):break
 if not parts:continue
 tree_total+=len(parts)
 # glTF reuses the same two vertex buffers; each transform remains editable.
# Neighbouring ground carries actual editable fir instances, never forest cards.
for x,y,z,cluster,index in context_tree_roots:
 random=np.random.default_rng(93214+cluster*1000+index)
 donor=masters[index%2];o=donor.copy();o.data=donor.data;scene.collection.objects.link(o)
 o.hide_render=False;o.hide_viewport=False;o.hide_set(False);o.location=(x,y,surface(x,y))
 o.name='BLD_ForestMass_Context_'+str(cluster).zfill(2)+'_'+str(index).zfill(3)
 scale=random.uniform(1.3,3.9);o.scale=(scale*random.uniform(1.15,1.5)*1.35,scale*1.25*1.35,scale*random.uniform(.78,1.05));o.rotation_euler.z=random.uniform(0,math.tau)
 tree_total+=1
print('CONTINENTAL_CONNECTED_FOREST_TREES',tree_total)
# Distant LODs are simplified copies of the same silhouettes; no canopy substitutions.
far_cache={}
for near in list(scene.objects):
 if near.type!='MESH' or not (near.name.startswith('BLD_ForestMass_') or near.name.startswith('Pilot_Pine')):continue
 if near.hide_render:continue
 far=near.copy();far.data=near.data;scene.collection.objects.link(far)
 far.name=near.name.replace('BLD_ForestMass_','BLD_ForestFar_') if near.name.startswith('BLD_ForestMass_') else 'Far_'+near.name
 key=near.data.name
 if key in far_cache:far.data=far_cache[key]
 else:
  far.data=near.data.copy()
  bpy.ops.object.select_all(action='DESELECT');far.select_set(True);bpy.context.view_layer.objects.active=far
  mod=far.modifiers.new('Mobile distant silhouette','DECIMATE');mod.ratio=.09 if near.name.startswith('BLD_ForestMass_') else .16;bpy.ops.object.modifier_apply(modifier=mod.name);far_cache[key]=far.data
# Source meshes remain editable but excluded from export; remove no historical file.
# Export includes only genuinely visible scene geometry and the assembled geography.
anchor=bpy.data.objects.new('BLD_BreachAnchor',None);scene.collection.objects.link(anchor);anchor.location=(-260,150,0)
selected=[anchor]
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render and not o.hide_get():selected.append(o)
bpy.ops.object.select_all(action='DESELECT')
for o in selected:o.select_set(True)
final_faces=Counter(face_signature(terrain,poly) for poly in terrain.data.polygons)
assert not core_faces-final_faces,'Final export changed retained core after bridge/fracture authoring'
core_proof['verified_again_before_final_export']=True
(out/'retained-core-integrity.json').write_text(json.dumps(core_proof,indent=2))
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Region1_Production.blend'),compress=True)
bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneBlenderWorld.glb'),export_format='GLB',use_selection=True)
# Query samples are taken from the actual exported mesh (not the height recipe).
NX,NZ=456,901;data=[]
QX,QY=np.meshgrid(np.linspace(-910,910,NX),np.linspace(-1800,1800,NZ));RD=line_distance(QX,QY);FO=np.zeros(QX.shape)
FO=forest_field(QX,QY)
for cx,cy,r in cities:FO*=smooth((np.hypot(QX-cx,QY-cy)-r-8)/14)
FO*=smooth((RD-8)/8)
qriver=np.array([rivercentre(float(x)) for x in QX[0]])[None,:];qwidth=np.array([riverwidth(float(x)) for x in QX[0]])[None,:]
FO*=smooth((np.abs(QY-qriver)-qwidth-8)/12)
for bx,by,w,l in bridge_specs:FO*=smooth(np.maximum(np.abs(QX-bx)-12,np.abs(QY-by)-l/2-8)/6)
MM=np.zeros((NZ,NX,4),np.uint8);MM[:,:,0]=(np.clip(.5+coast(QX,QY)/32,0,1)*255).astype(np.uint8);MM[:,:,2]=(np.clip(1-RD/5,0,1)*255).astype(np.uint8);MM[:,:,3]=(FO*255).astype(np.uint8)
for j,y in enumerate(np.linspace(-1800,1800,NZ)):
 for i,x in enumerate(np.linspace(-910,910,NX)):
  data.append(surface(float(x),float(y)))
  riverwater=abs(x)>96 and abs(y-rivercentre(x))<riverwidth(x)
  if -96<=x<=96 and -78<=y<=270:
   hit,point,normal,face=river.ray_cast(Vector((x,y,150)),Vector((0,0,-1)));riverwater=hit
  if any(abs(x-cx)<w/2 and abs(y-cy)<length/2 for cx,cy,w,length in bridge_specs):riverwater=False
  MM[j,i,1]=255 if riverwater else 0
masks=MM.tobytes()
with gzip.open(out/'RegionOneBlenderQueries.bytes','wb') as f:
 f.write(struct.pack('<iiiff',0x31424c45,NX,NZ,1820.,3600.));f.write(np.array(data,dtype='<f4').tobytes());f.write(bytes(masks));f.write(struct.pack('<ii',0x32564c45,len(terrain.data.vertices)))
 for v in terrain.data.vertices:f.write(struct.pack('<fff',v.co.x,v.co.z,v.co.y))
(out/'production-region.json').write_text(json.dumps({'source_ai_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'bounds':[-910,-1800,910,1800],'terrain_vertices':len(terrain.data.vertices),'retained_material_samples':core_samples,'faults':[{'ax':a[0],'az':a[1],'bx':b[0],'bz':b[1],'radius':28} for path in faults for a,b in zip(path,path[1:])],'city_reserves':cities,'cities':[{'x':x,'z':y,'radius':rad} for x,y,rad in cities+[(-62,204,15),(-18,250,15),(38,202,15),(67,250,15)]],'forest_groups':len(groves),'bridges':[{'x':x,'z':y,'width':w,'length':l} for x,y,w,l in bridge_specs],'source_seam_max_spread_m':seam_spread,'dynamic_gameplay_baked':0,'art_status':'CHATGPT_DIRECTED_BLENDER_PENDING_VISUAL_ACCEPTANCE','continental_trees':tree_total,'physical_device':False},indent=2))
print('REGION1_AI_CONTINENT_EXPORTED',len(terrain.data.vertices))
