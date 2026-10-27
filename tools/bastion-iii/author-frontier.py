"""Cost-zero authored candidate. Never an artistic approval. Blender 5.x, no downloads.
Ash wolf silhouette: deep chest, tucked belly, digitigrade hocks, tapered muzzle,
separate ear planes and swept tail. Fissure: a low geological split, no portal.
Exports an editable source and native glTF; evaluate same-camera Unity evidence.
"""
import bpy, math, os
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Unity/Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/BastionIII'
EV=ROOT/'docs/bastion-iii/native'
OUT.mkdir(parents=True,exist_ok=True);EV.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def mat(name,color,rough=.85):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=rough
 return m
ash=mat('Ash grey fur',(.22,.26,.28));silver=mat('Guard hair silver',(.40,.44,.43));ivory=mat('Warm pale muzzle',(.58,.57,.49));dark=mat('Charcoal paws and nose',(.055,.065,.07));eye=mat('Amber eyes',(.64,.35,.06),.38)
def mesh(name,verts,faces,material):
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update();o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(material);return o
def sweep(name,points,radii,material,sides=10):
 # Each ring lies normal to the centerline, giving real connected topology.
 verts=[]
 for i,(p,r) in enumerate(zip(points,radii)):
  p=Vector(p);t=Vector(points[min(i+1,len(points)-1)])-Vector(points[max(0,i-1)])
  t.normalize();u=t.cross(Vector((0,1,0)))
  if u.length<.01:u=t.cross(Vector((0,0,1)))
  u.normalize();v=t.cross(u).normalized()
  for k in range(sides):
   a=k*2*math.pi/sides;verts.append(p+u*math.cos(a)*r[0]+v*math.sin(a)*r[1])
 faces=[]
 for i in range(len(points)-1):
  for k in range(sides):faces.append((i*sides+k,i*sides+(k+1)%sides,(i+1)*sides+(k+1)%sides,(i+1)*sides+k))
 faces.extend([tuple(reversed(range(sides))),tuple((len(points)-1)*sides+k for k in range(sides))]);return mesh(name,verts,faces,material)
# x points towards the muzzle; z is Blender up.
sweep('Wolf torso', [(-.95,0,1.13),(-.60,0,1.34),(-.10,0,1.42),(.47,0,1.49),(.70,0,1.55)],[(.20,.18),(.42,.32),(.37,.30),(.55,.38),(.30,.25)],ash,12)
sweep('Raised neck and cheek',[(.42,0,1.43),(.73,0,1.75),(.92,0,2.02),(1.20,0,2.04)],[(.42,.35),(.40,.32),(.31,.29),(.23,.24)],silver,12)
sweep('Long tapered muzzle',[(1.13,0,1.98),(1.40,0,1.93),(1.65,0,1.94)],[(.21,.21),(.14,.16),(.09,.12)],ivory,10)
sweep('Nose',[(1.63,0,1.94),(1.72,0,1.94)],[(.095,.12),(.08,.10)],dark,8)
for side in [-1,1]:
 y=side*.27
 sweep('Front leg '+str(side),[(.47,y,1.43),(.49,y,.92),(.40,y,.37),(.54,y,.16),(.70,y,.13)],[(.20,.17),(.12,.11),(.075,.075),(.09,.09),(.06,.10)],ash,8)
 sweep('Rear hock '+str(side),[(-.66,y,1.30),(-.44,y,.88),(-.80,y,.52),(-.76,y,.19),(-.55,y,.13)],[(.27,.21),(.15,.12),(.09,.08),(.07,.07),(.075,.10)],ash,8)
 # Strong asymmetric triangular ears, closed volumes rather than flat billboards.
 yy=side*.21
 mesh('Alert ear '+str(side),[(.83,yy-.10,2.17),(1.15,yy-.08,2.16),(.91,yy,2.57),(.85,yy+.10,2.16),(1.13,yy+.08,2.17)],[(0,1,2),(1,4,2),(4,3,2),(3,0,2),(0,3,4,1)],ash)
 mesh('Ear inner '+str(side),[(.88,yy-.105,2.21),(1.07,yy-.085,2.21),(.92,yy-.025,2.48)],[(0,1,2)],ivory)
 sweep('Eye '+str(side),[(1.19,side*.222,2.095),(1.205,side*.247,2.095)],[(.041,.04),(.032,.032)],eye,8)
 # Shoulder fur overlaps into the torso in jagged planes, preserving the canine outline.
 for k in range(4):
  x=.55-k*.15
  mesh('Shoulder guard hairs',[(x,y*1.25,1.79-k*.065),(x+.18,y*1.4,1.67-k*.07),(x-.19,y*1.48,1.45-k*.07)],[(0,1,2)],silver)
sweep('Swept bushy tail',[(-.82,0,1.37),(-1.22,0,1.13),(-1.57,.05,.76),(-1.76,.12,.62)],[(.20,.19),(.25,.23),(.19,.17),(.025,.035)],ash,10)
def export(name):
 bpy.ops.object.select_all(action='DESELECT')
 objs=[o for o in bpy.context.scene.objects if o.type=='MESH']
 for o in objs:o.select_set(True)
 bpy.ops.export_scene.gltf(filepath=str(OUT/(name+'.glb')),export_format='GLB',use_selection=True)
 bpy.ops.wm.save_as_mainfile(filepath=str(EV/(name+'.blend')))
export('AshWolf')
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
rock=mat('Weathered fissure stone',(.18,.16,.19));black=mat('Deep fissure',(.022,.012,.032));violet=mat('Residual ether',(.25,.045,.34))
# Irregular rim strata; silhouette remains ground-bound and elongated.
centres=[(-3.6,-.4),(-2.7,.12),(-1.6,-.18),(-.5,.35),(.65,-.23),(1.7,.12),(2.65,-.3),(3.5,.12)]
for side in [-1,1]:
 verts=[]
 for i,(x,y) in enumerate(centres):
  gap=.14+(.19 if i in [2,3,4,5] else .06)
  for offset,z in [(gap,-.09),(gap+.22,.16+(i%3)*.055),(gap+.68,.02)]:verts.append((x,y+side*offset,z))
 faces=[]
 for i in range(len(centres)-1):
  for k in range(2):faces.append((i*3+k,i*3+k+1,(i+1)*3+k+1,(i+1)*3+k))
 mesh('Fractured low rim '+str(side),verts,faces,rock)
verts=[]
for i,(x,y) in enumerate(centres):
 gap=.14+(.19 if i in [2,3,4,5] else .06)
 verts.extend([(x,y-gap,.005),(x,y+gap,.005)])
mesh('Dark split floor',verts,[(i*2,i*2+1,i*2+3,i*2+2) for i in range(len(centres)-1)],black)
mesh('Fine ether seam',[(x,y+dy,.012) for x,y in centres for dy in [-.018,.018]],[(i*2,i*2+1,i*2+3,i*2+2) for i in range(len(centres)-1)],violet)
export('PrivateFissure')
print('BASTION_III_AUTHORED_CANDIDATES_EXPORTED')
