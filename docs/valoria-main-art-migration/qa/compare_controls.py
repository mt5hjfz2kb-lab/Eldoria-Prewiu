"""Compare two matched live inventory states. Exit 1 for drift, 2 for a bad pair."""
import argparse
import json
import re
import sys
from pathlib import Path

IDENTITY = ("path", "component", "componentOrdinal")
FIELDS = ("activeInHierarchy", "enabled", "interactable", "effectiveInteractable",
          "navigation", "anchorMin", "anchorMax", "anchoredPosition", "sizeDelta",
          "pivot", "screenCorners", "events")


def by_identity(report):
    controls = report["controls"]
    if not isinstance(controls, list) or not controls:
        raise ValueError("empty control capture")
    if any(not entry["path"] or not entry["component"] for entry in controls):
        raise ValueError("missing control identity")
    indexed = {tuple(control[field] for field in IDENTITY): control for control in controls}
    if len(indexed) != len(controls):
        raise ValueError("duplicate control identity")
    return indexed


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("main", type=Path)
    parser.add_argument("candidate", type=Path)
    args = parser.parse_args()
    reports = [json.loads(path.read_text(encoding="utf-8-sig")) for path in (args.main, args.candidate)]
    for report in reports:
        if not re.fullmatch("[0-9a-f]{40}", report["sourceCommit"]):
            raise ValueError("invalid or missing source commit")
    left, right = reports
    for key in ("width", "height", "unity", "state", "scene"):
        if left[key] != right[key]:
            raise ValueError(f"mismatched {key}: {left[key]!r} vs {right[key]!r}")
    left_index, right_index = map(by_identity, reports)
    diffs = []
    for key in sorted(left_index.keys() | right_index.keys()):
        if key not in right_index:
            diffs.append({"control": key, "status": "missing"})
        elif key not in left_index:
            diffs.append({"control": key, "status": "added"})
        else:
            changes = {field: [left_index[key][field], right_index[key][field]] for field in FIELDS
                       if left_index[key][field] != right_index[key][field]}
            if changes:
                diffs.append({"control": key, "changes": changes})
    print(json.dumps({"state": left["state"], "main": left["sourceCommit"], "candidate": right["sourceCommit"],
                      "controls": [len(left_index), len(right_index)], "differences": diffs}, ensure_ascii=False, indent=2))
    return bool(diffs)


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (OSError, ValueError, KeyError, TypeError) as error:
        print(f"Invalid capture pair: {error}", file=sys.stderr)
        sys.exit(2)
