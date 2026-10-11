# VALORIA BASTION-TO-CITY ARCHITECTURAL FRAME v1 — RESULT

Date: 2026-10-04  
Canonical reference: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`

## Final verdict

**TECH PASS / ART SOURCE FAIL / UNITY NOT RUN / VISUAL NOT RUN / NOT PROMOTED**

The block followed the required stop-gate. A single complete Bastion-to-city frame was authored as one source candidate under `BLENDER_PROFESSIONAL_V1`, and isolated evidence was reviewed before Unity. The source did not reach production-art quality, so integration and final A/B were deliberately not executed.

## Authoritative evidence

- Art-production planner: **37225548192 — SUCCESS**
- Successful Blender ART SOURCE run: **37226253119 — SUCCESS**
- Source artifact: **11312172507**
- Source commit: `649e9401bdaa0404aada894b42754983d72c59ed`
- Tripo credits: **0**
- Paid credits: **0**

Technical-only failed/cancelled source attempts preceding the successful source run did not produce an art verdict. They were limited to preview-pipeline fixes (Blender World initialization, escaped newline repair, EGL/headless rendering dependencies) and did not change the candidate design.

## Source identity

- Blend: `art-source/valoria/production/bastion-to-city-architectural-frame-v1/Valoria_BastionToCityArchitecturalFrame_v1.blend`
- Blend SHA-256: `420099f745ab839a8a3080953b198e790bc0a2222f4a247815c9beeea0230e75`
- GLB: `art-source/valoria/production/bastion-to-city-architectural-frame-v1/Valoria_BastionToCityArchitecturalFrame_v1.glb`
- GLB SHA-256: `772f90196ac50e0fb874e95db492e615b92495a649b7a42ebeb0abf2ae8f0957`
- Mesh objects: 67
- Approx. triangles: 5,028

These metrics are recorded only for reproducibility. They are not quality evidence.

## Isolated ART SOURCE evidence

Reviewed directly:
- `pipeline/evidence/valoria-bastion-to-city-architectural-frame-v1/source/01-silhouette-clay.png`
- `pipeline/evidence/valoria-bastion-to-city-architectural-frame-v1/source/02-lit-three-quarter.png`
- `pipeline/evidence/valoria-bastion-to-city-architectural-frame-v1/source/03-game-camera-proxy.png`

## Required ART SOURCE questions

### 1. Does the silhouette look designed rather than generated?

**FAIL.**

The candidate is asymmetric and no longer a repeated-bay formula, but the dominant visual language still reads as large rectilinear masses with trims, arch frames and buttresses added onto them. It looks intentionally assembled, but not sufficiently sculpted/authored to read as premium final environment art.

### 2. Is there real depth?

**PARTIAL.**

There are actual recessed openings, a central stair throat, setbacks, terraces and shadow planes. However, the depth hierarchy is shallow and schematic. The massing remains dominated by planar fronts and box-like shoulders rather than convincing architectural volumes with believable returns and nested spaces.

### 3. Is repetition controlled?

**PASS, but insufficient.**

The west/east halves are deliberately different and there is no simple mirrored or formulaic bay repetition. This solves one failure mode, but controlled repetition alone does not make the source production-ready.

### 4. Does it materially belong to Valoria?

**PARTIAL-FAIL.**

The warm stone / dark recess / restrained blue palette is directionally compatible with the Visual Bible, but the form language is still too clean, blocky and generic. The rock interfaces are wedge-like and the upper arch reads as an attached symbol rather than a naturally integrated Bastion-city construction.

### 5. Does it clearly improve the target frame?

**FAIL at source gate.**

The candidate would replace technical supports with recognizable architecture, but the isolated proxy already shows that it would substitute one unfinished read for another: a more architectural greybox rather than a premium capital frame. That is not enough to justify Unity integration.

## Direct visual diagnosis

The strongest problem is not polygon count, lack of asymmetry, or lack of architectural vocabulary. It is the authoring method itself.

The scripted source still resolves major masses as rectilinear custom meshes with secondary trims and arch parts. Even though these are not Blender primitive objects, the visual result remains equivalent to a sophisticated blockout. The source lacks the continuous freeform silhouette, nuanced wall thickness, believable erosion/construction transitions, irregular structural logic and authored stone/rock interlock visible in the approved reference.

The large upper arch particularly exposes the problem: it reads as an element placed on top of the retaining system rather than as part of one continuous capital architecture.

## Stop-gate decision

Per the mandatory sequence:

**Reference -> screen-space design -> material/value plan -> Blender Professional Authoring -> ART SOURCE PASS -> Unity -> A/B**

the candidate stops at **ART SOURCE FAIL**.

Therefore:
- no Unity integration request was dispatched;
- no HOME/PAN/16:9 AFTER captures were produced;
- no gameplay/camera/parcel/runtime surfaces were changed;
- no production hook was added;
- nothing was promoted.

## Anti-loop conclusion

Do **not** open a v2/v3 of this scripted frame.
Do **not** micro-adjust dimensions, trims, arches, colors or placement.
Do **not** add more modules or detail to rescue this source.

The demonstrated limit is: **scripted explicit box-derived mesh authoring is still not sufficient for this high-salience final-art frame, even when asymmetry and architectural vocabulary are deliberate.**

The next valid method, if this area is revisited, must change authoring mode rather than iterate this source. It should begin from a high-fidelity 2D design/paintover of the exact played silhouette and use genuinely freeform/direct DCC reauthoring (sculpt/boolean/manual mesh deformation and richer stone-rock interlock), or another owner-approved high-quality source route followed by Blender reauthoring. Any Tripo use remains blocked without explicit authorization.

## Mandatory final question

**¿LA CAPTURA HOME EMPIEZA YA A PARECER EL JUEGO FINAL QUE QUEREMOS CONSTRUIR, AUNQUE TODAVÍA FALTE CONTENIDO?**

**NO / NOT TESTED IN UNITY.** There is intentionally no AFTER HOME capture because the source failed before integration. The isolated proxy already showed insufficient final-art quality, so producing a Unity HOME would have violated the ART SOURCE stop-gate.
