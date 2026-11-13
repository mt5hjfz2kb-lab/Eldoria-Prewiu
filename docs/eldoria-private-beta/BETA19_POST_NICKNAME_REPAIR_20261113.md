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

## Subsequent verified evidence and readability correction

Candidate 1a73e962d9fdc2a6c3177195f845860474cde95c finished native capture, terrain/campaign checks, fresh WebGL and packaging. Native screenshot artifact 14018772762 was independently SHA256 verified (2e94140903a16327c55ae7b3e85eb98bff67ac54cdcdb4765d21f958891f4862); six real narrative pointer touches pass. Actual WebKit gameplay experiment in run 43387563588, artifact 14019471483 (SHA256 d523a7f92a8b23af0e7d2de8fdef82b6acd507e93e2fb5d23c342b1227de3873), entered Entrada QA, advanced visible story controls, selected Provisions, clicked Construir and observed Aserradero NIVEL 1 plus the next objective. report.passed and constructionVerified are true, errors empty, around 28.76 FPS. Screenshots were independently reviewed. This proves the geometry repair permits gameplay, not a physical-iPhone pass.

The screenshots also reveal unreadable legacy sepia ink on the dark replacement surface. Do not publish that candidate as finished. Successor cf5a9f8c814e829b84664807e48f27a0068322f3 preserves the same geometry fix and reasserts existing Vigilia ivory body/muted speaker colors after canonical painting, restoring original colors on teardown. Native regression checks rendered narrative ink after the actual canonical layout. Exact new run 43387431116 is queued behind the existing job; no active Editor was cancelled. Its subsequent source, native, browser and full regression results remain pending.

Final browser gate 43388198558 additionally requires a real reload after saving the nickname before advancing story, and another after construction. It must establish returning-name and built-sawmill persistence. Superseded hosted diagnostic waits were cancelled; full Unity jobs were not. The public URL still serves fb8727d3ac45e31e61ab8ba244fa395477eff16e.

The stager now supports exact replacement of only the current Vigilia route from the latest full Pages snapshot. Five synthetic tests verify preservation of all unrelated routes and rejection of an unexpected previous source. This is file-preservation evidence, not gameplay acceptance. A draft publication workflow is not deployed and still requires the final exact hashes/certificates.
