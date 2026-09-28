# Eldoria — Development Pipeline v2

Status: active optimization plan.  
Updated: 2026-09-28.

## Objective

Reduce elapsed development time without weakening certification. The pipeline should automatically carry work to the last point where human judgment is actually required.

Human gates remain mandatory for:
- paid/credit-spending actions;
- visual/art-direction acceptance;
- irreversible city/topology decisions.

Everything else should be machine-checkable and chainable.

## Current measured baseline

Recent real runs on the Windows self-hosted runner show:
- Tripo Studio bridge staging: about 44 seconds end-to-end.
- Canonical Tripo -> Blender -> isolated Unity module gate: about 5–7 minutes.
- Full Unity editor/build/capture gate: about 9.5 minutes in a representative run.
- In that full run, the interval from EditMode start to PlayMode start was about 5m16s, dominated by Unity startup/import/compile work.

These are operational observations, not performance guarantees. Keep measuring after each optimization.

## Pipeline profiles

### FAST
Use while iterating on one bounded surface.
- exact targeted tests;
- no historical art experiments;
- no desktop player build unless required;
- no web deploy;
- only required captures.

### PRODUCTION
Use after a coherent block is ready for owner review.
- EditMode + relevant PlayMode;
- official Valoria captures when presentation changed;
- interaction regressions;
- production asset checks;
- no unrelated historical experiments.

### RELEASE
Use only for milestone/release confidence.
- full Unity test/build;
- full web integral gate when web changed;
- published verification;
- frozen tester guard;
- broad visual/UX review.

The important rule is that RELEASE remains strict; speed comes from not running RELEASE for every intermediate commit.

## Change-aware routing

A change should trigger only the systems it can affect.

| Changed surface | Required default gates |
| --- | --- |
| docs only | consistency/source checks only |
| pipeline request only | relevant pipeline only |
| Tripo/Blender tooling | module pipeline + isolated Unity gate |
| production Valoria art/material | targeted Unity tests + official Valoria captures |
| Unity domain/application rules | EditMode + relevant PlayMode |
| Unity presentation/camera | PlayMode + official captures |
| web runtime/QA/build tooling | web focused/segment/integral according to risk |
| release metadata | release-specific checks only |

Web Pages must not run for Unity-only, Tripo-only, Blender-only or documentation-only commits.

## Persistent Unity import-cache experiment

Do not use `actions/checkout clean:false` as the first optimization because the self-hosted runner also receives generated/untracked test assets. Preserving the entire workspace could contaminate certification.

Preferred experiment:
1. keep a clean Git checkout;
2. keep Unity import cache outside the workspace;
3. key it by Unity editor version + Packages manifest/lock + relevant ProjectSettings;
4. restore only `Unity/Library` data into an isolated test workspace;
5. compare cold vs warm EditMode startup;
6. verify identical test/build/capture results;
7. promote only if deterministic.

Unity Accelerator is the second option, especially if more runners/dev machines appear. For one persistent runner, prove whether local persistent Library reuse is simpler/faster first.

## Asset production orchestrator target

Canonical state machine:

`approved reference -> exact-input identity -> pre-spend stage -> OWNER CREDIT APPROVAL -> generation -> task watch -> export -> SHA identity -> Blender normalization/refine -> isolated Unity gate -> visual evidence -> OWNER VISUAL APPROVAL -> promotion -> integrated Unity regression -> certification`

Rules:
- no new paid generation after the approved spend without a new owner gate;
- automatic retries are allowed only for zero-credit infrastructure failures;
- never regenerate geometry to fix a material/lighting-only defect;
- every state writes structured evidence.

## Asset manifest

Each production asset should converge on one machine-readable record under `pipeline/assets/<asset>.json` containing:
- asset ID/function;
- approved reference SHA/size/dimensions;
- authorized credit amount;
- generation task ID/model/version;
- raw GLB SHA/bytes/tris;
- optimized GLB SHA/tris;
- texture/material identity;
- dimensions/orientation;
- parcel/camera/interface constraints;
- TECH / INTERACTION / VISUAL results;
- promotion destination;
- source commit, workflow run and artifact IDs.

This is the durable source for automation. Human docs summarize decisions; they should not be required to reconstruct binary identity.

## Development telemetry

For every significant workflow, collect checkout/setup, dependency install, Blender, Unity import/startup, EditMode, PlayMode, build, capture and artifact-upload elapsed times. Review median and p90 after enough samples. Optimize measured bottlenecks, not assumptions.

## Do not optimize yet

Do not introduce broad matrices or multiple parallel Unity jobs on the single Windows runner until queue time proves a second runner is justified. Splitting one machine into more jobs can increase elapsed time.

Do not add Addressables solely as a speed optimization. Adopt it when content scale/loading/update requirements justify it.

## Promotion criteria for an optimization

An optimization becomes canonical only if it reduces measured elapsed time or manual intervention, does not weaken a gate, does not make source identity ambiguous, survives at least one clean/cold run and one warm/repeated run, and has a simple rollback.

## Visual diagnostic warm-workspace rule

The single Windows Unity runner now preserves `Unity/Library` for the lightweight visual diagnostic lanes:
- `valoria-lookdev.yml`;
- `valoria-visual-formula.yml`;
- `historical-surface-rescue.yml`.

Those workflows use `actions/checkout` with `clean: false`, while each tool deletes/recreates only its own capture/output folders. This is intentional: repeated LookDev/material iterations must not pay a full Unity asset reimport on every commit.

The canonical production/release Unity gate remains clean and authoritative. Do not copy the warm-workspace rule into release validation merely for speed.

If a warm visual diagnostic behaves suspiciously, force a one-off clean diagnostic rather than reverting all fast visual loops to cold imports.
