"""Stage C: painted ground-cover brush atlas on the same UV terrain, no new world."""
import bpy,sys,pathlib,json,hashlib,math
import numpy as np
from mathutils.geometry import interpolate_bezier
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);source=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(source.read_bytes()).hexdigest()=='0592845ca56cc9aa05131227defdb7fa64315b3c19bdfe380e49131bf045f355'
bpy.ops.wm.open_mainfile(filepath=str(source));scene=bpy.context.scene
base=bpy.data.images['Ribera painted surface plus baked AO'];S=base.size[0]
rgb=np.array(base.pixels[:],np.float32).reshape(S,S,4);rgb[:,:,:3]=np.power(np.maximum(rgb[:,:,:3],0),.84)*np.array((1.02,.98,.89))
coverage=np.zeros((S,S),np.float32);earth=np.zeros_like(coverage)
brushes={
 'earth':[(-1,-.2),(-.75,-.65),(-.25,-.55),(.1,-.8),(.5,-.5),(.9,-.55),(1,.05),(.65,.4),(.3,.35),(0,.7),(-.4,.4),(-.8,.5)],
 'grass':[(-.8,0),(-.95,.4),(-.5,.18),(-.45,.8),(-.17,.25),(.08,1),(.22,.24),(.6,.7),(.5,.12),(.9,.3),(.7,-.1),(.1,-.22),(-.4,-.12)],
 'stone':[(-.8,-.25),(-.7,.3),(-.2,.7),(.5,.5),(.8,-.05),(.35,-.4)],
}
operations={'earth_strokes':0,'grass_strokes':0,'stone_strokes':0}
def stroke(kind,x,y,sx,sy,angle,colour,opacity=1,target=None):
 pts=brushes[kind];ca=math.cos(angle);sa=math.sin(angle);pts=[(x+px*sx*ca-py*sy*sa,y+px*sx*sa+py*sy*ca) for px,py in pts]
 xmin=min(p[0] for p in pts);xmax=max(p[0] for p in pts);ymin=min(p[1] for p in pts);ymax=max(p[1] for p in pts)
 x0=max(0,int((xmin+96)/192*S)-1);x1=min(S,int((xmax+96)/192*S)+2);y0=max(0,int((ymin+78)/156*S)-1);y1=min(S,int((ymax+78)/156*S)+2)
 if x0>=x1 or y0>=y1:return
 Y,X=np.mgrid[y0:y1,x0:x1].astype(np.float32);X=X/S*192-96;Y=Y/S*156-78;mask=np.zeros(X.shape,bool);a=pts[-1]
 for b in pts:
  mask^=((a[1]>Y)!=(b[1]>Y))&(X<(b[0]-a[0])*(Y-a[1])/(b[1]-a[1]+1e-12)+a[0]);a=b
 alpha=mask.astype(np.float32)*opacity
 # A one-pixel brush fringe retains a painted edge without blurred broad stains.
 alpha=(alpha*4+np.roll(alpha,1,0)+np.roll(alpha,-1,0)+np.roll(alpha,1,1)+np.roll(alpha,-1,1))/8
 rgb[y0:y1,x0:x1,:3]=rgb[y0:y1,x0:x1,:3]*(1-alpha[:,:,None])+np.array(colour)*alpha[:,:,None]
 if target is not None:target[y0:y1,x0:x1]=np.maximum(target[y0:y1,x0:x1],alpha)
 operations[kind+'_strokes']+=1
# Exposed soil grows from actual path shoulders, alternating sides and keeping paths readable.
for o in scene.objects:
 if o.type=='CURVE' and o.name.startswith('Editable path'):
  bp=o.data.splines[0].bezier_points
  for seg,(a,b) in enumerate(zip(bp,bp[1:])):
   pts=interpolate_bezier(a.co,a.handle_right,b.handle_left,b.co,13)
   for k in (2,6,10):
    p=pts[k];q=pts[k+1];ang=math.atan2(q.y-p.y,q.x-p.x);side=1 if (k+seg)%3 else -1;x=p.x-math.sin(ang)*side*1.65;y=p.y+math.cos(ang)*side*1.65
    stroke('earth',x,y,2.9,1.35,ang,(.69,.56,.32),.65,earth)
    for dx,dy in [(-1.4,.9),(.2,1.4),(1.7,1.0)]:
     xx=x+dx*math.cos(ang)-dy*math.sin(ang)*side;yy=y+dx*math.sin(ang)+dy*math.cos(ang)*side
     stroke('grass',xx,yy,.48,.65,ang,(.32,.43,.12),.75,coverage)
     stroke('grass',xx+.04,yy+.1,.29,.38,ang,(.60,.64,.24),.65,coverage)
