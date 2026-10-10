"""Eldoria isolated model-guided Blender lookdev feasibility loop.

AI sees actual pre-render plus canonical reference and chooses bounded material
corrections. This does NOT assert art quality and does not touch Unity gameplay.
"""
import bpy, json, os, base64, urllib.request, re, math
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[2]
src=root/'Unity/Assets/Eldoria/ProductionSlice/Experimental/ValoriaMeshOnlyWorld.glb'
reference=root/'references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg'
out=Path(os.environ.get('ELDORIA_VISION_OUTPUT',str(root/'artifacts-local/eldoria-ai-lookdev')))
out.mkdir(parents=True,exist_ok=True)
assert src.is_file(),str(src)
assert reference.is_file(),str(reference)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(src))
scene=bpy.context.scene
scene.render.engine='CYCLES'
scene.cycles.samples=8
scene.cycles.seed=17
scene.render.resolution_x=960
scene.render.resolution_y=640
scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG'
scene.view_settings.view_transform='Standard'
world=bpy.data.worlds.new('Valoria dusk neutral');scene.world=world;world.use_nodes=True
world.node_tree.nodes['Background'].inputs['Color'].default_value=(.25,.31,.40,1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value=.80
light=bpy.data.lights.new('Area softbox','AREA')
lob=bpy.data.objects.new('Area softbox',light);scene.collection.objects.link(lob)
lob.location=(-38,-24,66);light.energy=22000;light.shape='DISK';light.size=32
camd=bpy.data.cameras.new('Fixed Valoria comparison camera');cam=bpy.data.objects.new('Fixed Valoria comparison camera',camd)
scene.collection.objects.link(cam);scene.camera=cam
cam.location=(55,-82,78);t=Vector((0,6,8));cam.rotation_euler=(t-Vector(cam.location)).to_track_quat('-Z','Y').to_euler()
camd.type='ORTHO';camd.ortho_scale=94
before=out/'before.png';after=out/'after.png'
scene.render.filepath=str(before);bpy.ops.render.render(write_still=True)
prompt=(
 'You are the art-direction assistant for a dark medieval 4X mobile game. '
 'The FIRST image is the latest actual Blender rendering; SECOND image is the approved visual reference. '
 'Choose one conservative MATERIAL lookdev correction; do not claim shape, architecture or art PASS. '
 'Respond ONLY as JSON: '
 '{"stone_brightness":number,"rock_brightness":number,"roof_brightness":number,"observation":string}. '
 'Each brightness is a multiplier between 0.75 and 1.25. '
 'Identify a concrete visual weakness in observation. Do not demand paid tools.'
)
images=[base64.b64encode(p.read_bytes()).decode('ascii') for p in (before,reference)]
payload={"model":"gemma3:4b","stream":False,"format":"json",
         "messages":[{"role":"user","content":prompt,"images":images}],
         "options":{"temperature":0.2,"num_predict":210}}
body=json.dumps(payload).encode('utf-8')
req=urllib.request.Request('http://127.0.0.1:11434/api/chat',data=body,headers={'Content-Type':'application/json'})
with urllib.request.urlopen(req,timeout=210) as f: response=json.loads(f.read().decode('utf-8'))
msg=response.get('message',{}).get('content','')
data=json.loads(msg)
factors={k:max(.75,min(1.25,float(data.get(k,1)))) for k in ('stone_brightness','rock_brightness','roof_brightness')}
changes=0
for m in bpy.data.materials:
 if not m.use_nodes:continue
 label=m.name.lower()
 if any(w in label for w in ('rock','cliff','bedrock','earth','terrain')):factor=factors['rock_brightness']
 elif any(w in label for w in ('roof','slate','tile','shingle')):factor=factors['roof_brightness']
 elif any(w in label for w in ('stone','brick','masonry','wall','plaster','limestone')):factor=factors['stone_brightness']
 else:continue
 bs=m.node_tree.nodes.get('Principled BSDF')
 if bs:
  socket=bs.inputs['Base Color']
  if socket.is_linked:
   # glTF textures override default socket colors: actively insert a multiplier.
   prior=socket.links[0].from_socket
   mix=m.node_tree.nodes.new('ShaderNodeMixRGB')
   mix.blend_type='MULTIPLY'
   mix.inputs[0].default_value=1.0
   mix.inputs[2].default_value=(factor,factor,factor,1.0)
   m.node_tree.links.new(prior,mix.inputs[1])
   m.node_tree.links.new(mix.outputs[0],socket)
  else:
   color=socket.default_value
   socket.default_value=tuple(min(1,max(0,float(c)*factor)) for c in color[:3])+(float(color[3]),)
  changes+=1
if changes==0: raise RuntimeError('No material categories matched actual source')
if all(abs(x-1.0)<.007 for x in factors.values()):
 raise RuntimeError('Local model proposed no meaningful material correction')
scene.render.filepath=str(after);bpy.ops.render.render(write_still=True)
# Pixel-level evidence is not artistic approval, but rules out false no-op renders.
def pixel_sample(path):
 image=bpy.data.images.load(str(path),check_existing=False)
 rgba=list(image.pixels[::557])
 bpy.data.images.remove(image)
 return rgba
before_sample=pixel_sample(before)
after_sample=pixel_sample(after)
assert len(before_sample)==len(after_sample)
mean_pixel_delta=sum(abs(a-b) for a,b in zip(before_sample,after_sample))/max(1,len(before_sample))
if mean_pixel_delta < 0.0005:
 raise RuntimeError('Blender art model proposed edits that did not materially change rendered pixels')
bpy.ops.wm.save_as_mainfile(filepath=str(out/'eldoria-vision-lookdev.blend'))
report={"visual_pass":False,"unity_tested":False,"model":"gemma3:4b",
        "actual_blender_pre_and_post":True,"fixed_camera":True,"material_count_touched":changes,"mean_pixel_delta":round(mean_pixel_delta,6),
        "factors":factors,"model_observation":str(data.get('observation',''))[:1000],
        "note":"Bounded material-only feasibility; geometry and commercial art reference still unapproved."}
(out/'agent-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
print('ELDORIA_LOCAL_VISION_ITERATION_PASS',json.dumps(report,ensure_ascii=False))
