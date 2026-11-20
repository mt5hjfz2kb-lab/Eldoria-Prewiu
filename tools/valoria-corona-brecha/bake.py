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

def remove_detached_artist_fragments(image, name):
 """Preserve only the original principal connected illustration and a 2-pixel
 antialias fringe. Source-proven alpha fragments on icon_wood and panel_obsidian
 were genuine disconnected output debris, NOT valid resource/gameplay objects.
 Intentionally do not apply this to icon_food: its spilled grain is artistic.
 """
 from PIL import ImageFilter
 w,h=image.size
 alpha=image.getchannel("A").tobytes()
 seen=bytearray(w*h)
 main=[]
 discarded=0
 for seed,value in enumerate(alpha):
  if value<=20 or seen[seed]: continue
  queue=[seed];seen[seed]=1;component=[]
  while queue:
   index=queue.pop()
   component.append(index)
   x=index%w; y=index//w
   for yy in range(max(0,y-1),min(h,y+2)):
    for xx in range(max(0,x-1),min(w,x+2)):
     ni=yy*w+xx
     if alpha[ni]>20 and not seen[ni]:
      seen[ni]=1;queue.append(ni)
  if len(component)>len(main):
   discarded+=len(main);main=component
  else: discarded+=len(component)
 if len(main)<.95*(len(main)+discarded):
  raise ValueError("Corona painter output is not a single object: "+name)
 mask=bytearray(w*h)
 for idx in main:mask[idx]=255
 fringe=Image.frombytes("L",(w,h),bytes(mask)).filter(ImageFilter.MaxFilter(5))
 cleaned=Image.composite(image,Image.new("RGBA",image.size),fringe)
 print("CORONA_ORIGINAL_DETACHED_PIXELS_REMOVED",name,discarded)
 return cleaned

def remove_identity_horizontal_export_line(original):
 """Source-grounded two-row artifact with EXACT constant RGBA pixels.
 Repair with the immediately neighboring original painter pixel rows,
 preserving the central gold crystal and real antialias edges.
 """
 if original.size!=(580,260): raise ValueError("Identity source dimension changed")
 image=original.copy()
 p=image.load()
 mark=(202,153,241,102)
 repaired=0
 for x in range(image.width):
  if p[x,187]==mark and p[x,188]==mark:
   upper=p[x,186];lower=p[x,189]
   p[x,187]=tuple(round((2*u+d)/3) for u,d in zip(upper,lower))
   p[x,188]=tuple(round((u+2*d)/3) for u,d in zip(upper,lower))
   repaired+=1
 if not 380<=repaired<=405:
  raise ValueError("Identity exporter guideline not known; refuse arbitrary image edit: "+str(repaired))
 print("CORONA_IDENTITY_EXACT_SOURCE_GUIDELINE_REPAIRED",repaired)
 return image

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
     painted=remove_identity_horizontal_export_line(original) if name=="identity_crest" else original
     if name in ("panel_obsidian","action_arcane"):
      painted=remove_detached_artist_fragments(painted,name)
     alpha=painted.getchannel("A").point(lambda pixel:255 if pixel>20 else 0)
     bounds=alpha.getbbox()
     if not bounds:
      raise ValueError("Original premium frame is transparent: "+name)
     left,top,right,bottom=bounds
     pad=4
     bounds=(max(0,left-pad),max(0,top-pad),
             min(original.width,right+pad),min(original.height,bottom+pad))
     actual=painted.crop(bounds).resize(original.size,Image.Resampling.LANCZOS)
     # The premium source is ALREADY dark amethyst with clean artist-painted
     # borders. Old rounded_rectangle enamel insets overprinted a SECOND
     # rectangle over each gem, obscured half the source artwork and looked
     # unlike a commercial 4X game. Preserve original pixels; text contrast
     # belongs in live Unity Text/Outline, not in flattened image overlays.
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
    if name=="icon_wood":
     with Image.open(premium) as original:
      original.load()
      if original.mode!="RGBA" or original.size!=(288,288):
       raise ValueError("Unknown source icon_wood art dimensions")
      cleaned=remove_detached_artist_fragments(original,name)
      bounds=cleaned.getchannel("A").point(lambda a:255 if a>20 else 0).getbbox()
      if not bounds: raise ValueError("Original logs lost during cleanup")
      padding=7
      x0,y0,x1,y1=bounds
      bounds=(max(0,x0-padding),max(0,y0-padding),
              min(original.width,x1+padding),min(original.height,y1+padding))
      cleaned.crop(bounds).resize(original.size,Image.Resampling.LANCZOS).save(
       dest,optimize=True,compress_level=9)
      origin="original_illustration_png_logs_detached_export_art_removed"
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
