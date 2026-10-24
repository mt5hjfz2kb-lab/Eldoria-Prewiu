"""E: author a legible meadow/earth mosaic and water on preserved D geometry."""
import bpy,sys,pathlib,hashlib,json,math
import numpy as np
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);src=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='51152e69cb3a06ac6401351cf3ca6ca7beab82889ee2f45411740d510078ef3d'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene
S=2048;rgb=np.ones((S,S,4),np.float32);rgb[:,:,:3]=np.array((187,154,88))/255
paint=np.zeros((S,S),np.float32)
# Three distinct, irregular brush silhouettes, used to connect grass masses, not scatter noise.
shapes=[ [(-1,-.3),(-.7,-.8),(-.2,-.65),(.15,-.95),(.55,-.65),(.9,-.7),(1,-.1),(.7,.15),(.9,.6),(.35,.55),(.1,.85),(-.35,.6),(-.85,.7),(-.6,.1)], [(-.9,-.5),(-.4,-.8),(.1,-.55),(.65,-.7),(.85,-.2),(.55,.1),(1,.45),(.5,.8),(0,.6),(-.3,.95),(-.7,.4),(-1,.1)], [(-1,-.2),(-.8,-.7),(-.25,-.4),(.1,-.7),(.4,-.2),(.9,-.4),(1,.2),(.55,.45),(.2,.9),(-.15,.5),(-.55,.7),(-.65,.15)] ]
def brush(cx,cy,rx,ry,angle,shape,colour,opacity=1):
 ca=math.cos(angle);sa=math.sin(angle);pts=[(cx+x*rx*ca-y*ry*sa,cy+x*rx*sa+y*ry*ca) for x,y in shapes[shape%3]]
 x0=max(0,int((min(x for x,y in pts)+96)/192*S)-2);x1=min(S,int((max(x for x,y in pts)+96)/192*S)+3);y0=max(0,int((min(y for x,y in pts)+78)/156*S)-2);y1=min(S,int((max(y for x,y in pts)+78)/156*S)+3)
 if x0>=x1 or y0>=y1:return
 Y,X=np.mgrid[y0:y1,x0:x1].astype(np.float32);X=X/S*192-96;Y=Y/S*156-78;m=np.zeros(X.shape,bool);a=pts[-1]
 for b in pts:m^=((a[1]>Y)!=(b[1]>Y))&(X<(b[0]-a[0])*(Y-a[1])/(b[1]-a[1]+1e-12)+a[0]);a=b
 m=m.astype(np.float32)
 for _ in range(4):m=(m*4+np.roll(m,1,0)+np.roll(m,-1,0)+np.roll(m,1,1)+np.roll(m,-1,1))/8
 a=m[:,:,None]*opacity;rgb[y0:y1,x0:x1,:3]=rgb[y0:y1,x0:x1,:3]*(1-a)+np.array(colour)/255*a;paint[y0:y1,x0:x1]=np.maximum(paint[y0:y1,x0:x1],m)
# Designed broad overlapping meadow groups. Clear earth corridors are kept around the inherited paths.
rows=[(-67,[-84,-62,-39,-17,8,32,57,82]),(-47,[-92,-71,-48,-26,-2,23,47,72,94]),(-26,[-83,-60,-38,-12,12,36,60,83]),(-5,[-94,-72,-49,-25,0,25,49,73,95]),(17,[-85,-61,-37,-13,12,36,61,84]),(38,[-94,-72,-48,-24,0,25,49,74,96]),(59,[-84,-61,-37,-12,13,37,61,85]),(77,[-94,-72,-47,-23,1,26,51,76,97])]
for row,(y,xs) in enumerate(rows):
 for k,x in enumerate(xs):
  angle=[-.2,.35,-.45,.1][(row+k)%4];shape=(row+k)%3
  brush(x,y,17,14,angle,shape,(143,153,63),.96)
  brush(x-3,y+2,10,7,angle+.2,shape+1,(130,147,54),.6)
  brush(x+5,y-3,6,5,angle-.3,shape+2,(160,164,76),.5)
# Retain paths and geographic masks from the existing source, sampled to this texture resolution.
def mask(name):
 im=bpy.data.images[name];w,h=im.size;a=np.array(im.pixels[:],np.float32).reshape(h,w,4)[:,:,0];ix=np.minimum((np.arange(S)*w/S).astype(int),w-1);iy=np.minimum((np.arange(S)*h/S).astype(int),h-1);return a[iy[:,None],ix[None,:]]
