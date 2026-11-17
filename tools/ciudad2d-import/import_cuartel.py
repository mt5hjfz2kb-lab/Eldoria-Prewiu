#!/usr/bin/env python3
"""One-off, isolated authored Cuartel source import.

The Runway URL is consumed from an ephemeral branch file, never emitted in logs
or persisted in manifests. Safe host allowlist prevents fetching arbitrary URLs.
Original artwork and gameplay files remain unchanged.
"""
from __future__ import annotations
from pathlib import Path
from io import BytesIO
from urllib.parse import urlsplit
from urllib.request import Request, urlopen
from PIL import Image, ImageFilter
import numpy as np
import cv2
import hashlib
import json

ROOT = Path(__file__).resolve().parents[2]
URL_FILE = Path(__file__).resolve().parent / 'candidate_url.txt'
DEST = ROOT / 'Unity/Assets/Experimentos/Ciudad2D/Sprites/Candidates'
QA = ROOT / 'Unity/Assets/Experimentos/Ciudad2D/QA'
SAMPLE = '8acabc64-899d-4a77-889a-710e8eb9537c'

def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()

def main() -> None:
    url = URL_FILE.read_text('utf-8').strip()
    parts = urlsplit(url)
    if parts.scheme != 'https' or parts.hostname != 'dnznrvs05pmza.cloudfront.net':
        raise ValueError('Refusing untrusted sprite source host')
    if SAMPLE not in parts.path or not parts.path.endswith('.png'):
        raise ValueError('Unexpected source asset identity')
    req = Request(url, headers={'User-Agent':'EldoriaAssetPipeline/1.0'})
    with urlopen(req, timeout=45) as response:
        content_type=response.headers.get('Content-Type','')
        if 'image/' not in content_type:
            raise ValueError('Source did not return an image')
        data=response.read(15_000_001)
        if len(data)>15_000_000 or len(data)<10_000:
            raise ValueError('Unexpected image source size')
    with Image.open(BytesIO(data)) as dec:
        image=dec.convert('RGBA')
    if image.width<900 or image.height<900 or image.width>4096 or image.height>4096:
        raise ValueError('Unexpected generated source dimensions')

    DEST.mkdir(parents=True, exist_ok=True)
    raw=DEST/'Cuartel_Runway_Source.png'
    raw.write_bytes(data)

    a=np.asarray(image).copy()
    original_alpha=a[:,:,3]
    # Only attempt a transparent cutout if the generator did not already supply one.
    if np.mean(original_alpha<245)>0.22:
        foreground=(original_alpha>=245).astype(np.uint8)*255
        # Preserve anti-aliasing from alpha where present.
        mask=original_alpha.astype(np.uint8)
    else:
        rgb=a[:,:,:3]
        h,w=rgb.shape[:2]
        # First identify bright neutral matte that is connected to the canvas edge.
        lab=cv2.cvtColor(rgb,cv2.COLOR_RGB2LAB)
        background=cv2.inRange(rgb,np.array([213,213,213],np.uint8),
                                   np.array([255,255,255],np.uint8))
        # Flood fill from all four corners; this does not erase white limestone
        # inside a building because only perimeter-connected background is selected.
        flood=np.zeros((h+2,w+2),np.uint8)
        segmented=np.zeros((h,w),np.uint8)
        for x,y in [(0,0),(w-1,0),(0,h-1),(w-1,h-1)]:
            tmp=background.copy()
            cv2.floodFill(tmp,flood,(x,y),128,flags=4|(255<<8))
            segmented=np.maximum(segmented,(tmp==128).astype(np.uint8))
            flood[:]=0
        # Refine by foreground/background distance. Background only at image
        # perimeter; keep details and flags in the central area.
        mask=np.uint8(255*(1-segmented))
        # Taper only 1-2px to avoid white fringing; do not blur the entire sprite.
        mask=cv2.GaussianBlur(mask,(3,3),.6)
    a[:,:,3]=mask
    out=Image.fromarray(a,'RGBA')
    bbox=out.getchannel('A').getbbox()
    if not bbox or (bbox[2]-bbox[0])<400:
        raise ValueError('Candidate image segmentation produced empty subject')
    margin=9
    box=(max(0,bbox[0]-margin),max(0,bbox[1]-margin),
         min(out.width,bbox[2]+margin),min(out.height,bbox[3]+margin))
    out=out.crop(box)
    # Preserve full original pixels and alpha; no fake upscale or invented detail.
    target=DEST/'Cuartel_Candidato.png'
    out.save(target,optimize=True)
    for art in [raw,target]:
        srcmeta=(ROOT/'Unity/Assets/Experimentos/Ciudad2D/Sprites/Cuartel.png.meta').read_text('utf-8')
        guid=hashlib.md5(str(art.relative_to(ROOT)).encode()).hexdigest()
        import re
        srcmeta=re.sub(r'guid: [a-f0-9]{32}','guid: '+guid,srcmeta,count=1)
        art.with_suffix('.png.meta').write_text(srcmeta,encoding='utf-8')
    info={
        'schema':1,'source':'Runway generated original Cuartel isolated asset',
        'task_id':SAMPLE,'source_image':str(raw.relative_to(ROOT)),
        'source_sha256':sha(data),'candidate_image':str(target.relative_to(ROOT)),
        'candidate_sha256':sha(target.read_bytes()),
        'candidate_dimensions':list(out.size),
        'state':'NOT_INTEGRATED_ART_REVIEW_PENDING',
        'security':'signed source URL consumed only in ephemeral branch input, not persisted in manifest',
        'limitations':'Background segmentation and in-game perspective/alpha require native Unity review.',
    }
    (QA/'cuartel_candidate_manifest.json').write_text(json.dumps(info,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('CITY2D_CUARTEL_CANDIDATE_IMPORTED',info['source_sha256'],info['candidate_sha256'],info['candidate_dimensions'])

if __name__=='__main__':
    main()
