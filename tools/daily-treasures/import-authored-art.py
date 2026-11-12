#!/usr/bin/env python3
"""Import the exact reviewed 512px Eldoria daily chest from its source ZIP.
Runs before Unity, never touches gameplay, does not overwrite different assets.
"""
import argparse
import hashlib
import json
from pathlib import Path, PurePosixPath
import struct
import sys
import zipfile

SHA={
 "treasure_closed_512.png":"313c17d35dcb6f44129a143fabc0448e5a20bc3fb7d00f700394bb73b2d0a0dd",
 "treasure_opened_512.png":"a75afb45b6183d0a8d7e2841c82ef13b812d08fb7620f90c849e679f24429ccb"
}
GUID={
 "treasure_closed_512.png":"bc7cd5adddda42e69f1cfc86d478990e",
 "treasure_opened_512.png":"69a4e0c9aa4a443abf6fe9f379cd86bd"
}
TARGET=Path("Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureArt")

def png_check(data,name):
    if not data.startswith(b"\x89PNG\r\n\x1a\n") or len(data)<100:
        raise ValueError("Not PNG: "+name)
    w,h,depth,kind=struct.unpack_from(">IIBB",data,16)
    if (w,h,depth)!=(512,512,8) or kind not in (3,6):
        raise ValueError("Unexpected sprite dimensions or format: "+name)
    if kind==3 and b"tRNS" not in data:
        raise ValueError("Indexed sprite has no transparency: "+name)
    if hashlib.sha256(data).hexdigest()!=SHA[name]:
        raise ValueError("Source bytes differ from owner-art candidate: "+name)

def meta(guid):
    return ("""fileFormatVersion: 2
guid: %s
TextureImporter:
  serializedVersion: 12
  externalObjects: {}
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  textureFormat: 1
  maxTextureSize: 512
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  alphaIsTransparency: 1
  alphaSource: 1
  textureType: 8
  spriteMode: 1
  spriteMeshType: 0
  spritePixelsToUnits: 100
  spriteAlignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
""" % guid).encode("utf-8")

def main():
    ap=argparse.ArgumentParser(description=__doc__)
    ap.add_argument("zip_file",type=Path)
    ap.add_argument("--repo-root",type=Path,default=Path("."))
    ap.add_argument("--dry-run",action="store_true")
    args=ap.parse_args()
    if not args.zip_file.is_file():
        raise ValueError("No real art ZIP was supplied; do not invent dummy assets")
    payload={}
    with zipfile.ZipFile(args.zip_file) as z:
        names=set(z.namelist())
        if any(x.startswith("/") or ".." in PurePosixPath(x).parts for x in names):
            raise ValueError("Unsafe archive path")
        for name in SHA:
            available=[x for x in ("assets/"+name,"export/"+name) if x in names]
            if len(available)!=1:
                raise ValueError("Missing or ambiguous file in ZIP: "+name)
            raw=z.read(available[0])
            if len(raw)>1000000:raise ValueError("File exceeds safe 1MB size: "+name)
            png_check(raw,name)
            payload[name]=raw
    target=args.repo_root.resolve()/TARGET
    if not target.parent.is_dir():
        raise ValueError("Eldoria module directory is absent: "+str(target.parent))
    files=[]
    for name,raw in payload.items():
        dest=target/name
        files.extend(((dest,raw),(Path(str(dest)+".meta"),meta(GUID[name]))))
    # All-or-nothing validation before writing any bytes.
    for dest,raw in files:
        if dest.exists() and dest.read_bytes()!=raw:
            raise ValueError("Do not overwrite an existing different asset: "+str(dest))
    if not args.dry_run:
        target.mkdir(parents=True,exist_ok=True)
        for dest,raw in files:
            if not dest.exists():dest.write_bytes(raw)
    print(json.dumps({"status":"DRY_RUN_PASS" if args.dry_run else "SOURCE_ASSETS_IMPORTED",
        "files":[str(f[0]) for f in files],"art_approved":False,
        "unity_compiled":False},ensure_ascii=False))

if __name__=="__main__":
    try:main()
    except (OSError,ValueError,zipfile.BadZipFile) as exc:
        print("ELDORIA_ART_IMPORT_FAILED: "+str(exc),file=sys.stderr)
        sys.exit(1)
