"""Execute existing metadata checks in isolated copies of their actual sparse inputs.

No Unity/Blender, network, deletions from the repository, or test reimplementations.
Historical cross-branch feedback validation is outside this bounded change.
"""
import json
from pathlib import Path
import re
import shutil
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[2]
CASES = {
    "valoria-full-frame-convergence-control.yml": {
        "commands": [["node", "tools/validate-valoria-full-frame-convergence.mjs"]],
        "required": "pipeline/valoria-full-frame-convergence-v1.json",
    },
    "visual-fallback-route.yml": {
        "commands": [["node", "tools/route-visual-fallback.mjs",
                      "pipeline/visual-fallback-policy.json", mode, mode + ".json"]
                     for mode in ["geometry_identity_or_silhouette",
                                  "depth_or_occlusion_alignment", "unknown"]],
        "required": "pipeline/visual-fallback-policy.json",
    },
}


def inputs(workflow):
    text = (ROOT / ".github/workflows" / workflow).read_text()
    match = re.search(r"^          sparse-checkout: \|\n((?:            .+\n)+)",
                      text, re.MULTILINE)
    if not match or "sparse-checkout-cone-mode: false" not in text:
        raise AssertionError(f"Missing explicit sparse inputs: {workflow}")
    result = set()
    for line in match[1].splitlines():
        pattern = line.strip()
        if not pattern.startswith("/") or ".." in pattern or "*" in pattern:
            raise AssertionError(f"Expected exact bounded path: {pattern}")
        source = ROOT / pattern.lstrip("/")
        if not source.exists():
            raise AssertionError(f"Missing sparse input: {pattern}")
        if source.is_dir():
            result.update(p for p in source.rglob("*") if p.is_file()
                          and "__pycache__" not in p.parts)
        else:
            result.add(source)
    return sorted(result)


def verify():
    report = []
    for workflow, case in CASES.items():
        files = inputs(workflow)
        with tempfile.TemporaryDirectory(prefix="eldoria-sparse-check-") as directory:
            target = Path(directory)
            for source in files:
                output = target / source.relative_to(ROOT)
                output.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(source, output)
            for command in case["commands"]:
                result = subprocess.run(command, cwd=target, capture_output=True, text=True)
                if result.returncode:
                    raise AssertionError(f"{workflow}: {result.stdout}\n{result.stderr}")
            if workflow == "visual-fallback-route.yml":
                unknown = json.loads((target / "unknown.json").read_text())
                assert unknown["decision"] == "STOP_AND_CLASSIFY"
                assert unknown["open_new_research"] is False
            # Prove the check still rejects absent inputs, instead of silently
            # passing just because the expensive repository was not downloaded.
            (target / case["required"]).unlink()
            missing = subprocess.run(case["commands"][0], cwd=target,
                                     capture_output=True, text=True)
            assert missing.returncode != 0, f"Missing input accepted: {workflow}"
        report.append({"workflow": workflow, "files": len(files),
                       "checkout_bytes": sum(p.stat().st_size for p in files),
                       "existing_checks": "PASS", "missing_input": "REJECTED"})
    print(json.dumps({"status": "SPARSE_INPUTS_PASS", "profiles": report}, indent=2))


if __name__ == "__main__":
    verify()
