# VALORIA STRUCTURAL FRAME / URBAN SUPPORT INTEGRATION v1 — RESULT

Date: 2026-10-04  
Canonical base at claim: `5e1470422137d3d3e6013edfa885586849f650d4`  
Direct visual reference: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`

## Final verdict

**TECH PASS / VISUAL PARTIAL-FAIL / NOT PROMOTED**

This block changed method rather than repeating the previous certified-wall recomposition. It deliberately authored a new structural family in Blender, integrated it once in Unity, captured the required matched A/B views, and stopped at the visual gate when the result did not meet the promotion threshold.

The candidate remains disabled and is not hooked into canonical `ProductionVisualIntegration.City()`.

## Authoritative evidence

- Art-production planner: run **37223687136 — SUCCESS**
- First Blender source attempt: run **37223697615 — TECH FAIL before export** because Ubuntu Blender's glTF exporter lacked `numpy`; no geometry candidate was produced and no visual iteration occurred.
- Blender source authoring retry: run **37223931751 — SUCCESS**
- Blender source artifact: **11310713075**
- First Unity attempt: run **37224089729 — TECH FAIL at C# compile**, before scene creation; no visual judgment.
- Authoritative Unity A/B: run **37224171472 — SUCCESS**
- Authoritative Unity artifact: **11311297059**
- Credits: **0 Tripo / 0 paid**

The authoritative artifact contains:
1. HOME mobile BEFORE / AFTER
2. PAN Granero mobile BEFORE / AFTER
3. PAN intermediate mobile BEFORE / AFTER
4. PAN Cuartel mobile BEFORE / AFTER
5. 16:9 BEFORE / AFTER
6. `VALORIA_APPROVED_VISUAL_REFERENCE.jpg`
7. `evidence.json`

## Structural hypothesis executed

One strong hypothesis only:

> Replace the dominant technical support masses with a deliberately authored civic structural family, instead of recomposing the same support modules again.

Blender authored three reusable sources:

- `CivicRetainingBay.glb` — 166,136 bytes / 40 objects
- `LowerArcadedFront.glb` — 209,724 bytes / 55 objects
- `CuartelTerraceSupport.glb` — 181,820 bytes / 42 objects

The design vocabulary intentionally introduced:

- arcade openings
- real vertical piers / buttresses
- recessed shadow
- cornices
- parapets
- civic banner relief
- differentiated foundation / cap hierarchy

Unity placed four authored structural pieces:

- west civic retaining bay
- east civic retaining bay
- lower civic arcade
- Cuartel terrace support

The candidate suppressed **84 renderers** belonging to targeted technical-support shelves, seams and retaining presentation. The promoted lower-city urban massing remained present.

## Technical result

PASS.

Authoritative gate evidence reports:

- gameplay collider/hotspot signature preserved
- camera policy preserved
- promoted lower-city solution preserved
- exactly 4 authored structural placements built
- 84 target renderers suppressed
- Granero/Cuartel not moved
- Hero Bastion not changed
- camera not changed
- 0 Tripo / 0 paid credits

Focused gameplay tests also passed in the authoritative run.

## Direct visual review

### 1. Form and silhouette

**Improved, but insufficient.**

The new arcades, cornices and parapets visibly replace some undifferentiated support volume with recognizable architectural form. This is a materially stronger shape language than a repeated straight-wall kit.

However, the two largest original grey vertical support/pillar silhouettes flanking the ceremonial stair remain visually dominant in HOME and intermediate/Granero pans. The candidate therefore does not fully solve the actual high-pixel-count defect it was meant to solve.

### 2. Architecture

**Partial improvement.**

The new pieces unmistakably read as designed architecture rather than raw terrain shelves. In particular, the lower front gains a civic facade rhythm and the Cuartel side gains an explicit architectural support.

But the structural family does not yet become one continuous architectural system from lower city to Hero Bastion. Old grey supports and new authored fronts coexist instead of forming a single integrated retaining/civic frame.

### 3. Materials

**Fail for promotion.**

In the authoritative AFTER captures, the new structural sources render substantially lighter/cleaner than the surrounding Bastion, lower-city and stone grammar. The resulting pale masses attract attention as newly inserted pieces rather than disappearing into Valoria's existing material family.

This is not a reason to micro-tune colors in this block. It is evidence that material design must be authored as part of the structural-frame design before Unity integration, not treated as a late patch.

### 4. Depth and vertical transitions

**Partial.**

Recessed arches and buttresses add useful depth, and the lower frontage is less flat than the BEFORE.

The major vertical Bastion-to-city transition is still split by surviving technical pillars and exposed rock/support seams. The transition therefore remains visibly assembled from systems rather than reading as one built capital.

### 5. Bastion ↔ city integration

**Partial-FAIL.**

The new architecture provides more authored language below the Bastion, but it does not make the Bastion convincingly belong to the lower city yet. The central stair remains framed by visually separate grey support towers, while the bright new civic pieces sit around them.

### 6. Capital / premium read

**Improved locally, not enough globally.**

The AFTER has more architectural vocabulary and less anonymous support surface. At first glance, however, it also exposes a new contrast between premium Hero Bastion, surviving technical grey masses, and very light newly authored arcades. It still reads as a production scene in transition rather than a visually final capital.

## Eight mandatory questions

1. **Do the large grey masses still look like technical supports?**  
   **Yes, partly.** The most damaging stair-flanking grey pillars remain dominant, even though several shelves/fronts have been replaced.

2. **Does the Bastion now look like part of the city?**  
   **More than before, but not sufficiently.** The intervening frame is still visually discontinuous.

3. **Do the vertical transitions look like real architecture?**  
   **Partly.** The authored arcades do; the surviving central supports do not.

4. **Do the supports look designed for Valoria?**  
   **The new geometry does structurally, but its final material/read is not integrated enough.**

5. **Has the greybox effect clearly dropped?**  
   **It drops locally but not decisively across the whole frame.**

6. **Is BEFORE/AFTER obvious at first glance?**  
   **Yes.** The change is large and unmistakable. This is not a micro-adjustment.

7. **Does the whole materially approach the approved reference?**  
   **Only partially.** Architectural articulation moves in the right direction, but the reference has one coherent stone/rock/civic system rather than separate grey pillars plus bright inserted facades.

8. **Does HOME now begin to look visually final rather than like a production scene?**  
   **No, not yet.** It is structurally richer, but the remaining grey supports and mismatched new structural masses keep the prototype/assembly read alive.

## Comparison against the approved reference

The approved reference's important lesson is not simply "more arches" or "more stone." Its terraces, retaining walls, stairs, facades and rock interfaces are designed as one continuous massing and material system. Large structural surfaces are broken by intentional vertical rhythm, shadow, habitable or architectural recesses and integrated transitions, while remaining subordinate to the capital focal point.

The candidate captures one part of that lesson — deliberate architectural articulation — but not the complete system. It still layers new architecture around legacy support silhouettes instead of replacing the entire dominant frame with one authored shape/material solution.

## Promotion decision

**NOT PROMOTED.**

Promotion is rejected because:
- the largest grey stair-side supports remain visually dominant;
- the new structural material/value does not integrate with the established Valoria/Bastion palette;
- the Bastion-to-city transition is still visibly assembled from separate systems;
- HOME does not yet cross the requested "looks like the final game" threshold.

The candidate remains `Enabled=false`; canonical runtime stays on the previously promoted lower-city state.

## Anti-loop / next method

Do **not** open STRUCTURAL FRAME v2/v3 and do not solve this by moving pieces, recoloring them repeatedly, adding more modules, or decorating around the surviving supports.

The next valid artistic process must be:

**approved reference → deliberate screen-space structural-frame design → explicit complete 3D authoring (shape + material + interfaces) → Unity once → matched comparison**

Specifically, the design phase must first define the *entire* dominant stair/Bastion retaining silhouette that should replace the surviving grey pillars, including its material/value relationship to Hero Bastion, before a new 3D source is authored.

That is a method-level change, not another support micro-pass.
