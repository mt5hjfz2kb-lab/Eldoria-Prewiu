#!/usr/bin/env python3
"""Real Unity screenshot audit for PR181. Automatic prefilter, NOT artistic approval.

Reads original PNGs and report.json from the same Unity commit; rejects missing/
blank/identical reward stages, corrupted images, altered bytes, absent milestone
art or false source labels. Produces a compact contact sheet for HUMAN comparison
with the owner-approved visual reference, and machine-readable evidence.

Requires Pillow>=11 (only in Ubuntu evidence-analysis job, not game runtime).
"""
import argparse
import hashlib
import json
import math
from pathlib import Path
import re
from PIL import Image, ImageDraw, ImageFont, ImageStat, ImageChops

REQUIRED_SIZES = [(1600, 900), (900, 1600), (844, 390), (390, 844)]
DAILY = (
    "calendar", "closed", "opening", "opened",
    "special-day4-calendar", "special-day4-opening", "special-day4-received",
)
MILESTONE = (
    "style-lyra-opening", "style-lyra-result",
    "style-bastion-opening", "style-bastion-result",
    "style-vigilants-opening", "style-vigilants-result",
    "style-scouts-opening", "style-scouts-result",
)

def assert_quality(condition, message):
    if not condition:
        raise ValueError("VISUAL_QUALITY_GATE: " + message)

