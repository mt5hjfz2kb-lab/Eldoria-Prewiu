#!/usr/bin/env python3
"""Acquire exact free 2022 Quaternius files for **temporary private review**.
Never commits or packages vendor source into public Git. Uses original source IDs
from artist's Google Drive folder, NOT third-party mirrors. Does not assert
that mere download proves a right to distribute source under latest 2026 QAL.
"""
import hashlib
import json
import os
import pathlib
import sys

SOURCE={
 "Barracks_SecondAge_Level3.gltf":"1Yh-YB_Ft0uy_3DcJqKXR1UNlcH1Y5UPb",
 "TownCenter_SecondAge_Level3.gltf":"1y1AtgLu7B9AFgfohwZ5LvtN-_XUH5veX",
 "Storage_FirstAge_Level1.gltf":"1gTOKxkeaD-foMDHbCEsfgn8o1gHkZux5",
 "Mine.gltf":"1cF334DhQWNtVhX9pIAN84i1an0GnLIfh",
 "Resource_PineTree_Group.gltf":"1UcIrT1Rbokb1TC6N80Tzex-9KyVF0kFX",
 "WallTowers_Door_FirstAge.gltf":"1mIK14JIPfTFfDNNW1mxngv05qCzIfFkU",
 "License.txt":"1Vf_6mtufbmV2mQHY35l9tPqaR3Oe8Did"
}
ROOT=pathlib.Path(__file__).resolve().parents[2]
TMP=pathlib.Path(os.environ.get("ELDORIA_VENDOR_REVIEW", str(ROOT/"_vendor_review_temp")))
TMP.mkdir(parents=True,exist_ok=True)
# Fail if destination points into publicly committed asset directory.
assert not str(TMP.resolve()).startswith(str((ROOT/"Unity/Assets").resolve())), "Vendor input must remain outside public Unity Assets!"
assert not str(TMP.resolve()).startswith(str((ROOT/"docs").resolve())), "Vendor source must not go into docs!"
import gdown
report={"source_site":"https://quaternius.com/packs/ultimatefantasyrts.html",
        "source_folder":"https://drive.google.com/drive/folders/1h7sztlZyavWla-JDk3jp6KiWDdMh08yd",
        "source_subfolder":"https://drive.google.com/drive/folders/17T5i1YOWsJRZD10Ko0rm5pKjXjke7UTK",
        "acquisition_date":"2026-11-16",
        "publisher_2022_statement":"https://www.patreon.com/quaternius/posts/ultimate-fantasy-71326249",
        "legacy_page_badge":"CC0",
        "new_QAL_terms":"https://quaternius.com/license.html",
        "rights_to_redistribute_sources":"not_certified",
        "rights_to_use_in_commercial_game":"publisher_claims_yes",
        "materials_status":"unapproved",
        "files":[]}
for name,file_id in SOURCE.items():
    dest=TMP/name
    if not dest.exists():
        result=gdown.download(id=file_id,output=str(dest),quiet=True,use_cookies=False)
        if result is None or not dest.is_file():
            raise RuntimeError(f"Publisher file inaccessible: {name} ({file_id})")
    data=dest.read_bytes()
    if len(data)<100:
        raise RuntimeError(f"Unexpected empty file {name}")
    record={"name":name,"drive_file_id":file_id,"size_bytes":len(data),
            "sha256":hashlib.sha256(data).hexdigest()}
    if name.endswith(".gltf"):
        try: gltf=json.loads(data)
        except Exception as e: raise RuntimeError(f"{name} was not glTF JSON") from e
        if str(gltf.get("asset",{}).get("version","")).split(".")[0]!="2":
            raise RuntimeError(f"File {name} is not glTF 2.x")
        refs=[b.get("uri","") for b in gltf.get("buffers",[])]
        # The official file listing is only .gltf files. Missing .bin is a silent fidelity trap.
        for u in refs:
            if not u.startswith("data:"):
                raise RuntimeError(f"{name} references external buffer {u!r}; find proper companion before import")
        record.update({"meshes":len(gltf.get("meshes",[])),
                       "materials":len(gltf.get("materials",[])),
                       "vertices":sum(x.get("count",0) for x in gltf.get("accessors",[])
                                      if x.get("type")=="VEC3"), 
                       "self_contained_buffers":True})
        if record["meshes"]<1: raise RuntimeError(f"{name} missing meshes")
    else:
        license_text=data.decode("utf-8")
        if "CC0 1.0" not in license_text:
            raise RuntimeError("Original License.txt did not claim CC0 1.0")
        record["license_mismatch"]="The actual text names Ultimate Platformer Pack, NOT Ultimate Fantasy RTS. Confirm publisher Patreon 2022 CC0 claim before source redistribution."
        report["license_text_mismatch"]=True
    report["files"].append(record)
    print("VENDOR_REVIEW_FILE_VERIFIED", name, record["sha256"], len(data))
audit=TMP/"quaternius-source-review.json"
audit.write_text(json.dumps(report,indent=2)+"\n")
print("QUATERNIUS_INPUTS_VERIFIED",len(report["files"]), "NO_PUBLIC_VENDOR_SOURCE_COMMITTED")
