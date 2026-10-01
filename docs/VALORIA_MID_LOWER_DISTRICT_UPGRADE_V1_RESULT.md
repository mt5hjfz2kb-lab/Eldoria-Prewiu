# Valoria Mid/Lower District Upgrade v1 — Final Result

Status: **CLOSED — TECH PASS / VISUAL PASS (sector scale)**
Date: 2026-10-01
Branch: `visual-proof/mid-lower-district-upgrade-v1`
Certified run: `36869777315`
Evidence artifact: `11167470142`
Certified source commit: `1130773ea59edfb680fd58b60a9f64cc873bcc91`

## Scope
Scale the certified Mid-Tier parcel method from one parcel to a coherent chain of three adjacent real Mid/Lower parcels, without production promotion and without Tripo generation.

## Source integrity
- Exact Mid-Tier raw GLB reused: 68,231,740 bytes.
- SHA-256: `5e5432b78151a837eea1a77c2ea5783c543da10da747add208adfdc21cbf284a`.
- Tripo credits: **0**.
- Piece 01 conservative cleanup remained certified.
- Four-piece recovered kit remained 48,172 tris total.

## District variants
- D1 front guardhouse residence: strong two-storey core + workshop wing + roof crown + arched porch.
- D2 civic corner residence: lower/wider civic core + side residence + offset roof + recessed entry.
- D3 upper fortified residence: compact core + tall roofline + lower service wing + fortified entry.

All three use the same recovered family but deliberately different assembly grammar.

## Technical evidence
- assemblies active: **3**
- target renderers hidden: **31**
- collider/hotspot signature equal: **true**
- gameplay colliders added: **0**
- gameplay hotspots added: **0**
- Hero District preserved: **true**
- Hero Bastion bounds equal: **true**

| Metric | Baseline | District | Delta |
|---|---:|---:|---:|
| Triangles | 1,601,568 | 1,764,084 | +162,516 |
| Renderers | 1,201 | 1,194 | -7 |
| Materials | 622 | 596 | -26 |
| Lights | 24 | 27 | +3 |

The renderer/material reductions are expected because 31 legacy target renderers are visually suppressed and replaced by shared-material assembled architecture.

## Visual review
Official baseline/after evidence exists at zoom 19, 12, 9 and mobile.

At zoom 9 the three-parcel chain reads as a coherent built district rather than repeated placeholder houses. The variants differ in massing and silhouette: D1 is dominant and layered, D2 is lower/extended, D3 is compact/fortified. The architecture has stronger facade depth, roof articulation, entrances and rock/terrace integration while preserving the Hero Bastion / Hero District hierarchy.

At mobile framing the nearest upgraded parcel is materially richer than baseline and the adjacent district continuation remains visible.

### Repetition limit
The proof also establishes a real limit: all three parcels share a recognizable red-tile Mid-Tier family. That is desirable as district identity at this scale, but these four recovered pieces alone must **not** be stamped across all Valoria. Wider city expansion needs additional architectural families / civic-specialized vocabulary before broad production dressing.

## Verdict
**TECH PASS**

**VISUAL PASS — sector scale**

## Conclusion
The Mid-Tier method successfully scales from one parcel to a three-parcel Mid/Lower sector without changing gameplay topology or interaction authority. It is suitable for controlled sector-by-sector expansion.

This does **not** certify whole-city repetition. The next visual capability gap is diversity beyond this one Mid-Tier family: specialized/civic architecture and additional roof/facade vocabulary should be proven before using the method across the full city.

No production merge or promotion to `main` was performed.
