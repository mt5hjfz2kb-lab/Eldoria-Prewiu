"""H: directed forest-edge composition on the retained ribera; no gameplay changes."""
import bpy,sys,pathlib,json,hashlib,math
import numpy as np
from mathutils import Vector
from mathutils.geometry import interpolate_bezier
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);src=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='b91c9c991257dabdcfacfe83cbd67a248bab54080eb845704cf24b1b372bb339'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene
assert scene.get('finishing_stage','').startswith('G D foliage')
terrain=[bpy.data.objects['Pilot_Terrain_'+str(i)] for i in (-1,1)]
def ground(x,y):
 for o in terrain:
  hit,co,n,index=o.ray_cast(Vector((x,y,80)),Vector((0,0,-1)))
  if hit:return co.z
 return None
# Identify the 18 inherited pines moved in B; redistribute the same objects into distinct silhouettes.
anchors=[(-39,-13),(-8,-4),(17,-17),(42,-21),(-8,-38),(49,-48)]
old_offsets=[(-2.2,.5),(1.3,1.7),(.2,-2.1)]
trees=[o for o in scene.objects if o.name.startswith('Pilot_Pine')];selected=[]
for cx,cy in anchors:
 for dx,dy in old_offsets:
  o=min(trees,key=lambda o:(o.location.x-cx-dx)**2+(o.location.y-cy-dy)**2)
  assert o not in selected;selected.append(o)
# A crescent, a solitary anchor, an uneven ridge group, a fork, a pair and a low cluster.
placements=[(-43,-12,.85),(-40,-9,1.05),(-37,-11,.67),(-36,-15,.82),(-42,-16,.55),(-8,-3,1.10),(13,-15,.72),(17,-13,1.08),(20,-16,.87),(22,-20,.55),(39,-22,.92),(43,-19,1.12),(46,-23,.65),(-11,-37,.92),(-6,-40,.68),(46,-47,.68),(50,-46,.9),(52,-50,.52)]
moved=[]
for i,(o,(x,y,s)) in enumerate(zip(selected,placements)):
 z=ground(x,y);assert z is not None
 moved.append({'object':o.name,'from':tuple(o.location),'to':(x,y,z-.08)});o.location=(x,y,z-.08);o.scale=(s*(.9 if i%3==0 else 1),s,s*(1.08 if i%4==0 else .94));o.rotation_euler[2]=[.25,1.2,2.7,.85,2.0][i%5]
# Editable Bezier meadow edges: islands tied to the six actual clusters and three deliberate open meadows.
coll=bpy.data.collections.new('09_Directed_forest_edges');scene.collection.children.link(coll)
S=2048;Y,X=np.mgrid[0:S,0:S].astype(np.float32);X=X/S*192-96;Y=Y/S*156-78
coverage=np.zeros((S,S),np.float32)
def outline(name,points):
 curve=bpy.data.curves.new(name,'CURVE');curve.dimensions='3D';spl=curve.splines.new('BEZIER');spl.bezier_points.add(len(points)-1);spl.use_cyclic_u=True
 for p,(x,y) in zip(spl.bezier_points,points):p.co=(x,y,12);p.handle_left_type='AUTO';p.handle_right_type='AUTO'
 obj=bpy.data.objects.new(name,curve);coll.objects.link(obj);obj.hide_render=True;bpy.context.view_layer.update()
 pts=[]
 for i,p in enumerate(spl.bezier_points):
  q=spl.bezier_points[(i+1)%len(points)];pts.extend([(v.x,v.y) for v in interpolate_bezier(p.co,p.handle_right,q.handle_left,q.co,12)[:-1]])
 a=pts[-1];m=np.zeros((S,S),bool)
 for b in pts:m^=((a[1]>Y)!=(b[1]>Y))&(X<(b[0]-a[0])*(Y-a[1])/(b[1]-a[1]+1e-12)+a[0]);a=b
 return m.astype(np.float32)
shapes=[ [(-7,1),(-5,5),(-1,6),(2,3),(6,2),(5,-2),(2,-4),(-1,-3),(-4,-5),(-7,-2)], [(-4,0),(-3,3),(0,4),(3,1),(2,-2),(0,-4),(-2,-2)], [(-7,3),(-4,6),(0,4),(3,2),(5,-1),(7,-4),(4,-6),(1,-3),(-3,-3),(-5,0)], [(-6,1),(-4,4),(0,5),(3,3),(6,2),(5,-2),(1,-3),(-1,-5),(-4,-3)], [(-6,0),(-4,3),(-1,2),(2,0),(5,-3),(2,-5),(-1,-3),(-5,-3)], [(-6,2),(-3,4),(0,2),(3,3),(5,-1),(3,-4),(0,-3),(-2,-5),(-5,-2)] ]
for i,((cx,cy),pts) in enumerate(zip(anchors,shapes)):coverage=np.maximum(coverage,outline('Meadow edge cluster '+str(i),[(cx+x,cy+y) for x,y in pts]))
# Central negative space is divided into broad irregular meadows with clear earth connections, not rows of repeated stamps.
for i,pts in enumerate([
 [(-38,4),(-34,12),(-23,15),(-16,11),(-11,14),(-5,10),(-7,4),(-15,2),(-20,-1),(-29,1)],
 [(0,8),(4,16),(10,18),(15,14),(22,15),(27,10),(22,5),(16,6),(13,1),(6,3)],
 [(27,-32),(33,-28),(38,-31),(40,-37),(35,-40),(31,-38),(25,-39),(21,-35)] ]):coverage=np.maximum(coverage,outline('Open meadow '+str(i),pts))
