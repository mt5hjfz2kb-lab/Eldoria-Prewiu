# Eldoria Toolchain Automation v2 — Result

Date: 2026-10-01  
Branch: `pipeline/toolchain-automation-v2`  
Spend: **0 Tripo credits**

## Verdict

**TOOLCHAIN ROUTING: PASS**  
**BLENDER LOD: TECH PASS**  
**BLENDER HIGH->LOW BAKE: TECH PASS**  
**UNITY BAKED-ASSET GATE: PASS**  
**UNITY ADVANCED CAPABILITY AVAILABILITY: PASS**  
**TRIPO READ-ONLY ADVANCED UI PROBE: PASS**  
**TRIPO ADVANCED TRANSFORMS: NOT YET EXECUTION-CERTIFIED**

## Evidence

Final proof run: `36859609770`

Artifacts:
- planner/static Unity audit: `11161505766`
- Blender + Unity + Tripo proof: `11161572190`
- hosted Blender bake cross-check: `11160897012`

## Concrete measured improvement

On `ResidentialTerraceRock`:

| Stage | Tris | Materials |
|---|---:|---:|
| Source | 49,800 | 4 |
| LOD1 | 24,900 | 4 |
| LOD2 | 12,450 | 4 |
| High->low bake proof | 17,430 | 1 |

The baked-low Unity gate preserved UVs and normals, produced eight valid isolated captures, passed positive raycast and empty-space miss, and used one packed 1024x1024 normal map.

## Development effect

The pipeline no longer treats every visual problem as a request for a new 3D model.

Default routing is now:
- composition/layout -> Unity first;
- surface/detail -> Unity, then Blender bake/processing when justified;
- new geometry -> only after a proven geometry gap;
- Tripo spend -> only after explicit fresh authorization.

This should reduce needless Tripo generations, let existing assets scale further through LOD/baking, and reserve high-detail geometry for the distances where it is visible.

## Boundaries

This proof does **not** claim that every lower-poly bake is visually equivalent in the real Valoria frame. Real 19/12/9/mobile validation remains mandatory before promotion.

The current Tripo Studio session visibly offers Smart Mesh P2.0, UV Smart, humanoid rigging/text-to-motion and export. Those advanced transforms were not executed in this proof, so they remain unspent and unpromoted until a dedicated zero-risk/authorized proof is justified.
