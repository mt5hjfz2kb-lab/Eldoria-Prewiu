"""Stage B: open reviewed stage A, compose inherited pines, bake actual terrain AO."""
import bpy,sys,pathlib,json,hashlib,math
import numpy as np
from mathutils.geometry import interpolate_bezier
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);source=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(source.read_bytes()).hexdigest()=='931f4e742095556c8edee0887ede872de3f83ed784f4128fbef66cb521c41ea4'
bpy.ops.wm.open_mainfile(filepath=str(source));scene=bpy.context.scene
assert scene.get('finishing_stage')=='A surfaces and sculpted banks'
terrain=[bpy.data.objects['Pilot_Terrain_'+str(i)] for i in (-1,1)]
def ground(x,y):
 for o in terrain:
  hit,co,n,index=o.ray_cast(Vector((x,y,100)),Vector((0,0,-1)))
  if hit:return co.z
 raise ValueError('Placement outside existing terrain')
# Relocate, do not manufacture, a small selection from peripheral groups into river clearings.
trees=sorted([o for o in scene.objects if o.name.startswith('Pilot_Pine')],key=lambda o:abs(o.location.x)+abs(o.location.y),reverse=True)
clusters=[(-39,-13),(-8,-4),(17,-17),(42,-21),(-8,-38),(49,-48)]
offsets=[(-2.2,.5,.85),(1.3,1.7,1.05),(.2,-2.1,.70)]
moved=[]
for index,(cx,cy) in enumerate(clusters):
 for j,(dx,dy,scale) in enumerate(offsets):
  o=trees[index*3+j];old=tuple(o.location);x=cx+dx;y=cy+dy;o.location=(x,y,ground(x,y)-.08);o.scale=(scale,scale,scale);moved.append({'name':o.name,'from':old,'to':tuple(o.location)})
# Ground existing rocks in designed satellite groups; varied silhouettes, no new gameplay node.
rocks=sorted([o for o in scene.objects if o.name.startswith('Pilot_Rock')],key=lambda o:o.name)
for i,(cx,cy) in enumerate(clusters):
 for j,(dx,dy) in enumerate([(-3.5,-2.5),(3,-1.5)]):
  o=rocks[i*2+j];x=cx+dx;y=cy+dy;o.location=(x,y,ground(x,y)+.25);o.scale=(.7+j*.35,.65+j*.2,.8+j*.25)
soil=bpy.data.materials['Ochre meadow'];nodes=soil.node_tree.nodes;links=soil.node_tree.links
base=bpy.data.images['Ribera_surface_painted'];S=base.size[0]
# Stage-A visual rejection: hard polygon stains. Replace representation with curved paint shapes.
# Keep all source masks for recovery, add true editable Bezier outlines and softer connected washes.
Y,X=np.mgrid[0:S,0:S].astype(np.float32);X=X/S*192-96;Y=Y/S*156-78
rgb=np.ones((S,S,4),np.float32);rgb[:,:,:3]=(.49,.53,.205)
def soften(a,n):
 for _ in range(n):a=(a*4+np.roll(a,1,0)+np.roll(a,-1,0)+np.roll(a,1,1)+np.roll(a,-1,1))/8
 return a
paint_collection=bpy.data.collections.new('06_Painterly_surface_outlines');scene.collection.children.link(paint_collection)
def curved_mask(name,points):
 curve=bpy.data.curves.new(name,'CURVE');curve.dimensions='3D';spl=curve.splines.new('BEZIER');spl.bezier_points.add(len(points)-1);spl.use_cyclic_u=True
 for p,(x,y) in zip(spl.bezier_points,points):p.co=(x,y,12);p.handle_left_type='AUTO';p.handle_right_type='AUTO'
 obj=bpy.data.objects.new(name,curve);paint_collection.objects.link(obj);obj.hide_render=True;bpy.context.view_layer.update()
 pts=[];bp=spl.bezier_points
 for i,p in enumerate(bp):
  q=bp[(i+1)%len(bp)];pts.extend([(v.x,v.y) for v in interpolate_bezier(p.co,p.handle_right,q.handle_left,q.co,12)[:-1]])
 inside=np.zeros((S,S),bool);a=pts[-1]
 for b in pts:
  inside^=((a[1]>Y)!=(b[1]>Y))&(X<(b[0]-a[0])*(Y-a[1])/(b[1]-a[1]+1e-12)+a[0]);a=b
 return soften(inside.astype(np.float32),45)
