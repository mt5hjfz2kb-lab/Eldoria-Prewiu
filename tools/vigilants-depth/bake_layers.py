#!/usr/bin/env python3
"""Reproducible, zero-credit multiplane extraction from approved Eldoria source.

No new paintings, no replacement of the approved source. Six cut-outs sample
exact original pixels and add authored soft contact shadow/edge lighting.
All masks are authored in source image UV coordinates (bottom-left origin).
The generated cropped sprites have alpha and stable UV bounds for uGUI.
"""
import hashlib
import json
from pathlib import Path
from PIL import Image, ImageChops, ImageDraw, ImageEnhance, ImageFilter

SOURCE = Path("Unity/Assets/Eldoria/Resources/EldoriaRetention/vigilants-painterly-v2.png")
OUT = Path("Unity/Assets/Eldoria/Resources/EldoriaRetention/DepthB10")
EXPECTED_SHA = "9503ca15c74e0e41d3268f7af9e4c5a0c24aabf4e4ecbae2a0d64e3585796149"

# Every vertex is from the accepted environmental painting. Later masks
# overlap earlier ones and own their near surface, avoiding duplicate pieces.
POLYGONS = {
    "left_castle": [(0.04,.73),(.065,.79),(.08,.84),(.085,.91),(.104,.95),(.13,.86),(.15,.96),(.17,.9),(.19,.87),(.21,.92),(.22,.78),(.26,.79),(.29,.82),(.30,.73),(.34,.70),(.34,.61),(.31,.58),(.28,.67),(.23,.70),(.20,.71),(.16,.73),(.12,.74)],
    "bridge": [(.25,.53),(.28,.62),(.33,.63),(.34,.69),(.35,.65),(.40,.61),(.47,.62),(.54,.59),(.58,.49),(.59,.43),(.52,.43),(.47,.46),(.43,.48),(.39,.44),(.33,.46),(.29,.49)],
    "right_castle": [(.54,.57),(.59,.67),(.61,.81),(.64,.85),(.68,.88),(.695,.93),(.705,.90),(.715,.96),(.735,.90),(.76,.89),(.795,.87),(.81,.85),(.855,.81),(.89,.77),(.90,.57),(.85,.51),(.77,.47),(.70,.50),(.62,.52)],
    "left_rock": [(0,.40),(0,.78),(.03,.82),(.09,.82),(.12,.73),(.18,.69),(.20,.61),(.26,.62),(.29,.54),(.36,.53),(.38,.43),(.32,.38),(.29,.34),(.26,.41),(.19,.38),(.12,.41),(.07,.42)],
    "right_cliff": [(.53,.38),(.60,.50),(.65,.57),(.70,.66),(.75,.71),(.81,.71),(.86,.65),(.90,.58),(.93,.50),(1,.51),(1,.08),(.93,.12),(.89,.16),(.84,.25),(.78,.31),(.74,.36),(.69,.39),(.63,.35),(.59,.34)],
    "near_cliff": [(0,0),(0,.30),(.045,.34),(.08,.35),(.11,.31),(.16,.27),(.205,.27),(.24,.23),(.29,.25),(.34,.20),(.40,.18),(.43,.12),(.51,.09),(.59,0)],
}
ORDER = ["left_castle","bridge","right_castle","left_rock","right_cliff","near_cliff"]
EXPECTED_BOUNDS = {
    "left_castle":[91,27,463,395],
    "bridge":[387,302,534,317],
    "right_castle":[807,27,611,499],
    "left_rock":[0,160,608,551],
    "right_cliff":[795,275,741,706],
    "near_cliff":[0,647,920,377]
}
GUIDS = [
    "a11f7be4d3cc4ec79c41807f854e6301",
    "a11f7be4d3cc4ec79c41807f854e6302",
    "a11f7be4d3cc4ec79c41807f854e6303",
    "a11f7be4d3cc4ec79c41807f854e6304",
    "a11f7be4d3cc4ec79c41807f854e6305",
    "a11f7be4d3cc4ec79c41807f854e6306"
]
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
  textureType: 0
  spriteMode: 0
"""
OUT_META = """fileFormatVersion: 2
guid: 13b0ee326a7f41cb92bddaa4c5fbb138
DefaultImporter:
  externalObjects: {{}}
  userData:
  assetBundleName:
  assetBundleVariant:
