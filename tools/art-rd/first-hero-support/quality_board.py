"""Render-only contact sheet; images are unchanged and camera-matched."""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw

out = Path(__file__).resolve().parents[3] / 'art-source/valoria/production/first-hero-gate-v1/quality-v2'
before = Image.open(out / 'before-hero-matched.png').convert('RGB')
after = Image.open(out / 'after-hero-matched.png').convert('RGB')
assert before.size == after.size == (1280, 960)
board = Image.new('RGB', (2560, 1008), '#111820')
board.paste(before, (0, 48))
board.paste(after, (1280, 48))
draw = ImageDraw.Draw(board)
draw.text((24, 14), 'V1  |  same Blender scene, camera and lighting', fill='white')
draw.text((1304, 14), 'V2  |  reworked geometry and PBR surfaces', fill='white')
board.save(out / 'before-after-matched.png', optimize=True)
files = ['ValoriaHeroGate.blend', 'ValoriaHeroGate.glb', 'ValoriaHeroGate_LOD1.glb',
         'before-hero-matched.png', 'after-hero-matched.png',
         'after-glb-roundtrip.png', 'after-lod1.png', 'before-after-matched.png']
files += [str(p.relative_to(out)) for p in sorted((out / 'textures').glob('*.png'))]
(out / 'file-hashes.json').write_text(json.dumps({name: hashlib.sha256((out / name).read_bytes()).hexdigest()
                                                 for name in files}, indent=2) + '\n')
