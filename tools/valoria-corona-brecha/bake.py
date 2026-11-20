#!/usr/bin/env python3
"""Deterministic original Corona de la Brecha SVG->Unity high-resolution RGBA sprites."""
from pathlib import Path
import hashlib,json
import cairosvg
from PIL import Image
R=Path(__file__).resolve().parents[2]
SOURCE=R/"tools/valoria-corona-brecha/source"
OUT=R/"Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D/UI/CoronaBrecha"
SPEC=json.loads((R/"tools/valoria-corona-brecha/manifest.json").read_text())
def main():
 OUT.mkdir(parents=True,exist_ok=True)
 (OUT.parent/"CoronaBrecha.meta").write_text("fileFormatVersion: 2\nguid: 79f6030ed90712d9a1c8a5bd32f9a186\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")
 report=[]
 for item in SPEC["assets"]:
  name=item["name"];w,h=item["size"];border=item["border"]
  source=(SOURCE/(name+".svg")).read_bytes()
  assert b"<svg" in source and b"</svg>" in source
  dest=OUT/(name+".png")
  # Prioritize professionally illustrated independent original sources.
  # Stable SVGs remain editable/testable fallback; NEVER regenerate over a
  # higher-quality source PNG or silently mix art when a supplied file fails.
  premium=R/"tools/valoria-corona-brecha/premium"/(name+".png")
  if premium.exists():
   # Preserve the artist's original PNG byte-for-byte as the SOURCE in premium/.
   # The artwork's large transparent authoring margins destroyed readability
   # when rendered at 117x43. Crop to nontrivial-alpha bounds and fill the
   # output source frame; use Image.Type.Simple on these complete illustrations.
   art_source=premium.read_bytes()
   frames={"identity_crest","resource_siglet","world_rune","objective_rune",
           "action_arcane","panel_obsidian","portrait_ring"}
   if name in frames:
    with Image.open(premium) as original:
     original.load()
     if original.mode!="RGBA":
      raise ValueError("Original Corona premium art is not RGBA: "+name)
     alpha=original.getchannel("A").point(lambda pixel:255 if pixel>20 else 0)
     bounds=alpha.getbbox()
     if not bounds:
      raise ValueError("Original premium frame is transparent: "+name)
     left,top,right,bottom=bounds
     pad=4
     bounds=(max(0,left-pad),max(0,top-pad),
             min(original.width,right+pad),min(original.height,bottom+pad))
     actual=original.crop(bounds).resize(original.size,Image.Resampling.LANCZOS)
     actual.save(dest,optimize=True,compress_level=9)
    origin="original_illustration_png_opaque_art_bounds_normalized"
   else:
    import shutil
    shutil.copyfile(premium,dest)
    origin="original_illustration_png_exact_bytes"
  else:
   cairosvg.svg2png(bytestring=source,write_to=str(dest),output_width=2*w,output_height=2*h)
   origin="original_deterministic_svg_fallback"
   art_source=source
  with Image.open(dest) as im:
   im.load()
   assert im.mode=="RGBA",(name,im.mode)
   assert im.width>=120 and im.height>=120,(name,im.size)
   assert im.getpixel((0,0))[3]<40,name
   image_size=list(im.size)
  guid=hashlib.md5(("Eldoria-Corona-Brecha-"+name).encode()).hexdigest()
  m=("fileFormatVersion: 2\nguid: "+guid+"\nTextureImporter:\n"
    "  serializedVersion: 12\n  externalObjects: {}\n  mipmaps:\n"
    "    enableMipMap: 0\n  sRGBTexture: 1\n  isReadable: 0\n"
    "  textureFormat: 1\n  maxTextureSize: 2048\n  textureSettings:\n"
    "    serializedVersion: 2\n    filterMode: 1\n    aniso: 1\n    mipBias: 0\n"
    "    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n"
    "  nPOTScale: 0\n  alphaIsTransparency: 1\n  alphaSource: 1\n"
    "  textureType: 8\n  spriteMode: 1\n  spriteMeshType: 0\n"
    "  spritePixelsToUnits: 200\n  spriteAlignment: 0\n"
    "  spritePivot: {x: 0.5, y: 0.5}\n"
    "  spriteBorder: {x: "+str(2*border)+", y: "+str(2*border)+
    ", z: "+str(2*border)+", w: "+str(2*border)+"}\n")
  (OUT/(name+".png.meta")).write_text(m)
  report.append({**item,"guid":guid,"sha256":hashlib.sha256(dest.read_bytes()).hexdigest(),"source_sha256":hashlib.sha256(art_source).hexdigest(),"source_type":origin,"export_size":image_size,"svg_fallback_sha256":hashlib.sha256(source).hexdigest()})
  print("CORONA_ORIGINAL_PNG",name,report[-1]["sha256"])
 proof=R/"docs/valoria-modular-25d/CORONA_BRECHA_SPRITE_PROVENANCE.json"
 proof.parent.mkdir(parents=True,exist_ok=True)
 proof.write_text(json.dumps({"schema":1,"source":"premium original illustrated PNGs with independent SVG fallbacks","visual_approved":False,"unity_runtime_tested":False,"mechanics_changed":False,"sprites":report},indent=2)+"\n")
 assert len(report)==15
 print("CORONA_ART_SOURCE_BAKE_PASS",len(report))
if __name__=="__main__":main()