# Outside the central river corridor retain the D grass treatment and existing forest floor.
central=np.clip((63-np.abs(X))/8,0,1)*np.clip((39-np.abs(Y+14))/8,0,1)
for _ in range(12):coverage=(4*coverage+np.roll(coverage,1,0)+np.roll(coverage,-1,0)+np.roll(coverage,1,1)+np.roll(coverage,-1,1))/8
def pix(name):
 im=bpy.data.images[name];w,h=im.size;a=np.array(im.pixels[:],np.float32).reshape(h,w,4);ix=np.minimum((np.arange(S)*w/S).astype(int),w-1);iy=np.minimum((np.arange(S)*h/S).astype(int),h-1);return a[iy[:,None],ix[None,:]]
base=pix('G stylized grass and earth finish');d=pix('Ribera_pictorial_ground');ratio=np.clip(base[:,:,:3]/np.maximum(d[:,:,:3],.05),.82,1.16)
earth=np.array([.75,.61,.34]);grass=np.array([.48,.55,.20]);new=(earth[None,None,:]*(1-coverage[:,:,None])+grass[None,None,:]*coverage[:,:,None])*ratio
for name,col,strength in [('07 footpath',(.81,.67,.41),.92),('04 eroded bank crown',(.72,.61,.39),.9),('05 moist edge',(.39,.45,.23),.5)]:
 a=pix(name)[:,:,0,None]*strength;new=new*(1-a)+np.array(col)*a
rgba=np.ones((S,S,4),np.float32);rgba[:,:,:3]=base[:,:,:3]*(1-central[:,:,None])+new*central[:,:,None]
soil=bpy.data.materials['Ochre meadow'];nodes=soil.node_tree.nodes;links=soil.node_tree.links;bs=next(n for n in nodes if n.type=='BSDF_PRINCIPLED')
def new_image(name,a):
 im=bpy.data.images.new(name,width=S,height=S);im.colorspace_settings.name='sRGB';im.pixels.foreach_set(a.astype(np.float32).ravel());im.pack();return im
im=new_image('H directed meadow and earth',rgba);n=nodes.new('ShaderNodeTexImage');n.image=im;links.new(n.outputs['Color'],bs.inputs['Base Color']);nodes.active=n
mask=np.ones((S,S,4),np.float32);mask[:,:,:3]=coverage[:,:,None];new_image('H editable meadow coverage',mask)
# Low curved leaf fans, composed at forest margins. One combined mesh, three shared pigments.
materials=[]
for name,c in [('Understory shade',(.13,.23,.055,1)),('Understory green',(.24,.35,.08,1)),('Understory tips',(.43,.46,.13,1))]:
 m=bpy.data.materials.new(name);m.diffuse_color=c;m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=c;materials.append(m)
verts=[];faces=[];mi=[];plants=0
edge_sites=[(-45,-13),(-40,-18),(-35,-13),(-37,-8),(-11,-4),(-6,-5),(12,-17),(17,-18),(21,-14),(23,-21),(37,-24),(42,-25),(46,-18),(-13,-38),(-7,-42),(44,-48),(48,-51),(54,-49),(-28,6),(-23,13),(-10,6),(5,11),(15,14),(21,8),(29,-35),(36,-32)]
for idx,(cx,cy) in enumerate(edge_sites):
 for j,(dx,dy) in enumerate([(0,0),(.85,-.45),(-.65,.6)] if idx%3 else [(0,0),(.9,.3)]):
  x,y=cx+dx,cy+dy;z=ground(x,y)
  if z is None:continue
  plants+=1;scale=[.7,.95,.55,1.15][(idx+j)%4]
  for k,angle in enumerate([.1,.95,2.1,3.0,4.3,5.4]):
   angle+=idx*.27;height=scale*(.6+.13*(k%3));length=scale*(.7+.16*((k+1)%3));width=.17*scale;start=len(verts)
   for t in [0,.25,.55,.8,1]:
    r=length*t;zz=z+height*math.sin(t*math.pi*.66);ww=width*math.sin(math.pi*t)**.7
    for side in [-1,1]:verts.append((x+math.cos(angle)*r-math.sin(angle)*ww*side,y+math.sin(angle)*r+math.cos(angle)*ww*side,zz))
   for q in range(4):faces.append((start+q*2,start+q*2+1,start+q*2+3,start+q*2+2));mi.append(2 if q==3 else (k+idx)%2)
mesh=bpy.data.meshes.new('Curved understory leaves');mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new('Directed understory mesh',mesh);coll.objects.link(o)
for m in materials:mesh.materials.append(m)
for p,k in zip(mesh.polygons,mi):p.material_index=k;p.use_smooth=True
scene['finishing_stage']='H directed meadow islands and asymmetric inherited forest groups';scene['H_source_sha256']=hashlib.sha256(src.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.type=='IMAGE' and im.size[0]>0:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'H','source_G_sha256':scene['H_source_sha256'],'moved_inherited_pines':moved,'editable_meadow_curves':9,'understory_plants':plants,'understory_quads':len(faces),'art_approved':False},indent=2))
