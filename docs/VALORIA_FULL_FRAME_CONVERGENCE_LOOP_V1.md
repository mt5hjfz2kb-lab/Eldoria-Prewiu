# Valoria Full-Frame Convergence Loop v1

Date: 2026-10-02
Status: CANONICAL VISUAL EXECUTION LOOP

## Purpose

Stop treating Valoria visual production as a chain of isolated micro-fixes. The production unit is now the **entire official gameplay frame**.

The loop is:

`capture current frame -> inventory every visible benchmark gap -> batch all currently actionable fixes -> capture 19/12/9/mobile -> compare BEFORE/AFTER -> keep only net-positive changes -> repeat`.

The loop continues until the integrated frame is materially closer to `docs/ELDORIA_VISUAL_BENCHMARK.md` or a documented external blocker prevents further progress.

## Acceptance hierarchy

Three statuses are separate and must never be conflated:

1. **TECH PASS** — build/tests/gameplay signature are safe.
2. **LOCAL VISUAL PASS** — a bounded element improved.
3. **FULL-FRAME VISUAL PASS** — the integrated Valoria frame is materially closer to the benchmark.

Only **FULL-FRAME VISUAL PASS** counts as completed visual progress.

A TECH PASS or LOCAL VISUAL PASS may be retained as evidence, but it must not terminate the convergence loop.

## Mandatory defect ledger

Every iteration must review the whole official frame and classify every significant visible gap under these lanes:

- composition_and_mass
- architecture_coherence
- terrain_and_transitions
- materials_and_surface
- lighting_and_value_hierarchy
- atmosphere_horizon_depth
- vegetation_and_environment
- life_props_storytelling
- corruption_world_threat
- hud_world_balance

Do not create a separate workstream for each defect. Defects are entries in one iteration ledger unless a genuine exclusive-resource blocker requires isolation.

## Batch rule

Within an iteration, execute **all currently actionable zero-credit defects** that can be changed coherently without violating gameplay authority or another active workstream.

Order by dependency, not by chat turn:

1. composition / mass / terrain continuity;
2. architecture and transitions;
3. shared surface/material/value system;
4. lighting / atmosphere / depth;
5. vegetation / props / life / corruption;
6. HUD/world framing.

A defect may be deferred only with an explicit reason: dependency, missing geometry, runner/resource conflict, credit authorization, or measured performance risk.

## Tool routing

Toolchain Automation v2 remains authoritative inside the loop:

- composition -> Unity first;
- surface -> Unity, then Blender only when source processing is justified;
- geometry -> prove a real gap before Tripo;
- Tripo spend -> exact input + visible cost + fresh owner authorization.

The toolchain is subordinate to the full-frame objective. Successful tool execution is not itself a visual acceptance result.

## Iteration evidence

Each iteration must produce:

- BEFORE and AFTER at official zoom 19;
- BEFORE and AFTER at zoom 12;
- BEFORE and AFTER at zoom 9;
- BEFORE and AFTER mobile framing;
- unchanged gameplay collider/hotspot/circulation signature for visual-only passes;
- defect ledger with status per lane;
- one integrated verdict: FULL-FRAME VISUAL PASS, PARTIAL, or FAIL.

If the AFTER frame is not materially better, revert or disable the responsible batch rather than accumulating technical changes with no visible return.

## Anti-micro-pass rule

A single stair, wall, building, material, decal, prop family or asset uplift must not become the terminal visual objective.

A bounded proof is allowed only when it is necessary to unblock the batch. After proof, it must immediately re-enter the same full-frame iteration and be judged in context.

## Publish rule

Owner-facing WebGL must track coherent visual milestones. After a FULL-FRAME VISUAL PASS is promoted to `main`, update `pipeline/unity-publish-request.json` to the promoted source SHA and publish a fresh OWNER I-II build.

Do not ask the owner to evaluate current visual quality from a stale WebGL build.

## Stop conditions

The loop stops only when one of these is true:

- the frame reaches an accepted benchmark milestone;
- a concrete blocker requires owner authorization/input;
- all remaining defects require new geometry/paid action and zero-credit routes are exhausted;
- a measured device/performance constraint requires a separate decision.

“Workflow succeeded”, “artifact uploaded”, “asset improved”, or “TECH PASS” are not stop conditions.


## Owner execution directive

The approved reference frame is the quality target, not loose inspiration.

The acting lead programmer must:
- continue from the **real current repository and run state**, never reconstruct project state from chat memory when the repository can answer it;
- use any available tool/documentation/research needed to understand the production tools correctly;
- pursue the target through continuous full-frame convergence, not isolated defect-by-defect completion;
- keep working without asking the owner to say "continue" after intermediate stages;
- stop only for a genuine blocker that cannot be resolved autonomously, a required fresh owner authorization/decision, or owner input that materially changes the target;
- state plainly when the current stack has reached a demonstrated visual ceiling below the approved reference;
- when such a ceiling is demonstrated, identify the concrete missing capability/assets/process and present the practical solutions rather than lowering the target silently;
- never present technical success, workflow success, or a local asset improvement as equivalent to reaching the approved visual quality.

When a new chat/work session begins, the repository, this directive, the active workstream registry and the latest successful visual artifacts are the handoff authority.
