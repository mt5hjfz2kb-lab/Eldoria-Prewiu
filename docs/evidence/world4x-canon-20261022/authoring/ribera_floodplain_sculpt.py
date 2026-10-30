"""M: native cross-section sculpt of K into alternating cutbanks and low alluvial shelves."""
import bpy,sys,pathlib,hashlib,json
import numpy as np
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);src=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(src.read_bytes()).hexdigest()=='1089e26bcb66f874d41a3579a1027916d5b7619ccc2892f8ac9c6c00b4234566'
bpy.ops.wm.open_mainfile(filepath=str(src));scene=bpy.context.scene;assert scene.get('finishing_stage','').startswith('K branch-level')
# Long asymmetric reaches, authored as separate brush selections in the existing topology.
reaches=[(-1,-44,-13,4.5,1.0),(1,-6,25,3.7,.90),(-1,32,60,4.8,1.15),(1,-77,-56,3.0,1.2)]
profile=[0,.12,.42,.75,.94,1,.97,.84,.57,.23,0]
def weight(x,a,b):
 t=max(0,min(10,(x-a)/(b-a)*10));i=min(9,int(t));return profile[i]*(1-(t-i))+profile[i+1]*(t-i)
old_roots={o.name:tuple(o.location) for o in scene.objects if o.name.startswith('Pilot_Pine')}
records=[]
for side,a,b,width,target in reaches:
 terrain=bpy.data.objects['Pilot_Terrain_'+str(side)];bank=bpy.data.objects['Pilot_Cutbank_'+str(side)];group=terrain.vertex_groups.new(name='M alluvial shelf '+str(a));count=0
 for i in range(193):
  x=float(terrain.data.vertices[i*85].co.x)
  if not a<x<b:continue
  w=weight(x,a,b);edge=terrain.data.vertices[i*85].co.copy();shift=width*w;drop=max(0,edge.z-target)*w
  for row in range(12):
   f=(1-row/12)**2;v=terrain.data.vertices[i*85+row];v.co.y+=side*shift*f;v.co.z-=drop*f;group.add([v.index],w*f,'REPLACE');count+=1
  crown=terrain.data.vertices[i*85].co.copy();toe=bank.data.vertices[i*5+4].co.copy()
  # Crown, shoulder, sand shelf, wet foot, submerged toe; no new coastline function.
  for row,t in enumerate([0,.20,.59,.91,1]):
   v=bank.data.vertices[i*5+row];new_y=crown.y*(1-t)+toe.y*t
   new_z=[crown.z,crown.z*.78+.04,crown.z*.32+.19,.36,.08][row]
   v.co.y=v.co.y*(1-w)+new_y*w if row else crown.y
   v.co.z=v.co.z*(1-w)+new_z*w if row else crown.z
 terrain.data.update();bank.data.update();records.append({'side':side,'from_x':a,'to_x':b,'maximum_outward_width':width,'target_crown_height':target,'selected_vertices':count})
# Soil material follows actual surface coordinates; previous horizontal strata paint is retired.
base=bpy.data.images['I recovered exact retained surface'];S=base.size[0];rgba=np.array(base.pixels[:],np.float32).reshape(S,S,4);lum=rgba[:,:,:3].mean(2);grain=np.clip(lum/(np.mean(lum)+1e-6),.86,1.12)
soil=np.ones_like(rgba);soil[:,:,:3]=np.array([.64,.555,.385])[None,None,:]*grain[:,:,None]
im=bpy.data.images.new('M alluvial clay and mineral grain',width=S,height=S);im.colorspace_settings.name='sRGB';im.pixels.foreach_set(soil.ravel());im.filepath_raw=str(out/'M_alluvial_clay_and_mineral_grain.png');im.file_format='PNG';im.save();im.pack()
for side in [-1,1]:
 bank=bpy.data.objects['Pilot_Cutbank_'+str(side)];mat=bank.data.materials[0].copy();mat.name='M exposed alluvial earth '+str(side);bank.data.materials[0]=mat;bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;mat.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color'])
 uv=bank.data.uv_layers.active;uv.name='M World mapped earth'
 for p in bank.data.polygons:
  for li in p.loop_indices:
   v=bank.data.vertices[bank.data.loops[li].vertex_index].co;uv.data[li].uv=((v.x+96)/192,(v.y+78)/156)
# Reground only decorative vegetation affected by the sculpt; keep horizontal coordinates.
bpy.context.view_layer.update()
def ground(x,y):
 for side in [-1,1]:
  hit,co,n,index=bpy.data.objects['Pilot_Terrain_'+str(side)].ray_cast(Vector((x,y,80)),Vector((0,0,-1)))
  if hit:return co.z
 return None
for o in scene.objects:
 if o.name.startswith('Pilot_Pine'):
  z=ground(o.location.x,o.location.y)
  if z is not None and any(a<o.location.x<b and abs(o.location.y-bpy.data.objects['Pilot_Terrain_'+str(side)].data.vertices[int(round(o.location.x+96))*85].co.y)<12 for side,a,b,_,_ in reaches):o.location.z=z
scene['finishing_stage']='M asymmetric alluvial shelf sculpt on owner retained K';scene['M_source_sha256']=hashlib.sha256(src.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.size[0]>0 and im.source in {'GENERATED','FILE'}:im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'));scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'M','source_sha256':scene['M_source_sha256'],'sculpted_reaches':records,'retained':'K terrain pigment, river course and surface, paths, forest placement, gameplay and cameras','changed':'native bank/terrain vertices, bank UV/material, decorative tree height only where affected','original_tree_locations':old_roots,'art_approved':False},indent=2))
