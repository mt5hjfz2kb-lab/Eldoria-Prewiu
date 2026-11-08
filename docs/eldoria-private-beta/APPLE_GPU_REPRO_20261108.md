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
- This is desktop Apple GPU evidence, not physical iPhone verification. Pages promotion run 42494677413 publishes the exact checked candidate. Public entry check still pending.
- External testers remain blocked until physical stability, full gameplay and telemetry readiness are verified.

## Lesson
Desktop Chromium success cannot certify WebKit. Capture the GPU process report and distinguish the triggering operation from secondary JavaScript errors. Use exact same-payload A/B before changing unrelated assets or assuming OOM.
