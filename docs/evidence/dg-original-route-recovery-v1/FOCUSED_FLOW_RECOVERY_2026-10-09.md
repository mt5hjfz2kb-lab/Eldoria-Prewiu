# Focused Unity iteration recovery — 2026-10-09

Owner mandate: recover the workflow before Directorate/departments, keep the existing architecture and return production to game delivery. Resumed existing `eldoria-dg-original-route-recovery-v1`; no new department, coordinator, site or parallel pipeline.

## Verified defects and repairs

1. **False touch failure during Unity splash.** Failed candidate run 37958633271, QA artifact 11631956140, actual before screenshots in BOTH orientations are monochrome Made with Unity. Scene-loaded and the HTML loader disappeared before the game frame was rendered. The probe sent its gesture before visible Valoria; the subsequent mouse diagnostic happened later. This was not sufficient evidence of a product touch bug. The repaired probe requires real rendered coloured Valoria pixels after actual Unity scene telemetry before input, keeps the original touch response threshold and separate mouse evidence, and fails closed on absent/corrupt frames.
2. **Verifier change reached the wrong delivery path.** A QA-only push previously fell into legacy/stable Pages deployment despite no current-run Unity artifact. Existing Pages now distinguishes verifier-only changes, resolves a genuinely published, package-verified candidate from real run jobs, verifies its runtime/assets/project settings against current Unity source, and tests that exact existing binary. No Windows build, Pages publication, new JSON handoff or owner prompt is needed for this lane. Runtime/provenance drift blocks binary reuse.
3. **Incremental cache deleted before compilation.** Windows build job 113915750898 logs `git clean -ffdx` deleting Unity/Library including PackageCache. Merely removing CleanBuildCache from the builder was insufficient. Focused candidate checkout now retains ONLY generated Unity/Library, resets tracked source to exact github.sha and removes every other ignored/untracked residue, including old binaries. Release checkout remains clean. A real temporary Git regression confirms source restoration, stale source/output removal and Library retention. Warm Windows compilation speed has NOT been benchmarked; no speed claim is made for it.
4. **Duplicate historical candidate diagnostic.** M16 source/provenance checks remain automatic. Its additional fixed-6e0 historical playable job becomes explicit manual-only, with its required PNG dependency installed. Current independent actual-binary QA remains automatically chained in Pages. This prevents old-candidate diagnostics from reopening current-source work on each QA push. No canonical coordinator or architecture guard is disabled.

Implementation:
- rendered-frame prerequisite and existing-binary feedback: `adff3567102d23379af3d4ae65fd38b8ec84c8c9`
- strict focused checkout/cache and historical diagnostic isolation: `e2df4c12521a7fbc13697d478ec63b5ccc0b2006`

## Real independent execution

Both runs test the SAME actual Unity binary, build SHA `a66cc7b5931a4e59dd551e3388a5c6a12743a8aa`, not source-only or fixture substitutes.

| Run | Result | Evidence artifact | GitHub elapsed |
| --- | --- | --- | --- |
| 37967607802 | SUCCESS | 11634073412 | 4 min 14 sec |
| 37968189300 | SUCCESS | 11634805526 | 3 min 13 sec |

Both independently pass real CDP touch in landscape and portrait, forest survey (+20 wood) and harvest (+40 wood) through real game navigation, and exact reward persistence after browser reload. Second run additionally executes all 6 rendered-readiness and strict checkout contract tests.
Touch-only camera deltas: landscape 0.716, portrait 0.426; mouse diagnostic runs afterward and does not contribute to touch PASS. Actual before/after screenshots from first run were directly reviewed: Valoria is visible before the gesture and changes its framing afterward. No artistic-quality, subjective-enjoyment or physical-iPhone certification is implied.

Candidate:
https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/r2-candidates/r2-b-swipe-a66cc7b5931a4e59dd551e3388a5c6a12743a8aa/

Baseline 37958633271 took 29 min 38 sec including actual Unity compilation, staging and additive Pages publication before failing the incorrectly synchronized camera probe. The new runs took 4:14 and 3:13 WITHOUT recompilation/publication. These are different workloads; this proves eliminated unnecessary rebuild/delivery stages and reproducible focused feedback, NOT that every future Unity source correction takes 3 minutes. It is also not proof of two separate accepted player-facing features.

Second implementation SHA guards: repository architecture 37968189422, workflow governance 37968189396, quarantine 37968189361, M16 independent provenance 37968189318 — SUCCESS. M16 runtime historical job skipped deliberately; its independent source/provenance tests actually ran and passed.

## Acceptance boundaries and continuation

Support-only workflow recovery is accepted with real GitHub execution and playable-binary evidence. Existing policy in AGENTS.md already assigns direct bounded iteration to the owning specialist and places DG/M16 oversight outside sequential microfix approvals; this implementation makes the verifier-only path effective.

Keep the original stable site, frozen tester, runtime source, all other owners and paid-credit restrictions intact. The owner of R2B may consume this touch/reward/reload evidence, but EXP-BLD-02 selection and broader visual/experience acceptance remain separate existing gates. M07 and World1 are not closed by this work.

Not certified: warm-cache Windows build speed; autonomy of all fifteen departments; historical external owner-notification receipt. No chat is represented as an unattended background worker.

Operational continuation: owning department implements authorized source directly -> focused source check -> exact Unity candidate with real build when product bytes changed -> independent actual touch/adjacent regression/captures -> repair within same owner/scope -> accept/deliver. Verifier-only corrections reuse only a provenance-checked existing binary. Full fresh-save matrix remains for milestones/release/cross-system risk; stable promotion retains full gates.
