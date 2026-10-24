# Decision after 00-continental-observe and real run400874 comparison.
# Broaden the geological focal structure; clear only intersecting new forest crowns.
import bpy,json,pathlib,hashlib,math
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
"""Authored geological hierarchy after inspecting real Unity and MCP baseline.
Coordinates are deliberate branching strokes, not a noise-generated terrain.
The original AF/AG rectangle and its topology/materials/UV remain protected.
"""
import numpy as np
ORIGINAL_FAULTS=[[(-385,260),(-345,218),(-308,206),(-280,165),(-260,150),(-222,137),(-195,86),(-135,40)],[(-308,206),(-350,140),(-365,110)],[(-222,137),(-175,187),(-150,230)]]
AUTHOR_FAULTS=ORIGINAL_FAULTS+[
 [(-385,260),(-430,292),(-470,280),(-510,322),(-560,335),(-590,382)],
 [(-308,206),(-292,285),(-320,352),(-285,410),(-300,470),(-275,540)],
 [(-150,230),(-125,280),(-155,340),(-125,385)],
 [(-260,150),(-335,102),(-400,125),(-460,85),(-530,120)],
]
def stroke_distance(x,y,paths):
 d=np.full(np.shape(x),1e6,dtype=np.float64)
 for path in paths:
  for (ax,ay),(bx,by) in zip(path,path[1:]):
   t=np.clip(((x-ax)*(bx-ax)+(y-ay)*(by-ay))/((bx-ax)**2+(by-ay)**2),0,1)
   d=np.minimum(d,np.hypot(x-ax-t*(bx-ax),y-ay-t*(by-ay)))
 return d

def geological_paint(rgb,x,y):
 """Bounded material relationship: pale broken shoulders, dark mineral floor."""
 d=stroke_distance(x,y,AUTHOR_FAULTS)
 core_clear=np.clip((-102-x)/23,0,1)
 apron=np.clip((70-d)/44,0,1);apron=apron*apron*(3-2*apron)*.64*core_clear
 # Broad directional strata, chosen to follow the west/north scar fan.
 # Existing fine grain and faceted rock pigment remain visible underneath.
 shoulder=np.empty_like(rgb);shoulder[:]=(.49,.43,.43)
 rgb[:]=rgb*(1-apron[...,None])+shoulder*apron[...,None]
 floor=np.clip((27-d)/21,0,1)*.70*core_clear
 rgb[:]=rgb*(1-floor[...,None])+np.array((.245,.17,.29))*floor[...,None]
 heart=np.clip(1-((x+260)/92)**2-((y-150)/100)**2,0,1)*.58*core_clear
 rgb[:]=rgb*(1-heart[...,None])+np.array((.30,.12,.37))*heart[...,None]
 return apron

terrain=bpy.data.objects['BLD_Terrain'];mesh=terrain.data
positions=np.array([tuple(v.co) for v in mesh.vertices]);x,y=positions[:,0],positions[:,1]
protected=(x>=-96)&(x<=96)&(y>=-78)&(y<=270)
before=positions[protected].copy()
old=12*np.clip(1-stroke_distance(x,y,ORIGINAL_FAULTS)/24,0,1)**2
new=17*np.clip(1-stroke_distance(x,y,AUTHOR_FAULTS)/29,0,1)**2
change=old-new;change[protected]=0
for vertex,delta in zip(mesh.vertices,change):vertex.co.z+=float(delta)
mesh.update();bpy.context.view_layer.update()
assert np.array_equal(before,np.array([tuple(v.co) for v in mesh.vertices])[protected])
bv=BVHTree.FromObject(terrain,bpy.context.evaluated_depsgraph_get())
def surface(x,y):
 p,_,_,_=bv.ray_cast(Vector((x,y,200)),Vector((0,0,-1)))
 assert p is not None,(x,y)
 return p.z
# Re-author the existing floor mesh in place from connected hand-drawn strokes.
vv=[];ff=[]
for branch,path in enumerate(AUTHOR_FAULTS):
 points=[]
 for aa,bb in zip(path,path[1:]):
  for t in np.linspace(0,1,max(2,int(math.dist(aa,bb)/5)),endpoint=False):points.append((aa[0]+(bb[0]-aa[0])*t,aa[1]+(bb[1]-aa[1])*t))
 points.append(path[-1])
 for i,(cx,cy) in enumerate(points):
  prev=points[max(0,i-1)];nxt=points[min(len(points)-1,i+1)];a=math.atan2(nxt[1]-prev[1],nxt[0]-prev[0]);t=i/(len(points)-1)
  w=(.6+4.0*math.sin(math.pi*t)**.7)*(1 if branch==0 else .7)
  for sign in (-1,1):
   xx=cx+sign*w*math.sin(a);yy=cy-sign*w*math.cos(a);vv.append((xx,yy,surface(xx,yy)+.025))
  if i:ff.append((len(vv)-4,len(vv)-3,len(vv)-1,len(vv)-2))
floor=bpy.data.objects['BLD_BreachFloor'];newmesh=bpy.data.meshes.new('Authored continental fracture fan');newmesh.from_pydata(vv,[],ff)
for mat in floor.data.materials:newmesh.materials.append(mat)
floor.data=newmesh
image=bpy.data.images['Region1 authored meadow and woodland pigment'];W,H=image.size
pixels=np.empty(W*H*4,np.float32);image.pixels.foreach_get(pixels);pixels=pixels.reshape(H,W,4)
# Work only inside the new geological footprint, never the source AF/AG images.
x0,x1=int((-675+910)/1820*(W-1)),int((-100+910)/1820*(W-1))+1
y0,y1=int((-5+1800)/3600*(H-1)),int((620+1800)/3600*(H-1))+1
X,Y=np.meshgrid(np.linspace(-910,910,W)[x0:x1],np.linspace(-1800,1800,H)[y0:y1])
mask=geological_paint(pixels[y0:y1,x0:x1,:3],X,Y)
image.pixels.foreach_set(pixels.ravel());image.filepath_raw=str(out/'01-geological-pigment.png');image.file_format='PNG';image.save();image.pack()
# No relocation or replacement of entities: only experimental forest exclusion.
cleared=[]
for tree in bpy.context.scene.objects:
 if not tree.name.startswith('BLD_ForestMass_'):continue
 tx,ty=tree.location.x,tree.location.y
 if float(stroke_distance(np.array(tx),np.array(ty),AUTHOR_FAULTS))<46:
  tree.hide_render=True;far=bpy.data.objects.get(tree.name.replace('BLD_ForestMass_','BLD_ForestFar_'))
  if far:far.hide_render=True
  cleared.append(tree.name)
report={'source_baseline':'00-continental-observe.blend','changed_vertices':int(np.count_nonzero(np.abs(change)>1e-6)),'protected_vertices':int(protected.sum()),'protected_positions_unchanged':True,'new_faults':AUTHOR_FAULTS,'cleared_crowns':cleared,'geological_extent_m':[-660,0,-100,610],'art_approved':False,'unity_integrated':False,'decision':'Broader connected ground fracture and woodland clearance as one focal-composition batch; no extra props or new game entities.'}
(out/'01-geological-operations.json').write_text(json.dumps(report,indent=2))
bpy.app.driver_namespace['interactive_snapshot']('01-geological-hierarchy')
print(json.dumps({k:v for k,v in report.items() if k!='cleared_crowns'}))
