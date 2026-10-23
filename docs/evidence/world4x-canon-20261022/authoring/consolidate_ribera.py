"""Retain C painted ground; restore B water presentation. No geography changes."""
import bpy,sys,pathlib,hashlib,json
args=sys.argv[sys.argv.index('--')+1:];out=pathlib.Path(args[0]);source=pathlib.Path(args[1]);out.mkdir(parents=True,exist_ok=True)
assert hashlib.sha256(source.read_bytes()).hexdigest()=='9850c77ef20f87f793d1ca9936a93ccaaaaae9c9ad7b1f4c6661828cc6d29187'
bpy.ops.wm.open_mainfile(filepath=str(source));scene=bpy.context.scene
mat=bpy.data.materials['River turquoise'];bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
for link in list(bs.inputs['Base Color'].links):mat.node_tree.links.remove(link)
bs.inputs['Base Color'].default_value=(.025,.32,.40,1)
for name in ['Pilot_Deep','Pilot_RimSouth','Pilot_RimNorth']:bpy.data.objects[name].hide_render=False
scene['finishing_stage']='D retained C groundcover plus B water and foliage presentation';scene['stage_C_source_sha256']=hashlib.sha256(source.read_bytes()).hexdigest()
for im in bpy.data.images:
 if im.type=='IMAGE' and im.size[0]>0:
  im.filepath_raw=str(out/(im.name.replace(' ','_')+'.png'));im.file_format='PNG';im.save();im.pack()
bpy.ops.object.select_all(action='DESELECT')
for o in scene.objects:
 if o.type=='MESH' and not o.hide_render:o.select_set(True)
bpy.ops.export_scene.gltf(filepath=str(out/'RiberaPilot.glb'),export_format='GLB',use_selection=True)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Eldoria_Ribera_Finished.blend'))
scene.cycles.samples=24;scene.render.filepath=str(out/'blender-ribera.png');bpy.ops.render.render(write_still=True)
(out/'finishing.json').write_text(json.dumps({'stage':'D','source_sha256':scene['stage_C_source_sha256'],'retained':'A bank sculpture; B grouping/contact AO/lighting; C UV groundcover','reverted':'C too-dark water and harsh foliage shader','geography_changed':False,'art_approved':False},indent=2))
