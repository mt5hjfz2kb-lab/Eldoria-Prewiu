# Real mailbox pointer lifecycle correction

Actual combined native43158384727 reached the visible CORREO control at screen28.67,659.72 with live graphic depth23 but EventSystem returned no hit, even after a normal player frame. Five actual bag controls passed and all245 overlay inputs restored exactly.

Primary source explains a deferred lifecycle defect: BuildIndependentComponents creates utilityRail inactive before BuildButton configures its BreachWatchPlate raycastTarget=true. First activation then executes BreachWatchPlate.Awake, which unconditionally reset raycastTarget=false. Waiting cannot repair that production state. This diagnosis is based on actual failure plus source order; the correction still needs native verification.

BreachWatchPlate now preserves explicit pointer intent through Awake. Only the actual owning BuildButton opts in; decorative plates remain noninteractive. Native destination QA replaces the former direct mailbox and message-row onClick.Invoke calls with the first actual EventSystem hit, matching normal click-handler routing. It also asserts targetGraphic.raycastTarget after activation. Original source artwork and layout stay intact, including the concurrent real Hall glyph-overlap correction c6acdef.

Pending: native48images and original A/B, two actual mailbox pointer hits, then PR187 full52click bag/mail/save-reload proof. Neither source success nor this document approves art or physical devices.
