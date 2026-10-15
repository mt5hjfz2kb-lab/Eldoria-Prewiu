# Status — pilot reference-first (2026-10-15)

This is not a visual-quality approval.

## Evidence checked
- Real reference image and three previously captured OriginalHero Unity PNGs inspected.
- Native Unity attempt 38794504051: failed during unisolated Git checkout before Unity started.
- Native Unity attempt 38794999022: clean isolated checkout succeeded; C# compilation failed because code resolved `Application.isBatchMode` and `Application.dataPath` as `Eldoria.Application`.
- Native Unity attempt 38795936317 (SHA 5c4dbdcb1fde43559023abc3ac1c9a7bfa7e3d13): clean checkout succeeded, but its source had **`UnityEngine.UnityEngine.Application`** at both reported lines 53 and 178, causing `CS0234`; that SHA is superseded by the corrected code already in the live pilot branch, where the calls read exactly `UnityEngine.Application.isBatchMode` and `UnityEngine.Application.dataPath`. Do **not** rerun the obsolete SHA.
- Static Ubuntu preflight run 38794583466 was PASS for prior code. Later 38795936387 failure belongs to intermediate concurrent source `5c4dbd...` and cannot be treated as current. This status change intentionally triggers a fresh static gate for the present corrected branch.
- No new 1536x1024 PNG has passed Unity generation. Not ART_APPROVED. Not validated on target mobile device.

## Next safe step
1. Await fresh static Ubuntu gate on the newest exact SHA (not a replacement for Unity).
2. Coordinate exclusive Windows runner slot or confirm the owner has yielded it.
3. Execute real Unity **once** on the current corrected SHA with an isolated checkout; preserve full logs and screenshot bytes.
4. Evaluate the image with approved reference; evaluate bastion and gate family geometries before making new assets.
5. Keep every existing production branch, pause, and asset approval requirement intact.
