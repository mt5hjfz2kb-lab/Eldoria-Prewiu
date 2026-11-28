"""Deduplicate the actual control geometries and event records across baseline states."""
import argparse
import gzip
import hashlib
import json
from pathlib import Path
import zipfile

def compact(data):
    return json.dumps(data, separators=(",", ":"), ensure_ascii=False, sort_keys=True).encode()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("artifact", type=Path)
    parser.add_argument("out", type=Path)
    args = parser.parse_args()
    controls = {}
    states = {}
    with zipfile.ZipFile(args.artifact) as zip_file:
        def read_json(path):
            return json.loads(zip_file.read(str(path)).decode("utf-8-sig"))
        meta = read_json("manifest.json")
        if meta["status"] != "CAPTURED":
            raise ValueError("Capture manifest incomplete")
        for suite, run in meta["runs"].items():
            if run["exit"] or run["missingExpected"] or run.get("validationError"):
                raise ValueError(f"Incomplete suite {suite}")
            for item in run["samples"]:
                stem = item["state"]
                prefix = Path(suite)
                sample = read_json(prefix / (stem + ".sample.json"))
                raw = zip_file.read((prefix / sample["inventory"]).as_posix())
                inv = json.loads(raw.decode("utf-8-sig"))
                assert hashlib.sha256(raw).hexdigest() == item["inventorySha256"]
                assert hashlib.sha256(zip_file.read((prefix / sample["screenshot"]).as_posix())).hexdigest() == item["screenshotSha256"]
                assert (inv["frame"], inv["sourceCommit"], inv["state"]) == (sample["frame"], meta["sourceCommit"], stem)
                rows = []
                for control in inv["controls"]:
                    digest = hashlib.sha256(compact(control)).hexdigest()
                    controls.setdefault(digest, control)
                    rows.append(digest)
                state = (prefix / stem).as_posix()
                if state in states:
                    raise ValueError("Duplicate state " + state)
                states[state] = {"controls": rows, "canvases": inv["canvases"], "cameras": inv["cameras"],
                                 "raycasters": inv["raycasters"], "target": [sample["width"], sample["height"]],
                                 "editorSize": [inv["width"], inv["height"]], "scene": inv["scene"],
                                 "frame": inv["frame"], "inventorySha256": item["inventorySha256"],
                                 "screenshotSha256": item["screenshotSha256"]}
    result = {"source": meta["sourceCommit"], "unity": meta["projectVersion"],
              "controlsBySha256": controls, "states": states}
    encoded = compact(result)
    args.out.write_bytes(gzip.compress(encoded, mtime=0))
    assert gzip.decompress(args.out.read_bytes()) == encoded
    print(meta["sourceCommit"], len(states), sum(len(v["controls"]) for v in states.values()),
          len(controls), args.out.stat().st_size, hashlib.sha256(args.out.read_bytes()).hexdigest())

if __name__ == "__main__":
    main()
