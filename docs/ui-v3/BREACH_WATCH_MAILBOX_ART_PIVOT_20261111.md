# Mailbox: change of production technique

Source candidate only. ART_APPROVED=false. No main integration or publication.

The exact Unity run 43089450921 (source72d132cf789738278ca17d1580e472b29a0eadd8) was inspected at 390x844. It still shows a flat beige polygon envelope and visibly triangular feather vanes. Earlier source and native passes did not prove the requested tactile art quality. More geometric edging would repeat the failed technique.

Replace the procedural drawing with one genuinely authored transparent raster illustration of parchment, burgundy crown wax seal, engraved aged bronze and natural raven quill. Style reference: the approved navigation source reference-80.webp.b64, SHA256dff461d5e4c7d3de7ec6ccb641f7e372aee4f324310ca0b25ea630dfcb157437. This reference establishes materials, not a new approved mailbox design.

Generated source PNG SHA256: 2fc1e38d18e68ab9f144f2742594ac4c1ca2fb6011659a3fbd78ddd26d84c1f1,1536x1024 RGBA,2628545bytes. Importer max512, unreadable, no mipmaps, alpha transparency, no new Canvas/raycast/event handlers. Keep the complete aspect and margins. Actual Unity GPU/residency cost remains to measure; the source PNG size is not runtime memory.

The authoritative empty-label routing and category handling stay in BreachWatchMailboxTheme. The illustration disappears on populated categories; short landscape keeps the existing compact empty message. Missing sprite leaves the honest message visible rather than showing the rejected geometric substitute.

Pending: native exact-head Unity compilation and four-resolution empty/populated/rotation capture; compare against both the original approved material reference and prior exact native. Check clipping, alpha haze, readability, no interception, save unchanged and missing-resource behavior. Only request Windows after PR179/181/187 rotation. Present actual native A/B to owner BEFORE any merge into main. No global artistic score asserted.

## Verified cross-orientation diagnosis, 12 November 2026

Real Unity source `af658a6cc1a02d45d7efc9c57dafef24e8512564`,
run [43142184165](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43142184165),
native artifact 13910066930: 48 scene/route/profile/mail captures, native and
visual-AB jobs succeeded, exact manifests include old Canvas off and disposable
save preserved. Portrait 390x844 shows the full real parchment, wax and quill.
However `real-unity-mail-1600x900.png` SHA256
`a2d4c34f525fc4dc3c03334534ec08166714b55111e45b535185d8f6f4359570`
still shows a broad dark, featureless folio and an isolated empty sentence near
the upper third. This is a **new cross-orientation composition defect**, not
evidence that the portrait original art or asset importer failed.

**Causal trace:** `BreachWatchMailboxEmptyArtwork.OnPopulateMesh` explicitly
returned on any `r.width > r.height`. Thus the original imported 1536x1024
raster was not drawn at 1600x900 even though the screen has enough genuine
space below the real tabs. Existing `BreachWatchMailboxTheme` only moved the
canonical empty sentence in portrait. Repeated changes to the stone matte or
button borders would not fix either condition. Do not return to the rejected
polygonal envelope or introduce a fake full-screen painting.

**Change of composition (source commit `80c605ae8b3c23fc55a1c4d5c3a67fb4c8b4f2fb`):**
reuse the unchanged approved-source `sealed_letter` texture, without texture
copy, RenderTexture or a second Canvas. A dedicated spacious-landscape layout
(`width>=1100, height>=650`) reserves a true-size, original-aspect painting
below tabs; places the existing category-specific empty sentence below the
painted seal; restores the original RectTransform anchors, pivot, dimensions
and position on exit or orientation change. Compact 844x390 landscape
deliberately omits the painting and keeps the genuine message legible.
Populated mail, live read/unread flags, reports, receipts, button commands,
save state and original parchment/ink policy remain untouched.

**Independent gates:** source guard must pass, then the **next legitimately
leased** Unity batch must capture 900x1600, 390x844, 1600x900 and 844x390
on exact new HEAD. It must prove actual framebuffer pixels contributed by
the texture at 1600x900 (>1% of image, upper-half contribution), check caption
clearance and compact orientation by visual review, check source-bound report
and populated categories, old Canvas off, raycasts/clicks and save unmodified.
Any new build/art status belongs to that **new exact HEAD only**; the old
48-capture success does not certify the revision. Continue real original-owner
reference comparison and A/B approval before main. No owner artistic approval,
WebGL/public release or physical-device validation is inferred.
