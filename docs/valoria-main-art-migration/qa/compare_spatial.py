"""Compare state-matched 3D selection geometry and rendered march polylines."""
import argparse
import json
import re
import sys
from pathlib import Path

GROUPS = {
    "hotspots": ("id", "key", "kind", "ordinal"),
    "colliders3D": ("key", "kind", "ordinal"),
    "projectedMarchRoutes": ("key", "kind", "ordinal"),
}

POSE = {"layer", "active", "enabled", "position", "rotation", "localPosition", "localRotation",
        "localScale", "localToWorld"}
REQUIRED = {
    "hotspots": POSE,
    "colliders3D": POSE | {"boundsCenter", "boundsSize", "isTrigger", "directHotspotIds", "includeLayers",
                            "excludeLayers", "layerOverridePriority", "contactOffset"},
    "projectedMarchRoutes": POSE | {"pointsReadable", "projectedCanvasPoints"},
}


def index(report, name):
    items = report[name]
    if not isinstance(items, list) or (not items and name != "projectedMarchRoutes"):
        raise ValueError(f"invalid or empty {name}")
    rows = {}
    for record in items:
        missing = REQUIRED[name].difference(record)
        if missing:
            raise ValueError(f"{name} missing fields {sorted(missing)}")
        key = tuple(record[field] for field in GROUPS[name])
        if any(not str(value).strip() or value is None for value in key) or key in rows:
            raise ValueError(f"{name}: missing or duplicate identity {key!r}")
        if name == "projectedMarchRoutes" and record["pointsReadable"] is not True:
            raise ValueError("unreadable route points")
        rows[key] = record
    return rows


def compare(a, b):
    for report in (a, b):
        if not re.fullmatch("[0-9a-f]{40}", report["sourceCommit"]):
            raise ValueError("invalid or missing sourceCommit")
        for field in ("scene", "state", "unity"):
            if not report[field]:
                raise ValueError(f"empty {field}")
    for field in ("scene", "state", "unity", "width", "height"):
        if a[field] != b[field]:
            raise ValueError(f"unmatched {field}")
    diffs = []
    for group in GROUPS:
        lhs, rhs = index(a, group), index(b, group)
        for key in sorted(lhs.keys() | rhs.keys()):
            if key not in lhs or key not in rhs:
                diffs.append({"group": group, "key": key, "status": "added" if key in rhs else "missing"})
            else:
                changes = {f: [lhs[key].get(f), rhs[key].get(f)] for f in sorted(lhs[key].keys() | rhs[key].keys())
                           if lhs[key].get(f) != rhs[key].get(f)}
                if changes:
                    diffs.append({"group": group, "key": key, "changes": changes})
    return diffs


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("main", type=Path)
    parser.add_argument("candidate", type=Path)
    parser.add_argument("--require-active-route", action="store_true",
                        help="require at least one active, readable march polyline with two or more points in each state")
    args = parser.parse_args()
    a, b = (json.loads(p.read_text(encoding="utf-8-sig")) for p in (args.main, args.candidate))
    diffs = compare(a, b)
    if args.require_active_route:
        for r in (a, b):
            if not any(s["active"] and s["enabled"] and len(s["projectedCanvasPoints"]) >= 2 for s in r["projectedMarchRoutes"]):
                raise ValueError("missing active projected march route")
    print(json.dumps({"main": a["sourceCommit"], "candidate": b["sourceCommit"], "state": a["state"],
                      "counts": {k: [len(a[k]), len(b[k])] for k in GROUPS}, "differences": diffs},
                     indent=2, ensure_ascii=False))
    return bool(diffs)


if __name__ == "__main__":
    try:
        sys.exit(main())
    except (OSError, ValueError, KeyError, TypeError) as error:
        print(f"Invalid capture pair: {error}", file=sys.stderr)
        sys.exit(2)
