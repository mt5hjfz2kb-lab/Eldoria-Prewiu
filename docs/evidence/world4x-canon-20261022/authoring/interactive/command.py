"""Owner-approved quarry and timber depot, authored through existing BlenderMCP.
Keeps the six real resource roots; replaces only their presentation geometry.
"""
import bpy,math,pathlib,json,hashlib
import numpy as np
from mathutils import Vector
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
scene=bpy.context.scene
original_roots=[bpy.data.objects['Resource_'+f+'_'+str(i)] for f in ['Wood','Stone'] for i in range(3)]
input_hash=hashlib.sha256(pathlib.Path(bpy.data.filepath).read_bytes()).hexdigest()
for root in original_roots:
 for ob in list(root.children):bpy.data.objects.remove(ob,do_unlink=True)
materials=[]
def painted(name,base,kind):
 size=512;u,v=np.meshgrid(np.linspace(0,1,size),np.linspace(0,1,size))
 colour=np.empty((size,size,4),np.float32);colour[:]=(*base,1)
 if kind=='wood':
  grain=.025*np.sin(u*87+2*np.sin(v*6))+.013*np.sin(u*211+np.sin(v*17))
  grain+=.012*np.sin(u*39+3*np.sin(v*3));colour[:,:,:3]+=grain[:,:,None]
 elif kind=='cut':
  r=np.hypot(u-.5,v-.5);a=np.arctan2(v-.5,u-.5)
  ring=np.sin((r+.008*np.sin(a*4))*145)
  colour[:,:,:3]-=(.055*np.maximum(0,ring)**9)[:,:,None]
  for angle in [-2.1,.25,1.6]:
   gap=np.abs(np.sin(a-angle))*r
   colour[:,:,:3]-=(.14*((gap<.004)&(r>.29)))[:,:,None]
 elif kind=='stone':
  chisel=.009*np.sin(u*257+v*34)+.008*np.cos(v*191-u*21)
  layers=.018*np.sin(v*34)+.012*np.sin(v*87)
  colour[:,:,:3]+=(chisel+layers)[:,:,None]
  for x,slope in [(.14,.04),(.62,-.08),(.89,.06)]:
   line=np.exp(-((u-x-v*slope)/.0025)**2)*.065
   colour[:,:,:3]-=line[:,:,None]
 elif kind=='cloth':
  colour[:,:,:3]+=(.007*np.sin(u*800)+.007*np.sin(v*800))[:,:,None]
  green=np.array((.13,.20,.105));stripe=(u<.14)|(u>.86)
  # Painted pine emblem belongs to the approved physical cloth, not map UI.
  tree=((v>.25)&(v<.47)&(abs(u-.5)<(.47-v)*.85))|((v>.40)&(v<.62)&(abs(u-.5)<(.62-v)*.67))|((v>.55)&(v<.79)&(abs(u-.5)<(.79-v)*.50))|((abs(u-.5)<.018)&(v>.18)&(v<.35))
  colour[stripe|tree,:3]=green
 colour=np.clip(colour,0,1)
 image=bpy.data.images.new(name+' painted map',size,size,alpha=True)
 image.pixels.foreach_set(colour.ravel());image.filepath_raw=str(out/(name+'.png'));image.file_format='PNG';image.save()
 packed=bpy.data.images.load(image.filepath_raw,check_existing=False);packed.pack()
 mat=bpy.data.materials.new(name);mat.use_nodes=True
 bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Roughness'].default_value=.88
 node=mat.node_tree.nodes.new('ShaderNodeTexImage');node.image=packed
 mat.node_tree.links.new(node.outputs['Color'],bs.inputs['Base Color'])
 materials.append(mat);return len(materials)-1
def flat(name,colour):
 m=bpy.data.materials.new(name);m.use_nodes=True;b=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');b.inputs['Base Color'].default_value=(*colour,1);b.inputs['Roughness'].default_value=.8
 materials.append(m);return len(materials)-1
