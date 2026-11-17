#!/usr/bin/env python3
"""Five-stage I-V art production gate. Never mark 5/5 based on scene screenshots."""
from pathlib import Path
import hashlib,json
root=Path(__file__).resolve().parents[1]
data=json.loads((root/'BASTION_I_V_RECONSTRUCTION_CONTRACT_v1.json').read_text())
m=json.loads((root/'master_manifest_v1.json').read_text())
assert data['schema']==1 and len(data['stages'])==5
assert [s['level'] for s in data['stages']]==[1,2,3,4,5]
assert data['preservation']['main_unchanged'] is True
byfile={x['file']:x for x in m['masters']}
for stage in data['stages']:
 assert len(stage['native_capture_sha256'])==64
 if stage['level'] in (1,5):
  key='Bastion_I_MASTER.png' if stage['level']==1 else 'Bastion_V_MASTER.png'
  ref=root/'Masters'/key
  digest=hashlib.sha256(ref.read_bytes()).hexdigest()
  assert digest==stage['master_sha256']==byfile[key]['sha256']
  assert stage['status']=='FROZEN_ORIGINAL'
 else:
  # MISSING is a technical truth, not a command to fabricate pixel art.
  assert stage['status']=='MISSING_UNIQUE_PREMIUM_SPRITE'
  assert stage['currently_renders']=='Bastion I original master'
count=sum(s['status']=='FROZEN_ORIGINAL' for s in data['stages'])
assert count==2 and data['current_unique_bastion_sprites']==2
assert data['target_unique_bastion_sprites']==5
assert data['stage_2_3_4_art_approved'] is False
assert data['interface_world_phase_allowed'] is False
assert data['visual_reconstruction_complete'] is False
print('NATIVE_I_TO_V_BASELINE_5_HASHES_SAVED; UNIQUE_ART_TIERS_2_OF_5; ART_NOT_APPROVED')
