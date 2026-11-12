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
        # The native portrait story heading now sits at about 25-29% of
        # screen height. This deliberately samples BELOW its 40px ink band
        # and ABOVE the actual coffer. Otherwise a decorative caption can
        # fake painterly detail in an empty scene.
        bounds = (.23, .295, .77, .36)
    else:
        raise ValueError("Scenic aperture unsupported: %sx%s" % (width,height))
    x0,y0,x1,y1 = bounds
    return image.crop((round(x0*width),round(y0*height),
                       round(x1*width),round(y1*height)))

def paid_label_pixel_proof(image, variant, width, height):
    """Verify names AND paid quantities actually contribute visible pixels.

    The first *real* Unity 1600x900 frames showed four gold-framed cards
    with their icons but entirely blank label/amount columns. Their Unity
    Text components were nonempty and height metrics passed, yet the GPU
    image had literally zero illuminated pixels in the right-side column.
    Sampling the genuine rendered RGB24 output prevents that false PASS.
    This geometry contract applies to current one-row landscape layout only;
    portrait is still checked via native RectTransform and human captures.
    """
    assert_quality(width>height, "Pixel proof requires landscape reward rows")
    count=4 if variant=="opened" else 2
    assert_quality(variant in ("opened","special-day4-received"),
                   "Unknown actual-paid reward style")
    # Unity DailyTreasurePanel.Layout uses a focused 650px paid coffer,
    # not the 740px calendar: day 4 has 190px cards, day 7 has 146px.
    # Native 42818514510 visibly drew x90/x60 in the TWO day-four cards;
    # the previous 140px-only sampling read an empty edge and falsely
    # rejected true text. Pixel evidence must follow actual card geometry.
    panel_width=min(width-24,650)
    gap=6
    card_width=min(190 if count==2 else 146,
                   (panel_width-38-(count-1)*gap)/count)
    shelf_y=round(height*.5+69)
    rgb=image.convert("RGB")
    results=[]
    for i in range(count):
        center_x=round(width*.5+
            (i-(count-1)/2)*(card_width+gap))
        # The new premium short-card layout reserves this RIGHT column for
        # resource name and actual quantity, away from card frame and icon.
        # Separate text rectangles, not one merged bright-pixel count:
        # otherwise a rendered NAME can hide a missing or cropped QUANTITY.
        # These are from the actual landscape short-card production geometry:
        # label center y=+12, quantity y=-15 in Unity's upward Y system.
        label_width=card_width-62
        # Unity places both text rectangles at +24px relative to the
        # card centre; take the INNER 76% of the actual right-side column.
        # Do not sample the gilt border, wood/stone icon or adjacent card.
        label_center=center_x+card_width*.5-label_width*.5-7
        label_left=round(label_center-label_width*.38)
        label_right=round(label_center+label_width*.38)
        name_window=(label_left,shelf_y-23,label_right,shelf_y)
        quantity_window=(label_left,shelf_y+5,
                         label_right,shelf_y+24)
        painted=[]
        for field,window in (("name",name_window),
                             ("actual quantity",quantity_window)):
            crop=rgb.crop(window)
            colored=sum(1 for r,g,b in crop.getdata()
                if r>95 and g>65 and b>40)
            assert_quality(colored>=8,
                "Paid reward card "+str(i+1)+" has missing/occluded "+field+
                " in real "+str(width)+"x"+str(height)+" Unity pixels: "+
                variant+" count="+str(colored))
            painted.append(colored)
        results.append({"namePixels":painted[0],
                        "quantityPixels":painted[1]})
    return results



QUIET_DAYS = (1, 2, 3, 5, 6)
QUIET_ITEM_COUNTS = {1:1, 2:1, 3:2, 5:1, 6:2}

def quiet_card_centers(day,width,height):
    """Native production quiet layout, from DailyTreasurePanel.Layout()."""
    assert_quality(day in QUIET_DAYS, "Unknown ordinary reward day")
    portrait=height>width
    panel_width=min(width-24,440 if portrait else 740)
    count=QUIET_ITEM_COUNTS[day]
    gap=8 if portrait else 6
    card_width=min(175,(panel_width-38-(count-1)*gap)/count)
    shelf_y=height*.5-(64 if portrait else 35)
    centers=[width*.5+(i-(count-1)*.5)*(card_width+gap)
             for i in range(count)]
    return centers,shelf_y,(129 if portrait else 115)

