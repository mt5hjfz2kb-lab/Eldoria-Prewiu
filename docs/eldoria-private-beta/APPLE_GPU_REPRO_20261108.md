# Apple GPU investigation, 2026-11-08

## Physical device boundary
Mobile5 fails on the owner's iPhone after nickname appears. The supplied 15:11 Jetsam file predates the 15:32 failure and does not report a WebContent termination. It does not establish the game's cause. Do not publish that private device log in this repository.
APK reportedly works. Native Android success is not browser certification.
Physical iPhone cause and the fix below still require direct device validation.

## Reproduced failure
Exact published mobile5 on macOS 15 arm64, Apple GPU, Playwright WebKit 26.0:
- Run 42492680390: loses WebGL context during startup; shader creation subsequently receives null.
- Run 42492934981: repeats with no extra probe context, ruling out that probe as trigger.
- Run 42493152725: unchanged and ASTC-disabled both fail.
- Artifacts 13622014347 (unchanged) and 13622626633 (ASTC disabled) include GPU crash reports.
- Both GPU reports: EXC_BAD_ACCESS / SIGSEGV, invalid-address access in _platform_memmove -> doCompressedBlit -> ANGLE/Metal Texture::replaceRegion -> setCompressedSubImage -> compressedTexSubImage2D.
This is an observed compressed-texture GPU-process crash, not an observed jetsam memory-limit termination.
The later shader/uniform-binding errors are downstream of context loss. Hiding those errors is not a fix.

## Controlled contrast
Run 42493395837, source eb62c6de58dab7f25125fbbc8a7a08393d7803db:
- Same game, same WebKit 26, S3TC capability disabled: exact world initialization plus 90 seconds PASSED, Apple GPU screenshot obtained (artifact 13622716463).
- Disabling ASTC alone had failed, so do not disable all compression.
- Current Playwright 1.65.0 / WebKit 27.2 also loses context with original payload; its API returns non-null shader objects after context loss, so it fails world initialization instead of throwing the same null-shader exception (artifact 13621669125).
- Chromium control uses SwiftShader on hosted macOS; it completed world initialization and observation without JS errors, but screenshot timed out. It is NOT a complete QA pass.
S3TC fallback is an isolation test only: disabling all S3TC could expand many 2D textures unnecessarily.

## Narrow candidate
Exactly five Cubemap objects, sharedassets0.assets IDs 446-450, ReflectionProbe-0 through ReflectionProbe-4:
- Original: DXT5, 128x128, six faces, eight mips.
- Replacement: decode every existing face/mip to RGBA32, no resampling, mip regeneration, shader/material/gameplay changes.
- All 240 face/mip pixel payloads independently checked after serialization.
- 57375 total object hashes checked; only five changed. All other objects identical.
- Additional serialized bytes: 1965240.
- Parent bundle SHA256: 2c502a1d766aca83c2377347de290467d16d75efd8c6f5edad41766f4d3ce73a.
- Candidate bundle SHA256: e54d547487e0d915f5a8c3fbbd0354a79672041dd201b3e63672de5e740ea6c1.
- Candidate revision: native325-mobile6-cubemap.
- Candidate build/QA run: 42493894120, source 5842448fe1ec024eff1d368d003d5da3fd70adc6.
- Candidate ZIP artifact 13623230011, SHA256 417f15c01f3c65f81a61e7fd88b3bd52a91b7c07eba5bed37b7b9192294047ee.
- Runtime PASSED on both WebKit 26.0 and 27.2, native Apple GPU, no extension overrides. Artifacts 13622663288 and 13622517677. World initialization + 90 seconds, nickname entry + 30 seconds, graceful Unity Quit then actual page reload + 45 seconds; no runtime/context-loss errors. Both reloaded screenshots manually inspected: Beta QA persisted and normal chapter intro visible.
- This is desktop Apple GPU evidence, not physical iPhone verification. Pages promotion run 42494677413 publishes the exact checked candidate. Public entry check PASSED: run 42494930537, exact source 066b375babe2dbb58ac04c0f9916263c913e5b3e; artifact 13623376058, ZIP SHA256 0c9f7dad64554bb176d4b18c9c03096dd8a509eed33b692dc13810759003ccc7. Actual public WebKit26 Apple GPU initialization + 90 seconds, nickname + 30 seconds, graceful Quit then actual reload +45 seconds; zero runtime errors. Reloaded screenshot manually inspected: Beta QA and normal intro visible. Diagnostic revision native325-mobile6-cubemap-d4 / visible beta 6.
- Sites source 0a1549ff07cac486f56c554fc77b470c589db4fc, saved version 5 (game candidate beta 6), deployment appgdep_6af094df6f9881918253a11bc83d7bb6. Existing Pages owner site preserved (21 files), combined site 959605376 bytes. Live first and last decoded chunk hashes verified. Local direct Sites HTTP fetch was restricted (403); the public hosted WebKit navigation and screenshots verify the actual entry instead.
- Owner URL: https://eldoria-valoria-beta-20261108.kbkvc5c45n.chatgpt.site/?eldoria_diag=1&build=mobile6-cubemap . Physical iPhone remains UNVERIFIED, not resolved by desktop evidence alone.
- External testers remain blocked until physical stability, full gameplay and telemetry readiness are verified.

## Lesson
Desktop Chromium success cannot certify WebKit. Capture the GPU process report and distinguish the triggering operation from secondary JavaScript errors. Use exact same-payload A/B before changing unrelated assets or assuming OOM.

## Owner physical result — mobile6 FAILED
Owner reports: «Hace lo mismo que la 5». Desktop compressed-cubemap fix is confirmed only in desktop WebKit; it did not resolve the physical iPhone restart. No new device exception or lifecycle log accompanies this result. Do not claim the phone root cause identified, and do not request another attempt at the same build as a fix.

### Reinspection after physical mobile6 failure
Published QA report42494930537: Unity usedWASMHeapSize rises from306012928bytes at ready to945430712bytes after world initialization; WASM capacity1052049408bytes. Desktop WebContent sampled peak RSS2119824KiB, GPU517632KiB; separate process peaks, not a simultaneous physical footprint or phone measurement. The data file is501036611 decodedbytes. High memory remains a concrete investigation target, not a proven iPhone termination cause. Existing serialized asset audit lists361295804bytes of Mesh objects, many readable. Do not disable readability blindly: inspect script access, collider and batching requirements and measure runtime A/B. Unity6000.3 Mesh.isReadable documentation confirms CPU copies may be retained in addition to GPU buffers. Preserve mobile6 desktop GPU fix; no new phone candidate published from this hypothesis alone.

### IMG_5052.jpeg — physical beta6 evidence
Visible label confirms beta6. Ready at16:57:53 Europe/Madrid, heap capacity412MiB, ASTC supported. At16:57:55 alive2seconds then grow-request-mib495 is last visible line. No matching grow-ok, thrown error or subsequent navigation is visible in this screenshot; do not infer that allocation actually failed or that the visible excerpt is the complete journal. Public desktop QA at the same startup transition: ready used291.8MiB/capacity412.2MiB; +9.5s used327.1MiB/capacity494.8MiB; +11.1s used524.0MiB/capacity593.8MiB. This demonstrates substantial post-ready loading and that capacity is not equivalent to occupied memory. Owner report establishes physical failure; crash mechanism remains unconfirmed. Native APK success is a distinct platform result.
