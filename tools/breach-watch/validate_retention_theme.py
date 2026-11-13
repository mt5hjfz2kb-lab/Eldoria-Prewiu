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
    A=(R/"BreachWatchRecoveredRetentionArt.cs").read_text()
    need((R/"BreachWatchRecoveredRetentionArt.cs.meta").exists(),"Recovered artwork import metadata missing")
    need("EldoriaRetention/DepthB11/clean_backdrop" in A and
         '"left_castle","bridge","right_castle","left_rock","right_cliff","near_cliff"' in A and
         "EldoriaRetention/scouts-painterly-v2" in A and
         "EldoriaRetention/scout-parchment-v3" in A,
         "Recovery must preserve the authored B11 layers, Scout illustration and blank paper")
    need("owned.Contains(graphic)" in A and "graphic.enabled=false" in A and
         "image.raycastTarget=false" in A and "pair.Key.enabled=pair.Value" in A and
         "source.uvRect" in A and "theme.SynchronizeRecovered()" in B,
         "Recovery needs explicit new ownership, reversible retired painters and authoritative crop")
    need("HasSingleRestoredScoutsPainting" in B and "RestoredPlaneCount" in B,
         "Native QA must inspect actual visible restored components")
    need("AddComponent<Canvas>" not in A and "onClick" not in A and "Save(" not in A,
         "Visual recovery must not create a second Canvas, command or save")
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
