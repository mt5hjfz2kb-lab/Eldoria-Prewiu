# Valoria — official-camera visual baseline diagnosis

Status: evidence baseline before Surface v1 / LookDev closure.  
Date: 2026-09-29.

Evidence:
- Unity full gate run: 36493002127 — SUCCESS
- capture artifact: 11002367555
- commit: 694ff8336bad3dd42d1e2ee2f13edb6e15700a3a
- official views reviewed: overview/zoom 12, Aserradero zoom 9, Cuartel zoom 9, mobile portrait.

## What the real captures show

### Aserradero
The dedicated geometry reads as a useful compact production building, but the surface is pale/low-contrast and loses timber/stone separation at gameplay distance. This matches the later diagnostic evidence that the source GLB had basecolor + normal + RM while the integration path reduced it to a simplified surface.

Classification: **SURFACE FAIL, geometry retained**.

### Cuartel
The dedicated Cuartel is strongly over-bright / near-white in the baseline integrated capture. Its silhouette and parcel fit remain readable, but material identity is overwhelmed by the imported/integration response.

Classification: **SURFACE / IMPORT-INTEGRATION FAIL in this baseline capture, geometry retained**.

### Integrated Valoria
The current baseline environment uses very high ambient light plus relatively bright fog. In the official overview this compresses value separation across rock, ground and architecture. The distant and mid-ground layers merge instead of producing a strong vertical dark-fantasy read.

Classification: **LOOKDEV BASELINE DISFAVORED** pending the controlled profile comparison.

### Mobile portrait
The same issue is amplified in portrait: the bright Cuartel becomes a false focal point due to surface exposure, not authored hierarchy. This is exactly the type of defect the formula must prevent.

## Production decision

1. Do not regenerate Aserradero or Cuartel for these defects.
2. Preserve authored PBR first.
3. Compare controlled LookDev profiles.
4. Prefer a profile that restores rock/stone/timber/roof separation without crushing readable gameplay geometry.
5. The previous `baseline` LookDev values are not eligible to become the final formula merely because they were the historical default.

This evidence supports `docs/VALORIA_SURFACE_V1.md` and `docs/VALORIA_VISUAL_FORMULA_v1.md`.
