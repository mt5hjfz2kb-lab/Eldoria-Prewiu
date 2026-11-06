#!/usr/bin/env python3
"""Reassemble and verify the isolated Forja Oscura v0.4 Unity production input.
No Unity run, no gameplay writes and no untrusted path extraction.
"""
from pathlib import Path
import sys,hashlib,json,tempfile,zipfile
ROOT=Path(__file__).resolve().parent
EXPECTED="7fc8ad748199375c2ab8e3b4a64e5b1d3e0bf027a33b31e83452f5b134f4d74f"
COUNT=15
BYTES=3489120
PREFIX="Unity/Assets/EldoriaVisualKit/"
ALLOWED={".png",".cs",".asmdef"}
parts=[ROOT/"parts"/("part%02d.bin"%n) for n in range(COUNT)]
if not all(p.is_file() for p in parts):
    raise SystemExit("FORJA_INPUT_MISSING: expected 15 raw binary parts")
with tempfile.TemporaryDirectory(prefix="eldoria-forja-") as temp:
    archive=Path(temp)/"runtime.zip"
    with archive.open("wb") as out:
        for p in parts:
            with p.open("rb") as inp:
                while True:
                    data=inp.read(1024*1024)
                    if not data:break
                    out.write(data)
    digest=hashlib.sha256(archive.read_bytes()).hexdigest()
    if archive.stat().st_size!=BYTES or digest!=EXPECTED:
        raise SystemExit("FORJA_SHA256_MISMATCH: refuse import")
    with zipfile.ZipFile(archive) as z:
        names=z.namelist()
        if len(names)!=48 or len(set(names))!=len(names):
            raise SystemExit("FORJA_UNEXPECTED_FILE_COUNT")
        if any(not n.startswith(PREFIX) or ".." in Path(n).parts or
               Path(n).suffix.lower() not in ALLOWED for n in names):
            raise SystemExit("FORJA_UNSAFE_ARCHIVE_PATH")
        if "--extract" in sys.argv:
            target=Path(sys.argv[sys.argv.index("--extract")+1]).resolve()
            for item in z.infolist():
                dest=(target/item.filename).resolve()
                if not dest.is_relative_to(target):raise SystemExit("FORJA_ZIP_SLIP")
                dest.parent.mkdir(parents=True,exist_ok=True)
                dest.write_bytes(z.read(item))
    print(json.dumps({"status":"FORJA_ARCHIVE_SHA256_PASS",
        "sha256":digest,"bytes":archive.stat().st_size,"entries":len(names),
        "extracted":"--extract" in sys.argv}))
