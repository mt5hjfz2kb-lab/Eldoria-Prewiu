"""Compare presentation, input surfaces and editor residency across matched Unity observations."""
import argparse
from collections import Counter
import json
from pathlib import Path


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def inspect_pair(before, after, retired):
    for key in ("unity", "scene", "state", "width", "height"):
        if before[key] != after[key]:
            raise ValueError(f"Unmatched {key}")
    differences = {}
    for group in ("cameras", "canvases", "raycasters", "graphics"):
        def records(report):
            rows = report[group]
            if group == "graphics":
                rows = [r for r in rows if r["raycastTarget"]]
            if not rows and group != "graphics":
                raise ValueError(f"Missing {group} in {report['state']}")
            indexed = {}
            for row in rows:
                key = (row["path"], row["component"], row["componentOrdinal"])
                if key in indexed:
                    raise ValueError(f"Duplicate {group} component at {key}")
                fields = dict(row)
                fields.pop("hideFlags", None)
                if group == "graphics":
                    fields.pop("culled", None)
                indexed[key] = fields
            return indexed
        left, right = records(before), records(after)
        changes = []
        for key in sorted(left.keys() | right.keys()):
            if key not in left or key not in right:
                changes.append({"component": key, "status": "added" if key in right else "removed"})
            else:
                fields = sorted(f for f in left[key].keys() | right[key].keys()
                                if left[key].get(f) != right[key].get(f))
                if fields:
                    changes.append({"component": key, "fields": fields})
        if changes:
            differences[group] = changes
    views = []
    for report in (before, after):
        counts = Counter("inactive" if not r["activeInHierarchy"] else "disabled" if not r["enabled"]
                         else "forceRenderingOff" if r["forceRenderingOff"] else "active"
                         for r in report["renderers"])
        residency = [obj for obj in report["loadedEditorObjects"] if obj["assetPath"] in retired]
        views.append({"rendererCounts": dict(counts), "editorRetiredObjects": residency})
    return {"state": before["state"], "differences": differences, "main": views[0], "candidate": views[1]}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("main", type=Path)
    parser.add_argument("candidate", type=Path)
    parser.add_argument("--retired", required=True, type=Path,
                        help="JSON array of exact retired Assets/... paths")
    args = parser.parse_args()
    retired = read(args.retired)
    if not isinstance(retired, list) or not all(isinstance(p, str) and p.startswith("Assets/") for p in retired):
        parser.error("Provide exact Unity asset paths")
    reports = []
    for folder in (args.main, args.candidate):
        states = {p.relative_to(folder): p for p in folder.rglob("*.inventory.json")}
        if not states:
            parser.error(f"No live inventories in {folder}")
        reports.append(states)
    left, right = reports
    if left.keys() != right.keys():
        parser.error(f"Unpaired inventories: {sorted(map(str, left.keys() ^ right.keys()))}")
    failures = 0
    for path in sorted(left):
        pair = inspect_pair(read(left[path]), read(right[path]), set(retired))
        print(json.dumps(pair, ensure_ascii=False))
        failures += bool(pair["differences"] or pair["candidate"]["editorRetiredObjects"])
    return int(failures > 0)


if __name__ == "__main__":
    raise SystemExit(main())
