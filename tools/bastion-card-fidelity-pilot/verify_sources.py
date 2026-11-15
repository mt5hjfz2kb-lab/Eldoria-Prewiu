#!/usr/bin/env python3
"""Source-art preservation check and SOURCE-ONLY material preview.
No Unity Editor, no live gameplay, no owner art approval.
"""
from __future__ import annotations
import hashlib
import json
import pathlib
import re
from PIL import Image, ImageDraw, ImageFont

ROOT = pathlib.Path(__file__).resolve().parents[2]
ART = ROOT / "Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt"
OUT = ROOT / "source-proof"
PILOT = ROOT / "Unity/Assets/Eldoria/BastionCardFidelityPilot/Editor/BastionCardFidelityCapture.cs"

FILES = [
    ("A baseline panel", "CleanRoom/panel.png", "d7556228f58cc7ae4930a684f2b108246fe9c9c4178096cade9fe8c267c8627b", (965,1630)),
    ("A baseline action", "CleanRoom/upgrade.png", "0b52838109a550b7074fdea148b6d58941653a7a6c2fefbfae866d33fb236706", (2322,494)),
    ("B earlier context", "OwnerReference/carved_context.png", "667e10d1270b30946f2c2252e05aa14519b9098a0f4e28b2bbf6b3741549bffb", (256,398)),
    ("B/C reference action", "OwnerControls/upgrade_button.png", "45812c616ff7ce1b59af48b8e877bf661ac04134933021fa47c8030f0fad1109", (246,60)),
    ("C blank owner material", "PremiumSurfaces/panel_fill.png", "dda2c871b42a1004d1cad88d048e57f70bdc7b019f53068360addb7d3600e8f6", (512,736)),
    ("C exact owner contour", "ApprovedContour/owner_gold_border.png", "a60d14e6e45a262b9ecd27bfa60a21a71440f182ea4dbc9d829739a8991701b9", (256,427)),
]

def assert_meta(file: pathlib.Path, require_sliced: bool) -> tuple[int,int,int,int]:
    txt = file.with_suffix(file.suffix+".meta").read_text(encoding="utf-8")
    assert re.search(r"textureType:\s*8\b",txt), f"Texture must import as UI Sprite: {file}"
    m = re.search(r"spriteBorder:\s*\{x:\s*(\d+),\s*y:\s*(\d+),\s*z:\s*(\d+),\s*w:\s*(\d+)\}",txt)
    assert m, f"Sprite borders missing: {file}"
    b = tuple(int(i) for i in m.groups()) # left,bottom,right,top
    if require_sliced: assert all(x>0 for x in b), f"Nine-slice corners absent: {file}"
    assert re.search(r"enableMipMap:\s*0",txt), f"Unnecessary UI mipmaps: {file}"
    return b

def nine_slice(img: Image.Image, target: tuple[int,int], border: tuple[int,int,int,int]) -> Image.Image:
    """Visualize edge-preserving 9-slice from ORIGINAL PNG, never rerender art."""
    dst=Image.new("RGBA",target,(0,0,0,0))
    sw,sh=img.size;tw,th=target
    l,b,r,t=border
    if tw < l+r or th < t+b:
        raise ValueError("Nine-slice target too narrow for original corner widths")
    xs=[0,l,sw-r,sw];xd=[0,l,tw-r,tw]
    ys=[0,t,sh-b,sh];yd=[0,t,th-b,th]
    for yi in range(3):
        for xi in range(3):
            crop=img.crop((xs[xi],ys[yi],xs[xi+1],ys[yi+1]))
            w=xd[xi+1]-xd[xi];h=yd[yi+1]-yd[yi]
            if w<=0 or h<=0: continue
            if crop.size!=(w,h):crop=crop.resize((w,h),Image.Resampling.BICUBIC)
            dst.alpha_composite(crop,(xd[xi],yd[yi]))
    return dst

