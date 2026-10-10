# Native mobile playtesting

Android preparation is authorized by the owner on 2026-10-10. This is an additional native delivery target for the existing Unity game, not a replacement game or renderer. WebGL remains the currently certified playable delivery until a native build passes its own acceptance.

## Build

`.github/workflows/android-playtest.yml` consumes `pipeline/android-playtest-request.json`. The request is disabled while Region 1 owns the Windows runner. Enabling requires the Android workstream to hold the exact runner resource with no other heavy owner. The preflight fails closed on contention and validates the production artifact identity. Workflow edits alone do not launch Windows builds.

The APK uses Unity 6000.3.23f1, the original certified Valoria production scene, current Frontier source, four embedded SHARP parcel assets, ARM64/IL2CPP and Vulkan. The builder under `tools/android-playtest/` is staged into the CI Editor folder only, avoiding source edits that would invalidate an in-flight WebGL candidate. It deliberately does not use the browser's frame-only scene. Android SDK/NDK/OpenJDK and Android Build Support must already be installed; missing support is reported before Unity builds. No purchase or store release is involved. This diagnostic build uses a debug signature, not a production signing key.

The production scene artifact currently comes from the existing publish request. Its large SHARP assets and Vulkan renderer are unverified on Android: build size, shader compatibility, peak memory and actual frame times must be measured before adopting Android as the main certification target.

## Acceptance

An APK compilation is BUILD PASS only. Record SHA-256, source SHA, production artifact ID, exact device/GPU/API, logs and landscape/portrait captures. Test cold launch, real touch pan in four directions, HOME, building selection, construction, Mundo/resources/rewards, Bastion I to II, recruitment/combat, process kill/relaunch persistence and deliberate reset. Inspect visuals, safe areas and measured performance on a real compatible device. Do not label native production PASS from WebGL probes or editor captures.

The user's iPhone cannot install an Android APK. iOS requires its own Unity/Xcode build and signing/device provisioning. A Windows Android runner does not provide an iOS delivery. No iOS build capability or signing access has been verified in this workstream.

Pending native acceptance does not invalidate existing WebGL releases and does not remove the active Region 1 workstream's release gates. Development should batch WebGL publications at coherent review milestones; Android becomes the principal mobile QA route only after native acceptance.
