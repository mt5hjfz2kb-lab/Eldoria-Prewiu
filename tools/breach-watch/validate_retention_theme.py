#!/usr/bin/env python3
"""Source-only guard. Vigilia must replace original Vigilants/Scouts chrome,
not overlay another screen or change the game command, rewards or save.
"""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
R=ROOT/"Unity/Assets/Eldoria/BreachWatch/Runtime"
T=(R/"BreachWatchRetentionTheme.cs").read_text()
B=(R/"BreachWatchRetentionBridge.cs").read_text()
H=(R/"BreachWatchHud.cs").read_text()
def need(ok,reason):
    if not ok:raise AssertionError(reason)
def main():
    for name in ("BreachWatchRetentionTheme","BreachWatchRetentionBridge"):
        need((R/(name+".cs.meta")).exists(),name+" missing Unity import metadata")
    need("RoyalGoldV2FrameGraphic" in T and
         "RoyalGoldV3RetentionActionGraphic" in T and
         "old.enabled=false" not in T and
         "original.enabled=false" in T,
         "Original Royal frame and action paintings must be disabled")
    need("oldTargets" in T and "oldTransitions" in T and
         "OldRoyalChromeHidden" in T and "Restore()" in T,
         "Keep canonical touch targets and reversible chrome")
    need("RoyalGoldV3EarlyRetentionScreens retention" in B and
         "retention.Open(destination)" in B and
         "retention.VisibleScreen" in B and
         "retention.SynchronizeLayout()" in B and
         "root.SetParent(canvas.transform,false)" in B and
         "root.SetParent(parent,false)" in B,
         "Use the ONE real Vigilants/Scouts screen and restore original parent")
    need('GetField(' in B and '"safe",BindingFlags.NonPublic|BindingFlags.Instance' in B and
         "actualSafeField.SetValue(retention,root)" in B and
         "actualSafeField.SetValue(retention,canonicalSafe)" in B and
         "OwnsActualSafeViewport" in B and "SynchronizeVisual()" in B,
         "Canonical retention safe rect must be temporarily redirected to actual new Canvas; never cropped from old landscape viewport")
    # The grading changes the SAME original illustrated source RawImage;
    # a full-screen extra Image/Canvas would visually resurrect the
    # old pasted-on layer and could obscure real hit targets.
    need("scenicPainting.color=new Color(.78f,.77f,.95f" in T and
         "scenicPainting.color=originalScenicColor" in T and
         "if(scoutsDestination)" in T and
         "Original illustrated world / decorative only" in T and
         "HasIndependentScoutColorGrade" in T,
         "Scouts must use a reversible per-material palette, never a second overlay")
    need("theme.Apply(root,retention.OpenDestination ==" in B and
         "HasSingleGradedScoutsPainting" in B,
         "Only actual Scouts may receive this color grade")
    # Rejected native dark-stone A/B: genuine painted parchment with its
    # contact bevel and wax medallion was richer and better contrasted.
    # Do not keep an unused layered substitute on top of it.
    need("BreachWatchScoutStoneContracts" not in T and
         "RealScoutsContractStoneReplaced" not in B,
         "Rejected low-contrast Scouts slate may not replace original approved art")
    need("retentionBridge.Initialize(host,canvas)" in H,
         "Independent Canvas must register the canonical retention bridge")
    nav=H.split("void Navigate(int index)",1)[1].split("void LateUpdate()",1)[0]
    need("retentionBridge.TryOpen" not in nav,
         "No new shortcut before native destination artwork verification")
    print("VIGILIA_VIGILANTS_SCOUTS_STAGE_SOURCE_PASS")
    print("NATIVE_IMAGE_AND_COMBAT_INTERACTION_QA_NOT_YET_CERTIFIED")
if __name__=="__main__":main()