def quiet_label_pixel_proof(image,day,width,height):
    """Require *both* distinct visible fields on every real quiet card.

    A working command and existing text nodes are not enough. On the first
    real Unity 1600x900 QA, paid cards were effectively empty despite source
    and typography preflights passing. Keep each ROI inside its actual item
    art: title at -0.08 card height and quantity at -0.35 in Unity Y.
    """
    rgb=image.convert("RGB")
    centers,shelf_y,card_height=quiet_card_centers(day,width,height)
    evidence=[]
    for i,center in enumerate(centers):
        regions=(("name",shelf_y+card_height*.08,9),
                 ("quantity",shelf_y+card_height*.35,10))
        pixel_counts=[]
        for field,y,half_h in regions:
            roi=(round(center-48),round(y-half_h),
                 round(center+48),round(y+half_h))
            crop=rgb.crop(roi)
            bright=sum(1 for red,green,blue in crop.getdata()
                if red>95 and green>65 and blue>40)
            assert_quality(bright>=12,
                "Ordinary day "+str(day)+" card "+str(i+1)+
                " has no visible "+field+" in real Unity "+
                str(width)+"x"+str(height)+" pixels; count="+str(bright))
            pixel_counts.append(bright)
        evidence.append({"namePixels":pixel_counts[0],
                         "quantityPixels":pixel_counts[1]})
    return evidence

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
    assert_quality(r.get("quietImages")==20 and
         r.get("quietGatewayClaims")==20 and
         r.get("quietPointerHits")==60 and
         len(r.get("quietShots") or [])==20,
         "Five ordinary days across four native viewports are missing")
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
        is_preclaim=variant in ("calendar","special-day4-calendar")
        assert_quality(metrics["meanStdDev"]>(2.0 if is_entry else 8.0) and
            metrics["uniqueColorBins"]>=(8 if is_entry else 25),
            "Blank or flat native canvas; original painted artwork not rendered: "+filename)
        assert_quality(metrics["warmPixels"]>(1 if is_entry else 4),
            "Missing gilded accents, chest medallion or warm light: "+filename)
        if is_entry or is_preclaim:
            assert_quality(record.get("preserved") is False and
                record.get("coversReward") is False and
                record.get("readyToClaim") is is_preclaim,
                "Unclaimed HUD/calendar may never masquerade as already-paid loot: "+filename)
        elif variant.startswith("style-"):
            assert_quality(record.get("preserved") is False and
                record.get("coversReward") is False and
                record.get("readyToClaim") is False,
                "Visual-only style probe may never masquerade as paid loot: "+filename)
        else:
            assert_quality(record.get("preserved") is True and
                record.get("coversReward") is True and
                record.get("readyToClaim") is False,
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

    # A screenshot of the four luxury cards is NOT quality evidence if
    # only the icons and frames render. Require real luminous TEXT pixels.
    # This rejects the exact first native defect independently of C# bounds.
    for width,height in REQUIRED_SIZES:
        if width<=height:
            continue
        for variant in ("opened","special-day4-received"):
            paid_label_pixel_proof(
                actual[(variant,width,height)][0],variant,width,height)


    # Do not hide the ordinary paid cards under the old exact 52-shot count.
    # The 20 extra screenshots live in a separate directory and represent
    # independently credited days 1,2,3,5,6 on all four screen geometries.
    quiet_dir=folder/"quiet"
    assert_quality(quiet_dir.is_dir(),
        "Ordinary-day native receipt directory absent")
    quiet_expected={
        ("quiet-day"+str(day)+"-received",w,h)
        for w,h in REQUIRED_SIZES for day in QUIET_DAYS
    }
    quiet_actual={}
    quiet_details=[]
    for record in (r.get("quietShots") or []):
        filename=record.get("file","")
        parsed=re.fullmatch(
            r"real-unity-daily-quiet-day([12356])-received-([0-9]+)x([0-9]+)\.png",
            filename)
        assert_quality(parsed is not None,
            "Wrong ordinary-day evidence name: "+filename)
        day,width,height=map(int,parsed.groups())
        key=("quiet-day"+str(day)+"-received",width,height)
        assert_quality(key in quiet_expected and key not in quiet_actual,
            "Duplicate/unknown native ordinary-day image: "+filename)
        assert_quality((record.get("width"),record.get("height"))==
            (width,height) and record.get("nativeCanvas") is True and
            record.get("preserved") is True and
            record.get("coversReward") is True and
            record.get("readyToClaim") is False,
            "Ordinary reward not backed by real paid data: "+filename)
        file=quiet_dir/filename
        assert_quality(file.is_file(),
            "Missing original Unity ordinary-day PNG: "+filename)
        data=file.read_bytes()
        assert_quality(len(data)>7000 and len(data)==record.get("bytes") and
            data.startswith(b"\x89PNG\r\n\x1a\n") and
            hashlib.sha256(data).hexdigest()==record.get("sha256"),
            "Corrupted or altered ordinary-day Unity PNG: "+filename)
        with Image.open(file) as image:
            assert_quality(image.format=="PNG" and
                           image.size==(width,height) and
                           image.mode in ("RGB","RGBA"),
                           "Ordinary reward not genuine expected PNG: "+filename)
            rgb=image.convert("RGB").copy()
        style=sample(rgb)
        assert_quality(style["meanStdDev"]>8 and
                       style["uniqueColorBins"]>25 and
                       style["warmPixels"]>4,
                       "Ordinary native reward blank or no genuine premium art: "+filename)
        pixels=quiet_label_pixel_proof(rgb,day,width,height)
        quiet_actual[key]=(rgb,style)
        quiet_details.append({
            "file":filename,"day":day,"size":[width,height],
            "visibleReceiptTextPixels":pixels,
            "artApproved":False
        })
    assert_quality(set(quiet_actual)==quiet_expected and
                   len(list(quiet_dir.glob("*.png")))==20,
                   "Not exactly twenty authentic ordinary-day receipts")

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
    def sheet(keys,filename,columns,origin=actual):
        fw,fh=260,200
        rows=math.ceil(len(keys)/columns)
        out=Image.new("RGB",(columns*fw,rows*fh),(18,16,15))
        draw=ImageDraw.Draw(out)
        for i,key in enumerate(keys):
            variant,w,h=key
            source=origin[key][0]
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
    # Keep one predictable comparison row per orientation and group the
    # two frames of each real event together. A set's random iteration made
    # art reviews hard to compare and could conceal scene regressions.
    style=[(variant,w,h) for w,h in ((844,390),(390,844))
           for variant in MILESTONE]
    sheet(style,"milestone_visual_styles_contact_sheet.png",4)
    quiet_keys=[("quiet-day"+str(day)+"-received",w,h)
        for w,h in REQUIRED_SIZES for day in QUIET_DAYS]
    sheet(quiet_keys,"ordinary_paid_rewards_contact_sheet.png",5,
          origin=quiet_actual)

    result={
        "status":"AUTOMATED_NATIVE_PREFLIGHT_PASSED_ART_REVIEW_REQUIRED",
        "sourceSha":expected_sha,
        "screenshotsChecked":len(actual),
        "verifiedActualGatewayClaims":8,
        "verifiedUnpaidCalendarFrames":8,
        "verifiedNativePointerInteractions":56,
        "verifiedVisualOnlyMilestoneFrames":16,
        "additionalQuietDayRealGatewayClaims":20,
        "additionalQuietDayNativePointerInteractions":60,
        "additionalQuietDayOriginalPngCount":len(quiet_actual),
        "ordinaryDayReceiptSamples":quiet_details,
        "artApproved":False,
        "fullGameVerified":False,
        "humanReviewStillRequired":True,
        "checks":{
            "imageHashesMatchUnityReport":True,
            "unclaimedCalendarsAreNotMislabelledPaid":True,
            "noFlatOrBlankNativeCanvases":True,
            "ceremonialStagesVisiblyDifferent":True,
            "eventThemesNotIdentical":True,
            "originalSceneryVisibleInItsOwnAperture":True,
            "allFourAspectRatiosPresent":True,
            "ordinaryDayNamesAndAmountsVisibleIndependently":True,
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


def analyze_partial(folder, expected_sha, output):
    """Inspect authentic frames captured BEFORE a failing native assertion.

    This is intentionally a diagnostic, not a shortcut around the exact 72
    frame/116 pointer/28 claim certification. In particular, a failure with
    eight PNGs must not be misreported as zero rendered evidence.
    """
    folder=folder.resolve()
    output=output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    report_file=folder/"report.json"
    assert_quality(report_file.is_file(), "Missing real Unity partial run report")
    report=json.loads(report_file.read_text(encoding="utf-8-sig"))
    assert_quality(report.get("status")=="NATIVE_COMPONENT_FAILED",
                   "Partial review is reserved for a FAILED Unity capture")
    assert_quality(report.get("source")==expected_sha,
                   "Partial frames do not belong to this exact PR HEAD")
    assert_quality(report.get("artApproved") is False and
                   report.get("fullSceneVerified") is False,
                   "Native failure must never claim full game or art approval")
    regular=report.get("shots") or []
    quiet=report.get("quietShots") or []
    assert_quality(len(regular)==report.get("images") and
                   len(quiet)==report.get("quietImages"),
                   "Partial screenshot ledger does not match files captured")
    assert_quality(len(regular)<=52 and len(quiet)<=20 and
                   report.get("graphicalPointerHits",0)<=56 and
                   report.get("quietPointerHits",0)<=60 and
                   report.get("realGatewayClaims",0)<=8 and
                   report.get("quietGatewayClaims",0)<=20,
                   "Native partial ledger exceeds its genuine test limits")
    unique=set()
    verified=[]
    previews=[]
    for quiet_kind, records in ((False,regular),(True,quiet)):
        for record in records:
            name=record.get("file","")
            valid=re.fullmatch(
                r"real-unity-daily-[a-z0-9-]+-(?:1600x900|900x1600|844x390|390x844)\.png",
                name)
            assert_quality(valid is not None and name not in unique,
                           "Unexpected/duplicated partial frame "+name)
            unique.add(name)
            original=folder/("quiet" if quiet_kind else "")/name
            assert_quality(original.is_file(),
                           "Listed genuine partial frame absent "+name)
            data=original.read_bytes()
            assert_quality(len(data)>7000 and
                           len(data)==record.get("bytes") and
                           data.startswith(b"\x89PNG\r\n\x1a\n") and
                           hashlib.sha256(data).hexdigest()==record.get("sha256"),
                           "Partial screenshot metadata/hash invalid "+name)
            with Image.open(original) as frame:
                w,h=frame.size
                assert_quality(frame.format=="PNG" and
                               (w,h) in REQUIRED_SIZES and
                               (w,h)==(record.get("width"),record.get("height")),
                               "Wrong native frame dimensions "+name)
                pixels=frame.convert("RGB").copy()
            assert_quality(record.get("nativeCanvas") is True,
                           "Partial screenshot was not produced on Unity canvas")
            m=sample(pixels)
            verified.append({
                "name":name,"quiet":quiet_kind,"width":w,"height":h,
                "sourceSha256":record["sha256"],"view":record.get("view"),
                "paid":record.get("coversReward"),
                "luminance":m["luminance"],"colorBins":m["uniqueColorBins"]})
            previews.append((name,pixels))
    # Every file should be accounted for. Otherwise an orphaned screenshot
    # may represent exactly the stage at which a Unity exception occurred.
    present={p.name for p in folder.glob("*.png")}
    present.update(p.name for p in (folder/"quiet").glob("*.png"))
    assert_quality(present==unique,
                   "Native PNG on disk missing from partial capture ledger")
    for start in range(0,len(previews),16):
        selected=previews[start:start+16]
        cols=4
        rows=math.ceil(len(selected)/cols)
        sheet=Image.new("RGB",(cols*280,rows*205),(20,18,17))
        draw=ImageDraw.Draw(sheet)
        for i,(name,frame) in enumerate(selected):
            x=(i%cols)*280
            y=(i//cols)*205
            thumb=frame.copy()
            thumb.thumbnail((268,168),Image.Resampling.LANCZOS)
            sheet.paste(thumb,(x+(280-thumb.width)//2,y+31+
                               (168-thumb.height)//2))
            draw.text((x+4,y+4),name.replace("real-unity-daily-",""),
                      fill=(224,187,129))
        sheet.save(output/f"FAILED_unity_partial_frames_{start//16+1}.png",
                   optimize=True)
    summary={
        "status":"INCOMPLETE_NATIVE_FAILURE_DIAGNOSTICS_ONLY",
        "sourceSha":expected_sha,
        "originalFailedStatus":report["status"],
        "exception":report.get("note",""),
        "realUnityPngInspected":len(verified),
        "realGatewayClaimsBeforeFailure":report.get("realGatewayClaims",0),
        "nativePointerHitsBeforeFailure":report.get("graphicalPointerHits",0),
        "quietGatewayClaimsBeforeFailure":report.get("quietGatewayClaims",0),
        "quietPointerHitsBeforeFailure":report.get("quietPointerHits",0),
        "expectedForCertification":{"png":72,"pointer":116,"claims":28},
        "artApproved":False,"unityNativePass":False,
        "fullGameVerified":False,"humanReviewRequired":True,
        "frames":verified
    }
    (output/"FAILED_unity_partial_manifest.json").write_text(
        json.dumps(summary,indent=2,ensure_ascii=False),encoding="utf-8")
    print("PARTIAL NATIVE FAILURE:",len(verified),
          "original Unity frames retained for corrective review; NO ART PASS.")
    return summary

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
        # A new portrait story-title band must not be mistaken for the
        # genuine art. Draw a highly textured FAKE title precisely where
        # Unity now places it; it must be excluded from our art aperture.
        flat_portrait=Image.new("RGB",(390,844),(24,25,27))
        fake_heading=flat_portrait.copy()
        pen=ImageDraw.Draw(fake_heading)
        for x in range(68,322,7):
            pen.rectangle((x,193,x+3,241),
                          fill=(170+x//8,130+x//12,75))
        assert_quality(sample(scenery_window(fake_heading,390,844))[
            "meanStdDev"]<1,
            "Portrait fake-ink heading contaminated scenic picture sampling")
        painted_portrait=flat_portrait.copy()
        pen=ImageDraw.Draw(painted_portrait)
        for x in range(88,310,7):
            pen.rectangle((x,245,x+4,320),
                          fill=(65+x//12,48+x//20,35+x//30))
        assert_quality(sample(scenery_window(painted_portrait,390,844))[
            "meanStdDev"]>4,
            "Authentic textured portrait scenery must appear below heading")
        # Native run 42609669589 had ZERO visible text pixels across
        # the four paid card columns. A source-only positive/negative probe
        # proves that our actual-image gate does reject that severe regression
        # and accepts visible dark-fantasy ivory/gold glyph strokes.
        blank=Image.new("RGB",(1600,900),(20,18,19))
        failed=False
        try:
            paid_label_pixel_proof(blank,"opened",1600,900)
        except ValueError:
            failed=True
        assert_quality(failed,"A completely blank paid receipt must fail pixel QA")
        ink=ImageDraw.Draw(blank)
        shelf_y=450+69
        for i in range(4):
            center=round(800+(i-1.5)*152)
            ink.rectangle((center+25,shelf_y-12,center+45,shelf_y-10),
                          fill=(220,183,129))
        # Regression: a screenshot may show every resource NAME while the
        # actual earned quantity is missing. Both must be independently real.
        failed=False
        try:
            paid_label_pixel_proof(blank,"opened",1600,900)
        except ValueError:
            failed=True
        assert_quality(failed,"Visible names with absent quantities must fail")
        for i in range(4):
            center=round(800+(i-1.5)*152)
            ink.rectangle((center+25,shelf_y+8,center+45,shelf_y+10),
                          fill=(235,214,173))
        assert_quality(len(paid_label_pixel_proof(
            blank,"opened",1600,900))==4,
            "Names AND actual paid quantities must pass independently")
        only_quantity=Image.new("RGB",(1600,900),(20,18,19))
        ink=ImageDraw.Draw(only_quantity)
        for i in range(4):
            center=round(800+(i-1.5)*152)
            ink.rectangle((center+25,shelf_y+8,center+45,shelf_y+10),
                          fill=(235,214,173))
        failed=False
        try:
            paid_label_pixel_proof(only_quantity,"opened",1600,900)
        except ValueError:
            failed=True
        assert_quality(failed,"Visible quantities with missing names must fail")

        # Native 42818514510 proved day-four names AND counts exist in two
        # 190px cards; previous 140px ROI sampled blank card edges. This
        # synthetic two-card case rejects EITHER missing field and accepts
        # real-looking letters in the correct Unity right-side column.
        day4=Image.new("RGB",(1600,900),(20,18,19))
        pen=ImageDraw.Draw(day4)
        for center in (702,898):
            pen.rectangle((center+9,507,center+39,511),
                          fill=(220,183,129))
            pen.rectangle((center+9,533,center+39,537),
                          fill=(235,214,173))
        assert_quality(len(paid_label_pixel_proof(
            day4,"special-day4-received",1600,900))==2,
            "Two authentic day-four cards must allow separate paid labels")
        for center in (702,898):
            pen.rectangle((center+9,533,center+39,537),
                          fill=(20,18,19))
        failed=False
        try:
            paid_label_pixel_proof(day4,"special-day4-received",1600,900)
        except ValueError:
            failed=True
        assert_quality(failed,
            "Day-four names must not conceal a missing genuine quantity")

        # Five day-specific ordinary reward receipts need visual proof too.
        # A fully blank image, a title-only image, or a quantity-only image
        # must never pass the separate right-side paid receipt check.
        for width,height,day in ((844,390,6),(390,844,3),(1600,900,1)):
            blank=Image.new("RGB",(width,height),(19,18,20))
            failed=False
            try: quiet_label_pixel_proof(blank,day,width,height)
            except ValueError: failed=True
            assert_quality(failed,"Blank ordinary-day labels were accepted")
            centers,y,card_h=quiet_card_centers(day,width,height)
            name_only=blank.copy()
            dr=ImageDraw.Draw(name_only)
            for x in centers:
                dr.rectangle((round(x-15),round(y+card_h*.08-2),
                              round(x+15),round(y+card_h*.08+2)),
                              fill=(215,184,123))
            failed=False
            try: quiet_label_pixel_proof(name_only,day,width,height)
            except ValueError: failed=True
            assert_quality(failed,"Quiet day with missing quantity was accepted")
            both=name_only.copy()
            dr=ImageDraw.Draw(both)
            for x in centers:
                dr.rectangle((round(x-15),round(y+card_h*.35-2),
                              round(x+15),round(y+card_h*.35+2)),
                              fill=(235,215,174))
            assert_quality(len(quiet_label_pixel_proof(both,day,width,height))==
                           QUIET_ITEM_COUNTS[day],
                           "Valid ordinary paid text should pass independently")
        # Failed captures retain useful pixels but NEVER earn certification.
        partial_dir=Path(tmp)/"failed"
        partial_dir.mkdir()
        diagnostic_image=Image.new("RGB",(390,844))
        for yy in range(844):
            for xx in range(390):
                diagnostic_image.putpixel((xx,yy),
                    ((xx*19+yy*7)%256,(xx*11+yy*3)%256,(xx*5+yy*13)%256))
        diagnostic_name="real-unity-daily-calendar-390x844.png"
        diagnostic_path=partial_dir/diagnostic_name
        diagnostic_image.save(diagnostic_path)
        raw=diagnostic_path.read_bytes()
        partial_report={
            "status":"NATIVE_COMPONENT_FAILED","source":"testsha",
            "note":"synthetic fixture interruption",
            "artApproved":False,"fullSceneVerified":False,
            "images":1,"quietImages":0,"realGatewayClaims":0,
            "graphicalPointerHits":1,"quietPointerHits":0,
            "quietGatewayClaims":0,"quietShots":[],
            "shots":[{"file":diagnostic_name,"width":390,"height":844,
                      "bytes":len(raw),"sha256":hashlib.sha256(raw).hexdigest(),
                      "nativeCanvas":True,"coversReward":False,
                      "view":"Test-only interruption"}]
        }
        (partial_dir/"report.json").write_text(
            json.dumps(partial_report),encoding="utf-8")
        partial_out=Path(tmp)/"partial_review"
        result=analyze_partial(partial_dir,"testsha",partial_out)
        assert_quality(result["realUnityPngInspected"]==1 and
            result["unityNativePass"] is False and
            (partial_out/"FAILED_unity_partial_frames_1.png").is_file(),
            "Partial diagnostic lost a captured Unity screenshot")
        partial_report["shots"][0]["sha256"]="0"*64
        (partial_dir/"report.json").write_text(
            json.dumps(partial_report),encoding="utf-8")
        tampered_rejected=False
        try:
            analyze_partial(partial_dir,"testsha",partial_out)
        except ValueError:
            tampered_rejected=True
        assert_quality(tampered_rejected,
                       "Partial diagnostic accepted a mismatched PNG hash")
        a.save(Path(tmp)/"original.png")
        assert_quality((Path(tmp)/"original.png").is_file(),
            "PNG save smoke failed")
    print("PASS native art audit selftest: honest pixel comparison and no Unity claims")

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument("--selftest",action="store_true")
    ap.add_argument("--partial",action="store_true",
                    help="Diagnose a FAILED run; never count as native PASS")
    ap.add_argument("--input",type=Path)
    ap.add_argument("--source-sha",type=str)
    ap.add_argument("--output",type=Path)
    args=ap.parse_args()
    if args.selftest:
        selftest()
    else:
        if not args.input or not args.source_sha or not args.output:
            ap.error("Native artifact, source SHA and audit output are all required")
        if args.partial:
            analyze_partial(args.input,args.source_sha,args.output)
        else:
            analyze(args.input,args.source_sha,args.output)

if __name__=="__main__":
    main()
