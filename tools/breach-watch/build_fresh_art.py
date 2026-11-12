#!/usr/bin/env python3
"""Build independent 2K AI-painted Unity sprites, never crops from gameplay screenshot."""
import pathlib,json,hashlib,io
import numpy as np
from PIL import Image,ImageOps,ImageDraw
BASE=pathlib.Path(__file__).parent
SRC=BASE/'fresh-art-inbox'
OUT=pathlib.Path('Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt/FreshArt')
OUT.mkdir(parents=True,exist_ok=True)
manifest=json.loads((SRC/'manifest.json').read_text())
for name in ('panels','medallions','resources'):
    blob=(SRC/(name+'.png')).read_bytes()
    assert hashlib.sha256(blob).hexdigest()==manifest[name]['sha256'],'source SHA changed'
    assert Image.open(io.BytesIO(blob)).size==(2400,1792),name
# Coords from four/ five/ six disconnected original high-res elements.
boxes={
 'panels':{
   'building_panel':(120,58,1080,1292),
   'quest_panel':(1184,470,2352,814),
   'profile_panel':(39,1384,1177,1696),
   'upgrade_button':(1286,1394,2298,1687),
 },
 'medallions':{
   'castle':(48,98,827,900),
   'provisions':(810,98,1588,900),
   'swords':(1572,98,2352,900),
   'raven':(362,884,1128,1648),
   'helm':(1243,884,2009,1648),
 },
 'resources':{
   'resource_chip':(826,354,1574,622),
   'wood':(69,1039,776,1606),
   'stone':(857,1031,1542,1584),
   'food':(1673,1030,2306,1604),
 },
}
assert sum(map(len,boxes.values()))==13
def alpha_key(im):
    rgb=np.asarray(im.convert('RGB'),dtype=np.float32)
    red,green,blue=rgb[:,:,0],rgb[:,:,1],rgb[:,:,2]
    chroma=np.minimum(red,blue)-green
    # Generated magenta is near (253,2,253), not a hard-coded perfect fill.
    # Soften chroma-key boundary: unmultiply from magenta for clean anti-alias.
    a=np.clip((205.0-chroma)/150.0,0.0,1.0)
    full=(red>185)&(blue>162)&(green<130)&(chroma>91)
    a[full] = np.clip((225.0-chroma[full])/135.0,0.0,1.0)
    # Discard low-alpha magenta fringing at the perimeter.
    a=np.where(a<.08,0.0,a)
    safe=np.maximum(a,.09)
    fg=(rgb-(1-a[...,None])*np.array([254,1,254],dtype=np.float32))/safe[...,None]
    fg=np.clip(fg,0,255).astype('uint8')
    rgba=np.concatenate((fg,np.round(a[...,None]*255).astype('uint8')),2)
    return Image.fromarray(rgba,'RGBA')
def guid(seed):
    return hashlib.sha256(('eldoria-fresh-art-'+seed).encode()).hexdigest()[:32]
def meta_for(name,image):
    w,h=image.size
    if name=='building_panel': border=(round(w*.18),round(h*.08),round(w*.18),round(h*.08))
    elif name in ('quest_panel','profile_panel','resource_chip','upgrade_button'):
        border=(round(w*.12),round(h*.18),round(w*.12),round(h*.18))
    else:border=(0,0,0,0)
    x,y,z,t=border
    return f'''fileFormatVersion: 2
guid: {guid(name)}
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
  textureType: 8
  spriteMode: 1
  spriteMeshType: 0
  spritePixelsToUnits: 100
  spriteAlignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spriteBorder: {{x: {x}, y: {y}, z: {z}, w: {t}}}
'''
(OUT.parent/'FreshArt.meta').write_text(f'fileFormatVersion: 2\nguid: {guid("folder")}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
cards=[];result={}
for sheet,parts in boxes.items():
    im=Image.open(SRC/(sheet+'.png')).convert('RGB')
    for name,rectangle in parts.items():
        cut=alpha_key(im.crop(rectangle))
        cut.save(OUT/(name+'.png'),optimize=True)
        (OUT/(name+'.png.meta')).write_text(meta_for(name,cut))
        pixels=np.asarray(cut)
        opaque=float((pixels[:,:,3]>220).mean())
        transparent=float((pixels[:,:,3]<15).mean())
        assert opaque>.08 and transparent>.04,(name,opaque,transparent)
        result[name]={'origin':sheet,'rect':rectangle,'dimensions':list(cut.size),
                      'sha256':hashlib.sha256((OUT/(name+'.png')).read_bytes()).hexdigest(),
                      'opaque_fraction':round(opaque,3),'transparent_fraction':round(transparent,3)}
        tile=Image.new('RGBA',(420,340),(36,35,38,255))
        scaled=ImageOps.contain(cut,(396,297))
        tile.alpha_composite(scaled,((420-scaled.width)//2,30+(300-scaled.height)//2))
        ImageDraw.Draw(tile).text((12,10),name,fill=(243,227,193,255))
        cards.append(tile.convert('RGB'))
# Visual proof: three rows, five columns, each asset is independent, alpha composited over neutral background.
board=Image.new('RGB',(420*5,340*3),(27,27,30))
for i,card in enumerate(cards):
    board.paste(card,((i%5)*420,(i//5)*340))
board.save(SRC/'fresh-sprite-sheet-preview.jpg',quality=91)
(OUT/'manifest.json').write_text(json.dumps({'source':'fresh 2K generated atlas, NOT any original screenshot crop','files':result,'models':{k:manifest[k]['task_id'] for k in manifest}},indent=2,ensure_ascii=False))
print('VIGILIA_FRESH_NATIVE_ART_13_NEW_SPRITES',len(result),'no old art reused')
