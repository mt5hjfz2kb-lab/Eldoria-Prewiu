# PR187: opt-in resource bag UI (not art-approved)

The existing Mailbox HUD owner now initializes one opt-in BAG overlay on
the same Canvas and gateway. No new Canvas/EventSystem or separate state
store; the bridge queries persisted `PlayerState` on every refresh.
Packets held in mail are absent from bag until explicit Claim.
Tapping a pictured packet sends `MarkResourceBagSeen` without consuming
or crediting anything. USAR sends `UseResourceRewardPacket` with a fresh
revision and displays the genuine command result. Only that successful
saved transaction alters the wallet and completes first-use guidance.
Never consume because of a novelty badge.

The 12 authored source packet PNGs are used for the visible resource
rows, with live EXACT quantities inside the painted plaque, including
nonstandard amounts. Unread and actionable mail use distinct real PNG
badges (NEW vs CLAIMABLE). First-use hand/halo are bound only to the
actual, topmost USAR Button via the existing EventSystem all-canvases
raycast guard. Bag is only advertised after collected unused packets
exist. The established Royal Gold frame and serif assets are reused
in slate, dark wine and old gold, without new generated graphics.

This is an unmerged **candidate**, not Unity QA or owner approval. Next
legitimate Windows turn must compile the actual I-V scene and render
390x844, 844x390, 900x1600 and 1600x900. Inspect layout, usability,
occlusion, saved consumption and the two attention states. Compare
the actual Unity result against owner-approved art before proposing
any merge/main activation. Current native artifact13903005932 proved
import16PASS but failed disposable Editor EventSystem lifecycle before
producing screenshots.
