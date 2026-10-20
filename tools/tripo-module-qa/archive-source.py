#!/usr/bin/env python3
"""Archive an EXISTING Tripo model + Unity QA; makes NO paid requests.

The source artifacts must come from the original charged run; the Unity
evidence must come from a separate zero-credit isolated review run.
All file identities, asset ownership and required evidence are checked
before any persistent source is accepted.
"""
import argparse
import hashlib
import json
import re
import shutil
from pathlib import Path

ALLOWED = {
    "Eldoria_Spawnling_Bastion2_v1": "Spawnling_v1",
    "Eldoria_RiftFocus_v1": "RiftFocus_v1",
}
SHA256 = re.compile(r"^[a-f0-9]{64}$")


def one_file(folder, filename):
    files = list(Path(folder).rglob(filename))
    if len(files) != 1 or not files[0].is_file():
        raise RuntimeError(f"Expected exactly one {filename}, found {len(files)}")
    return files[0]


def verify(file, expected):
    actual = hashlib.sha256(file.read_bytes()).hexdigest()
    if not SHA256.fullmatch(expected) or actual != expected:
        raise RuntimeError(f"SHA256 mismatch for {file.name}: expected {expected}, got {actual}")
    if file.stat().st_size < 10000:
        raise RuntimeError(f"Suspiciously small file: {file.name}")
    return actual


def main():
    p = argparse.ArgumentParser()
    for arg in ("request", "raw_dir", "optimized_dir", "qa_dir", "out_root"):
        p.add_argument("--" + arg.replace("_", "-"), required=True)
    args = p.parse_args()
    req = json.loads(Path(args.request).read_text(encoding="utf-8"))
    review = req.get("review") or {}
    name = str(review.get("asset_name", ""))
    if name not in ALLOWED:
        raise RuntimeError("Unknown or unauthorized asset")
    if req.get("enabled") is not False or (req.get("credit_authorization") or {}).get("approved") is not False:
        raise RuntimeError("Archive MUST be a disabled, zero-spend request")
    if str(req.get("module_label")) != name:
        raise RuntimeError("Asset name does not match canonical request")
    raw = one_file(args.raw_dir, "Eldoria_Module_UnderReview_HQ.glb")
    optimized = one_file(args.optimized_dir, "Eldoria_Module_UnderReview_50K.glb")
    blend = one_file(args.optimized_dir, "Eldoria_Module_UnderReview_50K.blend")
    raw_sha = verify(raw, str(review.get("raw_sha256", "")).lower())
    opt_sha = verify(optimized, str(review.get("expected_sha256", "")).lower())
    blend_sha = verify(blend, str(review.get("blend_sha256", "")).lower())

    metrics_path = one_file(args.qa_dir, "tripo-source-metrics.json")
    metrics = json.loads(metrics_path.read_text(encoding="utf-8"))
    if metrics.get("asset") != name or metrics.get("unityVersion") != "6000.3.23f1":
        raise RuntimeError("QA metrics for different asset or Unity version")
    if (not metrics.get("uv0AllMeshes") or not metrics.get("normalsAllMeshes")
            or not metrics.get("fullResolutionTexturePass")
            or metrics.get("importedBaseColorWidth", 0) < 1024
            or metrics.get("importedBaseColorHeight", 0) < 1024
            or not metrics.get("positiveSelection") or not metrics.get("emptySelectionMiss")
            or not metrics.get("triangles", 0) > 500):
        raise RuntimeError("Isolated QA did not verify geometry, images and interaction")
    images = {}
    for name_png in ("iso.png", "side.png", "front.png", "strategic.png"):
        file = one_file(args.qa_dir, name_png)
        if file.stat().st_size < 15000:
            raise RuntimeError(f"Unity screenshot too small: {name_png}")
        images[name_png] = file

    receipt = one_file(args.raw_dir, "tripo-module-source.txt").read_text(encoding="utf-8-sig")
    values = dict(line.strip().split("=", 1) for line in receipt.splitlines() if "=" in line)
    task_id = values.get("tripo_task_id", "")
    try:
        credits = int(values.get("credits_consumed", "-1"))
    except ValueError:
        credits = -1
    if credits < 0 or credits > 20 or not re.fullmatch(r"[a-f0-9-]{20,}", task_id):
        raise RuntimeError("Missing or over-budget Tripo task receipt")
    if values.get("source_sha256", "").lower() != raw_sha:
        raise RuntimeError("Raw GLB hash and paid receipt differ")
    if values.get("request_id") != req.get("request_id"):
        raise RuntimeError("Charged source belongs to a different request")

    dest = Path(args.out_root) / ALLOWED[name]
    if dest.exists() and any(dest.iterdir()):
        raise RuntimeError(f"Archive target already exists and is nonempty: {dest}")
    qa_dest = dest / "UnityQA"
    qa_dest.mkdir(parents=True, exist_ok=False)
    for path, target in ((raw, "Tripo_H31_Original.glb"),
                         (optimized, "Optimized_49800.glb"),
                         (blend, "Optimized_49800.blend")):
        shutil.copy2(path, dest / target)
    shutil.copy2(metrics_path, qa_dest / "tripo-source-metrics.json")
    for filename, path in images.items():
        shutil.copy2(path, qa_dest / filename)
    lines = [
        "# Eldoria — " + ("Engendro de la Fisura" if name == "Eldoria_Spawnling_Bastion2_v1" else "Foco de corrupción de la Brecha"),
        "",
        "## Evidencia de la fuente",
        "",
        f"- Identidad exacta: \`{name}\`",
        f"- Tripo task: \`{task_id}\`",
        f"- Créditos **reales** del origen: **{credits}**, sin una nueva generación para el archivo.",
        f"- GLB original SHA-256: \`{raw_sha}\`",
        f"- GLB optimizado SHA-256: \`{opt_sha}\`",
        f"- Fuente editable BLEND SHA-256: \`{blend_sha}\`",
        f"- Run origen: \`{review['source_run_id']}\`",
        f"- Run QA aislada: \`{review['qa_run_id']}\`",
        f"- Tris importados Unity: {metrics['triangles']} / albedo: {metrics['importedBaseColorWidth']} × {metrics['importedBaseColorHeight']}.",
        "",
        "## Estado",
        "",
        "**SOURCE PRESERVED + UNITY ISOLATED TECH PASS**. La aprobación artística, el rigging, la animación, los LOD y el rendimiento físico móvil siguen pendientes de verificación. No está incorporado al runtime de Bastión I–II, no modifica guardados ni misiones y se archiva **fuera de Unity/Assets**.",
        "",
        "Para una comparación artística real usar las cuatro capturas de \`UnityQA/\`, no el resultado de CI en solitario.",
        "",
    ]
    (dest / "PROVENANCE.md").write_text("\n".join(lines), encoding="utf-8")
    print(json.dumps({
        "status": "TRIPO_SOURCE_AND_QA_ARCHIVED",
        "asset": name,
        "task_id": task_id,
        "credits": credits,
        "raw_sha": raw_sha,
        "opt_sha": opt_sha,
        "blend_sha": blend_sha,
        "unity_triangles": metrics["triangles"],
        "texture": [metrics["importedBaseColorWidth"], metrics["importedBaseColorHeight"]],
        "target": str(dest),
    }))


if __name__ == "__main__":
    main()
