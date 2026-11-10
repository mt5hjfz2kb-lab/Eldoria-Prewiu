#!/usr/bin/env python3
"""Check the original high-resolution, isolated mission plate used by the cleanroom HUD."""
from pathlib import Path
from PIL import Image
import hashlib,json
root=Path('Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/FreshArt')
source=root/'quest_panel.png'
assert source.exists() and (root/'quest_panel.png.meta').exists()
info=json.loads((root/'manifest.json').read_text())['files']['quest_panel']
assert hashlib.sha256(source.read_bytes()).hexdigest()==info['sha256']
with Image.open(source) as image:
    image=image.convert('RGBA')
    assert image.width>1000 and image.height>=330,image.size
    assert image.getpixel((0,0))[3]<16,'chroma edge not removed'
    assert image.getpixel((image.width//2,image.height//2))[3]>230,'empty in-game content panel'
metadata=(root/'quest_panel.png.meta').read_text()
assert 'textureType: 8' in metadata
assert 'alphaIsTransparency: 1' in metadata
hud=Path('Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs').read_text()
assert 'VigiliaArt/FreshArt/quest_panel' in hud
assert 'tablet.sprite.name=="quest_panel"' in hud
assert 'chapterStone.ApplyIndependentArt(angledPlate);' in hud
assert 'chapterStone.preserveAspect=false;' in hud
assert 'Glyph(objective,"Chapter stone rune"' not in hud
assert 'realChapter.ToUpperInvariant()' in hud
assert 'chapterProgress.text=snapshot.ObjectiveProgress;' in hud
assert 'VigiliaArt/OwnerControls/mission_plate' not in hud
print('VIGILIA_FRESH_QUEST_2K_ALPHA_LIVE_TEXT_PASS native image pending')
