"""L: paint connected worn earth at existing path shoulders and sculpted shore approaches."""
import bpy,sys,pathlib,hashlib,json
import numpy as np
from mathutils.geometry import interpolate_bezier
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);src=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='1089e26bcb66f874d41a3579a1027916d5b7619ccc2892f8ac9c6c00b4234566'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene;assert scene.get('finishing_stage','').startswith('K branch-level')
S=2048;Y,X=np.mgrid[0:S,0:S].astype(np.float32);X=X/S*192-96;Y=Y/S*156-78
coll=bpy.data.collections.new('13_Editable_connected_earth_paint');scene.collection.children.link(coll)
def mask(name,pts):
 c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';sp=c.splines.new('BEZIER');sp.bezier_points.add(len(pts)-1);sp.use_cyclic_u=True
 for p,(x,y) in zip(sp.bezier_points,pts):p.co=(x,y,0);p.handle_left_type='AUTO';p.handle_right_type='AUTO'
 o=bpy.data.objects.new(name,c);coll.objects.link(o);o.hide_render=True;o['purpose']='editable ground paint boundary in original world XY';bpy.context.view_layer.update();poly=[]
 for i,p in enumerate(sp.bezier_points):
  q=sp.bezier_points[(i+1)%len(pts)];poly.extend([(v.x,v.y) for v in interpolate_bezier(p.co,p.handle_right,q.handle_left,q.co,10)[:-1]])
 m=np.zeros((S,S),bool);a=poly[-1]
 for b in poly:m^=((a[1]>Y)!=(b[1]>Y))&(X<(b[0]-a[0])*(Y-a[1])/(b[1]-a[1]+1e-12)+a[0]);a=b
 m=m.astype(np.float32)
 for _ in range(12):m=(4*m+np.roll(m,1,0)+np.roll(m,-1,0)+np.roll(m,1,1)+np.roll(m,-1,1))/8
 return m
# Irregular connected wear attached to paths, not isolated meadow ovals or global camouflage.
shapes=[('Main road western shoulders',[(-50,25),(-44,24),(-39,20),(-34,21),(-30,17),(-22,17),(-17,13),(-8,14),(-5,11),(-10,8),(-16,10),(-22,8),(-26,12),(-32,12),(-38,17),(-44,18)]),('Main road eastern shoulders',[(4,12),(12,11),(17,8),(24,9),(29,6),(38,5),(43,1),(52,1),(59,-3),(55,-6),(48,-3),(42,-4),(36,0),(29,-1),(23,3),(17,2),(11,7),(5,7)]),('Southern road shoulders',[(-44,-39),(-36,-39),(-30,-44),(-23,-43),(-16,-47),(-8,-46),(1,-51),(9,-52),(15,-55),(11,-58),(4,-55),(-3,-55),(-9,-51),(-17,-52),(-24,-48),(-31,-49),(-37,-44),(-43,-43)])]
paint=np.zeros((S,S),np.float32)
for name,pts in shapes:paint=np.maximum(paint,mask(name,pts))
# Selected bank approaches use the actual edited mesh boundary, not another generated coastline.
for side,cx in [(-1,-30),(1,-5),(-1,22),(1,52)]:
 terrain=bpy.data.objects['Pilot_Terrain_'+str(side)];pts=[]
 for off,row in [(-5,0),(-3,2),(0,5),(3,3),(5,0),(2,0),(0,0),(-2,0)]:
  v=terrain.data.vertices[int(cx+96+off)*85+row].co;pts.append((v.x,v.y))
 paint=np.maximum(paint,mask('Worn earth at bank '+str(side)+' '+str(cx),pts))
base=bpy.data.images['I recovered exact retained surface'];rgba=np.array(base.pixels[:],np.float32).reshape(S,S,4)
# Reuse the reviewed surface's brush grain and value detail; no added random noise.
lum=rgba[:,:,:3].mean(2);local=lum.copy()
for _ in range(20):local=(4*local+np.roll(local,1,0)+np.roll(local,-1,0)+np.roll(local,1,1)+np.roll(local,-1,1))/8
grain=np.clip((lum+.01)/(local+.01),.88,1.12)
earth=np.array([.64,.53,.31],np.float32)[None,None,:]*grain[:,:,None]
alpha=paint[:,:,None]*.72;rgba[:,:,:3]=rgba[:,:,:3]*(1-alpha)+earth*alpha
# Maintain editable paint and mask as separate packed resources.
def save(name,data):
 im=bpy.data.images.new(name,width=S,height=S);im.colorspace_settings.name='sRGB';im.pixels.foreach_set(data.ravel());im.filepath_raw=str(out/(name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack();return im
image=save('L connected earth over retained I grain',rgba);m=np.ones((S,S,4),np.float32);m[:,:,:3]=paint[:,:,None];save('L editable connected earth mask',m)
mat=bpy.data.materials['Ochre meadow'];bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=image;mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
scene['finishing_stage']='L connected path shoulder and bank approach paint over K';scene['L_source_sha256']=hashlib.sha256(src.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.size[0]>0 and im.source in {'GENERATED','FILE'}:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'L','source_sha256':scene['L_source_sha256'],'paint_shapes':shapes,'unchanged':'all K geometry, tree placement, water, camera, gameplay','hypothesis':'connected eroded earth at existing paths and physical bank approaches can break grass uniformity without disconnected H ovals','art_approved':False},indent=2))
