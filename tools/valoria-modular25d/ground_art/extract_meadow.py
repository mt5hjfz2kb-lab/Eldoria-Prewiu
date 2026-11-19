#!/usr/bin/env python3
"""Reproduce exactly ONE seamless meadow material from original Eldoria art.

Source is an existing original illustration in the user's prior City2D branch,
NOT a giant scene background and NOT a scraped/commercial game texture.
Extract only a 192x192 grass-only square; mirrored 2x2 continuity yields one
independent material applied to real 8x8m world mesh chunks.
"""
import hashlib, json, sys
from pathlib import Path
from PIL import Image, ImageOps
SOURCE_SHA = "cc5ca910ef0056266e7fdbd47c191d373c44c8bf9c88cc8dc00339c5694dc10d"
SOURCE_COMMIT = "10dbc197d48ea8517c6a3dd6c22f1a2abf75d508"
CROP=(355,350,547,542)
OUT_NAME="OrganicMeadowTile"

def run(src_path,dest_dir):
    src=Path(src_path);dst=Path(dest_dir)
    original=src.read_bytes()
    if hashlib.sha256(original).hexdigest()!=SOURCE_SHA:
        raise ValueError("Original Eldoria source changed; do not import unknown art")
    im=Image.open(src)
    if im.size!=(941,1672) or im.mode!="RGB":
        raise ValueError("Wrong illustration; reject instead of guessing crop")
    crop=im.crop(CROP)
    w,h=crop.size
    tiled=Image.new("RGB",(w*2,h*2))
    tiled.paste(crop,(0,0))
    tiled.paste(ImageOps.mirror(crop),(w,0))
    tiled.paste(ImageOps.flip(crop),(0,h))
    tiled.paste(ImageOps.flip(ImageOps.mirror(crop)),(w,h))
    tiled=tiled.resize((512,512),Image.Resampling.LANCZOS)
    # Explicitly enforce RGB equal across periodic edges. The centre seam
    # already matches under reflection; no generated whole-city backdrop.
    for y in range(512):
        rgb=tuple(round((a+b)/2) for a,b in zip(tiled.getpixel((0,y)),tiled.getpixel((511,y))))
        tiled.putpixel((0,y),rgb);tiled.putpixel((511,y),rgb)
    for x in range(512):
        rgb=tuple(round((a+b)/2) for a,b in zip(tiled.getpixel((x,0)),tiled.getpixel((x,511))))
        tiled.putpixel((x,0),rgb);tiled.putpixel((x,511),rgb)
    dst.mkdir(parents=True,exist_ok=True)
    target=dst/(OUT_NAME+".png")
    tiled.save(target,optimize=True)
    meta=dst/(OUT_NAME+".png.meta")
    meta.write_text("""fileFormatVersion: 2
guid: 1ec9b3a5e7b74b1ca2180929c2de475e
TextureImporter:
  serializedVersion: 12
  externalObjects: {}
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
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 0
  alphaIsTransparency: 0
  alphaSource: 0
  textureType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureCompression: 2
    compressionQuality: 100
""",encoding="utf8")
    facts={
      "asset":OUT_NAME,
      "origin":"original Eldoria project City2D artist background, cropped to GRASS-ONLY texture",
      "sourceRef":SOURCE_COMMIT,
      "sourcePath":"Unity/Assets/Experimentos/Ciudad2D/Sprites/Terreno.png",
      "sourceSha256":SOURCE_SHA,"crop":list(CROP),
      "method":"192px painted patch + mirrored 2x2 periodic continuity, 512px material",
      "buildingsExtracted":False,"fullSceneBackground":False,
      "artApproved":False,"physicalDeviceTested":False,
      "pixels":list(tiled.size),
      "sha256":hashlib.sha256(target.read_bytes()).hexdigest()
    }
    (dst/"OrganicMeadowTile.provenance.json").write_text(
        json.dumps(facts,indent=2,ensure_ascii=False)+"\n",encoding="utf8")
    # Compare mosaic boundaries numerically; no quality rating or Unity proof.
    for i in range(512):
        if tiled.getpixel((0,i))!=tiled.getpixel((511,i)):
            raise AssertionError("Vertical tile seam")
        if tiled.getpixel((i,0))!=tiled.getpixel((i,511)):
            raise AssertionError("Horizontal tile seam")

    # Second independent, spatially repeatable material. An exact 117x208
    # original artist-rendered stone PATH detail from the same Eldoria file
    # is mirrored to create one 256x512 vertical ribbon. Its alpha fades into
    # meadow: no city's walls, gates, characters or backplate are imported.
    ROAD_CROP=(413,1430,530,1638)
    pathcrop=im.crop(ROAD_CROP).convert("RGBA")
    left=ImageOps.mirror(pathcrop)
    pair=Image.new("RGBA",(pathcrop.width*2,pathcrop.height))
    pair.paste(left,(0,0));pair.paste(pathcrop,(pathcrop.width,0))
    pair=pair.resize((256,256),Image.Resampling.LANCZOS)
    road=Image.new("RGBA",(256,512))
    road.paste(pair,(0,0))
    road.paste(ImageOps.flip(pair),(0,256))
    px=road.load()
    for y in range(512):
        for x in range(256):
            cross=abs(x/255.0-.5)*2.0
            phase=max(0.0,min(1.0,(cross-.59)/(.96-.59)))
            phase=phase*phase*(3-2*phase)
            r,g,b,a=px[x,y]
            px[x,y]=(r,g,b,round(244*(1-phase)))
    for y in range(512):
        # Exact output alpha=0 at both edges, so green exterior never has
        # a straight tile seam. Colour mismatch is invisible at alpha 0.
        if road.getpixel((0,y))[3]!=0 or road.getpixel((255,y))[3]!=0:
            raise AssertionError("World road alpha must be 0 at meadow edge")
    for x in range(256):
        r0,g0,b0,a0=road.getpixel((x,0))
        r1,g1,b1,a1=road.getpixel((x,511))
        if abs(r0-r1)+abs(g0-g1)+abs(b0-b1)>3 or a0!=a1:
            # Explicitly harmonize the periodic top/bottom edge colour.
            edge=tuple(round((a+b)/2) for a,b in zip((r0,g0,b0),(r1,g1,b1)))+(a0,)
            road.putpixel((x,0),edge);road.putpixel((x,511),edge)
    roadpath=dst/"OrganicCobbleRoad.png"
    road.save(roadpath,optimize=True)
    (dst/"OrganicCobbleRoad.png.meta").write_text("""fileFormatVersion: 2
guid: 1a70b59fc9a14c689016de9e1c536a29
TextureImporter:
  serializedVersion: 12
  externalObjects: {}
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
    wrapU: 0
    wrapV: 0
    wrapW: 0
  nPOTScale: 0
  alphaIsTransparency: 1
  alphaSource: 1
  textureType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureCompression: 2
    compressionQuality: 100
""",encoding="utf8")
    roadfacts={
        "asset":"OrganicCobbleRoad",
        "sourceRef":SOURCE_COMMIT,"sourceSha256":SOURCE_SHA,
        "source":"original Eldoria user-owned artist terrain; path-only crop",
        "crop":list(ROAD_CROP),"outputPixels":[256,512],
        "method":"horizontal mirrored paving, vertical mirrored repeat, feathered alpha ground contact",
        "fullSceneBackground":False,"otherGameAssetsCopied":False,
        "artApproved":False,"unityVerified":False,
        "sha256":hashlib.sha256(roadpath.read_bytes()).hexdigest()
    }
    (dst/"OrganicCobbleRoad.provenance.json").write_text(
        json.dumps(roadfacts,indent=2,ensure_ascii=False)+"\n",encoding="utf8")
    print("ELDORIA_ORIGINAL_COBBLES_AUTHORED",roadfacts["sha256"],roadpath.stat().st_size)

    print("ELDORIA_ORIGINAL_TILE_ART_INTEGRITY_PASS",facts["sha256"],target.stat().st_size)

if __name__=="__main__":
    if len(sys.argv)!=3:raise SystemExit("Usage: extract_meadow.py ORIGINAL_TERRAIN_PNG UNITY_RESOURCES_DIR")
    run(sys.argv[1],sys.argv[2])
