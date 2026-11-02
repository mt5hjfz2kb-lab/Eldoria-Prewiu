#!/usr/bin/env python3
"""Conservative, source-anchored training-art patch for canonical PR136.

Does NOT run Unity, alter saves, claim a Windows runner or merge anything.
Preflight requires the exact observed original Unity source blob. Apply only
after the current Valoria owner releases its overlapping UI write scope.
"""
import argparse
import difflib
import hashlib
from pathlib import Path

TARGET = Path("Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV3ManagementScreens.cs")
EXPECTED_BLOB = "04c6216dcf486ba56588021c6412e11312569425"


def git_blob_sha(data):
    return hashlib.sha1(b"blob " + str(len(data)).encode() + b"\\0" + data).hexdigest()


def replace_once(text, before, after, name):
    found = text.count(before)
    if found != 1:
        raise RuntimeError(f"{name}: expected exactly one anchor; found {found}")
    return text.replace(before, after, 1)


def patch(original):
    source = original
    source = replace_once(source,
        "RectTransform root, layoutRoot, header, left, right, footer, quantityRow;",
        "RectTransform root, layoutRoot, header, left, right, footer, quantityRow;\n"
        "        RectTransform trainingCostCard, trainingStatusCard, trainingIdentityCard;",
        "field registration")

    source = replace_once(source,
        "                trainingTierPortraits[i].preserveAspect=true;",
        "                trainingTierPortraits[i].preserveAspect=true;\n"
        "                // Paper and independent nine-slice metal trim live UNDER the real card controls.\n"
        "                TrainingPaper((RectTransform)trainingTierChoices[i].transform,\n"
        "                    \"Training / tier \"+tier+\" layered paper\",.92f);",
        "tier card material")

    source = replace_once(source,
        '            status=Text("Available and pending troops",right,16,Muted);',
        '            status=Text("Available and pending troops",right,16,Muted);\n'
        '            // Separate reading islands, no full-height opaque blue order window.\n'
        '            trainingCostCard=TrainingPaper(right,"Training / real cost manuscript",.91f);\n'
        '            trainingStatusCard=TrainingPaper(right,"Training / real timer manuscript",.89f);\n'
        '            trainingIdentityCard=TrainingPaper(left,"Training / archer identity manuscript",.82f);',
        "real information plates")

    source = replace_once(source,
        """            unitTitle.color=training?new Color(.10f,.08f,.055f):Ivory;""",
        """            trainingCostCard.gameObject.SetActive(training);
            trainingStatusCard.gameObject.SetActive(training);
            trainingIdentityCard.gameObject.SetActive(training);
            if(training)
            {
                // Deliberate structural pivot: courtyard is the scene, not an
                // oversized blue document obscuring two-thirds of the screen.
                paneSurfaces[right].enabled=false;
                right.GetComponent<RoyalGoldV2FrameGraphic>().enabled=false;
            }
            summary.color=training?new Color(.13f,.10f,.065f):Ivory;
            status.color=training?new Color(.20f,.16f,.105f):Muted;
            unitTitle.color=training?new Color(.10f,.08f,.055f):Ivory;""",
        "remove full-blue order surface and keep ink legible")

    source = replace_once(source,
        """            unitTitle.rectTransform.SetParent(left,false);
            unitDetail.rectTransform.SetParent(left,false);
            bool stackedIdentity=wide&&!compactLandscape;
            // Reserve a separate identity column: the previous compact title crossed the face.
            float titleX=stackedIdentity?width*.56f:wide?width*.36f:width*.67f;
            float titleW=stackedIdentity?width*.44f:wide?left.rect.width-titleX:width-titleX-12;
            Place(unitTitle.rectTransform,titleX,artH*.04f,titleW,wide?72:90);
            Place(unitDetail.rectTransform,titleX,artH*.04f+(wide?82:100),titleW,wide?160:230);
            unitTitle.fontSize=wide?28:34;
            unitDetail.fontSize=wide?20:28;""",
        """            // The wide title used x=.56*TOTAL width in a .54*TOTAL-wide
            // LEFT panel. It could render outside its parent, under the right
            // pane and across the archer. Give it a separate raised column.
            bool elevatedIdentity=wide&&!compactLandscape;
            var identityParent=elevatedIdentity?layoutRoot:left;
            unitTitle.rectTransform.SetParent(identityParent,false);
            unitDetail.rectTransform.SetParent(identityParent,false);
            trainingIdentityCard.SetParent(identityParent,false);
            float titleX=elevatedIdentity?x+width*.56f:
                wide?left.rect.width*.67f:width*.69f;
            float titleW=elevatedIdentity?width*.40f:
                Mathf.Max(84f,left.rect.width-titleX-12f);
            float titleY=elevatedIdentity?bodyY+artH*.055f:artH*.04f;
            float titleHeight=wide?68f:90f;
            float detailY=titleY+(wide?76f:100f);
            float detailHeight=elevatedIdentity?Mathf.Min(140f,artH*.45f):
                Mathf.Max(65f,Mathf.Min(wide?150f:210f,artH-(detailY-titleY)-15f));
            Place(unitTitle.rectTransform,titleX,titleY,titleW,titleHeight);
            Place(unitDetail.rectTransform,titleX,detailY,titleW,detailHeight);
            Place(trainingIdentityCard,titleX-9f,titleY-10f,titleW+18f,
                detailY+detailHeight-titleY+21f);
            trainingIdentityCard.SetAsFirstSibling();
            unitTitle.fontSize=wide?28:32;
            unitDetail.fontSize=wide?20:26;""",
        "identity collision correction")

    source = replace_once(source,
        """            // Explicit stacking retains live controls above source surfaces.
            approvedArcher.rectTransform.SetAsFirstSibling();""",
        """            // Size real cost and countdown islands to their existing live
            // labels. They must never intercept touch/slider raycasts.
            if(wide)
            {
                Place(trainingCostCard,pad-8f,orderH*.28f-9f,
                    orderW-pad*2f+16f,orderH*.20f+18f);
                Place(trainingStatusCard,pad-8f,orderH*.63f-8f,
                    orderW-pad*2f+16f,wide?68f:84f);
            }
            else
            {
                Place(trainingCostCard,pad-8f,145f,
                    orderW-pad*2f+16f,86f);
                Place(trainingStatusCard,pad-8f,331f,
                    orderW-pad*2f+16f,84f);
            }
            trainingCostCard.SetAsFirstSibling();
            trainingStatusCard.SetAsFirstSibling();
            // Explicit stacking retains live controls above source surfaces.
            approvedArcher.rectTransform.SetAsFirstSibling();""",
        "bounded training cost and timer layout")

    source = replace_once(source,
        """        static RectTransform Rect(string name,Transform parent)""",
        """        static RectTransform TrainingPaper(RectTransform parent,string name,float opacity)
        {
            var r=Rect(name,parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var paper=r.gameObject.AddComponent<Image>();
            paper.color=new Color(.96f,.88f,.70f,opacity);
            paper.raycastTarget=false;
            var rim=RoyalGoldV41SourceArt.Surface(r,"HeroCardNineSliceGold",48);
            if(rim.sprite)
            {
                rim.type=Image.Type.Sliced;
                rim.fillCenter=false;
                rim.pixelsPerUnitMultiplier=3.4f;
                rim.color=new Color(.82f,.65f,.34f,.72f);
                rim.rectTransform.offsetMin=new Vector2(2f,2f);
                rim.rectTransform.offsetMax=new Vector2(-2f,-2f);
            }
            else rim.enabled=false;
            rim.raycastTarget=false;
            r.SetAsFirstSibling();
            return r;
        }

        static RectTransform Rect(string name,Transform parent)""",
        "reusable source-backed paper and metal")

    # These are assertions about source wiring, NOT claims of a real Unity PASS.
    assert source.count("trainingCostCard.gameObject.SetActive(training);") == 1
    assert source.count('TrainingPaper(right,"Training / real cost manuscript"') == 1
    assert source.count("SliceRules.RecruitWoodCostFor(selectedCount)") == 1
    assert source.count("SliceRules.RecruitStoneCostFor(selectedCount)") == 1
    assert source.count("SliceRules.RecruitSecondsFor(selectedCount)") >= 1
    assert source.count('string kind=training?"Recruit":"ConfigureMarch";') == 1
    assert source.count('gateway.Snapshot()') >= 2
    assert source != original
    return source


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--path",type=Path,default=TARGET)
    parser.add_argument("--apply",action="store_true",
        help="Write candidate ONLY after canonical PR136 UI ownership is released.")
    args=parser.parse_args()
    raw=args.path.read_bytes()
    actual=git_blob_sha(raw)
    if actual != EXPECTED_BLOB:
        raise SystemExit(f"REFUSED: source blob {actual}, expected {EXPECTED_BLOB}. "
                         "Reconcile newer PR136 by hand; never overwrite it.")
    original=raw.decode("utf-8")
    changed=patch(original)
    diff="".join(difflib.unified_diff(original.splitlines(True),
        changed.splitlines(True),fromfile="a/"+str(TARGET),
        tofile="b/"+str(TARGET),n=5))
    if args.apply:
        args.path.write_text(changed,encoding="utf-8",newline="")
        print("Applied guarded TRAINING_ART_FINISH candidate. Native Unity A/B still required.")
    else:
        print(diff,end="")
        print("DRY RUN only. Does not modify Unity. Test four resolutions on exact new HEAD.")


if __name__=="__main__":
    main()
