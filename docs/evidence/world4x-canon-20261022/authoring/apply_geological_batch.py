"""Exact artist operation reviewed through BlenderMCP, applied to current production.
No scene reset, no new terrain, no gameplay targets, no change to source AF/AG.
"""
import bpy,json,pathlib,math
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree
exec(compile(pathlib.Path(__file__).with_name('interactive').joinpath('macro_fracture_recipe.py').read_text(encoding='utf-8'),'macro_fracture_recipe.py','exec'),globals())
def apply_geological_batch(output):
 out=pathlib.Path(output)
 terrain=bpy.data.objects['BLD_Terrain'];mesh=terrain.data
 positions=np.array([tuple(v.co) for v in mesh.vertices]);x,y=positions[:,0],positions[:,1]
 protected=(x>=-96)&(x<=96)&(y>=-78)&(y<=270)
 before=positions[protected].copy()
 old=12*np.clip(1-stroke_distance(x,y,ORIGINAL_FAULTS)/24,0,1)**2
 new=12*np.clip(1-stroke_distance(x,y,AUTHOR_FAULTS)/24,0,1)**2
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
 floor=bpy.data.objects['BLD_BreachFloor'];floor_materials=list(floor.data.materials)
 for name,paths in [('BLD_BreachFloor',ORIGINAL_FAULTS),('BLD_BreachOutlyingFractures',AUTHOR_FAULTS[3:])]:
  vv=[];ff=[]
  for branch,path in enumerate(paths):
   points=[]
   for aa,bb in zip(path,path[1:]):
    for t in np.linspace(0,1,max(2,int(math.dist(aa,bb)/5)),endpoint=False):points.append((aa[0]+(bb[0]-aa[0])*t,aa[1]+(bb[1]-aa[1])*t))
   points.append(path[-1])
   for i,(cx,cy) in enumerate(points):
    prev=points[max(0,i-1)];nxt=points[min(len(points)-1,i+1)];a=math.atan2(nxt[1]-prev[1],nxt[0]-prev[0]);t=i/(len(points)-1)
    w=(.5+3.2*math.sin(math.pi*t)**.7)*(1 if branch==0 else .6)
    for sign in (-1,1):
     xx=cx+sign*w*math.sin(a);yy=cy-sign*w*math.cos(a);vv.append((xx,yy,surface(xx,yy)+.025))
    if i:ff.append((len(vv)-4,len(vv)-3,len(vv)-1,len(vv)-2))
  newmesh=bpy.data.meshes.new(name+' authored mineral incision');newmesh.from_pydata(vv,[],ff)
  for mat in floor_materials:newmesh.materials.append(mat)
  obj=bpy.data.objects.get(name)
  if obj:obj.data=newmesh
  else:
   obj=bpy.data.objects.new(name,newmesh);bpy.context.scene.collection.objects.link(obj)
 image=bpy.data.images['Region1 authored meadow and woodland pigment'];W,H=image.size
 pixels=np.empty(W*H*4,np.float32);image.pixels.foreach_get(pixels);pixels=pixels.reshape(H,W,4)
 # Work only inside the new geological footprint, never the source AF/AG images.
 x0,x1=int((-675+910)/1820*(W-1)),int((-100+910)/1820*(W-1))+1
 y0,y1=int((-5+1800)/3600*(H-1)),int((620+1800)/3600*(H-1))+1
 X,Y=np.meshgrid(np.linspace(-910,910,W)[x0:x1],np.linspace(-1800,1800,H)[y0:y1])
 mask=geological_paint(pixels[y0:y1,x0:x1,:3],X,Y)
 image.pixels.foreach_set(pixels.ravel());image.filepath_raw=str(out/'MacroGeologicalPigment.png');image.file_format='PNG';image.save()
 # MCP reopen diagnosis40097057090 proved repacking an already packed image
 # kept stale bytes. Load the saved PNG as a fresh image and relink only this
 # continental pigment; preserve every original AF/AG image and material.
 fresh=bpy.data.images.load(str(out/'MacroGeologicalPigment.png'),check_existing=False)
 fresh.name='Region1 authored geological pigment';fresh.pack()
 import hashlib
 assert hashlib.sha256(fresh.packed_file.data).hexdigest()==hashlib.sha256((out/'MacroGeologicalPigment.png').read_bytes()).hexdigest()
 for material in bpy.data.materials:
  if material.use_nodes:
   for node in material.node_tree.nodes:
    if node.type=='TEX_IMAGE' and node.image==image:node.image=fresh
 # No relocation or replacement of entities: only experimental forest exclusion.
 cleared=[]
 for tree in bpy.context.scene.objects:
  if not tree.name.startswith('BLD_ForestMass_'):continue
  tx,ty=tree.location.x,tree.location.y
  if float(stroke_distance(np.array(tx),np.array(ty),AUTHOR_FAULTS))<46:
   tree.hide_render=True;far=bpy.data.objects.get(tree.name.replace('BLD_ForestMass_','BLD_ForestFar_'))
   if far:far.hide_render=True
   cleared.append(tree.name)
 report={'source_baseline':'Current production source; original AF/AG bytes retained','changed_vertices':int(np.count_nonzero(np.abs(change)>1e-6)),'protected_vertices':int(protected.sum()),'protected_positions_unchanged':True,'new_faults':AUTHOR_FAULTS,'cleared_crowns':cleared,'geological_extent_m':[-660,0,-100,610],'art_approved':False,'unity_integrated':False,'core_floor_bounds':[list(floor.matrix_world @ Vector(p)) for p in floor.bound_box],'decision':'Broader connected ground fracture and woodland clearance as one focal-composition batch; no extra props or new game entities.'}
 (out/'macro-geological-operations.json').write_text(json.dumps(report,indent=2))
 return report
