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
    row_height = icon + 23.0
    gap_y = 10.0 if portrait else 7.0
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
        # Target area must be large enough for mobile touch; probe CTA only.
        assert d["claim"].right-d["claim"].left>=92
        checks+=1
        print(f"ISSUE186_LAYOUT_SOURCE_PASS {width}x{height} icon={d['icon']:.1f}px "
              f"4 tiers per resource row, safe CTA/title/alert boundaries")
    # Contract-crosscheck the genuine C# result/acceptance and the Windows
    # workflow. A prior version returned firstUseCompleted=true and then
    # demanded !x.firstUseCompleted, making ANY native Unity capture fail
    # even after a real successful button click. Pure geometry missed it.
    root=Path(__file__).resolve().parents[2]
    editor=(root/"Unity/Assets/Eldoria/Issue186Art/Editor/Issue186NativeEvidence.cs").read_text(encoding="utf-8")
    workflow=(root/".github/workflows/issue186-native.yml").read_text(encoding="utf-8")
    assert re.search(r"firstUseCompleted\s*=\s*true\s*[,}]",editor), "Capture must record a successful action"
    checks+=1
    assert re.search(r"results\.TrueForAll\([\s\S]*?x\.firstUsePointer\s*&&\s*x\.firstUseCompleted\s*&&",editor), \
        "Native acceptance cannot invert a completed first-use click"
    checks+=1
    assert re.search(r"\$shot\.firstUseCompleted\s+-ne\s+\$true",workflow), \
        "Native Windows workflow must require the completed click, not an absent action"
    checks+=1
    print(f"ISSUE186_LAYOUT_MODEL_PASS checks={checks}; SOURCE ONLY, UNITY PIXELS PENDING")


if __name__ == "__main__":
    verify()
