"""Regression QA for the real V2.3 indestructible SVG and overlay geometry.
Requires cairosvg and Pillow. This test does not run Unity or claim gameplay validation.
"""
from pathlib import Path
from io import BytesIO
import xml.etree.ElementTree as ET
import cairosvg
from PIL import Image

root=Path(__file__).resolve().parent.parent
svg=root/'assets/overlay_indestructible.svg'
source=svg.read_text(encoding='utf-8')
node=ET.fromstring(source)
assert node.attrib.get('viewBox')=='0 0 320 512'
assert '<rect' not in source and 'M24 19 L296 19' not in source and 'M24 22 L296 22' not in source
assert '<text' not in source, 'Infinity must be drawn as a vector rune'
mask=Image.open(BytesIO(cairosvg.svg2png(bytestring=source.encode(),output_width=320,output_height=512))).convert('RGBA').getchannel('A')
# Central sections of every outer edge are clear: no surrounding green box.
for x in range(100,221):
    assert max(mask.getpixel((x,y)) for y in range(512))==0,('vertical green rectangle',x)
for y in range(140,451):
    assert max(mask.getpixel((x,y)) for x in range(320))==0,('horizontal green rectangle',y)
# Never repaint the numbered plaques or the effect/wood text panel.
for x1,y1,x2,y2 in [(122,5,198,93),(0,145,55,252),(265,145,320,252),(129,244,192,342),(64,404,256,469)]:
    assert mask.crop((x1,y1,x2,y2)).getbbox() is None,(x1,y1,x2,y2)
assert mask.crop((240,46,298,103)).getbbox() is not None, 'Infinity inlaid seal missing'
assert mask.size==(320,512)
css=(root/'polish.css').read_text(encoding='utf-8')
assert '.relic-card.is-indestructible { filter:none; }' in css, 'Green full-frame CSS glow not removed'
print('PASS: same 5:8 geometry, no green outer perimeter, 4 stats and effect clear; infinity seal visible')
