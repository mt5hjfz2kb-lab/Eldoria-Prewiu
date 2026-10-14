"""Ground-only surface stage subordinate to original Qwen decisions and the canonical typed API."""
import argparse,base64,hashlib,json,math,pathlib,sys,urllib.request,time
ROOT=pathlib.Path(__file__).resolve().parents[3]
TARGET='ZoneOne continuous prairie source'
def sha(b):return hashlib.sha256(b).hexdigest()
def prepare(c,source):
 import bpy
 blend=source/'edited/artist-output.blend'
 bpy.ops.wm.open_mainfile(filepath=str(blend.resolve()))
 terrain=bpy.data.objects[TARGET];coords=[list(v.co) for v in terrain.data.vertices]
 imported_custom_normals=terrain.data.has_custom_normals
 # Official Mesh API: zero vectors restore automatic normals after sculpting.
 # https://docs.blender.org/api/4.0/bpy.types.Mesh.html#bpy.types.Mesh.normals_split_custom_set
 if imported_custom_normals:
  terrain.data.normals_split_custom_set([(0.,0.,0.)]*len(terrain.data.loops));terrain.data.update()
 assert coords==[list(v.co) for v in terrain.data.vertices]
 bpy.ops.wm.save_as_mainfile(filepath=str((c/'geometry-normal-fixed.blend').resolve()))
 scene=bpy.context.scene
 scene.render.engine='CYCLES';scene.cycles.samples=16;scene.cycles.use_denoising=False
 # Freeze the identical lit renderer in both source files, not Workbench's grey material display.
 bpy.ops.wm.save_as_mainfile(filepath=str((c/'geometry-normal-fixed.blend').resolve()))
 scene.render.filepath=str((c/'geometry-normal-fixed.png').resolve());bpy.ops.render.render(write_still=True)
 (c/'normal-refresh-proof.json').write_text(json.dumps({'original_artist_blend_sha256':sha(blend.read_bytes()),
  'imported_custom_normals_present':imported_custom_normals,'automatic_normals_refreshed':imported_custom_normals,
  'geometry_unchanged':True,'image_type':'BLENDER_NOT_UNITY','render_engine':'CYCLES','camera_matrix':[list(row) for row in scene.camera.matrix_world],'art_approved':False},indent=2))
