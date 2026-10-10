# Native mobile playtesting

Android preparation is authorized by the owner on 2026-10-10. This is an additional native delivery target for the existing Unity game, not a replacement game or renderer. WebGL remains the currently certified playable delivery until a native build passes its own acceptance.

## Build

`.github/workflows/android-playtest.yml` consumes `pipeline/android-playtest-request.json`. The request is disabled while Region 1 owns the Windows runner. Enabling requires the Android workstream to hold the exact runner resource with no other heavy owner. The preflight fails closed on contention and validates the production artifact identity. Workflow edits alone do not launch Windows builds.

The APK uses Unity 6000.3.23f1, the original certified Valoria production scene, current Frontier source, four embedded SHARP parcel assets, ARM64/IL2CPP and Vulkan. The builder under `tools/android-playtest/` is staged into the CI Editor folder only, avoiding source edits that would invalidate an in-flight WebGL candidate. Hotspot script identities are checked through a temporary-scene serialization roundtrip. It deliberately does not use the browser's frame-only scene. After obtaining the exclusive lease, missing Android support/dependencies can be installed using the existing Unity CLI (`install-modules -e 6000.3.23f1 -m android --cm`). Installation refuses to proceed while Unity is running and verifies all module paths afterwards. No purchase or store release is involved. This diagnostic build uses a debug signature, not a production signing key.

## Verified preparation and current dependency

Read-only toolchain run **38058101010**, job **114230713698**, artifact **11672875414** completed successfully. Android support, SDK, NDK and OpenJDK are all absent at the current Editor's standard module paths. No authorized Android device was detected through the bundled adb check; adb itself is absent, so this is not proof that no phone could be connected through another installation. Unity was not started and credits spent were zero. Architecture, workflow governance and quarantine checks passed for preparation commit `31912e1638b8fa63fffe065796203b3c5b52d750`.

The active `world-region-1-visual-convergence-v3-20261010` owner still holds the Windows Unity lease. Its source/editor/desktop run **38057319052** completed successfully, but job completion does not release another owner's reservation. Do not appropriate that lease. No Android compilation, installed app, native visual approval or platform migration is claimed. The C# builder and module installer have not executed yet.

Resume from live main after the claimant releases or legitimately transfers the Windows lease: reclaim the same Android workstream; update/verify the production artifact against live publication state; enable the request; install missing support; build; diagnose/retest; then inspect the native app using an available Android device or suitable emulator. Native test-device/emulator availability remains unverified.

Official installation reference: https://docs.unity.com/en-us/unity-cli/unity-cli-reference .

The production scene artifact currently comes from the existing publish request. Its large SHARP assets and Vulkan renderer are unverified on Android: build size, shader compatibility, peak memory and actual frame times must be measured before adopting Android as the main certification target.

## Acceptance

An APK compilation is BUILD PASS only. Record SHA-256, source SHA, production artifact ID, exact device/GPU/API, logs and landscape/portrait captures. Test cold launch, real touch pan in four directions, HOME, building selection, construction, Mundo/resources/rewards, Bastion I to II, recruitment/combat, process kill/relaunch persistence and deliberate reset. Inspect visuals, safe areas and measured performance on a real compatible device. Do not label native production PASS from WebGL probes or editor captures.

The user's iPhone cannot install an Android APK. iOS requires its own Unity/Xcode build and signing/device provisioning. A Windows Android runner does not provide an iOS delivery. No iOS build capability or signing access has been verified in this workstream.

Pending native acceptance does not invalidate existing WebGL releases and does not remove the active Region 1 workstream's release gates. Development should batch WebGL publications at coherent review milestones; Android becomes the principal mobile QA route only after native acceptance.
