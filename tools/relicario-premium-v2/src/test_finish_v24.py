"""V2.4 asset and typography regression; does not modify Unity or its runtime."""
from pathlib import Path
from io import BytesIO
import xml.etree.ElementTree as ET
import cairosvg
from PIL import Image
root=Path(__file__).resolve().parents[1]
source=(root/'stat-plaques.js').read_text()
css=(root/'polish.css').read_text()
svg=(root/'assets/overlay_indestructible.svg').read_text()
assert 'plaque-engraving' in source and 'recess-' in source and '--stat-text:' in source
for name in ('common','rare','epic','legendary'):
    assert name+':{light:' in source
assert '@container (min-width:245px)' in css
assert "font-size:4.5cqw" in css
assert 'scale(.88)' in svg
assert ET.fromstring(svg).attrib.get('viewBox')=='0 0 320 512'
mask=Image.open(BytesIO(cairosvg.svg2png(bytestring=svg.encode(),output_width=320,output_height=512))).convert('RGBA').getchannel('A')
for x in range(100,221):
    assert max(mask.getpixel((x,y)) for y in range(512))==0
for y in range(140,451):
    assert max(mask.getpixel((x,y)) for x in range(320))==0
for bbox in ((122,5,198,93),(0,145,55,252),(265,145,320,252),(129,244,192,342),(64,404,256,469)):
    assert mask.crop(bbox).getbbox() is None,bbox
assert mask.crop((240,46,298,103)).getbbox()
print('PASS V2.4: 4 metallic variants, restrained inlay, 5:8 layout and responsive text rule')
