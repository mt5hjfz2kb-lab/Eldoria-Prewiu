# Native325 mobile texture candidate

Current status: published successfully by run42418899866 after independent runtime/visual run42418416629. Both versions reach Unity ready and show the normal nickname welcome. Physical iPhone and full campaign remain unverified. Public link: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/beta-native325/mobile-v1.html

The first run reached Unity ready but its Playwright screenshots timed out. Direct compositor capture at device scale0.5 resolved the capture failure; this lower-resolution evidence is not an art-quality certification. The current artifact regenerates decoded_files correctly, verified after publication. QA telemetry backend accepts one consented diagnostic event and rejects absent consent; the frontend and native events remain unconnected.

The original frozen archive remains unchanged. This is a repackaged texture candidate, not a Unity rebuild or certified mobile fix.

## Verified locally

- 54,338 objects reopened and verified; all untouched object bytes match the original.
- 58 color textures re-encoded to ASTC 4x4. Material-referenced data textures and normal maps excluded.
- Every original mip is preserved and compared independently; minimum PSNR 40 dB for first three mips and 34 dB later, alpha mean absolute error <=0.6. Failed candidates retain original bytes.
- All texture metadata except format, payload and payload size is preserved.
- Serialized texture savings: 314,268,436 bytes. This is not a measured resident-memory reduction.
- Bundle SHA256: d169fee32bca1b2e26aae3693a6bd53bab683d11a8a5c8103b4aedff28c7eeb7.
- Public distribution readiness gate checked with a passing synthetic fixture plus five negative assertions: missing identity, backend delivery, physical iPhone, full normal-clock campaign, or actual Unity events remains blocked.

## Runtime gate

GitHub Actions run 42417656424 independently reproduces the candidate and attempts original/candidate Unity startup in separate Chromium contexts. Actual screenshots must be reviewed. A varied screenshot alone is insufficient proof of playable quality.

Do not publish this candidate unless the engine starts, the game renders correctly, decoded transport checks pass and the combined Pages payload stays under 1 GiB. Before promotion, regenerate release-identity decoded_files from the candidate manifest (the initial candidate artifact inherited the original identity list; payload checks use the correct manifest).

## Remaining certification

Physical iPhone startup and normal gameplay are still unverified. Do not label desktop emulation as an iPhone test. The observed involuntary return at startup90 is not conclusive evidence of an OS memory kill.

Actual gameplay metrics are not connected. Defined indicators must not be described as collected. Preserve normal tutorial, normal clock and existing saves. Do not send new tester invites until these gates are satisfied.

## Rollback

Original frozen native325 ZIP SHA256 fb1292b3d9ce4d6096d69fa18b417bc0a8cc6016f100defb3deaf29fbdb95771 remains available in the frozen draft release and source Actions artifact. Re-run the original Pages preparation workflow pinned to 08fb51bb492bb81f89e42c1e59de1ff1ada40287 to restore transport-only native325; preserve /unity-owner/.
