#!/usr/bin/env python3
"""Extract traceable source pixels; never generate or inpaint artwork.

Blank scalable surfaces use only text-free source patches. Originals are
immutable. These derivatives require real Unity visual review.
"""
import argparse, hashlib, json, uuid
from pathlib import Path
from PIL import Image
from verify_art_references import check

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--reference-dir',type=Path,required=True)
    ap.add_argument('--out',type=Path,required=True);args=ap.parse_args()
    refs=check(args.reference_dir);args.out.mkdir(parents=True,exist_ok=True)
    images={k:Image.open(v['file']).convert('RGBA') for k,v in refs.items()}
    records=[]
    def save(name,im,screen,regions):
        path=args.out/(name+'.png');im.save(path)
        records.append({'name':name,'source_sha256':refs[screen]['sha256'],'regions_xyxy':regions,
                        'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'method':'source_pixels_only'})
        guid=uuid.uuid5(uuid.NAMESPACE_URL,'eldoria/v41/'+name).hex
        Path(str(path)+'.meta').write_text('fileFormatVersion: 2\nguid: '+guid+'\nTextureImporter:\n  serializedVersion: 13\n  mipmaps:\n    enableMipMap: 0\n  isReadable: 0\n  npotScale: 0\n  textureType: 0\n  textureShape: 1\n  maxTextureSize: 2048\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 1\n    aniso: 1\n    wrapU: 1\n    wrapV: 1\n  alphaIsTransparency: 1\n  textureCompression: 0\n')
    crops={
      'mail-battle':('mailbox',(109,407,237,513)),
      'mail-march':('mailbox',(109,564,237,693)),
      'mail-reward':('mailbox',(109,746,237,876)),
      'mail-system':('mailbox',(109,929,237,1060)),
      'mail-narrative':('mailbox',(109,1112,237,1244)),
      'archer-row':('march',(40,689,309,850)),
      'aldric-face':('march',(118,213,256,447)),
      'hero-support':('march',(365,210,609,460)),
      'hero-locked':('march',(634,210,880,460)),
      'training-archer':('training',(111,59,295,535)),
      'training-landscape':('training',(345,64,537,100)),
      'march-scenery':('march',(4,95,937,125)),
    }
    for name,(screen,box) in crops.items():save(name,images[screen].crop(box),screen,[box])
    # Nine-slice templates retain exact corners/edges; the centre is a quiet
    # region of that same panel, never its illustrative numbers or lettering.
    def panel(name,screen,box,center,border=18):
        original=images[screen].crop(box);w,h=original.size;b=border
        im=images[screen].crop(center).resize((w,h),Image.Resampling.BILINEAR)
        for region in [(0,0,w,b),(0,h-b,w,h),(0,b,b,h-b),(w-b,b,w,h-b)]:
            im.paste(original.crop(region),region[:2])
        save(name,im,screen,[box,center])
    panel('mail-card','mailbox',(63,544,1125,710),(800,580,950,620),20)
    panel('tab-active','mailbox',(337,204,594,326),(380,225,550,240),16)
    panel('tab-idle','mailbox',(337,204,594,326),(380,225,550,240),16)
    panel('blue-panel','march',(363,208,612,594),(400,525,570,570),14)
    panel('gold-button','march',(219,1544,723,1626),(249,1560,671,1570),14)
    for ref in refs.values():ref['file']=Path(ref['file']).name
    (args.out/'provenance.json').write_text(json.dumps({'references':refs,'derivatives':records,'art_approved':False},indent=2))
    print(json.dumps({'assets':len(records),'bytes':sum(p.stat().st_size for p in args.out.glob('*.png'))}))
if __name__=='__main__':main()
