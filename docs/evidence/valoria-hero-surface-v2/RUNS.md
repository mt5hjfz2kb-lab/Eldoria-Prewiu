# Exact V2 production record

- Branch: `art/valoria-first-hero-asset-v1-20261011`; `main` was not modified.
- Immutable V1 source parent: `4d9ac9ebfb713898c37572604b2c03238afec4e5`.
- V2 source and procedure: `4ef6ddf69e497e5ef9bf72e6c05ec2d9ec0b8123`.
- Blender 4.2.5 GitHub Actions run: [38149199734](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38149199734), success. The job ran the existing art-brain check, exact V1 `.blend` reauthoring, fixed-camera before/after rendering, GLB roundtrip, conservative LOD1 and comparison board.
- Preserved output commit: `326cfbd5c52fda3ba9512bb25be9f8d6d4188a4f` (`[skip ci]`).
- Downloadable Actions artifact: `valoria-hero-gate-quality-v2`, ID `11706272740`, SHA-256 ZIP digest `dc9a3e296f8a07cec80cff6f4165218a7394acf3dbfa6ba7fdce010326cc0a66`, retained until 2026-11-10. The files are also in the branch after artifact expiry.
- Exact output directory: `art-source/valoria/production/first-hero-gate-v1/quality-v2/` with `.blend`, GLB, LOD1, six PBR bitmaps, full renders, roundtrip captures, `file-hashes.json`, `quality-report.json`, `export-verification.json`.
- Remote `.blend` SHA-256: `7ebd002e9078534adf9805661e18a9070056959c34b59020d96569083f6d1be0`; GLB SHA-256: `71bcc140152894a1749c22c893401da7fe8c0f2544db447072cbcfdee7666370`. The remote render and GLB roundtrip PNG were actually opened and visually checked. See the full per-file manifest rather than assuming local and hosted render PNG bytes match.
- GLB: 57,912 triangles and 14 materials; LOD1: 53,784 triangles, 14 materials. This modest reduction is no mobile performance claim. The destructive LOD2 candidate was rejected and is absent from the V2 output.

Art verdict remains **REWORK**, approximately 6.5–7/10 by visual judgment; the workflow's success records reproducibility, not final visual acceptance. Unity 6000.3.23f1, saved-scene persistence and Android/iOS performance were not run because no local editor exists and the live main registry reserves the Windows runner under another workstream. To continue in a normal chat, follow `REPRODUCE.md` on this same branch, then make a focused sculpt/material pass on the current `.blend` before requesting Unity access from the current runner owner.
