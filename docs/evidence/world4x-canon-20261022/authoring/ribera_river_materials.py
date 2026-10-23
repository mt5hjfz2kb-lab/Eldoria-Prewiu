"""J: local river look development on owner-retained I, with editable pigment masks."""
import bpy,sys,pathlib,hashlib,json,math
import numpy as np
from mathutils.geometry import interpolate_bezier
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);src=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='878f14116db4159b0b354dbff8b97e47a9d92180d345eb7fe4e728b530599642'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene
assert scene.get('finishing_stage','').startswith('I retained G')
guides=bpy.data.collections.new('10_River_pigment_guides');scene.collection.children.link(guides)
H,W=384,2048;Y,X=np.mgrid[0:H,0:W].astype(np.float32);X/=W;Y/=H
# Author UV shapes as editable Bezier curves; the meshes and UVs are the inherited river.
def shape(name,points):
 c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';sp=c.splines.new('BEZIER');sp.bezier_points.add(len(points)-1);sp.use_cyclic_u=True
 for p,(x,y) in zip(sp.bezier_points,points):p.co=(x,y,0);p.handle_left_type='AUTO';p.handle_right_type='AUTO'
 o=bpy.data.objects.new(name,c);guides.objects.link(o);o.hide_render=True;o['coordinate_space']='UV; u follows inherited river, v crosses surface';bpy.context.view_layer.update()
 pts=[]
 for i,p in enumerate(sp.bezier_points):
  q=sp.bezier_points[(i+1)%len(points)];pts.extend([(v.x,v.y) for v in interpolate_bezier(p.co,p.handle_right,q.handle_left,q.co,12)[:-1]])
 m=np.zeros((H,W),bool);a=pts[-1]
 for b in pts:m^=((a[1]>Y)!=(b[1]>Y))&(X<(b[0]-a[0])*(Y-a[1])/(b[1]-a[1]+1e-12)+a[0]);a=b
 m=m.astype(np.float32)
 for _ in range(5):m=(4*m+np.roll(m,1,0)+np.roll(m,-1,0)+np.roll(m,1,1)+np.roll(m,-1,1))/8
 return m
def paint(canvas,mask,colour,opacity=1):
 a=mask[:,:,None]*opacity;canvas[:]=canvas*(1-a)+np.array(colour)/255*a

