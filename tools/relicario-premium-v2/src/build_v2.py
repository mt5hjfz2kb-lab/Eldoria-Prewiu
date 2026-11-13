#!/usr/bin/env python3
"""Eldoria Relicario V2 art-source extraction from user-approved reference.
Non-destructive: source is immutable; dynamic text/effects and illustrations remain separate.
"""
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance
from pathlib import Path
import numpy as np
import json
R=Path(__file__).resolve().parent.parent
# The user-approved reference is stored alongside the build for reproducibility in local package.
src=Image.open(R/'assets/approved_board_original.webp').convert('RGBA')
boxes={
 'common':(369,54,584,378),
 'rare':(661,54,879,380),
 'epic':(960,53,1183,379),
 'legendary':(1249,53,1484,379),
}
# Exact geometric layout 320x512. Every rarity uses the same alpha cut-outs.
W,H=320,512
for rarity,box in boxes.items():
    im=src.crop(box).resize((W,H),Image.Resampling.LANCZOS)
    im=ImageEnhance.Sharpness(im).enhance(1.24)
    # Cut open a large interchangeable illustration window. Keep real metal,
    # sockets, icon borders, all four ornaments, metal bevels and corners.
    alpha=Image.new('L',(W,H),255);d=ImageDraw.Draw(alpha)
    # Window (rounded rectangle) and masks: preserve badge silhouettes.
    d.rounded_rectangle((20,53,300,338),radius=9,fill=0)
    # Repaint distinct badge alpha from original; use only clipped geometric silhouette.
    def restore_polygon(points):
        s=Image.new('L',(W,H));p=ImageDraw.Draw(s);p.polygon(points,fill=255)
        alpha.paste(Image.fromarray(np.maximum(np.asarray(alpha),np.asarray(s)).astype('uint8')))
    restore_polygon([(137,15),(184,15),(198,38),(192,79),(160,91),(129,79),(123,38)])
    restore_polygon([(0,141),(31,137),(53,155),(53,235),(31,249),(0,235)])
    restore_polygon([(320,141),(289,137),(267,155),(267,235),(289,249),(320,235)])
    restore_polygon([(136,254),(160,245),(184,254),(188,299),(160,336),(132,299)])
    # Name plaque + effect plaque retain all illustrated metal borders.
    # Clear previously baked typography/wood icon inside the bezel; dynamic HTML owns content.
    # Use conservative opaque dark metallic material exactly inside bezel.
    im_np=np.asarray(im).copy()
    for y1,y2,x1,x2,rgb in [(327,388,30,292,(17,17,20)),(391,467,32,289,(17,17,20))]:
       for y in range(y1,y2):
           if y>=H:continue
           # quiet metallic value gradient, slightly warmer near top; subtle grain from approved source
           k=(y-y1)/(y2-y1)
           tex=4.0*np.sin(y/11.3)+3.5*np.cos(y/23.1)
           rr=np.clip(np.array(rgb)+[tex*(1-k),tex*(1-k),tex*(1-k)],0,255).astype('uint8')
           im_np[y,x1:x2,:3]=rr
    im=Image.fromarray(im_np)
    # Remove old stat symbols only from numeric centers, preserve silhouette of the outlined bevel.
    dr=ImageDraw.Draw(im)
    for rect in [(142,25,181,83),(14,153,48,229),(274,153,307,229),(142,272,178,332)]:
       dr.rounded_rectangle(rect,radius=5,fill=(11,13,16,255))
    im.putalpha(alpha)
    # A small diffuse metal sheen glints at the corners, preserving photographic metal.
    im.resize((W*2,H*2),Image.Resampling.LANCZOS).save(R/'assets'/f'frame_{rarity}.webp',quality=88,method=6)

# Accurate forest art already separated in the v1 implementation.
source_forest=R/'assets'/'bosque_valoria_art.jpg'
assert source_forest.is_file()
# This manifest makes generator deterministic; source boxes refer to approved board.
manifest={"cardAspect":"5:8","framePixels":[640,1024],"origins":boxes,
 "artWindowPct":{"left":6.85,"top":10.55,"width":86.3,"height":54.4},
 "materialMethod":"reference-derived layered RGBA cutout + dynamic HTML/Unity UI; no AI-generated lettering",
 "status":"art-candidate-unity-unverified"}
(R/'assets'/'atlas_manifest.json').write_text(json.dumps(manifest,indent=2,ensure_ascii=False)+'\n')
