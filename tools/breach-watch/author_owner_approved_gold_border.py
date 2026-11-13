#!/usr/bin/env python3
"""Extract a genuine complete gold border from the exact owner-approved
Vigilia vertical illustration, retaining only its original metal outer ring.

No castle, words, prices, characters, old RoyalHUD or background screenshot is
baked into the Unity image. The blank interior stays fully transparent so
actual Unity text, costs, actions and real 3D building remain separate children.
The source is a user-approved visual reference, not a Unity proof.
"""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter, ImageChops
import hashlib, json

SOURCE=Path("tools/breach-watch/source/owner_gold_reference.webp")
DEST=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/ApprovedContour")
SOURCE_SHA="79e2264961b3ea4462f9fc3dc65f513e2f4e76f59f4f6b2e3d27a5d06013a5a1"
GUID="a4fb6c6c38eb4651a5d44acb8fe939da"
FOLDER_META="""fileFormatVersion: 2
guid: 13599ba9cfe049408d45be8fa339819e
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
"""
SPRITE_META="""fileFormatVersion: 2
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
  spritePixelsToUnits: 200
  spriteAlignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spriteBorder: {{x: 53, y: 61, z: 53, w: 60}}
"""

def main():
    data=SOURCE.read_bytes()
    assert hashlib.sha256(data).hexdigest()==SOURCE_SHA
    img=Image.open(SOURCE).convert("RGBA")
    assert img.size==(256,427)
    # The separate lower finial formerly duplicated/distorted the actual
    # lower gem over the gold frame. Use it now only as a local intermediate:
    # the final SINGLE image includes all ornamentation, and Unity does not
    # paint an extra component on top.
    finial=img.crop((98,377,158,427))
    mask=Image.new("L",finial.size,0)
    ImageDraw.Draw(mask).ellipse((2,1,finial.width-2,finial.height-1),fill=255)
    mask=mask.filter(ImageFilter.GaussianBlur(1))
    finial.putalpha(ImageChops.multiply(finial.getchannel("A"),mask))
    # The frame itself must have a transparent center: no jewel or
    # vertical gold line can cross real Unity text, costs or the CTA.
    px=img.load()
    for y in range(53,415):
        for x in range(43,213):
            r,g,b,_=px[x,y]
            px[x,y]=(r,g,b,0)
    for y in range(415,427):
        for x in range(101,156):
            r,g,b,_=px[x,y]
            px[x,y]=(r,g,b,0)
    # Restore only the feathered approved gem IN the same gold border file.
    # This replaces the old layered second Image, rather than stacking it.
    img.alpha_composite(finial,(98,377))
    a=img.getchannel("A")
    assert a.getpixel((0,0))==0, "Screenshot exterior must be transparent"
    assert a.getpixel((128,213))==0, "No baked price/letters/castle may cover game data"
    assert a.getpixel((17,150))>80, "Source metalwork ring missing"
    assert a.getpixel((128,20))>60, "Top source diamond missing"
    DEST.mkdir(parents=True,exist_ok=True)
    (DEST.parent/"ApprovedContour.meta").write_text(FOLDER_META,encoding="utf8")
    png=DEST/"owner_gold_border.png"
    img.save(png,optimize=True)
    (DEST/"owner_gold_border.png.meta").write_text(
        SPRITE_META.format(guid=GUID),encoding="utf8")
    manifest={
        "source_sha256":SOURCE_SHA,
        "origin":"Exact owner-approved 864x1536 vertical Vigilia concept border, user-provided in chat",
        "pixel_art_derived_only_from_user_approved_concept":True,
        "new_independent_uGUI_one_piece_border":True,
        "no_baked_player_data_or_real_gameplay":True,
        "size":[256,427],
        "png_sha256":hashlib.sha256(png.read_bytes()).hexdigest(),
        "guid":GUID,
        "art_approval":"pending real Unity exact-head screenshot review"
    }
    (DEST/"manifest.json").write_text(json.dumps(manifest,indent=2,ensure_ascii=False))
    print("VIGILIA_EXACT_APPROVED_ART_RING_SPRITE_CREATED",manifest["png_sha256"])

if __name__=="__main__":
    main()