def author(c,source):
 from PIL import Image
 sys.path.insert(0,str(ROOT/'tools/art-direction/ai-authoring'))
 from artist_plan_validation import inspect_artist_plan
 role=(ROOT/'tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md').read_bytes()
 donor=ROOT/'Unity/Assets/Mega Fantasy Props Pack/Textures/grass_1_fixed_d.png'
 paths=[ROOT/'docs/world4x-zone1/evidence/local-cycle-20261014/references/local-reference.png',c/'geometry-normal-fixed.png',donor]
 images=[];provenance=[]
 for i,path in enumerate(paths):
  im=Image.open(path).convert('RGB');im.thumbnail((512,512));dest=c/f'surface-vision-{i}.jpg';im.save(dest,quality=85)
  images.append(base64.b64encode(dest.read_bytes()).decode());provenance.append({'source':str(path),'sha256':sha(path.read_bytes()),'vision_sha256':sha(dest.read_bytes())})
 recipe={'rgb':{'type':'array','items':{'type':'number','minimum':0,'maximum':1},'minItems':3,'maxItems':3},
  'roughness':{'type':'number','minimum':.7,'maximum':1},'secondary_rgb':{'type':'array','items':{'type':'number','minimum':0,'maximum':1},'minItems':3,'maxItems':3},
  'dry_rgb':{'type':'array','items':{'type':'number','minimum':0,'maximum':1},'minItems':3,'maxItems':3},
  'rock_rgb':{'type':'array','items':{'type':'number','minimum':0,'maximum':1},'minItems':3,'maxItems':3},
  'patch_scale_m':{'type':'number','minimum':12,'maximum':45},'detail_tile_m':{'type':'number','minimum':2,'maximum':8},
  'dry_fraction':{'type':'number','minimum':.1,'maximum':.45},'detail_strength':{'type':'number','minimum':.1,'maximum':.5},
  'rock_slope_degrees':{'type':'number','minimum':22,'maximum':40}}
 fields={key:{'type':'string','minLength':35,'maxLength':250} for key in ['observed_defect','design_intent']}
 fields.update({'asset_decision':{'type':'string','const':'MODIFY'},'reason_for_new_geometry':{'type':'string'},
 'planned_actions':{'type':'array','minItems':1,'maxItems':1,'items':{'type':'object','properties':{
 'id':{'type':'string'},'type':{'type':'string','const':'assign_material'},'target':{'type':'string','const':TARGET},
 'rationale':{'type':'string','minLength':15},'parameters':{'type':'object','properties':recipe,'required':list(recipe),'additionalProperties':False}},'required':['id','type','target','rationale','parameters'],'additionalProperties':False}},
 'expected_visual_difference':{'type':'string'},'mobile_risks':{'type':'string'}})
 schema={'type':'object','properties':fields,'required':list(fields),'additionalProperties':False}
 prompt=('Zone One ground-only SOURCE surface stage, not Valoria architecture. Images are owner reference, ACTUAL BLENDER SOURCE AFTER TERRAIN EDIT (NOT Unity), '
  'and existing donor grass texture. Preserve all terrain geometry, boundary, city pads and source props. '
  'Design textured, luminous, fantasy-medieval prairie using the donor as microdetail, not a flat green field or a gritty photoreal floor. '
  'Choose a coherent primary and secondary meadow color, soft golden dry-grass transitions, pale exposed slope stone. No buildings, scatter or new geometry. '
  'Do not paint paths or violet corruption during this ground-only stage. Decide every palette/scale/roughness parameter yourself. '
  'The assigned RGB and roughness execute in the existing typed API. A scoped technical bake will realize your additional exact recipe: '
  'macro meadow variations at patch_scale_m, soft dry_fraction blend, donor tiled at detail_tile_m, detail_strength modulated color/normal, rock color only above your slope threshold. '
  'The bake makes glTF-compatible color and tangent normal atlases (1024 square) and retains A/B camera, lights and geometry. '
  'These are production proposals, not Unity/device approval. Return a SINGLE assign_material action, with strict schema. No self-approval.')
 payload={'model':'qwen3-vl:4b-instruct','stream':True,'think':False,'format':schema,'options':{'num_ctx':8192,'num_predict':1100,'temperature':.3},
  'messages':[{'role':'system','content':role.decode()},{'role':'user','content':prompt,'images':images}]}
 (c/'surface-request.json').write_text(json.dumps(payload,indent=2))
 for attempt in range(2):
  chunks=[];reply={};start=time.monotonic();last=start
  req=urllib.request.Request('http://127.0.0.1:11434/api/chat',data=json.dumps(payload).encode(),headers={'Content-Type':'application/json'})
  with urllib.request.urlopen(req,timeout=540) as response, (c/f'surface-stream-{attempt+1}.jsonl').open('w') as log:
   for line in response:
    if not line.strip():continue
    log.write(line.decode());log.flush();piece=json.loads(line)
    if piece.get('error'):raise RuntimeError(piece['error'])
    chunks.append(piece.get('message',{}).get('content',''))
    if time.monotonic()-last>30:print('REAL_SURFACE_ARTIST_STREAM',len(''.join(chunks)),flush=True);last=time.monotonic()
    if piece.get('done'):reply=piece;break
  if not reply.get('done'):raise RuntimeError('Incomplete genuine artist response')
  raw=''.join(chunks);(c/f'surface-response-{attempt+1}.txt').write_text(raw)
  errors=[]
  try:
   design=json.loads(raw);errors=inspect_artist_plan(design,TARGET,'existing',1,require_geometry=False)
   pr=design['planned_actions'][0]['parameters']
   for key,spec in recipe.items():
    val=pr.get(key)
    if spec['type']=='array':
     if not isinstance(val,list) or len(val)!=3 or not all(isinstance(v,(int,float)) and 0<=v<=1 for v in val):errors.append('Invalid RGB '+key)
    elif not isinstance(val,(int,float)) or not spec['minimum']<=val<=spec['maximum']:errors.append('Invalid bound '+key)
   if design['planned_actions'][0]['type']!='assign_material':errors.append('Only material assignment allowed')
  except (ValueError,TypeError,KeyError) as exc:errors.append(str(exc))
  if not errors:
   (c/'artist-plan.json').write_text(json.dumps(design['planned_actions'],indent=2))
   (c/'artist-inference.json').write_text(json.dumps({'model':payload['model'],'system_role':'system','system_prompt_sha256':sha(role),
    'input_images':provenance,'image_type':'BLENDER_NOT_UNITY','direction':design,'seconds':time.monotonic()-start,
    'model_weights_trained':False,'self_approval':False},indent=2));return
  if attempt==1:raise RuntimeError('Material design rejected: '+str(errors))
  payload['messages'] += [{'role':'assistant','content':raw},{'role':'user','content':'Correct these exact errors by your own decisions: '+str(errors)}]