WOOD=painted('Approved worn honey timber',(.32,.16,.065),'wood')
BARK=painted('Approved dark fissured bark',(.19,.095,.035),'wood')
CUT=painted('Approved cut end grain',(.62,.37,.14),'cut')
STONE=painted('Approved pale quarry limestone',(.72,.61,.44),'stone')
CLOTH=painted('Approved linen with painted fir',(.63,.56,.40),'cloth')
ROPE=flat('Approved flax rope',(.47,.33,.17))
IRON=flat('Approved forged iron',(.075,.085,.095))
STEEL=flat('Approved worn axe edge',(.30,.34,.37))
LEAF=flat('Approved low crevice vegetation',(.19,.29,.075))
SOIL=flat('Approved exposed quarry floor',(.43,.36,.235))
class Builder:
 def __init__(self):self.v=[];self.f=[];self.m=[];self.uv=[]
 def face(self,coords,mat,uv=None):
  n=len(self.v);self.v.extend([tuple(x) for x in coords]);self.f.append(tuple(range(n,n+len(coords))));self.m.append(mat)
  self.uv.append(uv or [(0,0),(1,0),(1,1),(0,1)][:len(coords)])
 def prism(self,p,q,width,depth=None,mat=WOOD,cap=None):
  p,q=Vector(p),Vector(q);d=(q-p).normalized();a=d.cross(Vector((0,0,1)))
  if a.length<.01:a=Vector((1,0,0))
  a.normalize();b=d.cross(a).normalized();depth=width if depth is None else depth
  profile=[(-1,-.78),(-.78,-1),(.78,-1),(1,-.78),(1,.78),(.78,1),(-.78,1),(-1,.78)]
  rings=[[o+a*x*width*.5+b*y*depth*.5 for x,y in profile] for o in [p,q]]
  for i in range(8):
   j=(i+1)%8;self.face([rings[0][i],rings[0][j],rings[1][j],rings[1][i]],mat,[(0,0),(1,0),(1,(q-p).length/max(width,.2)),(0,(q-p).length/max(width,.2))])
  for ring,reverse in [(rings[0],True),(rings[1],False)]:
   centre=sum(ring,Vector())/8
   for i in range(8):
    j=(i+1)%8;ids=[i,j] if not reverse else [j,i]
    self.face([centre,ring[ids[0]],ring[ids[1]]],mat if cap is None else cap,[(.5,.5),((profile[ids[0]][0]+1)/2,(profile[ids[0]][1]+1)/2),((profile[ids[1]][0]+1)/2,(profile[ids[1]][1]+1)/2)])
 def box(self,x,y,z,dx,dy,dz,mat=STONE):self.prism((x,y,z),(x,y,z+dz),dx,dy,mat,mat)
 def tube(self,points,radius,mat=ROPE,sides=6):
  points=[Vector(p) for p in points];rings=[]
  for i,p in enumerate(points):
   direction=(points[min(i+1,len(points)-1)]-points[max(0,i-1)]).normalized();a=direction.cross(Vector((0,0,1)))
   if a.length<.01:a=Vector((1,0,0))
   a.normalize();b=direction.cross(a).normalized();rings.append([p+radius*(a*math.cos(j*math.tau/sides)+b*math.sin(j*math.tau/sides)) for j in range(sides)])
  for k in range(len(rings)-1):
   for j in range(sides):n=(j+1)%sides;self.face([rings[k][j],rings[k][n],rings[k+1][n],rings[k+1][j]],mat)
 def log(self,x,y,z,radius,length):
  n=12;profiles=[]
  for k in range(4):
   yy=y-length*.5+length*k/3;rr=radius*(1-.08*k/3)
   profiles.append([Vector((x+rr*(1+.035*math.sin(j*2.5))*math.cos(j*math.tau/n),yy,z+rr*(1+.035*math.sin(j*2.5))*math.sin(j*math.tau/n))) for j in range(n)])
  for k in range(3):
   for j in range(n):a=(j+1)%n;self.face([profiles[k][j],profiles[k][a],profiles[k+1][a],profiles[k+1][j]],BARK,[(j/n,k),(a/n,k),(a/n,k+1),(j/n,k+1)])
  for k in [0,3]:
   centre=Vector((x,y-length*.5+length*k/3,z))
   for j in range(n):
    a=(j+1)%n;ids=[j,a] if k==0 else [a,j]
    self.face([centre,profiles[k][ids[0]],profiles[k][ids[1]]],CUT,[(.5,.5),(.5+.48*math.cos(ids[0]*math.tau/n),.5+.48*math.sin(ids[0]*math.tau/n)),(.5+.48*math.cos(ids[1]*math.tau/n),.5+.48*math.sin(ids[1]*math.tau/n))])
 def make(self,name,root):
  me=bpy.data.meshes.new(name);me.from_pydata(self.v,[],self.f);me.update()
  for mat in materials:me.materials.append(mat)
  uv=me.uv_layers.new(name='Authored surfaces')
  for face,slot,coords in zip(me.polygons,self.m,self.uv):
   face.material_index=slot
   for loop,co in zip(face.loop_indices,coords):uv.data[loop].uv=co
  ob=bpy.data.objects.new(name,me);root.users_collection[0].objects.link(ob);ob.parent=root;return ob
