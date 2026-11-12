#!/usr/bin/env python3
"""Protect real three-space March composition at both mobile orientations.

Source contract only. Unity native render and pointer QA are separate.
"""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
FILE = ROOT / "Unity/Assets/Eldoria/BreachWatch/Runtime/BreachWatchMarchComposition.cs"
QA = ROOT / "Unity/Assets/Eldoria/BreachWatch/Editor/BreachWatchDestinationNativeEvidence.cs"


def require(yes, reason):
    if not yes:
        raise AssertionError(reason)


def main():
    text = FILE.read_text(encoding="utf-8")
    scorecard = text.split("public bool RefreshScorecard()", 1)[1].split(
        "static RectTransform FindPortrait", 1
    )[0]
    portraits = text.split("bool RefreshActualHeroCards(RectTransform root)", 1)[1].split(
        "public void Restore()", 1
    )[0]
    restore = text.split("public void Restore()", 1)[1]
    require('if(root.rect.width>root.rect.height)' in scorecard and
            'RefreshActualHeroCards(root);' in scorecard,
            "Landscape layout must reclaim the same real three hero spaces")
    require('bool landscape=root.rect.width>root.rect.height;' in portraits,
            "Hero composition must adapt without portrait-only gate")
    require('third.interactable=' not in portraits,
            "Presentation must never change actual third-slot availability")
    require('heroSlots[2].gameObject.SetActive(true);' in portraits,
            "Owner requests a visible third hero space")
    require('Remember(card);' in portraits and
            'Remember(heroViewports[i]);' in portraits and
            'Remember(heroLabels[i]);' in portraits,
            "Reversible canonical hero buttons and portrait layout required")
    require('heroSlots[2].gameObject.SetActive(thirdHeroWasActive);' in restore,
            "Restore third slot exactly when the destination closes")
    require('label.horizontalOverflow=HorizontalWrapMode.Wrap;' in portraits and
            'label.verticalOverflow=VerticalWrapMode.Truncate;' in portraits and
            'label.resizeTextForBestFit=false;' in portraits,
            "Compact landscape hero labels may not spill past the actual card")
    require('originalLabels.Clear();' in restore and
            'label.horizontalOverflow=old.Horizontal;' in restore,
            "Original canonical hero typography must be restored on exit")
    native = QA.read_text(encoding="utf-8")
    require('RealMarchThreeSpaceRoster' in native,
            "Native QA must check the actual landscape and portrait roster")
    print("VIGILIA_MARCH_THREE_HERO_SPACES_ALL_ORIENTATIONS_SOURCE_PASS")
    print("Native 390x844, 900x1600, 844x390 and 1600x900 still required")


if __name__ == "__main__":
    main()