def bake(c,source):
 import bpy
 bpy.ops.wm.open_mainfile(filepath=str((c/'typed/artist-output.blend').resolve()))
 terrain=bpy.data.objects[TARGET];coordinates=[list(v.co) for v in terrain.data.vertices]
 recipe=json.loads((c/'artist-plan.json').read_text())[0]['parameters']
 mat=terrain.active_material.copy();mat.name='ZoneOne Qwen prairie surface baked';mat.use_nodes=True
 terrain.active_material=mat;nodes=mat.node_tree.nodes;nodes.clear();links=mat.node_tree.links
 def node(kind):return nodes.new(kind)
 def connect(a,b):links.new(a,b)
 geo=node('ShaderNodeNewGeometry');noise=node('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=1/recipe['patch_scale_m'];noise.inputs['Detail'].default_value=2
 connect(geo.outputs['Position'],noise.inputs['Vector'])
 green=node('ShaderNodeMixRGB');green.inputs[1].default_value=(*recipe['rgb'],1);green.inputs[2].default_value=(*recipe['secondary_rgb'],1)
 connect(noise.outputs['Fac'],green.inputs[0])
 drynoise=node('ShaderNodeTexNoise');drynoise.inputs['Scale'].default_value=.73/recipe['patch_scale_m'];drynoise.inputs['Detail'].default_value=1
 drynoise.inputs['Distortion'].default_value=.7;connect(geo.outputs['Position'],drynoise.inputs['Vector'])
 drymask=node('ShaderNodeMapRange');drymask.clamp=True
 drymask.inputs['From Min'].default_value=.65-recipe['dry_fraction'];drymask.inputs['From Max'].default_value=.75
 connect(drynoise.outputs['Fac'],drymask.inputs['Value'])
 dry=node('ShaderNodeMixRGB');dry.inputs[2].default_value=(*recipe['dry_rgb'],1);connect(green.outputs[0],dry.inputs[1]);connect(drymask.outputs[0],dry.inputs[0])
 donorpath=ROOT/'Unity/Assets/Mega Fantasy Props Pack/Textures/grass_1_fixed_d.png'
 donor=node('ShaderNodeTexImage');donor.image=bpy.data.images.load(str(donorpath),check_existing=True)
 tile=node('ShaderNodeVectorMath');tile.operation='MULTIPLY';tile.inputs[1].default_value=(1/recipe['detail_tile_m'],1/recipe['detail_tile_m'],1)
 connect(geo.outputs['Position'],tile.inputs[0]);connect(tile.outputs[0],donor.inputs['Vector'])
 gray=node('ShaderNodeRGBToBW');connect(donor.outputs['Color'],gray.inputs[0])
 detail=node('ShaderNodeMixRGB');detail.blend_type='MULTIPLY';detail.inputs[0].default_value=recipe['detail_strength']
 connect(dry.outputs[0],detail.inputs[1]);connect(gray.outputs[0],detail.inputs[2])
 separate=node('ShaderNodeSeparateXYZ');connect(geo.outputs['Normal'],separate.inputs[0])
 slope=node('ShaderNodeMapRange');slope.clamp=True;slope.inputs['From Min'].default_value=math.cos(math.radians(recipe['rock_slope_degrees']+8));slope.inputs['From Max'].default_value=math.cos(math.radians(recipe['rock_slope_degrees']))
 slope.inputs['To Min'].default_value=1;slope.inputs['To Max'].default_value=0;connect(separate.outputs['Z'],slope.inputs['Value'])
 rock=node('ShaderNodeMixRGB');rock.inputs[2].default_value=(*recipe['rock_rgb'],1);connect(slope.outputs[0],rock.inputs[0]);connect(detail.outputs[0],rock.inputs[1])
 shader=node('ShaderNodeBsdfPrincipled');shader.inputs['Roughness'].default_value=recipe['roughness'];connect(rock.outputs[0],shader.inputs['Base Color'])
 bump=node('ShaderNodeBump');bump.inputs['Strength'].default_value=recipe['detail_strength'];bump.inputs['Distance'].default_value=.025
 connect(gray.outputs[0],bump.inputs['Height']);connect(bump.outputs[0],shader.inputs['Normal'])
 output=node('ShaderNodeOutputMaterial');connect(shader.outputs[0],output.inputs['Surface'])
 bpy.ops.object.select_all(action='DESELECT');terrain.select_set(True);bpy.context.view_layer.objects.active=terrain
 scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=8;scene.render.bake.use_pass_direct=False;scene.render.bake.use_pass_indirect=False;scene.render.bake.use_pass_color=True
 baked=[]
 for kind,name in [('DIFFUSE','prairie-color'),('NORMAL','prairie-normal')]:
  image=bpy.data.images.new(name,width=1024,height=1024,alpha=False)
  if kind=='NORMAL':image.colorspace_settings.name='Non-Color'
  texture=node('ShaderNodeTexImage');texture.image=image;nodes.active=texture
  bpy.ops.object.bake(type=kind,margin=4)
  path=c/(name+'.png');image.filepath_raw=str(path.resolve());image.file_format='PNG';image.save();image.pack();baked.append((image,path))
 nodes.clear();shader=node('ShaderNodeBsdfPrincipled');shader.inputs['Roughness'].default_value=recipe['roughness']
 color=node('ShaderNodeTexImage');color.image=baked[0][0];normaltex=node('ShaderNodeTexImage');normaltex.image=baked[1][0]
 normal=node('ShaderNodeNormalMap');connect(normaltex.outputs['Color'],normal.inputs['Color']);connect(normal.outputs[0],shader.inputs['Normal']);connect(color.outputs['Color'],shader.inputs['Base Color'])
 output=node('ShaderNodeOutputMaterial');connect(shader.outputs[0],output.inputs['Surface'])
 assert coordinates==[list(v.co) for v in terrain.data.vertices],'Surface stage must not alter terrain geometry'
 bpy.ops.wm.save_as_mainfile(filepath=str((c/'ground-surface.blend').resolve()))
 bpy.ops.export_scene.gltf(filepath=str((c/'ground-surface.glb').resolve()),export_format='GLB',use_selection=False)
 scene.render.filepath=str((c/'blender-surface-after.png').resolve());scene.cycles.samples=16;bpy.ops.render.render(write_still=True)
 (c/'surface-execution.json').write_text(json.dumps({'artist_plan_sha256':sha((c/'artist-plan.json').read_bytes()),'source_blend_sha256':sha((source/'edited/artist-output.blend').read_bytes()),
  'technical_interpretation':'glTF-compatible texture bake of exact Qwen material recipe; existing canonical assign_material already executed',
  'recipe':recipe,'donor_sha256':sha(donorpath.read_bytes()),'geometry_unchanged':True,'model_sha256':sha((c/'ground-surface.glb').read_bytes()),
  'textures':[{'path':p.name,'sha256':sha(p.read_bytes())} for image,p in baked],
  'camera_matrix':[list(row) for row in scene.camera.matrix_world],'image_type':'BLENDER_NOT_UNITY','art_approved':False},indent=2))

if __name__=='__main__':
 p=argparse.ArgumentParser();p.add_argument('--phase',choices=['prepare','author','bake'],required=True);p.add_argument('--work-dir',dest='cycle',type=pathlib.Path,required=True);p.add_argument('--source',type=pathlib.Path,required=True)
 args=p.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else sys.argv[1:]);args.cycle.mkdir(parents=True,exist_ok=True)
 {'prepare':prepare,'author':author,'bake':bake}[args.phase](args.cycle,args.source)
