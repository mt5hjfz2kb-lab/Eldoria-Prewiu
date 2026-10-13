"""Recover artist RGB factors lost by Blender's MixRGB-to-glTF exporter. No new art decision."""
import argparse,hashlib,json,pathlib,struct
p=argparse.ArgumentParser();p.add_argument('--root',type=pathlib.Path,required=True);a=p.parse_args();r=a.root
planfile=r/'artist-plan.json';plan=json.loads(planfile.read_text());proof=json.loads((r/'inference-proof.json').read_text());assert hashlib.sha256(planfile.read_bytes()).hexdigest()==proof['plan_sha256']
raw=r/f"raw-attempt-{proof['attempt']}.json";assert hashlib.sha256(raw.read_bytes()).hexdigest()==proof['raw_sha256'];assert json.loads(json.loads(raw.read_text())['message']['content'])==plan
src=r/'edited/Bastion1QwenFinish.glb';b=src.read_bytes();assert hashlib.sha256(b).hexdigest()=='731799f563ce1cf731d2226767585eaaebd1c094712f26d45a1c95653f08bbc9'
n=struct.unpack_from('<I',b,12)[0];d=json.loads(b[20:20+n]);original=json.loads(json.dumps(d));changes=[]
for m in d['materials']:
 name=m['name'];kind='grass_rgb' if 'terrace earth olive grass' in name else 'cliff_rgb' if 'schist dark fractured cliff' in name else None
 if not kind:continue
 pbr=m['pbrMetallicRoughness'];assert pbr.get('baseColorFactor',[1,1,1,1])==[1,1,1,1], 'Unexpected baked source factor, never double-multiply'
 factor=plan['parameters'][kind]+[1];pbr['baseColorFactor']=factor;changes.append({'material':name,'original_factor':[1,1,1,1],'artist_factor':factor})
assert len(changes)==2
# Verify only two factor fields changed; mesh/BIN, images, UV mappings and all
# other fields remain byte-for-byte/semantically identical.
check=json.loads(json.dumps(d))
for m in check['materials']:
 if any(c['material']==m['name'] for c in changes):m['pbrMetallicRoughness'].pop('baseColorFactor')
assert check==original
chunk=json.dumps(d,separators=(',',':')).encode();chunk+=b' '*((-len(chunk))%4);tail=b[20+n:]
out=r/'factor-corrected';out.mkdir(exist_ok=True)
new=struct.pack('<4sII',b'glTF',2,20+len(chunk)+len(tail))+struct.pack('<I4s',len(chunk),b'JSON')+chunk+tail
f=out/'Bastion1QwenFinish.glb';f.write_bytes(new)
assert new[20+len(chunk):]==tail
report=json.loads((r/'edited/execution-proof.json').read_text());report.update({'exported_glb_before_factor_repair_sha256':hashlib.sha256(b).hexdigest(),'output_glb_sha256':hashlib.sha256(new).hexdigest(),'factor_export_repair':changes,'mesh_and_texture_binary_unchanged':True,'artist_parameters_modified':False,'art_approved':False,'unity_verified':False})
(out/'execution-proof.json').write_text(json.dumps(report,indent=2)+'\n');print('ORIGINAL_ARTIST_RGB_FACTORS_TRANSFERRED',json.dumps(report))
