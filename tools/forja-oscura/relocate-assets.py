#!/usr/bin/env python3
"""Move first-party Forja UI kit into Eldoria without regenerating ANY Unity GUID.
Existing Unity metadata is authoritative. Never edit another workstream's files.
"""
from pathlib import Path
import re,shutil
root=Path(__file__).resolve().parents[2]
old=root/"Unity/Assets/EldoriaVisualKit"
dest=root/"Unity/Assets/Eldoria/VisualKit"
oldmeta=Path(str(old)+".meta")
newmeta=Path(str(dest)+".meta")
if not old.is_dir() or dest.exists() or not oldmeta.is_file() or newmeta.exists():
    raise SystemExit("FORJA_RELOCATION_FAIL_CLOSED: old source and its folder meta expected, new root must be absent")
source=[p for p in old.rglob("*") if p.is_file() and p.suffix in {".png",".cs",".asmdef"}]
folders=[old]+[p for p in old.rglob("*") if p.is_dir()]
expected=[Path(str(p)+".meta") for p in source+folders]
if len(source)!=48 or len(expected)!=55 or not all(x.is_file() for x in expected):
    raise SystemExit(f"FORJA_SOURCE_OR_METADATA_INCOMPLETE: files={len(source)} metas={sum(x.is_file() for x in expected)}/{len(expected)}")
ids=[]
for p in expected:
    match=re.search(r"(?m)^guid: ([0-9a-fA-F]{32})$",p.read_text())
    if not match:raise SystemExit("FORJA_INVALID_META_GUID: "+str(p))
    ids.append(match.group(1).lower())
if len(set(ids))!=55:raise SystemExit("FORJA_DUPLICATE_SOURCE_GUID")
dest.parent.mkdir(parents=True,exist_ok=True)
shutil.move(str(old),str(dest))
shutil.move(str(oldmeta),str(newmeta))
for cs in dest.rglob("*.cs"):
    text=cs.read_text()
    text=text.replace("Assets/EldoriaVisualKit/","Assets/Eldoria/VisualKit/")
    cs.write_text(text)
relocated=[p for p in dest.rglob("*") if p.is_file() and p.suffix in {".png",".cs",".asmdef"}]
metadata=[Path(str(p)+".meta") for p in relocated]+[newmeta]+[Path(str(p)+".meta") for p in dest.rglob("*") if p.is_dir()]
if len(relocated)!=48 or len(metadata)!=55 or not all(x.exists() for x in metadata):
    raise SystemExit("FORJA_MOVED_ASSETS_OR_METADATA_MISSING")
after=[re.search(r"(?m)^guid: ([0-9a-fA-F]{32})$",p.read_text()).group(1).lower() for p in metadata]
if set(after)!=set(ids) or len(set(after))!=55:
    raise SystemExit("FORJA_GUID_IDENTITY_CHANGED")
if old.exists() or oldmeta.exists():raise SystemExit("FORJA_OLD_ROOT_REMAINS")
print("FORJA_FIRST_PARTY_RELOCATION_PASS: 48 assets + 55 original Unity .meta with EXACT GUIDs retained")
