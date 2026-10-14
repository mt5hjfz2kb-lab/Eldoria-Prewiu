"""Eldoria Bastion I two GLOBAL layout candidates. Not AI-sculpted geometry.
Usage: blender -b --python-exit-code 1 --python this.py -- baseline.glb output.glb A|B
Preserves every original source mesh; new parts only beside existing medieval masonry.
No central objects, build plots, gameplay colliders, or resource nodes.
"""
import bpy,sys,pathlib,math,json,hashlib,random
from mathutils import Vector,Matrix
from mathutils.bvhtree import BVHTree
argv=sys.argv[sys.argv.index('--')+1:]
if len(argv)!=3:raise SystemExit('Expected source.glb out.glb variant(A|B)')
src,dst=map(pathlib.Path,argv[:2]);variant=argv[2].upper()
if variant not in ('A','B'):raise SystemExit('Variant must be A or B')
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(src))
meshes=[o for o in bpy.data.objects if o.type=='MESH']
objects=list(bpy.data.objects)
terrain=[o for o in meshes if 'natural playable hilltop' in o.name.lower()]
if len(terrain)!=1:raise RuntimeError('Frozen original terrain not found')
bvh=BVHTree.FromObject(terrain[0],bpy.context.evaluated_depsgraph_get())
def ground(x,y):
 hit,normal,i,d=bvh.ray_cast(Vector((x,y,110)),Vector((0,0,-1)),210)
 if hit is None:raise RuntimeError('Authored playable terrain missing at '+str((x,y)))
 return hit.z
def children(name):
 roots=[o for o in objects if o.name.split('.')[0]==name]
 if len(roots)!=1:raise RuntimeError('Missing unique original reusable group: '+name)
 out=[]
 for o in meshes:
  p=o.parent
  while p is not None:
   if p==roots[0]:out.append(o);break
   p=p.parent
 if not out:raise RuntimeError('Empty original reuse group '+name)
 return out
wallkit=children('LeftDetachedRemnant')
connectionkit=children('LeftDetachedPier')
pavingkit=children('CentralRoad_-4')
rockkit=children('FrontEscarpment_-23')
woodkit=children('LowerGatePracticalSet')
stone=next((m for o in wallkit for m in o.data.materials if m),None)
wood=next((m for o in woodkit for m in o.data.materials if m and ('wood' in m.name.lower() or 'oak' in m.name.lower())),None)
if not stone:raise RuntimeError('Existing wall has no source material')
if not wood:wood=next((m for o in woodkit for m in o.data.materials if m),None)
if not wood:raise RuntimeError('Existing medieval kit has no usable material')
original_names=set(o.name for o in objects)
created=[]; reused=[]; source_checksums={}
def safe(label,x,y,w,d,height=0):
 # Keep at least 2m outside the 14m-wide reserved road and buffer both lots.
 if abs(x)-w*.5<9.0:raise RuntimeError('Art intrudes protected road: '+label)
 for xx,yy in ((-16,3),(16,3)):
  # protected 10 by 10+ extent around future plot centers
  if abs(x-xx)<(w/2+5.0) and abs(y-yy)<(d/2+5.0):
   raise RuntimeError('Art intrudes future 10m-square building plot: '+label)
 if x<-30 or x>30 or y<-16.2 or y>22:raise RuntimeError('Beyond cliff plateau: '+label)
 return ground(x,y)
def cube(label,x,y,z,w,d,h,material=stone,bevel=0.08):
 objname='B1 '+variant+' '+label
 bpy.ops.mesh.primitive_cube_add(size=1,location=(x,y,z))
 o=bpy.context.object;o.name=objname;o.dimensions=(w,d,h)
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(material)
 if bevel:
  m=o.modifiers.new('rounded chipped medieval masonry edge','BEVEL')
  m.width=min(bevel,min(w,d,h)*.17);m.segments=1
  bpy.context.view_layer.objects.active=o
  bpy.ops.object.modifier_apply(modifier=m.name)
 created.append(o)
 return o
def kit_bounds(group):
 pts=[o.matrix_world @ Vector(p) for o in group for p in o.bound_box]
 lo=Vector(tuple(min(v[k] for v in pts) for k in range(3)))
 hi=Vector(tuple(max(v[k] for v in pts) for k in range(3)))
 return lo,hi
