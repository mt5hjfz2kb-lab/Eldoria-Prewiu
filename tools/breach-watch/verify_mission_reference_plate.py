#!/usr/bin/env python3
"""Validate exact owner-approved mission plate and genuine dynamic Unity content.

The image was extracted from the user's accepted 864x1536 concept, NOT
redesigned by AI or captured from the running game. This proves provenance
and surface integrity; exact-head Unity screenshots are still mandatory.
"""
from pathlib import Path
from PIL import Image
import hashlib

root=Path('Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/OwnerControls')
source=root/'mission_plate.png'
metadata=(root/'mission_plate.png.meta').read_text()
assert source.is_file()
assert hashlib.sha256(source.read_bytes()).hexdigest()==(
    '067568edfe8d59a0087e733131257e3aafe537df30ab8248e728069f7a0d5e2a')
with Image.open(source).convert('RGBA') as art:
    assert art.size==(303,100)
    assert art.getpixel((0,0))[3]<16
    assert art.getpixel((art.width//2,art.height//2))[3]>230
    assert art.getchannel('A').getextrema()==(0,255)
assert 'textureType: 8' in metadata
assert 'spriteBorder: {x: 75, y: 12, z: 40, w: 16}' in metadata
assert 'spritePixelsToUnits: 150' in metadata

hud=Path('Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchHud.cs').read_text()
qa=Path('Unity/Assets/Eldoria/BreachWatch/Editor/BreachWatchNativeEvidence.cs').read_text()
for snippet in (
    'VigiliaArt/OwnerControls/mission_plate',
    'chapterStone.ApplyIndependentArt(angledPlate);',
    'chapterStone.preserveAspect=true;',
    'tablet.type==Image.Type.Simple && tablet.preserveAspect;',
    'chapterObjective.verticalOverflow=VerticalWrapMode.Overflow;',
    'chapterProgress.verticalOverflow=VerticalWrapMode.Overflow;',
    'chapterProgress.text=snapshot.ObjectiveProgress;',
    'realChapter.ToUpperInvariant()',
):
    assert snippet in hud,snippet
assert 'Glyph(objective,"Chapter stone rune"' not in hud
assert 'ui.HasPremiumCarvedQuest' in qa
assert 'oldCanvasVisible' in qa
assert 'VigiliaArt/CleanRoom/mission' not in hud
print('VIGILIA_EXACT_OWNER_MISSION_SHA256_AND_REAL_UNITY_TEXT_SOURCE_PASS — no artistic approval')
