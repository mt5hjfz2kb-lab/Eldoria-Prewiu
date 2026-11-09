#!/usr/bin/env python3
"""Use three independent text-free controls from the owner's chosen image.

The *actual source* is the owner's 864x1536 approved Vigilia image. Only
separate control fragments (not the screenshot) were kept. Text and numeric
amounts were removed before pinning the three source WebPs. All player
identity, resources, upgrades and button interaction remain native Unity.
Never use the composed image as a HUD background or claim native art approval.
"""
import base64,hashlib,io,json
from pathlib import Path
from PIL import Image

BASE=Path("tools/breach-watch/source")
DEST=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/OwnerControls")
SPEC={
  "profile":{
    "source":"owner-profile-blank.webp.b64",
    "source_sha256":"19f4e3be74818f7c773521e4a0cc490570b6d6313ce278ca6c355a5ce3fbbc8e",
    "output":"profile_bar",
    "size":[257,66],
    "border":[15,10,32,11],
    "guid":"a12e48665c36425f8fe3baa01e8c3967"},
  "resource":{
    "source":"owner-resource-blank.webp.b64",
    "source_sha256":"67807c607d61717493af032b5333c9af3c95fa2354a19eaeb8748f4266535133",
    "output":"resource_capsule",
    "size":[135,52],
    "border":[20,9,20,9],
    "guid":"ccb5b1b76e774a508f0cd3abf77312a2"},
  "action":{
    "source":"owner-upgrade-blank.webp.b64",
    "source_sha256":"099c9c4684d00183367899c5c0ebccc85f036f6d37004a1f787dc551af201a58",
    "output":"upgrade_button",
    "size":[246,60],
    "border":[78,11,22,9],
    "guid":"9c3913dbbcb043bd8ef494a3ef7f443a"}
}
FOLDER_META="""fileFormatVersion: 2
guid: 7292f4fef15843a8b2676a1b0f154b6d
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
IMAGE_META="""fileFormatVersion: 2
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
  spriteBorder: {{x: {left}, y: {bottom}, z: {right}, w: {top}}}
"""
def main():
    DEST.mkdir(parents=True,exist_ok=True)
    (DEST.parent/"OwnerControls.meta").write_text(FOLDER_META,encoding="utf-8")
    provenance=[]
    for key,entry in SPEC.items():
        value=(BASE/entry["source"]).read_text().strip()
        source=base64.b64decode(value,validate=True)
        assert hashlib.sha256(source).hexdigest()==entry["source_sha256"],key
        image=Image.open(io.BytesIO(source)).convert("RGBA")
        assert list(image.size)==entry["size"],(key,image.size)
        assert image.getpixel((0,0))[3]<128,key
        # An independent painted object: NO price, state, real player name
        # or action caption is part of the output. The small hammers are
        # genuinely authored within the approved burgundy button itself.
        out=DEST/(entry["output"]+".png")
        image.save(out,optimize=True)
        left,bottom,right,top=entry["border"]
        (DEST/(entry["output"]+".png.meta")).write_text(
            IMAGE_META.format(guid=entry["guid"],left=left,bottom=bottom,
                              right=right,top=top),encoding="utf-8")
        provenance.append({
            "control":key,"name":entry["output"],
            "source_sha256":entry["source_sha256"],
            "png_sha256":hashlib.sha256(out.read_bytes()).hexdigest(),
            "dimensions":entry["size"],"border":entry["border"],
            "guid":entry["guid"],
            "origin":"owner-provided 864x1536 Vigilia concept, independently extracted text-free detail",
            "no_baked_gameplay_text":True
        })
    (DEST/"manifest.json").write_text(json.dumps({
        "origin":"User-selected owner reference image, not an old Eldoria HUD",
        "usage":"three independent live Unity UI pieces, not a fake screenshot",
        "art_approved":False,
        "controls":provenance,
    },ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print("VIGILIA_APPROVED_INDEPENDENT_CONTROLS_SOURCE_PASS",len(provenance))
if __name__=="__main__":
    main()
