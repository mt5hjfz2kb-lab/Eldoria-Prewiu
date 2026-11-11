#!/usr/bin/env python3
"""Fail-closed source/asset gate for the STAGED (not yet native-certified) mailbox.
Source checks are not Unity screenshots and never mean artistic sign-off.
"""
from pathlib import Path

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
    seal=R/"BreachWatchEmptyMailSeal.cs"
    check(seal.is_file() and seal.with_suffix(".cs.meta").is_file() and
          "MaskableGraphic" in seal.read_text() and "OnPopulateMesh" in seal.read_text(),
          "Native empty mail engraving must be a crisp reversible vector drawing")
    check("BreachWatchEmptyMailSeal.Install" in S and
          'text.name=="Mailbox / genuine empty state"' in S and
          "realEmptySentence.gameObject.activeInHierarchy" in S and
          "realListSummary.enabled=originalSummaryVisible&&!actuallyEmpty" in S,
          "Decorative wax seal must only follow the canonical empty-state label")
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
