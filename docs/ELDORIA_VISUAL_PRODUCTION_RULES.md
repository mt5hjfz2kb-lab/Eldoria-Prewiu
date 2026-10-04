# Eldoria — Visual Production Rules

Status: canonical visual-production governance  
Applies to: Valoria and later Eldoria visual production blocks  
Owner intent: durable repository rules so future chat prompts stay short

## 1. Source of truth

The repository, live branch/HEAD, workflow runs, artifacts and real Unity captures override chat memory or summaries.

At the start of any visual block:
1. read `AGENTS.md`, `SESSION_HANDOFF.md`, `PROJECT_STATE.md` and `pipeline/active-workstreams.json`;
2. for Valoria, read `docs/VALORIA_VISUAL_BIBLE.md` and `docs/ELDORIA_PRODUCTION_ART_SOURCE_PIPELINE.md`;
3. read this file;
4. read only the result/research/evidence documents relevant to the previous block and proposed next block;
5. verify live `main` HEAD, active workstream ownership and the last valid visual artifact before editing.

Prompts should describe only the new block, its objective, scope and any exceptional constraint. Do not restate these permanent rules in every prompt.

## 2. Product intent versus technical prescription

The project owner defines product goals, desired feeling, references, priorities and acceptable trade-offs. The owner is not assumed to be a programmer, technical artist or rendering specialist.

Treat owner-proposed technical solutions as hypotheses, not as automatically correct implementation instructions.

For every significant proposal:
1. identify the underlying product/visual problem;
2. test whether the suggested solution actually addresses it;
3. identify technical/artistic risks and side effects;
4. compare with better alternatives when available;
5. recommend and implement the approach best supported by evidence.

Do not agree merely to validate an owner suggestion. Do not preserve an agent's earlier recommendation when new evidence shows it is inferior. The goal is to improve Eldoria.

## 3. Locked Valoria structural direction

Unless a later promoted proof explicitly replaces it, preserve the accepted Flat Citadel structural direction and its gameplay contracts.

Do not reopen a large-mountain/world-frame solution merely to solve an art-quality problem.

Preserve:
- gameplay topology, hotspots and colliders;
- progression and building roles;
- `VALORIA PARCEL RESERVATION MAP` and future building space;
- C0/C1/C2 circulation and documented expansion exits;
- camera/gameplay contracts unless the block specifically concerns framing;
- future headroom for additional Bastion levels and districts.

Visual dressing must not consume reserved future parcels irreversibly.

## 4. Screen-space scale is a hard visual requirement

A technically correct city that reads as a small model sitting in a field is not an acceptable final presentation.

The approved visual direction requires the city to dominate the primary gameplay viewport while preserving playable camera movement.

For primary gameplay and especially mobile:
- Bastion and key buildings must be large enough to read;
- walls should approach the useful frame rather than sit as a tiny island in large empty terrain;
- exterior terrain is context, not the main subject;
- roads, buildings and activity must remain readable;
- framing must be evaluated before solving the problem by enlarging the world or adding density.

A mobile capture that still reads as "a piece of city in the middle of grass" is a VISUAL FAIL even if CI is green.

Screen-space framing and world size are different problems. Do not enlarge the city or repopulate reserved parcels merely to fill the screen.

## 5. Zoom fidelity and sharpness

Beautiful distant assets are insufficient if the playable camera exposes visibly pixelated or degraded close views.

When a block changes or certifies a major asset, inspect at the closest intended gameplay zoom:
- texture resolution and texel density;
- mipmaps;
- compression;
- anisotropic/filtering settings where applicable;
- LOD transitions;
- render/capture resolution;
- material/shader detail loss;
- whether the artifact comes from the asset itself or capture scaling.

Do not assume visible pixelation is only a screenshot problem. Verify it in a real Unity capture at the intended gameplay zoom.

Sharpness work should not interrupt an unrelated proof unless it blocks valid comparison; record it and open a focused block when necessary.

## 6. Mandatory production loop

Use this loop for visual production:

**problem -> research -> hypothesis -> implement a meaningful block -> Unity -> artifact -> real captures -> compare -> keep/revert -> document -> continue**

Rules:
- prioritize high-impact defects;
- research before improvising when the technical route is uncertain;
- implement coherent blocks rather than tiny position/scale/color changes;
- use deterministic real Unity evidence;
- compare BEFORE/AFTER and against `docs/ELDORIA_VISUAL_BENCHMARK.md`;
- revert variants that are worse;
- after 2–3 failed attempts with one technique, change technique instead of continuing by inertia;
- a commit, green run or artifact upload is not a stopping point.

Do not accumulate failed visual experiments in the accepted implementation.

## 7. Visual acceptance is separate from technical acceptance

Use explicit verdicts:
- `TECH PASS / VISUAL PASS / PROMOTED`;
- `TECH PASS / VISUAL FAIL / NOT PROMOTED`;
- `TECH FAIL` when the implementation/evidence itself is invalid.

Green CI does not imply a visual pass.

Promotion requires a visible improvement in real played-camera captures, not merely better isolated assets, cleaner code or richer materials.

When a proof is intentionally scoped to zoom 9/mobile, fail fast there before spending time on wider capture sets.

## 8. Toolchain evolution

Do not assume the current toolset is the ceiling, and do not add tools merely because they are fashionable.

For each important blocker ask, in order:

