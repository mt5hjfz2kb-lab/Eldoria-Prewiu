# Valoria Hero Bastion v1 — integrated hero-frame proof

Closed: 2026-10-01.  
Branch: `visual-proof/hero-bastion-integrated-v1`  
Final visual code: `d8645a6dfdce8e31eed4d62c123640ce20b35f90`  
Final run: **36825675469 SUCCESS**  
Artifact: **11145137063**

Verdict: **INTEGRATED VISUAL STEP-CHANGE PASS / NOT PRODUCTION READY**.

## Scope

This proof answers one narrow question: does the already-generated Hero Bastion materially improve the real Valoria frame when it replaces only the current Bastion visual shell, without changing gameplay topology?

No Tripo generation occurred. No credits were spent. The canonical optimized Hero Bastion was recovered from certified artifact `11143009723` by exact SHA and staged only inside the experimental workflow.

Source identities:
- raw GLB SHA-256: `fd859fe52b45dbdd3224f7f607897c1d36f326dc0ff2632b0a0f2a4b14348ebf`
- optimized GLB SHA-256: `afb6cee6ae572b0879650f18285b32798e263c17359a158ffe2bdd03fb62ad5c`
- optimized geometry: 49,800 triangles
- canonical orientation: yaw 180°

## Real-frame integration

The proof builds the real current `VisualWorld` city and records same-scene BEFORE/AFTER at:
- strategic 19
- city 12
- detail 9
- 390×844 mobile framing

Only the current Bastion visual families are suppressed. The authoritative gameplay target, hotspots, colliders, certified central stair and city topology remain unchanged. The generated asset contributes no gameplay colliders or hotspots.

Final evidence:
- same_scene_before_after: **true**
- collider_hotspot_signature_equal: **true**
- previous Bastion renderers suppressed: **19**
- generated asset colliders enabled: **false**
- generated asset hotspots added: **false**
- Tripo credits in this proof: **0**

Complexity:
- BEFORE: 1,574,646 triangles / 1,199 renderers / 629 materials / 22 lights
- AFTER: 1,529,895 triangles / 1,181 renderers / 609 materials / 22 lights

The replacement therefore does not obtain its visual improvement by increasing scene complexity.

Generated hero bounds in the integrated frame:
- centre: (0, 7.14, 8.75)
- size: (12.8, 9.239, 10.962)

## Visual result

### What is proven

The generated Bastion creates a clear category change at the focal point.

Compared with the previous current-main Bastion:
- the skyline is substantially more vertical and distinctive;
- the central keep reads immediately;
- the unequal flank towers survive official camera distance;
- blue roofs and heraldry establish a strong Eldoria identity;
- the gate/stair/front-access read survives zoom 9 and mobile framing;
- the rock-integrated base gives the upper city a much stronger mountain-fortress silhouette;
- at zoom 19 the Bastion remains legible as the primary landmark instead of collapsing into a grey block.

This is the first real-frame proof in this sequence where replacing the hero architecture itself produces a visually material whole-frame difference.

### Zero-credit surface-fit recovery

The first successful integrated capture (run **36825288415**, artifact **11144987985**) proved the geometry/composition jump but the imported Tripo material response was too pale and overexposed.

A second zero-credit pass retained the exact generated mesh and texture/normal content while normalizing only its URP lighting response. No geometry was edited and no regeneration occurred.

Final surface-fit run **36825675469** is visibly better balanced with Valoria while retaining the blue/gold identity. It is the canonical evidence for this proof.

### Why this is not production-ready yet

The new Bastion now exposes the next bottleneck rather than solving the whole frame:
- the lower/middle city is visibly lower-category than the hero asset;
- some Bastion highlights are still too bright under the current frame lighting;
- the rock/base-to-existing-terrace and stair seam needs an authored integration pass;
- 49.8K reduction still softens some small architectural details, although the loss is far less important at the actual mobile/game cameras than in isolated clay diagnostics;
- physical mobile performance is still unmeasured.

Therefore the correct production conclusion is **not** to regenerate the Bastion and **not** to promote the current proof directly.

## Decision

Keep the generated Hero Bastion source.

Do **not** spend more Tripo credits on this Bastion.

The major uncertainty is resolved: a custom, frame-specific hero Bastion can materially lift Valoria in the real game frame.

Next art work should use this Bastion as the visual quality anchor and attack the newly exposed gap:
1. authored rock/terrace/stair seating around the Bastion;
2. lighting/exposure match;
3. bring the immediate upper/middle district architecture and surfaces toward the same category;
4. only then consider production promotion and device profiling.

`Valoria.unity`, gameplay rules, hotspot ownership and physical circulation were not modified by this experiment.
