#!/usr/bin/env python3
"""Import three recognizable real CC0 resource icons into the NEW Vigilia HUD.

This only reads the published pixel source; it never touches production RoyalHUD,
camera, save, Forja assets or the user's current game version.
"""
import hashlib
import io
import json
import pathlib
import urllib.request
import zipfile
from PIL import Image

URL = "https://annomotion.studio/packs/fantasy-rpg-item-icons/download"
SOURCE_SHA = "20f324feb652b2a5959c634a95c694512d21193d346d2204bddd0e0d350bbe75"
OUT = pathlib.Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/ResourceIcons")

# Real 12x4 PSD sheet, exactly 512px per illustrated item.
PIECES = {
    "food": (3,0,"3510b17fa8ab660346343b1c6ec7cf6b6abc9db49bc9f1b76e34209823ab55f2",
             "1b740a65de65461494ac685afef8c0f1"),
    "stone": (3,4,"05d760420c88ccb216768088a4e12616f263715f53d1777a4db61e83d93f90e4",
              "252920a7416d45479d45f302a1e8d620"),
    "wood": (3,5,"da04372901d424859224edaaf80098b190b43f0404910589dc1cd7f213b38e57",
             "988748ef59b74ca19ed47754f47f0a22"),
}
FOLDER_META="""fileFormatVersion: 2
guid: 4be19b6f6c8c43fd87b24de03427c6a1
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
PNG_META="""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  serializedVersion: 12
  externalObjects: {{}}
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
  spritePivot: {{x: 0.5, y: 0.5}}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
"""
def main():
    request=urllib.request.Request(URL,headers={"User-Agent":"Eldoria-Vigilia-CC0-resource-validation"})
    with urllib.request.urlopen(request,timeout=60) as response:
        data=response.read(20_000_001)
    if not (100_000<len(data)<=20_000_000 and
            hashlib.sha256(data).hexdigest()==SOURCE_SHA):
        raise RuntimeError("Unexpected publisher archive/version/size, no art modified")
    with zipfile.ZipFile(io.BytesIO(data)) as z:
        files=set(z.namelist())
        if files != {"am_free_rpg_icons.psd","free_assets_Fantasy_rpg_items.jpg","LICENCE.txt"}:
            raise RuntimeError("Unexpected file inventory; do not trust uninspected source")
        license_text=z.read("LICENCE.txt").decode("utf-8")
        if "CC0 1.0" not in license_text or "public domain" not in license_text:
            raise RuntimeError("Pack is not the CC0 licence checked previously")
        psd=Image.open(io.BytesIO(z.read("am_free_rpg_icons.psd")))
        psd.seek(1)
        sheet=psd.convert("RGBA")
        if sheet.size != (6144,2048):
            raise RuntimeError("Unknown artwork layout")
    OUT.mkdir(parents=True,exist_ok=True)
    (OUT.parent / "ResourceIcons.meta").write_text(FOLDER_META,encoding="utf-8")
    results=[]
    for kind,(row,col,expected,guid) in PIECES.items():
        cell=sheet.crop((col*512,row*512,(col+1)*512,(row+1)*512))
        if hashlib.sha256(cell.tobytes()).hexdigest()!=expected:
            raise RuntimeError("The selected source icon changed: "+kind)
        bbox=cell.getchannel("A").getbbox()
        if not bbox:raise RuntimeError("No actual pixels in "+kind)
        clip=cell.crop(bbox)
        canvas=Image.new("RGBA",(512,512))
        max_side=max(clip.size)
        scale=440/max_side
        resized=clip.resize((round(clip.width*scale),round(clip.height*scale)),Image.Resampling.LANCZOS)
        canvas.alpha_composite(resized,((512-resized.width)//2,(512-resized.height)//2))
        canvas=canvas.resize((256,256),Image.Resampling.LANCZOS)
        name=kind+".png"
        path=OUT/name
        canvas.save(path,optimize=True)
        (OUT/(name+".meta")).write_text(PNG_META.format(guid=guid),encoding="utf-8")
        results.append({"name":kind,"sha256":hashlib.sha256(path.read_bytes()).hexdigest(),
                        "source_cell_rgba":expected,"guid":guid,"size":[256,256]})
    (OUT/"LICENSE_CC0_RESOURCE_SOURCE.txt").write_text(license_text,encoding="utf-8")
    (OUT/"manifest.json").write_text(json.dumps({
        "origin":URL,"source_sha256":SOURCE_SHA,"license":"CC0 1.0",
        "original_file":"am_free_rpg_icons.psd","sources":results,
        "scope":"Vigilia independent new HUD only; no HUD skin/reused control"
    },ensure_ascii=False,indent=2),encoding="utf-8")
    print("BREACHWATCH_CC0_ILLUSTRATED_RESOURCES_CREATED "+json.dumps(results))
if __name__=="__main__":main()
