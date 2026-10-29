# ELDORIA — HEROES UI — ART LOCK / APPROVED REFERENCE (2026-10-30)

## Owner approval
The owner explicitly approved the six-panel concept shown in the conversation immediately before this document and requests **100% visual fidelity**. Treat this image as the art-direction authority, not as proof of implemented UI. Do not redesign, restyle, simplify, substitute characters, or invent visual alternatives without owner approval.

**Reference image:** generated in the conversation, image generation ID `95d70929-2cba-4d2d-802e-fd47f555ef4d`; conversation attachment `file_00000000467081f4b8a36e14886348fc` (local rendering path `/mnt/data/a_detailed_high_quality_fantasy_game_ui_concept_b.png` is conversation-scoped, NOT a repository path). The image MUST be copied into the repository's art-source/asset vault before implementation; do not claim it is already stored in GitHub. If unavailable to a worker, obtain this exact approved image rather than regenerating it.

## Six approved views
1. Salón de Héroes: dark medieval vaulted hall, golden trims, Aldric/Lyra illustrated portrait cards, locked Maelis/unknown cards, filters, bottom navigation.
2. Aldric / Resumen: full-height hero center, compact right panel, four combat attributes, hero power, hero carousel.
3. Aldric / Habilidades: same shell, illustrated ability list, selected tab and hero carousel.
4. Aldric / Talentos: same shell, three visually distinct vertical talent paths, locks and selected talent details.
5. Aldric / Equipamiento: same shell, model center, four gear slots and detail/upgrade panel.
6. Lyra / Resumen: identical component system with Lyra's own visual identity.

## Mandatory design constraints
- NO top materials/resources strip, under any circumstances.
- Preserve approved colors, framing, layout, spacing, typographic hierarchy, decorative motifs, selected-state glow, tabs, cards and navigation.
- Implement 2D UI with individually authored reusable assets. On individual hero profiles, use the **actual approved Tripo character model** rendered by Unity in a controlled camera/render texture, once the exact model asset and import status are verified. The illustration is not proof that the actual model matches it. Never replace the approved source with a lookalike without approval.
- Combat stat labels are **Vida, Ataque, Defensa, Ruptura**, not Velocidad/Alcance/Capacidad/Liderazgo/Apoyo. Source of truth: `v0220/js/hero-army.js` and Unity canonical combat domain. Aldric BASE: health=420, attack=78, defense=112, break=34. Lyra BASE: health=300, attack=116, defense=62, break=82. Display runtime values if leveled/equipped, never hardcode concept numbers as game balance.
- Power of Hero and Power of March are distinct. Mockup figures 2450/1830, stars, hero levels, equipment levels, abilities/talents shown in image are illustrative, not authorized gameplay data. Existing skills: Aldric Baluarte; Lyra Disparo de Ruptura. Additional illustrated skills/talent branches are NOT approved mechanics; keep locked/reserved or wire only to real data.
- Heroes screen is unlocked after recruiting Lyra through Bastion III Fissure narrative; Relicario after Bastion VII, as defined by web vertical slice. Preserve all existing gameplay, save, routing, tutorial and accessibility.
- Ensure portrait-first mobile safe areas, readability and performance. Art assets must match original reference; use actual screenshot pixel comparisons at target portrait resolution and manual owner art acceptance, not only compile/test PASS.
- Never modify current main gameplay, scenes, PRs, builds or release channels to prepare the art package. Implement separately only after source art/model inventory, reference preservation, component slicing, technical checks and owner go-ahead.

## Readiness checklist (not yet certified)
[ ] Exact approved reference image archived in canonical asset vault with hash and provenance.
[ ] Approved Tripo Aldric/Lyra model IDs and exported files located, visually inspected and imported in isolation.
[ ] All image elements sliced into named assets and fonts/icons verified.
[ ] Unity layout prefab for each of six views, mobile responsive and state-bound.
[ ] Real four-stat labels and values verified against combat domain.
[ ] Full flow/navigation, lock rules, equipment, skills/talents tested.
[ ] Screenshot comparison with approved image; owner confirms 100% visual acceptance.
[ ] Only then merge and publish.

## Priority
**ART LOCK, NOT IMPLEMENTED.** User approval is for the visual reference, not a declaration that the generated 2D art or 3D character is already production-ready. Preserve existing progress and avoid parallel conflicting work.
