#!/usr/bin/env python3
"""Reproducible upsampling for the isolated City2D candidate.

A quality-preserving RESAMPLE, not new high-resolution source detail. The
unchanged authored image remains available; file and settings are reversible.
"""
from pathlib import Path
from PIL import Image, ImageFilter
import hashlib, json, re

ROOT=Path(__file__).resolve().parents[1]
SRC=ROOT/"Sprites/Terreno_Organico.png"
DST=ROOT/"Sprites/Retina/Terreno_Organico.png"
META=ROOT/"Sprites/Terreno_Organico.png.meta"
MAN=ROOT/"QA/retina_manifest.json"

def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def bake():
    with Image.open(SRC) as original:
        assert original.size==(941,1672) and original.mode=="RGB"
        baked=original.resize((1412,2508),Image.Resampling.LANCZOS)
        baked=baked.filter(ImageFilter.UnsharpMask(radius=1.05,percent=95,threshold=3))
        DST.parent.mkdir(parents=True,exist_ok=True)
        baked.save(DST,format="PNG",optimize=True)
    original_meta=META.read_text(encoding="utf-8")
    guid=hashlib.md5(b"Eldoria/Ciudad2D/Retina/Terreno_Organico.png").hexdigest()
    meta=re.sub(r"guid: [a-f0-9]{32}","guid: "+guid,original_meta,count=1)
    meta=meta.replace("spritePixelsToUnits: 100","spritePixelsToUnits: 150")
    meta=meta.replace("maxTextureSize: 2048","maxTextureSize: 4096")
    assert "spritePixelsToUnits: 150" in meta
    DST.with_suffix(".png.meta").write_text(meta,encoding="utf-8")
    data={
       "schema":1,"kind":"resampling_not_new_detail",
       "source":"Sprites/Terreno_Organico.png","source_sha256":sha(SRC),
       "retina":"Sprites/Retina/Terreno_Organico.png","retina_sha256":sha(DST),
       "original_size":[941,1672],"baked_size":[1412,2508],
       "ppu_original":100,"ppu_baked":150,
       "method":"Pillow 11.3.0 Lanczos 1.5x + UnsharpMask r1.05 p95 threshold3",
       "limitations":["No additional original semantic detail","Not certified on mobile memory/GPU"],
    }
    MAN.write_text(json.dumps(data,indent=2,ensure_ascii=False)+"\n",encoding="utf-8")
    print("CITY2D_RETINA_BAKE_PASS",sha(DST),DST.stat().st_size)

if __name__=="__main__":bake()
