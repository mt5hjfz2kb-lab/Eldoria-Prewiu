#!/usr/bin/env python3
"""Import single transparent atmospheric fissure motif from user-approved image.

This replaces (never overlays) the procedural X/star, but not any game
interaction or camera. Gold hairline / orange crack pixels are isolated from
the original owner's vertical design using a color-key alpha and a soft mask.
"""
import base64,hashlib,io,json
from pathlib import Path
from PIL import Image
SOURCE=Path("tools/breach-watch/source/owner-rift-separator.webp.b64")
SHA="f3faa8014ecdb658560d02ba7e04cd0514aec7d6b179a135440ef45e89b1962c"
DEST=Path("Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/OwnerControls")
GUID="f3e70c1d5da84ca38e91b63904ff7e6a"
META="""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  serializedVersion: 12
  externalObjects: {{}}
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  maxTextureSize: 256
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
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
    raw=base64.b64decode(SOURCE.read_text().strip(),validate=True)
    assert hashlib.sha256(raw).hexdigest()==SHA
    image=Image.open(io.BytesIO(raw)).convert('RGBA')
    assert image.size==(198,50)
    assert image.getpixel((0,0))[3]<20
    assert image.getpixel((98,25))[3]>160
    out=DEST/'breach_signal.png'
    DEST.mkdir(parents=True,exist_ok=True)
    image.save(out,optimize=True)
    (DEST/'breach_signal.png.meta').write_text(META.format(guid=GUID))
    proof={
        'name':'breach_signal','size':[198,50],
        'source_sha256':SHA,'png_sha256':hashlib.sha256(out.read_bytes()).hexdigest(),
        'guid':GUID,'art_approved':False,
        'origin':'One transparent orange/gold fissure extracted from owner-approved portrait',
        'source_is_not_gameplay':True}
    (DEST/'breach_signal_provenance.json').write_text(
        json.dumps(proof,ensure_ascii=False,indent=2)+'\n')
    print('VIGILIA_AUTHENTIC_USER_APPROVED_GOLD_FISSURE_IMPORT_PASS')
if __name__=='__main__':
    main()
