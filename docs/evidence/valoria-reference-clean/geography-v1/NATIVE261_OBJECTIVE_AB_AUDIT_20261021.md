# Native261 — matched A/B pixel audit (21 October 2026)

**ART_REVIEW_REQUIRED / not integrated / not published.** Numerical image difference measures visibility, never aesthetic quality or ART_APPROVED. All samples are genuine Unity captures from [run 39641901228](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39641901228), artifact 12365323917. Canonical source-scene SHA-256: `4c39edeb8fa03f6b7235dc7bad0a041b06de9384b8e8bd6f1e3c23e8cf860237`.

For each matched before/after pair, compute the fraction of RGB pixels whose largest absolute channel difference exceeds 12/255 and 30/255. Values below are direct calculations from PNG pixel bytes (not quality scores).

| Unity camera | >12/255 | >30/255 | Channel MAE (0–255) |
|---|---:|---:|---:|
| Entire Region1 orthographic 2048×1152 | 7.26% | 1.06% | 2.87 |
| Normal world strategy 1600×900 | 17.16% | 4.68% | 7.32 |
| Forest POI 1600×900 | 30.01% | 14.12% | 13.54 |
| Quarry POI 1600×900 | 25.81% | 2.19% | 6.37 |
| Corrupt scout POI 1600×900 | 7.06% | 0.26% | 4.25 |
| Engendro POI 1600×900 | 7.00% | 0.24% | 4.17 |
| Valoria Bastion I wide 1600×900 | 8.16% | below 0.01% | 4.15 |
| Valoria Bastion II wide 1600×900 | 6.31% | below 0.01% | 3.93 |
| Valoria Bastion II portrait 900×1600 | 5.52% | below 0.01% | 3.07 |

Checks on extracted artifact: all 15 after-image SHA-256 values agree with `trial.json`; original six POI ID/position strings exactly match candidate; source-world ground 28,800 triangles in both. Four city camera stamp files are byte-identical before/after. **Legacy native261 stamp JSON is malformed due to unescaped matrix control characters** (although the raw before/after bytes match), so do not claim validated JSON metadata for that run. Current source subsequently fixed camera JSON serialization; the fix requires native262 proof.

**Independent visual judgement (not independent model acceptance):** native261 improves the close-up forest canopy, but full Region1 remains a flat grassy rectangle with isolated geology/corruption; city retains the model-on-grass impression. Thus do **not** promote this trial or claim target City 8/10 and World 7.5/10. The accepted next technical hypothesis is more connected macrogeography + genuine city height variation and correct grounded tree bases. Source for this remains separate and **untested** under geography request revision 262 because the UI V3 workstream holds `windows-runner-heavy`.

No Unity scene, player-camera, gameplay, UI, save, APK or itch file is changed by this review.