def platform(b,x,y,z,w,d):
 for j in range(7):b.box(x,y-d*.5+(j+.5)*d/7,z,w,d/7-.035,.15,WOOD)
 for xx in [x-w*.43,x+w*.43]:
  for yy in [y-d*.43,y+d*.43]:
   b.prism((xx,yy,.1),(xx,yy,z+1.0),.22,mat=WOOD,cap=CUT)
 b.prism((x-w*.43,y+d*.43,z+.8),(x+w*.43,y+d*.43,z+.8),.18)
 b.prism((x-w*.43,y-d*.43,z+.8),(x-w*.43,y+d*.43,z+.8),.18)
 for xx,sign in [(x-w*.43,1),(x+w*.43,-1)]:b.prism((xx,y-d*.43,z-.95),(xx+sign*.9,y-d*.43,z-.1),.20)
def quarry(root,variant):
 b=Builder()
 # Clearly cut stepped stone mass, with open excavation at its front.
 for row,(yy,height,width) in enumerate([(3.8,4.8,2.35),(1.5,2.8,2.25)]):
  for k in range(5):
   xx=-4.8+k*2.4+.10*math.sin(k*2+variant)
   b.box(xx,yy,0,width,2.35,height+.18*math.sin(k*1.7+variant),STONE)
 for side in [-1,1]:
  for k in range(3):b.box(side*5.45,-3.7+k*2.05,0,1.8,2.02,1.5+.6*k+.10*math.sin(k+variant),STONE)
 for k in range(8):
  xx=-5.1+k*1.45
  if abs(xx)<1.3:continue
  b.box(xx,-5.35,0,1.40,1.3,.62+.12*math.sin(k*1.6),STONE)
 # Three deliberate broad access steps, no random stone pile in the pit.
 for j in range(4):b.box(-.6,-.6+j*.48,.0,2.4,.55,.32*(j+1),STONE)
 b.box(-4.3,3.2,4.5,2.8,2.2,.85,STONE);b.box(-1.5,4.1,4.7,2.5,1.7,.75,STONE)
 platform(b,3.55,-.15,3.65,3.9,3.5);platform(b,-3.9,-2.1,1.95,2.4,1.9)
 # Counter-braced timber derrick, winch and genuinely suspended stone.
 mast=(3.6,.35);base=3.8;top=9.5
 b.prism((3.6,.35,base),(3.6,.35,top+.35),.58,mat=WOOD,cap=CUT)
 b.prism((-2.4,.35,top),(5.25,.35,top),.52,.62,WOOD,CUT)
 b.prism((3.6,.35,base+.35),(-.35,.35,top-.35),.36)
 for sign in [-1,1]:b.prism((4.35,.35+sign*1.25,base),(3.6,.35,top-.55),.31)
 for zz in [base+.6,base+2.0,top-.45]:
  b.box(3.6,.35,zz,.66,.66,.17,IRON)
  for sx in [-1,1]:b.box(3.6+sx*.34,.01,zz+.035,.085,.07,.085,STEEL)
 # Winch drum axis along Y, with a visible winding spiral.
 b.prism((4.0,-.38,base+.7),(4.0,1.05,base+.7),.8,.8,WOOD,CUT)
 b.tube([(4+.46*math.cos(t),-.30+1.28*t/(math.tau*7),base+.7+.46*math.sin(t)) for t in np.linspace(0,math.tau*7,85)],.045)
 b.tube([(-1.85,.35,top+.05),(-1.85,.35,6.35)],.06)
 # Pulley axle and dark sheave.
 b.prism((-1.85,.12,top-.25),(-1.85,.58,top-.25),.38,.38,IRON)
 b.box(-1.85,.35,4.95,1.35,1.2,1.35,STONE)
 for xx in [-2.48,-1.22]:b.tube([(xx,-.25,4.97),(xx,.35,6.36),(-1.85,.35,6.8)],.045)
 # Rope follows upper beam toward the winch, never floats as a detached part.
 b.tube([(-1.85,.35,top+.35),(3.65,.35,top+.35),(4.0,.35,base+1.15)],.055)
 # Readable ladder resting between real extraction levels.
 for xx in [-2.8,-2.03]:b.prism((xx,-1.65,.12),(xx,.65,2.9),.12)
 for k in range(8):
  t=(k+.5)/8;b.prism((-2.83,-1.65+2.3*t,.12+2.78*t),(-2.0,-1.65+2.3*t,.12+2.78*t),.10)
 for x,y,size in [(-4.6,-4.0,.75),(-3.65,-4.05,.6),(2.65,-4.25,1.35),(3.85,-4.1,.55)]:b.box(x,y,0,size,size*.83,size*.8,STONE)
 # Crevice leaves are modelled accents, not another decorative forest.
 for cx,cy,cz in [(-5.6,3.9,4.8),(-3.4,4.2,5.4),(1.0,4.5,4.9),(5.3,1.8,3.3),(-5.8,-.2,2.2)]:
  for j in range(9):
   a=j*2.4;r=.12+.06*(j%4);x=cx+math.cos(a)*r;y=cy+math.sin(a)*r
   b.face([(x-.14,y,cz),(x,y-.10,cz+.27),(x+.14,y,cz),(x,y+.13,cz+.18)],LEAF)
 ob=b.make('Approved quarry carved mass derrick and platforms '+str(variant),root)
 # Independent extraction surface: importer conforms it to actual terrain.
 f=Builder();profile=[(-6.4,-5.9),(-2.5,-6.1),(2.7,-5.8),(6.4,-4.8),(6.6,.5),(5.9,4.9),(-5.6,5.2),(-6.6,.6)]
 for a,c in zip(profile,profile[1:]+profile[:1]):f.face([(0,0,.02),(a[0],a[1],.02),(c[0],c[1],.02)],SOIL,[(.5,.5),(0,0),(1,1)])
 f.make('Quarry cleared extraction floor '+str(variant),root)
 return ob