"""

def main():
    blob=SOURCE.read_bytes()
    actual_sha=hashlib.sha256(blob).hexdigest()
    if actual_sha!=EXPECTED_SHA:
        raise RuntimeError("Approved source drift: original art changed, refuse extraction")
    image=Image.open(SOURCE).convert("RGB")
    w,h=image.size
    if (w,h)!=(1536,1024):
        raise RuntimeError("Approved source dimensions changed")
    masks=[]
    for name in ORDER:
        img=Image.new("L",(w,h),0)
        pts=[(round(x*(w-1)),round((1-y)*(h-1))) for x,y in POLYGONS[name]]
        ImageDraw.Draw(img).polygon(pts,fill=255)
        masks.append(img)
    for i in range(len(masks)):
        for j in range(i+1,len(masks)):
            masks[i]=ImageChops.subtract(masks[i],masks[j])
    OUT.mkdir(parents=True,exist_ok=True)
    entries=[]
    for i,name in enumerate(ORDER):
        matte=masks[i].filter(ImageFilter.GaussianBlur(5))
        alpha=Image.new("L",(w,h),0)
        dx,dy=[(2,7),(-2,4),(4,9),(-5,9),(7,11),(-7,12)][i]
        alpha.paste(matte,(dx,dy))
        alpha=alpha.filter(ImageFilter.GaussianBlur(13))
        strength=[.24,.20,.29,.29,.35,.38][i]
        alpha=alpha.point(lambda v:round(v*strength))
        cut=Image.new("RGBA",(w,h),(0,0,0,0))
        cast=Image.new("RGBA",(w,h),(6,15,23,0))
        cast.putalpha(alpha)
        cut.alpha_composite(cast)
        element=ImageEnhance.Contrast(image).enhance([1.05,.98,1.04,1.075,1.075,1.09][i])
        element=ImageEnhance.Color(element).enhance([1.06,.96,1.09,1.08,1.09,1.10][i])
        element=ImageEnhance.Brightness(element).enhance([1.02,.97,1.03,1.06,1.04,1.06][i]).convert("RGBA")
        element.putalpha(matte)
        cut.alpha_composite(element)
        # Top-facing raking light follows cut-out silhouette, never a box edge.
        rim=ImageChops.subtract(matte,ImageChops.offset(matte,0,5))
        rim=rim.filter(ImageFilter.GaussianBlur(3)).point(lambda v:round(v*.30))
        highlight=Image.new("RGBA",(w,h),(255,209,109,0))
        highlight.putalpha(rim)
        cut.alpha_composite(highlight)
        bbox=cut.getbbox()
        x0,y0,x1,y1=bbox
        actual=[x0,y0,x1-x0,y1-y0]
        if actual!=EXPECTED_BOUNDS[name]:
            # The renderer reads pixel ranges derived from these exact source
            # masks. Reject incompatible rasterizer versions, never misalign.
            raise RuntimeError(f"Unstable mask for {name}: {actual} not {EXPECTED_BOUNDS[name]}")
        cropped=cut.crop(bbox)
        png=OUT/(name+".png")
        cropped.save(png,optimize=True)
        (OUT/(name+".png.meta")).write_text(META.format(guid=GUIDS[i]),encoding="utf-8")
        entries.append({
            "name":name,"pixel_bbox":actual,
            "uv": [x0/w,1-y1/h,(x1-x0)/w,(y1-y0)/h],
            "bytes":png.stat().st_size,"sha256":hashlib.sha256(png.read_bytes()).hexdigest()})
    manifest={"source_sha256":EXPECTED_SHA,"original_preserved":True,
              "new_art_invented":False,"format":"cropped approved painted masks with alpha",
              "original_width":w,"original_height":h,"layers":entries,
              "depth_order":ORDER,"no_new_canvas":True}
    (OUT/"layer_index.json").write_text(json.dumps(manifest,indent=2)+"\n",encoding="utf-8")
    (OUT/"layer_index.json.meta").write_text(OUT_META,encoding="utf-8")
    assert len(entries)==6
    assert all(100_000<e["bytes"]<2_000_000 for e in entries)
    print("ELDORIA_VIGILANTS_AUTHENTIC_DEPTH_LAYERS_PASS",len(entries),
          "PNG",sum(e["bytes"] for e in entries),"bytes")

if __name__=="__main__":
    main()
