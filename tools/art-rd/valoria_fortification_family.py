"""Valoria modular fortification library: export six independent GLBs and Blender renders."""
import bpy,math,random,json,os
from pathlib import Path
from mathutils import Vector
random.seed(6)
root=Path(__file__).resolve().parents[2]
out=Path(os.environ.get('ELDORIA_FORTIFICATION_OUTPUT',str(root/'artifacts-local/fortifications')))
out.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
def material(name,rgb):
 m=bpy.data.materials.new(name);m.diffuse_color=(*rgb,1);return m
stones=[material('ashlar '+str(i),v) for i,v in enumerate([(.40,.41,.39),(.49,.48,.43),(.32,.34,.35),(.43,.42,.36)])]
oak=material('oak',(.24,.12,.075))
def block(name,at,dimensions,mat,bevel=.025):
 bpy.ops.mesh.primitive_cube_add(size=1,location=at)
 o=bpy.context.object;o.name=name;o.dimensions=dimensions
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 o.data.materials.append(mat)
 b=o.modifiers.new('subtle worn edges','BEVEL');b.width=bevel;b.segments=2
 o.modifiers.new('weighted normals','WEIGHTED_NORMAL')
 return o
def courses(name,x0,x1,y0,y1,z0,z1):
 z=z0;course=0
 while z<z1-.02:
  h=min(.46,z1-z);x=x0-(course%2)*.51
  while x<x1-.05:
   a=max(x,x0);b=min(x+1.04,x1)
   if b-a>.13:block(name+'_ashlar',((a+b)/2,(y0+y1)/2,z+h/2),(b-a-.035,y1-y0-.035,h-.025),random.choice(stones))
   x+=1.04
  z+=h;course+=1
def parapet(name,left,right,y,z):
 for i in range(int(right-left)):
  if i%2==0:block(name+'_merlon',(left+i+.5,y,z+.43),(.83,.86,.86),stones[i%4])
def wall(name,x=0,y=0,length=8):
 courses(name,x-length/2,x+length/2,y-.65,y+.65,0,4.4)
 block(name+'_coping',(x,y,4.5),(length+0.16,1.53,.18),stones[1])
 parapet(name,x-length/2,x+length/2,y,4.61)
def tower(name,x=0,y=0):
 r=2.2
 for row in range(15):
  for k in range(12):
   theta=(k+.5)*math.tau/12+(row%2)*math.tau/24
   o=block(name+'_curved_ashlar',(x+r*math.cos(theta),y+r*math.sin(theta),row*.48+.24),(.95,.76,.45),stones[(k+row)%4])
   o.rotation_euler[2]=theta+math.pi/2
 for k in range(12):
  if k%2==0:
   a=k*math.tau/12
   o=block(name+'_tower_merlon',(x+2.28*math.cos(a),y+2.28*math.sin(a),7.65),(.95,.78,.88),stones[k%4])
   o.rotation_euler[2]=a+math.pi/2
def gate():
 courses('gate_left',-4.5,-1.7,-.8,.8,0,5.8)
 courses('gate_right',1.7,4.5,-.8,.8,0,5.8)
 for k in range(12):
  a=math.pi*(k+.5)/12
  ob=block('arch_voussoir',(1.88*math.cos(a),0,3.15+1.88*math.sin(a)),(.51,1.77,.88),stones[k%4])
  ob.rotation_euler[1]=-a
 courses('gate_crown',-4.5,4.5,-.8,.8,5.15,6.35)
 parapet('gate',-4.5,4.5,0,6.4)
 for side in [-1,1]:block('oak_gate_leaf',(side*1.83,0,1.35),(.16,.23,2.7),oak)
def build(kind):
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 if kind=='wall':wall('wall')
 if kind=='corner_inner' or kind=='corner_outer':
  wall('first_wall')
  existing={o.name for o in bpy.data.objects}
  wall('second_wall')
  for o in bpy.data.objects:
   if o.name not in existing and o.type=='MESH':
    o.rotation_euler[2]=math.pi/2
    ox,oy=o.location.x,o.location.y
    o.location.x,o.location.y=(-3 if kind=='corner_inner' else 3)-oy,3+ox
 if kind=='tower':tower('round_tower')
 if kind=='gate':gate()
 if kind=='parapet':wall('reinforced_wall',length=5)
def scene(target,scale):
 s=bpy.context.scene;s.render.engine='CYCLES';s.cycles.samples=16;s.cycles.use_denoising=False;s.view_layers[0].cycles.use_denoising=False
 s.render.resolution_x=960;s.render.resolution_y=640;s.render.resolution_percentage=100
 world=bpy.data.worlds.new('overcast sky');s.world=world;world.use_nodes=True
 world.node_tree.nodes['Background'].inputs['Color'].default_value=(.38,.40,.43,1)
 light=bpy.data.lights.new('soft daylight','AREA');ob=bpy.data.objects.new('soft daylight',light);s.collection.objects.link(ob)
 ob.location=(-9,-12,19);light.energy=3500;light.size=8
 cam=bpy.data.cameras.new('orthographic');ob=bpy.data.objects.new('orthographic',cam);s.collection.objects.link(ob)
 ob.location=(16,-22,16);ob.rotation_euler=(Vector(target)-ob.location).to_track_quat('-Z','Y').to_euler()
 cam.type='ORTHO';cam.ortho_scale=scale;s.camera=ob
def export(name):
 meshes=[o for o in bpy.data.objects if o.type=='MESH']
 assert meshes
 bpy.ops.object.select_all(action='DESELECT')
 for o in meshes:o.select_set(True)
 bpy.context.view_layer.objects.active=meshes[0]
 bpy.ops.export_scene.gltf(filepath=str(out/(name+'.glb')),export_format='GLB',use_selection=True)
 s=bpy.context.scene;s.render.filepath=str(out/(name+'.png'));bpy.ops.render.render(write_still=True)
 return {'name':name,'meshes':len(meshes),'glb_bytes':(out/(name+'.glb')).stat().st_size}
manifest=[]
for kind in ['wall','corner_inner','corner_outer','tower','gate','parapet']:
 build(kind);scene((0,0,3.3),18);manifest.append(export(kind))
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
wall('west',-10,0,8);wall('east',10,0,8);gate();tower('west_tower',-16,0);tower('east_tower',16,0)
scene((0,0,4),46);assembly=export('assembled_gatehouse')
(out/'manifest.json').write_text(json.dumps({'status':'awaiting visual review and Unity import','pieces':manifest,'assembly':assembly},indent=2))
print('ELDORIA_FORTIFICATION_EXPORT_PASS',len(manifest))