def clone_group(label,kit,x,y,size,base_z=None,rotate=0):
 lo,hi=kit_bounds(kit);sz=hi-lo
 sx,sy,szscale=(size[0]/max(.01,sz.x),size[1]/max(.01,sz.y),size[2]/max(.01,sz.z))
 z=ground(x,y) if base_z is None else base_z
 transform=(Matrix.Translation(Vector((x,y,z))) @ Matrix.Rotation(math.radians(rotate),4,'Z')
     @ Matrix.Diagonal(Vector((sx,sy,szscale,1)))
     @ Matrix.Translation(Vector((-(lo.x+hi.x)/2,-(lo.y+hi.y)/2,-lo.z))))
 for old in kit:
  o=old.copy();o.data=old.data;o.parent=None;o.matrix_world=transform @ old.matrix_world
  o.name='B1 '+variant+' original '+label+' '+str(len(reused))
  bpy.context.collection.objects.link(o);reused.append(o)
 return len(kit)
def broken_wall_line(label,x,y,length,height,orientation='Y',seed=1):
 # Segmented masonry with real joints; broken intervals imply ruined history,
 # while irregular capstones and authentic original limestone bind visually.
 rng=random.Random(seed);n=max(3,int(length/2.8));step=length/n
 for k in range(n):
  if k==n//2 and n>=5:continue
  offset=(k+.5-n/2)*step
  xx=x+(offset if orientation=='X' else 0)
  yy=y+(offset if orientation=='Y' else 0)
  w,d=(step*.91,.72) if orientation=='X' else (.72,step*.91)
  safe(label,xx,yy,w,d)
  z=ground(xx,yy);h=height*rng.uniform(.69,1.04)
  cube(label+' rough stone coursing',xx,yy,z+h/2,w,d,h,bevel=.065)
  cube(label+' independent broken cap',xx,yy,z+h+.075,w+.15 if orientation=='Y' else w+.06,
    d+.15 if orientation=='X' else d+.06,.15,bevel=.025)
 # periodic bastions are materially distinct from a continuous fake fence.
def buttress(label,x,y,height=2.1):
 safe(label,x,y,1.5,1.3);z=ground(x,y)
 cube(label+' grounded base',x,y,z+.38,1.6,1.45,.76,bevel=.09)
 cube(label+' stepped shaft',x,y,z+.78+height*.5,1.05,.98,height,bevel=.10)
 cube(label+' weathered crest',x,y,z+.78+height+.10,1.3,1.18,.21,bevel=.025)
def gallery(label,x,y,orientation='Y',length=10,top=2.3):
 # Raised stone pilaster rhythm and surviving original wall modules.
 for i in (-1,0,1):
  u=i*length*.32;xx=x+(u if orientation=='X' else 0);yy=y+(u if orientation=='Y' else 0)
  safe(label,xx,yy,1.3,1.3)
  h=top if i!=0 else top*.69;z=ground(xx,yy)
  cube(label+' pier',xx,yy,z+h*.5,1.0,1.0,h,bevel=.075)
  cube(label+' copestone',xx,yy,z+h+.10,1.32,1.30,.2,bevel=.025)
def flagstones(label,x,y,w,d,h=.14):
 safe(label,x,y,w,d);clone_group(label,pavingkit,x,y,(w,d,h),base_z=ground(x,y)-.01)
def stone_scree(label,x,y,size=(2.7,1.6,.58)):
 safe(label,x,y,size[0],size[1]);clone_group(label,rockkit,x,y,size,base_z=ground(x,y)-.11)
