#!/usr/bin/env python3
"""Read-only acceptance for ONE clean, reference-guided UI skin.
This validates source provenance, alpha and single-surface architecture.
Real Unity screenshots and owner's artistic approval remain separate.
"""
from pathlib import Path
from PIL import Image
import hashlib,json

ROOT=Path('Unity/Assets/Eldoria/BreachWatch')
HUD=(ROOT/'Runtime/BreachWatchHud.cs').read_text()
QA=(ROOT/'Editor/BreachWatchNativeEvidence.cs').read_text()
ART=ROOT/'Resources/VigiliaArt/CleanRoom'
m=json.loads((ART/'manifest.json').read_text())
asset_names=('panel','mission','profile','resource','upgrade','nav_castle',
             'nav_provisions','nav_swords','nav_raven','nav_helm')
assert m['output_count']==10
assert m['reference_pixels_reused'] is False
assert m['legacy_hud_asset_reused'] is False
entries={x['asset']:x for x in m['assets']}
assert set(entries)==set(asset_names),set(asset_names)^set(entries)
expected_ratios={
    'panel':(.38,1.1),'mission':(2.6,5.2),'profile':(2.4,5.0),
    'resource':(2.4,6.5),'upgrade':(3.0,6.5)
}
for name in asset_names:
    file=ART/(name+'.png')
    assert file.exists(),name
    assert hashlib.sha256(file.read_bytes()).hexdigest()==entries[name]['sha256'],name
    meta=(ART/(name+'.png.meta')).read_text()
    assert 'textureType: 8' in meta and 'spriteMode: 1' in meta,name
    with Image.open(file) as im:
        assert im.mode=='RGBA',name
        assert im.width>=300 and im.height>=160,name
        a=im.getchannel('A')
        assert a.getextrema()==(0,255),name
        assert im.getpixel((0,0))[3]<12,name
        aspect=im.width/im.height
        lo,hi=expected_ratios.get(name,(.81,1.2))
        assert lo<aspect<hi,(name,aspect)
    if name in ('profile','mission','resource'):
        assert 'spriteBorder: {x: ' in meta,name
        assert ('VigiliaArt/CleanRoom/'+name) in HUD,name
for name in ('panel','mission','profile','resource','upgrade'):
    assert 'VigiliaArt/CleanRoom/'+name in HUD,name
assert '"VigiliaArt/CleanRoom/nav_"+symbol' in HUD
assert '"VigiliaArt/CleanRoom/nav_"+ReferenceNavArt[i]' in HUD
# The old painted profile, resource, quest, buttons and panel may stay in
# version control for audit, but must be visually ABSENT from the new Canvas.
for path in ('FreshArt/profile_panel','FreshArt/quest_panel',
             'FreshArt/building_panel','FreshArt/resource_chip',
             'FreshArt/upgrade_button','OwnerReference/lion_crest',
             'OwnerControls/profile_bar','PremiumSurfaces/panel_fill'):
    assert path not in HUD,path
for legacy in ('var brassFrame=Plate(', 'Glyph(objective,"Chapter stone rune"',
               'Rect("Soft volcanic atmosphere / no world replacement"',
               'Rect("Independent circular realm crest",identity)'):
    assert legacy not in HUD,legacy
assert 'identityCrest=null;' in HUD
assert 'identity.GetComponentsInChildren<Graphic>(true).Length==3' in HUD
assert 'exactOwnerGoldBorder=null;' in HUD
assert '!portraitFrame.GetComponent<Graphic>()' in HUD
assert 'Need(!atmosphericVeil,' in QA
assert 'oldGold==0 && oldBrass==0' in QA
assert 'ui.UsesSinglePremiumContext' in QA
# These fields prove the source remains a functional game, not a static
# composited reference bitmap or invisible placeholder.
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
    assert clause in HUD+'\n'+QA,('lost real gameplay',clause)
assert HUD.count('AddContactShadow(')==1
assert HUD.count('UseSourceArtInteraction(')==4
print('VIGILIA_NEW_REFERENCE_GUIDED_10_ORIGINAL_SPRITES_INDEPENDENT_HUD_PASS')
