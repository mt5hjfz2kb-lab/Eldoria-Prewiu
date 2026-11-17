#!/usr/bin/env python3
"""Normalize real Blender Color + ContactShadow to ONE shared foot pivot.
All operations deterministic. Does NOT modify original .blend/source renders.
Output is a pair of sprites for Unity (rgba, aligned, no rectangular ground).
"""
import argparse,hashlib,json,pathlib
from PIL import Image, ImageChops, ImageDraw, ImageFilter
parse=argparse.ArgumentParser()
parse.add_argument('--dir',required=True,type=pathlib.Path)
args=parse.parse_args()
root=args.dir
manifest=json.loads((root/'blender-pilot-manifest.json').read_text())
report={'schema':1,'blender_master_sha256':manifest['master_sha256'],'buildings':{},'camera':manifest['camera'],
'method':'crop identical color/shadow rect with preserved projection x-centre and bottom foot anchor; no retouch/upsampling'}
for name in ('Cuartel','Aserradero'):
    color=Image.open(root/(name+'_Color.png')).convert('RGBA')
    shadow=Image.open(root/(name+'_ContactShadow.png')).convert('RGBA')
    assert color.size==shadow.size and color.size[0]==color.size[1],name
    w,h=color.size
    ca=color.getchannel('A');sa=shadow.getchannel('A')
    cbox=ca.point(lambda v:255 if v>=20 else 0).getbbox()
    sbox=sa.point(lambda v:255 if v>=6 else 0).getbbox()
    if not cbox or not sbox:raise ValueError('Rendered silhouette/shadow missing: '+name)
    if cbox[2]-cbox[0]<140 or cbox[3]-cbox[1]<140:raise ValueError('3D model too small: '+name)
    # Alpha detected outside content must never imply opaque rectangular matte.
    assert ca.getpixel((0,0))<10 and sa.getpixel((0,0))<10,name
    left=min(cbox[0],sbox[0]); right=max(cbox[2],sbox[2])
    top=max(0,min(cbox[1],sbox[1])-18)
    bottom=min(h,max(cbox[3],sbox[3])+18)
    # Keep the calibrated camera projection origin (x=w/2) fixed,
    # rather than centering a potentially asymmetric building.
    half=max(w//2-left,right-w//2)+18
    x0=max(0,w//2-half); x1=min(w,w//2+half)
    box=(x0,top,x1,bottom)
    # The same cropped extent is used for both render passes.
    cc=color.crop(box); ss=shadow.crop(box)
    # Physically rendered shadow catcher extends over the whole floor and
    # looks like a huge dark rectangle on the painted city. Keep ONLY the
    # contact region around the actual ground footprint, with a soft falloff.
    # Ground anchor follows the color silhouette rather than image edges.
    anchor_x = w//2 - x0
    anchor_y = cbox[3] - top - max(20, (cbox[3]-cbox[1])*.065)
    radius_x = max(100, (cbox[2]-cbox[0])*.54)
    radius_y = max(52, (cbox[3]-cbox[1])*.145)
    footprint = Image.new('L', cc.size, 0)
    d = ImageDraw.Draw(footprint)
    d.ellipse((round(anchor_x-radius_x), round(anchor_y-radius_y),
               round(anchor_x+radius_x), round(anchor_y+radius_y)), fill=255)
    footprint = footprint.filter(ImageFilter.GaussianBlur(radius=max(12, round(radius_y*.22))))
    ss.putalpha(ImageChops.multiply(ss.getchannel('A'), footprint))
    if cc.size!=ss.size:raise ValueError('Color/shadow registration drift')
    for output,im in [(name+'_Ready.png',cc),(name+'_ShadowReady.png',ss)]:
        im.save(root/output,optimize=True)
    # Relevant details at natural iPhone sprite size remain: not proof of quality.
    alpha_count=sum(1 for val in cc.getchannel('A').getdata() if val>40)
    shadow_count=sum(1 for val in ss.getchannel('A').getdata() if val>3)
    if alpha_count<12000 or shadow_count<1500:raise ValueError('Not enough artwork/shadow')
    report['buildings'][name]={
      'src_color_alpha_bbox':list(cbox),'src_shadow_alpha_bbox':list(sbox),
      'cropping_rect':list(box),'output_dimensions':list(cc.size),
      'source_color_sha256':manifest['buildings'][name]['color_sha256'],
      'source_shadow_sha256':manifest['buildings'][name]['shadow_sha256'],
      'ready_sha256':hashlib.sha256((root/(name+'_Ready.png')).read_bytes()).hexdigest(),
      'shadow_ready_sha256':hashlib.sha256((root/(name+'_ShadowReady.png')).read_bytes()).hexdigest(),
      'color_alpha_gt40_count':alpha_count,'shadow_alpha_gt3_count':shadow_count,
      'render_3d_mesh_objects':manifest['buildings'][name]['mesh_objects'],
      'shadow_contact_mask':{'method':'original Cycles shadow x Gaussian softened measured footprint', 'anchor': [int(anchor_x),round(anchor_y)],'radii':[round(radius_x),round(radius_y)]},
      'approved':False
    }
(root/'unity-sprite-contract.json').write_text(json.dumps(report,indent=2)+'\n')
print('BLENDER_SPRITES_FOOT_ALIGNED_READY',json.dumps(report))