def wood(root,variant):
 b=Builder()
 for row in range(3):
  for col in range(3-row):b.log((col-(2-row)/2)*1.22,0,.62+row*1.05,.59,4.5+.15*math.sin(col+variant))
 for x in [-2.6,2.6]:
  for y in [-1.9,1.9]:
   b.prism((x,y,0),(x,y,4.05+.18*(x+y>0)),.43,.43,WOOD,CUT)
   b.tube([(x+.27*math.cos(t),y+.27*math.sin(t),3.45+.30*t/(math.tau*3)) for t in np.linspace(0,math.tau*3,31)],.06)
   b.prism((x*1.35,y,0),(x,y,3.35),.25)
 for y in [-1.9,1.9]:b.prism((-2.8,y,3.65),(2.8,y,3.65),.30,mat=WOOD,cap=CUT)
 for x in [-2.6,2.6]:b.prism((x,-2.15,3.7),(x,2.15,3.7),.25)
 # Woven canopy drapes across the same loaded rack, with painted pine motif.
 nx,ny=12,8
 def canopy(i,j):
  x=-2.8+5.6*i/nx;y=-2.15+4.3*j/ny
  z=3.90-.25*(1-(x/2.8)**2)+.035*math.sin(j*2.1)
  if j==0:z-=.12+.04*math.sin(i*2.4)
  return(x,y,z)
 for i in range(nx):
  for j in range(ny):b.face([canopy(i,j),canopy(i+1,j),canopy(i+1,j+1),canopy(i,j+1)],CLOTH,[(i/nx,j/ny),((i+1)/nx,j/ny),((i+1)/nx,(j+1)/ny),(i/nx,(j+1)/ny)])
 # Broad chopping stump with real cut top, flared roots and embedded axe.
 b.prism((-3.45,-3.0,0),(-3.45,-3.0,1.30),1.95,1.95,BARK,CUT)
 for j in range(5):
  a=j*math.tau/5;b.prism((-3.45+1.15*math.cos(a),-3+.95*math.sin(a),.05),(-3.45+.45*math.cos(a),-3+.45*math.sin(a),.8),.28,mat=BARK)
 b.prism((-4.6,-3.3,2.05),(-2.9,-2.6,1.58),.15,mat=WOOD,cap=CUT)
 b.box(-3.35,-2.8,1.18,.16,.67,.85,STEEL)
 # Smaller bound log bundle rests on a real wooden skid.
 for x in [3.55,4.75]:b.prism((x,-3.9,.10),(x,-1.0,.10),.20)
 for row in range(3):
  for col in range(3-row):b.log(4.15+(col-(2-row)/2)*.55,-2.5,.46+row*.48,.27,2.7)
 for yy in [-3.35,-1.65]:b.tube([(4.15+.92*math.cos(t),yy,.73+.76*math.sin(t)) for t in np.linspace(0,math.tau,25)],.055)
 # Split billets and chips are subordinate to the broad harvest silhouette.
 for j in range(5):b.prism((-3.8+j*.31,-4.1,.08),(-3.2+j*.17,-3.45,.65),.19,.29,CUT,CUT)
 return b.make('Approved bound timber rack canopy stump axe and side bundle '+str(variant),root)
