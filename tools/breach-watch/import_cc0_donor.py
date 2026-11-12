#!/usr/bin/env python3
"""Isolated, reproducible extraction of selected first-party licensed CC0 UI art.

Only the specific tested source archive is accepted. Crops contain ONE original
empty illustrated panel/medallion; no old RoyalHUD or Forja art is used.
The generated Unity GUIDs are stable and content remains in the PR, not main.
"""
import hashlib
import io
import json
import pathlib
import urllib.request
import zipfile
from PIL import Image, ImageEnhance

URL = "https://annomotion.studio/public/packs/dark-fantasy-ui-hud-kit/download"
ARCHIVE_SHA = "f089903c2a515c1737344772c538e2bc2d8a153e219096d9118f74db5bf47d76"
ART_ROOT = pathlib.Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt")
RECIPES = {
    "nav_sculpted_medallion": ((860,94,959,199),"e5ca6a18be1944a8a780b9f9a8f61892",(0,0,0,0)),
    "portrait_forged_iron": ((0,190,104,296),"2b38a86c7b6a4faf874c1433d995c2ae",(76,76,76,76)),
    "valoria_burgundy_banner": ((765,192,862,298),"6d983814e38d431586bb8f9cabd4a063",(0,0,0,0)),
    "action_antique_bronze": ((864,210,959,292),"bed3b6d3f7754323a2dc41844acf3b6f",(52,28,52,28)),
}
META = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  serializedVersion: 12
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
  spriteMode: 1
  spriteMeshType: 0
  spritePixelsToUnits: 360
  spriteAlignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spriteBorder: {{x: {left}, y: {bottom}, z: {right}, w: {top}}}
"""
FOLDER_META = """fileFormatVersion: 2
guid: 401ac13fe93d47e49d7478e7fce38931
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"""

def main():
    req=urllib.request.Request(URL,headers={"User-Agent":"Eldoria-BreachWatch-license-verification/1.0"})
    with urllib.request.urlopen(req,timeout=40) as response:
        raw=response.read(20_000_001)
    if not (10_000<len(raw)<20_000_000 and
            hashlib.sha256(raw).hexdigest()==ARCHIVE_SHA):
        raise RuntimeError("Publisher source archive differs from approved immutable hash")
    with zipfile.ZipFile(io.BytesIO(raw)) as pack:
        names=pack.namelist()
        if set(names)!={"am_free_dark_fantasy.psd","Dark Fantasy Thumbnail.jpg","LICENCE.txt"}:
            raise RuntimeError("Archive content changed unexpectedly")
        license_text=pack.read("LICENCE.txt").decode("utf-8")
        if "CC0 1.0" not in license_text or "public domain" not in license_text:
            raise RuntimeError("Publisher's distributed licence does not match CC0 claim")
        image=Image.open(io.BytesIO(pack.read("am_free_dark_fantasy.psd")))
        image.seek(1)
        image=image.convert("RGBA")
        if image.size!=(5120,2048):
            raise RuntimeError("Unexpected PSD composite dimensions")
    ART_ROOT.mkdir(parents=True,exist_ok=True)
    (ART_ROOT.parent.parent/"Resources.meta").write_text(
        "fileFormatVersion: 2\nguid: 9b96e204e7384b3a85762ad1b6f38965\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n",encoding="utf-8")
    (ART_ROOT.parent/"VigiliaArt.meta").write_text(FOLDER_META,encoding="utf-8")
    items=[]
    for key,(coords,guid,border) in RECIPES.items():
        area=tuple(round(v*scale) for v,scale in zip(coords,(5120/960,2048/384,5120/960,2048/384)))
        cropped=image.crop(area)
        bbox=cropped.getchannel("A").getbbox()
        if not bbox:raise RuntimeError("No painted pixels in "+key)
        cropped=cropped.crop(bbox)
        if key=="action_antique_bronze":
            # Damp the stock yellow: Valoria uses burnished copper, not royal gold.
            rgb=ImageEnhance.Brightness(cropped.convert("RGB")).enhance(.62)
            cropped=Image.merge("RGBA",(*rgb.split(),cropped.getchannel("A")))
        path=ART_ROOT/(key+".png")
        cropped.save(path,optimize=True)
        (ART_ROOT/(key+".png.meta")).write_text(META.format(
            guid=guid,left=border[0],bottom=border[1],right=border[2],top=border[3]),encoding="utf-8")
        items.append({"name":key,"size":cropped.size,"sha256":hashlib.sha256(path.read_bytes()).hexdigest(),
                      "license":"CC0 1.0","source":"am_free_dark_fantasy.psd"})
    (ART_ROOT/"LICENSE_CC0_SOURCE.txt").write_text(license_text,encoding="utf-8")
    (ART_ROOT/"source-manifest.json").write_text(json.dumps({
        "origin":URL,"publisher":"AnnoMotion / ArcadeOn Studios","archive_sha256":ARCHIVE_SHA,
        "license":"CC0 1.0 in archive", "format":"PSD layered source; extracted transparent sprite crops",
        "purpose":"independent Vigilia candidate; not final art approval", "sprites":items
    },ensure_ascii=False,indent=2),encoding="utf-8")
    print("BREACHWATCH_CC0_CANONICAL_ART_EXTRACTED "+json.dumps(items))

if __name__=="__main__":main()
