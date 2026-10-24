import bpy,json,pathlib,hashlib,os
out=pathlib.Path(bpy.app.driver_namespace['interactive_output'])
pigment=out.parent/'executive-e5c63ba475cd-20261024'/'02-mineral-pigment.png'
old=bpy.data.images['Region1 authored meadow and woodland pigment']
fresh=bpy.data.images.load(str(pigment),check_existing=False);fresh.name='Region1 authored geological pigment';fresh.pack()
expected=hashlib.sha256(pigment.read_bytes()).hexdigest()
assert hashlib.sha256(fresh.packed_file.data).hexdigest()==expected
links=0
for material in bpy.data.materials:
 if material.use_nodes:
  for node in material.node_tree.nodes:
   if node.type=='TEX_IMAGE' and node.image==old:node.image=fresh;links+=1
assert links>0
for request in out.glob('request-*.json'):
 data=json.loads(request.read_text(encoding='utf-8-sig'))
 bpy.data.texts.new('MCP_'+request.stem).write(data.get('params',{}).get('code',''))
target=out/'Eldoria_Macro_Canopy_HANDOFF_PREVIEW_NOT_FOR_EXPORT.blend'
bpy.ops.wm.save_as_mainfile(filepath=str(target),compress=True)
bpy.ops.wm.open_mainfile(filepath=str(target))
loaded=bpy.data.images['Region1 authored geological pigment']
assert hashlib.sha256(loaded.packed_file.data).hexdigest()==expected,'Saved source did not retain actual painted PNG'
report={'owned_pid':os.getpid(),'user_requested_handoff':True,'packed_image_saved_and_reopened_sha256':expected,'material_links_corrected':links,'blend_sha256':hashlib.sha256(target.read_bytes()).hexdigest(),'preview_only':True,'preview_note':'Far tree render scale and visibility emulate Unity LOD; do not export this preview directly. Native builder applies authored recipes to certified source.','native_certified':False,'art_approved':False}
(out/'SAFE_HANDOFF_VERIFICATION.json').write_text(json.dumps(report,indent=2))
(out/'stop.request').write_text('Owner requested safe handoff. Close only this owned Blender session after verified save/reopen.')
print('SAFE_HANDOFF_VERIFICATION',json.dumps(report))
