#!/usr/bin/env python3
"""Validate exact-head native art-workflow YAML without using the Windows runner.

GitHub Actions does not execute this workflow until its manual source request
and a formal exclusive CAS lease are both satisfied. This source-only check
catches invalid/doubled workflow keys, unsafe full-archive fallback and removal
of original 52-image/56-pointer artistic acceptance conditions.
"""
from pathlib import Path
import sys

try:
    import yaml
except ImportError:
    raise SystemExit("PyYAML==6.0.2 is required for the source-only YAML gate")

class NoDuplicateSafeLoader(yaml.SafeLoader):
    pass

def no_duplicate_mapping(loader, node):
    data = {}
    for key, value in node.value:
        k = loader.construct_object(key)
        if k in data:
            raise ValueError(f"Duplicate YAML key in native workflow: {k}")
        data[k] = loader.construct_object(value)
    return data

NoDuplicateSafeLoader.add_constructor(
    yaml.resolver.BaseResolver.DEFAULT_MAPPING_TAG,no_duplicate_mapping)

root=Path(__file__).resolve().parents[2]
native=root/".github/workflows/daily-treasures-native.yml"
code=native.read_text(encoding="utf-8")
workflow=yaml.load(code,Loader=NoDuplicateSafeLoader)

assert isinstance(workflow,dict)
trigger=workflow.get("on",workflow.get(True))
assert isinstance(trigger,dict)
assert "workflow_dispatch" in trigger and "push" in trigger
push=trigger["push"]
assert push["branches"]==["feature/eldoria-daily-treasures-20261108"]
assert push["paths"]==["tools/daily-treasures/native-request.txt"]

jobs=workflow["jobs"]
assert set(jobs)=={
    "verify-exclusive-lease","native-ui",
    "analyze-original-native-screenshots"}
preflight=jobs["verify-exclusive-lease"]
assert preflight["runs-on"]=="ubuntu-latest"
assert preflight["timeout-minutes"]<=10
assert "windows-runner-heavy" in str(preflight)
assert "eldoria-daily-treasures-20261108" in str(preflight)
assert "GITHUB_SHA" in str(preflight)
assert "authorization" not in code.lower() or "gh_token" in code.lower()

nativejob=jobs["native-ui"]
assert nativejob["needs"]=="verify-exclusive-lease"
assert nativejob["runs-on"]==["self-hosted","windows","unity-6000-3-23f1"]
assert nativejob["timeout-minutes"]<=90
steps=nativejob["steps"]
names=[step.get("name",step.get("uses","")) for step in steps]
assert len(names)==len(set(names)), "Duplicate native step names"
assert any("installed Git" in name for name in names)
assert any("Fetch exact PR SHA by safe sparse" in name for name in names)
assert not any("checkout@" in step.get("uses","").lower() for step in steps),(
    "Do not use actions/checkout on the gigantic Unity repo Windows runner: "
    "it can fall back to >26GB archive extraction")
source=next(step for step in steps
    if "Fetch exact PR SHA by safe sparse" in step.get("name",""))
fetch=source["run"]
for contract in (
    "git -C $root init","core.sparseCheckout",
    "--filter=blob:none","--depth=1","$env:GITHUB_SHA",
    "GeneratedEntryV","Original","treasure_opened_512.png",
    "DailyTreasureStageDirection.cs","vigilants-painterly-v2.png",
):
    if contract!="Original":
        assert contract in fetch, f"Missing safe source checkout gate: {contract}"
assert "http.extraheader" in fetch and "GH_TOKEN" in fetch
assert "http://" not in fetch, "Source must not use plaintext HTTP"
assert "release" not in names, "This is not an APK/WebGL release workflow"
# A real Unity 6000 compiler attempt previously failed because project Audio
# was disabled; never spend another Windows reservation on that same mistake.
assert "com.unity.modules.audio" in code
assert "DAILY_TREASURE_AUDIO_MODULE_ENABLED" in code
assert "manifest.clean_resource_icons" in code
assert "Required thirteen original source PNGs" in code
assert "thirteen exact SHA256 files" in code

native_render=next(step for step in steps
    if "render genuine day-four/day-seven" in step.get("name",""))
render=native_render["run"]
for requirement in (
    "$report.images -ne 52",
    "$report.graphicalPointerHits -ne 56",
    "$report.realGatewayClaims -ne 8",
    "$report.visualOnlyMilestoneCaptures -ne 16",
    "$item.readyToClaim -ne $claimCalendar",
    "$item.coversReward -ne (!$unclaimed)",
    "$report.source -ne $env:GITHUB_SHA",
    "$report.artApproved -ne $false",
    "DAILY_TREASURE_AUTHORED_PNG_BYTES_AND_IMPORTERS_PASS",
):
    # The art digest preflight marker is in its own step.
    if requirement.startswith("DAILY_TREASURE_"):
        assert requirement in code
    else:
        assert requirement in render, f"Native acceptance contract missing: {requirement}"

audit=jobs["analyze-original-native-screenshots"]
assert audit["needs"]=="native-ui"
assert audit["runs-on"]=="ubuntu-latest"
assert any("audit_native_art.py" in str(step) for step in audit["steps"])
assert any("actions/download-artifact@" in str(step) for step in audit["steps"])
assert not any("curl" in str(step) for step in audit["steps"]),(
    "Pixel audit must use same-run originals, not arbitrary web screenshots")
print("PASS genuine native Unity workflow YAML: one exclusive-owner gate, "
      "safe thin checkout, original images and isolated art review, no release")
