#!/usr/bin/env python3
"""Source-exact V3 isolation and contact layer gate, NOT approval of artwork."""
import hashlib,json
from pathlib import Path
from PIL import Image
BASE=Path(__file__).resolve().parents[2]
PILOT=BASE/'ArtBible/Pilots/forja_guardia/v3'
ASSETS=BASE/'Sprites/AIPilot/Versions'
src=PILOT/'source_original_rgba.png'
candidate=ASSETS/'Forja_Guardia_RGBA_v3.png'
shadow=ASSETS/'Forja_Guardia_ContactShadow_v3.png'
expected={
 src:'1fb781c22a60ae61ce4a3cb28d6f394be3b15d8d344ca5e48a3afbd37877bd63',
 candidate:'ed42684004cd891c83d933d38b195fb794f5db8449bb4436b2c043a7ab8c99ad',
 shadow:'24bcd1e5ec6abbbf5ce3455632b3728a5a732a2057a46d17625483a5167c8e80'}
for file,hash_value in expected.items():
 assert file.is_file() and hashlib.sha256(file.read_bytes()).hexdigest()==hash_value,str(file)+' hash drift'
original=Image.open(src)
art=Image.open(candidate)
layer=Image.open(shadow)
assert original.mode=='RGBA' and original.size==(1536,1024)
assert art.mode=='RGBA' and layer.mode=='RGBA' and art.size==layer.size==(1433,1433)
a=art.getchannel('A')
s=layer.getchannel('A')
assert a.getextrema()==(0,254),a.getextrema()
assert s.getextrema()==(0,91),s.getextrema()
assert all(a.getpixel(q)==0 and s.getpixel(q)==0 for q in ((0,0),(1432,0),(0,1432),(1432,1432)))
assert max(s.crop((0,0,1433,1000)).getextrema()) <= 20,'Shadow above visual foot'
assert (ASSETS/'Forja_Guardia_RGBA_v3.png.meta').exists()
assert (ASSETS/'Forja_Guardia_ContactShadow_v3.png.meta').exists()
man=json.loads((PILOT/'asset_manifest.json').read_text())
sm=json.loads((PILOT/'shadow_manifest.json').read_text())
assert man['candidate_sha256']==expected[candidate] and man['art_approved'] is False
assert sm['shadow_sha256']==expected[shadow] and sm['art_approved'] is False
assert sm['real_physical_shadow'] is False
print('FORJA_V3_SOURCE_CANDIDATE_AND_INDEPENDENT_CONTACT_LAYER_SHA_VERIFIED | ART_NOT_APPROVED')
