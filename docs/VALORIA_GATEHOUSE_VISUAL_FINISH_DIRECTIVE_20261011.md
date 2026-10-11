# ELDORIA — Visual Finish First / Gatehouse Art Direction V6

## Scope and authority
Art-direction handoff for the existing gatehouse workstream on this branch. **This is a new acceptance criterion, not proof that any automated agent has read it.** Keep `main` unchanged; coordinate ownership before modifying files touched by another worker.

## Explicit change of course
**Stop increasing complexity for complexity's sake.** The goal is no longer to add another roof, tower, chamber, crenellation, buttress, or decorative stone. The goal is to make the **existing gatehouse** look materially and visually better, at unchanged or reduced silhouette complexity where appropriate. Keep the strongest existing version as a reversible baseline, particularly V4/V5 and historical renders. Do not call extra geometry automatically "higher fidelity."

Prioritize, in this order:
1. **Visual quality of materials:** physically coherent, varied stone/wood/metal with appropriate PBR roughness/normal/color, scale-consistent texturing and no tiling artifacts. Source only license-compatible zero-cost assets; record provenance.
2. **Surface realism and craftsmanship:** varied stone courses, bevels, weighted normals, selective wear and grime; remove obvious Lego-like repetition and disconnected-looking modules. Use actual geo detail only where silhouette or shadowing requires it.
3. **Coherent art direction:** controlled palette and style consistent with `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`; architectural integration rather than accumulating decorative pieces.
4. **Lighting and presentation:** matched Blender before/after renders with fixed camera, exposure, focal length and lighting; a second mobile-scale shot. Do not confuse lighting improvement with asset improvement; capture both neutral and beauty views where possible.
5. **Real Unity appearance:** material import/persistence, scale, normals, camera-strategy readability and mobile budget, as soon as a visually improved candidate exists.

## Required decision loop
- Inspect exact reference image, historical strongest gatehouse, and the latest V4/V5 images.
- Identify top three **finish/quality** failures with crop or image evidence.
- Make **one controlled material/finish pass** before building any new architectural elements. Use Blender, existing studio tools, BlenderMCP when feasible, Qwen only as optional auxiliary, never single judge.
- Render A/B at fixed settings; keep improvement only when actual images show it. Classify as ACCEPT, REWORK or REJECT with concise why.
- Preserve `.blend`, `.glb`, textures, manifests and image comparisons in the existing art branch; don't overwrite previous approved-best files.
- If a tool is missing, use available offline Blender/PBR tooling and continue; don't create a new subsystem just to make this pass.

## Hard stop on scope creep
No new asset families and **no more structural additions unless an evidenced visual defect cannot be fixed through materials, shape refinement or composition**. A successful GitHub Action is a technical PASS, not visual approval. Do not claim 7.5–8/10 without visual proof. Target a clearly visible improvement in **surface quality, convincing material response, reduced modular repetition, and coherence** compared with V5.

## Next deliverable
Same-angle V5 vs finish-pass stills + mobile camera stills, original image unmodified, annotated diagnosis, editable Blender source, GLB/textures, and if available Unity import visual proof. The user needs a **quality improvement**, not another layer of stuff.
