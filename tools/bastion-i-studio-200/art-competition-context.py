"""Third iteration: scale the owned craft sign, calibrate roof contrast and integrate quarry ground.
Reuses existing paid assets. No image generation, no game branch writes."""
import os,json,hashlib,zipfile,shutil,math
from pathlib import Path
from decimal import Decimal as D
from PIL import Image,ImageDraw,ImageFilter,ImageChops
root=Path(os.environ["RUNNER_TEMP"]);src=root/"previous";out=root/"competition";out.mkdir(exist_ok=True)
p=json.loads((src/"parameters.json").read_text());audit=json.loads((root/"audit/summary.json").read_text())
assert audit["estimated_total_usd"]=="2.8945928"
# Include the paid no-op audit as well; it was not included in the second production ledger.
prior=D(audit["estimated_total_usd"])+D("0.0363285")
assert prior-D("1.9100013")+D(".20")<=D("2")
names=("AceraZapateros","ArmeriaDelPatio","CorteCaliza")
def sha(x):return hashlib.sha256(x.read_bytes()).hexdigest()
def dump(n,x):(out/n).write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding="utf-8")
metrics={}
for n in names:
    source=src/(n+"-final.png");assert sha(source)==p["assets"][n]["final_sha256"]
    a=Image.open(source).convert("RGBA");b=a.copy()
    if n=="AceraZapateros":
        assert a.size==(1157,1360)
        # Reuse the original boot icon and its actual wooden frame. Register to the
        # blank gable, inside the original alpha silhouette. The existing shop sign stays.
        crop=a.crop((497,943,560,1027)).convert("RGBA")
        # The board occupies this quadrilateral in the source, not the wall behind it.
        m=Image.new("L",crop.size,0)
        ImageDraw.Draw(m).polygon([(2,13),(47,3),(57,13),(56,66),(48,76),(4,82),(0,72)],fill=255)
        crop.putalpha(ImageChops.multiply(crop.getchannel("A"),m))
        crop=crop.resize((139,185),Image.Resampling.LANCZOS)
        # Location on the blank face of the west gable; no outside-sprite change.
        x,y=245,833
        overlay=Image.new("RGBA",a.size);overlay.paste(crop,(x,y))
        mask=overlay.getchannel("A")
        shadow=Image.new("RGBA",a.size,(31,24,19,0))
        shift=Image.new("L",a.size,0);shift.paste(mask,(3,5))
        shadow.putalpha(shift.filter(ImageFilter.GaussianBlur(3)).point(lambda v:round(v*.48)))
        b=Image.alpha_composite(b,shadow);b=Image.alpha_composite(b,overlay);b.putalpha(a.getchannel("A"))
        # Enforce preserved original silhouette and pixels outside the localized zone.
        region=Image.new("L",a.size,0);ImageDraw.Draw(region).rectangle((237,825,397,1030),fill=255)
        b=Image.composite(b,a,region);b.putalpha(a.getchannel("A"))
    elif n=="ArmeriaDelPatio":
        assert a.size==(509,406)
        # Keep the improved armory metalwork and open workshop, lower the
        # brightest exterior roof/plinth values so the main barracks dominates.
        mask=Image.new("L",a.size,0);d=ImageDraw.Draw(mask)
        d.polygon([(114,91),(297,52),(356,162),(484,221),(340,258),(218,190),(164,210)],fill=255)
        d.polygon([(18,321),(84,318),(91,365),(25,377)],fill=255)
        d.polygon([(152,337),(201,336),(212,391),(164,397)],fill=255)
        d.polygon([(343,354),(380,350),(396,399),(348,403)],fill=255)
        d.polygon([(450,337),(478,330),(495,372),(449,385)],fill=255)
        mask=ImageChops.multiply(mask,mask.filter(ImageFilter.GaussianBlur(5)))
        graded=a.copy();pix=graded.load()
        for y in range(a.height):
            for x in range(a.width):
                r,g,v,alpha=a.getpixel((x,y));lum=(54*r+183*g+19*v)/256
                strength=.11+.07*min(1,max(0,(lum-80)/125))
                pix[x,y]=(round(r*(1-strength)),round(g*(1-strength)),round(v*(1-strength)),alpha)
        b=Image.composite(graded,a,mask);b.putalpha(a.getchannel("A"))
    else:
        assert a.size==(640,213)
        # soften the block-like dark seams but preserve low-frequency strata.
        rgb=a.convert("RGB");low=rgb.filter(ImageFilter.GaussianBlur(2.4));p0=rgb.load();pl=low.load();q=b.load()
        for y in range(12,a.height-10):
            for x in range(8,a.width-8):
                r,g,v,alpha=a.getpixel((x,y));l=p0[x,y];t=pl[x,y]
                if alpha>128 and sum(t)-sum(l)>40:
                    # Restrict correction to strongly dark high-frequency lines.
                    q[x,y]=tuple(round(.76*z+.24*w) for z,w in zip(l,t))+(alpha,)
    dest=out/(n+"-final.png");b.save(dest)
    assert a.size==b.size and a.getchannel("A").tobytes()==b.getchannel("A").tobytes()
    aa,bb=a.tobytes(),b.tobytes();changed=sum(aa[i:i+4]!=bb[i:i+4] for i in range(0,len(aa),4));assert changed>0
    metrics[n]={"changed_pixels":changed,"total_pixels":a.width*a.height,"alpha_identical":True,"input_sha256":sha(source),"output_sha256":sha(dest)}
    p["assets"][n]["final_sha256"]=sha(dest)
    p["assets"][n]["selected"]="context-correction"
    # Earlier isolated scores describe the prior candidate, not this new edit.
    p["assets"][n]["final"]["score_scope"]="previous_isolated_candidate_only"
    # User-facing contact sheet at common scale.
    canv=Image.new("RGB",(1000,600),(52,55,53));d=ImageDraw.Draw(canv)
    for i,im in enumerate((a,b)):
        im=im.copy();im.thumbnail((480,550));canv.paste(im,(i*500+(500-im.width)//2,35+(550-im.height)//2),im.getchannel("A"));d.text((i*500+12,10),["Previous finalist","Context correction"][i],fill="white")
    canv.save(out/(n+"-context-comparison.jpg"),quality=94)
p.update(estimated_total_usd=str(prior),estimated_new_usd=str(prior-D("1.9100013")),context_correction=True,prior_audit_counted=True)
dump("parameters.json",p);dump("pixel-proof.json",metrics)
shutil.copyfile(src/"ledger.json",out/"ledger.json")
print("CONTEXT_CORRECTION_ASSETS",json.dumps(metrics),flush=True)
