#!/usr/bin/env python3
"""Validate the self-contained premium 2.5D prop cut-outs; not a Unity test."""
from pathlib import Path
from hashlib import sha256
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
PROPS = {
 'ArbolAntiguo.png': ((380,340),'263516708e6fede98d537cff871b07e98c0074fe2923eabc4919eba659c4bb1b'),
 'MatojosFlores.png': ((415,258),'5d25f70b346b6b4c3c4de776e697f4f2db724d59bcb807ec5fb0f005598ee114'),
 'MuroDerruido.png': ((442,271),'c2de297a5e560ad0f209d5bb585b7dfb441c63b591da58d1bc08751080b203ad'),
 'CarroMadera.png': ((406,319),'27c3f6abd18f18cdbf902c603a37aa11abfaee81a2f245b661fcca350ba20f0c'),
 'PozoPiedra.png': ((349,334),'17bc3648a4455e1fb62a065e10caf29620cd477590412b072192a7a359057690'),
 'AlmacenBarriles.png': ((413,241),'39fdcf000a82299d220a1b0125e54fde825f9172e0227bbad3e0d06068530737'),
 'VallaLenador.png': ((458,285),'81cbda8363fb09a91979d92fb24d83032d8b329cb52fd4fa5fa49321ffd6b069'),
 'AntorchasGuardian.png': ((367,314),'e82c06881a0edad65f0c70c267f3a56448c032286ce462a4609905295f46cd17'),
 'FisuraBrecha.png': ((420,225),'0704007cd027a8533c3514566e109588d321ea5753921e2da17d878fbcf40c53'),
}
for name,(dims,digest) in PROPS.items():
 path=ROOT/'Sprites'/'Props'/name
 assert path.is_file(), f'Missing {name}'
 assert sha256(path.read_bytes()).hexdigest()==digest, name
 with Image.open(path) as im:
  assert im.size==dims, (name,im.size)
  assert im.mode=='RGBA', (name,im.mode)
  assert im.getchannel('A').getextrema()[0] < 30, name
  assert im.getchannel('A').getextrema()[1] > 220, name
import json
layout=json.loads((ROOT/'QA'/'city_layout.json').read_text(encoding='utf-8'))
configured={x['filename'] for x in layout['decor']}
for name in PROPS:
 assert 'Props/'+name in configured, name
builder=(ROOT/'Scripts'/'Editor'/'City2DSceneBuilder.cs').read_text(encoding='utf-8')
assert 'JsonUtility.FromJson<Layout>' in builder
assert 'AddComponent<BoxCollider2D>()' in builder
print('DECOR_OFFLINE_PASS: 9 transparent cutouts, reproducible SHA256/dimensions, independently placed Unity sprites')
