"""Execute unchanged Qwen parameters using a bounded rock brush and packed material tints."""
import argparse,hashlib,json,math,pathlib,sys,bpy
from mathutils import Vector
p=argparse.ArgumentParser();p.add_argument('--root',type=pathlib.Path,required=True);a=p.parse_args(sys.argv[sys.argv.index('--')+1:]);r=a.root
plan=json.loads((r/'artist-plan.json').read_text());proof=json.loads((r/'inference-proof.json').read_text());assert hashlib.sha256((r/'artist-plan.json').read_bytes()).hexdigest()==proof['plan_sha256'];assert plan['operation']=='finish_existing_terrain';q=plan['parameters']
bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(r/'source.glb'))
t=bpy.data.objects['Valoria natural playable hilltop and rock scarps'];m=t.data
assert len(m.vertices)==12601 and len(m.polygons)==23296
others={o.name:hashlib.sha256(str([tuple(v.co) for v in o.data.vertices]).encode()).hexdigest() for o in bpy.data.objects if o.type=='MESH' and o!=t}
original=[v.co.copy() for v in m.vertices];original_uv=[tuple(x.uv) for x in m.uv_layers.active.data];original_faces=[(tuple(f.vertices),f.material_index) for f in m.polygons]
rock_slots={i for i,mat in enumerate(m.materials) if 'schist dark fractured cliff' in mat.name};assert len(rock_slots)==1
rock_vertices={i for f in m.polygons if f.material_index in rock_slots for i in f.vertices};changed=0
for v in m.vertices:
 if v.index not in rock_vertices or v.co.z>=-1:continue
 n=Vector((v.normal.x,v.normal.y,0))
 if n.length<.05:continue
 n.normalize();x,y,z=v.co
 along=x if abs(n.y)>abs(n.x) else y
 triangle=1-4*abs(((along/q['flute_pitch_m']+q['phase_radians']/(2*math.pi))%1)-.5)
 layer=1+q['layer_strength']*math.sin(z*2*math.pi/q['layer_pitch_m']+q['phase_radians'])
 fade=min(1,max(0,(-1-z)/2))
 v.co+=n*(triangle*q['fracture_depth_m']*layer*fade);changed+=1
for f in m.polygons:
 if f.material_index in rock_slots:f.use_smooth=not q['flat_rock_faces']
m.update();assert changed>500
assert original_faces==[(tuple(f.vertices),f.material_index) for f in m.polygons];assert original_uv==[tuple(x.uv) for x in m.uv_layers.active.data]
assert all((v.co-original[v.index]).length<1e-7 for v in m.vertices if original[v.index].z>=-1)
for mat in m.materials:
 if 'terrace earth olive grass' in mat.name:rgb=q['grass_rgb'];scale=q['grass_uv_scale']
 elif 'schist dark fractured cliff' in mat.name:rgb=q['cliff_rgb'];scale=q['cliff_uv_scale']
 else:continue
 # glTF exports a constant multiply of its packed basecolor image as a factor.
 nt=mat.node_tree;bs=nt.nodes.get('Principled BSDF');links=list(bs.inputs['Base Color'].links);assert len(links)==1
 prev=links[0].from_socket;nt.links.remove(links[0]);mix=nt.nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=1;mix.inputs[2].default_value=(*rgb,1);nt.links.new(prev,mix.inputs[1]);nt.links.new(mix.outputs[0],bs.inputs['Base Color'])
 for tex in (n for n in list(nt.nodes) if n.type=='TEX_IMAGE'):
  uv=nt.nodes.new('ShaderNodeTexCoord');mapping=nt.nodes.new('ShaderNodeMapping');mapping.inputs['Scale'].default_value=(scale,scale,1);nt.links.new(uv.outputs['UV'],mapping.inputs['Vector']);nt.links.new(mapping.outputs['Vector'],tex.inputs['Vector'])
assert others=={o.name:hashlib.sha256(str([tuple(v.co) for v in o.data.vertices]).encode()).hexdigest() for o in bpy.data.objects if o.type=='MESH' and o!=t}
out=r/'edited';out.mkdir(exist_ok=True);bpy.ops.wm.save_as_mainfile(filepath=str(out/'Bastion1QwenFinish.blend'),compress=True);bpy.ops.export_scene.gltf(filepath=str(out/'Bastion1QwenFinish.glb'),export_format='GLB',export_cameras=False,export_lights=False)
result={'plan_sha256':proof['plan_sha256'],'source_glb_sha256':hashlib.sha256((r/'source.glb').read_bytes()).hexdigest(),'output_glb_sha256':hashlib.sha256((out/'Bastion1QwenFinish.glb').read_bytes()).hexdigest(),'changed_vertices':changed,'protected_plateau_changes':0,'topology_uv_other_meshes_preserved':True,'parameters_modified':False,'art_approved':False,'unity_verified':False}
(out/'execution-proof.json').write_text(json.dumps(result,indent=2));print('GENUINE_QWEN_FINISH_EXECUTED',json.dumps(result))