### A. Are current tools being used properly?
Investigate advanced/correct use of:
- Unity / URP;
- Blender;
- GLB import/export;
- materials, shaders, decals and blending;
- baking, UV, mesh and LOD workflows;
- asset library and reusable source files;
- repository automation and existing generic pipelines.

### B. Is a better specialized tool justified?
If a real ceiling remains, investigate plugins, packages, add-ons or external tools that specifically address it.

Before adoption:
- verify capability and documentation;
- check compatibility, maintenance, license and cost;
- test with a small reversible proof;
- measure visual/productivity gain;
- document reproducibility and removal path.

Adopt only when it provides a clear advantage.

Tripo or any paid/generative path is selective, never the default. Credit spend requires the existing explicit authorization rules in `AGENTS.md`.

## 8a. Greybox is not final art

For Valoria, classify visual geometry using `docs/ELDORIA_PRODUCTION_ART_SOURCE_PIPELINE.md`.

Primitive-based architecture created from Unity `CreatePrimitive`, simple cubes/cylinders/cones, minimal mathematical roof modules or equivalent procedural Blender primitive assembly defaults to GREYBOX / SUPPORT / TEMPORARY.

Do not call it final production architecture merely because:
- it was produced in Blender;
- it has PBR materials;
- CI is green;
- it looks cleaner than the previous prototype.

Production architecture requires authored source quality and real official-camera acceptance.

## 9. Asset strategy: preserve, reauthor or replace based on evidence

Do not preserve an asset simply because work has already been invested in it.

Use this decision:
- **preserve** when the source already fits the target language;
- **reauthor** when high-value geometry/identity is strong but materials, source construction or selected forms can be corrected efficiently;
- **replace** when silhouette, proportions, construction, detail density and material language are so incompatible that repair is more expensive or less coherent than a new source.

Unity is primarily the integration/presentation/runtime environment. Deep 3D source authoring should normally happen in Blender or another justified source-authoring tool.

A newly generated asset must still pass cleanup, reproducibility, Unity integration and real-camera comparison.

## 10. Coherent visual language means more than shared colors

Do not call assets unified merely because they share a palette or material set.

Coherence includes:
- silhouette vocabulary;
- proportions;
- construction logic;
- density and scale of detail;
- stone/wood/roof grammar;
- bases and terrain transitions;
- material response;
- lighting response;
- wear and trim language;
- readability at official zooms.

Wrappers, recolors and common materials are insufficient when source geometry remains visually incompatible.

## 11. World edge and environment

Avoid flat-board, infinite-grass and isolated-diorama readings.

The exterior should imply a continuing world efficiently through authored combinations of roads, low vegetation, fields, low-relief terrain, fog/atmosphere, shallow banks, small rocks, water/ditches or camera-contained backdrop strategies where appropriate.

Do not solve world-edge problems by restoring a giant mountain or constructing unnecessary kilometres of environment.

## 12. Scalability before rollout

Do not migrate a visual language to multiple buildings/Bastions merely because one showcase looks attractive.

A proof intended for rollout must demonstrate:
- a repeatable source-authoring workflow;
- shared/reusable rules or components;
- acceptable cleanup/integration cost;
- mobile and official-camera readability;
- ability to improve later without rebuilding the city;
- preserved gameplay and progression contracts.

Before broad rollout, answer:
**Can this be extended to the next buildings/Bastions without recreating each result as a one-off artwork?**

If not, keep it as a proof and do not promote.

## 13. Evidence expectations

Use the capture set required by the block. Full production reviews normally use official 19/12/9/mobile views; early stop-gate proofs may intentionally use only 9/mobile.

Evidence should record:
- exact implementation HEAD;
- run and artifact IDs;
- BEFORE/AFTER;
- real HUD when presentation is in scope;
- gameplay/collider/hotspot preservation where relevant;
- credits/cost when external tools are used;
- source files and reproducible export route for authored assets;
- honest remaining defects;
- promotion decision.

## 14. Prompt-size rule

Permanent process rules belong in the repository, not in repeated chat prompts.

A normal continuation prompt should usually contain only:
- repository and base branch;
- instruction that repo state wins;
- required canonical docs to read;
- previous block/run/artifact only when needed to disambiguate;
- new workstream name;
- one clear objective;
- narrow scope and stop gate;
- any exceptional authorization or constraint.

Future agents must read this document instead of requiring the owner to paste long governance instructions repeatedly.

## 15. Default short continuation template

Use this shape unless the task truly requires more detail:

> Continue Eldoria from the real canonical repository state. Repo: `mt5hjfz2kb-lab/Eldoria-Prewiu`, base `main`. Repo state wins over chat memory. Read `AGENTS.md`, `SESSION_HANDOFF.md`, `PROJECT_STATE.md`, `pipeline/active-workstreams.json`, `docs/ELDORIA_VISUAL_PRODUCTION_RULES.md`, the visual benchmark, and the previous block's result/evidence. Verify live HEAD/runs/artifacts before editing.
>
> New block: **<name>**.
>
> Objective: **<single measurable objective>**.
>
> Scope: **<bounded assets/systems>**.
>
> Stop gate: **<e.g. zoom 9 + mobile; promote only on clear visual win>**.
>
> Preserve locked gameplay, parcels and macrostructure. Continue until the block is closed or a genuine owner decision/authorization is required.

This template deliberately omits permanent rules because this document and `AGENTS.md` already govern them.
