"""Finish a pinned, existing ribera .blend. Never rebuild the pilot or gameplay.
Paint layers are independent packed images; bank edits act on inherited boundaries.
"""
import bpy, sys, pathlib, hashlib, json, math
import numpy as np
from mathutils import Vector
from mathutils.geometry import interpolate_bezier
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);source=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(source.read_bytes()).hexdigest()=='259932e1ac071417ba579edde228890c74ee97d0e0d6c3fee92ace894df113a4'
bpy.ops.wm.open_mainfile(filepath=str(source))
scene=bpy.context.scene
before={o.name:len(o.data.vertices) for o in scene.objects if o.type=='MESH'}
ops=[]
# Proportional sculpt strokes: alternating eroded pockets and resistant promontories.
# Each is an individually placed brush, not a regenerated river function.
strokes={-1:[(-77,5,1.0,-.5),(-60,6,-1.1,.4),(-43,4,1.4,-.8),(-27,7,-.9,.3),(-12,4,1.2,-.65),(4,6,-1.0,.5),(22,5,1.1,-.7),(39,6,-.9,.35),(58,4,1.3,-.6),(77,7,-.8,.3)],1:[(-84,6,-.8,.4),(-65,4,1.2,-.5),(-48,7,-1.2,.45),(-32,4,1.1,-.7),(-16,6,-.8,.4),(2,4,1.2,-.55),(18,7,-1.0,.3),(35,4,1.3,-.7),(52,7,-.9,.4),(72,5,1.1,-.5)]}
def sculpt(x,side):
 dy=dz=0.
 for cx,r,push,raise_z in strokes[side]:
  w=max(0,1-abs(x-cx)/r);w=w*w*(3-2*w);dy+=side*push*w;dz+=raise_z*w
 return dy,dz
