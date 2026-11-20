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
def artist_aspect_recompose(image,target_width):
 """Reuse ONLY original painted source pixels; preserve left/right gemstone
 terminals plus the central diamond at their correct aspect, expanding
 solely the left and right enamel fields. This is FIVE-region remapping,
 not Unity 9-slice across the crystal nor a composed city screenshot.
 Source must be the premium 580x260 RGBA art. Target widths correspond
 to the real Unity 180x44 badge and 270x42 Build CTA.
 """
 assert image.size==(580,260),("premium art size changed",image.size)
 assert target_width in (1036,1665)
 cuts=(0,110,253,327,470,580)
 west=(target_width-294)//2
 widths=(110,west,74,target_width-294-west,110)
 assert sum(widths)==target_width
 output=Image.new("RGBA",(target_width,260))
 start=0
 for index,(x0,x1) in enumerate(zip(cuts,cuts[1:])):
  strip=image.crop((x0,0,x1,260))
  if strip.width!=widths[index]:
   strip=strip.resize((widths[index],260),Image.Resampling.LANCZOS)
  output.alpha_composite(strip,(start,0))
  start+=widths[index]
 assert start==target_width
 return output

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
     # Text-safe insets: the painted jewel in the center of the
     # lozenge clashed with MADERA / PIEDRA / COMIDA and VALORIA. Use
     # the original outer filigree, add a deep enamel writing field
     # WITHIN the artifact instead of floating black HUD labels.
     insets={
       "resource_siglet":(200,47,540,215),
       "identity_crest":(315,52,552,212),
       "objective_rune":(165,49,525,208),
       "world_rune":(112,48,515,215)
     }
     if name in insets:
      from PIL import ImageDraw
      mask=Image.new("RGBA",actual.size)
      painter=ImageDraw.Draw(mask)
      x0,y0,x1,y1=insets[name]
      painter.rounded_rectangle((x0,y0,x1,y1),radius=31,fill=(30,20,47,225))
      painter.rounded_rectangle((x0+5,y0+5,x1-5,y1-5),
        radius=28,outline=(122,88,146,95),width=3)
      actual=Image.alpha_composite(actual,mask)
     # Full painted premium ornaments were severely SQUASHED to the
     # world-nameplate (180x44) and real build button (270x42).
     # Adapt the source aspect WITHOUT shrinking the central gold gem.
     if name=="world_rune":
      actual=artist_aspect_recompose(actual,1036)
      origin="painted_original_aspect_safe_world_plaque_5_regions"
     elif name=="action_arcane":
      actual=artist_aspect_recompose(actual,1665)
      origin="painted_original_aspect_safe_build_cta_5_regions"
     else:
      origin="original_illustration_png_opaque_art_bounds_normalized"
     actual.save(dest,optimize=True,compress_level=9)
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
