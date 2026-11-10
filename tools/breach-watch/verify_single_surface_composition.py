#!/usr/bin/env python3
"""Strict cleanroom 2K source-art gate. Unity-native pixel proof is separate."""
from pathlib import Path
from PIL import Image
import hashlib,json
ROOT=Path('Unity/Assets/Eldoria/BreachWatch')
HUD=(ROOT/'Runtime/BreachWatchHud.cs').read_text()
QA=(ROOT/'Editor/BreachWatchNativeEvidence.cs').read_text()
ART=ROOT/'Resources/VigiliaArt/FreshArt'
manifest=json.loads((ART/'manifest.json').read_text())['files']
names={'building_panel','quest_panel','profile_panel','upgrade_button',
       'resource_chip','wood','stone','food','castle','provisions','swords','raven','helm'}
assert set(manifest)==names,('missing or unexpected artwork',set(manifest)^names)
for name in sorted(names):
    f=ART/(name+'.png')
    raw=f.read_bytes()
    assert hashlib.sha256(raw).hexdigest()==manifest[name]['sha256'],name
    assert (ART/(name+'.png.meta')).exists(),name
    with Image.open(f) as im:
        assert im.mode=='RGBA',name
        assert im.width>=290 and im.height>=190,name
        alpha=im.getchannel('A')
        assert alpha.getextrema()[0]==0 and alpha.getextrema()[1]==255,name
        assert im.getpixel((0,0))[3]<15,name
assert (ART.parent/'FreshArt.meta').exists()
# Each UI surface is an independent NEW sprite, not a crop of the previous
# finished screenshot. No old gilded overlay, brass insert or fullscreen veil.
for src in ('FreshArt/profile_panel','FreshArt/quest_panel',
            'FreshArt/building_panel','FreshArt/upgrade_button',
            'FreshArt/resource_chip','FreshArt/'):
    assert src in HUD,('missing new resource',src)
for obsolete in ('"VigiliaArt/OwnerControls/profile_bar"',
                 '"VigiliaArt/OwnerControls/mission_plate"',
                 '"VigiliaArt/OwnerControls/resource_capsule"',
                 '"VigiliaArt/PremiumSurfaces/panel_fill"',
                 'var outlined=Resources.Load<Sprite>',
                 'var brassFrame=Plate(',
                 'Rect("Soft volcanic atmosphere / no world replacement"'):
    assert obsolete not in HUD,('stacked or old art reused',obsolete)
assert 'exactOwnerGoldBorder=null;' in HUD
assert '!portraitFrame.GetComponent<Graphic>()' in HUD
assert 'Need(!atmosphericVeil,' in QA
assert 'oldGold==0 && oldBrass==0' in QA
assert 'ui.UsesSinglePremiumContext' in QA
# Preserve the real independent canvas, live session, data/input and strict
# native 8-view GPU/canvas tests, rather than declaring quality from Python.
for clause in (
    'legacyCanvas.enabled=false;',
    'legacyGroup.alpha=0;',
    'ExternalPointerBlocked=BlocksNativePointer',
    'resourceAmounts[0].text=RoyalFormatting.Amount(snapshot.Wood);',
    'chapterProgress.text=snapshot.ObjectiveProgress;',
    'buildingPortrait.Initialize(portraitView);',
    'AuditActualCanvases',
    'RealTextContribution',
    'functioningNewNavigationClicks',
    'saveUnchanged',
):
    assert clause in (HUD+'\n'+QA),('lost genuine gameplay proof',clause)
assert 'Glyph(objective,"Chapter stone rune"' not in HUD
assert 'Glyph(panel,"Building crest"' not in HUD
assert HUD.count('AddContactShadow(')==1,'extra UI shadow reintroduced'
assert HUD.count('UseSourceArtInteraction(')==4
print('VIGILIA_CLEANROOM_13_INDEPENDENT_2K_SPRITES_SOURCE_PASS_NATIVE_PENDING')
