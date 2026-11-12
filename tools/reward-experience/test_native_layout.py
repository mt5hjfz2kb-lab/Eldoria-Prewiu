#!/usr/bin/env python3
"""Source-level layout regression model for the four native Unity capture sizes.

Not an artistic approval, and not a substitute for actual Unity GPU pixels.
The model guards the mobile clipping/overlap failure previously found in PR187.
"""
from dataclasses import dataclass
from pathlib import Path
import re

SIZES = [(1600, 900), (900, 1600), (844, 390), (390, 844)]
RESOURCES = ["wood", "stone", "food"]
TIERS = [100, 1000, 10000, 100000]


@dataclass
class Box:
    left: float
    bottom: float
    right: float
    top: float

    def overlaps(self, other):
        return self.left < other.right and self.right > other.left and (
            self.bottom < other.top and self.top > other.bottom)


def layout(width, height):
    portrait = width < height
    cols, rows = 4, 3
    gap_x = 7.0 if portrait else 12.0
    slot_width = min(220.0 if portrait else 235.0,
                     (width * (.93 if portrait else .91) - 3 * gap_x) / 4)
    icon = min(96.0, slot_width - 8.0,
               max(48.0, (height - (170.0 if portrait else 100.0)) / 3 - 30.0))
    # Imported card includes its own gold border and lower value plaque.
    row_height = icon + 8.0
    gap_y = 14.0 if portrait else 11.0
    grid_height = 3 * row_height + 2 * gap_y
    top = height * .5 - 74.0
    bottom = -height * .5 + 25.0
    center_y = (top + bottom) * .5
    first_center = center_y + (grid_height - row_height) * .5

    claim_center_x = width * .5 - (70.0 if portrait else 90.0)
    claim_width = 92.0 if portrait else 118.0
    title_left = -width * .5 + 70.0
    title_right = claim_center_x - claim_width * .5 - 16.0
    title = Box(title_left, height*.5 - 52.5, title_right, height*.5 - 7.5)
    claim = Box(claim_center_x - claim_width*.5, height*.5 - 47,
                claim_center_x + claim_width*.5, height*.5 - 13)
    new_badge = Box(-width*.5 + 29, height*.5 - 44,
                    -width*.5 + 57, height*.5 - 16)

    cards = []
    for i in range(12):
        col, row = i % cols, i // cols
        x = (col - 1.5) * (slot_width + gap_x)
        y = first_center - row * (row_height + gap_y)
        rect = Box(x - slot_width*.5, y - row_height*.5,
                   x + slot_width*.5, y + row_height*.5)
        cards.append((RESOURCES[i//4], TIERS[i%4], rect))
    return {
        "icon": icon, "top": top, "bottom": bottom, "title": title,
        "claim": claim, "unread": new_badge, "cards": cards,
        "width": width, "height": height,
    }



def choose_hand(width, height, claim):
    """Exact local-rect bounds for the Unity Image hand at 90/72px.

    The ORIGINAL 386x245 PNG points left; opaque fingertip x62, y72..77.
    RenderTexture/mobile safe rectangle includes a six-pixel inset.
    Try original right, then horizontal mirror, with center/bottom/top
    fingertip contact to avoid clipping header/notch.
    """
    size=72.0 if width <= 480 else 90.0
    hand_height=size*245.0/386.0
    px,py=.16,.696
    safe=Box(-width*.5+6, -height*.5+6,
             width*.5-6,height*.5-6)
    inset_y=min(4.0,(claim.top-claim.bottom)*.12)
    contacts=( (claim.bottom+claim.top)*.5,
               claim.bottom+inset_y,claim.top-inset_y )
    attempted=[]
    for vertical, y in enumerate(contacts):
        for mirrored in (False,True):
            tip_x=claim.left-3 if mirrored else claim.right+3
            if mirrored:
                left,right=tip_x-(1-px)*size,tip_x+px*size
            else:
                left,right=tip_x-px*size,tip_x+(1-px)*size
            box=Box(left,y-py*hand_height,right,y+(1-py)*hand_height)
            fits=(box.left>=safe.left and box.right<=safe.right and
                  box.bottom>=safe.bottom and box.top<=safe.top)
            attempted.append((vertical,mirrored,fits,box))
            if fits:
                return vertical,mirrored,box,attempted
    raise AssertionError(f"Tutorial hand cannot fit safe viewport {width}x{height}")


def verify_hand_geometry(width,height,data):
    contact,mirrored,hand,attempts=choose_hand(width,height,data["claim"])
    claim=data["claim"]
    # All four component fixtures put the action near the right edge.
    # Mirroring is required, while native-aspect height can legitimately
    # allow the first central contact instead of the former square fallback.
    assert mirrored, (width,height,contact,mirrored)
    # A native-aspect hand is shorter than the former square box: center
    # contact can now fit. Require the FIRST safe placement, not a forced
    # old fallback chosen for the obsolete square geometry.
    chosen=next(a for a in attempts if a[2])
    assert chosen[0]==contact and chosen[1]==mirrored
    assert all(not a[2] for a in attempts[:-1]), "Skipped a safe earlier placement"
    center_width=(claim.right-claim.left)*.6
    core_left=(claim.left+claim.right)*.5-center_width*.5
    core_right=(claim.left+claim.right)*.5+center_width*.5
    assert hand.right<=core_left or hand.left>=core_right, (
        width,height,"pointer blocks central button label")
    assert hand.left>=-width*.5+6 and hand.right<=width*.5-6
    assert hand.bottom>=-height*.5+6 and hand.top<=height*.5-6
    print(f"ISSUE186_POINTER_MODEL_PASS {width}x{height} "
          f"mirrored={mirrored} contact={contact} hand={hand}")
    return 6


def verify():
    checks=0
    for width, height in SIZES:
        d=layout(width,height)
        assert 48 <= d["icon"] <= 96, (width,height,d["icon"])
        checks+=1
        assert d["title"].right < d["claim"].left, "Title overlaps CTA"
        checks+=1
        assert d["unread"].right < d["title"].left, "Alert overlaps title"
        checks+=1
        assert d["title"].right-d["title"].left>=150, "Title cannot fit"
        checks+=1
        for i,(resource,tier,rect) in enumerate(d["cards"]):
            assert resource == RESOURCES[i//4] and tier == TIERS[i%4]
            checks+=1
            assert rect.top <= d["top"]+.001 and rect.bottom>=d["bottom"]-.001
            checks+=1
            assert rect.left >= -width*.5 and rect.right <= width*.5
            checks+=1
            assert not rect.overlaps(d["title"]), "Reward card touches heading"
            checks+=1
            assert not rect.overlaps(d["claim"]), "Reward card touches CTA"
            checks+=1
            for j,(_,_,previous) in enumerate(d["cards"][:i]):
                assert not rect.overlaps(previous), f"Cards {i} and {j} overlap"
                checks+=1
        # The real source artwork has a lower 17%-high inset value plaque.
        # Live values must be contained in it, not rendered beneath the card.
        for _,_,rect in d["cards"]:
            x=(rect.left+rect.right)*.5
            y=(rect.bottom+rect.top)*.5
            label=Box(x-d["icon"]*.35, y-d["icon"]*.47,
                      x+d["icon"]*.35, y-d["icon"]*.30)
            assert label.left >= rect.left and label.right <= rect.right
            assert label.bottom >= rect.bottom and label.top <= rect.top
            checks+=2
        # Target area must be large enough for mobile touch; probe CTA only.
        assert d["claim"].right-d["claim"].left>=92
        checks+=1
        checks+=verify_hand_geometry(width,height,d)
        print(f"ISSUE186_LAYOUT_SOURCE_PASS {width}x{height} icon={d['icon']:.1f}px "
              f"4 tiers per resource row, safe CTA/title/alert boundaries")
    # A real UGUI raycast/click tests tap routing, NOT a completed gameplay
    # claim. Canonical gateway claims are tested separately: no real reward
    # result is produced by this disposable art-only callback. Its guide must
    # remain pending until a REAL gateway command succeeds. Do not let source
    # gates contradict that contract or certify fake completion.
    root=Path(__file__).resolve().parents[2]
    editor=(root/"Unity/Assets/Eldoria/Issue186Art/Editor/Issue186NativeEvidence.cs").read_text(encoding="utf-8")
    workflow=(root/".github/workflows/issue186-native.yml").read_text(encoding="utf-8")
    runtime=(root/"Unity/Assets/Eldoria/Issue186Art/Runtime/Issue186ArtKit.cs").read_text(encoding="utf-8")
    assert "eventSystem.RaycastAll(pointerData, pointerHits)" in runtime and \
        "ExecuteEvents.GetEventHandler<IPointerClickHandler>(" in runtime and \
        "pointerHits[0].gameObject) != actualControl.gameObject" in runtime, \
        "A tutorial must not point to a Selectable obscured by any modal raycaster"
    checks+=1
    assert editor.index('new GameObject("Disposable event dispatch"') < \
        editor.index("bool pointed=Issue186ArtKit.BindFirstUsePointer"), \
        "Native fixture must create the real EventSystem before binding the pointer"
    checks+=1
    assert "Runtime tutorial still points at a button hidden by a modal blocker" in editor and \
        "Runtime tutorial did not recover when modal obstruction disappeared" in editor, \
        "Native QA must test real pointer hiding and rediscovery across modal blocker"
    checks+=1
    assert re.search(r"firstUseCompleted\s*=\s*false\s*[,}]",editor), \
        "Art-only tap may not masquerade as authoritative reward completion"
    checks+=1
    assert re.search(r"results\.TrueForAll\([\s\S]*?x\.firstUsePointer\s*&&\s*!x\.firstUseCompleted\s*&&",editor), \
        "Native acceptance must keep first-use tutorial pending without gateway success"
    checks+=1
    assert re.search(r"\$shot\.firstUseCompleted\s+-ne\s+\$false",workflow), \
        "Windows native proof must agree that UI-only click does not complete tutorial"
    checks+=1
    assert re.search(r"GuidedTutorialRules\.ConfirmAction\([\s\S]{0,180}?false\)",editor) and \
        "pointer.gameObject.activeInHierarchy" in editor, \
        "UI art fixture must explicitly reject false-positive action completion"
    checks+=1
    # A raycast list may contain a valid Button behind an intercepting
    # Graphic. Searching all hits for that Button (instead of the first)
    # gave a false "real tap" PASS. Demand a negative-control blocker and
    # then a genuinely frontmost Claim Button hit on the same native canvas.
    assert "hits.Find(" not in editor, \
        "Searching any raycast hit can certify an occluded, untappable button"
    checks+=1
    assert re.search(r"blocker\.raycastTarget\s*=\s*true;[\s\S]{0,250}?raycaster\.Raycast\(pd,hits\);[\s\S]{0,180}?hits\[0\]\.gameObject==blocker\.gameObject",editor), \
        "Native QA must first establish a topmost blocker at the real tap coordinates"
    checks+=1
    assert "Need(!action,\"Claim button clicked through the blocking overlay\")" in editor, \
        "Occluded pointer dispatch must be proven not to call Claim"
    checks+=1
    assert re.search(r"blocker\.gameObject\.SetActive\(false\);[\s\S]{0,250}?raycaster\.Raycast\(pd,hits\);[\s\S]{0,180}?hits\[0\]\.gameObject==buttonRect\.gameObject",editor), \
        "Native QA must re-test the frontmost real Button after removing the blocker"
    checks+=1
    assert "x.blockedUiClickRejected" in editor and \
        "$shot.blockedUiClickRejected -ne $true" in workflow, \
        "Four-viewpoint native report and Windows CI must require blocker rejection"
    checks+=1
    assert 'float rowH=icon+8f;' in editor and \
        'float gapY=portrait?14f:11f;' in editor, \
        "Native capture must match plaque-aware source geometry"
    checks+=1
    assert 'Paint(root,"dark cartridge "' not in editor and \
        'new Vector2(x,y-icon*.385f)' in editor and \
        'new Vector2(plaqueWidth,icon*.17f)' in editor, \
        "Imported cards already have a border and painted plaque: no duplicate overlays"
    checks+=1
    assert "quantity.preferredWidth<=plaqueWidth-2f" in editor, \
        "Native Unity must measure text to prevent spill outside the real label plaque"
    checks+=1
    # Native script exits on normal return, but not if compilation fails before
    # executeMethod runs. Require an explicit batch quit to free the scarce PC.
    assert "-batchmode -quit -force-d3d11" in workflow, (
        "ISSUE186_NATIVE_QUIT_GUARD: Unity batch must exit on compile failure")
    checks+=1
    print(f"ISSUE186_LAYOUT_MODEL_PASS checks={checks}; SOURCE ONLY, UNITY PIXELS PENDING")


if __name__ == "__main__":
    verify()

