# VALORIA GRANERO RICH CAMERA-FIRST v1 — RESULT

Status: CLOSED  
Final verdict: **TECH PASS / SOURCE VISUAL PASS / UNITY DEFERRED BY WORKSTREAM CONFLICT / NOT YET PROMOTED**  
Date: 2026-10-04

## Objective
Replace the failed procedural camera-first Granero technique with one camera-authored source composed only from already-rich, SOURCE-VISUAL-PASS Granero geometry.

## Authoritative source gate
- Workflow run: **37209284772**
- Successful attempt: **2**
- Authoritative artifact: **11306380357**
- First attempt build/artifact also succeeded, but persistence hit a concurrent-main ref race; the zero-spend rerun persisted the same technique successfully.
- Tripo / paid credits: **0**

## Source
`Unity/Assets/Eldoria/Resources/Valoria/ProductionArt/GraneroRichCameraFirstV1/Valoria_Granero_RCFv1.glb`

Geometry:
- 3 rich source volumes
- 149,400 triangles
- no visible primitive/procedural architecture
- main storage hall + two angled rich storage annexes

## Visual verdict
**SOURCE VISUAL PASS.**

The camera-first color preview now reads as a coherent agricultural/storage complex:
- dominant central loading opening;
- broad storage roof mass;
- repeated agricultural tower/roof language;
- secondary storage annexes frame the center;
- rich materials and authored detail remain coherent across all visible architecture;
- the result is materially stronger than the failed smooth canopy/bin candidate and no longer looks like a generic medieval house plus add-ons.

The source is intentionally larger/more explicit than the previous Granero because functional identity must survive the constrained orthographic/mobile screen.

## Silhouette preview note
The generated silhouette helper is not used as decisive evidence because material-override behavior did not produce a pure black mask. The source-camera color preview is valid and directly demonstrates the macro silhouette/read.

## Unity decision
Unity was not started from this workstream because a separate active workstream currently owns:
- `VisualWorld.cs`
- the heavy Windows Unity runner
- Valoria production composition

Active conflicting workstream:
`valoria-camera-first-functional-unity-gate-v1`

Per the parallel-workstream protocol, this block stops rather than colliding with that owner.

## Next
As soon as the conflicting Unity workstream releases its resources:
1. pair `Valoria_Granero_RCFv1.glb` with retained source-pass `Valoria_Cuartel_CFSv1.glb`;
2. run one real matched Unity gate: BEFORE/AFTER zoom9 + mobile;
3. preserve camera, Final Look, Hero Bastion, defense, terrain, gameplay, parcels/routes/hotspots/colliders;
4. promote only if both functions survive simultaneously.

No further Granero source iteration is authorized before that paired gate.
