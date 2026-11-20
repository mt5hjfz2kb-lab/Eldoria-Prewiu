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

def original_connected_art_only(artist_png):
 """Remove isolated image-generation alpha chips outside the main frame.
 Preserve the largest original 8-connected alpha component and a 2px
 antialias fringe. Specifically prevents the 548-pixel gold sliver
 floating to the left of the actual obsidian panel.
 """
 from PIL import ImageFilter
 w,h=artist_png.size
 alpha=artist_png.getchannel("A").tobytes()
 seen=bytearray(w*h)
 largest=[]
 others=0
 for seed,value in enumerate(alpha):
  if value<=20 or seen[seed]:
   continue
  pending=[seed];seen[seed]=1;connected=[]
  while pending:
   index=pending.pop()
   connected.append(index)
   x,y=index%w,index//w
   for ny in range(max(0,y-1),min(h,y+2)):
    for nx in range(max(0,x-1),min(w,x+2)):
     j=ny*w+nx
     if alpha[j]>20 and not seen[j]:
      seen[j]=1;pending.append(j)
  if len(connected)>len(largest):
   others+=len(largest)
   largest=connected
  else:
   others+=len(connected)
 if len(largest)<.5*w*h:
  raise ValueError("Corona panel image is disconnected or mostly empty")
 mask=bytearray(w*h)
 for i in largest:mask[i]=255
 feather=Image.frombytes("L",(w,h),bytes(mask)).filter(ImageFilter.MaxFilter(5))
 result=Image.composite(artist_png,Image.new("RGBA",(w,h)),feather)
 print("CORONA_PANEL_FLOATING_ISLANDS_REMOVED",others)
 return result

def commercial_wide_panel(artist_png):
 """Preserve the sculpted CENTRAL gems and solid corners when making a
 355:244 mobile modal. Unity Image.Type.Simple stretched the 500x540
 original to a 355x244 rectangle, flattening every jewel. This
 deterministic, original-pixel remap only expands the side enamel/rims.
 The center 93px contains the original crown and remains 1:1.
 Rejected naive repeated-vertical-strip method (visible seams).
 """
 # Authored input from the preserved original PNG is exact 500x540,
 # normalized by the existing opaque art bounds logic before this call.
 if artist_png.size!=(500,540):
  raise ValueError("Corona wide panel source changed: "+str(artist_png.size))
 base=artist_png.resize((463,500),Image.Resampling.LANCZOS)
 cuts=(0,103,185,278,363,463)
 widths=(103,208,93,216,100)
 assert sum(widths)==720
 result=Image.new("RGBA",(720,500))
 dest=0
 for (left,right),width in zip(zip(cuts,cuts[1:]),widths):
  strip=base.crop((left,0,right,500))
  if strip.width!=width:
   strip=strip.resize((width,500),Image.Resampling.LANCZOS)
  result.paste(strip,(dest,0))
  dest+=width
 assert dest==720 and result.getbbox() is not None
 # All original pixels are sourced from the user's own painted panel;
 # this is an artist-material aspect conversion, not a screenshot hack.
 return result

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
     if name=="panel_obsidian":
      actual=commercial_wide_panel(original_connected_art_only(actual))
      origin="original_painted_panel_aspect_mapped_720x500_central_gems_intact"
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
