"""Reusable wood/stone representations from certified AI source mesh topology.
Not live content: logical placement/progression remain owned by the existing gateway.
"""
import bpy,sys,pathlib,hashlib,math,json
from mathutils import Vector
out,src=map(pathlib.Path,sys.argv[sys.argv.index('--')+1:]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='28f5bc9665145244fb3b3ac400983e184ef664afdd6da0b8c04dac6fae5a23fc'
bpy.ops.wm.open_mainfile(filepath=str(src));bpy.context.preferences.filepaths.save_version=0;scene=bpy.context.scene
fir=bpy.data.objects['Editable Layered fir source'];slender=bpy.data.objects['Editable Slender fir source'];rock=next(o for o in scene.objects if o.name.startswith('Pilot_Rock') and o.type=='MESH')
collection=bpy.data.collections.new('Dynamic resource library - authored separately from geography');scene.collection.children.link(collection)
objects=[];roots=[]
def material(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;b=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');b.inputs['Base Color'].default_value=(*color,1);b.inputs['Roughness'].default_value=.95;return m
stone=[material('Stone warm limestone',(.48,.44,.33)),material('Stone weathered planes',(.35,.34,.29)),material('Stone pale broken faces',(.64,.59,.46))]
bark=material('Wood deeply grooved bark',(.19,.115,.055));heart=material('Wood cut heart',(.58,.37,.17))
# Hand-worked resource features use ONLY the real root/model, no fake nodes.
# Faceted low-poly geometry is shared by the three editable Blender variants.
def finished_mesh(root,name,points,faces,mats,face_mats):
 me=bpy.data.meshes.new(name+' mesh')
 me.from_pydata(points,[],faces);me.update()
 for mat in mats:me.materials.append(mat)
 for face,index in zip(me.polygons,face_mats):face.material_index=index
 ob=bpy.data.objects.new(name,me);collection.objects.link(ob)
 ob.parent=root;objects.append(ob)
 return ob

def chopped_stump(root,x,y,height,radius,variant,number):
 sides=12;lower=[];upper=[]
 for i in range(sides):
  angle=math.tau*i/sides
  jitter=1+.07*math.sin(i*2.71+variant+number)
  lower.append((x+radius*math.cos(angle)*jitter,y+radius*math.sin(angle)*jitter,0))
  upper.append((x+radius*.89*math.cos(angle)*jitter,y+radius*.89*math.sin(angle)*jitter,height+.055*math.cos(angle*3+variant)))
 points=lower+upper+[(x,y,height)]
 faces=[];ids=[]
 for i in range(sides):
  j=(i+1)%sides
  faces.append((i,j,j+sides,i+sides));ids.append(0)
  faces.append((i+sides,j+sides,2*sides));ids.append(1)
 finished_mesh(root,'Hand-cut timber stump '+str(number),points,faces,[bark,heart],ids)

def dressed_stone(root,name,x,y,z,dx,dy,dz,variant):
 # Eight chamfered perimeter corners: carved ledges, not generic stone cubes.
 profile=[(-1,-.71),(-.72,-1),(.75,-1),(1,-.70),(1,.72),(.73,1),(-.77,1),(-1,.72)]
 bottom=[(x+u*dx/2,y+v*dy/2,z) for u,v in profile]
 top=[(x+u*dx*.48,y+v*dy*.48,z+dz+.035*math.sin(variant+u*4+v*2)) for u,v in profile]
 pts=bottom+top+[(x,y,z+dz+.035)]
 faces=[];ids=[]
 for k in range(8):
  j=(k+1)%8
  faces.append((k,j,8+j,8+k));ids.append((k//3+variant)%3)
  faces.append((8+k,8+j,16));ids.append((1+variant)%3)
 finished_mesh(root,name,pts,faces,stone,ids)

# One authentic hand-built lifting gantry per authored QUARRY node. The real
# current Unity camera shows the weathered rock outcrop remains nearly
# indistinguishable from generic stones; this gives a worked-site silhouette.
# Every timber/rope is part of that resource's existing GLB, not an extra node,
# hotspot, loot entity or separate scene. ~120 triangles / instance.
oak=material('Quarry sun-baked timber',(.39,.28,.16))
rope=material('Quarry hoist dark hemp',(.17,.12,.067))
def quarry_hoist(root,variant):
 vertices=[];faces=[];slots=[]
 def prism(p,q,thickness,slot):
  p=Vector(p);q=Vector(q);axis=(q-p).normalized()
  side=axis.cross(Vector((0,0,1)))
  if side.length<.001:side=Vector((1,0,0))
  side.normalize()
  up=axis.cross(side).normalized()
  offsets=[(-1,-1),(1,-1),(1,1),(-1,1)]
  base=len(vertices)
  for origin in (p,q):
   for a,b in offsets:
    pos=origin+side*a*thickness/2+up*b*thickness/2
    vertices.append(tuple(pos))
  for ids in ((0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)):
   faces.append(tuple(base+k for k in ids));slots.append(slot)
 side=3.5
 height=6.2+variant*.06
 for yy in (-2.8,2.8):
  prism((side,yy,0),(side,yy,height),.39,0)
  prism((side,yy*1.39,0),(side,yy,height-.65),.27,0)
 prism((side,-3.25,height),(side,3.25,height),.54,0)
 prism((side,0,height-.15),(side,0,2.35),.085,1)
 prism((side-.72,-.6,2.4),(side+.72,-.6,2.4),.23,0)
 prism((side-.72,.6,2.4),(side+.72,.6,2.4),.23,0)
 prism((side-.72,-.6,2.4),(side-.72,.6,2.4),.23,0)
 prism((side+.72,-.6,2.4),(side+.72,.6,2.4),.23,0)
 finished_mesh(root,'Quarry lifting gantry and suspended wooden stone sling',
               vertices,faces,[oak,rope],slots)

def clone(donor,name,root,position,scale,rotation):
 o=donor.copy();o.data=donor.data.copy();collection.objects.link(o);o.name=name;o.parent=root;o.hide_render=False;o.hide_viewport=False;o.hide_set(False);o.location=position;o.scale=scale;o.rotation_euler=(0,0,rotation);objects.append(o);return o
for family in ['Wood','Stone']:
 for variant in range(3):
  root=bpy.data.objects.new('Resource_'+family+'_'+str(variant),None);collection.objects.link(root);roots.append(root)
  if family=='Wood':
   for k,(x,y,s) in enumerate([(-2.6,.7,.92),(.8,2.1,1.18),(2.3,-.4,.78),(-.8,-1.9,.67)]):
    a=variant*.65;xx=x*math.cos(a)-y*math.sin(a);yy=x*math.sin(a)+y*math.cos(a)
    clone(fir if k%2==0 else slender,'Wood crown '+str(k),root,(xx,yy,0),(s*1.5,s*1.5,s*1.7),variant+k*1.7)
   # Wood nodes need harvest identity, unlike the surrounding decorative firs.
   # Three cut stumps show worked timber in the very same resource model.
   chopped_stump(root,-3.6,-2.1,.66,.73,variant,0)
   chopped_stump(root,3.45,-2.35,.44,.51,variant,1)
   chopped_stump(root,2.95,2.55,.90,.62,variant,2)
   # A recognizable harvested timber stack in front of the canopy.
   # Unlike more decorative pines, this is composed of cut trunks with clear
   # heartwood end rings; one grounded mesh means stacked pieces stay stacked
   # after the real world-fitting importer adjusts bottom contact.
   stack_verts=[];stack_faces=[];stack_materials=[]
   for row in range(2):
    for col in range(3):
     px=-2.35+col*1.05;py=-3.55+row*.62
     z=.38+row*.57
     radius=.37+.04*math.sin(col+variant)
     n=len(stack_verts)
     for end_x in [-1.65,1.65]:
      for side in range(12):
       angle=math.tau*side/12
       stack_verts.append((px+end_x,py+radius*math.cos(angle),z+radius*math.sin(angle)))
     for side in range(12):
      following=(side+1)%12
      stack_faces.append((n+side,n+following,n+12+following,n+12+side));stack_materials.append(0)
     stack_faces.append(tuple(reversed(tuple(n+i for i in range(12)))));stack_materials.append(1)
     stack_faces.append(tuple(n+12+i for i in range(12)));stack_materials.append(1)
   finished_mesh(root,'Sawn timber reserve with end grain',stack_verts,stack_faces,[bark,heart],stack_materials)
   # Curved/tapered trunk loft with exposed end grain, not a stack of cylinder primitives.
   for log in range(2):
    vv=[];ff=[]
    for j in range(5):
     t=j/4;rad=(.32-.09*t)*(1+.06*math.sin(j*2.1))
     for i in range(10):
      a=i*math.tau/10;rr=rad*(1+.07*math.sin(i*2.3+variant));vv.append((-1.4+t*3.1,-2.8+log*.65+math.cos(a)*rr,.34+math.sin(a)*rr+.08*math.sin(t*math.pi)))
    for j in range(4):
     for i in range(10):a=j*10+i;b=j*10+(i+1)%10;ff.append((a,b,b+10,a+10))
    ff.extend([tuple(reversed(range(10))),tuple(range(40,50))]);me=bpy.data.meshes.new('Tapered split timber');me.from_pydata(vv,[],ff);me.materials.append(bark);me.materials.append(heart)
    for p in me.polygons:p.material_index=1 if p.index>=40 else 0
    o=bpy.data.objects.new('Wood cut trunk '+str(log),me);collection.objects.link(o);o.parent=root;objects.append(o)
  else:
   for k,(x,y,h,s) in enumerate([(-1.7,.2,4.8,1.8),(.6,1.2,6.5,2.1),(2.4,-.5,3.7,1.6),(-.3,-1.8,2.3,1.3),(-3.4,-1.3,1.3,.8),(3.8,1.4,1.5,.85)]):
    o=clone(rock,'Stone fractured mass '+str(k),root,(x,y,0),(1,1,1),variant*.7+k*.93)
    coords=[v.co.copy() for v in o.data.vertices];lo=Vector(tuple(min(p[i] for p in coords) for i in range(3)));hi=Vector(tuple(max(p[i] for p in coords) for i in range(3)));span=hi-lo
    for v in o.data.vertices:
     v.co.x=(v.co.x-(hi.x+lo.x)/2)/max(.01,span.x)*s*1.7
     v.co.y=(v.co.y-(hi.y+lo.y)/2)/max(.01,span.y)*s*1.45
     v.co.z=(v.co.z-lo.z)/max(.01,span.z)*h*.65
     v.co.x+=v.co.z*.15*math.sin(k+variant)
    o.data.materials.clear()
    for m in stone:o.data.materials.append(m)
    for p in o.data.polygons:p.material_index=(p.index+k+variant)%3;p.use_smooth=False
# Carved extraction ledges and original weathered rock read as a worked
# quarry from ABOVE. One lightweight mesh parented to the same real resource.
def quarry_rim(root,variant):
 vertices=[];faces=[];slots=[]
 n=16
 for radius,height in ((4.55,.05),(3.75,.50),(2.85,1.05),(2.0,1.48)):
  for j in range(n):
   angle=math.tau*j/n
   jitter=1+.065*math.sin(j*1.43+variant*2.7)+.04*math.sin(j*2.81)
   vertices.append((.3+radius*math.cos(angle)*jitter,
                    .15+.83*radius*math.sin(angle)*jitter,
                    height+.04*math.sin(3*angle+variant)))
 for layer in range(3):
  for j in range(n):
   nxt=(j+1)%n
   faces.append((layer*n+j,layer*n+nxt,(layer+1)*n+nxt,(layer+1)*n+j))
   slots.append((layer+j//5+variant)%len(stone))
 finished_mesh(root,'Quarry terraced exposed ledges',vertices,faces,stone,slots)

# Carved extraction benches and prepared masonry change the quarry silhouette.
# The original rock donor still supplies the central weathered geology.
for variant in range(3):
 root=next(x for x in roots if x.name=='Resource_Stone_'+str(variant))
 quarry_rim(root,variant)
 quarry_hoist(root,variant)
 dressed_stone(root,'Quarry exposed lower bench',-.55,-2.35,.02,5.3,2.6,.46,variant)
 dressed_stone(root,'Quarry weathered second bench',-.65,-1.8,.44,4.4,1.9,.48,variant+1)
 dressed_stone(root,'Quarry stepped cut face',-.85,-1.33,.87,3.1,1.15,.42,variant+2)
 for i,(x,y,z) in enumerate([(-3.45,-2.55,.08),(3.45,-2.05,.07),(2.85,2.45,.07)]):
  dressed_stone(root,'Quarry worked stone '+str(i),x,y,z,1.22,1.0,.74+.12*i,variant+i)
# Place all variant bases exactly on z=0, preserving individual silhouettes and meaningful pivots.
for root in roots:
 children=list(root.children);bpy.context.view_layer.update()
 bottom=min((o.matrix_world@Vector(c)).z for o in children for c in o.bound_box)
 for o in children:o.location.z-=bottom
bpy.ops.object.select_all(action='DESELECT')
for o in roots+objects:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneResources.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Resource_Library.blend'))
(out/'resource-library.json').write_text(json.dumps({'families':['wood','stone'],'variants_per_family':3,'unlocked_content':'existing Bastion I-II wood and stone only','food_field_not_represented_as_harvest_node':True,'creatures_generated':0,'source_ai_sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'art_approved':False},indent=2))
# Genuine orthographic impostors rendered from this exact reusable library.
# They depict the same authorized wood/stone models, never a second geography.
scene.render.engine='BLENDER_EEVEE';scene.render.resolution_x=256;scene.render.resolution_y=256;scene.render.resolution_percentage=100
scene.render.film_transparent=True;scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA';scene.use_nodes=False
for obj in scene.objects:obj.hide_render=True
for root in roots:root.hide_render=False
camera_data=bpy.data.cameras.new('Resource impostor orthographic camera');camera=bpy.data.objects.new(camera_data.name,camera_data);scene.collection.objects.link(camera);camera.hide_render=False;camera_data.type='ORTHO';camera_data.ortho_scale=12;camera.location=(10,-16,16);camera.rotation_euler=(Vector((0,0,3))-camera.location).to_track_quat('-Z','Y').to_euler();scene.camera=camera
light_data=bpy.data.lights.new('Resource impostor soft key','AREA');light_data.energy=1800;light_data.shape='DISK';light_data.size=8;light=bpy.data.objects.new(light_data.name,light_data);scene.collection.objects.link(light);light.hide_render=False;light.location=(-8,-10,18);light.rotation_euler=(Vector((0,0,2))-light.location).to_track_quat('-Z','Y').to_euler()
scene.world.use_nodes=True;scene.world.node_tree.nodes.get('Background').inputs[0].default_value=(.45,.53,.64,1);scene.world.node_tree.nodes.get('Background').inputs[1].default_value=.65
import numpy as np
atlas=np.zeros((256,512,4),np.float32)
for column,family in enumerate(['Wood','Stone']):
 for obj in objects:obj.hide_render=obj.parent.name!='Resource_'+family+'_0'
 path=out/('Resource_'+family+'_impostor.png');scene.render.filepath=str(path);bpy.ops.render.render(write_still=True)
 picture=bpy.data.images.load(str(path));atlas[:,column*256:(column+1)*256,:]=np.array(picture.pixels[:]).reshape(256,256,4)
im=bpy.data.images.new('RegionOneResourceImpostors',512,256,alpha=True);im.pixels.foreach_set(atlas.ravel());im.filepath_raw=str(out/'RegionOneResourceImpostors.png');im.file_format='PNG';im.save();im.pack()
for obj in objects:obj.hide_render=False
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Resource_Library.blend'),compress=True)
