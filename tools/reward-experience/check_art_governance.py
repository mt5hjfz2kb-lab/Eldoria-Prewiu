#!/usr/bin/env python3
"""Fail-closed Issue186 first-party source-art governance, no Unity/editor."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
ID = "eldoria-issue186-reward-attention-guidance-20261111"
REQUEST = ROOT / "pipeline/art-production-requests" / (ID + ".json")
MANIFEST = ROOT / "Unity/Assets/Eldoria/Issue186Art/issue186_manifest.json"

if __name__ == "__main__":
    if not REQUEST.is_file() or not MANIFEST.is_file():
        raise SystemExit("ISSUE186_ART_GOVERNANCE_FAIL missing routing request or manifest")
    req = json.loads(REQUEST.read_text(encoding="utf-8"))
    art = json.loads(MANIFEST.read_text(encoding="utf-8"))
    checks = {
        "canonical workstream": req.get("workstream_id") == ID and req.get("request_id") == ID,
        "enabled request": req.get("enabled") is True and req.get("status") == "ACTIVE",
        "mandatory art authorship": req.get("ai_authorship_policy") == "required",
        "no paid credits": req.get("budget_eur") == 0
            and req.get("credit_authorization", {}).get("approved") is False
            and req.get("credit_authorization", {}).get("authorized_credit_cost") == 0,
        "no false native or mobile approval": req.get("art_approved") is False
            and req.get("physical_mobile_tested") is False
            and req.get("visual_gate_state") == "SOURCE_VERIFIED_NATIVE_UI_PENDING",
        "16 correct assets": art.get("assetCount") == 16
            and len(art.get("artifacts", [])) == 16,
        "all 12 package variants": {
            row.get("file") for row in art["artifacts"]
            if row.get("file", "").startswith("pack_")
        } == {
            f"pack_{kind}_{tier}.png"
            for kind in ("wood", "stone", "food")
            for tier in ("100", "1k", "10k", "100k")
        },
        "four UI components": {
            row.get("file") for row in art["artifacts"]
            if not row.get("file", "").startswith("pack_")
        } == {
            "badge_new.png", "badge_claimable.png",
            "tutorial_pointing_hand.png", "tutorial_target_halo.png"
        },
    }
    for label, ok in checks.items():
        if not ok:
            print("ISSUE186_ART_GOVERNANCE_ERROR", label)
    if not all(checks.values()):
        raise SystemExit("ISSUE186_ART_GOVERNANCE_FAIL")
    print(f"ISSUE186_ART_GOVERNANCE_PASS: {len(checks)} assertions; native art review still pending")
