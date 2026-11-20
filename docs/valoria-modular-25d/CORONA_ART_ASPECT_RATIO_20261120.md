# Corona — deformations caused by scaling full-color original UI artwork

The game uses actual independent premium RGBA files. Their measured pixel dimensions are **580×260** for `identity_crest` and `resource_siglet` (2.231:1) and **590×260** for `objective_rune` (2.269:1). Previous gameplay sizes flattened these to 194×68 (2.85), 117×43 (2.72) and 190×60 (3.17); this is not commercial-quality rendering, even though every asset was detailed.

The Corona Unity UI now uses **182×82**, **117×52** (3 separate chips), **190×84** in portrait and **180×79** in landscape, respectively, retaining original silhouettes. Resource row is 23pt further below the crest to avoid an overlap. Its dimensions still fit 390pt wide. No game rules, buttons, buildings, camera or save were changed. The NUnit gate compares actual Sprite.pixel aspect with its instantiated RectTransform and rejects over 12% distortion. Browser pixel ROI is moved to reflect the reflow and must be validated with fresh exact-SHA images.

Caveat: `action_arcane` and in-world nameplates have separate unusually wide gameplay footprints. Their artwork needs a dedicated A/B aspect-safe source treatment or proportion-aware UI layout, not a return to the previously broken 9-slice paint-gem distortion. Overall visual approval pending real Unity/Chromium captures.