edges={}
for side in (-1,1):
 o=bpy.data.objects['Pilot_Terrain_'+str(side)];n=85
 for i in range(193):
  dy,dz=sculpt(-96+i,side)
  for j in range(n):
   v=o.data.vertices[i*n+j];w=max(0,1-j/9);v.co.y+=dy*w;v.co.z+=dz*w
 edges[side]=np.array([tuple(o.data.vertices[i*n].co) for i in range(193)])
 for p in o.data.polygons:p.use_smooth=True
 # The UVs track world positions after sculpt, preserving paths and paint alignment.
 for loop in o.data.loops:
  co=o.data.vertices[loop.vertex_index].co;o.data.uv_layers.active.data[loop.index].uv=((co.x+96)/192,(co.y+78)/156)
 o.data.update();ops.append({'object':o.name,'operation':'proportional bank sculpt','vertices':193*9})
 # Edit inherited bank into a bevelled exposed earth/stone profile, sharing the exact top edge.
 bank=bpy.data.objects['Pilot_Cutbank_'+str(side)];oldmat=bank.data.materials[0]
 vv=[];ff=[]
 for i,(x,y,z) in enumerate(edges[side]):
  vv.extend([(x,y,z),(x,y-side*.28,z-.22),(x,y-side*.34,z*.58),(x,y+side*.14,.48),(x,y+side*.4,.08)])
 for i in range(192):
  for j in range(4):
   a=i*5+j;ff.append((a,a+5,a+6,a+1) if side==-1 else (a+1,a+6,a+5,a))
 data=bpy.data.meshes.new(bank.name+' sculpted profile');data.from_pydata(vv,[],ff);data.materials.append(oldmat);bank.data=data
 uv=data.uv_layers.new(name='BankStrataUV')
 for loop in data.loops:
  k=loop.vertex_index;uv.data[loop.index].uv=(k//5/192,(1,.9,.52,.14,0)[k%5])
 ops.append({'object':bank.name,'operation':'bevel and undercut existing bank boundary','vertices':len(vv)})
# Match water edges to the sculpt, never leave a gap or cover land with another terrain.
river=bpy.data.objects['Pilot_River']
for i in range(193):
 for j,side in enumerate((-1,1)):river.data.vertices[2*i+j].co.y=edges[side][i,1]+side*.45
for name,side in [('Pilot_RimSouth',-1),('Pilot_RimNorth',1)]:
 o=bpy.data.objects.get(name)
 if o:
  for i in range(193):
   y=edges[side][i,1]-side*.28
   for j in range(2):o.data.vertices[i*2+j].co.y=y+(j*2-1)*.10
# Painted material fields. Authored contours, paths and actual roots drive the layers.
S=1536;Y,X=np.mgrid[0:S,0:S].astype(np.float32);X=X/S*192-96;Y=Y/S*156-78
layers={};colours={}
def polygon(points):
 inside=np.zeros((S,S),bool);a=points[-1]
 for b in points:
  inside^=((a[1]>Y)!=(b[1]>Y))&(X<(b[0]-a[0])*(Y-a[1])/(b[1]-a[1]+1e-12)+a[0]);a=b
 return inside.astype(np.float32)
def soften(a,n=3):
 for _ in range(n):a=(a*4+np.roll(a,1,0)+np.roll(a,-1,0)+np.roll(a,1,1)+np.roll(a,-1,1))/8
 return a
def layer(name,mask,colour):layers[name]=np.clip(mask,0,1);colours[name]=colour
def stamp(a,x,y,rx,ry,opacity=1):
 x0=max(0,int((x-rx+96)/192*S));x1=min(S,int((x+rx+96)/192*S)+1);y0=max(0,int((y-ry+78)/156*S));y1=min(S,int((y+ry+78)/156*S)+1)
 if x0>=x1 or y0>=y1:return
 d=((X[y0:y1,x0:x1]-x)/rx)**2+((Y[y0:y1,x0:x1]-y)/ry)**2
 a[y0:y1,x0:x1]=np.maximum(a[y0:y1,x0:x1],np.clip((1-d)*3,0,1)*opacity)
contours=[ [(-94,35),(-78,29),(-65,33),(-53,19),(-38,16),(-31,23),(-47,29),(-52,39),(-72,42),(-87,48)], [(-26,4),(-15,1),(-2,4),(9,0),(25,-1),(31,-8),(18,-12),(3,-8),(-7,-12),(-22,-8)], [(29,40),(40,31),(47,19),(62,14),(74,18),(83,29),(72,35),(64,31),(52,36),(44,48)], [(-79,-33),(-68,-35),(-56,-44),(-43,-48),(-39,-57),(-54,-61),(-69,-53),(-82,-46)], [(-31,-34),(-22,-31),(-7,-37),(6,-40),(15,-48),(6,-57),(-6,-56),(-13,-47),(-27,-45)], [(36,-41),(49,-39),(62,-46),(73,-48),(81,-60),(69,-66),(55,-59),(44,-55)], [(-12,24),(-2,20),(12,26),(18,37),(9,48),(-2,40),(-14,37)] ]
meadow=np.zeros((S,S),np.float32)
for pts in contours:meadow=np.maximum(meadow,polygon(pts))
layer('01 meadow openings',soften(meadow,5),(.63,.64,.24))
dry=np.zeros_like(meadow)
for pts in contours[1:6:2]:dry=np.maximum(dry,polygon([(x+3,y-3) for x,y in pts]))
layer('02 exposed ochre earth',soften(dry,4)*.75,(.70,.54,.28))
forest=np.zeros_like(meadow)
for o in scene.objects:
 if o.name.startswith('Pilot_Pine'):
  x,y,z=o.location;stamp(forest,x,y,3.0*o.scale.x,2.6*o.scale.y,.75)
layer('03 pine litter',soften(forest,3),(.25,.33,.10))
bankmask=np.zeros_like(meadow);wet=np.zeros_like(meadow)
for side in (-1,1):
 edge=edges[side];ey=np.interp(X[0],edge[:,0],edge[:,1]);dist=side*(Y-ey[None,:])
 bankmask=np.maximum(bankmask,np.clip(1-dist/2.6,0,1)*(dist>=0))
 wet=np.maximum(wet,np.clip(1-dist/.6,0,1)*(dist>=0))
layer('04 eroded bank crown',soften(bankmask,2),(.68,.53,.29));layer('05 moist edge',soften(wet,2),(.32,.34,.16))
path=np.zeros_like(meadow);shoulder=np.zeros_like(meadow)
for o in scene.objects:
 if o.type=='CURVE' and o.name.startswith('Editable path'):
  bp=o.data.splines[0].bezier_points
  for a,b in zip(bp,bp[1:]):
   for p in interpolate_bezier(a.co,a.handle_right,b.handle_left,b.co,64):
    stamp(shoulder,p.x,p.y,2.1,1.8,.65);stamp(path,p.x,p.y,.95,.9,.98)
layer('06 worn shoulders',soften(shoulder,3),(.62,.52,.25));layer('07 footpath',soften(path,2),(.77,.62,.37))
stone=np.zeros_like(meadow)
for o in scene.objects:
 if o.name.startswith('Pilot_Rock'):stamp(stone,o.location.x,o.location.y,2.7,2.2,.72)
layer('08 mineral outcrops',soften(stone,3),(.49,.47,.33))
def image(name,rgb):
 h,w=rgb.shape[:2];rgba=np.ones((h,w,4),np.float32);rgba[:,:,:3]=rgb if rgb.ndim==3 else rgb[:,:,None]
 im=bpy.data.images.new(name,width=w,height=h);im.pixels.foreach_set(rgba.ravel());im.filepath_raw=str(out/(name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack();return im
rgb=np.empty((S,S,3),np.float32);rgb[:]=(.47,.53,.18)
soil=bpy.data.materials['Ochre meadow'];nodes=soil.node_tree.nodes;bs=next(n for n in nodes if n.type=='BSDF_PRINCIPLED')
frame=nodes.new('NodeFrame');frame.label='EDITABLE PAINT MASKS — meadow / soil / moisture / litter / paths';frame.name='Paint layers'
for k,(name,a) in enumerate(layers.items()):
 rgb=rgb*(1-a[:,:,None])+np.array(colours[name])*a[:,:,None]
 im=image(name,a);node=nodes.new('ShaderNodeTexImage');node.image=im;node.label=name;node.parent=frame;node.location=(k%4*260,-(k//4)*300)
# Deliberate grass blade brush hatching inside meadow margins. No random-noise surface.
for pts in contours:
 for (x,y),(xx,yy) in zip(pts,pts[1:]+pts[:1]):
  for t in (.15,.4,.7):
   cx=x+(xx-x)*t;cy=y+(yy-y)*t
   for dx,dy in [(-.3,0),(0,.15),(.3,-.15)]:
    a=polygon([(cx+dx-.09,cy+dy),(cx+dx+.08,cy+dy+.7),(cx+dx+.18,cy+dy+.1)])*.35
    rgb=rgb*(1-a[:,:,None])+np.array((.67,.68,.29))*a[:,:,None]
im=image('Ribera_surface_painted',rgb);tex=nodes.new('ShaderNodeTexImage');tex.image=im;tex.label='Exported painted composite — source masks retained';nodes.active=tex;soil.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);bs.inputs['Roughness'].default_value=.9
# Bank UV painting: a pale lip, cooler exposed strata and dark damp foot, plus fractures.
H=256;W=1536;v,u=np.mgrid[0:H,0:W].astype(np.float32);v/=H;u/=W
bankrgb=np.zeros((H,W,3),np.float32)
for channel,(bottom,top) in enumerate(zip((.19,.25,.22),(.58,.49,.34))):bankrgb[:,:,channel]=bottom+(top-bottom)*np.clip(v*1.2,0,1)
for level,colour in [(.25,(.29,.33,.28)),(.52,(.45,.43,.34)),(.87,(.69,.58,.37))]:
 a=np.clip(1-abs(v-level)/.035,0,1)[:,:,None]*.42;bankrgb=bankrgb*(1-a)+np.array(colour)*a
for x,tilt in [(i,t) for i,t in zip([.04,.09,.16,.22,.28,.35,.41,.49,.56,.62,.69,.76,.83,.91,.97],[.006,-.009,.003,.008,-.005,.004,-.01,.005,.009,-.004,.007,-.003,.005,-.008,.004])]:
 a=np.clip(1-abs(u-x-tilt*v)/.0013,0,1)[:,:,None]*np.clip((.95-v)*3,0,1)[:,:,None]*.5;bankrgb*=1-a
bankim=image('Ribera_bank_strata',bankrgb);mat=bpy.data.materials['Warm cut riverbank'];bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bankim;mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);mat.node_tree.nodes.active=tex
ops.append({'operation':'eight editable UV paint masks, painted composite and bank strata','random_noise':False})
# Keep all inherited objects, curves and gameplay-free source. Inspectable provenance.
scene['source_sha256']=hashlib.sha256(source.read_bytes()).hexdigest();scene['finishing_stage']='A surfaces and sculpted banks';scene['art_approved']=False
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH':o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'))
scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'source_sha256':scene['source_sha256'],'stage':'A','operations':ops,'inherited_mesh_objects':len(before),'preserved_objects':all(n in bpy.data.objects for n in before),'art_approved':False},indent=2))
