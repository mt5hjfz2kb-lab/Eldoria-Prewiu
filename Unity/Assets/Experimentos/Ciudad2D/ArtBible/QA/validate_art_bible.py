#!/usr/bin/env python3
"""Verifies six frozen user-selected visual masters and reports incomplete pilot honestly."""
import hashlib,json
from pathlib import Path
from PIL import Image
root=Path(__file__).resolve().parents[1]
manifest=json.loads((root/'master_manifest_v1.json').read_text(encoding='utf-8'))
pilot=json.loads((root/'pilot_three_buildings.json').read_text(encoding='utf-8'))
assert manifest['schema']==1 and len(manifest['masters'])==6
assert manifest['approval_scope'].endswith('not_overall_game_art_final')
for m in manifest['masters']:
    path=root/'Masters'/m['file']
    assert path.is_file(),f'Missing master: {path}'
    assert hashlib.sha256(path.read_bytes()).hexdigest()==m['sha256'],f'Source drift: {path}'
    with Image.open(path) as im:
        assert list(im.size)==m['size'] and im.mode=='RGBA',path
assert pilot['schema']==1 and len(pilot['entries'])==3
assert pilot['status']!='ART_APPROVED' and pilot['art_approved'] is False
# Editor RenderTexture gallery is now evidenced by one immutable action run.
# This is NOT PlayMode coverage of any pilot building or iPhone validation.
gallery=pilot.get('unity_integration')=='verified_editor_gallery_only'
if gallery:
    assert pilot.get('unity_gallery_run_id')==44040675403
    assert pilot.get('unity_gallery_source_sha')=='b7b914176821d6400a77fa6953acbc9469f1f1c1'
    assert pilot.get('unity_gallery_capture_count')==8
    assert pilot.get('playmode_for_new_candidates') is False
    assert pilot.get('iPhone_for_new_candidates') is False
else:
    assert pilot['unity_integration']!='verified'
for row in pilot['entries']:
    assert row.get('native_unity_capture') is gallery
    assert row.get('art_assessment'),row
print('VISUAL_MASTER_6_SHA_PASS | PILOT_3_ENTRIES_NOT_ART_CERTIFIED | EDITOR_GALLERY='+str(gallery))
