#!/usr/bin/env python3
"""Move verified original UI kit into first-party Eldoria and generate stable Unity metadata."""
from pathlib import Path
from hashlib import sha256
import re, shutil
root=Path(__file__).resolve().parents[2]
old=root/"Unity/Assets/EldoriaVisualKit"
dest=root/"Unity/Assets/Eldoria/VisualKit"
if not old.is_dir() or dest.exists():
    raise SystemExit("FORJA_RELOCATION_FAIL_CLOSED: expected old root only, new root absent")
dest.parent.mkdir(parents=True,exist_ok=True)
shutil.move(str(old),str(dest))
for cs in dest.rglob("*.cs"):
    body=cs.read_text()
    body=body.replace("Assets/EldoriaVisualKit/","Assets/Eldoria/VisualKit/")
    cs.write_text(body)
source=[p for p in dest.rglob("*") if p.is_file() and p.suffix in {".png",".cs",".asmdef"}]
assert len(source)==48,("FORJA_INPUT_COUNT",len(source))
def guid(p):return sha256(("eldoria-forja-first-party-v1:"+p.relative_to(root).as_posix()).encode()).hexdigest()[:32]
def border(name):
    if name.startswith(("button_primary_","button_secondary_")) or name in ("panel_dark","panel_light","panel_quest"):return 56
    if name.startswith("nav_tile_"):return 48
    if name in ("resource_chip","tab_normal","tab_selected"):return 36
    if name=="badge_level":return 32
    if name.startswith("progress_"):return 20
    if name=="toast_background":return 40
    return 0
def meta(p):
    header="fileFormatVersion: 2\nguid: "+guid(p)+"\n"
    if p.is_dir():
        return header+"""folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    if p.suffix==".cs":
        return header+"""MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    if p.suffix==".asmdef":
        return header+"""AssemblyDefinitionImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
    assert p.suffix==".png",p
    b=border(p.stem) if "/Surfaces/" in p.as_posix() else 0
    return header+f"""TextureImporter:
  serializedVersion: 13
  internalIDToNameTable: []
  externalObjects: {{}}
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  textureFormat: 1
  maxTextureSize: 2048
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
  textureShape: 1
  spriteMode: 1
  spriteMeshType: 1
  spritePixelsToUnits: 100
  spriteBorder: {{x: {b}, y: {b}, z: {b}, w: {b}}}
  spriteGenerateFallbackPhysicsShape: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""
folders=[dest]+sorted((p for p in dest.rglob("*") if p.is_dir()),key=lambda p:len(p.parts))
created=[]
for p in folders+source:
    path=Path(str(p)+".meta")
    assert not path.exists(),("METADATA_ALREADY_EXISTS",path)
    path.write_text(meta(p))
    created.append(path)
ids=[re.search(r"(?m)^guid: ([0-9a-f]{32})$",p.read_text()).group(1) for p in created]
assert len(created)==55 and len(set(ids))==55,("INVALID_META_GUIDS",len(created),len(set(ids)))
assert not old.exists()
print("FORJA_FIRST_PARTY_RELOCATION_PASS: 48 genuine source assets and 55 stable metadata")
