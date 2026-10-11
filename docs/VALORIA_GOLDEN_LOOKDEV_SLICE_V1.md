# VALORIA GOLDEN LOOKDEV SLICE v1

Date: 2026-10-05  
Status: **ACTIVE — GOLDEN LOOKDEV DEFINITION**

## Why this block exists

The current Valoria frame has repeatedly passed technical, integration and gameplay gates while remaining far below the canonical premium visual target. Five bounded Premium Hero Test Zone iterations established that broad-scene lighting tweaks, flat-color material tuning and local contact patches are not enough.

This block intentionally stops broad content production and proves one complete, reproducible visual recipe first.

**Core rule: prove one premium square metre before producing another district.**

No Props + Background, new production family, city expansion, world map, animation or Tripo work may start while this gate is unresolved.

## Canonical authority

- Visual target: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`
- Camera/composition authority remains the approved reset camera and matched-camera family.
- Structural positions and gameplay remain locked.
- Existing closed families remain evidence and reusable geometry; they are not automatically reopened.
- Tripo: **0 credits** unless the owner separately authorizes a specific exact input and cost.

## Proven failure that this supersedes

`VALORIA PREMIUM TARGET GAP REVIEW + VISUAL UPLIFT v1` ended after five attempts:
- iteration01: VISUAL FAIL
- iteration02: VISUAL FAIL
- iteration03: VISUAL FAIL
- iteration04: TECH PASS / VISUAL INVALID
- iteration05: TECH PASS / VISUAL FAIL

Conclusion: the project cannot reach the reference by stacking more broad-scene cosmetic corrections on top of flat/simple surface response. The next proof must include real look-development and surface authoring.

## Golden Slice scope

Use a tiny, fixed section of the existing real scene, not a disconnected showcase diorama:

**Lower Gate edge + one adjacent rock/cliff contact + one ground patch + one water/shore contact + 2–3 existing vegetation instances.**

The slice must remain captured from the official gameplay camera family. It exists to prove the rendering/material language, not to redesign composition.

## Target translation step — mandatory before implementation

The canonical reference is an artistic target. Before tuning Unity, derive a **Golden Slice target sheet** from the real current scene/camera that states exactly how the reference language maps onto the slice:

- value hierarchy
- warm/cool relationship
- stone hue family
- rock hue family
- ground hue family
- water hue/depth family
- shadow softness and density
- highlight/specular hierarchy
- edge wear policy
- dirt/wetness/contact policy
- vegetation value variation
- atmospheric depth
- acceptable detail frequencies at gameplay scale

This target sheet is the bridge:

**concept reference → realizable lookdev target → 3D implementation**

Do not improvise the look directly inside Unity.

## Surface-authoring requirement

The Golden Slice must test an actual authored surface stack, not only flat colors.

For hero-visible stone/rock/ground/wet-contact surfaces, evaluate and use only what materially improves the target:

- authored albedo/base-color variation
- normal detail
- roughness/smoothness variation
- AO/contact information
- masks/vertex colors where useful
- tileable + macro breakup
- terrain-layer or mesh-layer blending
- selective decals/contact masks
- wetness/darkening near water
- edge/trim/breakup treatment visible at gameplay scale

The result must contain **mid-frequency information**. Large screen-space surfaces may not jump directly from macro geometry to uniform color.

## Geometry policy

Do not reauthor geometry by default.

Use current geometry first. Blender becomes justified only if the Golden Slice proves a geometry-limited defect such as:
- silhouette remains primitive after proper materials/light,
- contact geometry cannot blend convincingly,
- edge profile is too mathematically sharp,
- required depth/parallax cannot be produced by the surface stack.

Any Blender uplift must be local and evidence-driven.

## Lighting / rendering proof

The slice must test a coherent lighting setup rather than isolated tweaks:

- directional/key hierarchy
- ambient fill
- mobile-safe shadows
- contact/AO support
- indirect-light strategy appropriate to the current Unity pipeline
- reflection/environment response where useful
- controlled fog/atmospheric perspective
- exposure/contrast
- color grading

The target is not “more dramatic”; it is **more material separation, depth and premium readability**.

## Water / shoreline proof

Water must have enough depth/value/roughness response to support the bridge/gate foreground without stealing focus. The shoreline must not read as a perfect cut line.

Test only bounded techniques:
- shallow-to-deep value shift
- controlled specular
- wet/dark contact
- restrained foam/reed/stone accent only if visually necessary
- mobile-safe transparency/overdraw

## Vegetation proof

Reuse the accepted vegetation geometry first. Improve only:
- grounding/contact
- trunk/canopy value separation
- controlled hue/value variation
- light response
- shadow integration

Do not add density to hide surface problems.

## Texture-authoring route

A serious texture-authoring tool is allowed and should be evaluated for the Golden Slice. Substance 3D Painter/Designer or an equivalent workflow may be adopted if it materially improves repeatable stone/rock/ground authored surfaces.

This does **not** authorize purchased art packs. The objective remains Eldoria-owned geometry/material identity.

If a new external tool is adopted, record:
- exact purpose,
- source/output formats,
- reproducibility,
- license/cost impact,
- mobile texture budget,
- fallback route.

## Performance constraints

Mobile viability remains mandatory, but optimization must not erase the quality proof.

Track:
- texture dimensions and memory
- material count
- draw calls
- instancing
- shader complexity
- transparency/overdraw
- shadow cost
- LOD/culling where applicable

First identify the correct visual recipe, then optimize the same recipe without materially degrading it.

## Evidence set

Every meaningful iteration must produce:
- REFERENCE crop/target
- BASELINE Golden Slice crop
- AFTER 16:9
- AFTER mobile landscape
- AFTER 3:2
- AFTER portrait relevant view
- close crop of stone/rock/ground/water contact
- technical metrics
- gameplay regression
- performance sanity

Do not judge from isolated material spheres or editor beauty shots.

## Golden quality bar

Scale:
- 0 = prototype
- 1 = very weak
- 2 = acceptable prototype
- 3 = production-mid
- 4 = premium-close
- 5 = reference-class

Mandatory core metrics:
- material richness
- lighting depth
- contact quality
- terrain/architecture integration
- water/shore integration
- natural integration
- premium perception
- mobile readability

The Golden Slice does not pass unless:
- **material richness >= 4/5**
- **lighting depth >= 4/5**
- **contact quality >= 4/5**
- **environment integration >= 4/5**
- **premium perception >= 4/5**
- mobile readability remains at least 4/5
- gameplay regression PASS
- performance sanity PASS

A run being green is irrelevant to the visual verdict.

## Anti-loop rule

Do not make endless micro-iterations.

After each failed iteration, classify the failure:
- TARGET TRANSLATION
- SURFACE
- LIGHTING
- GEOMETRY
- WATER
- VEGETATION
- PERFORMANCE

The next iteration must change the dominant failure class materially. Do not keep nudging values inside a failed method.

After **three technically valid visual failures within the same method**, stop and change method. Do not fill the scene with more layers to hide the failure.

## Closure

This block may close only as:

**VALORIA GOLDEN LOOKDEV SLICE v1 — GOLDEN LOOKDEV PASS / CLOSED**

The closure must include:
- approved target translation sheet
- approved final capture set
- approved material/surface recipe
- approved lighting/presentation recipe
- mobile/performance constraints
- reusable implementation rules for all future Valoria families

Only after this pass may broad Valoria production resume.

The immediate next production action after closure is **not automatic**. The owner must explicitly authorize scaling the Golden Lookdev recipe to the rest of Valoria.

## 2026-10-05 — Surface + bounded geometry final gate
Status is now **BLOCKED — HUMAN SCOPE AUTHORIZATION REQUIRED**.

The surface-only method was exhausted after three technically valid iterations. A persisted Blender-authored PBR stack (20 maps: albedo/normal/AO/smoothness across stone/rock/ground/shore/vegetation) is reproducible and mobile-sane, but the official Golden gate remained below 4/5.

Bounded geometry escalation tested three local shells. Source02 is the strongest bounded result (run 37368033376 / artifact 11368942273), reaching approximately production-mid response without catastrophic boundary artifacts. Source03 final proof (run 37368890740 / artifact 11368813433) failed because continuous cliff aprons read as pasted substrate islands.

**SURFACE AUTHORING PASS is not granted. GOLDEN LOOKDEV PASS is not granted.**

The remaining blocker is structural and scope-locked: continuous Rock/Terrain + local ground substrate, with Lower Gate/Bridge edge profiles also constraining premium response. More local overlays are prohibited by evidence.

Owner decision required before further work:
- A: reopen continuous Rock/Terrain + local ground around Lower Gate/Bridge as one authored replacement surface; or
- B: reopen that substrate together with Lower Gate + Bridge edge profiles as one integrated Golden micro-environment.

Canonical detail: `docs/evidence/valoria-golden-lookdev-slice-v1/FINAL_SURFACE_GEOMETRY_GATE.md`.


## 2026-10-05 — Owner-authorized Method B final source gate

Owner authorization **B** was executed exactly as a pre-Unity source phase: target translation → Blender base reauthor → isolated hero preview → visual review.

Two integrated source attempts were produced. Source01 (run **37370017454**, artifact **11370535174**) failed visually at approximately 2–3/5. Source02 materially changed method by using the real closed Lower Gate + Bridge production geometry at canonical origins, a continuous authored substrate and already-committed higher-information transition geometry. Its authoritative evidence run **37370727523** / artifact **11369951282** is technically valid (~147k tris) but the isolated visual gate still fails: geometry/silhouette ~2.5/5, material response ~3/5, contact/integration ~2.5/5, environment coherence ~2.5/5, premium perception ~2/5.

Therefore **Unity integration was not run**. This is intentional and required by the source-first gate.

The new proven ceiling is that edge/profile reauthoring plus continuous local substrate cannot overcome the screen-space dominance of the existing Lower Gate/Bridge **primary forms** and the retained vegetation source. More donor pieces, overlays, rocks, decals, lighting compensation or another source iteration are prohibited.

Current status: **BLOCKED — HUMAN SCOPE BLOCKER / INTEGRATED BASE METHOD EXHAUSTED**.

Smallest viable next owner authorization: reopen the **full screen-visible primary forms** of Lower Gate + Bridge together with the continuous local cliff/ground/shore and local vegetation source, while preserving canonical camera, macro positions, gameplay topology, parcels, Bastion and every family outside the Golden crop. A separate external generation route, including Tripo, remains unauthorized unless explicitly approved.
