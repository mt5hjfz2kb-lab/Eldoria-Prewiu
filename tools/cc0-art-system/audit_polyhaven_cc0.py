#!/usr/bin/env python3
"""Download actual 1K CC0 PBR maps from official Poly Haven, verifying file MD5.
Original publisher's assets CC0: https://polyhaven.com/license
No live API is called by this script; exact URLs and checksums are pinned from
the public official API on 2026-11-16 for reproducibility.
"""
from pathlib import Path
from urllib.request import Request,urlopen
import hashlib,os,json,time

SOURCES={
  "stone_diff":{"url":"https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/stone_brick_wall_001/stone_brick_wall_001_diff_1k.jpg","md5":"fe05d5a4be3e01e7d922e7e935d8dfd9","size":259947},
  "stone_normal":{"url":"https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/stone_brick_wall_001/stone_brick_wall_001_nor_gl_1k.jpg","md5":"a389d00861e02eef39967dde975a8af4","size":355904},
  "stone_rough":{"url":"https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/stone_brick_wall_001/stone_brick_wall_001_rough_1k.jpg","md5":"dac1157e3105f9cd63df26d72873d538","size":197026},
  "slate_diff":{"url":"https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/roof_slates_02/roof_slates_02_diff_1k.jpg","md5":"d7683ac63f6d46159790186f2e19919f","size":515626},
  "slate_normal":{"url":"https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/roof_slates_02/roof_slates_02_nor_gl_1k.jpg","md5":"e661093ac3305df7de9159ad07ef5f75","size":763206},
  "slate_rough":{"url":"https://dl.polyhaven.org/file/ph-assets/Textures/jpg/1k/roof_slates_02/roof_slates_02_rough_1k.jpg","md5":"90922aa9a6fe1bd6b6fc59548e476705","size":307076}
}
ROOT=Path(__file__).resolve().parents[2]
OUT=Path(os.environ.get("ELDORIA_PBR_REVIEW",str(ROOT/"_pbr_temporary_review")))
if str(OUT.resolve()).startswith(str((ROOT/"Unity/Assets").resolve())):
    raise RuntimeError("Do not put third-party material source files directly in Assets without checking license provenance")
OUT.mkdir(parents=True,exist_ok=True)
manifest={"license":"CC0 1.0","license_url":"https://polyhaven.com/license",
          "download_date":"2026-11-16",
          "source_ids":["stone_brick_wall_001","roof_slates_02"],
          "source_api":"https://api.polyhaven.com/files/<asset_id>",
          "authoritative_files_verified":[],"contemporary_asset_rights":"CC0, commercial use and embedded redistribution allowed"}
for key,data in SOURCES.items():
    target=OUT/(key+".jpg")
    if not target.exists():
        last=None
        for attempt in range(3):
            try:
                request=Request(data["url"],headers={"User-Agent":"EldoriaCC0ArtAudit/1.0","Accept":"image/jpeg"})
                with urlopen(request,timeout=35) as response:
                    if response.status!=200:raise RuntimeError("HTTP "+str(response.status))
                    raw=response.read()
                target.write_bytes(raw);break
            except Exception as exc:
                last=exc
                if target.exists():target.unlink()
                if attempt<2:time.sleep(attempt+1)
        if not target.exists():raise RuntimeError("Could not fetch Poly Haven "+key+": "+str(last))
    blob=target.read_bytes()
    digest=hashlib.md5(blob).hexdigest()
    if len(blob)!=data["size"] or digest!=data["md5"] or not blob.startswith(b"\xff\xd8"):
        raise RuntimeError("Publisher MD5 / JPEG / size mismatch for "+key)
    item={"key":key,"md5":digest,"sha256":hashlib.sha256(blob).hexdigest(),
          "bytes":len(blob),"source_url":data["url"]}
    manifest["authoritative_files_verified"].append(item)
    print("POLYHAVEN_REAL_CC0_VERIFIED",key,digest,len(blob))
(OUT/"polyhaven-input-evidence.json").write_text(json.dumps(manifest,indent=2)+"\n")
print("ELDORIA_CC0_PHYSICAL_TEXTURE_INPUTS_PASS",len(manifest["authoritative_files_verified"]))