def create_preview(images:dict[str,Image.Image], borders:dict[str,tuple[int,int,int,int]]) -> str:
    """Actual isolated SOURCE sprites only: deliberately no portrait, costs or action text."""
    cw,ch=252,394
    canvas=Image.new("RGB",(1080,575),(22,23,26))
    draw=ImageDraw.Draw(canvas)
    captions=[
        ("A: CURRENT ART", "CleanRoom full panel + button"),
        ("B: EARLIER CONTEXT", "Earlier panel source; not same ref"),
        ("C: EXACT GOLD FRAME", "Blank material + approved border"),
    ]
    xvals=[55,415,775]
    for i,x in enumerate(xvals):
        draw.text((x,18),captions[i][0],fill=(234,207,157))
        draw.text((x,40),captions[i][1],fill=(182,187,191))
        if i==0:
            panel=images["CleanRoom/panel.png"].resize((cw,ch),Image.Resampling.LANCZOS)
            button=images["CleanRoom/upgrade.png"]
        elif i==1:
            panel=nine_slice(images["OwnerReference/carved_context.png"],(cw,ch),
                             borders["OwnerReference/carved_context.png"])
            button=images["OwnerControls/upgrade_button.png"]
        else:
            panel=images["PremiumSurfaces/panel_fill.png"].resize((cw,ch),Image.Resampling.LANCZOS)
            layer=nine_slice(images["ApprovedContour/owner_gold_border.png"],(cw,ch),
                             borders["ApprovedContour/owner_gold_border.png"])
            panel.alpha_composite(layer,(0,0))
            button=images["OwnerControls/upgrade_button.png"]
        canvas.paste(panel,(x,74),panel)
        bh=52;bw=220
        paint=button.resize((bw,bh),Image.Resampling.LANCZOS)
        canvas.paste(paint,(x+16,485),paint)
    draw.text((55,548),"SOURCE SPRITES ONLY - NOT UNITY, NOT GAMEPLAY, NOT ART APPROVAL",fill=(224,170,143))
    OUT.mkdir(parents=True,exist_ok=True)
    fn=OUT/"source-components-only.png"
    canvas.save(fn,optimize=True)
    return str(fn.relative_to(ROOT))

def main():
    entries=[]
    images={}
    borders={}
    for label,rel,expected,size in FILES:
        file=ART/rel
        assert file.is_file(),f"Actual existing UI asset missing: {rel}"
        actual=hashlib.sha256(file.read_bytes()).hexdigest()
        assert actual==expected,f"Original source modified: {rel}: {actual}"
        img=Image.open(file).convert("RGBA")
        assert img.size==size,(rel,img.size,size)
        b=assert_meta(file,rel in ("OwnerReference/carved_context.png","ApprovedContour/owner_gold_border.png"))
        images[rel]=img;borders[rel]=b
        entries.append({"name":label,"asset":rel,"sha256":actual,"size":size,"border":list(b)})
    gold=images["ApprovedContour/owner_gold_border.png"]
    assert gold.getpixel((128,213))[3]==0,"Gold contour must not cover genuine 3D or text"
    assert gold.getpixel((17,150))[3]>80,"Owner border missing metalwork"
    code=PILOT.read_text(encoding="utf8")
    # Static assertions are useful for regression protection but NOT a Unity compile.
    for invariant in (
        "Need(host.City.Probe","ownerGoldObject","C-exact-approved-contour",
        "B-earlier-context","A-current","shots.Count == 12",
        "initialSnapshot ==","ui.HasLiveBuildingPortrait",
        "ui.LegacyPresentationHidden","AssetDatabase.GetAssetPath",
        "Object.DestroyImmediate(ownerGoldObject)",
        "shaderErrorMagentaFraction", "magentaFraction < .006f",
    ):
        assert invariant in code, f"Source-only pilot missing required invariant: {invariant}"
    preview=create_preview(images,borders)
    payload={
        "status":"SOURCE_ASSET_INTEGRITY_PASS_NOT_UNITY",
        "reference_note":"C exact gold contour derives from approved 864x1536 art; B carved_context belongs to a different approved-era source.",
        "native_build_compilation":"NOT_EXECUTED",
        "native_iphone":"NOT_EXECUTED",
        "art_approved":False,
        "owner_save_modified":False,
        "variants":["A-current","B-earlier-context","C-exact-approved-contour"],
        "expected_unity_screenshots":12,
        "source_preview":preview,
        "assets":entries,
    }
    (OUT/"source-proof.json").write_text(json.dumps(payload,indent=2,ensure_ascii=False),encoding="utf8")
    print("BASTION_CARD_SOURCE_PRESERVATION_PASS_NOT_UNITY",preview)

if __name__=="__main__":
    main()
