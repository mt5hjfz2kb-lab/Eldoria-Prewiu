# Eldoria — native156 independent art decision (19 October 2026)

**Status: ART_REJECTED / ORIGINAL_NATIVE152_SCENE_RETAINED / NO_GAMEPLAY_REGRESSION.**

The Windows Unity native156 execution on source `865bd48d29e8df090468dce1c9f3fde140f5038b` (run [39314778625](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39314778625)) produced authentic A/B captures, shared I–II operation proof and an  `integration-result.json` marked `INTEGRATION_PASS_ART_REVIEW_REQUIRED`. The final direct push to `main` was rejected as non-fast-forward because the external delivery session changed unrelated files while Unity was rendering. **This does not prove Unity or gameplay failed.** The exact output stayed in [artifact 12215949809](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39314778625/artifacts/12215949809).

Only the needed real-image previews and proof were recovered from that exact artifact on [GitHub run 39316270805](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39316270805), without recompiling or rerunning Unity:

- [Wide city before](native156-recovery/environment-city-wide-survivors-before.jpg)
- [Wide city after](native156-recovery/environment-city-wide-survivors-after.jpg)
- [Sawmill after](native156-recovery/environment-sawmill-survivors-after.jpg)
- [Barracks after](native156-recovery/environment-barracks-survivors-after.jpg)
- [Portrait after](native156-recovery/environment-city-portrait-survivors-after.jpg)
- [Native technical report](native156-recovery/native156-unity-report.json) and [I–II game operation report](native156-recovery/native156-integration-result.json)

**Objective evidence:** two surviving background structures, 99 retained mesh renderers from the existing Slavic Medieval Village Free prefab kit; zero additional colliders; terrain heights, plot references, hotspot positions and lighting unchanged; 32 gameplay/narrative progression and save-related checks in retained I–II integration report. No physical iPhone/Android profiling.

**Independent visual verdict: ART REJECTED.** Both architectural volumes sit too far beyond the functional yards to create a convincing rebuilding settlement. One house has tall exposed supports and appears suspended or badly founded on the lawn. The two timber/sand-roof stylistic accents lack integration with the slate-roof stone central castle; the normal and portrait frames still resemble a lone keep amid prairie. Barracks detailed view is essentially unchanged. Thus copying 99 renderers is not a meaningful graphical advance.

**Decision:** do not promote this candidate. The game's integrated scene remains the byte-identical native152 baseline Git blob `49a42927468fec02e950c040fa47ed7302814695`. Disable the v9 call in `ValoriaIntegrationBuild.VisualLeap`; retain diagnostic source, captures and historic failures. Do not repeat scattered ruined walls, almost invisible terrain paint or distant prefab houses. Next proper technique must establish functional grounded architecture + urban hierarchy at player camera scale, with before/after and protected gameplay checks.

No Sites private update, WebGL rebuild, APK update, Bastion III or paid credits was conducted under this trial. The native152 artifact remains ready for controlled deployment.