# Small painterly understory follows actual tree groups and rocks; no uniform scatter field.
for i,o in enumerate(sorted([o for o in scene.objects if o.name.startswith('Pilot_Pine')],key=lambda o:o.name)):
 x,y,z=o.location
 for j,(dx,dy,sz) in enumerate([(-2.1,-1.1,.7),(1.7,-1.6,.55),(.7,2.3,.65)]):
  xx=x+dx*o.scale.x;yy=y+dy*o.scale.y;ang=(i%5-2)*.3
  if j==0:stroke('earth',xx,yy,1.1,.6,ang,(.60,.48,.26),.50,earth)
  stroke('grass',xx,yy,sz,sz*.9,ang,(.25,.37,.12),.88,coverage)
  stroke('grass',xx+.07,yy+.12,sz*.65,sz*.55,ang,(.52,.58,.19),.80,coverage)
 for j,(dx,dy) in enumerate([(-1.8,-2.2),(2.4,-.8)]):
  stroke('stone',x+dx,y+dy,.22,.22,0,(.37,.40,.29),.95)
  stroke('stone',x+dx-.04,y+dy+.05,.14,.13,0,(.76,.70,.48),.9)
# River edge accents follow inherited sculpted vertices and respect the bank side.
for side in (-1,1):
 o=bpy.data.objects['Pilot_Terrain_'+str(side)]
 for index in range(8,185,5):
  p=o.data.vertices[index*85].co
  stroke('earth',p.x,p.y+side*.8,1.1,.5,.2*side,(.74,.61,.37),.55,earth)
  if index%3:
   stroke('grass',p.x+.3,p.y+side*1.4,.45,.65,.2*side,(.36,.46,.14),.9,coverage)
# Retain true contact occlusion under added pigment, with bounded strength.
ao=bpy.data.images['Ribera baked contact AO'];small=np.array(ao.pixels[:],np.float32).reshape(768,768,4)[:,:,:3];big=np.repeat(np.repeat(small,2,0),2,1)
changed=np.maximum(coverage,earth)[:,:,None];rgb[:,:,:3]*=1-changed*(1-big)*.3
soil=bpy.data.materials['Ochre meadow'];nodes=soil.node_tree.nodes;bs=next(n for n in nodes if n.type=='BSDF_PRINCIPLED')
def save(name,arr):
 h,w=arr.shape[:2];data=np.ones((h,w,4),np.float32);data[:,:,:3]=arr[:,:,:3] if arr.ndim==3 else arr[:,:,None]
 im=bpy.data.images.new(name,width=w,height=h);im.pixels.foreach_set(data.ravel());im.filepath_raw=str(out/(name+'.png'));im.file_format='PNG';im.save();im.pack();return im
for name,arr in [('Painted_groundcover_mask',coverage),('Painted_soil_edges_mask',earth)]:
 im=save(name,arr);n=nodes.new('ShaderNodeTexImage');n.image=im;n.label='Editable brush layer '+name
im=save('Ribera_pictorial_ground',rgb);n=nodes.new('ShaderNodeTexImage');n.image=im;nodes.active=n;soil.node_tree.links.new(n.outputs['Color'],bs.inputs['Base Color'])
# Original sparse triangular grass retained in source history, excluded from this presentation.
grass=bpy.data.objects.get('Pilot_Grass')
if grass:grass.hide_render=True;grass['replaced_by']='Painted_groundcover_mask; same terrain UV'
# Water receives an editable painted depth wash and restrained current highlights.
H,W=256,1536;v,u=np.mgrid[0:H,0:W].astype(np.float32);v/=H;u/=W
edge=np.abs(v-.5)*2;water=np.empty((H,W,3),np.float32)
for c,(deep,shallow) in enumerate(zip((.025,.22,.31),(.07,.43,.48))):water[:,:,c]=deep+(shallow-deep)*edge**1.5
for band in (.17,.38,.70,.88):
 centre=band+.018*np.sin(u*15+band*7);a=np.clip(1-np.abs(v-centre)/.009,0,1)*.25
 water=water*(1-a[:,:,None])+np.array((.18,.56,.61))*a[:,:,None]
wim=save('Ribera_painted_current',water);river=bpy.data.objects['Pilot_River'];uv=river.data.uv_layers.new(name='PaintedFlowUV')
for loop in river.data.loops:
 k=loop.vertex_index;uv.data[loop.index].uv=(k//2/192,k%2)
mat=river.data.materials[0];bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');n=mat.node_tree.nodes.new('ShaderNodeTexImage');n.image=wim;mat.node_tree.links.new(n.outputs['Color'],bs.inputs['Base Color'])
# Keep prior channel strips in the editable source but let the painted surface carry depth.
for name in ['Pilot_Deep','Pilot_RimSouth','Pilot_RimNorth']:
 o=bpy.data.objects.get(name)
 if o:o.hide_render=True;o['replaced_by']='Ribera_painted_current'
scene['finishing_stage']='C pictorial UV groundcover and water over unchanged B geography'
scene['stage_B_source_sha256']=hashlib.sha256(source.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.type=='IMAGE' and im.size[0]>0:
  im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'))
scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'C','source_sha256':scene['stage_B_source_sha256'],'method':'painted UV groundcover and flowing-water atlas, same geometry','brush_operations':operations,'original_hidden_meshes_preserved':True,'art_approved':False},indent=2))
