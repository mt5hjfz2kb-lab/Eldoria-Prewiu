"""I: consolidate retained materials/forest and sculpt selected bank approaches."""
import bpy,sys,pathlib,hashlib,json,math
import numpy as np
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);src=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='17d57738614bbf149a0df4e43e5df57ddaad75c011aba998e9c6b4d0d40ea1b8'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene
assert scene.get('finishing_stage','').startswith('H directed meadow')
# Preserve H forest organization, reject H isolated painted meadow islands.
soil=bpy.data.materials['Ochre meadow'];nodes=soil.node_tree.nodes;links=soil.node_tree.links;bs=next(n for n in nodes if n.type=='BSDF_PRINCIPLED');output=next(n for n in nodes if n.type=='OUTPUT_MATERIAL')
base=bpy.data.images['G stylized grass and earth finish'];S=base.size[0];rgba=np.array(base.pixels[:],np.float32).reshape(S,S,4)
from mathutils import Vector
def ground(x,y):
 for side in [-1,1]:
  hit,co,n,index=bpy.data.objects['Pilot_Terrain_'+str(side)].ray_cast(Vector((x,y,80)),Vector((0,0,-1)))
  if hit:return co.z
 return None
roots=[(o,ground(o.location.x,o.location.y)) for o in scene.objects if o.name.startswith('Pilot_Pine')]
under=bpy.data.objects['Directed understory mesh'];heights=[ground(v.co.x,v.co.y) for v in under.data.vertices]
# Native mesh sculpt on existing terrain/cutbank boundary: three authored approaches, no new river.
edits=[]
for side,cx,halfwidth,depth in [(-1,-24,9,9),(1,7,11,11),(-1,39,7,8)]:
 terrain=bpy.data.objects['Pilot_Terrain_'+str(side)];bank=bpy.data.objects['Pilot_Cutbank_'+str(side)];assert len(terrain.data.vertices)==193*85 and len(bank.data.vertices)==193*5
 changed=0
 for i in range(193):
  edge=terrain.data.vertices[i*85].co;x=edge.x;t=abs(x-cx)/halfwidth
  if t>=1:continue
  weight=(1-t*t)**2;delta=max(0,edge.z-.45)*weight
  for j in range(depth):terrain.data.vertices[i*85+j].co.z-=delta*(1-j/depth)**2;changed+=1
  for level in range(4):bank.data.vertices[i*5+level].co.z-=delta*(1-level/4)
 terrain.data.update();bank.data.update();edits.append({'side':side,'centre_x':cx,'halfwidth':halfwidth,'selected_vertices':changed})
bpy.context.view_layer.update()
for o,oldheight in roots:
 z=ground(o.location.x,o.location.y)
 if oldheight is not None and z is not None:o.location.z+=z-oldheight
for v,oldheight in zip(under.data.vertices,heights):
 z=ground(v.co.x,v.co.y)
 if oldheight is not None and z is not None:v.co.z+=z-oldheight
under.data.update()
# Re-ground small inherited stones nearest the edited approaches.
from mathutils import Vector
def ground(x,y):
 for side in [-1,1]:
  hit,co,n,index=bpy.data.objects['Pilot_Terrain_'+str(side)].ray_cast(Vector((x,y,80)),Vector((0,0,-1)))
  if hit:return co.z
 return None
for o in scene.objects:
 if o.name.startswith('Pilot_Rock') or o.name.startswith('Bank forest stone'):
  z=ground(o.location.x,o.location.y)
  if z is not None:o.location.z=z+.12
# The inherited terrain AO belongs to the previous tree positions. Remove its known factor and bake actual contact.
old=bpy.data.images['Ribera baked contact AO'];w,h=old.size;a=np.array(old.pixels[:],np.float32).reshape(h,w,4)[:,:,:3];ix=np.minimum((np.arange(S)*w/S).astype(int),w-1);iy=np.minimum((np.arange(S)*h/S).astype(int),h-1);oldao=a[iy[:,None],ix[None,:]];rgba[:,:,:3]/=.62+.38*oldao
# Restore the reviewed surface during AO bake; AO is kept as an independent editable image.
n=nodes.new('ShaderNodeTexImage');n.image=base;links.new(n.outputs['Color'],bs.inputs['Base Color'])
ao=bpy.data.images.new('I actual terrain forest contact',width=1024,height=1024);ao.colorspace_settings.name='Non-Color';target=nodes.new('ShaderNodeTexImage');target.image=ao;nodes.active=target
occ=nodes.new('ShaderNodeAmbientOcclusion');occ.inputs['Distance'].default_value=4.;occ.samples=16;em=nodes.new('ShaderNodeEmission');links.new(occ.outputs['Color'],em.inputs['Color']);links.new(em.outputs[0],output.inputs['Surface'])
bpy.ops.object.select_all(action='DESELECT')
for side in [-1,1]:bpy.data.objects['Pilot_Terrain_'+str(side)].select_set(True)
bpy.context.view_layer.objects.active=bpy.data.objects['Pilot_Terrain_1'];scene.render.engine='CYCLES';scene.cycles.samples=16;bpy.ops.object.bake(type='EMIT',margin=4,use_clear=True);links.new(bs.outputs[0],output.inputs['Surface'])
a=np.array(ao.pixels[:],np.float32).reshape(1024,1024,4)[:,:,:3];a=np.repeat(np.repeat(a,2,0),2,1);rgba[:,:,:3]=np.clip(rgba[:,:,:3]*(.68+.32*a),0,1)
im=bpy.data.images.new('I retained surface new contact',width=S,height=S);im.colorspace_settings.name='sRGB';im.pixels.foreach_set(rgba.ravel());n=nodes.new('ShaderNodeTexImage');n.image=im;nodes.active=n;links.new(n.outputs['Color'],bs.inputs['Base Color'])
scene['finishing_stage']='I retained G surfaces H forest E water and sculpted bank approaches';scene['I_source_H_sha256']=hashlib.sha256(src.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.type=='IMAGE' and im.size[0]>0:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'I','source_H_sha256':scene['I_source_H_sha256'],'bank_sculpt':edits,'retained':'G material; H asymmetric pine groups and understory; E water','rejected':'H isolated rounded meadow paint','contact':'native Cycles AO rebake after tree regrouping and bank sculpture','art_approved':False},indent=2))
