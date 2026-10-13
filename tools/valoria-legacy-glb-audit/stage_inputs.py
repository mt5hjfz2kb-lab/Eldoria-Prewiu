#!/usr/bin/env python3
"""Stage exact read-only GLB candidates for comparison. Never edits the originals."""
from pathlib import Path
import json,hashlib,subprocess,re

ROOT=Path.cwd()
OUTPUT=ROOT/"audit-output"
INPUTS=OUTPUT/"inputs"
INPUTS.mkdir(parents=True,exist_ok=True)
entries=[]
audit=json.loads((ROOT/"pipeline/asset-library-canonicalization-audit.json").read_text())
assert audit["glb_count"]==26
for a in audit["assets"]:
    src=ROOT/a["path"]
    if not src.is_file(): raise RuntimeError("Missing legacy GLB: "+str(src))
    digest=hashlib.sha256(src.read_bytes()).hexdigest()
    if digest!=a["sha256"]: raise RuntimeError("Unexpected legacy SHA: "+str(src))
    asset_id="legacy_"+str(len(entries)+1).zfill(2)+"_"+src.stem
    entries.append(dict(id=asset_id,origin="legacy_26",path=str(src),source_repo_path=a["path"],sha256=digest,bytes=src.stat().st_size))

frozen="0abf13870d21725c30acf3f3e3a8c86385470073"
subprocess.run(["git","fetch","--depth=1","origin",frozen],check=True)
parts=["wall_straight","watchtower","wall_corner","wall_connections","stone_path_platform","cliff_rocks","defensive_props"]
extra=[("hero_v3","art-source/valoria/production/first-hero-gate-v1/quality-v3/ValoriaHeroGate.glb")]
extra += [(a,"art-source/valoria/production/asset-family-v2/quality-v3/"+a+"/"+a+".glb") for a in parts]
extra += [("bridge","art-source/valoria/production/bridge-family-v1/BridgeFamilyV1.glb"),("stairs","art-source/valoria/production/stair-family-v1/StairFamilyV1.glb")]
for name,p in extra:
    ref=frozen
    data=subprocess.check_output(["git","show",ref+":"+p])
    dest=INPUTS/(name+".glb")
    dest.write_bytes(data)
    entries.append(dict(id="family_"+name if name in parts or name=="hero_v3" else "supplement_"+name,origin="hero_v3_family" if name in parts or name=="hero_v3" else "prior_supplement",path=str(dest),source_repo_path=p,source_commit=ref,sha256=hashlib.sha256(data).hexdigest(),bytes=len(data)))
assert len(entries)==36,len(entries)
(OUTPUT/"audit-inputs.json").write_text(json.dumps(dict(schema_version=1,legacy_count=26,candidate_count=10,entries=entries),indent=2))
print("AUDIT INPUTS PASS",len(entries),"sha256 verified. Historical sources unchanged.",flush=True)
