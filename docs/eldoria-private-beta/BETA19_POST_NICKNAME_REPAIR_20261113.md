# Beta19: post-nickname input block — repair in progress

Updated 2026-11-13. This is not a release or physical-device certificate.

## Observed failure
Public source fb8727d3ac45e31e61ab8ba244fa395477eff16e saves the nickname but does not respond to repeated visible sawmill-label clicks. Hosted WebKit diagnostic 43382127078 recorded twelve attempts, unchanged city screenshots, no page exceptions, and continued rendering (25.89 FPS). Its successful workflow conclusion means evidence was collected, not that gameplay passed. Artifact 14016669285 was downloaded and independently SHA256-verified: 32e392b7e71371541edd60ee1dbcfea8ca08d47e4a6d591acf3af587774ead04.

## Candidate and causal hypothesis
Candidate 1a73e962d9fdc2a6c3177195f845860474cde95c remains on feature/eldoria-private-beta-i-v-20261107. The canonical narrative frame was being positioned using the retired Canvas's world coordinates after reparenting into the independent Vigilia Canvas. The repair fits and anchors that same modal against its actual safe-area parent on every render. It preserves narrative callbacks, gameplay and saves.

Native regression now checks every Continue-button corner is on screen and that the first EventSystem raycast resolves to the real Continue button, then delivers pointer events. Earlier onClick-only tests could pass with an offscreen modal.

Run 43382792518 has passed source contracts, live ownership preflight, real city/first-entry capture, destination screens, glyph verification, and treasure/bag checks. Terrain/campaign/WebGL phase is still running at this checkpoint; no completed WebGL or public fix is claimed.

## Browser gate and next evidence
The main-only browser workflow waits for the exact candidate, dispatches its full regression certificate, and uses actual WebKit mouse/keyboard input plus screenshot OCR. Current gate source 0b38f69a4b95699f7b07d5b6979f9e977e98af17 selects the reviewed Provisions medallion, whose real navigation opens the sawmill, clicks visible Build, and requires the actual sawmill panel at level 1. The objective tablet only expands text and is not a navigation shortcut.

Pending: inspect native screenshots; finish fresh WebGL and package; inspect actual construction proof; full regression; reload proof; merge repair; publish exact corrected payload; repeat public-route verification. Physical iPhone acceptance remains separate and pending. Beta18 is unchanged. Beta19 is not ready for testers while this blocker remains.

## Lessons
A rendering heartbeat, nickname persistence, or green screenshot capture does not establish playable entry. Tests must exercise visible hit targets and require an observable gameplay result. Do not bypass invisible controls with direct callbacks or interpret a diagnostic collection job as acceptance.
