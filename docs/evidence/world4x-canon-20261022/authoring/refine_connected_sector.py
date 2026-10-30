"""Q: repair the observed P texture seam and plantation rows, retaining the welded source."""
import bpy,sys,pathlib,hashlib,json,random,math
import numpy as np
from mathutils import Vector
from mathutils.geometry import interpolate_bezier
out,src=map(pathlib.Path,sys.argv[sys.argv.index('--')+1:]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='f32122c1ab84f0521c1b1a2b648c09845123407f5d3f0f5566b22409ef7c3d81'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene;coll=bpy.data.collections['16 Connected sector P - forest meadow low ridge'];terrain=bpy.data.objects['Pilot_Terrain_1']
def ground(x,y):
 hit,p,n,i=terrain.ray_cast(Vector((x,y,100)),Vector((0,0,-1)));assert hit,(x,y);return p.z
samples=[]
for k in range(3):
 s=bpy.data.objects['Editable P connected path '+str(k)].data.splines[0];pts=[]
 for a,b in zip(s.bezier_points,s.bezier_points[1:]):pts.extend(interpolate_bezier(a.co,a.handle_right,b.handle_left,b.co,32)[:-1])
 pts.append(s.bezier_points[-1].co.copy());samples.append(pts)
poly=[(-83,82),(-47,80),(-26,89),(-31,105),(-23,119),(-41,129),(-57,138),(-81,129),(-88,107)]
def inside(x,y):
 c=False
 for (ax,ay),(bx,by) in zip(poly,poly[1:]+poly[:1]):
  if (ay>y)!=(by>y) and x<(bx-ax)*(y-ay)/(by-ay)+ax:c=not c
 return c
# Controlled Poisson placement inside the existing authored forest shape, with a deliberate inner glade.
rng=random.Random(81023);trees=sorted([o for o in scene.objects if o.name.startswith('Pilot_Pine_P_')],key=lambda o:o.name);points=[]
for attempt in range(20000):
 if len(points)==len(trees):break
 x=rng.uniform(-85,-24);y=rng.uniform(83,136)
 if not inside(x,y) or ((x+62)/9)**2+((y-119)/7)**2<1:continue
 if min((p.x-x)**2+(p.y-y)**2 for s in samples for p in s)<25:continue
 spacing=4.3 if (x+56)**2+(y-103)**2<400 else 5.4
 if any((x-a)**2+(y-b)**2<spacing**2 for a,b in points):continue
 points.append((x,y))
assert len(points)==len(trees)
for o,(x,y) in zip(trees,points):o.location=(x,y,ground(x,y))
m=bpy.data.meshes.new('Q editable forest planting points');m.from_pydata([(x,y,ground(x,y)) for x,y in points],[],[]);ob=bpy.data.objects.new('Editable Q forest planting points',m);coll.objects.link(ob);ob.hide_render=True
# Texture cloning from a verified grass-only part of the O pigment; no copied old path/river silhouettes.
base=bpy.data.images['I recovered exact retained surface'];S=base.size[0];original=np.array(base.pixels[:],np.float32).reshape(S,S,4)
x0,x1=[int((x+96)/192*(S-1)) for x in [22,42]];y0,y1=[int((y+78)/156*(S-1)) for y in [50,67]];patch=original[y0:y1,x0:x1]
W=2048;H=1024;X,Y=np.meshgrid(np.linspace(-96,96,W),np.linspace(78,170,H));h,w=patch.shape[:2]
def mirror(a,n):
 a=np.mod(a,2*n);return np.where(a<n,a,2*n-1-a).astype(int)
px=mirror((X+96)/20*w,w);py=mirror((Y-78)/17*h,h);pixels=patch[py,px].copy()
# Broad brush glazing follows the open eastern meadow, very low contrast, not detached painted islands.
glaze=np.clip((X+15)/70,0,1)*np.clip((Y-86)/50,0,1)*.06;pixels[:,:,:3]=pixels[:,:,:3]*(1-glaze[:,:,None])+np.array([.64,.61,.30])*glaze[:,:,None]
forest=np.zeros((H,W),np.float32)
for x,y in points:
 d=((X-x)/5.5)**2+((Y-y)/5.5)**2;forest=np.maximum(forest,np.clip(1-d,0,1)*.26)
pixels[:,:,:3]*=(1-forest[:,:,None]*np.array([.28,.18,.22]))
path=np.zeros((H,W),np.float32)
for s in samples:
 for p in s:
  xx=(p.x+96)/192*(W-1);yy=(p.y-78)/92*(H-1);rx=1.65/192*W;ry=1.65/92*H;a=max(0,int(xx-rx-1));b=min(W,int(xx+rx+2));c=max(0,int(yy-ry-1));d=min(H,int(yy+ry+2))
  if a>=b or c>=d:continue
  iy,ix=np.mgrid[c:d,a:b];dist=np.sqrt(((ix-xx)/rx)**2+((iy-yy)/ry)**2);path[c:d,a:b]=np.maximum(path[c:d,a:b],np.clip((1-dist)*2.1,0,1))
colour=original[-1,int((15+96)/192*(S-1)),:3];pixels[:,:,:3]=pixels[:,:,:3]*(1-path[:,:,None])+colour*path[:,:,None]
# Mirror a 2D strip across the actual boundary: unlike P, never stretch one row into a vertical band.
ix=np.clip(((X+96)/192*(S-1)).astype(int),0,S-1);iy=np.clip(((78-(Y-78)+78)/156*(S-1)).astype(int),0,S-1);seam=np.clip((Y-78)/7,0,1);seam=seam*seam*(3-2*seam);pixels[:,:,:3]=original[iy,ix,:3]*(1-seam[:,:,None])+pixels[:,:,:3]*seam[:,:,None];pixels[:,:,3]=1
im=bpy.data.images.new('Q continuous sector meadow pigment',width=W,height=H);im.colorspace_settings.name='sRGB';im.pixels.foreach_set(pixels.ravel())
mat=bpy.data.materials['P connected meadow forest floor paths'];bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
for name,mask in [('Q editable forest mask',forest),('Q editable path mask',path)]:
 a=np.ones((H,W,4),np.float32);a[:,:,:3]=mask[:,:,None];m=bpy.data.images.new(name,width=W,height=H);m.colorspace_settings.name='Non-Color';m.pixels.foreach_set(a.ravel())
scene['finishing_stage']='Q continuous grass clone paint and glade-directed forest on P sector';scene['Q_source_sha256']=hashlib.sha256(src.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.size[0]>0 and im.source in {'GENERATED','FILE'}:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.render.filepath=str(out/'blender-sector.png');bpy.ops.render.render(write_still=True)
(out/'sector-authoring.json').write_text(json.dumps({'stage':'Q','source_sha256':scene['Q_source_sha256'],'terrain_geometry':'P unchanged, O vertices preserved','new_forest_trees':len(points),'planting_points':points,'correction':'2D clone transition replaces stretched 1D row; old river/path silhouettes excluded from pigment; Poisson forest inside retained boundary with glade','art_approved':False},indent=2))