def save(name,arr):
 h,w=arr.shape[:2];rgba=np.ones((h,w,4),np.float32);rgba[:,:,:3]=arr[:,:,:3] if arr.ndim==3 else arr[:,:,None]
 im=bpy.data.images.new(name,width=w,height=h);im.colorspace_settings.name='sRGB';im.pixels.foreach_set(rgba.ravel());im.filepath_raw=str(out/(name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack();return im

def material_image(mat,im):
 bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');n=mat.node_tree.nodes.new('ShaderNodeTexImage');n.image=im;mat.node_tree.links.new(n.outputs['Color'],bs.inputs['Base Color']);mat.node_tree.nodes.active=n
# Independent marginal geology: oblique clay/rock faces, discontinuous moist foot and turf roots.
segments={-1:[(.02,.09,.68),(.13,.06,.45),(.23,.10,.72),(.38,.08,.38),(.49,.12,.64),(.66,.07,.51),(.76,.10,.75),(.9,.08,.46)],1:[(.05,.10,.42),(.20,.08,.73),(.34,.11,.60),(.48,.07,.42),(.59,.10,.78),(.73,.11,.46),(.89,.12,.66)]}
for side in [-1,1]:
 canvas=np.zeros((H,W,3),np.float32);canvas[:]=np.array([183,163,120])/255
 coverage=np.zeros((H,W),np.float32)
 for k,(cx,rx,cy) in enumerate(segments[side]):
  mask=shape('Bank %s mineral face %02d'%(side,k),[(cx-rx,.10),(cx-rx*.6,cy),(cx-rx*.25,cy+.19),(cx+rx*.6,cy+.11),(cx+rx,.22),(cx+rx*.4,.01)])
  colour=[(151,155,139),(197,178,135),(162,151,125),(179,163,137)][(k+(side==1))%4];paint(canvas,mask,colour,.86);coverage=np.maximum(coverage,mask)
  # Short fractures terminate inside selected exposed faces; never a single horizontal stripe.
  joint=shape('Bank %s joint %02d'%(side,k),[(cx-rx*.25,.20),(cx-rx*.10,.48),(cx+rx*.08,cy+.13),(cx+rx*.14,cy+.1),(cx-rx*.03,.45),(cx-rx*.18,.18)])
  paint(canvas,joint,(102,111,91),.52)
 for k,(cx,rx) in enumerate([(.09,.055),(.29,.08),(.54,.06),(.81,.075)] if side==-1 else [(.17,.07),(.42,.055),(.67,.085),(.94,.05)]):
  mask=shape('Bank %s turf edge %02d'%(side,k),[(cx-rx,1.05),(cx-rx*.5,.88),(cx,.80),(cx+rx*.2,.91),(cx+rx,.89),(cx+rx,1.1)])
  paint(canvas,mask,(114,139,70),.85)
 mat=bpy.data.objects['Pilot_Cutbank_'+str(side)].data.materials[0].copy();mat.name='J marginal earth rock '+str(side);bpy.data.objects['Pilot_Cutbank_'+str(side)].data.materials[0]=mat;material_image(mat,save('J_bank_'+str(side),canvas));save('J_bank_mineral_mask_'+str(side),coverage)
# Pools and glides are localized shapes, avoiding coast-to-coast colour stripes.
water=np.zeros((H,W,3),np.float32);water[:]=np.array([44,132,174])/255
poolmask=np.zeros((H,W),np.float32)
for k,(cx,rx,cy,ry) in enumerate([(.10,.12,.55,.28),(.35,.15,.36,.22),(.60,.13,.64,.26),(.85,.15,.42,.24)]):
 m=shape('River pool '+str(k),[(cx-rx,cy),(cx-rx*.5,cy-ry),(cx+rx*.5,cy-ry*.7),(cx+rx,cy+.05),(cx+rx*.25,cy+ry),(cx-rx*.6,cy+ry*.7)])
 paint(water,m,(30,103,155),.64);poolmask=np.maximum(poolmask,m)
shallowmask=np.zeros((H,W),np.float32)
for k,(cx,rx,side) in enumerate([(.07,.08,0),(.22,.09,1),(.42,.11,0),(.60,.07,1),(.73,.08,0),(.91,.10,1)]):
 pts=[(cx-rx,-.03),(cx-rx*.6,.08),(cx,.22),(cx+rx*.55,.13),(cx+rx,-.02)]
 if side:pts=[(x,1-y) for x,y in pts]
 m=shape('River shallow gravel '+str(k),pts);paint(water,m,(91,172,180),.85);shallowmask=np.maximum(shallowmask,m)
# Broken flow strokes: different lengths and width, not continuous outlines of the river mesh.
for k,(cx,rx,cy) in enumerate([(.08,.035,.38),(.23,.05,.55),(.40,.055,.67),(.57,.045,.32),(.76,.065,.58),(.93,.04,.70)]):
 m=shape('River flow brush '+str(k),[(cx-rx,cy),(cx,cy+.035),(cx+rx,cy+.015),(cx+rx*.2,cy+.012),(cx-rx*.6,cy-.008)])
 paint(water,m,(137,195,207),.55)
material_image(bpy.data.objects['Pilot_River'].data.materials[0],save('J_river_pools_and_shallows',water));save('J_pool_mask',poolmask);save('J_shallow_mask',shallowmask)
# Re-author existing rock geometry once, then place four authored shore accumulations.
coll=bpy.data.collections.new('11_Shore_rock_studies');scene.collection.children.link(coll)
source=next(o for o in scene.objects if o.name.startswith('Pilot_Rock'))
mat=bpy.data.materials.new('J weathered shore rock');mat.use_nodes=True;mat.node_tree.nodes.clear();bs=mat.node_tree.nodes.new('ShaderNodeBsdfPrincipled');bs.inputs['Base Color'].default_value=(.40,.43,.34,1);output=mat.node_tree.nodes.new('ShaderNodeOutputMaterial');mat.node_tree.links.new(bs.outputs[0],output.inputs[0])
master=bpy.data.objects.new('Shore rock editable master',source.data.copy());coll.objects.link(master);master.data.materials.clear();master.data.materials.append(mat)
for v in master.data.vertices:
 v.co.x*=1.25;v.co.z*=.6
 if v.co.z>.3:v.co.z=.3+(v.co.z-.3)*.3
bpy.ops.object.select_all(action='DESELECT');master.select_set(True);bpy.context.view_layer.objects.active=master
bevel=master.modifiers.new('Eroded stone corners','BEVEL');bevel.width=.15;bevel.segments=1;bpy.ops.object.modifier_apply(modifier=bevel.name);master.hide_render=True;master.hide_viewport=True
placements=[]
for k,(side,x,layout) in enumerate([(-1,-30,[(-2,.2,1.8),(0,.6,2.2),(2.1,-.1,1.2),(3,.9,.7)]),(1,-5,[(-1.8,.4,1.3),(0,.1,2.0),(1.7,.65,.9)]),(-1,22,[(-2.3,.2,1.0),(0,.5,2.2),(2,.1,1.5),(3,1,.7)]),(1,52,[(-1.8,.3,1.6),(0,.8,2.0),(2.1,.1,1.1)])]):
 bank=bpy.data.objects['Pilot_Cutbank_'+str(side)]
 for j,(dx,push,s) in enumerate(layout):
  xx=x+dx;i=max(0,min(192,int(round(xx+96))));v=bank.data.vertices[i*5+3].co
  o=bpy.data.objects.new('J shore stone %d_%d'%(k,j),master.data);coll.objects.link(o);o.location=(xx,v.y-side*push,.25+s*.12);o.scale=(s,s*(.7 if j%2 else 1.0),s);o.rotation_euler[2]=[.3,1.1,-.4,2.2][j%4];placements.append({'name':o.name,'position':list(o.location),'scale':list(o.scale)})
scene['finishing_stage']='J authored pools shallow gravel and marginal rock-clay surfaces on retained I';scene['J_source_I_sha256']=hashlib.sha256(src.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.type=='IMAGE' and im.size[0]>0:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
visible=[o for o in scene.objects if o.type=='MESH' and not o.hide_render]
for o in visible:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'J','source_I_sha256':scene['J_source_I_sha256'],'retained':'all I geography terrain trees camera paths','changes':'separate bank paint, localized pools/shallow masks, tapered flow marks, bevelled inherited shore rocks','shore_stones':placements,'art_approved':False},indent=2))
manifest={'source_units':'metres','Blender_up':'Z','Unity_up':'Y','authoritative_geography':'editable I meshes; river and terrain boundary untouched in J','origin':[0,0,0],'meshes':[{'name':o.name,'vertices':len(o.data.vertices),'polygons':len(o.data.polygons),'position':list(o.location),'scale':list(o.scale),'materials':[m.name for m in o.data.materials if m],'uv_maps':[u.name for u in o.data.uv_layers]} for o in visible],'textures':[{'name':im.name,'width':im.size[0],'height':im.size[1],'packed':bool(im.packed_file)} for im in bpy.data.images if im.type=='IMAGE'],'gameplay_colliders':0,'Unity_hybrid':'existing mesh-derived atlas and 36–50 ortho fade; no new runtime geography'}
(out/'RiberaAssetManifest.json').write_text(json.dumps(manifest,indent=2))