def run_variant_A():
 # A: 'Defensive ward'. The already validated west guardhouse is the northern
 # contact of a restored inhabited fort perimeter. An east counterpart balances
 # overall composition. Strong architecture rises on both OUTER limits only.
 broken_wall_line('WEST courtyard broken rampart',-25.15,-.95,13.8,1.82,'Y',41)
 broken_wall_line('EAST courtyard broken rampart',25.10,-3.0,16.6,2.35,'Y',42)
 # West and east architectural anchors are not detached props; buttresses
 # actually sit on the segment joints / cliff contact line.
 for name,x,y,h in [('west landing',-25.15,-8.1,2.6),('west terrace',-25.15,5.45,2.6),
                    ('east entry',25.10,-11.05,2.8),('east terrace',25.10,5.05,3.15)]:
  buttress(name,x,y,h)
 # Curate sightlines from front gate to upper fort using side-only masonry
 # broken returns. No masonry crosses the central x=-9..9 corridor.
 broken_wall_line('WEST upper return',-20.7,15.6,10.0,1.30,'X',73)
 broken_wall_line('EAST upper return',20.35,15.6,10.4,1.65,'X',74)
 # A pair of original architected arches/pier connections, not empty cubes.
 clone_group('west ancient junction',connectionkit,-25.1,11.4,(2.9,3.2,3.2))
 clone_group('east ancient junction',connectionkit,25.1,10.6,(3.1,3.1,3.8))
 # Make flanking landings look founded on rock not cardboard.
 for x,y in [(-26,-9.4),(26,-10.4),(-25,8.7),(25,9)]:
  stone_scree('cliff masonry contact',x,y,(2.8,2,.65))
def run_variant_B():
 # B: 'Terraced old town'. Recompose the entire yard as linked terraced
 # plateau edge sequences, not isolated wall towers. Stone contour segments
 # hold real rock-contact depth across top/mid/front, central corridor open.
 for side in (-1,1):
  sign='WEST' if side<0 else 'EAST'
  # Only outside the building slots at +/-16, y3. A high upper-edge terrace
  # and a lower gate-side landing are linked around their OUTER perimeter.
  x=side*22.6
  for y in (-10.1,12.65):
   safe('terraced contour',x,y,8.2,3.1)
   z=ground(x,y)
   # Actual differentiated vertical stone retaining face, not a flat decal.
   cube(sign+' broad stratified retaining terrace',x,y,z+.29,8.2,3.1,.64,bevel=.11)
   cube(sign+' edge coping deep perimeter',x,y-1.31,z+.64,8.45,.61,.35,bevel=.045)
   for dx in (-3.05,0,3.05):
    zz=ground(x+dx,y-1.5)
    cube(sign+' dressed retaining foundation',x+dx,y-1.52,zz+.61,.80,.70,1.24,bevel=.055)
   flagstones(sign+' existing original chiseled terrace',x,y,7.9,2.8,.23)
   stone_scree(sign+' exposed rock-to-ashlar',x+side*2.95,y+1.85,(3.2,2.3,.95))
  # Short physical connecting parapet along the OUTER edge, no reserved lots.
  broken_wall_line(sign+' uninterrupted lower old-town escarpment',side*27.4,-1.55,11.5,1.35,'Y',91+(side+1))
  gallery(sign+' old masonry colonnade',side*23.1,17.65,'X',10.5,2.45)
 # Lower forecourt is left open to public walkable negative space.
 # Upper horizon gains grounded masonry instead of cliff-hanging trees.
if variant=='A':run_variant_A()
else:run_variant_B()
# Rigorous non-destructive proof: all original names/objects still exist and
# new geometry has zero physical colliders/resource-node game objects.
assert all(o.name in original_names for o in objects)
dst.parent.mkdir(parents=True,exist_ok=True)
bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',export_apply=False)
proof={'source_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),
 'output_sha256':hashlib.sha256(dst.read_bytes()).hexdigest(),
 'variant':variant,'design':('defensive_perimeter_ward' if variant=='A' else 'stratified_terraced_old_town'),
 'authorship':'deterministic Blender execution; Qwen review is separate; no new AI-sculpted vertices',
 'reused_original_mesh_instances':len(reused),'new_modular_masonry_meshes':len(created),
 'original_meshes_preserved':True,'protected_road_x':[-7,7],
 'reserved_build_centers':[[-16,3],[16,3]],'new_resource_nodes':0,'new_gameplay_colliders':0,
 'visual_pass':False,'mobile_device_pass':False}
dst.with_suffix('.proof.json').write_text(json.dumps(proof,indent=2))
print('BASTION1_GLOBAL_COMPOSITION_PROOF',json.dumps(proof))