authored=[]
for i in range(3):authored.append(quarry(bpy.data.objects['Resource_Stone_'+str(i)],i));authored.append(wood(bpy.data.objects['Resource_Wood_'+str(i)],i))
library=[ob for root in original_roots for ob in root.children]
for ob in scene.objects:ob.hide_render=True
for root in original_roots:root.hide_render=False
camera=scene.camera;camera.hide_render=False;camera.data.type='ORTHO'
light=bpy.data.objects.get('Resource impostor soft key');light.hide_render=False;light.data.energy=2400;light.data.size=9
light.location=(-8,-10,20);light.rotation_euler=(Vector((0,0,2))-light.location).to_track_quat('-Z','Y').to_euler()
scene.render.engine='BLENDER_EEVEE';scene.render.film_transparent=True;scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA'
scene.view_settings.view_transform='Standard';scene.view_settings.look='Medium High Contrast' if 'Medium High Contrast' in [a.name for a in []] else 'None'
def render(family,label,position,scale,size):
 for ob in library:ob.hide_render=ob.parent.name!='Resource_'+family+'_0'
 camera.location=position;camera.rotation_euler=(Vector((0,0,3 if family=='Stone' else 1.8))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.ortho_scale=scale
 scene.render.resolution_x=scene.render.resolution_y=size;scene.render.resolution_percentage=100;scene.render.filepath=str(out/label);bpy.ops.render.render(write_still=True)
for family in ['Stone','Wood']:
 render(family,'approved-'+family+'-front.png',(13,-21,20),21 if family=='Stone' else 15,1024)
 render(family,'approved-'+family+'-game-axis.png',(0,-22,21),21 if family=='Stone' else 15,768)
atlas=np.zeros((256,512,4),np.float32)
for column,family in enumerate(['Wood','Stone']):
 render(family,'approved-'+family+'-impostor.png',(10,-16,16),20 if family=='Stone' else 16,256)
 picture=bpy.data.images.load(scene.render.filepath,check_existing=False);atlas[:,column*256:(column+1)*256,:]=np.array(picture.pixels[:]).reshape(256,256,4)
im=bpy.data.images.new('Approved resource camera impostors',512,256,alpha=True);im.pixels.foreach_set(atlas.ravel());im.filepath_raw=str(out/'RegionOneResourceImpostors.png');im.file_format='PNG';im.save()
fresh=bpy.data.images.load(im.filepath_raw,check_existing=False);fresh.pack()
for ob in library:ob.hide_render=False
bpy.ops.object.select_all(action='DESELECT')
for ob in original_roots+library:ob.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RegionOneResources.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Resource_Library.blend'),compress=True)
report={'method':'Actual BlenderMCP execute_code; hand-authored approved geometry and UV-painted material maps','approved_reference_library_ids':['libfile_69f102c63f908191888794200775733e','libfile_ea187f8f47c48191b0f6e170aecdc4e6'],'roots':[o.name for o in original_roots],'root_count':6,'new_gameplay_targets':0,'input_working_copy_sha256':input_hash,'output_files':{n:hashlib.sha256((out/n).read_bytes()).hexdigest() for n in ['RegionOneResources.glb','RegionOneResourceImpostors.png','Eldoria_Resource_Library.blend']},'triangle_count':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in library),'material_count':len(materials),'preview_is_Unity':False,'art_approved':False}
(out/'approved-node-authoring.json').write_text(json.dumps(report,indent=2));print('APPROVED_REFERENCE_NODE_ASSETS_AUTHORED',json.dumps(report))
(out/'stop.request').write_text('Approved reference asset edit saved; close owned BlenderMCP session.')
