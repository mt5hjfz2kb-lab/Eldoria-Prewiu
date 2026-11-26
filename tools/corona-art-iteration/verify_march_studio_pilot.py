"""Fail-closed review of real studio March native evidence. No paid calls."""
import hashlib, io, json, math, statistics, sys, zipfile
from decimal import Decimal
from pathlib import Path
from PIL import Image, ImageChops, ImageDraw
SOURCE="0a31a5f5f00a7e4d3ec583d07c7c78adaeb6e643"
ASSET="a42b20c2ca7bd57a8b09d95a7442ccc81bb33c9f23f2e1869112b10e4071cab7"
ROLES=("t1","mixed","landscape")
WEIGHTS=dict(resources=20,integration=20,language=15,composition=15,light_material=10,mobile=10,finish=10)
def need(ok,msg):
    if not ok:raise ValueError(msg)
def verify(archive,out):
    out.mkdir(parents=True,exist_ok=True)
    with zipfile.ZipFile(archive) as z:
        r=json.loads(z.read("evidence.json"))
        need(r["sourceSha"]==SOURCE and r["engineVersion"]=="6000.3.23f1","Wrong source or engine")
        need(r["actualPrefab"] and r["actualGateway"] and r["assetSha"]==ASSET,"Invalid actual scene or material")
        need(not r["artApproved"] and not r["physicalIphone"] and not r["fullBrowserJourney"],"Unexpected claims")
        frames={(f["fixture"],f["variant"]):f for f in r["frames"]}
        need(len(r["frames"])==6 and set(frames)=={(f,v) for f in ROLES for v in ("before","after")},"Expected six frames")
        images={}
        for k,f in frames.items():
            b=z.read(f["file"]);need(hashlib.sha256(b).hexdigest()==f["sha256"] and f["actualCamera"] and f["playMode"],"Wrong frame proof")
            im=Image.open(io.BytesIO(b)).convert("RGB")
            need(im.size==(f["width"],f["height"])==((844,390) if k[0]=="landscape" else (390,844)),"Wrong viewport")
            images[k]=im
        result={"source":SOURCE,"asset_sha":ASSET,"fixture_pairs":{},"control_defects":[],"native_defects":r.get("defects",[]),"checks":r["checks"]}
        for role in ROLES:
            a,b=frames[role,"before"],frames[role,"after"]
            need(all(a[k]==b[k] for k in ("t1","t2","selectedHero","revision")),"Fixture mismatch "+role)
            l,q=images[role,"before"],images[role,"after"]
            w,h=l.size;pair=Image.new("RGB",(2*w+12,h+28),"#172028");pair.paste(l,(0,28));pair.paste(q,(w+12,28))
            d=ImageDraw.Draw(pair);d.text((6,6),"BEFORE / "+role,fill="white");d.text((w+18,6),"STUDIO / "+role,fill="white")
            pair.save(out/(role+"-ab.png"));diff=ImageChops.difference(l,q)
            result["fixture_pairs"][role]={"before_sha":a["sha256"],"after_sha":b["sha256"],"identical_pixels":diff.getbbox() is None,"changed_pixel_fraction":round(diff.convert("L").point(lambda x:255 if x>3 else 0).histogram()[255]/(w*h),5)}
        for role in ("t1","mixed"):
            after=role+"-after.png";controls=[c for c in r["controls"] if c["frame"]==after]
            names={c["name"] for c in controls}
            need({"Hero slot 0","Hero slot 1","Hero slot 2","Confirm military command","Increase quantity","Reduce quantity","Edit march by percent"}<=names,"Missing live controls "+role)
            locked=next(c for c in controls if c["name"]=="Hero slot 2");need(not locked["interactable"],"Third slot unlocked")
            for c in controls:
                if c["interactable"] and (min(c["width"],c["height"])<43.5 or not c["hit"]):result["control_defects"].append(c)
        result["ready_for_art_review"]=True
        result["technical_gate_passed"]=not(result["control_defects"] or result["native_defects"])
        (out/"verified-native.json").write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding="utf-8")
        return result
def weighted(s):
    need(set(s)==set(WEIGHTS) and all(type(v) in (int,float) and math.isfinite(v) and 0<=v<=10 for v in s.values()),"Invalid original rubric scores")
    return float(sum(Decimal(str(s[k]))*w for k,w in WEIGHTS.items())/100)
def score(native,review):
    need(native["ready_for_art_review"] and native["source"]==review["source"]==SOURCE,"Missing actual evidence")
    need(review["reviewer"] in ("assistant-visual","independent-agent","human-art-director") and review["motion_evaluated"] is False,"Invalid review")
    need(set(review["fixtures"])=={"t1","mixed"},"Both portrait fixtures required")
    rows={}
    for role,r in review["fixtures"].items():
        p=native["fixture_pairs"][role]
        need(r["before_sha"]==p["before_sha"] and r["after_sha"]==p["after_sha"],"Review not bound to actual pixels")
        need(r["visible_changes"] and r["remaining_defects"],"Missing visual rationale")
        a,b=weighted(r["before"]),weighted(r["after"])
        ref=None if r["reference"] is None else weighted(r["reference"])
        need(r["reference_kind"] in ("direct","analog","none") and ((ref is None)==(r["reference_kind"]=="none")),"Invalid reference")
        rows[role]={"before":round(a,2),"after":round(b,2),"delta":round(b-a,2),"reference":None if ref is None else round(ref,2),"target_met":b>=8 and b-a>=1,"visible_changes":r["visible_changes"],"remaining_defects":r["remaining_defects"]}
    met=all(x["target_met"] for x in rows.values());landscape=native["fixture_pairs"]["landscape"]["identical_pixels"]
    return {"source":SOURCE,"reviewer":review["reviewer"],"rubric":WEIGHTS,"fixtures":rows,"mean_gain":round(statistics.mean(x["delta"] for x in rows.values()),2),"artistic_gate_met":met,"technical_gate_met":native["technical_gate_passed"],"landscape_identical_pixels":landscape,"eligible_for_owner_review":met and native["technical_gate_passed"] and landscape,"art_approved":False,"physical_iphone":False,"motion_evaluated":False,"full_browser_journey":False}
if __name__=="__main__":
    if sys.argv[1]=="verify":print(json.dumps(verify(Path(sys.argv[2]),Path(sys.argv[3])),ensure_ascii=False))
    elif sys.argv[1]=="score":
        r=score(json.loads(Path(sys.argv[2]).read_text()),json.loads(Path(sys.argv[3]).read_text()))
        Path(sys.argv[4]).write_text(json.dumps(r,ensure_ascii=False,indent=2));print(json.dumps(r,ensure_ascii=False))
    else:raise ValueError("Choose verify or score")
