#!/usr/bin/env python3
"""Recover *exact* approved 27 Oct 2026 Blender art-pass recipe.
The source is stored as zlib+base64 text because the original source was
created during a prior ChatGPT asset session; this fails closed on SHA drift.
It never contacts Tripo or creates a new paid task.
"""
import base64, zlib, hashlib, sys
from pathlib import Path

assert len(sys.argv)==4, "usage: unpack_approved_source.py input_file approved_work_dir out_dir"
glb=Path(sys.argv[1]).absolute()
source=Path("tools/aldric-dialogue/approved-artpass-source.zlib.b64")
raw=zlib.decompress(base64.b64decode("".join(source.read_text().split())))
expected="5715c18b8e559bd752cc291451da2c768b3b72bec19ca3d652b8d1b6ea877315"
if hashlib.sha256(raw).hexdigest()!=expected:
    raise SystemExit("APPROVED_ARTPASS_SCRIPT_HASH_MISMATCH")
out_dir=Path(sys.argv[2]).absolute()
out_dir.mkdir(parents=True,exist_ok=True)
script=raw.decode("utf-8")
source_input="Path('/mnt/data/aldric_real_3d/entrega/SirAldric_Blender_49800tris.glb')"
source_output="Path('/mnt/data/aldric_artpass')"
if source_input not in script or source_output not in script:
    raise SystemExit("APPROVED_ARTPASS_SOURCE_PATHS_CHANGED")
script=script.replace(source_input, "Path("+repr(str(glb))+")")
script=script.replace(source_output, "Path("+repr(str(out_dir))+")")
dest=Path(sys.argv[3])
dest.parent.mkdir(parents=True,exist_ok=True)
dest.write_text(script,encoding="utf-8")
print("APPROVED_ARTPASS_RECIPE_SHA256=PASS",expected)
print("APPROVED_ARTPASS_COMMAND=python",str(dest))
