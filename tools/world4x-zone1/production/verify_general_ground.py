"""Independent source geometry gate and normal refresh, never an artistic approval."""
import argparse, hashlib, json, pathlib, sys
import bpy
p=argparse.ArgumentParser();p.add_argument('--before',type=pathlib.Path,required=True);p.add_argument('--after',type=pathlib.Path,required=True);p.add_argument('--out',type=pathlib.Path,required=True)
a=p.parse_args(sys.argv[sys.argv.index('--')+1:]);a.out.mkdir(parents=True,exist_ok=False)
bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(a.before.resolve()))
def ground():
 objects=[o for o in bpy.data.objects if o.type=='MESH' and not o.name.startswith('_EldoriaReview')]
 if len(objects)!=1:raise ValueError('One continuous source terrain required; no props or platforms')
 return objects[0]
original=ground();before=[list(original.matrix_world @ v.co) for v in original.data.vertices]
bpy.ops.wm.open_mainfile(filepath=str(a.after.resolve()));candidate=ground()
after=[list(candidate.matrix_world @ v.co) for v in candidate.data.vertices]
errors=[]
if len(before)!=len(after):errors.append('Topology changed; protected pads cannot be verified by original vertex identity')
else:
 for i,(b,c) in enumerate(zip(before,after)):
  if abs(b[0]-c[0])>1e-4 or abs(b[1]-c[1])>1e-4:errors.append('XY placement changed');break
 for b,c in zip(before,after):
  protected=abs(b[0])>=118 or abs(b[1])>=118 or (-40<=b[0]<=4 and -52<=b[1]<=-8)
  if protected and abs(b[2]-c[2])>1e-4:errors.append('Border or city pad changed');break
 if not any(abs(b[2]-c[2])>1e-4 for b,c in zip(before,after)):errors.append('No actual terrain relief change')
proof={'source_sha256':hashlib.sha256(a.before.read_bytes()).hexdigest(),
 'artist_blend_sha256':hashlib.sha256(a.after.read_bytes()).hexdigest(),
 'independent_geometry_gate_passed':not errors,'errors':errors,'art_approved':False,'unity_executed':False}
(a.out/'independent-ground-gate.json').write_text(json.dumps(proof,indent=2))
if errors:raise ValueError('; '.join(errors))
coordinates=[list(v.co) for v in candidate.data.vertices]
if candidate.data.has_custom_normals:
 candidate.data.normals_split_custom_set([(0.,0.,0.)]*len(candidate.data.loops));candidate.data.update()
assert coordinates==[list(v.co) for v in candidate.data.vertices]
bpy.ops.wm.save_as_mainfile(filepath=str((a.out/'normal-fixed-ground.blend').resolve()))
bpy.ops.object.select_all(action='DESELECT');candidate.select_set(True);bpy.context.view_layer.objects.active=candidate
bpy.ops.export_scene.gltf(filepath=str((a.out/'general-ground-candidate.glb').resolve()),export_format='GLB',use_selection=True)
proof['normal_refresh_geometry_preserved']=True
proof['candidate_sha256']=hashlib.sha256((a.out/'general-ground-candidate.glb').read_bytes()).hexdigest()
proof['changed_vertices']=sum(abs(b[2]-c[2])>1e-4 for b,c in zip(before,after))
(a.out/'independent-ground-gate.json').write_text(json.dumps(proof,indent=2))
