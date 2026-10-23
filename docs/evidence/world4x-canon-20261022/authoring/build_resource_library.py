"""Reusable wood/stone representations from certified AI source mesh topology.
Not live content: logical placement/progression remain owned by the existing gateway.
"""
import bpy,sys,pathlib,hashlib,math,json
from mathutils import Vector
out,src=map(pathlib.Path,sys.argv[sys.argv.index('--')+1:]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='28f5bc9665145244fb3b3ac400983e184ef664afdd6da0b8c04dac6fae5a23fc'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene
fir=bpy.data.objects['Editable Layered fir source'];slender=bpy.data.objects['Editable Slender fir source'];rock=next(o for o in scene.objects if o.name.startswith('Pilot_Rock') and o.type=='MESH')
collection=bpy.data.collections.new('Dynamic resource library - authored separately from geography');scene.collection.children.link(collection)
objects=[];roots=[]
def material(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;b=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');b.inputs['Base Color'].default_value=(*color,1);b.inputs['Roughness'].default_value=.95;return m
stone=[material('Stone warm limestone',(.48,.44,.33)),material('Stone weathered planes',(.35,.34,.29)),material('Stone pale broken faces',(.64,.59,.46))]
bark=material('Wood deeply grooved bark',(.19,.115,.055));heart=material('Wood cut heart',(.58,.37,.17))
def clone(donor,name,root,position,scale,rotation):
 o=donor.copy();o.data=donor.data.copy();collection.objects.link(o);o.name=name;o.parent=root;o.hide_render=False;o.hide_viewport=False;o.hide_set(False);o.location=position;o.scale=scale;o.rotation_euler=(0,0,rotation);objects.append(o);return o
for family in ['Wood','Stone']:
 for variant in range(3):
  root=bpy.data.objects.new('Resource_'+family+'_'+str(variant),None);collection.objects.link(root);roots.append(root)
  if family=='Wood':
   for k,(x,y,s) in enumerate([(-2.6,.7,.92),(.8,2.1,1.18),(2.3,-.4,.78),(-.8,-1.9,.67)]):
    a=variant*.65;xx=x*math.cos(a)-y*math.sin(a);yy=x*math.sin(a)+y*math.cos(a)
    clone(fir if k%2==0 else slender,'Wood crown '+str(k),root,(xx,yy,0),(s,s,s),variant+k*1.7)
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
