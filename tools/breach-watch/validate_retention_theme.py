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
    need("retentionBridge.Initialize(host,canvas)" in H,
         "Independent Canvas must register the canonical retention bridge")
    nav=H.split("void Navigate(int index)",1)[1].split("void LateUpdate()",1)[0]
    need("retentionBridge.TryOpen" not in nav,
         "No new shortcut before native destination artwork verification")
    print("VIGILIA_VIGILANTS_SCOUTS_STAGE_SOURCE_PASS")
    print("NATIVE_IMAGE_AND_COMBAT_INTERACTION_QA_NOT_YET_CERTIFIED")
if __name__=="__main__":main()
