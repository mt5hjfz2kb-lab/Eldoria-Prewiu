#!/usr/bin/env python3
"""Valoria 63 bidirectional CI feedback gate. Standard library only, no paid tools.
Usage:
  python gate.py selection SELECTED.json ACK.json
  python gate.py art STATUS.json GLB_DIR PREVIEWS_DIR OUTPUT.json
  python gate.py selftest
Read registry.json from the SAME checkout; consumers must checkout current main.
"""
import hashlib
import json
import re
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
HEX64 = re.compile(r"^[0-9a-f]{64}$")
SCOPES = {"ASSET_GLOBAL", "ASSEMBLY_CONTRACT", "SCENE_LOCAL"}
DECISIONS = {
    "ASSET_GLOBAL": {"pending_review", "applied"},
    "ASSEMBLY_CONTRACT": {"pending_review", "preserved_assembly"},
    "SCENE_LOCAL": {"pending_review", "scene_only"},
}


def fail(message):
    raise ValueError("VALORIA_FEEDBACK_GATE_FAIL: " + message)


def read(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def registry():
    ids = read(HERE / "asset-ids.json")
    reg = read(HERE / "registry.json")
    if len(ids) != 63 or len(set(ids)) != 63:
        fail("catalogue is not 63 unique IDs")
    items = reg["assets"]
    if len(items) != 63 or {x["id"] for x in items} != set(ids):
        fail("registry must cover exactly the canonical 63 assets")
    by_asset = {x["id"]: x for x in items}
    corrections = {}
    for c in reg["corrections"]:
        if c["scope"] not in SCOPES or c["asset_id"] not in by_asset:
            fail("invalid correction scope/asset " + c["id"])
        if c["id"] in corrections:
            fail("duplicate correction " + c["id"])
        corrections[c["id"]] = c
    for a in items:
        if len(a["correction_ids"]) != len(set(a["correction_ids"])):
            fail("duplicate correction reference " + a["id"])
        for cid in a["correction_ids"]:
            if cid not in corrections or corrections[cid]["asset_id"] != a["id"]:
                fail("orphan or cross-piece reference " + a["id"] + "/" + cid)
    if set(corrections) != {cid for a in items for cid in a["correction_ids"]}:
        fail("unlinked corrections exist")
    return ids, by_asset, corrections


def sha(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            h.update(chunk)
    return h.hexdigest()


def verify_selection(selected_path, ack_path):
    _, assets, corrections = registry()
    selection = read(selected_path)
    ack = read(ack_path)
    selected = selection.get("selected")
    rows = ack.get("selected_feedback")
    if not isinstance(selected, list) or not isinstance(rows, list):
        fail("selection and explicit feedback acknowledgement arrays required")
    if len({x["id"] for x in selected}) != len(selected):
        fail("duplicate selected asset")
    if len(rows) != len(selected) or len({x["id"] for x in rows}) != len(rows):
        fail("one individual acknowledgement per selected asset required")
    selected_map = {x["id"]: x for x in selected}
    if set(selected_map) != {x["id"] for x in rows}:
        fail("acknowledgements do not cover the selected asset set")
    if selection.get("constraints", {}).get("art_approved") is not False:
        fail("overall art approval must be explicitly false until actual visual sign-off")
    pending_total = 0
    for row in rows:
        asset_id = row["id"]
        if asset_id not in assets:
            fail("unknown selected asset " + asset_id)
        pin = selected_map[asset_id]
        if not HEX64.fullmatch(str(pin.get("sha256", ""))):
            fail("missing pinned SHA on " + asset_id)
        if pin["sha256"] != row.get("sha256"):
            fail("asset version changed without new acknowledgements: " + asset_id)
        needed = set(assets[asset_id]["correction_ids"])
        given = row.get("decisions")
        if not isinstance(given, list):
            fail("missing per-piece decision list " + asset_id)
        ids = [d.get("id") for d in given]
        if len(set(ids)) != len(ids) or set(ids) != needed:
            fail("unacknowledged new/previous correction " + asset_id)
        for decision in given:
            correction = corrections[decision["id"]]
            disposition = decision.get("disposition")
            if disposition not in DECISIONS[correction["scope"]]:
                fail("wrong global/scene/assembly disposition " + decision["id"])
            evidence = decision.get("evidence_url")
            if not isinstance(evidence, str) or not evidence.startswith("https://"):
                fail("missing correction evidence " + decision["id"])
            if disposition == "applied":
                if not str(decision.get("validation_ab_url", "")).startswith("https://"):
                    fail("globally applied change has no independent A/B " + decision["id"])
            if disposition == "pending_review":
                pending_total += 1
                if pin.get("usage") not in ("candidate_in_scene", "staged_only"):
                    fail("unresolved correction promoted as approved " + asset_id)
        if row.get("art_approved") is not False:
            fail("per-piece art approval not established " + asset_id)
    print(f"VALORIA_SELECTION_FEEDBACK_PASS selected={len(rows)} pending={pending_total} registry_corrections={len(corrections)} main_registry_dynamic=true")
    return len(rows), pending_total


def validate_art(status_path, glb_dir, previews_dir, output_path):
    ids, assets, corrections = registry()
    status = read(status_path)
    rows = status.get("assets")
    if status.get("art_approved") is not False or not isinstance(rows, list):
        fail("standalone renders cannot imply approved art")
    if len(rows) != 63 or {x.get("asset") for x in rows} != set(ids):
        fail("exactly 63 individual asset statuses required")
    glb_dir, previews_dir = Path(glb_dir), Path(previews_dir)
    if {p.stem for p in glb_dir.glob("*.glb")} != set(ids):
        fail("incomplete / extra GLB set")
    validated = []
    for row in rows:
        name = row["asset"]
        if row.get("technical") != "GLB_EXPORTED" or row.get("art_approved") is not False:
            fail("asset failure or premature approval " + name)
        model = glb_dir / (name + ".glb")
        digest = sha(model)
        if row.get("glb_sha256") != digest:
            fail("GLB binary differs from source report " + name)
        for key in ("individual_front", "individual_rear"):
            preview = previews_dir / str(row.get(key, ""))
            if not preview.is_file() or preview.stat().st_size == 0:
                fail("missing before/after view " + name + "/" + key)
        feedback = [corrections[cid] for cid in assets[name]["correction_ids"]]
        validated.append({
            "id": name,
            "sha256": digest,
            "visual_status": "UNREVIEWED_NOT_PROMOTABLE",
            "ready_for_unity": False,
            "requires_explicit_acknowledgement": bool(feedback),
            "feedback": [{
                "id": c["id"], "scope": c["scope"], "state": c["state"],
                "summary": c["summary"], "regression": c["regression"],
                "origin_sha256": c["source_sha256"], "proof": c["proof"],
            } for c in feedback],
        })
    out = {
        "schema": "eldoria.valoria63.artist-feedback-output.v1",
        "source": "live-main-registry-at-workflow-start",
        "art_approved": False,
        "assets": validated,
    }
    output_path = Path(output_path)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(out, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(f"VALORIA_ART_FEEDBACK_PASS assets={len(validated)} with_corrections={sum(bool(x['feedback']) for x in validated)} approved=0")


if __name__ == "__main__":
    try:
        mode = sys.argv[1]
        if mode == "selection" and len(sys.argv) == 4:
            verify_selection(sys.argv[2], sys.argv[3])
        elif mode == "art" and len(sys.argv) == 6:
            validate_art(*sys.argv[2:6])
        else:
            fail("usage: selection SELECTED.json ACK.json | art STATUS GLB PREVIEWS OUTPUT")
    except (KeyError, ValueError, OSError, IndexError, TypeError) as e:
        print(str(e), file=sys.stderr)
        sys.exit(1)
