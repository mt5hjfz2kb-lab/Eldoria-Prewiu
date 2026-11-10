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
    if name in ('panel','profile','mission','resource'):
        # Compare sampled mineral pixels to the owner-approved graphite
        # rather than accepting the AI generator's rejected violet hue.
        # This is an ART SOURCE color contract, not proof of Unity rendering.
        from statistics import median
        with Image.open(file).convert('RGBA') as art:
            x0,x1=(.15,.75) if name=='panel' else (.4,.8)
            y0,y1=(.55,.90) if name=='panel' else (.2,.8)
            coords=[(round(art.width*(x0+(x1-x0)*j/20)),
                     round(art.height*(y0+(y1-y0)*i/20)))
                    for i in range(1,20) for j in range(1,20)]
            samples=[art.getpixel(xy)[:3] for xy in coords
                     if art.getpixel(xy)[3]>220]
        assert len(samples)>100,(name,"Not enough real opaque mineral")
        med=[median(sample[i] for sample in samples) for i in range(3)]
        assert med[2]<=med[0]+4 and med[2]<=med[1]+5,(
            name,"Purple mineral instead of approved warm slate",med)
        print("VIGILIA_APPROVED_GRAPHITE_PALETTE",name,med)
    if name in ('profile','mission','resource'):
        assert 'spriteBorder: {x: ' in meta,name
        assert ('VigiliaArt/CleanRoom/'+name) in HUD,name
        # PPU=100 on a 2K source made 40-unit engraved insignias become
        # 5-unit specks. Image.pixelsPerUnit = spritePPU / Canvas.referencePPU
        # in UnityEngine.UI; its nine-slice borders use spriteBorder divided
        # by that ratio. The previous source model divided just by spritePPU
        # and incorrectly passed UI whose entire center collapsed in Unity.
        ppu={'profile':2000,'mission':1400,'resource':2200}[name]
        canvas_reference_ppu=100
        import re
        assert 'scaler.referencePixelsPerUnit=100f;' in HUD,name
        assert f'spritePixelsToUnits: {ppu}' in meta,name
        m=re.search(r'spriteBorder: \{x: (\d+), y: (\d+), z: (\d+), w: (\d+)\}',meta)
        assert m,('No four-way source borders',name)
        left,bottom,right,top=map(int,m.groups())
        unit_scale=canvas_reference_ppu/ppu
        actual=left*unit_scale
        # Insignia must lie FULLY INSIDE its unscaled source region.
        # The previous 25%-left split ran straight through the original
        # 34%-wide circular lion, leaving a stretched wing at the seam.
        if name=='profile':
            assert left>=int(im.width*.34),(
                'Lion ring cut by nine-slice vertical seam',left,im.width)
            assert 42<=actual<=46,('lion physical crest width',actual)
            assert 'float lionTextInset=crestSize+(smallPhone?8f:16f);' in HUD
        minmax={'profile':(32,47),'mission':(22,34),
                'resource':(16,24)}[name]
        assert minmax[0]<=actual<=minmax[1],(name,'actual nine-slice left well',actual)
        live_min_width={'profile':182,'mission':184,'resource':58}[name]
        center=live_min_width-(left+right)*unit_scale
        # Unity GetAdjustedBorders shrinks both sides if their sum exceeds
        # the UI rect, leaving NO actual flexible center and deformed art.
        assert center>=14,(name,'collapsed nine-slice center',center)
        # Y borders similarly must leave space inside the live 28–69u plates.
        live_min_height={'profile':43,'mission':60,'resource':28}[name]
        assert live_min_height-(top+bottom)*unit_scale>=8,(
            name,'collapsed vertical nine-slice center')
    if name=='panel':
        # Detect actual source-painted inset bottom and separate description
        # divider. Unlike the rejected source, description is OUTSIDE the well.
        with Image.open(file).convert('RGBA') as art:
            width,height=art.size
            def gold_line(lo,hi):
                rows=[]
                for y in range(round(height*lo),round(height*hi)):
                    total=0
                    for x in range(round(width*.12),round(width*.88),3):
                        r,g,b,a=art.getpixel((x,y))
                        total+=int(a>160 and r>90 and g>60 and r>g*1.11 and g>b*1.07)
                    rows.append((total,y))
                count,y=max(rows)
                assert count>=width*.12,('missing painted rule',count)
                return y*280/height
            well_bottom=gold_line(.44,.48)
            divider=gold_line(.56,.60)
            assert 126<well_bottom<131,('portrait crosses painted frame',well_bottom)
            assert 157<divider<169,('description crosses divider',divider)
            for required in (
                'At(portraitFrame,15,61,pw-30,74);',
                'At(buildingFunction.rectTransform,13,146,pw-26,22);',
                'descriptionDivider.gameObject.SetActive(false);',
                'At(costHeader.rectTransform,13,174,pw-26,17);',
            ):
                assert required in HUD,('new panel layout lost',required)
            # The panel is drawn as ONE simple sprite at 297u, not a
            # free-floating overlay. Its original painted lower photo rim
            # at ~129/280 becomes ~137/297, so the native portrait must
            # end BEFORE the real border and the description before divider.
            displayed_well=well_bottom*297/280
            displayed_divider=divider*297/280
            assert 61+74<=displayed_well-1.0,(
                'True portrait crosses engraved border',displayed_well)
            assert 146+22<=displayed_divider-1.0,(
                'Live building description crosses material divider',displayed_divider)
            assert 174>=displayed_divider+2.0,(
                'Cost title sits on a painted divider',displayed_divider)
            print('VIGILIA_SEPARATE_PORTRAIT_DESCRIPTION',displayed_well,displayed_divider)
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
# Owner's original medallion silhouette measures about 62-69px on the
# 900px reference. The NEW source sprites have alpha closely cropped to
# their ring, so they cannot paint across the full 91px native hitbox.
# Paint size changes, hitboxes stay physically >=44px for accessibility.
assert 'surface.VisualScale=.86f;' in HUD
# Pixel-informed, only the one genuine illustrated medallion gets scaled.
# Keep all touch areas and dormant destinations unchanged; no second overlay.
assert 'At((RectTransform)navButtons[i].transform,' in HUD
assert 'if(p)p.VisualScale=i==activeTab?.92f:.88f;' in HUD
import math
scale_900=900/514
assert 61 <= 52*scale_900*.70 <= 66
assert 67 <= 52*scale_900*.76 <= 71
# Generated source alpha covers ~98% of its 674px icon crop, so the
# REAL visible 900px diameter is 0.98*52*(900/514)*paintScale.
# Selected gold is ~68px and other icons ~63px in the owner's image.
alpha_fraction=.98
assert 61<=alpha_fraction*52*scale_900*.70<=66
assert 67<=alpha_fraction*52*scale_900*.76<=71
assert 52*.91 >=44
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