def sample(image):
    # Normalized image samples make thresholds resolution-independent and
    # avoid using an external OCR engine or changing the real PNG file.
    rgb = image.convert("RGB").resize((196, 126), Image.Resampling.BOX)
    stat = ImageStat.Stat(rgb)
    means = stat.mean
    std = stat.stddev
    luminance = (means[0] * .2126 + means[1] * .7152 + means[2] * .0722)
    warm = sum(
        1 for r,g,b in rgb.getdata()
        if r > 65 and r > g*1.09 and g > b*1.05
    )
    colorbins = len({
        (r//24,g//24,b//24) for r,g,b in rgb.getdata()
    })
    # The owner explicitly rejected a neon/fairy-purple scene as off-style.
    # Log strongly saturated pink-violet coverage for *human* art direction:
    # a Guardian's actual violet Breach can be legitimate, so NEVER turn
    # this subjective heuristic into an automatic art APPROVED/REJECTED flag.
    neon_violet = sum(1 for r,g,b in rgb.getdata()
        if r>130 and b>150 and g<min(r,b)*.60 and r+b-2*g>135)
    neon_violet_ratio=round(neon_violet/(196*126),4)
    return {
        "luminance":round(luminance, 2),
        "meanStdDev":round(sum(std)/3, 2),
        "warmPixels":warm,
        "uniqueColorBins":colorbins,
        "neonVioletPixels":neon_violet,
        "neonVioletRatioForHumanReview":neon_violet_ratio,
        "thumb":rgb,
    }

def compare(a,b):
    # A change in gold glow, chest lid AND prize-card composition should
    # move more pixels than simple lossless PNG encoder noise.
    aa=a.convert("RGB").resize((196,126), Image.Resampling.BOX)
    bb=b.convert("RGB").resize((196,126), Image.Resampling.BOX)
    delta=ImageChops.difference(aa,bb)
    magnitude=sum(ImageStat.Stat(delta).mean)/3
    shifted=sum(1 for r,g,bl in delta.getdata()
                if (r+g+bl) > 48)
    return {"avgRgbDifference":round(magnitude,2),
            "changedPixels":shifted}

def scenery_window(image, width, height):
    """Source-art aperture, away from chest, headline and buttons.

    Portrait: the authentic painting occupies the upper third of the stage.
    Landscape: the authentic painting is the left-hand storytelling theatre.
    This is a PIXEL diagnostic, never owner art approval or a city test.
    """
    if (width, height) == (844, 390):
        bounds = (.105, .20, .42, .73)
    elif (width, height) == (390, 844):
        bounds = (.23, .185, .77, .36)
    else:
        raise ValueError("Scenic aperture unsupported: %sx%s" % (width,height))
    x0,y0,x1,y1 = bounds
    return image.crop((round(x0*width),round(y0*height),
                       round(x1*width),round(y1*height)))

def read_report(folder, expected_sha):
    report_file=folder/"report.json"
    assert_quality(report_file.is_file(),"Unity did not produce report.json")
    r=json.loads(report_file.read_text(encoding="utf-8-sig"))
    assert_quality(r.get("status")==
        "NATIVE_COMPONENT_RENDERED_WITH_AUTHORED_SPRITES_ART_REVIEW_PENDING",
        "Unity failed to render verified illustration; procedural backup is NOT art approval")
    assert_quality(r.get("images")==52 and r.get("realGatewayClaims")==8
         and r.get("graphicalPointerHits")==56
         and r.get("visualOnlyMilestoneCaptures")==16,
         "Incomplete real claims, native interactions or style preview coverage")
    assert_quality(r.get("authoredSpritesImported") is True and
        r.get("artApproved") is False and r.get("fullSceneVerified") is False,
        "Source component must not claim final artistic/full-game certification")
    assert_quality(r.get("source")==expected_sha,
        "Captured source commit does not match the head SHA of the PR build")
    return r

def analyze(folder,expected_sha,output):
    folder=folder.resolve()
    output=output.resolve()
    output.mkdir(parents=True,exist_ok=True)
    r=read_report(folder,expected_sha)
    shots=r.get("shots") or []
    assert_quality(len(shots)==52,"Not exactly 52 original screenshot records")
    expected=[]
    for w,h in REQUIRED_SIZES:
        expected.append(("hud-entry",w,h))
        expected.extend((variant,w,h) for variant in DAILY)
        if (w,h) in {(844,390),(390,844)}:
            expected.extend((variant,w,h) for variant in
                ("day7-prize-flight","special-day4-prize-flight"))
            expected.extend((variant,w,h) for variant in MILESTONE)
    assert_quality(len(expected)==52,"Internal evidence manifest arithmetic changed")
    wanted=set(expected)
    actual={}
    details=[]
    visual_language_warnings=[]
    for i,record in enumerate(shots):
        filename=record.get("file","")
        pattern=re.fullmatch(
            r"real-unity-daily-(.+)-([0-9]+)x([0-9]+)\.png",filename)
        assert_quality(pattern is not None,
            "Suspicious original Unity PNG file name: "+filename)
        variant=pattern.group(1)
        w,h=int(pattern.group(2)),int(pattern.group(3))
        key=(variant,w,h)
        assert_quality(key in wanted, "Unexpected screenshot "+filename)
        assert_quality(key not in actual, "Duplicated screenshot "+filename)
        assert_quality((record.get("width"),record.get("height"))==(w,h),
            "Manifest pixel dimensions mismatched "+filename)
        assert_quality(record.get("nativeCanvas") is True,
            "Non-native screenshot declared as Unity "+filename)
        file=folder/filename
        assert_quality(file.is_file(), "Missing screenshot "+filename)
        data=file.read_bytes()
        assert_quality(len(data)>7000 and len(data)==record.get("bytes"),
            "Empty/partial screenshot "+filename)
        assert_quality(hashlib.sha256(data).hexdigest()==record.get("sha256"),
            "Screenshot was modified after the original Unity render: "+filename)
        assert_quality(data.startswith(b"\x89PNG\r\n\x1a\n"),
            "Invalid PNG container "+filename)
        with Image.open(file) as im:
            assert_quality(im.format=="PNG" and im.size==(w,h),
                "Actual screenshot resolution mismatch "+filename)
            assert_quality(im.mode in ("RGB","RGBA"),
                "Unity did not export an RGB/RGBA screenshot "+filename)
            thumb=im.convert("RGB").copy()
        metrics=sample(thumb)
        is_entry=variant=="hud-entry"
        assert_quality(metrics["meanStdDev"]>(2.0 if is_entry else 8.0) and
            metrics["uniqueColorBins"]>=(8 if is_entry else 25),
            "Blank or flat native canvas; original painted artwork not rendered: "+filename)
        assert_quality(metrics["warmPixels"]>(1 if is_entry else 4),
            "Missing gilded accents, chest medallion or warm light: "+filename)
        if is_entry:
            assert_quality(record.get("preserved") is False and
                record.get("coversReward") is False,
                "A claimable HUD signal is not an already-paid reward: "+filename)
        elif variant.startswith("style-"):
            assert_quality(record.get("preserved") is False and
                record.get("coversReward") is False,
                "Visual-only style probe may never masquerade as paid loot: "+filename)
        else:
            assert_quality(record.get("preserved") is True and
                record.get("coversReward") is True,
                "Only a genuine reward gateway state may be labelled paid: "+filename)
        actual[key]=(thumb,metrics)
        if metrics["neonVioletRatioForHumanReview"]>.16 and (
            "vigilants" not in variant):
            visual_language_warnings.append({
                "file":filename,
                "ratio":metrics["neonVioletRatioForHumanReview"],
                "note":"Human check: unusually saturated purple area; owner rejected fairy/neon identity."
            })
        details.append({
            "file":filename,"view":record.get("view"),"size":[w,h],
            "luminance":metrics["luminance"],
            "stddev":metrics["meanStdDev"],"warmPixels":metrics["warmPixels"],
            "colorBins":metrics["uniqueColorBins"],
            "neonVioletRatioForHumanReview":
                metrics["neonVioletRatioForHumanReview"],
        })
    assert_quality(set(actual)==wanted,"Missing variant or device size")

    transitions=[]
    for w,h in REQUIRED_SIZES:
        for a,b in [("closed","opening"),("closed","opened"),
                    ("special-day4-opening","special-day4-received")]:
            diff=compare(actual[(a,w,h)][0],actual[(b,w,h)][0])
            # Deliberately low threshold: catches identical outputs while
            # respecting a small chest on a wide, world-oriented phone.
            assert_quality(diff["changedPixels"]>50 and
                diff["avgRgbDifference"]>.30,
                "Chest animation or reward result never changed the actual pixels: "+
                f"{a} -> {b} on {w}x{h}")
            transitions.append({"resolution":f"{w}x{h}",
                                "from":a,"to":b,**diff})
    for w,h in [(844,390),(390,844)]:
        # Flight frames must show resources truly EMERGING from the opened
        # illustrated chest, not the same final cards already standing still.
        for flight,settled in (
            ("day7-prize-flight","opened"),
            ("special-day4-prize-flight","special-day4-received"),
        ):
            diff=compare(actual[(flight,w,h)][0],actual[(settled,w,h)][0])
            assert_quality(diff["changedPixels"]>24 and
                diff["avgRgbDifference"]>.16,
                "Illustrated prize movement invisible to a human: "+
                f"{flight} on {w}x{h}")
            transitions.append({"resolution":f"{w}x{h}",
                                "from":flight,"to":settled,**diff})
    # Different milestones require DIFFERENT real Eldoria compositions.
    # A tinted copy of the same chest on a flat background is NOT sufficient.
    # This protects the hero hall, living explorer parchment and Vigilants
    # landscape against a regression to duplicated or invisible backgrounds.
    for w,h in [(844,390),(390,844)]:
        for left,right in (
            ("lyra","scouts"),
            ("lyra","vigilants"),
            ("scouts","vigilants"),
            ("bastion","lyra"),
            ("bastion","scouts"),
            ("bastion","vigilants"),
        ):
            a=actual[("style-"+left+"-opening",w,h)][0]
            b=actual[("style-"+right+"-opening",w,h)][0]
            diff=compare(a,b)
            assert_quality(diff["avgRgbDifference"]>.45 and
                diff["changedPixels"]>95,
                "Milestone stages still look like duplicated recolored chests: "+
                f"{left} / {right} at {w}x{h}")
            transitions.append({"resolution":f"{w}x{h}",
                                "from":left,"to":right,**diff})

    # Check the actual landscape pixels, not merely different bronze
    # captions and palette tints. A completely hidden or flat-painted backdrop
    # could pass the whole-frame comparison above; this detects that failure.
    scenic_checks=[]
    for w,h in [(844,390),(390,844)]:
        crops={}
        for style in ("lyra","scouts","vigilants"):
            source=actual[("style-"+style+"-opening",w,h)][0]
            cropped=scenery_window(source,w,h)
            metrics=sample(cropped)
            assert_quality(metrics["meanStdDev"]>4.0 and
                           metrics["uniqueColorBins"]>=13,
                "Original Eldoria scene is flattened, hidden or too dark in its own aperture: "+
                f"{style} at {w}x{h}")
            scenic_checks.append({
                "stage":style,"resolution":f"{w}x{h}",
                "sceneRoiStdDev":metrics["meanStdDev"],
                "sceneRoiColorBins":metrics["uniqueColorBins"],
            })
            crops[style]=cropped
        for left,right in (("lyra","scouts"),("lyra","vigilants"),
                           ("scouts","vigilants")):
            diff=compare(crops[left],crops[right])
            assert_quality(diff["avgRgbDifference"]>.65 and
                           diff["changedPixels"]>110,
                "Paintings are indistinguishable INSIDE their scenery aperture; "+
                f"UI tint cannot replace authentic {left}/{right} artwork at {w}x{h}")
            transitions.append({"resolution":f"{w}x{h}",
                                "from":"scenery:"+left,
                                "to":"scenery:"+right,**diff})

    # Contact sheet: one row per phone/desktop viewport, seven paid scenes;
    # second contact sheet for special-event presentation in both orientations.
    def sheet(keys,filename,columns):
        fw,fh=260,200
        rows=math.ceil(len(keys)/columns)
        out=Image.new("RGB",(columns*fw,rows*fh),(18,16,15))
        draw=ImageDraw.Draw(out)
        for i,key in enumerate(keys):
            variant,w,h=key
            source=actual[key][0]
            image=source.copy()
            image.thumbnail((fw-16,fh-37),Image.Resampling.LANCZOS)
            ix=(i%columns)*fw + (fw-image.width)//2
            iy=(i//columns)*fh + 27+(fh-37-image.height)//2
            out.paste(image,(ix,iy))
            name=f"{variant} / {w}x{h}"
            draw.text(((i%columns)*fw+6,(i//columns)*fh+5),
                      name,fill=(230,193,131))
        out.save(output/filename,optimize=True)

    paid=[(variant,w,h) for w,h in REQUIRED_SIZES for variant in DAILY]
    sheet(paid,"paid_reward_contact_sheet.png",7)
    entry=[("hud-entry",w,h) for w,h in REQUIRED_SIZES]
    sheet(entry,"authored_chest_entry_medallion_contact_sheet.png",4)
    moving=[(kind,w,h) for w,h in [(844,390),(390,844)]
        for kind in ("day7-prize-flight","special-day4-prize-flight")]
    sheet(moving,"actual_reward_card_flights_contact_sheet.png",2)
    style=[k for k in expected if k[0] in MILESTONE]
    sheet(style,"milestone_visual_styles_contact_sheet.png",4)

    result={
        "status":"AUTOMATED_NATIVE_PREFLIGHT_PASSED_ART_REVIEW_REQUIRED",
        "sourceSha":expected_sha,
        "screenshotsChecked":len(actual),
        "verifiedActualGatewayClaims":8,
        "verifiedNativePointerInteractions":56,
        "verifiedVisualOnlyMilestoneFrames":16,
        "artApproved":False,
        "fullGameVerified":False,
        "humanReviewStillRequired":True,
        "checks":{
            "imageHashesMatchUnityReport":True,
            "noFlatOrBlankNativeCanvases":True,
            "ceremonialStagesVisiblyDifferent":True,
            "eventThemesNotIdentical":True,
            "originalSceneryVisibleInItsOwnAperture":True,
            "allFourAspectRatiosPresent":True,
        },
        "transitions":transitions,
        "originalSceneryApertureChecks":scenic_checks,
        "images":details,
        # Diagnostics are not aesthetic success or failure: compare contact
        # sheets against the actual PR173 charcoal/copper/burgundy design.
        "ownerVisualLanguageWarnings":visual_language_warnings,
        "ownerRejectionOfNeonFantasyConceptRecorded":True
    }
    (output/"native_art_audit.json").write_text(
        json.dumps(result,indent=2,ensure_ascii=False),encoding="utf-8")
    print("PASS NATIVE SCREENSHOT PIXEL AUDIT:",len(actual),
          "original Unity PNGs; illustration/theme transitions visible.")
    print("REVIEW REQUIRED: owner aesthetic comparison and full game/mobile proof.")
    print("REJECTED STYLE WATCH: high-neon-violet snapshots needing human check:",
          len(visual_language_warnings))
    return result

def selftest():
    from tempfile import TemporaryDirectory
    with TemporaryDirectory() as tmp:
        # Two original image buffers at small size: verifies perceptual
        # comparison, pixel histogram and shape handling without a Unity run.
        a=Image.new("RGB",(240,160),(13,12,12))
        b=a.copy()
        dr=ImageDraw.Draw(a);dr.rectangle((55,35,170,130),fill=(89,53,25))
        dr=ImageDraw.Draw(b);dr.rectangle((55,35,170,130),fill=(220,151,61))
        source=sample(a)
        target=sample(b)
        assert_quality(source["meanStdDev"]>8,"Synthetic chest detail missing")
        assert_quality(target["warmPixels"]>100,"Warm colors not detected")
        d=compare(a,b)
        assert_quality(d["changedPixels"]>200,
            "Opened chest cannot be distinguished from closed")
        # This diagnostic catches the PARTICULAR rejected mockup aesthetic
        # while not interpreting a violet hint of the Breach as an art approval.
        wrong=Image.new("RGB",(240,160),(225,40,245))
        correct=Image.new("RGB",(240,160),(21,24,27))
        assert_quality(sample(wrong)["neonVioletRatioForHumanReview"]>.90,
            "Bright fairy-purple image must be marked for manual style review")
        assert_quality(sample(correct)["neonVioletRatioForHumanReview"]<.02,
            "Owner's matte-charcoal UI must not trigger purple-drift warning")
        # Explicit regression guard: a different bronze UI tint over a
        # FLAT colour must not count as a genuine Eldoria landscape.
        flat=Image.new("RGB",(844,390),(24,25,27))
        painted=flat.copy()
        paint=ImageDraw.Draw(painted)
        for stripe in range(28):
            paint.rectangle((90+stripe*10,80,99+stripe*10,281),
                fill=(45+stripe*4,36+stripe*2,20+stripe))
        assert_quality(sample(scenery_window(flat,844,390))["meanStdDev"]<1,
            "Flat tinted art should fail scenic-aperture preflight")
        assert_quality(sample(scenery_window(painted,844,390))["meanStdDev"]>4,
            "True varied stage art should be measurable within its scenic aperture")
        assert_quality(compare(scenery_window(flat,844,390),
                               scenery_window(painted,844,390))["changedPixels"]>100,
            "Scenic-aperture difference preflight did not detect real texture")
        a.save(Path(tmp)/"original.png")
        assert_quality((Path(tmp)/"original.png").is_file(),
            "PNG save smoke failed")
    print("PASS native art audit selftest: honest pixel comparison and no Unity claims")

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument("--selftest",action="store_true")
    ap.add_argument("--input",type=Path)
    ap.add_argument("--source-sha",type=str)
    ap.add_argument("--output",type=Path)
    args=ap.parse_args()
    if args.selftest:
        selftest()
    else:
        if not args.input or not args.source_sha or not args.output:
            ap.error("Native artifact, source SHA and audit output are all required")
        analyze(args.input,args.source_sha,args.output)

if __name__=="__main__":
    main()
