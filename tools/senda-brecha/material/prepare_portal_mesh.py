"""Turn the exact generated orthographic portal into a measured physical extrusion outline.
Keep the original RGBA artwork. No painted background, no scene overlay.
"""
import hashlib,json,sys
from pathlib import Path
import cv2,numpy as np
from PIL import Image
EXPECTED="d748cef798a69403f3b49fc48725e0c0eb1524b299b1b963f7eac5a8f57d33d9"
def sha(b):return hashlib.sha256(b).hexdigest()
def prepare(src,out):
    raw=src.read_bytes();assert sha(raw)==EXPECTED
    im=Image.open(src);assert im.mode=="RGBA" and im.size==(1024,1024)
    alpha=np.asarray(im)[:,:,3];assert alpha[550,512]==0 and alpha[0,0]==0
    mask=(alpha>=128).astype("uint8")
    n,labels,stats,_=cv2.connectedComponentsWithStats(mask)
    main=1+np.argmax(stats[1:,4]);solid=(labels==main).astype("uint8")
    assert stats[main,4]/mask.sum()>.995
    x,y,w,h,area=map(int,stats[main]);assert w==839 and h==979
    assert x>0 and y>0 and x+w<1024 and y+h<1024
    def contour(mask):
        cs,_=cv2.findContours(mask,cv2.RETR_EXTERNAL,cv2.CHAIN_APPROX_SIMPLE)
        c=max(cs,key=cv2.contourArea)
        return cv2.approxPolyDP(c,2.0,True).reshape(-1,2)
    shape=contour(solid)
    opening=np.zeros_like(solid)
    # Follow the actual clear aperture from its arch crown to the floor.
    # The center at x=512 is explicitly clear; find the nearest solid edges.
    for row in range(y,y+h):
        if solid[row,512]:continue
        a=np.flatnonzero(solid[row,:512]);b=np.flatnonzero(solid[row,512:])+512
        if len(a) and len(b) and b[0]>a[-1]+1:
            opening[row,a[-1]+1:b[0]]=1
    # Require one large continuous clear central opening; no background intrusion.
    assert opening.sum()>220000 and not np.any(opening&solid)
    assert float((alpha[opening>0]<=5).mean())>.97
    aperture=contour(opening)
    assert len(shape)>20 and len(shape)<300 and len(aperture)>10
    # Convert pixels to normalized local coordinates; origin at the floor.
    def vertices(a):return [[round(float((xx-x)/(w-1)-.5),7),round(float((y+h-1-yy)/(h-1)),7)] for xx,yy in a]
    report={"schema":1,"source_run":45718046144,"source_artifact":15070712509,"texture_sha256":EXPECTED,
            "size":[1024,1024],"bounds":[x,y,w,h],"outer":vertices(shape),"opening":vertices(aperture),
            "world_width":2.04,"world_height":3.02,"depth":.42,"opening_width_mid":float(opening[650].sum()/(w-1)*2.04),
            "alpha_extrema":[int(alpha.min()),int(alpha.max())],"opaque_pixels":int(solid.sum()),"opening_pixels":int(opening.sum()),
            "opening_clear_fraction":float((alpha[opening>0]<=5).mean()),"art_approved":False}
    out.mkdir(parents=True,exist_ok=False);(out/"senda-valoria-portal.png").write_bytes(raw)
    (out/"senda-valoria-portal.mesh.json").write_text(json.dumps(report,indent=2)+"\n")
    print("SENDA_PORTAL_CONTOUR_PASS",json.dumps({k:v for k,v in report.items() if k not in ("outer","opening")}))
if __name__=="__main__":prepare(Path(sys.argv[1]),Path(sys.argv[2]))
