#!/usr/bin/env python3
"""Fail-closed source/asset gate for the STAGED (not yet native-certified) mailbox.
Source checks are not Unity screenshots and never mean artistic sign-off.
"""
from pathlib import Path
import hashlib
import struct

ROOT = Path(__file__).resolve().parents[2]
BASE = ROOT / "Unity/Assets/Eldoria/BreachWatch"
R = BASE / "Runtime"
S = (R / "BreachWatchMailboxTheme.cs").read_text()
B = (R / "BreachWatchMailboxBridge.cs").read_text()
HUD = (R / "BreachWatchHud.cs").read_text()


def check(condition, detail):
    if not condition:
        raise AssertionError(detail)


def main():
    for name in ("BreachWatchMailboxTheme", "BreachWatchMailboxBridge"):
        check((R / (name + ".cs.meta")).exists(),
              "Mailbox code requires stable Unity meta: " + name)
    scene=R/"BreachWatchMailboxEmptyArtwork.cs"
    check(scene.is_file() and scene.with_suffix(".cs.meta").is_file(),
          "The original large illustrated empty-mail scene needs a stable Unity GUID")
    painting=scene.read_text()
    check("MaskableGraphic" in painting and
          "OnPopulateMesh" in painting and
          'VigiliaArt/MailboxArtwork/sealed_letter' in painting and
          "Resources.Load<Sprite>(ArtworkKey)" in painting and
          "art.raycastTarget = false" in painting and
          "if (!illustration) return" in painting and
          "vh.AddTriangle(0, 1, 2)" in painting,
          "Empty mail must render ONE genuine source-authored transparent raster via a non-interactive mesh, never the rejected procedural envelope")
    # The previous validator required 'Sprite' to be ABSENT and the rejected
    # vector drawing to be present. It was obsolete after 996deb9. Verify
    # the actual immutable authored original and its bounded GPU importer.
    image=BASE/"Resources/VigiliaArt/MailboxArtwork/sealed_letter.png"
    meta=image.with_suffix(".png.meta")
    check(image.is_file() and meta.is_file(), 
          "Original sealed-letter art and Unity importer must be in this sparse checkout")
    pixels=image.read_bytes()
    check(hashlib.sha256(pixels).hexdigest()==
          "2fc1e38d18e68ab9f144f2742594ac4c1ca2fb6011659a3fbd78ddd26d84c1f1",
          "Original sealed-letter PNG changed without owner-approved art provenance")
    check(pixels[:8]==bytes((137,80,78,71,13,10,26,10)) and
          len(pixels)>=29 and 
          struct.unpack(">IIBBBBB",pixels[16:29])[:4]==(1536,1024,8,6),
          "Sealed-letter source must stay authentic 1536x1024 RGBA PNG")
    importer=meta.read_text()
    check("guid: 46e84bddeab249449911cbd4ffef992e" in importer and
          "isReadable: 0" in importer and "enableMipMap: 0" in importer and
          "textureCompression: 0" in importer and
          importer.count("maxTextureSize: 512")>=6 and
          all("buildTarget: "+p in importer for p in
              ("DefaultTexturePlatform","Standalone","iPhone","Android","WebGL")),
          "Bound original mailbox artwork max512, non-readable, lossless on all platforms")
    check("BreachWatchMailboxEmptyArtwork.Install" in S and
          'text.name=="Mailbox / genuine empty state"' in S and
          "realEmptySentence.gameObject.activeInHierarchy" in S and
          "realListSummary.enabled=originalSummaryVisible&&!actuallyEmpty" in S and
          "pageCaption.enabled=originalPageCaptionVisible&&!actuallyEmpty" in S and
          "RestoreEmptyLabel()" in S,
          "Real empty status must be centered, reversible and never duplicated")
    check("mailbox.Open()" in B and "mailbox.SynchronizeLayout()" in B and
          "mailbox.Close()" in B,
          "Mailbox must reuse genuine page/command controller")
    check('child.name=="V41 / Buzón destination"' in B and
          "window.SetParent(vigilia.transform,false)" in B and
          "window.SetParent(canonicalParent,false)" in B,
          "Mailbox must move the existing screen, then restore the parent")
    check("RoyalGoldV41MailboxBackdrop" in S and
          "Mailbox / passive manuscript watermark" in S and
          "Approved source / mail-card" in S and
          "Approved source / tab-idle" in S and
          "independent metal nine-slice" in S,
          "All legacy mailbox painting must be disabled, not overlaid")
    check('Vigilia / one continuous charcoal stone folio' in S and
          'folio.raycastTarget=false' in S,
          "Empty mailbox must use one noninteractive premium stone background")
    check("originalTargets" in S and "originalEnabled" in S and
          "originalInkColors" in S and "Restore()" in S,
          "Visual replacement must restore controls and live ink")
    check("ActualMailboxEntry => mailUtilityButton" in HUD and
          "mailbox.ReceivedEventIds?.Count" in HUD and
          "mailbox.Messages?.Count" in HUD and
          "mailboxBridge.TryOpen()" in HUD and
          "utilityRail.gameObject.SetActive(show)" in HUD and
          "for(int i=1;i<utilityRail.childCount;i++)" in HUD,
          "Buzón must be an unlocked actual mail action, never a fake or stacked new control")
    check("BreachWatchMailboxBridge RealMailbox" in HUD and
          "mailboxBridge.Initialize(host,canvas)" in HUD,
          "Mailbox adapter must be available to native proof")
    nav = HUD.split("void Navigate(int index)",1)[1].split("void LateUpdate()",1)[0]
    check("mailboxBridge.TryOpen()" not in nav,
          "Do not expose mailbox before full native QA")
    check("new Button(" not in S and "new Button(" not in B,
          "Never duplicate real mailbox commands")
    print("VIGILIA_MAILBOX_STAGED_REPLACEMENT_SOURCE_PASS")
    print("UNITY_COMPILE_AND_ARTISTIC_APPROVAL_PENDING")


if __name__=="__main__":
    main()
