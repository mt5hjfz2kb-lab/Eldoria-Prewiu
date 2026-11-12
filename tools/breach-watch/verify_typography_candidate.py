#!/usr/bin/env python3
"""Original font bytes/Spanish outline check. NOT Unity import or raster QA."""
import hashlib
import json
from pathlib import Path
from fontTools.ttLib import TTFont
from PIL import ImageFont

root = Path('Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/Fonts')
expected = {
    'SourceSans3-Regular.ttf': 'c5dd0b88cc8ff4edd7ebdcefee0290df15529657',
    'SourceSans3-Semibold.ttf': '4aff9a3aea9b5f1e746f03ed40e1b293740dc8a2',
    'OFL_SOURCE_SANS_3.txt': '22c601b82f29fc6bb801445098c9f23ffdca4f94',
}
guids = []
for name, blob in expected.items():
    data = (root / name).read_bytes()
    assert hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest() == blob, name
    meta = (root / (name + '.meta')).read_text()
    guids.append(next(x.split(': ')[1] for x in meta.splitlines() if x.startswith('guid: ')))
    if name.endswith('.ttf'):
        assert 'includeFontData: 1' in meta and 'fallbackFontReferences: []' in meta
assert len(set(guids)) == len(guids)
license_text = (root / 'OFL_SOURCE_SANS_3.txt').read_text()
assert 'SIL OPEN FONT LICENSE Version 1.1' in license_text and 'Adobe' in license_text
provenance = json.loads((root / 'source-sans3-provenance.json').read_text())
assert provenance['commit'] == '87b37a2daaed80fcb8e8ccb0085c4d72ddade12e'
samples = ['áéíóúüñÁÉÍÓÚÜÑ', '1.234.567 / 8.500', '50% · 00:59',
           'RECLAMAR', 'USAR', 'VOLVER', 'PREPARAR MARCHA']
rows = []
for name in expected:
    if not name.endswith('.ttf'):
        continue
    cmap = TTFont(root / name).getBestCmap()
    for sample in samples:
        assert all(ord(c) in cmap for c in sample if not c.isspace()), (name, sample)
        for size in (14, 16, 18, 22):
            face = ImageFont.truetype(str(root / name), size)
            rows.append({'font': name, 'sample': sample, 'size': size,
                         'outline_width': round(face.getlength(sample), 3),
                         'outline_bbox': face.getbbox(sample)})
print(json.dumps({'method': 'ORIGINAL_FONT_OUTLINES_NOT_UNITY_RASTER', 'rows': rows}))