for name,colour,strength in [('06 worn shoulders',(181,150,83),.75),('07 footpath',(207,174,111),.95),('04 eroded bank crown',(197,168,109),.85),('05 moist edge',(112,128,73),.55),('08 mineral outcrops',(157,153,116),.7)]:
 a=mask(name)[:,:,None]*strength;rgb[:,:,:3]=rgb[:,:,:3]*(1-a)+np.array(colour)/255*a
# Foliage floor and edge strokes grow from actual trees, rather than uniform dots over the whole patch.
for i,o in enumerate(sorted([o for o in scene.objects if o.name.startswith('Pilot_Pine')],key=lambda o:o.name)):
 x,y,z=o.location;brush(x,y,3.1*o.scale.x,2.6*o.scale.y,.23*(i%4),i,(103,128,56),.7)
 for j,(dx,dy) in enumerate([(-2.6,-.8),(2,-1.5),(.8,2.6)]):
  brush(x+dx,y+dy,.65,.4,.3*j,i+j,(111,137,55),.8);brush(x+dx-.12,y+dy+.12,.35,.22,.3*j,i+j,(182,186,94),.65)
ao=mask('Ribera baked contact AO');rgb[:,:,:3]*=(.74+.26*ao[:,:,None])
def save(name,a):
 h,w=a.shape[:2];v=np.ones((h,w,4),np.float32);v[:,:,:3]=a[:,:,:3] if a.ndim==3 else a[:,:,None]
 im=bpy.data.images.new(name,width=w,height=h);im.colorspace_settings.name='sRGB';im.pixels.foreach_set(v.ravel());im.filepath_raw=str(out/(name+'.png'));im.file_format='PNG';im.save();im.pack();return im
im=save('E_meadow_earth_painted',rgb);soil=bpy.data.materials['Ochre meadow'];bs=next(n for n in soil.node_tree.nodes if n.type=='BSDF_PRINCIPLED');n=soil.node_tree.nodes.new('ShaderNodeTexImage');n.image=im;soil.node_tree.nodes.active=n;soil.node_tree.links.new(n.outputs['Color'],bs.inputs['Base Color'])
n=soil.node_tree.nodes.new('ShaderNodeTexImage');n.image=save('E_editable_meadow_mask',paint);n.label='Editable connected meadow coverage'
# Paint luminous depth variation, with explicitly sRGB pigment values (avoid the dark C texture).
H,W=384,2048;v,u=np.mgrid[0:H,0:W].astype(np.float32);v/=H;u/=W;edge=np.abs(v-.5)*2;water=np.empty((H,W,3),np.float32)
for c,(deep,shallow) in enumerate(zip((37,133,171),(91,183,186))):water[:,:,c]=(deep+(shallow-deep)*edge**2)/255
for band,phase in [(.12,0),(.29,1.2),(.66,2),(.87,.4)]:
 centre=band+.018*np.sin(u*18+phase);a=np.clip(1-abs(v-centre)/.008,0,1)*(.25+.15*np.sin(u*10+phase)**2);water=water*(1-a[:,:,None])+np.array((145,211,207))/255*a[:,:,None]
river=bpy.data.objects['Pilot_River'];mat=river.data.materials[0];bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');n=mat.node_tree.nodes.new('ShaderNodeTexImage');n.image=save('E_river_depth_and_current',water);mat.node_tree.links.new(n.outputs['Color'],bs.inputs['Base Color'])
for name in ['Pilot_Deep','Pilot_RimSouth','Pilot_RimNorth']:bpy.data.objects[name].hide_render=True
# Exposed faces: interrupt the continuous bank band with offset ledges while preserving both boundaries.
for side in (-1,1):
 bank=bpy.data.objects['Pilot_Cutbank_'+str(side)];widths=[4,7,3,6,5,8,4,6];station=0;segment=0
 while station<193:
  end=min(193,station+widths[segment%len(widths)]);push=[.35,-.20,.5,-.15,.25,-.3][segment%6]
  for i in range(station,end):
   for level in (1,2,3):bank.data.vertices[i*5+level].co.y+=side*push*(1 if level==2 else .45)
  station=end;segment+=1
 bank.data.update()
scene['finishing_stage']='E connected meadow, exposed soil, luminous painted water and broken bank ledges';scene['source_D_sha256']=hashlib.sha256(src.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.type=='IMAGE' and im.size[0]>0:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'E','source_D_sha256':scene['source_D_sha256'],'geography':'existing D, bank interior vertices edited; terrain boundaries and tree positions preserved','materials':'connected meadow/earth mosaic, packed editable mask, luminous water depth paint','colour_space':'explicit sRGB pigment canvas, PNG imported as sRGB','art_approved':False},indent=2))