shapes=[ [(-72,25),(-64,21),(-59,24),(-51,20),(-47,15),(-56,13),(-62,16),(-69,14),(-76,20)], [(-29,0),(-22,-3),(-17,0),(-11,-3),(-8,-8),(-17,-9),(-24,-7),(-30,-9),(-35,-4)], [(15,19),(23,17),(29,20),(33,16),(39,19),(40,25),(34,27),(30,24),(24,28),(19,25)], [(-51,-36),(-44,-34),(-39,-38),(-31,-38),(-27,-44),(-34,-47),(-42,-44),(-48,-46),(-54,-42)], [(-21,-49),(-13,-45),(-9,-49),(-1,-46),(7,-49),(11,-55),(5,-58),(-3,-55),(-11,-59),(-20,-55)], [(35,-57),(43,-54),(49,-56),(55,-53),(64,-57),(65,-63),(57,-65),(50,-61),(43,-65),(35,-62)], [(-6,41),(2,37),(6,42),(15,43),(18,50),(12,55),(4,51),(-3,55),(-10,48)] ]
wash=np.zeros((S,S),np.float32)
for i,pts in enumerate(shapes):wash=np.maximum(wash,curved_mask('Paint meadow contour '+str(i),pts))
rgb[:,:,:3]=rgb[:,:,:3]*(1-wash[:,:,None]*.55)+np.array((.62,.59,.27))*wash[:,:,None]*.55
# Reuse meaningful category masks from A; discard its hard block-shaped meadow and earth fields.
for name,colour,strength,blur in [('03 pine litter',(.28,.36,.12),.8,24),('04 eroded bank crown',(.68,.55,.32),.85,20),('05 moist edge',(.31,.35,.18),.6,12),('06 worn shoulders',(.62,.52,.29),.8,24),('07 footpath',(.75,.61,.38),.95,14),('08 mineral outcrops',(.50,.48,.35),.7,30)]:
 im=bpy.data.images[name];mask=np.array(im.pixels[:],np.float32).reshape(S,S,4)[:,:,0];mask=soften(mask,blur)*strength
 rgb[:,:,:3]=rgb[:,:,:3]*(1-mask[:,:,None])+np.array(colour)*mask[:,:,None]
# Painted litter under the revised clusters follows their footprint, not circles scattered over the map.
Y,X=np.mgrid[0:S,0:S].astype(np.float32);X=X/S*192-96;Y=Y/S*156-78
mask=np.zeros((S,S),np.float32)
for cx,cy in clusters:
 for dx,dy,scale in offsets:
  d=((X-cx-dx)/(2.6*scale))**2+((Y-cy-dy)/(2.3*scale))**2;mask=np.maximum(mask,np.clip((1-d)*2,0,1)*.55)
rgb[:,:,:3]=rgb[:,:,:3]*(1-mask[:,:,None])+np.array((.27,.34,.12))*mask[:,:,None]
def save_image(im,name):
 im.filepath_raw=str(out/name);im.file_format='PNG';im.save();im.pack()
litter=bpy.data.images.new('09 revised cluster litter',width=S,height=S);lr=np.ones((S,S,4),np.float32);lr[:,:,:3]=mask[:,:,None];litter.pixels.foreach_set(lr.ravel());save_image(litter,'09_revised_cluster_litter.png');ln=nodes.new('ShaderNodeTexImage');ln.image=litter;ln.label='Paintable revised understory mask'
# True Cycles occlusion of existing geometry, independent from sunlight. No fake oval shadows.
ao=bpy.data.images.new('Ribera baked contact AO',width=768,height=768);ao.colorspace_settings.name='Non-Color'
target=nodes.new('ShaderNodeTexImage');target.image=ao;nodes.active=target
occ=nodes.new('ShaderNodeAmbientOcclusion');occ.inputs['Distance'].default_value=5.;occ.samples=16
emission=nodes.new('ShaderNodeEmission');links.new(occ.outputs['Color'],emission.inputs['Color'])
output=next(n for n in nodes if n.type=='OUTPUT_MATERIAL');bs=next(n for n in nodes if n.type=='BSDF_PRINCIPLED');links.new(emission.outputs[0],output.inputs['Surface'])
bpy.ops.object.select_all(action='DESELECT')
for o in terrain:o.select_set(True)
bpy.context.view_layer.objects.active=terrain[0];scene.render.engine='CYCLES';scene.cycles.samples=16
bpy.ops.object.bake(type='EMIT',margin=4,use_clear=True)
save_image(ao,'Ribera_contact_AO.png');links.new(bs.outputs['BSDF'],output.inputs['Surface'])
small=np.array(ao.pixels[:],np.float32).reshape(768,768,4)[:,:,:3];big=np.repeat(np.repeat(small,2,axis=0),2,axis=1)
rgb[:,:,:3]*=.62+.38*np.clip(big,0,1)
composite=bpy.data.images.new('Ribera painted surface plus baked AO',width=S,height=S);composite.pixels.foreach_set(rgb.ravel());save_image(composite,'Ribera_surface_AO.png')
tex=nodes.new('ShaderNodeTexImage');tex.image=composite;tex.label='Unity export: paint + measured contact AO';nodes.active=tex;links.new(tex.outputs['Color'],bs.inputs['Base Color'])
scene['finishing_stage']='B curved paint, contact bake and environmental composition';scene['stage_A_source_sha256']=hashlib.sha256(source.read_bytes()).hexdigest()
# Retain every packed map as a separate exported editable PNG too.
for im in bpy.data.images:
 if im.type=='IMAGE' and im.size[0]>0 and im.name!='Render Result':
  save_image(im,im.name.replace(' ','_')+'.png')
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH':o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'))
scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'B','source_sha256':scene['stage_A_source_sha256'],'moved_existing_trees':moved,'moved_existing_rocks':12,'bake':'Cycles EMIT of AO node, distance5, 768px, samples16','art_approved':False},indent=2))
