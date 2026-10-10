#!/usr/bin/env python3
"""Read-only, dependency-free GLB 2.0 production preflight for Eldoria.

Usage: python tools/art-rd/audit_glb.py model.glb [model2.glb ...]
Emits JSON and exits 2 on structural errors. Warnings do not imply visual FAIL.
This is not a substitute for Blender renders or Unity/device performance tests.
"""
import json
import pathlib
import struct
import sys

COMPONENT_BYTES = {5120: 1, 5121: 1, 5122: 2, 5123: 2, 5125: 4, 5126: 4}
ACCESSOR_SIZES = {"SCALAR": 1, "VEC2": 2, "VEC3": 3, "VEC4": 4, "MAT2": 4, "MAT3": 9, "MAT4": 16}


def audit(path):
    p = pathlib.Path(path)
    findings = {"file": str(p), "bytes": p.stat().st_size, "errors": [], "warnings": []}
    raw = p.read_bytes()
    if len(raw) < 20:
        findings["errors"].append("Truncated GLB header")
        return findings
    magic, version, total = struct.unpack_from("<4sII", raw)
    if magic != b"glTF" or version != 2 or total != len(raw):
        findings["errors"].append("Invalid GLB 2.0 magic/version/declared length")
        return findings
    chunks = []
    pos = 12
    while pos < len(raw):
        if pos + 8 > len(raw):
            findings["errors"].append("Truncated chunk header")
            return findings
        n, typ = struct.unpack_from("<I4s", raw, pos)
        pos += 8
        if n % 4 or pos + n > len(raw):
            findings["errors"].append("Misaligned or truncated chunk")
            return findings
        chunks.append((typ, raw[pos:pos+n]))
        pos += n
    if not chunks or chunks[0][0] != b"JSON":
        findings["errors"].append("Missing first JSON chunk")
        return findings
    try:
        data = json.loads(chunks[0][1].decode("utf-8").rstrip(" \t\r\n\x00"))
    except (UnicodeError, ValueError):
        findings["errors"].append("Unparseable glTF JSON")
        return findings
    if data.get("asset", {}).get("version") != "2.0":
        findings["errors"].append("Unsupported glTF asset version")
    accessors = data.get("accessors", [])
    views = data.get("bufferViews", [])
    buffers = data.get("buffers", [])
    blob = next((payload for typ, payload in chunks if typ == b"BIN\x00"), b"")
    for i, b in enumerate(buffers):
        if not b.get("uri") and b.get("byteLength", 0) > len(blob):
            findings["errors"].append(f"Buffer {i} exceeds binary chunk")
    for i, a in enumerate(accessors):
        if a.get("componentType") not in COMPONENT_BYTES or a.get("type") not in ACCESSOR_SIZES:
            findings["errors"].append(f"Accessor {i} has invalid component/type")
            continue
        if a.get("count", 0) < 0:
            findings["errors"].append(f"Accessor {i} has invalid count")
        vi = a.get("bufferView")
        if vi is not None and (not isinstance(vi, int) or vi < 0 or vi >= len(views)):
            findings["errors"].append(f"Accessor {i} refers to absent bufferView")
    triangles = 0
    primitives = 0
    for mesh in data.get("meshes", []):
        for prim in mesh.get("primitives", []):
            primitives += 1
            mode = prim.get("mode", 4)
            if mode != 4:
                findings["warnings"].append(f"Primitive mode {mode} not triangle-list")
                continue
            idx = prim.get("indices")
            position = prim.get("attributes", {}).get("POSITION")
            ref = idx if idx is not None else position
            if not isinstance(ref, int) or ref < 0 or ref >= len(accessors):
                findings["errors"].append("Mesh primitive has absent index/position accessor")
                continue
            triangles += accessors[ref].get("count", 0) // 3
            if "NORMAL" not in prim.get("attributes", {}):
                findings["warnings"].append("Primitive missing NORMAL attribute")
            if "TEXCOORD_0" not in prim.get("attributes", {}):
                findings["warnings"].append("Primitive missing UV0; verify authored material")
    images = data.get("images", [])
    external = [im.get("uri") for im in images if im.get("uri") and not im["uri"].startswith("data:")]
    if external:
        findings["warnings"].append("External image references: asset is not self-contained")
    findings.update({"meshes": len(data.get("meshes", [])),
                     "primitives": primitives, "triangles_estimated": triangles,
                     "materials": len(data.get("materials", [])),
                     "textures": len(data.get("textures", [])),
                     "images": len(images), "external_images": external,
                     "has_skins": bool(data.get("skins")),
                     "has_animations": bool(data.get("animations"))})
    if not primitives:
        findings["warnings"].append("No mesh primitives")
    return findings


def main(argv):
    if not argv:
        print("Usage: audit_glb.py file.glb [...]", file=sys.stderr)
        return 2
    results = []
    for name in argv:
        try:
            results.append(audit(name))
        except (OSError, ValueError, struct.error) as exc:
            results.append({"file": name, "errors": [str(exc)], "warnings": []})
    print(json.dumps({"assets": results, "structural_pass": all(not r["errors"] for r in results)},
                     indent=2, ensure_ascii=False))
    return 2 if any(r["errors"] for r in results) else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
